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
using NetworkLibrary.Extension;
using NetworkLibrary.HTTP;
using System.Collections.Concurrent;
using SSFWServer.Helpers.FileHelper;

namespace SSFWServer
{
    public class SSFWClass
    {
        private const string LoginGUID = "bb88aea9-6bf8-4201-a6ff-5d1f8da0dd37";

        // Defines a list of web-related file extensions
        private static HashSet<string> allowedWebExtensions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            ".html", ".htm", ".cgi", ".css", ".js", ".svg", ".gif", ".ico", ".woff", ".woff2", ".ttf", ".eot"
        };


        private static string? legacykey;
        private static SSFWServer? _Server;
        private static HttpSSFWServer? _HttpServer;

        private static ConcurrentDictionary<string, string> LayoutGetOverrides = new();

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
                string absolutepath = HTTPProcessor.DecodeUrl(request.Url);

                if (!string.IsNullOrEmpty(absolutepath))
                {
                    (string HeaderIndex, string HeaderItem)[] Headers = CollectHeaders(request);

                    string? encoding = null;
                    string UserAgent = GetHeaderValue(Headers, "User-Agent", false);
                    string cacheControl = GetHeaderValue(Headers, "Cache-Control");

                    if (string.IsNullOrEmpty(cacheControl) || cacheControl != "no-transform")
                        encoding = GetHeaderValue(Headers, "Accept-Encoding");

                    // Split the URL into segments
                    string[] segments = absolutepath.Trim('/').Split('/');

                    // Combine the folder segments into a directory path
                    string directoryPath = Path.Combine(SSFWServerConfiguration.SSFWStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                    // Process the request based on the HTTP method
                    string filePath = Path.Combine(SSFWServerConfiguration.SSFWStaticFolder, absolutepath[1..]);

#if DEBUG
                    LoggerAccessor.LogInfo($"[SSFW] - Home Client Requested the SSFW Server with URL : {request.Method} {absolutepath} (Details: \n{{ \"NetCoreServer\":" + System.Text.Json.JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true })
                        + (Headers.Length > 0 ? $", \"Headers\":{System.Text.Json.JsonSerializer.Serialize(Headers.ToDictionary(header => header.HeaderIndex, header => header.HeaderItem), new JsonSerializerOptions { WriteIndented = true })} }} )" : "} )"));
#else
                    LoggerAccessor.LogInfo($"[SSFW] - Home Client Requested the SSFW Server with URL : {request.Method} {absolutepath}");
#endif

                    if (!string.IsNullOrEmpty(UserAgent) && UserAgent.Contains("PSHome")) // Host ban is not perfect, but netcoreserver only has that to offer...
                    {
                        string? env = ExtractBeforeFirstDot(GetHeaderValue(Headers, "Host", false));
                        string sessionid = GetHeaderValue(Headers, "X-Home-Session-Id");

                        if (string.IsNullOrEmpty(env) || !SSFWMisc.homeEnvs.Contains(env))
                            env = "cprod";

                        // Instantiate services
                        SSFWAuditService auditService = new(sessionid, env, legacykey);
                        SSFWRewardsService rewardSvc = new(legacykey);
                        SSFWLayoutService layout = new(legacykey);
                        SSFWAvatarLayoutService avatarLayout = new(sessionid, legacykey);

                        switch (request.Method)
                        {
                            case "GET":

                                #region LayoutService
                                if (absolutepath.Contains($"/LayoutService/{env}/person/") && IsSSFWRegistered(sessionid))
                                {
                                    string? res = null;

                                    if (LayoutGetOverrides.ContainsKey(sessionid))
                                        LayoutGetOverrides.Remove(sessionid, out res);
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
                                }
                                #endregion

                                #region AdminObjectService
                                else if (absolutepath.Contains("/AdminObjectService/start") && IsSSFWRegistered(sessionid))
                                {
                                    Response.Clear();
                                    if (new SSFWAdminObjectService(sessionid, legacykey).HandleAdminObjectService(UserAgent))
                                        Response.SetBegin(200);
                                    else
                                        Response.SetBegin(403);
                                    Response.SetBody();
                                }
                                #endregion

                                #region SaveDataService
                                else if (absolutepath.Contains($"/SaveDataService/{env}/{segments.LastOrDefault()}") && IsSSFWRegistered(sessionid))
                                {
                                    string? res = SSFWGetFileList.SSFWSaveDataDebugGetFileList(directoryPath, segments.LastOrDefault());
                                    if (res != null)
                                        Response.MakeGetResponse(res, "application/json");
                                    else
                                        Response.MakeErrorResponse();
                                }
                                #endregion

                                else if (IsSSFWRegistered(sessionid))
                                {
                                    //First check if this is a Inventory request
                                    if (absolutepath.Contains($"/RewardsService/") && absolutepath.Contains("counts"))
                                    {
                                        //Detect if existing inv exists
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
                                        else //fallback default 
                                            Response.MakeGetResponse(@"{ ""00000000-00000000-00000000-00000001"": 1 } ", "application/json");
                                    }
                                    //Check for specifically the Tracking GUID
                                    else if (absolutepath.Contains($"/RewardsService/") && absolutepath.Contains("object/00000000-00000000-00000000-00000001"))
                                    {
                                        //Detect if existing inv exists
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
                                        else //fallback default 
                                        {
#if DEBUG
                                            LoggerAccessor.LogWarn($"[SSFW] : {UserAgent} Non-existent inventories detected, using defaults!");
#endif
                                            if (absolutepath.Contains("p4t-cprod"))
                                            {
                                                #region Quest for Greatness
                                                Response.MakeGetResponse(@"{
                                                      ""result"": 0,
                                                      ""rewards"": {
                                                        ""00000000-00000000-00000000-00000001"": {
                                                          ""migrated"": 1,
                                                          ""_id"": ""1""
                                                        }
                                                      }
                                                    }", "application/json");
                                                #endregion
                                            }
                                            else
                                            {
                                                #region Pottermore
                                                Response.MakeGetResponse(@"{
                                                      ""result"": 0,
                                                      ""rewards"": [
                                                        {
                                                          ""00000000-00000000-00000000-00000001"": {
                                                          ""boost"": ""AQ=="",
                                                          ""_id"": ""tracking""
                                                          }
                                                        }
                                                      ]
                                                    }", "application/json");
                                                #endregion
                                            }

                                        }
                                    }
                                    else if (File.Exists(filePath + ".json"))
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
                                        LoggerAccessor.LogWarn($"[SSFW] : {UserAgent} Requested a non-existent file - {filePath}");
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

                                if (request.BodyLength <= Array.MaxLength)
                                {
                                    #region SSFW Login
                                    byte[] postbuffer = request.BodyBytes;
                                    if (absolutepath == $"/{LoginGUID}/login/token/psn")
                                    {
                                        string? XHomeClientVersion = GetHeaderValue(Headers, "X-HomeClientVersion");
                                        string? generalsecret = GetHeaderValue(Headers, "general-secret");

                                        if (!string.IsNullOrEmpty(XHomeClientVersion) && !string.IsNullOrEmpty(generalsecret))
                                        {
                                            SSFWLogin login = new(XHomeClientVersion, generalsecret, XHomeClientVersion.Replace(".", string.Empty).PadRight(6, '0'), GetHeaderValue(Headers, "x-signature"), legacykey);
                                            string? result = login.HandleLogin(postbuffer, env);
                                            if (!string.IsNullOrEmpty(result))
                                            {
                                                Response.Clear();
                                                Response.SetBegin(201);
                                                Response.SetContentType("application/json");
                                                Response.SetBody(result, encoding);
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
                                        Response.Clear();
                                        if (avatarLayout.HandleAvatarLayout(postbuffer, directoryPath, filePath, absolutepath, false))
                                            Response.SetBegin(200);
                                        else
                                            Response.SetBegin(403);
                                        Response.SetBody();
                                    }
                                    #endregion

                                    #region LayoutService
                                    else if (absolutepath.Contains($"/LayoutService/{env}/person/") && IsSSFWRegistered(sessionid))
                                    {
                                        Response.Clear();
                                        if (layout.HandleLayoutServicePOST(postbuffer, directoryPath, absolutepath))
                                            Response.SetBegin(200);
                                        else
                                            Response.SetBegin(403);
                                        Response.SetBody();
                                    }
                                    #endregion

                                    #region RewardsService
                                    else if (absolutepath.Contains($"/RewardsService/{env}/rewards/") && IsSSFWRegistered(sessionid))
                                        Response.MakeGetResponse(rewardSvc.HandleRewardServicePOST(postbuffer, directoryPath, filePath, absolutepath), "application/json");
                                    else if (absolutepath.Contains($"/RewardsService/trunks-{env}/trunks/") && absolutepath.Contains("/setpartial") && IsSSFWRegistered(sessionid))
                                    {
                                        rewardSvc.HandleRewardServiceTrunksPOST(postbuffer, directoryPath, filePath, absolutepath, env, SSFWUserSessionManager.GetIdBySessionId(sessionid));
                                        Response.MakeOkResponse();
                                    }
                                    else if (absolutepath.Contains($"/RewardsService/trunks-{env}/trunks/") && absolutepath.Contains("/set") && IsSSFWRegistered(sessionid))
                                    {
                                        rewardSvc.HandleRewardServiceTrunksEmergencyPOST(postbuffer, directoryPath, absolutepath);
                                        Response.MakeOkResponse();
                                    }
                                    else if (absolutepath.Contains($"/RewardsService/pmcards/")
                                        || absolutepath.Contains($"/RewardsService/p4t-cprod/")
                                        && IsSSFWRegistered(sessionid))
                                        Response.MakeGetResponse(rewardSvc.HandleRewardServiceInvPOST(postbuffer, directoryPath, filePath, absolutepath), "application/json");
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
                                                    if (absolutepath.Equals("/AuditService/log"))
                                                    {
                                                        auditService.HandleAuditService(absolutepath, putbuffer);
                                                        //Audit doesn't care we send ok!
                                                        Response.MakeOkResponse();
                                                    }
                                                    else
                                                    {
                                                        File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.json", putbuffer);
                                                        Response.MakeOkResponse();
                                                    }
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
                                        Response.Clear();
                                        if (avatarLayout.HandleAvatarLayout(request.BodyBytes, directoryPath, filePath, absolutepath, true))
                                            Response.SetBegin(200);
                                        else
                                            Response.SetBegin(403);
                                        Response.SetBody();
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
                                try
                                {
                                    string? extension = Path.GetExtension(filePath);

                                    if (!string.IsNullOrEmpty(extension) && allowedWebExtensions.Contains(extension))
                                    {
                                        if (File.Exists(filePath))
                                        {
                                            Response.Clear();
                                            Response.SetBegin(200);
                                            Response.SetContentType(HTTPProcessor.GetMimeType(extension, HTTPProcessor._mimeTypes));
                                            Response.SetBody(File.ReadAllBytes(filePath), encoding, GetHeaderValue(Headers, "Origin"));
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(404);
                                            Response.SetBody(string.Empty, null, GetHeaderValue(Headers, "Origin"));
                                        }
                                    }
                                    else
                                    {
                                        Response.Clear();
                                        Response.SetBegin(403);
                                        Response.SetBody(string.Empty, null, GetHeaderValue(Headers, "Origin"));
                                    }
                                }
                                catch
                                {
                                    Response.Clear();
                                    Response.SetBegin(500);
                                    Response.SetBody(string.Empty, null, GetHeaderValue(Headers, "Origin"));
                                }
                                break;
                            case "OPTIONS":
                                Response.Clear();
                                Response.SetBegin(200);
                                Response.SetHeader("Allow", HttpResponse.allowedMethods);
                                Response.SetBody(string.Empty, null, GetHeaderValue(Headers, "Origin"));
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
                                            KeyValuePair<string, string>? sceneData = ScenelistParser.GetSceneNameLike(sceneNameLike);

                                            if (sceneData != null && int.TryParse(sceneData.Value.Value, out int extractedId))
                                            {
                                                Response.SetBegin(200);
                                                Response.SetBody(sceneData.Value.Key + ',' + extractedId.ToUuid(), encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                            else
                                            {
                                                Response.SetBegin(500);
                                                Response.SetBody("SceneNameLike returned a null or empty sceneName!", encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                        }
                                        else
                                        {
                                            Response.SetBegin(403);
                                            Response.SetBody("Invalid like attribute was used!", encoding, GetHeaderValue(Headers, "Origin"));
                                        }
                                        break;
                                    case "/WebService/ApplyLayoutOverride/":
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);
                                        string targetUserName = GetHeaderValue(Headers, "targetUserName", false);
                                        string sceneId = GetHeaderValue(Headers, "sceneId", false);
                                        env = GetHeaderValue(Headers, "env", false);

                                        if (string.IsNullOrEmpty(env) || !SSFWMisc.homeEnvs.Contains(env))
                                            env = "cprod";

                                        Response.Clear();

                                        if (!string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(targetUserName) && !string.IsNullOrEmpty(sceneId) && IsSSFWRegistered(sessionId))
                                        {
                                            string? res = null;
                                            bool isRpcnUser = targetUserName.Contains("@RPCN");
                                            string LayoutDirectoryPath = Path.Combine(SSFWServerConfiguration.SSFWStaticFolder, $"LayoutService/{env}/person/");

                                            if (Directory.Exists(LayoutDirectoryPath))
                                            {
                                                string? matchingDirectory = null;
                                                string? username = SSFWUserSessionManager.GetUsernameBySessionId(sessionId);
                                                string? clientVersion = username?.Substring(username.Length - 6, 6);

                                                if (!string.IsNullOrEmpty(clientVersion))
                                                {
                                                    if (isRpcnUser)
                                                    {
                                                        string[] nameParts = targetUserName.Split('@');

                                                        if (nameParts.Length == 2 && !SSFWServerConfiguration.SSFWCrossSave)
                                                        {
                                                            matchingDirectory = Directory.GetDirectories(LayoutDirectoryPath)
                                                               .Where(dir =>
                                                                   Path.GetFileName(dir).StartsWith(nameParts[0]) &&
                                                                   Path.GetFileName(dir).Contains(nameParts[1]) &&
                                                                   Path.GetFileName(dir).Contains(clientVersion)
                                                               ).FirstOrDefault();
                                                        }
                                                        else
                                                            matchingDirectory = Directory.GetDirectories(LayoutDirectoryPath)
                                                              .Where(dir =>
                                                                  Path.GetFileName(dir).StartsWith(targetUserName.Replace("@RPCN", string.Empty)) &&
                                                                  Path.GetFileName(dir).Contains(clientVersion)
                                                              ).FirstOrDefault();
                                                    }
                                                    else
                                                        matchingDirectory = Directory.GetDirectories(LayoutDirectoryPath)
                                                          .Where(dir =>
                                                              Path.GetFileName(dir).StartsWith(targetUserName) &&
                                                              !Path.GetFileName(dir).Contains("RPCN") &&
                                                              Path.GetFileName(dir).Contains(clientVersion)
                                                          ).FirstOrDefault();
                                                }

                                                if (!string.IsNullOrEmpty(matchingDirectory))
                                                    res = new SSFWLayoutService(legacykey).HandleLayoutServiceGET(matchingDirectory, sceneId);

                                            } // if the dir not exists, we return 403.

                                            if (res == null)
                                            {
                                                Response.Clear();
                                                Response.SetBegin(403);
                                                Response.SetBody($"Override set for {sessionId}, but no layout was found for this scene.", encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                            else if (res == string.Empty)
                                            {
                                                Response.Clear();
                                                Response.SetBegin(404);
                                                Response.SetBody($"Override set for {sessionId}, but layout data was empty.", encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                            else
                                            {
                                                if (!LayoutGetOverrides.TryAdd(sessionId, res))
                                                    LayoutGetOverrides[sessionId] = res;

                                                Response.SetBegin(200);
                                                Response.SetContentType("application/json; charset=utf-8");
                                                Response.SetBody(res, encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                        }
                                        else
                                        {
                                            Response.SetBegin(403);
                                            Response.SetBody("Invalid sessionid or targetUserName attribute was used!", encoding, GetHeaderValue(Headers, "Origin"));
                                        }
                                        break;
                                    case "/WebService/R3moveLayoutOverride/":
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);

                                        Response.Clear();

                                        if (!string.IsNullOrEmpty(sessionId) && IsSSFWRegistered(sessionId))
                                        {
                                            if (LayoutGetOverrides.Remove(sessionId, out _))
                                            {
                                                Response.SetBegin(200);
                                                Response.SetBody($"Override removed for {sessionId}.", encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                            else
                                            {
                                                Response.SetBegin(404);
                                                Response.SetBody($"Override not found for {sessionId}.", encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                        }
                                        else
                                        {
                                            Response.SetBegin(403);
                                            Response.SetBody("Invalid sessionid attribute was used!", encoding, GetHeaderValue(Headers, "Origin"));
                                        }
                                        break;
                                    case "/WebService/GetMini/":
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);
                                        env = GetHeaderValue(Headers, "env", false);

                                        if (string.IsNullOrEmpty(env) || !SSFWMisc.homeEnvs.Contains(env))
                                            env = "cprod";

                                        userId = SSFWUserSessionManager.GetIdBySessionId(sessionId);

                                        if (!string.IsNullOrEmpty(userId))
                                        {
                                            string miniPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{userId}/mini.json";

                                            if (File.Exists(miniPath))
                                            {
                                                Response.Clear();

                                                try
                                                {
                                                    Response.SetBegin(200);
                                                    Response.SetContentType("application/json; charset=utf-8");
                                                    Response.SetBody(FileHelper.ReadAllText(miniPath, legacykey) ?? string.Empty, encoding, GetHeaderValue(Headers, "Origin"));
                                                }
                                                catch
                                                {
                                                    Response.SetBegin(500);
                                                    Response.SetBody($"Error while reading the mini file for User: {sessionId} on env:{env}!", encoding, GetHeaderValue(Headers, "Origin"));
                                                }
                                            }
                                            else
                                            {
                                                Response.Clear();
                                                Response.SetBegin(403);
                                                Response.SetBody($"User: {sessionId} on env:{env} doesn't have a ssfw mini file!", encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(403);
                                            Response.SetBody($"User: {sessionId} is not connected!", encoding, GetHeaderValue(Headers, "Origin"));
                                        }
                                        break;
                                    case "/WebService/AddMiniItem/":
                                        uuid = GetHeaderValue(Headers, "uuid", false);
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);
                                        env = GetHeaderValue(Headers, "env", false);

                                        if (string.IsNullOrEmpty(env) || !SSFWMisc.homeEnvs.Contains(env))
                                            env = "cprod";

                                        userId = SSFWUserSessionManager.GetIdBySessionId(sessionId);

                                        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(uuid) && byte.TryParse(GetHeaderValue(Headers, "invtype", false), out InventoryEntryType))
                                        {
                                            string miniPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{userId}/mini.json";

                                            if (File.Exists(miniPath))
                                            {
                                                try
                                                {
                                                    new SSFWRewardsService(legacykey).AddMiniEntry(uuid, InventoryEntryType, $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/trunks-{env}/trunks/{userId}.json", env, userId);
                                                    Response.Clear();
                                                    Response.SetBegin(200);
                                                    Response.SetBody($"UUID: {uuid} successfully added to the Mini rewards list.", encoding, GetHeaderValue(Headers, "Origin"));
                                                }
                                                catch (Exception ex)
                                                {
                                                    string errMsg = $"Mini rewards list file update errored out for file: {miniPath} (Exception: {ex})";
                                                    Response.Clear();
                                                    Response.SetBegin(500);
                                                    Response.SetBody(errMsg, encoding, GetHeaderValue(Headers, "Origin"));
                                                    LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                }
                                            }
                                            else
                                            {
                                                Response.Clear();
                                                Response.SetBegin(403);
                                                Response.SetBody($"User: {sessionId} on env:{env} doesn't have a ssfw mini file!", encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(403);
                                            Response.SetBody($"User: {sessionId} is not connected or sent invalid InventoryEntryType!", encoding, GetHeaderValue(Headers, "Origin"));
                                        }
                                        break;
                                    case "/WebService/AddMiniItems/":
                                        uuids = GetHeaderValue(Headers, "uuids", false).Split(',');
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);
                                        env = GetHeaderValue(Headers, "env", false);

                                        if (string.IsNullOrEmpty(env) || !SSFWMisc.homeEnvs.Contains(env))
                                            env = "cprod";

                                        userId = SSFWUserSessionManager.GetIdBySessionId(sessionId);

                                        if (!string.IsNullOrEmpty(userId) && uuids != null && byte.TryParse(GetHeaderValue(Headers, "invtype", false), out InventoryEntryType))
                                        {
                                            string miniPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{userId}/mini.json";

                                            if (File.Exists(miniPath))
                                            {
                                                Dictionary<string, byte> entriesToAdd = new();

                                                foreach (string iteruuid in uuids)
                                                {
                                                    entriesToAdd.TryAdd(iteruuid, InventoryEntryType);
                                                }
													
                                                try
                                                {
                                                    new SSFWRewardsService(legacykey).AddMiniEntries(entriesToAdd, $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/trunks-{env}/trunks/{userId}.json", env, userId);
                                                    Response.Clear();
                                                    Response.SetBegin(200);
                                                    Response.SetBody($"UUIDs: {string.Join(",", uuids)} successfully added to the Mini rewards list.", encoding, GetHeaderValue(Headers, "Origin"));
                                                }
                                                catch (Exception ex)
                                                {
                                                    string errMsg = $"Mini rewards list file update errored out for file: {miniPath} (Exception: {ex})";
                                                    Response.Clear();
                                                    Response.SetBegin(500);
                                                    Response.SetBody(errMsg, encoding, GetHeaderValue(Headers, "Origin"));
                                                    LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                }
                                            }
                                            else
                                            {
                                                Response.Clear();
                                                Response.SetBegin(403);
                                                Response.SetBody($"User: {sessionId} on env:{env} doesn't have a ssfw mini file!", encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(403);
                                            Response.SetBody($"User: {sessionId} is not connected or sent invalid InventoryEntryType!", encoding, GetHeaderValue(Headers, "Origin"));
                                        }
                                        break;
                                    case "/WebService/RemoveMiniItem/":
                                        uuid = GetHeaderValue(Headers, "uuid", false);
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);
                                        env = GetHeaderValue(Headers, "env", false);

                                        if (string.IsNullOrEmpty(env) || !SSFWMisc.homeEnvs.Contains(env))
                                            env = "cprod";

                                        userId = SSFWUserSessionManager.GetIdBySessionId(sessionId);

                                        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(uuid) && byte.TryParse(GetHeaderValue(Headers, "invtype", false), out InventoryEntryType))
                                        {
                                            string miniPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{userId}/mini.json";

                                            if (File.Exists(miniPath))
                                            {
                                                try
                                                {
                                                    new SSFWRewardsService(legacykey).RemoveMiniEntry(uuid, InventoryEntryType, $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/trunks-{env}/trunks/{userId}.json", env, userId);
                                                    Response.Clear();
                                                    Response.SetBegin(200);
                                                    Response.SetBody($"UUID: {uuid} successfully removed in the Mini rewards list.", encoding, GetHeaderValue(Headers, "Origin"));
                                                }
                                                catch (Exception ex)
                                                {
                                                    string errMsg = $"Mini rewards list file update errored out for file: {miniPath} (Exception: {ex})";
                                                    Response.Clear();
                                                    Response.SetBegin(500);
                                                    Response.SetBody(errMsg, encoding, GetHeaderValue(Headers, "Origin"));
                                                    LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                }
                                            }
                                            else
                                            {
                                                Response.Clear();
                                                Response.SetBegin(403);
                                                Response.SetBody($"User: {sessionId} on env:{env} doesn't have a ssfw mini file!", encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(403);
                                            Response.SetBody($"User: {sessionId} is not connected or sent invalid InventoryEntryType!", encoding, GetHeaderValue(Headers, "Origin"));
                                        }
                                        break;
                                    case "/WebService/RemoveMiniItems/":
                                        uuids = GetHeaderValue(Headers, "uuids", false).Split(',');
                                        sessionId = GetHeaderValue(Headers, "sessionid", false);
                                        env = GetHeaderValue(Headers, "env", false);

                                        if (string.IsNullOrEmpty(env) || !SSFWMisc.homeEnvs.Contains(env))
                                            env = "cprod";

                                        userId = SSFWUserSessionManager.GetIdBySessionId(sessionId);

                                        if (!string.IsNullOrEmpty(userId) && uuids != null && byte.TryParse(GetHeaderValue(Headers, "invtype", false), out InventoryEntryType))
                                        {
                                            string miniPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{userId}/mini.json";

                                            if (File.Exists(miniPath))
                                            {
                                                Dictionary<string, byte> entriesToRemove = new();

                                                foreach (string iteruuid in uuids)
                                                {
                                                    entriesToRemove.TryAdd(iteruuid, InventoryEntryType);
                                                }
												
                                                try
                                                {
                                                    new SSFWRewardsService(legacykey).RemoveMiniEntries(entriesToRemove, $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/trunks-{env}/trunks/{userId}.json", env, userId);
                                                    Response.Clear();
                                                    Response.SetBegin(200);
                                                    Response.SetBody($"UUIDs: {string.Join(",", uuids)} removed in the Mini rewards list.", encoding, GetHeaderValue(Headers, "Origin"));
                                                }
                                                catch (Exception ex)
                                                {
                                                    string errMsg = $"Mini rewards list file update errored out for file: {miniPath} (Exception: {ex})";
                                                    Response.Clear();
                                                    Response.SetBegin(500);
                                                    Response.SetBody(errMsg, encoding, GetHeaderValue(Headers, "Origin"));
                                                    LoggerAccessor.LogError($"[SSFW] - {errMsg}");
                                                }
                                            }
                                            else
                                            {
                                                Response.Clear();
                                                Response.SetBegin(403);
                                                Response.SetBody($"User: {sessionId} on env:{env} doesn't have a ssfw mini file!", encoding, GetHeaderValue(Headers, "Origin"));
                                            }
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(403);
                                            Response.SetBody($"User: {sessionId} is not connected or sent invalid InventoryEntryType!", encoding, GetHeaderValue(Headers, "Origin"));
                                        }
                                        break;
                                    default:
                                        Response.Clear();
                                        Response.SetBegin(403);
                                        Response.SetBody(string.Empty, null, GetHeaderValue(Headers, "Origin"));
                                        break;
                                }
                                break;
                            default:
                                Response.Clear();
                                Response.SetBegin(403);
                                Response.SetBody(string.Empty, null, GetHeaderValue(Headers, "Origin"));
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
