using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using MultiServer.HTTPService;
using NetCoreServer;

namespace MultiServer.HTTPSecureService
{
    public class HTTPSClass
    {
        public static HttpsRestServer server = null;

        public static bool httpsstarted = false;

        public static (string HeaderIndex, string HeaderItem)[] CollectHeaders(HttpRequest request)
        {
            int headerindex = (int)request.Headers; // There is a slight mistake in netcoreserver, where the index is long, and the parser is int
                                                    // So we accomodate that with a cast.

            (string HeaderIndex, string HeaderItem)[] CollectHeader = new (string, string)[headerindex];

            for (int i = 0; i < headerindex; i++)
            {
                CollectHeader[i] = request.Header(i);
#if DEBUG
                ServerConfiguration.LogInfo($"[HTTPS] - Debug Headers : HeaderIndex -> {CollectHeader[i].HeaderIndex} | HeaderItem -> {CollectHeader[i].HeaderItem}");
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

        // Replace the Query property with a custom method.
        public static string GetQueryFromUri(string uri)
        {
            // Check if the URI is absolute.
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            {
                ServerConfiguration.LogWarn("The URI is not absolute.");
                return null;
            }

            // Parse the URI.
            Uri parsedUri = new Uri(uri, UriKind.Absolute);

            // Get the query part of the URI.
            string query = parsedUri.Query;

            return query;
        }

        public static string RemoveQueryString(string input)
        {
            int indexOfQuestionMark = input.IndexOf('?');

            if (indexOfQuestionMark >= 0)
                return input.Substring(0, indexOfQuestionMark);
            else
                return input;
        }

        public static string[] GetSegmentsFromUrl(string input)
        {
            if (string.IsNullOrEmpty(input))
                return Array.Empty<string>();

            List<string> segments = new List<string>();
            int current = 0;

            while (current < input.Length)
            {
                int next = input.IndexOf('/', current);
                if (next == -1)
                    next = input.Length - 1;
                segments.Add(input.Substring(current, (next - current) + 1));
                current = next + 1;
            }

            return segments.ToArray();
        }

        private class HttpsRestSession : HttpsSession
        {
            public HttpsRestSession(HttpsServer server) : base(server) { }

            protected override void OnReceivedRequest(HttpRequest request)
            {
                Response.Clear();

                HttpResponse response = Response;

                (string HeaderIndex, string HeaderItem)[] Headers = CollectHeaders(request);

                if (request.Url == null || request.Url == "")
                {
                    ServerConfiguration.LogError($"[SECURITY] - Client - {GetHeaderValue(Headers, "Host")} Requested the HTTPS server while being banned!");

                    response.SetBegin((int)HttpStatusCode.Forbidden);
                    response.SetBody();
                    SendResponseAsync();
                    return;
                }
                else
                    ServerConfiguration.LogInfo($"[SECURITY] - Client - {GetHeaderValue(Headers, "Host")} Requested the HTTPS server.");

                int num = 0;
                foreach (char value in request.Url.ToLower().Replace(Path.DirectorySeparatorChar, '/'))
                {
                    num *= 37;
                    num += Convert.ToInt32(value);
                }

                string crc32 = num.ToString("X8");

                ServerConfiguration.LogInfo($"[HTTPS] - {GetHeaderValue(Headers, "User-Agent")} Requested {request.Url} with CRC32 {crc32}");

                bool specialrequest = true;

                string url = request.Url;

                switch (url)
                {
                    case "/":
                        response.SetBegin((int)HttpStatusCode.OK);
                        response.SetBody(PreMadeWebPages.homepage("Not Implemented"));
                        break;
                    case "\\":
                        response.SetBegin((int)HttpStatusCode.OK);
                        response.SetBody(PreMadeWebPages.homepage("Not Implemented"));
                        break;
                    default:
                        if (GetHeaderValue(Headers, "User-Agent").Contains("PSHome")) // Home has subservers running on Port 443.
                        {
                            string requesthost = GetHeaderValue(Headers, "Host");

                            if (requesthost == "game2.hellfiregames.com" && url.ToLower().Contains(".php"))
                                Addons.PlayStationHome.HELLFIREGAMES.HELLFIREGAMESClass.ProcessRequest(request, response, Headers);
                            else if ((requesthost == "away.veemee.com" || requesthost == "home.veemee.com") && url.Contains(".php"))
                                Addons.PlayStationHome.VEEMEE.VEEMEEClass.ProcessRequest(request, response, Headers);
                            else if (requesthost == "stats.outso-srv1.com" || url.Contains("/ohs"))
                                Addons.PlayStationHome.OHS.OHSClass.ProcessRequest(crc32, request, response, Headers);
                            else if (requesthost == "api.pottermore.com")
                                Addons.PlayStationHome.POTTERMORE.POTTERMOREClass.ProcessRequest(request, response, Headers);
                            else
                                specialrequest = false;
                        }
                        else
                            specialrequest = false;
                        break;
                }

                if (!specialrequest)
                {
                    // Split the URL into segments
                    string[] segments = url.Trim('/').Split('/');

                    // Combine the folder segments into a directory path
                    string directoryPath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                    // Process the request based on the HTTP method
                    string filePath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, url.Substring(1));

                    switch (request.Method)
                    {
                        case "GET":
                            switch (url)
                            {
                                case "/!videoplayer/":
                                    response.SetBegin((int)HttpStatusCode.OK);
                                    response.SetContentType("text/html");
                                    response.SetBody(PreMadeWebPages.videoplayer);
                                    break;
                                default:
                                    RootProcess.ProcessRootRequest(filePath, directoryPath, crc32, request, response, Headers);
                                    break;
                            }
                            break;
                        case "POST":
                            response.SetBegin((int)HttpStatusCode.Forbidden);
                            response.SetBody();
                            break;
                        case "HEAD":
                            if (File.Exists(filePath))
                            {
                                byte[] output = FileHelper.CryptoReadAsync(filePath, HTTPPrivateKey.HTTPPrivatekey);

                                if (output != null)
                                {
                                    response.SetBegin((int)HttpStatusCode.OK);
                                    response.SetContentType(MimeTypes.GetMimeType(filePath));
                                    response.SetHeader("Content-Length", output.Length.ToString());
                                    response.SetBody();
                                }
                                else
                                {
                                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                                    response.SetBody();
                                }
                            }
                            else
                            {
                                ServerConfiguration.LogWarn($"[HTTPS] : {GetHeaderValue(Headers, "User-Agent")} Requested a non-existing file: '{filePath}'.");
                                response.SetBegin((int)HttpStatusCode.NotFound);
                                response.SetBody();
                            }
                            break;
                        default:
                            response.SetBegin((int)HttpStatusCode.Forbidden);
                            response.SetBody();
                            break;
                    }
                }

                SendResponseAsync(); // Final step, ideally the only one to exist if the APIs are made right.
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

        public class HttpsRestServer : HttpsServer
        {
            public HttpsRestServer(SslContext context, IPAddress address, int port) : base(context, address, port) { }

            protected override SslSession CreateSession() { return new HttpsRestSession(this); }

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
                httpsstarted = false;
                ServerConfiguration.LogError($"[HTTPS] - server Stopped!");
            }
            return Task.CompletedTask;
        }

        public static Task StartHTTPSServer(int port)
        {
            try
            {
                // Create and prepare a new SSL server context
                var context = new SslContext(SslProtocols.Tls12, new X509Certificate2(Directory.GetCurrentDirectory() + "/static/SSL/MultiServer.pfx", "qwerty"));

                // Create a new HTTP server
                server = new HttpsRestServer(context, IPAddress.Any, port);
                server.OptionDualMode = true;
                server.OptionKeepAlive = true;

                // Start the server
                server.Start();
                httpsstarted = true;
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
