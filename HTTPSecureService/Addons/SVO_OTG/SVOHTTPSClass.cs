using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.SVO_OTG
{
    public class SVOHTTPSClass
    {
        public static HttpsWebSocketRestServer server = null;

        public static bool httpsstarted = false;

        public static int ExtractLanguageId(string inputString)
        {
            // Define a regex pattern to match the entire query string and extract the languageId parameter
            string pattern = @"languageId=(\d+)";

            // Use Regex.Match to find the match in the input string
            Match match = Regex.Match(inputString, pattern);

            if (match.Success)
            {
                // Extract the captured group and parse it as an integer
                string languageIdStr = match.Groups[1].Value;
                if (int.TryParse(languageIdStr, out int languageId))
                    return languageId;
            }

            // Return a default value or throw an exception if languageId is not found or cannot be parsed
            ServerConfiguration.LogError("languageId not found or invalid in the input string.");

            return -1;
        }

        private class HttpsWebSocketRestSession : WssSession
        {
            public HttpsWebSocketRestSession(WssServer server) : base(server) { }

            public override void OnWsConnected(HttpRequest request)
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

                ServerConfiguration.LogInfo($"[SVO - HTTPS] - {HTTPSClass.GetHeaderValue(Headers, "Host")} Requested {request.Url}");

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
                    default:
                        if (url.Contains("/HUBPS3_SVML/"))
                            Games.PlayStationHome.Home_SVO(request, response, Headers);
                        else
                        {
                            response.SetBegin((int)HttpStatusCode.Forbidden);
                            response.SetBody();
                        }
                        break;
                }

                SendResponseAsync(); // Final step, ideally the only one to exist if the APIs are made right.

                Close(1000);
            }

            public override void OnWsDisconnected()
            {
                ServerConfiguration.LogInfo($"[SVO - HTTPS] - WebSocket session with Id {Id} disconnected!");
            }

            protected override void OnError(SocketError error)
            {
                ServerConfiguration.LogError($"[SVO - HTTPS] - session caught an error: {error}");
            }
        }

        public class HttpsWebSocketRestServer : WssServer
        {
            public HttpsWebSocketRestServer(SslContext context, IPAddress address, int port) : base(context, address, port) { }

            protected override SslSession CreateSession() { return new HttpsWebSocketRestSession(this); }

            protected override void OnError(SocketError error)
            {
                ServerConfiguration.LogError($"[SVO - HTTPS] - server caught an error: {error}");
            }
        }

        public static Task StopSVOHTTPSServer()
        {
            if (server != null && server.IsStarted)
            {
                server.Stop();
                httpsstarted = false;
                ServerConfiguration.LogError($"[SVO - HTTPS] - server Stopped!");
            }
            return Task.CompletedTask;
        }

        public static Task StartSVOHTTPSServer()
        {
            try
            {
#pragma warning disable
                // Create and prepare a new SSL server context, we need ssl2 because SVO clients expect it.
                var context = new SslContext(SslProtocols.Ssl2, new X509Certificate2(Directory.GetCurrentDirectory() + "/static/SSL/SVO.pfx", "qwerty"));
#pragma warning restore
                // Create a new HTTP server
                server = new HttpsWebSocketRestServer(context, IPAddress.Any, 10061);
                server.OptionDualMode = true;
                server.OptionKeepAlive = true;

                // Start the server
                server.Start();
                httpsstarted = true;
                ServerConfiguration.LogInfo($"[SVO - HTTPS] - server started on: 10061");
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[SVO - HTTPS] - server Start has errored out - {ex}");
            }

            return Task.CompletedTask;
        }
    }
}
