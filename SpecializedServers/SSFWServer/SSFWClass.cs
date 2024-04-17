using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NetCoreServer;
using CustomLogger;
using System.Text.RegularExpressions;
using System.Net.Security;
using SSFWServer.Services;

namespace SSFWServer
{
    public class SSFWClass
    {
        public static bool IsStarted = false;
        public static string? legacykey;
        public static SSFWServer? _Server;
        public static HttpSSFWServer? _HttpServer;
        private string certpath;
        private string certpass;

        public SSFWClass(string certpath, string certpass, string? locallegacykey)
        {
            this.certpath = certpath;
            this.certpass = certpass;
            legacykey = locallegacykey;
        }

        public static (string HeaderIndex, string HeaderItem)[] CollectHeaders(HttpRequest request)
        {
            int headerindex = (int)request.Headers; // There is a slight mistake in netcoreserver, where the index is long, and the parser is int
                                                    // So we accomodate that with a cast.

            (string HeaderIndex, string HeaderItem)[] CollectHeader = new (string, string)[headerindex];

            for (int i = 0; i < headerindex; i++)
            {
                CollectHeader[i] = request.Header(i);
#if DEBUG
                LoggerAccessor.LogInfo($"[SSFW] - CollectHeaders - Debug Headers : HeaderIndex -> {CollectHeader[i].HeaderIndex} | HeaderItem -> {CollectHeader[i].HeaderItem}");
#endif
            }

            return CollectHeader;
        }

        public static string GetHeaderValue((string HeaderIndex, string HeaderItem)[] headers, string requestedHeaderIndex)
        {
            string pattern = @"^(.*?):\s(.*)$"; // Make a GITHUB ticket for netcoreserver, the header tuple can get out of sync with null values, we try to mitigate the problem.

            foreach ((string HeaderIndex, string HeaderItem) in headers)
            {
                Match match = Regex.Match(HeaderItem, pattern);

                if (HeaderIndex == requestedHeaderIndex)
                    return HeaderItem;
                else if (HeaderItem.Contains(requestedHeaderIndex) && match.Success) // Make a GITHUB ticket for netcoreserver, the header tuple can get out of sync with null values, we try to mitigate the problem.
                    return match.Groups[2].Value;
            }
            return string.Empty; // Return empty string if the header index is not found, why not null, because in this case it prevents us
                                 // from doing extensive checks everytime we want to display the User-Agent in particular.
        }

        public Task StartSSFW()
        {
            // Create and prepare a new SSL server context and start the server
            _Server = new SSFWServer(new SslContext(SslProtocols.Tls12, new X509Certificate2(certpath, certpass), MyRemoteCertificateValidationCallback), IPAddress.Any, 10443);
            // Create and start the server
            _HttpServer = new HttpSSFWServer(IPAddress.Any, 8080);

            _ = Task.Run(() => Parallel.Invoke(
                    () => _Server.Start(),
                    () => _HttpServer.Start()
                ));

            IsStarted = true;
            LoggerAccessor.LogInfo("[SSFW] - Server started on ports 8080 and 10443...");

            return Task.CompletedTask;
        }

        private static HttpResponse SSFWRequestProcess(HttpRequest request, HttpResponse Response)
        {
            try
            {
                (string HeaderIndex, string HeaderItem)[] Headers = CollectHeaders(request);

                string UserAgent = GetHeaderValue(Headers, "User-Agent");

                if (!string.IsNullOrEmpty(request.Url) && UserAgent.Contains("PSHome") && UserAgent.Contains("CellOS")) // Host ban is not perfect, but netcoreserver only has that to offer...
                {
                    LoggerAccessor.LogInfo($"[SSFW] - Home Client Requested the SSFW Server with URL : {request.Method} {request.Url}");

                    string sessionid = GetHeaderValue(Headers, "X-Home-Session-Id");

                    string absolutepath = request.Url;

                    // Split the URL into segments
                    string[] segments = absolutepath.Trim('/').Split('/');

                    // Combine the folder segments into a directory path
                    string directoryPath = Path.Combine(SSFWServerConfiguration.SSFWStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                    // Process the request based on the HTTP method
                    string filePath = Path.Combine(SSFWServerConfiguration.SSFWStaticFolder, absolutepath[1..]);

                    switch (request.Method)
                    {
                        case "GET":

                            #region LayoutService
                            if (absolutepath.Contains("/LayoutService/cprod/person/") && !string.IsNullOrEmpty(sessionid))
                            {
                                SSFWLayoutService layout = new(legacykey);
                                string? res = layout.HandleLayoutServiceGET(directoryPath, filePath);
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
                            else if (absolutepath.Contains("/AdminObjectService/start") && !string.IsNullOrEmpty(sessionid))
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
                            else if (absolutepath.Contains($"/SaveDataService/cprod/{segments.LastOrDefault()}") && !string.IsNullOrEmpty(sessionid))
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

                            else if (!string.IsNullOrEmpty(sessionid))
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
                            else
                            {
                                Response.Clear();
                                Response.SetBegin(403);
                                Response.SetBody();
                            }
                            break;
                        case "POST":

                            #region SSFW Login
                            // Create a byte array
                            byte[] postbuffer = request.BodyBytes;
                            if (absolutepath == "/bb88aea9-6bf8-4201-a6ff-5d1f8da0dd37/login/token/psn")
                            {
                                string? XHomeClientVersion = GetHeaderValue(Headers, "X-HomeClientVersion");
                                string? generalsecret = GetHeaderValue(Headers, "general-secret");

                                if (!string.IsNullOrEmpty(XHomeClientVersion) && !string.IsNullOrEmpty(generalsecret))
                                {
                                    SSFWLogin login = new(XHomeClientVersion, generalsecret, XHomeClientVersion.Replace(".", string.Empty), GetHeaderValue(Headers, "x-signature"), legacykey);
                                    string? result = login.HandleLogin(request.BodyBytes, "cprod"); // Todo, make env maybe more dynamic?
                                    if (result != null)
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
                                Response.MakeGetResponse("{}", "application/json");
                            #endregion

                            #region AvatarLayoutService
                            else if (absolutepath.Contains("/AvatarLayoutService/cprod/") && !string.IsNullOrEmpty(sessionid))
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
                            else if (absolutepath.Contains("/LayoutService/cprod/person/") && !string.IsNullOrEmpty(sessionid))
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
                            else if (absolutepath.Contains("/RewardsService/cprod/rewards/") && !string.IsNullOrEmpty(sessionid))
                            {
                                SSFWRewardsService reward = new(legacykey);
                                Response.MakeGetResponse(reward.HandleRewardServicePOST(postbuffer, directoryPath, filePath, absolutepath), "application/json");
                                reward.Dispose();
                            }
                            else if (absolutepath.Contains("/RewardsService/trunks-cprod/trunks/") && absolutepath.Contains("/setpartial") && !string.IsNullOrEmpty(sessionid))
                            {
                                SSFWRewardsService reward = new(legacykey);
                                reward.HandleRewardServiceTrunksPOST(postbuffer, directoryPath, filePath, absolutepath);
                                Response.MakeOkResponse();
                                reward.Dispose();
                            }
                            else if (absolutepath.Contains("/RewardsService/trunks-cprod/trunks/") && absolutepath.Contains("/set") && !string.IsNullOrEmpty(sessionid))
                            {
                                SSFWRewardsService reward = new(legacykey);
                                reward.HandleRewardServiceTrunksEmergencyPOST(postbuffer, directoryPath, absolutepath);
                                Response.MakeOkResponse();
                                reward.Dispose();
                            }
                            #endregion

                            else if (!string.IsNullOrEmpty(sessionid))
                            {
                                LoggerAccessor.LogWarn($"[SSFW] : Host requested a POST method I don't know about! - Report it to GITHUB with the request : {absolutepath}");
                                if (postbuffer != null)
                                {
                                    Directory.CreateDirectory(directoryPath);
                                    switch (GetHeaderValue(Headers, "Content-type"))
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
                            break;
                        case "PUT":
                            if (!string.IsNullOrEmpty(sessionid))
                            {
                                // Create a byte array
                                byte[] putbuffer = request.BodyBytes;
                                if (putbuffer != null)
                                {
                                    Directory.CreateDirectory(directoryPath);
                                    switch (GetHeaderValue(Headers, "Content-type"))
                                    {
                                        case "image/jpeg":
                                            File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.jpeg", putbuffer);
                                            break;
                                        case "application/json":
                                            File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.json", putbuffer);
                                            break;
                                        default:
                                            File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.bin", putbuffer);
                                            break;
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
                                Response.SetBegin(403);
                                Response.SetBody();
                            }
                            break;
                        case "DELETE":

                            #region AvatarLayoutService
                            if (absolutepath.Contains("/AvatarLayoutService/cprod/") && !string.IsNullOrEmpty(sessionid))
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
                            #endregion

                            else if (!string.IsNullOrEmpty(sessionid))
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
                    LoggerAccessor.LogInfo($"[SSFW] - Client Requested the SSFW Server with invalid parameters!");
                    Response.Clear();
                    Response.SetBegin(403);
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
