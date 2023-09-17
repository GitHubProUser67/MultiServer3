using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NetCoreServer;

namespace MultiServer.HTTPService.LowLevelEngine
{
    public class HTTPSClass
    {
        public static HttpsCacheServer server = null;

        private class HttpsCacheSession : HttpsSession
        {
            public HttpsCacheSession(HttpsServer server) : base(server) { }

            // Our server act as a fake https server, we can't actually use it as a classic server because the HTTP implementation is custom.

            protected override void OnReceivedRequest(HttpRequest request)
            {
                string PublicIp = string.Empty;

                try
                {
                    PublicIp = Misc.GetPublicIPAddress();
                }
                catch (Exception)
                {
                    PublicIp = "127.0.0.1";
                }

                bool accessible = Misc.IsHTTPServerAccessible(PublicIp);

                if (accessible && PublicIp != string.Empty && PublicIp != "127.0.0.1")
                {
                    if (request.Url != null && request.Url != "" && HTTPClass.httpstarted == true)
                    {
                        ServerConfiguration.LogInfo($"[HTTPS] - Received Request - {request.Url}");
                        Response.SetBegin(302);
                        Response.SetHeader("Location", $"http://{PublicIp}{request.Url}");
                    }
                    else
                    {
                        ServerConfiguration.LogInfo($"[HTTPS] - Received Invalid Request");
                        Response.SetBegin(403);
                    }
                }
                else
                {
                    if (request.Url != null && request.Url != "" && HTTPClass.httpstarted == true)
                    {
                        ServerConfiguration.LogInfo($"[HTTPS] - Received Request - {request.Url}");
                        Response.SetBegin(302);
                        Response.SetHeader("Location", $"http://127.0.0.1{request.Url}");
                    }
                    else
                    {
                        ServerConfiguration.LogInfo($"[HTTPS] - Received Invalid Request");
                        Response.SetBegin(403);
                    }
                }

                Response.SetBody();

                SendResponseAsync(Response);
            }

            protected override void OnReceivedRequestError(HttpRequest request, string error)
            {
                ServerConfiguration.LogError($"[HTTPS] - Request error: {error}");
            }

            protected override void OnError(SocketError error)
            {
                ServerConfiguration.LogError($"[HTTPS] - session caught an error: {error}");
            }
        }

        public class HttpsCacheServer : HttpsServer
        {
            public HttpsCacheServer(SslContext context, IPAddress address, int port) : base(context, address, port) { }

            protected override SslSession CreateSession() { return new HttpsCacheSession(this); }

            protected override void OnError(SocketError error)
            {
                ServerConfiguration.LogError($"[HTTPS] - server caught an error: {error}");
            }
        }

        public static Task StopHTTPSServer()
        {
            if (server != null && server.IsStarted)
            {
                server.Stop();
                ServerConfiguration.LogError($"[HTTPS] - server Stopped!");
            }
            return Task.CompletedTask;
        }

        public static Task StartHTTPSServer(int port)
        {
            try
            {
                // Create and prepare a new SSL server context
                var context = new SslContext(SslProtocols.Tls12, new X509Certificate2(Directory.GetCurrentDirectory() + "/static/RootCA.pfx", "qwerty"));

                // Create a new HTTP server
                server = new HttpsCacheServer(context, IPAddress.Any, port);
                server.OptionDualMode = true;
                server.OptionKeepAlive = true;

                // Start the server
                server.Start();

                ServerConfiguration.LogInfo($"[HTTPS] - server started on: {port}");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[HTTPS] - server Start has errored out - {ex}");
            }

            return Task.CompletedTask;
        }
    }
}
