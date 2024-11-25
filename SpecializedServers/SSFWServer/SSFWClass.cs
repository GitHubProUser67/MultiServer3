using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.RegularExpressions;
using NetCoreServer;
using CustomLogger;
using SSFWServer.Services;
using SSFWServer.SaveDataHelper;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;

namespace SSFWServer
{
    public class SSFWClass
    {
        private const string LoginGUID = "bb88aea9-6bf8-4201-a6ff-5d1f8da0dd37";

        private static string? legacykey;
        private static SSFWServer? _Server;
        private static HttpSSFWServer? _HttpServer;

        private static Dictionary<string, string> LayoutGetOverrides = new();

        private string certpath;
        private string certpass;

        public SSFWClass(string certpath, string certpass, string? locallegacykey)
        {
            this.certpath = certpath;
            this.certpass = certpass;
            legacykey = locallegacykey;
        }

        private static (string HeaderIndex, string HeaderItem)[] CollectHeaders(HttpRequest request)
        {
            int headerindex = (int)request.Headers; // There is a slight mistake in netcoreserver, where the index is long, and the parser is int
                                                    // So we accomodate that with a cast.

            (string HeaderIndex, string HeaderItem)[] CollectHeader = new(string, string)[headerindex];

            for (int i = 0; i < headerindex; i++)
            {
                CollectHeader[i] = request.Header(i);
            }

            return CollectHeader;
        }

        private static string GetHeaderValue((string HeaderIndex, string HeaderItem)[] headers, string requestedHeaderIndex, bool caseSensitive = true)
        {
            if (headers.Length > 0)
            {
                const string pattern = @"^(.*?):\s(.*)$"; // Make a GITHUB ticket for netcoreserver, the header tuple can get out of sync with null values, we try to mitigate the problem.

                foreach ((string HeaderIndex, string HeaderItem) in headers)
                {
                    if (caseSensitive ? HeaderIndex.Equals(requestedHeaderIndex) : HeaderIndex.Equals(requestedHeaderIndex, StringComparison.InvariantCultureIgnoreCase))
                        return HeaderItem;
                    else
                    {
                        try
                        {
                            Match match = Regex.Match(HeaderItem, pattern);

                            if (caseSensitive ? HeaderItem.Contains(requestedHeaderIndex) : HeaderItem.Contains(requestedHeaderIndex, StringComparison.InvariantCultureIgnoreCase)
                                && match.Success) // Make a GITHUB ticket for netcoreserver, the header tuple can get out of sync with null values, we try to mitigate the problem.
                                return match.Groups[2].Value;
                        }
                        catch
                        {

                        }
                    }
                }
            }

            return string.Empty; // Return empty string if the header index is not found, why not null, because in this case it prevents us
                                 // from doing extensive checks everytime we want to display the User-Agent in particular.
        }

        private static string? ExtractBeforeFirstDot(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            int dotIndex = input.IndexOf('.');
            if (dotIndex == -1)
                return null;

            return input[..dotIndex];
        }

        private static bool IsSSFWRegistered(string? sessionid)
        {
            if (string.IsNullOrEmpty(sessionid))
                return false;

            return !string.IsNullOrEmpty(SSFWUserSessionManager.GetIdBySessionId(sessionid));
        }

        public void StartSSFW()
        {
            // Create and prepare a new SSL server context and start the server
            _Server = new SSFWServer(new SslContext(SslProtocols.Tls12, new X509Certificate2(certpath, certpass), MyRemoteCertificateValidationCallback), IPAddress.Any, 10443);
            // Create and start the server
            _HttpServer = new HttpSSFWServer(IPAddress.Any, 8080);

            Parallel.Invoke(
                    () => _Server.Start(),
                    () => _HttpServer.Start()
                );

            LoggerAccessor.LogInfo("[SSFW] - Server started on ports 8080 and 10443...");
        }

        public void StopSSFW()
        {
            _Server?.Stop();
            _HttpServer?.Stop();
        }

        private static HttpResponse SSFWRequestProcess(HttpRequest request, HttpResponse Response)
        {
            try
            {
                (string HeaderIndex, string HeaderItem)[] Headers = CollectHeaders(request);

                string UserAgent = GetHeaderValue(Headers, "User-Agent", false);

                if (!string.IsNullOrEmpty(request.Url))
                {
                    string absolutepath = request.Url;

                    // Split the URL into segments
                    string[] segments = absolutepath.Trim('/').Split('/');

                    // Combine the folder segments into a directory path
                    string directoryPath = Path.Combine(SSFWServerConfiguration.SSFWStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                    // Process the request based on the HTTP method
                    string filePath = Path.Combine(SSFWServerConfiguration.SSFWStaticFolder, absolutepath[1..]);

#if DEBUG
                    LoggerAccessor.LogInfo($"[SSFW] - Home Client Requested the SSFW Server with URL : {request.Method} {request.Url} (Details: \n{{ \"NetCoreServer\":" + System.Text.Json.JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true })
                        + (Headers.Length > 0 ? $", \"Headers\":{System.Text.Json.JsonSerializer.Serialize(Headers.ToDictionary(header => header.HeaderIndex, header => header.HeaderItem), new JsonSerializerOptions { WriteIndented = true })} }} )" : "} )"));
#else
                    LoggerAccessor.LogInfo($"[SSFW] - Home Client Requested the SSFW Server with URL : {request.Method} {request.Url}");
#endif

                    if (!string.IsNullOrEmpty(UserAgent) && UserAgent.Contains("PSHome")) // Host ban is not perfect, but netcoreserver only has that to offer...
                    {
                        string? env = ExtractBeforeFirstDot(GetHeaderValue(Headers, "Host", false));
                        string sessionid = GetHeaderValue(Headers, "X-Home-Session-Id");

                        if (string.IsNullOrEmpty(env) || !SSFWMisc.homeEnvs.Contains(env))
                            env = "cprod";

                        switch (request.Method)
                        {
                            case "GET":

                                #region LayoutService
                                if (absolutepath.Contains($"/LayoutService/{env}/person/") && IsSSFWRegistered(sessionid))
                                {
                                    string? res = null;
                                    SSFWLayoutService layout = new(legacykey);

                                    if (LayoutGetOverrides.ContainsKey(sessionid))
                                    {
                                        string layoutNewUser = LayoutGetOverrides[sessionid];
                                        bool isRpcnUser = layoutNewUser.Contains("@RPCN");
                                        string LayoutDirectoryPath = Path.Combine(SSFWServerConfiguration.SSFWStaticFolder, $"LayoutService/{env}/person/");

                                        if (Directory.Exists(LayoutDirectoryPath))
                                        {
                                            string? matchingDirectory;

                                            if (isRpcnUser)
                                            {
                                                string[] nameParts = layoutNewUser.Split('@');

                                                if (nameParts.Length == 2 && !SSFWServerConfiguration.SSFWCrossSave)
                                                {
                                                    matchingDirectory = Directory.GetDirectories(LayoutDirectoryPath)
                                                       .Where(dir =>
                                                           Path.GetFileName(dir).StartsWith(nameParts[0]) &&
                                                           Path.GetFileName(dir).Contains(nameParts[1])
                                                       ).FirstOrDefault();
                                                }
                                                else
                                                    matchingDirectory = Directory.GetDirectories(LayoutDirectoryPath)
                                                      .Where(dir =>
                                                          Path.GetFileName(dir).StartsWith(layoutNewUser.Replace("@RPCN", string.Empty))
                                                      ).FirstOrDefault();
                                            }
                                            else
                                                matchingDirectory = Directory.GetDirectories(LayoutDirectoryPath)
                                                  .Where(dir =>
                                                      Path.GetFileName(dir).StartsWith(layoutNewUser) &&
                                                      !Path.GetFileName(dir).Contains("RPCN")
                                                  ).FirstOrDefault();

                                            res = layout.HandleLayoutServiceGET(!string.IsNullOrEmpty(matchingDirectory) ? matchingDirectory : directoryPath, filePath);

                                        } // if the dir not exists, we return 403.

                                        lock (LayoutGetOverrides)
                                            LayoutGetOverrides.Remove(sessionid);
                                    }
                                    else
                                        res = layout.HandleLayoutServiceGET(directoryPath, filePath);

                                    if (res == null)
                                    {
                                        Response.Clear();
                                        Response.SetBegin(403);
                                        Response.SetBody();
                                    }
                                    else if (res == string.Empty)
                                    {
                                        Response.Clear();
                                        Response.SetBegin(404);
                                        Response.SetBody();
                                    }
                                    else
                                        Response.MakeGetResponse(res, "application/json");
                                    layout.Dispose();
                                }
                                #endregion

                                #region AdminObjectService
                                else if (absolutepath.Contains("/AdminObjectService/start") && IsSSFWRegistered(sessionid))
                                {
                                    SSFWAdminObjectService iga = new(sessionid, legacykey);
                                    Response.Clear();
                                    if (iga.HandleAdminObjectService(UserAgent))
                                        Response.SetBegin(200);
                                    else
                                        Response.SetBegin(403);
                                    Response.SetBody();
                                    iga.Dispose();
                                }
                                #endregion

                                #region SaveDataService
                                else if (absolutepath.Contains($"/SaveDataService/{env}/{segments.LastOrDefault()}") && IsSSFWRegistered(sessionid))
                                {
                                    SSFWGetFileList filelist = new();
                                    string? res = filelist.SSFWSaveDataDebugGetFileList(directoryPath, segments.LastOrDefault());
                                    if (res != null)
                                        Response.MakeGetResponse(res, "application/json");
                                    else
                                        Response.MakeErrorResponse();
                                    filelist.Dispose();
                                }
                                #endregion

                                else if (IsSSFWRegistered(sessionid))
                                {
                                    if (File.Exists(filePath + ".json"))
                                    {
                                        string? res = FileHelper.ReadAllText(filePath + ".json", legacykey);

                                        if (!string.IsNullOrEmpty(res))
                                        {
                                            if (GetHeaderValue(Headers, "Accept") == "application/json")
                                                Response.MakeGetResponse(res, "application/json");
                                            else
                                                Response.MakeGetResponse(res);
                                        }
                                        else
                                            Response.MakeErrorResponse();
                                    }
                                    else if (File.Exists(filePath + ".bin"))
                                    {
                                        byte[]? res = FileHelper.ReadAllBytes(filePath + ".bin", legacykey);

                                        if (res != null)
                                            Response.MakeGetResponse(res, "application/octet-stream");
                                        else
                                            Response.MakeErrorResponse();
                                    }
                                    else if (File.Exists(filePath + ".jpeg"))
                                    {
                                        byte[]? res = FileHelper.ReadAllBytes(filePath + ".jpeg", legacykey);

                                        if (res != null)
                                            Response.MakeGetResponse(res, "image/jpeg");
                                        else
                                            Response.MakeErrorResponse();
                                    }
                                    else
                                    {
                                        LoggerAccessor.LogWarn($"[SSFW] : {UserAgent} Requested a non-exisant file - {filePath}");
                                        Response.Clear();
                                        Response.SetBegin(404);
                                        Response.SetBody();
                                    }
                                }
                                else if (absolutepath.Contains($"/SaveDataService/avatar/{env}/") && absolutepath.EndsWith(".jpg"))
                                {
                                    if (File.Exists(filePath))
                                    {
                                        byte[]? res = FileHelper.ReadAllBytes(filePath, legacykey);

                                        if (res != null)
                                            Response.MakeGetResponse(res, "image/jpg");
                                        else
                                            Response.MakeErrorResponse();
                                    }
                                    else
                                    {
                                        Response.Clear();
                                        Response.SetBegin(404);
                                        Response.SetBody();
                                    }
                                }
                                else
                                {
                                    Response.Clear();
                                    Response.SetBegin(403);
                                    Response.SetBody();
                                }
                                break;
                            case "POST":

                                #region SSFW Login

                                if (request.BodyLength <= Array.MaxLength)
                                {
                                    byte[] postbuffer = request.BodyBytes;
                                    if (absolutepath == $"/{LoginGUID}/login/token/psn")
                                    {
                                        string? XHomeClientVersion = GetHeaderValue(Headers, "X-HomeClientVersion");
                                        string? generalsecret = GetHeaderValue(Headers, "general-secret");

                                        if (!string.IsNullOrEmpty(XHomeClientVersion) && !string.IsNullOrEmpty(generalsecret))
                                        {
                                            SSFWLogin login = new(XHomeClientVersion, generalsecret, XHomeClientVersion.Replace(".", string.Empty), GetHeaderValue(Headers, "x-signature"), legacykey);
                                            string? result = login.HandleLogin(postbuffer, env);
                                            if (!string.IsNullOrEmpty(result))
                                            {
                                                Response.Clear();
                                                Response.SetBegin(201);
                                                Response.SetContentType("application/json");
                                                Response.SetBody(result);
                                            }
                                            else
                                                Response.MakeErrorResponse();
                                            login.Dispose();
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(403);
                                            Response.SetBody();
                                        }
                                    }
                                    #endregion

                                    #region PING
                                    else if (absolutepath.Contains("/morelife") && !string.IsNullOrEmpty(GetHeaderValue(Headers, "x-signature")))
                                    {
                                        const byte GuidLength = 36;
                                        int index = absolutepath.IndexOf("/morelife");

                                        if (index != -1 && index > GuidLength) // Makes sure we have at least 36 chars available beforehand.
                                        {
                                            // Extract the substring between the last '/' and the morelife separator.
                                            string resultSessionId = absolutepath.Substring(index - GuidLength, GuidLength);

                                            if (Regex.IsMatch(resultSessionId, @"^[{(]?([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})[)}]?$") && IsSSFWRegistered(resultSessionId))
                                            {
                                                SSFWUserSessionManager.UpdateKeepAliveTime(resultSessionId);
                                                Response.MakeGetResponse("{}", "application/json");
                                                break;
                                            }
                                            else
                                            {
                                                Response.Clear();
                                                Response.SetBegin(403);
                                                Response.SetBody();
                                            }
                                        }
                                        else
                                            Response.MakeErrorResponse();
                                    }
                                    #endregion

                                    #region AvatarLayoutService
                                    else if (absolutepath.Contains($"/AvatarLayoutService/{env}/") && IsSSFWRegistered(sessionid))
                                    {
                                        SSFWAvatarLayoutService layout = new(sessionid, legacykey);
                                        Response.Clear();
                                        if (layout.HandleAvatarLayout(postbuffer, directoryPath, filePath, absolutepath, false))
                                            Response.SetBegin(200);
                                        else
                                            Response.SetBegin(403);
                                        Response.SetBody();
                                        layout.Dispose();
                                    }
                                    #endregion

                                    #region LayoutService
                                    else if (absolutepath.Contains($"/LayoutService/{env}/person/") && IsSSFWRegistered(sessionid))
                                    {
                                        SSFWLayoutService layout = new(legacykey);
                                        Response.Clear();
                                        if (layout.HandleLayoutServicePOST(postbuffer, directoryPath, absolutepath))
                                            Response.SetBegin(200);
                                        else
                                            Response.SetBegin(403);
                                        Response.SetBody();
                                        layout.Dispose();
                                    }
                                    #endregion

                                    #region RewardsService
                                    else if (absolutepath.Contains($"/RewardsService/{env}/rewards/") && IsSSFWRegistered(sessionid))
                                    {
                                        SSFWRewardsService reward = new(legacykey);
                                        Response.MakeGetResponse(reward.HandleRewardServicePOST(postbuffer, directoryPath, filePath, absolutepath), "application/json");
                                        reward.Dispose();
                                    }
                                    else if (absolutepath.Contains($"/RewardsService/trunks-{env}/trunks/") && absolutepath.Contains("/setpartial") && IsSSFWRegistered(sessionid))
                                    {
                                        SSFWRewardsService reward = new(legacykey);
                                        reward.HandleRewardServiceTrunksPOST(postbuffer, directoryPath, filePath, absolutepath);
                                        Response.MakeOkResponse();
                                        reward.Dispose();
                                    }
                                    else if (absolutepath.Contains($"/RewardsService/trunks-{env}/trunks/") && absolutepath.Contains("/set") && IsSSFWRegistered(sessionid))
                                    {
                                        SSFWRewardsService reward = new(legacykey);
                                        reward.HandleRewardServiceTrunksEmergencyPOST(postbuffer, directoryPath, absolutepath);
                                        Response.MakeOkResponse();
                                        reward.Dispose();
                                    }
                                    #endregion

                                    else if (IsSSFWRegistered(sessionid))
                                    {
                                        LoggerAccessor.LogWarn($"[SSFW] : Host requested a POST method I don't know about! - Report it to GITHUB with the request : {absolutepath}");
                                        if (postbuffer != null)
                                        {
                                            Directory.CreateDirectory(directoryPath);
                                            switch (GetHeaderValue(Headers, "Content-type", false))
                                            {
                                                case "image/jpeg":
                                                    File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.jpeg", postbuffer);
                                                    break;
                                                case "application/json":
                                                    File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.json", postbuffer);
                                                    break;
                                                default:
                                                    File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.bin", postbuffer);
                                                    break;
                                            }
                                        }
                                        Response.MakeOkResponse();
                                    }
                                    else
                                    {
                                        Response.Clear();
                                        Response.SetBegin(403);
                                        Response.SetBody();
                                    }
                                }
                                else
                                {
                                    Response.Clear();
                                    Response.SetBegin(400);
                                    Response.SetBody();
                                }

                                break;
                            case "PUT":
                                if (IsSSFWRegistered(sessionid))
                                {
                                    if (request.BodyLength <= Array.MaxLength)
                                    {
                                        byte[] putbuffer = request.BodyBytes;
                                        if (putbuffer != null)
                                        {
                                            Directory.CreateDirectory(directoryPath);
                                            switch (GetHeaderValue(Headers, "Content-type", false))
                                            {
                                                case "image/jpeg":
                                                    string savaDataAvatarDirectoryPath = Path.Combine(SSFWServerConfiguration.SSFWStaticFolder, $"SaveDataService/avatar/{env}/");

                                                    Directory.CreateDirectory(savaDataAvatarDirectoryPath);

                                                    string? userName = SSFWUserSessionManager.GetFormatedUsernameBySessionId(sessionid);

                                                    if (!string.IsNullOrEmpty(userName))
                                                    {
                                                        Task.WhenAll(File.WriteAllBytesAsync($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.jpeg", putbuffer),
                                                            File.WriteAllBytesAsync($"{savaDataAvatarDirectoryPath}{userName}.jpg", putbuffer)).Wait();
                                                        Response.MakeOkResponse();
                                                    }
                                                    else
                                                        Response.MakeErrorResponse();
                                                    break;
                                                case "application/json":
                                                    File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.json", putbuffer);
                                                    Response.MakeOkResponse();
                                                    break;
                                                default:
                                                    File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.bin", putbuffer);
                                                    Response.MakeOkResponse();
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(403);
                                            Response.SetBody();
                                        }
                                    }
                                    else
                                    {
                                        Response.Clear();
                                        Response.SetBegin(400);
                                        Response.SetBody();
                                    }
                                }
                                else
                                {
                                    Response.Clear();
                                    Response.SetBegin(403);
                                    Response.SetBody();
                                }
                                break;
                            case "DELETE":

                                #region AvatarLayoutService
                                if (absolutepath.Contains($"/AvatarLayoutService/{env}/") && IsSSFWRegistered(sessionid))
                                {
                                    if (request.BodyLength <= Array.MaxLength)
                                    {
                                        SSFWAvatarLayoutService layout = new(sessionid, legacykey);
                                        Response.Clear();
                                        if (layout.HandleAvatarLayout(request.BodyBytes, directoryPath, filePath, absolutepath, true))
                                            Response.SetBegin(200);
                                        else
                                            Response.SetBegin(403);
                                        Response.SetBody();
                                        layout.Dispose();
                                    }
                                    else
                                    {
                                        Response.Clear();
                                        Response.SetBegin(400);
                                        Response.SetBody();
                                    }
                                }
                                #endregion

                                else if (IsSSFWRegistered(sessionid))
                                {
                                    if (File.Exists(filePath + ".json"))
                                    {
                                        File.Delete(filePath + ".json");
                                        Response.MakeOkResponse();
                                    }
                                    else if (File.Exists(filePath + ".bin"))
                                    {
                                        File.Delete(filePath + ".bin");
                                        Response.MakeOkResponse();
                                    }
                                    else if (File.Exists(filePath + ".jpeg"))
                                    {
                                        File.Delete(filePath + ".jpeg");
                                        Response.MakeOkResponse();
                                    }
                                    else
                                    {
                                        LoggerAccessor.LogError($"[SSFW] : {UserAgent} Requested a file to delete that doesn't exist - {filePath}");
                                        Response.Clear();
                                        Response.SetBegin(404);
                                        Response.SetBody();
                                    }
                                }
                                else
                                {
                                    Response.Clear();
                                    Response.SetBegin(403);
                                    Response.SetBody();
                                }
                                break;
                            default:
                                Response.Clear();
                                Response.SetBegin(403);
                                Response.SetBody();
                                break;
                        }
                    }
                    else
                    {
                        switch (request.Method)
                        {
                            case "GET":
                                switch (absolutepath)
                                {
                                    case "/AddMiniItem.html":
                                        if (File.Exists(filePath))
                                        {
                                            Response.Clear();
                                            Response.SetBegin(200);
                                            Response.SetContentType("text/html; charset=utf-8");
                                            Response.SetBody(File.ReadAllBytes(filePath));
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(404);
                                            Response.SetBody();
                                        }
                                        
                                        break;
                                    default:
                                        Response.Clear();
                                        Response.SetBegin(403);
                                        Response.SetBody();
                                        break;
                                }
                                break;
                            case "POST":
                                byte InventoryEntryType = 0;
                                string? userId = null;
                                string uuid = string.Empty;
                                string sessionId = string.Empty;
                                string env = string.Empty;
                                string[]? uuids = null;

                                switch (absolutepath)
                                {
                                    case "/WebService/GetSceneLike/":
                                        string sceneNameLike = GetHeaderValue(Headers, "like", false);

                                        Response.Clear();

                                        if (!string.IsNullOrEmpty(sceneNameLike))
                                        {
                                            string? sceneName = ScenelistParser.GetSceneNameLike(sceneNameLike);

                                            if (!string.IsNullOrEmpty(sceneName))
                                            {
                                                Response.SetBegin(200);
                                                Response.SetBody(sceneName);
                                            }
                                            else
                                            {
                                                Response.SetBegin(500);
                                                Response.SetBody("SceneNameLike returned a null or empty sceneName!");
                                            }
                                        }
                                        else
                                        {
                                            Response.SetBegin(403);
                                            Response.SetBody("Invalid like attribute was used!");
                                        }
                                        break;
                                    case "/WebService/ApplyLayoutOverride/":
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);
                                        string targetUserName = GetHeaderValue(Headers, "targetUserName", false);

                                        Response.Clear();

                                        if (!string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(targetUserName))
                                        {
                                            lock (LayoutGetOverrides)
                                            {
                                                if (!LayoutGetOverrides.ContainsKey(sessionId))
                                                    LayoutGetOverrides.Add(sessionId, targetUserName);
                                                else
                                                    LayoutGetOverrides[sessionId] = targetUserName;
                                            }

                                            Response.SetBegin(200);
                                            Response.SetBody($"Override set for {sessionId}.");
                                        }
                                        else
                                        {
                                            Response.SetBegin(403);
                                            Response.SetBody("Invalid sessionid or targetUserName attribute was used!");
                                        }
                                        break;
                                    case "/WebService/R3moveLayoutOverride/":
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);

                                        Response.Clear();

                                        if (!string.IsNullOrEmpty(sessionId))
                                        {
                                            lock (LayoutGetOverrides)
                                            {
                                                if (LayoutGetOverrides.ContainsKey(sessionId))
                                                    LayoutGetOverrides.Remove(sessionId);
                                            }

                                            Response.SetBegin(200);
                                            Response.SetBody($"Override removed for {sessionId}.");
                                        }
                                        else
                                        {
                                            Response.SetBegin(403);
                                            Response.SetBody("Invalid sessionid attribute was used!");
                                        }
                                        break;
                                    case "/WebService/AddMiniItem/":
                                        uuid = GetHeaderValue(Headers, "uuid", false);
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);
                                        env = GetHeaderValue(Headers, "env", false);
                                        userId = SSFWUserSessionManager.GetIdBySessionId(sessionId);

                                        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(uuid) && byte.TryParse(GetHeaderValue(Headers, "invtype", false), out InventoryEntryType))
                                        {
                                            string miniPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{userId}/mini.json";

                                            if (File.Exists(miniPath))
                                            {
                                                List<Dictionary<string, byte>>? rewardsList = JsonConvert.DeserializeObject<List<Dictionary<string, byte>>>(File.ReadAllText(miniPath));

                                                if (rewardsList != null)
                                                {
                                                    SSFWRewardsService.AddMiniEntry(rewardsList, uuid, InventoryEntryType);

                                                    try
                                                    {
                                                        File.WriteAllText(miniPath, JsonConvert.SerializeObject(rewardsList, Formatting.Indented));
                                                        Response.Clear();
                                                        Response.SetBegin(200);
                                                        Response.SetBody($"UUID: {uuid} successfully added to the Mini rewards list.");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        string errMsg = $"Mini rewards list file update errored out for file: {miniPath} (Exception: {ex})";
                                                        Response.MakeErrorResponse(errMsg);
                                                        LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                    }
                                                }
                                                else
                                                {
                                                    string errMsg = $"Mini rewards list deserializing errored out for file: {miniPath}";
                                                    Response.MakeErrorResponse(errMsg);
                                                    LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                }
                                            }
                                            else
                                            {
                                                Response.Clear();
                                                Response.SetBegin(403);
                                                Response.SetBody($"User: {sessionId} on env:{env} doesn't have a ssfw mini file!");
                                            }
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(403);
                                            Response.SetBody($"User: {sessionId} is not connected or sent invalid InventoryEntryType!");
                                        }
                                        break;
                                    case "/WebService/AddMiniItems/":
                                        uuids = GetHeaderValue(Headers, "uuids", false).Split(',');
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);
                                        env = GetHeaderValue(Headers, "env", false);
                                        userId = SSFWUserSessionManager.GetIdBySessionId(sessionId);

                                        if (!string.IsNullOrEmpty(userId) && uuids != null && byte.TryParse(GetHeaderValue(Headers, "invtype", false), out InventoryEntryType))
                                        {
                                            string miniPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{userId}/mini.json";

                                            if (File.Exists(miniPath))
                                            {
                                                List<Dictionary<string, byte>>? rewardsList = JsonConvert.DeserializeObject<List<Dictionary<string, byte>>>(File.ReadAllText(miniPath));

                                                if (rewardsList != null)
                                                {
                                                    foreach (string iteruuid in uuids)
                                                    {
                                                        SSFWRewardsService.AddMiniEntry(rewardsList, iteruuid, InventoryEntryType);
                                                    }

                                                    try
                                                    {
                                                        File.WriteAllText(miniPath, JsonConvert.SerializeObject(rewardsList, Formatting.Indented));
                                                        Response.Clear();
                                                        Response.SetBegin(200);
                                                        Response.SetBody($"UUIDs: {string.Join(",", uuids)} successfully added to the Mini rewards list.");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        string errMsg = $"Mini rewards list file update errored out for file: {miniPath} (Exception: {ex})";
                                                        Response.MakeErrorResponse(errMsg);
                                                        LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                    }
                                                }
                                                else
                                                {
                                                    string errMsg = $"Mini rewards list deserializing errored out for file: {miniPath}";
                                                    Response.MakeErrorResponse(errMsg);
                                                    LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                }
                                            }
                                            else
                                            {
                                                Response.Clear();
                                                Response.SetBegin(403);
                                                Response.SetBody($"User: {sessionId} on env:{env} doesn't have a ssfw mini file!");
                                            }
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(403);
                                            Response.SetBody($"User: {sessionId} is not connected or sent invalid InventoryEntryType!");
                                        }
                                        break;
                                    case "/WebService/RemoveMiniItem/":
                                        uuid = GetHeaderValue(Headers, "uuid", false);
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);
                                        env = GetHeaderValue(Headers, "env", false);
                                        userId = SSFWUserSessionManager.GetIdBySessionId(sessionId);

                                        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(uuid) && byte.TryParse(GetHeaderValue(Headers, "invtype", false), out InventoryEntryType))
                                        {
                                            string miniPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{userId}/mini.json";

                                            if (File.Exists(miniPath))
                                            {
                                                List<Dictionary<string, byte>>? rewardsList = JsonConvert.DeserializeObject<List<Dictionary<string, byte>>>(File.ReadAllText(miniPath));

                                                if (rewardsList != null)
                                                {
                                                    SSFWRewardsService.RemoveMiniEntry(rewardsList, uuid, InventoryEntryType);

                                                    try
                                                    {
                                                        File.WriteAllText(miniPath, JsonConvert.SerializeObject(rewardsList, Formatting.Indented));
                                                        Response.Clear();
                                                        Response.SetBegin(200);
                                                        Response.SetBody($"UUID: {uuid} successfully removed in the Mini rewards list.");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        string errMsg = $"Mini rewards list file update errored out for file: {miniPath} (Exception: {ex})";
                                                        Response.MakeErrorResponse(errMsg);
                                                        LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                    }
                                                }
                                                else
                                                {
                                                    string errMsg = $"Mini rewards list deserializing errored out for file: {miniPath}";
                                                    Response.MakeErrorResponse(errMsg);
                                                    LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                }
                                            }
                                            else
                                            {
                                                Response.Clear();
                                                Response.SetBegin(403);
                                                Response.SetBody($"User: {sessionId} on env:{env} doesn't have a ssfw mini file!");
                                            }
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(403);
                                            Response.SetBody($"User: {sessionId} is not connected or sent invalid InventoryEntryType!");
                                        }
                                        break;
                                    case "/WebService/RemoveMiniItems/":
                                        uuids = GetHeaderValue(Headers, "uuids", false).Split(',');
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);
                                        env = GetHeaderValue(Headers, "env", false);
                                        userId = SSFWUserSessionManager.GetIdBySessionId(sessionId);

                                        if (!string.IsNullOrEmpty(userId) && uuids != null && byte.TryParse(GetHeaderValue(Headers, "invtype", false), out InventoryEntryType))
                                        {
                                            string miniPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{userId}/mini.json";

                                            if (File.Exists(miniPath))
                                            {
                                                List<Dictionary<string, byte>>? rewardsList = JsonConvert.DeserializeObject<List<Dictionary<string, byte>>>(File.ReadAllText(miniPath));

                                                if (rewardsList != null)
                                                {
                                                    foreach (string iteruuid in uuids)
                                                    {
                                                        SSFWRewardsService.RemoveMiniEntry(rewardsList, iteruuid, InventoryEntryType);
                                                    }

                                                    try
                                                    {
                                                        File.WriteAllText(miniPath, JsonConvert.SerializeObject(rewardsList, Formatting.Indented));
                                                        Response.Clear();
                                                        Response.SetBegin(200);
                                                        Response.SetBody($"UUIDs: {string.Join(",", uuids)} removed in the Mini rewards list.");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        string errMsg = $"Mini rewards list file update errored out for file: {miniPath} (Exception: {ex})";
                                                        Response.MakeErrorResponse(errMsg);
                                                        LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                    }
                                                }
                                                else
                                                {
                                                    string errMsg = $"Mini rewards list deserializing errored out for file: {miniPath}";
                                                    Response.MakeErrorResponse(errMsg);
                                                    LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                }
                                            }
                                            else
                                            {
                                                Response.Clear();
                                                Response.SetBegin(403);
                                                Response.SetBody($"User: {sessionId} on env:{env} doesn't have a ssfw mini file!");
                                            }
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(403);
                                            Response.SetBody($"User: {sessionId} is not connected or sent invalid InventoryEntryType!");
                                        }
                                        break;
                                    default:
                                        Response.Clear();
                                        Response.SetBegin(403);
                                        Response.SetBody();
                                        break;
                                }
                                break;
                            default:
                                Response.Clear();
                                Response.SetBegin(403);
                                Response.SetBody();
                                break;
                        }
                    }
                }
                else
                {
                    LoggerAccessor.LogError($"[SSFW] - Client Requested the SSFW Server with invalid url!");
                    Response.Clear();
                    Response.SetBegin(400);
                    Response.SetBody();
                }
            }
            catch (Exception e)
            {
                Response.MakeErrorResponse();
                LoggerAccessor.LogError($"[SSFW] - Request thrown an error : {e}");
            }

            return Response;
        }

        private bool MyRemoteCertificateValidationCallback(object? sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; //This isn't a good thing to do, but to keep the code simple i prefer doing this, it will be used only on mono
        }

        private class SSFWSession : HttpsSession
        {
            public SSFWSession(HttpsServer server) : base(server) { }

            protected override void OnReceivedRequest(HttpRequest request)
            {
                SendResponseAsync(SSFWRequestProcess(request, Response));
            }

            protected override void OnReceivedRequestError(HttpRequest request, string error)
            {
                LoggerAccessor.LogError($"[SSFW] - Request error: {error}");
            }

            protected override void OnError(SocketError error)
            {
                LoggerAccessor.LogError($"[SSFW] - Session caught an error: {error}");
            }
        }

        private class HttpSSFWSession : HttpSession
        {
            public HttpSSFWSession(HttpServer server) : base(server) { }

            protected override void OnReceivedRequest(HttpRequest request)
            {
                SendResponseAsync(SSFWRequestProcess(request, Response));
            }

            protected override void OnReceivedRequestError(HttpRequest request, string error)
            {
                LoggerAccessor.LogError($"[SSFW] - Request error: {error}");
            }

            protected override void OnError(SocketError error)
            {
                LoggerAccessor.LogError($"[SSFW] - HTTP session caught an error: {error}");
            }
        }

        public class SSFWServer : HttpsServer
        {
            public SSFWServer(SslContext context, IPAddress address, int port) : base(context, address, port) { }

            protected override SslSession CreateSession() { return new SSFWSession(this); }

            protected override void OnError(SocketError error)
            {
                LoggerAccessor.LogError($"[SSFW] - Server caught an error: {error}");
            }
        }

        public class HttpSSFWServer : HttpServer
        {
            public HttpSSFWServer(IPAddress address, int port) : base(address, port) { }

            protected override TcpSession CreateSession() { return new HttpSSFWSession(this); }

            protected override void OnError(SocketError error)
            {
                LoggerAccessor.LogError($"[SSFW] - HTTPS session caught an error: {error}");
            }
        }
    }
}
