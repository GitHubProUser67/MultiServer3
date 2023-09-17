using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NetCoreServer;

namespace MultiServer.HTTPService.Addons.SVO
{
    public class SVOHTTPSClass
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
                        ServerConfiguration.LogInfo($"[SVO-HTTPS] - Received Request - {request.Url}");
                        Response.SetBegin(302);
                        Response.SetHeader("Location", $"http://{PublicIp}:10060{request.Url}");
                    }
                    else
                    {
                        ServerConfiguration.LogInfo($"[SVO-HTTPS] - Received Invalid Request");
                        Response.SetBegin(404);
                    }
                }
                else
                {
                    if (request.Url != null && request.Url != "" && HTTPClass.httpstarted == true)
                    {
                        ServerConfiguration.LogInfo($"[SVO-HTTPS] - Received Request - {request.Url}");
                        Response.SetBegin(302);
                        Response.SetHeader("Location", $"http://127.0.0.1:10060{request.Url}");
                    }
                    else
                    {
                        ServerConfiguration.LogInfo($"[SVO-HTTPS] - Received Invalid Request");
                        Response.SetBegin(404);
                    }
                }

                Response.SetBody();
                SendResponseAsync(Response);
            }

            protected override void OnReceivedRequestError(HttpRequest request, string error)
            {
                ServerConfiguration.LogError($"[SVO-HTTPS] - Request error: {error}");
            }

            protected override void OnError(SocketError error)
            {
                ServerConfiguration.LogError($"[SVO-HTTPS] - session caught an error: {error}");
            }
        }

        public class HttpsCacheServer : HttpsServer
        {
            public HttpsCacheServer(SslContext context, IPAddress address, int port) : base(context, address, port) { }

            protected override SslSession CreateSession() { return new HttpsCacheSession(this); }

            protected override void OnError(SocketError error)
            {
                ServerConfiguration.LogError($"[SVO-HTTPS] - server caught an error: {error}");
            }
        }

        public static Task StopSVOHTTPSServer()
        {
            if (server != null && server.IsStarted)
            {
                server.Stop();
                ServerConfiguration.LogError($"[SVO-HTTPS] - server Stopped!");
            }
            return Task.CompletedTask;
        }

        public static Task StartSVOHTTPSServer()
        {
            try
            {
#pragma warning disable
                // Create and prepare a new SSL server context
                var context = new SslContext(SslProtocols.Ssl3, new X509Certificate2(Directory.GetCurrentDirectory() + "/static/RootCA.pfx", "qwerty"));
#pragma warning restore
                // Create a new HTTP server
                server = new HttpsCacheServer(context, IPAddress.Any, 10061);
                server.OptionDualMode = true;
                server.OptionKeepAlive = true;

                // Start the server
                server.Start();
                ServerConfiguration.LogInfo($"[SVO-HTTPS] - server started on: 10061");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[SVO-HTTPS] - server Start has errored out - {ex}");
            }

            return Task.CompletedTask;
        }
    }
}
