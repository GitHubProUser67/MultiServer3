using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NetCoreServer;
using CustomLogger;
using System.Text.RegularExpressions;
using CryptoSporidium.FileHelper;

namespace SSFWServer
{
    public class SSFWClass
    {
        public static bool IsStarted = false;
        public static string? legacykey;
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
                LoggerAccessor.LogInfo($"[CollectHeaders] - Debug Headers : HeaderIndex -> {CollectHeader[i].HeaderIndex} | HeaderItem -> {CollectHeader[i].HeaderItem}");
#endif
            }

            return CollectHeader;
        }

        public static string GetHeaderValue((string HeaderIndex, string HeaderItem)[] headers, string requestedHeaderIndex)
        {
            string pattern = @"^(.*?):\s(.*)$"; // Make a GITHUB ticket for netcoreserver, the header tuple can get out of sync with null values, we try to mitigate the problem.

            foreach ((string HeaderIndex, string HeaderItem) header in headers)
            {
                Match match = Regex.Match(header.HeaderItem, pattern);

                if (header.HeaderIndex == requestedHeaderIndex)
                    return header.HeaderItem;
                else if (header.HeaderItem.Contains(requestedHeaderIndex) && match.Success) // Make a GITHUB ticket for netcoreserver, the header tuple can get out of sync with null values, we try to mitigate the problem.
                    return match.Groups[2].Value;
            }
            return string.Empty; // Return empty string if the header index is not found, why not null, because in this case it prevents us
                                 // from doing extensive checks everytime we want to display the User-Agent in particular.
        }

        public static bool IsIPBanned(string ipAddress)
        {
            if (SSFWServerConfiguration.BannedIPs != null && SSFWServerConfiguration.BannedIPs.Contains(ipAddress))
                return true;

            return false;
        }

        public Task StartSSFW()
        {
            // Create and prepare a new SSL server context
            var context = new SslContext(SslProtocols.Tls12, new X509Certificate2(certpath, certpass));

            // Create a new HTTP server
            var server = new SSFWServer(context, IPAddress.Any, 10443);

            // Start the server
            server.Start();
            IsStarted = true;
            LoggerAccessor.LogInfo("[SSFW] - Server started...");

            return Task.CompletedTask;
        }

        private class SSFWSession : HttpsSession
        {
            public SSFWSession(HttpsServer server) : base(server) { }

            protected override void OnReceivedRequest(HttpRequest request)
            {
                try
                {
                    (string HeaderIndex, string HeaderItem)[] Headers = CollectHeaders(request);

                    string UserAgent = GetHeaderValue(Headers, "User-Agent");

                    if (!string.IsNullOrEmpty(UserAgent) && !string.IsNullOrEmpty(request.Url) && UserAgent.Contains("PSHome") && !IsIPBanned(GetHeaderValue(Headers, "Host")))
                    {
                        string sessionid = GetHeaderValue(Headers, "X-Home-Session-Id");

                        string absolutepath = request.Url;

                        LoggerAccessor.LogInfo($"[SSFW] - Home Client Requested : {absolutepath}");

                        // Split the URL into segments
                        string[] segments = absolutepath.Trim('/').Split('/');

                        // Combine the folder segments into a directory path
                        string directoryPath = Path.Combine(SSFWServerConfiguration.SSFWStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                        // Process the request based on the HTTP method
                        string filePath = Path.Combine(SSFWServerConfiguration.SSFWStaticFolder, absolutepath.Substring(1));

                        switch (request.Method)
                        {
                            case "GET":
                                if (absolutepath.Contains("/LayoutService/cprod/person/") && !string.IsNullOrEmpty(sessionid))
                                {
                                    SSFWLayoutService layout = new(legacykey);
                                    string? res = layout.HandleLayoutServiceGET(directoryPath, filePath);
                                    if (res == null)
                                    {
                                        Response.Clear();
                                        Response.SetBegin(403);
                                        Response.SetBody();
                                        SendResponseAsync(Response);
                                    }
                                    else if (res == string.Empty)
                                    {
                                        Response.Clear();
                                        Response.SetBegin(404);
                                        Response.SetBody();
                                        SendResponseAsync(Response);
                                    }
                                    else
                                        SendResponseAsync(Response.MakeGetResponse(res, "application/json"));
                                    layout.Dispose();
                                }
                                else if (absolutepath.Contains("/AdminObjectService/start") && !string.IsNullOrEmpty(sessionid))
                                {
                                    SSFWAdminObjectService iga = new(sessionid, legacykey);
                                    Response.Clear();
                                    if (iga.HandleAdminObjectService(UserAgent))
                                        Response.SetBegin(200);
                                    else
                                        Response.SetBegin(403);
                                    Response.SetBody();
                                    SendResponseAsync(Response);
                                    iga.Dispose();
                                }
                                else if (absolutepath.Contains($"/SaveDataService/cprod/{segments.LastOrDefault()}") && !string.IsNullOrEmpty(sessionid))
                                {
                                    SSFWGetFileList filelist = new();
                                    string? res = filelist.SSFWSaveDataDebugGetFileList(directoryPath, segments.LastOrDefault());
                                    if (res != null)
                                        SendResponseAsync(Response.MakeGetResponse(res, "application/json"));
                                    else
                                    {
                                        Response.Clear();
                                        Response.SetBegin(500);
                                        Response.SetBody();
                                        SendResponseAsync(Response);
                                    }
                                    filelist.Dispose();
                                }
                                else if (!string.IsNullOrEmpty(sessionid))
                                {
                                    if (File.Exists(filePath + ".json"))
                                    {
                                        string? res = FileHelper.ReadAllText(filePath + ".json", legacykey);

                                        if (!string.IsNullOrEmpty(res))
                                        {
                                            if (GetHeaderValue(Headers, "Accept") == "application/json")
                                                SendResponseAsync(Response.MakeGetResponse(res, "application/json"));
                                            else
                                                SendResponseAsync(Response.MakeGetResponse(res));
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(500);
                                            Response.SetBody();
                                            SendResponseAsync(Response);
                                        }
                                    }
                                    else if (File.Exists(filePath + ".bin"))
                                    {
                                        byte[]? res = FileHelper.ReadAllBytes(filePath + ".bin", legacykey);

                                        if (res != null)
                                            SendResponseAsync(Response.MakeGetResponse(res, "application/octet-stream"));
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(500);
                                            Response.SetBody();
                                            SendResponseAsync(Response);
                                        }
                                    }
                                    else if (File.Exists(filePath + ".jpeg"))
                                    {
                                        byte[]? res = FileHelper.ReadAllBytes(filePath + ".jpeg", legacykey);

                                        if (res != null)
                                            SendResponseAsync(Response.MakeGetResponse(res, "image/jpeg"));
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(500);
                                            Response.SetBody();
                                            SendResponseAsync(Response);
                                        }
                                    }
                                    else
                                    {
                                        LoggerAccessor.LogWarn($"[SSFW] : {UserAgent} Requested a non-exisant file - {filePath}");
                                        Response.Clear();
                                        Response.SetBegin(404);
                                        Response.SetBody();
                                        SendResponseAsync(Response);
                                    }
                                }
                                else
                                {
                                    Response.Clear();
                                    Response.SetBegin(403);
                                    Response.SetBody();
                                    SendResponseAsync(Response);
                                }
                                break;
                            case "POST":
                                // Create a byte array
                                byte[] postbuffer = request.BodyBytes;
                                if (absolutepath == "/bb88aea9-6bf8-4201-a6ff-5d1f8da0dd37/login/token/psn")
                                {
                                    string? XHomeClientVersion = GetHeaderValue(Headers, "X-HomeClientVersion");
                                    string? generalsecret = GetHeaderValue(Headers, "general-secret");

                                    if (!string.IsNullOrEmpty(XHomeClientVersion) && !string.IsNullOrEmpty(generalsecret))
                                    {
                                        SSFWLogin login = new(XHomeClientVersion, generalsecret, XHomeClientVersion.Replace(".", ""), GetHeaderValue(Headers, "x-signature"), legacykey);
                                        string? result = login.HandleLogin(request.BodyBytes, "cprod"); // Todo, make env maybe more dynamic?
                                        if (result != null)
                                        {
                                            Response.Clear();
                                            Response.SetBegin(201);
                                            Response.SetContentType("application/json");
                                            Response.SetBody(result);
                                            SendResponseAsync(Response);
                                        }
                                        else
                                        {
                                            Response.Clear();
                                            Response.SetBegin(500);
                                            Response.SetBody();
                                            SendResponseAsync(Response);
                                        }
                                        login.Dispose();
                                    }
                                    else
                                    {
                                        Response.Clear();
                                        Response.SetBegin(403);
                                        Response.SetBody();
                                        SendResponseAsync(Response);
                                    }
                                }
                                else if (absolutepath.Contains("/morelife") && !string.IsNullOrEmpty(GetHeaderValue(Headers, "x-signature")))
                                    SendResponseAsync(Response.MakeGetResponse("{}", "application/json"));
                                else if (absolutepath.Contains("/AvatarLayoutService/cprod/") && !string.IsNullOrEmpty(sessionid))
                                {
                                    SSFWAvatarLayoutService layout = new(sessionid, legacykey);
                                    Response.Clear();
                                    if (layout.HandleAvatarLayout(postbuffer, directoryPath, filePath, absolutepath, false))
                                        Response.SetBegin(200);
                                    else
                                        Response.SetBegin(403);
                                    Response.SetBody();
                                    SendResponseAsync(Response);
                                    layout.Dispose();
                                }
                                else if (absolutepath.Contains("/LayoutService/cprod/person/") && !string.IsNullOrEmpty(sessionid))
                                {
                                    SSFWLayoutService layout = new(legacykey);
                                    Response.Clear();
                                    if (layout.HandleLayoutServicePOST(postbuffer, directoryPath, absolutepath))
                                        Response.SetBegin(200);
                                    else
                                        Response.SetBegin(403);
                                    Response.SetBody();
                                    SendResponseAsync(Response);
                                    layout.Dispose();
                                }
                                else if (absolutepath.Contains("/RewardsService/cprod/rewards/") && !string.IsNullOrEmpty(sessionid))
                                {
                                    SSFWRewardsService reward = new(legacykey);
                                    SendResponseAsync(Response.MakeGetResponse(reward.HandleRewardServicePOST(postbuffer, directoryPath, filePath, absolutepath), "application/json"));
                                    reward.Dispose();
                                }
                                else if (absolutepath.Contains("/RewardsService/trunks-cprod/trunks/") && absolutepath.Contains("/setpartial") && !string.IsNullOrEmpty(sessionid))
                                {
                                    SSFWRewardsService reward = new(legacykey);
                                    reward.HandleRewardServiceTrunksPOST(postbuffer, directoryPath, filePath, absolutepath);
                                    SendResponseAsync(Response.MakeOkResponse());
                                    reward.Dispose();
                                }
                                else if (absolutepath.Contains("/RewardsService/trunks-cprod/trunks/") && absolutepath.Contains("/set") && !string.IsNullOrEmpty(sessionid))
                                {
                                    SSFWRewardsService reward = new(legacykey);
                                    reward.HandleRewardServiceTrunksEmergencyPOST(postbuffer, directoryPath, absolutepath);
                                    SendResponseAsync(Response.MakeOkResponse());
                                    reward.Dispose();
                                }
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
                                    SendResponseAsync(Response.MakeOkResponse());
                                }
                                else
                                {
                                    Response.Clear();
                                    Response.SetBegin(403);
                                    Response.SetBody();
                                    SendResponseAsync(Response);
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
                                        SendResponseAsync(Response.MakeOkResponse());
                                    }
                                    else
                                    {
                                        Response.Clear();
                                        Response.SetBegin(403);
                                        Response.SetBody();
                                        SendResponseAsync(Response);
                                    }
                                }
                                else
                                {
                                    Response.Clear();
                                    Response.SetBegin(403);
                                    Response.SetBody();
                                    SendResponseAsync(Response);
                                }
                                break;
                            case "DELETE":
                                if (absolutepath.Contains("/AvatarLayoutService/cprod/") && !string.IsNullOrEmpty(sessionid))
                                {
                                    SSFWAvatarLayoutService layout = new(sessionid, legacykey);
                                    Response.Clear();
                                    if (layout.HandleAvatarLayout(request.BodyBytes, directoryPath, filePath, absolutepath, true))
                                        Response.SetBegin(200);
                                    else
                                        Response.SetBegin(403);
                                    Response.SetBody();
                                    SendResponseAsync(Response);
                                    layout.Dispose();
                                }
                                else if (!string.IsNullOrEmpty(sessionid))
                                {
                                    if (File.Exists(filePath + ".json"))
                                    {
                                        File.Delete(filePath + ".json");
                                        SendResponseAsync(Response.MakeOkResponse());
                                    }
                                    else if (File.Exists(filePath + ".bin"))
                                    {
                                        File.Delete(filePath + ".bin");
                                        SendResponseAsync(Response.MakeOkResponse());
                                    }
                                    else if (File.Exists(filePath + ".jpeg"))
                                    {
                                        File.Delete(filePath + ".jpeg");
                                        SendResponseAsync(Response.MakeOkResponse());
                                    }
                                    else
                                    {
                                        LoggerAccessor.LogError($"[SSFW] : {UserAgent} Requested a file to delete that doesn't exist - {filePath}");
                                        Response.Clear();
                                        Response.SetBegin(404);
                                        Response.SetBody();
                                        SendResponseAsync(Response);
                                    }
                                }
                                else
                                {
                                    Response.Clear();
                                    Response.SetBegin(403);
                                    Response.SetBody();
                                    SendResponseAsync(Response);
                                }
                                break;
                            default:
                                Response.Clear();
                                Response.SetBegin(403);
                                Response.SetBody();
                                SendResponseAsync(Response);
                                break;
                        }
                    }
                    else
                    {
                        Response.Clear();
                        Response.SetBegin(403);
                        Response.SetBody();
                        SendResponseAsync(Response);
                    }
                }
                catch (Exception e)
                {
                    Response.Clear();
                    Response.SetBegin(500);
                    Response.SetBody();
                    SendResponseAsync(Response);
                    LoggerAccessor.LogError($"[SSFW] - Request thrown an error : {e}");
                }
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

        private class SSFWServer : HttpsServer
        {
            public SSFWServer(SslContext context, IPAddress address, int port) : base(context, address, port) { }

            protected override SslSession CreateSession() { return new SSFWSession(this); }

            protected override void OnError(SocketError error)
            {
                LoggerAccessor.LogError($"[SSFW] - Server caught an error: {error}");
            }
        }
    }
}
