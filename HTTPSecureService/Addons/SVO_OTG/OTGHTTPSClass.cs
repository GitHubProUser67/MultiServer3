using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using HttpMultipartParser;
using System.Text;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.SVO_OTG
{
    public class OTGHTTPSClass
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
                    ServerConfiguration.LogError($"[SECURITY] - Client - {HTTPSClass.GetHeaderValue(Headers, "Host")} Requested the SVO - HTTPS server while being banned!");

                    response.SetBegin((int)HttpStatusCode.Forbidden);
                    response.SetBody();
                    SendResponseAsync();
                    return;
                }
                else
                    ServerConfiguration.LogInfo($"[SECURITY] - Client - {HTTPSClass.GetHeaderValue(Headers, "Host")} Requested the SVO - HTTPS server.");

                ServerConfiguration.LogInfo($"[OTG - HTTPS] - {HTTPSClass.GetHeaderValue(Headers, "Host")} Requested {request.Url}");

                string url = request.Url;

                switch (url)
                {
                    case "/":
                        response.SetBegin((int)HttpStatusCode.Forbidden);
                        response.SetBody();
                        break;
                    case "\\":
                        response.SetBegin((int)HttpStatusCode.Forbidden);
                        response.SetBody();
                        break;
                    case "/dataloaderweb/queue":
                        if (HTTPSClass.GetHeaderValue(Headers, "Content-type").StartsWith("multipart/form-data"))
                        {
                            switch (request.Method)
                            {
                                case "POST":
                                    response.SetContentType("application/xml;charset=UTF-8");
                                    response.SetHeader("Content-Language", "");

                                    string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                                        ms.Position = 0;

                                        var data = MultipartFormDataParser.Parse(ms, boundary);

                                        byte[] datatooutput = Encoding.UTF8.GetBytes(data.GetParameterValue("body"));

                                        DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}/dataloaderweb/queue");

                                        FileInfo[] files = directory.GetFiles();

                                        if (files.Length >= 20)
                                        {
                                            FileInfo oldestFile = files.OrderBy(file => file.CreationTime).First();
                                            ServerConfiguration.LogInfo("[SVO] - Replacing log file: " + oldestFile.Name);

                                            File.WriteAllBytes(oldestFile.FullName, datatooutput);
                                        }
                                        else
                                            File.WriteAllBytes(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}/dataloaderweb/queue/{Guid.NewGuid()}.xml", datatooutput);

                                        response.SetBegin((int)HttpStatusCode.OK);
                                        response.SetBody(datatooutput);

                                        ms.Dispose();
                                    }
                                    break;
                                default:
                                    response.SetBegin((int)HttpStatusCode.Forbidden);
                                    response.SetBody();
                                    break;
                            }
                            break;
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
                ServerConfiguration.LogError($"[OTG - HTTPS] - Request error: {error}");
            }

            protected override void OnError(SocketError error)
            {
                ServerConfiguration.LogError($"[OTG - HTTPS] - session caught an error: {error}");
            }
        }

        public class HttpsRestServer : HttpsServer
        {
            public HttpsRestServer(SslContext context, IPAddress address, int port) : base(context, address, port) { }

            protected override SslSession CreateSession() { return new HttpsRestSession(this); }

            protected override void OnError(SocketError error)
            {
                ServerConfiguration.LogError($"[OTG - HTTPS] - server caught an error: {error}");
            }
        }

        public static Task StopOTGHTTPSServer()
        {
            if (server != null && server.IsStarted)
            {
                server.Stop();
                httpsstarted = false;
                ServerConfiguration.LogError($"[OTG - HTTPS] - server Stopped!");
            }
            return Task.CompletedTask;
        }

        public static Task StartOTGHTTPSServer()
        {
            try
            {
#pragma warning disable
                // Create and prepare a new SSL server context, we need tls1.0 because OTG clients expect it
                var context = new SslContext(SslProtocols.Tls, new X509Certificate2(Directory.GetCurrentDirectory() + "/static/SSL/MultiServer.pfx", "qwerty"));
#pragma warning restore
                // Create a new HTTP server
                server = new HttpsRestServer(context, IPAddress.Any, 10061);
                server.OptionDualMode = true;
                server.OptionKeepAlive = true;

                // Start the server
                server.Start();
                httpsstarted = true;
                ServerConfiguration.LogInfo($"[OTG - HTTPS] - server started on: 10061");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[OTG - HTTPS] - server Start has errored out - {ex}");
            }

            return Task.CompletedTask;
        }
    }
}
