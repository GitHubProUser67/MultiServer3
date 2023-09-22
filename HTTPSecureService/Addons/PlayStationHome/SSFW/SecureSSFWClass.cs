using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.SSFW
{
    public class SecureSSFWClass
    {
        public static HttpsRestServer server = null;

        public static bool httpsstarted = false;

        private class HttpsRestSession : HttpsSession
        {
            public HttpsRestSession(HttpsServer server) : base(server) { }

            protected override void OnReceivedRequest(HttpRequest request)
            {
                Response.Clear();

                HttpResponse response = Response;

                (string HeaderIndex, string HeaderItem)[] Headers = HTTPSClass.CollectHeaders(request);

                if (request.Url == null || request.Url == "")
                {
                    ServerConfiguration.LogError($"[SECURITY] - Client - {HTTPSClass.GetHeaderValue(Headers, "Host")} Requested the Secured SSFW server while being banned!");

                    response.SetBegin((int)HttpStatusCode.Forbidden);
                    response.SetBody();
                    SendResponseAsync();
                    return;
                }
                else
                    ServerConfiguration.LogInfo($"[SECURITY] - Client - {HTTPSClass.GetHeaderValue(Headers, "Host")} Requested the Secured SSFW server.");

                ServerConfiguration.LogInfo($"[SSFW] - {HTTPSClass.GetHeaderValue(Headers, "User-Agent")} Requested {request.Url}");

                string url = request.Url;

                // Split the URL into segments
                string[] segments = url.Trim('/').Split('/');

                // Combine the folder segments into a directory path
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                // Process the request based on the HTTP method
                string filePath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder, url.Substring(1));

                switch (request.Method)
                {
                    case "PUT":
                        if (HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                            PUT.handlePUT(directoryPath, request, response, Headers);
                        else
                        {
                            response.SetBegin((int)HttpStatusCode.Forbidden);
                            response.SetBody();
                        }
                        break;
                    case "POST":
                        POST.handlePOST(directoryPath, filePath, request, response, Headers);
                        break;
                    case "GET":
                        if (request.Url.Contains("/LayoutService/cprod/person/") && HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                            SSFWLayoutService.HandleLayoutServiceGET(directoryPath, request.Url, request, response);
                        else if (request.Url.Contains("/AdminObjectService/start") && HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                            SSFWAdminObjectService.HandleAdminObjectService(response, Headers);
                        else if (request.Url.Contains($"/SaveDataService/cprod/{segments.LastOrDefault()}") && HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                            SSFWGetFileList.SSFWSaveDataDebugGetFileList(directoryPath, request.Url, response, segments);
                        else if (HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                        {
                            if (File.Exists(filePath + ".json"))
                            {
                                byte[] byteresponse = HTTPService.FileHelper.CryptoReadAsync(filePath + ".json", SSFWPrivateKey.SSFWPrivatekey);

                                if (byteresponse != null)
                                {
                                    response.SetBegin((int)HttpStatusCode.OK);
                                    if (HTTPSClass.GetHeaderValue(Headers, "Accept") == "application/json")
                                        response.SetContentType("application/json");
                                    response.SetBody(byteresponse);
                                }
                                else
                                {
                                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                                    response.SetBody();
                                }
                            }
                            else if (File.Exists(filePath + ".bin"))
                            {
                                byte[] byteresponse = HTTPService.FileHelper.CryptoReadAsync(filePath + ".bin", SSFWPrivateKey.SSFWPrivatekey);

                                if (byteresponse != null)
                                {
                                    response.SetBegin((int)HttpStatusCode.OK);
                                    response.SetBody(byteresponse);
                                }
                                else
                                {
                                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                                    response.SetBody();
                                }
                            }
                            else if (File.Exists(filePath + ".jpeg"))
                            {
                                byte[] byteresponse = HTTPService.FileHelper.CryptoReadAsync(filePath + ".jpeg", SSFWPrivateKey.SSFWPrivatekey);

                                if (byteresponse != null)
                                {
                                    response.SetBegin((int)HttpStatusCode.OK);
                                    response.SetBody(byteresponse);
                                }
                                else
                                {
                                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                                    response.SetBody();
                                }
                            }
                            else
                            {
                                ServerConfiguration.LogWarn($"[HTTPS - SSFW] : {HTTPSClass.GetHeaderValue(Headers, "User-Agent")} Requested a file that doesn't exist - {filePath}");
                                response.SetBegin((int)HttpStatusCode.NotFound);
                                response.SetBody();
                            }
                        }
                        else
                        {
                            response.SetBegin((int)HttpStatusCode.Forbidden);
                            response.SetBody();
                        }
                        break;
                    case "DELETE":
                        if (request.Url.Contains("/AvatarLayoutService/cprod/") && HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                            SSFWAvatarLayoutService.HandleAvatarLayout(directoryPath, filePath, request.Url, request, response, true);
                        else if (HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                        {
                            if (File.Exists(filePath + ".json"))
                            {
                                File.Delete(filePath + ".json");
                                response.SetBegin((int)HttpStatusCode.OK);
                                response.SetBody();
                            }
                            else if (File.Exists(filePath + ".bin"))
                            {
                                File.Delete(filePath + ".bin");
                                response.SetBegin((int)HttpStatusCode.OK);
                                response.SetBody();
                            }
                            else if (File.Exists(filePath + ".jpeg"))
                            {
                                File.Delete(filePath + ".jpeg");
                                response.SetBegin((int)HttpStatusCode.OK);
                                response.SetBody();
                            }
                            else
                            {
                                ServerConfiguration.LogError($"[HTTPS SSFW] : {HTTPSClass.GetHeaderValue(Headers, "User-Agent")} Requested a file to delete that doesn't exist - {filePath}");
                                response.SetBegin((int)HttpStatusCode.NotFound);
                                response.SetBody();
                            }
                        }
                        else
                        {
                            response.SetBegin((int)HttpStatusCode.Forbidden);
                            response.SetBody();
                        }
                        break;
                    default:
                        response.SetBegin((int)HttpStatusCode.Forbidden);
                        response.SetBody();
                        break;
                }

                SendResponseAsync(); // Final step, ideally the only one to exist if the APIs are made right.
            }

            protected override void OnReceivedRequestError(HttpRequest request, string error)
            {
                ServerConfiguration.LogError($"[SSFW] - Request error: {error}");
            }

            protected override void OnError(SocketError error)
            {
                ServerConfiguration.LogError($"[SSFW] - session caught an error: {error}");
            }
        }

        public class HttpsRestServer : HttpsServer
        {
            public HttpsRestServer(SslContext context, IPAddress address, int port) : base(context, address, port) { }

            protected override SslSession CreateSession() { return new HttpsRestSession(this); }

            protected override void OnError(SocketError error)
            {
                ServerConfiguration.LogError($"[SSFW] - server caught an error: {error}");
            }
        }

        public static Task StopSSFWSecureServer()
        {
            if (server != null && server.IsStarted)
            {
                server.Stop();
                httpsstarted = false;
                ServerConfiguration.LogError($"[SSFW] - server Stopped!");
            }
            return Task.CompletedTask;
        }

        public static Task StartSSFWSecureServer()
        {
            try
            {
                // Create and prepare a new SSL server context
                var context = new SslContext(SslProtocols.Tls12, new X509Certificate2(Directory.GetCurrentDirectory() + "/static/SSL/MultiServer.pfx", "qwerty"));

                // Create a new HTTP server
                server = new HttpsRestServer(context, IPAddress.Any, 10443);
                server.OptionDualMode = true;
                server.OptionKeepAlive = true;

                // Start the server
                server.Start();
                httpsstarted = true;
                ServerConfiguration.LogInfo($"[SSFW] - server started on: 10443");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[SSFW] - server Start has errored out - {ex}");
            }

            return Task.CompletedTask;
        }
    }
}
