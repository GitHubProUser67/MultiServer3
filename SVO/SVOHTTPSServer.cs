using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NetCoreServer;
using CustomLogger;
using System.Text.RegularExpressions;
using HttpMultipartParser;
using System.Net.Security;

namespace SVO
{
    public class SVOHTTPSServer
    {
        public static bool IsStarted = false;
        private string certpath;
        private string certpass;

        public SVOHTTPSServer(string certpath, string certpass)
        {
            this.certpath = certpath;
            this.certpass = certpass;
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

        public Task StartSVO()
        {
#pragma warning disable
            // Create and prepare a new SSL server context
            SslContext context = new(SslProtocols.Tls, new X509Certificate2(certpath, certpass), MyRemoteCertificateValidationCallback);
            // Create and prepare a new SSL server context
            SslContext legacycontext = new(SslProtocols.Ssl3, new X509Certificate2(certpath, certpass), MyRemoteCertificateValidationCallback);
#pragma warning restore

            // Create a new HTTP server
            SVOSecureServer server = new(context, IPAddress.Any, 10062);
            // Create a new HTTP server
            SVOSecureServer legacyserver = new(legacycontext, IPAddress.Any, 10061); // 10061 is default port, but some sslv3 games hardcode it in their code...

            // Start the server
            server.Start();
            // Start the server
            legacyserver.Start();

            IsStarted = true;
            LoggerAccessor.LogInfo("[SVO_HTTPS] - Server started...");

            return Task.CompletedTask;
        }

        private static HttpResponse SVOSecureRequestProcess(HttpRequest request, HttpResponse Response)
        {
            try
            {
                (string HeaderIndex, string HeaderItem)[] Headers = CollectHeaders(request);

                string Host = GetHeaderValue(Headers, "Host");

                string UserAgent = GetHeaderValue(Headers, "User-Agent");

                if (!string.IsNullOrEmpty(UserAgent) && (UserAgent.ToLower().Contains("firefox") || UserAgent.ToLower().Contains("chrome") || UserAgent.ToLower().Contains("trident")))
                {
                    LoggerAccessor.LogInfo($"[SVO_HTTPS] - Client - {Host} Requested the SVO_HTTPS Server while not being allowed!");
                    Response.Clear();
                    Response.SetBegin(403);
                    Response.SetBody();
                }
                else
                {
                    if (!string.IsNullOrEmpty(request.Url) && !SVOServer.IsIPBanned(Host))
                    {
                        LoggerAccessor.LogInfo($"[SVO_HTTPS] - Client Requested the SVO_HTTPS Server with URL : {request.Url}");

                        string absolutepath = request.Url;

                        LoggerAccessor.LogInfo($"[SVO_HTTPS] - Client Requested : {absolutepath}");

                        switch (request.Method)
                        {
                            case "GET":
                                Response.Clear();
                                Response.SetBegin(403);
                                Response.SetBody();
                                break;
                            case "POST":
                                switch (absolutepath)
                                {
                                    case "/dataloaderweb/queue":
                                        string? ContentType = GetHeaderValue(Headers, "Content-Type");
                                        if (!string.IsNullOrEmpty(ContentType))
                                        {
                                            Response.Clear();
                                            Response.SetBegin(200);
                                            Response.SetContentType("application/xml;charset=UTF-8");
                                            Response.SetHeader("Content-Language", string.Empty);
                                            Response.SetHeader("Transfer-Encoding", "chunked");

                                            string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);

                                            var data = MultipartFormDataParser.Parse(new MemoryStream(request.BodyBytes), boundary);

                                            string datatooutput = data.GetParameterValue("body");

                                            Directory.CreateDirectory($"{SVOServerConfiguration.SVOStaticFolder}/dataloaderweb/queue");

                                            DirectoryInfo directory = new($"{SVOServerConfiguration.SVOStaticFolder}/dataloaderweb/queue");

                                            FileInfo[] files = directory.GetFiles();

                                            if (files.Length >= 20)
                                            {
                                                FileInfo oldestFile = files.OrderBy(file => file.CreationTime).First();
                                                LoggerAccessor.LogInfo("[SVO_HTTPS] - Replacing Home Debug log file: " + oldestFile.Name);
                                                if (File.Exists(oldestFile.FullName))
                                                    File.Delete(oldestFile.FullName);
                                            }

                                            File.WriteAllText($"{SVOServerConfiguration.SVOStaticFolder}/dataloaderweb/queue/{Guid.NewGuid()}.xml", datatooutput);

                                            Response.SetBody(datatooutput);
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
                                break;
                            case "PUT":
                                Response.Clear();
                                Response.SetBegin(403);
                                Response.SetBody();
                                break;
                            case "DELETE":
                                Response.Clear();
                                Response.SetBegin(403);
                                Response.SetBody();
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
                        LoggerAccessor.LogInfo($"[SVO_HTTPS] - Client Requested the SVO_HTTPS Server with invalid parameters!");
                        Response.Clear();
                        Response.SetBegin(403);
                        Response.SetBody();
                    }
                }
            }
            catch (Exception e)
            {
                Response.MakeErrorResponse();
                LoggerAccessor.LogError($"[SVO_HTTPS] - Request thrown an error : {e}");
            }

            return Response;
        }

        private bool MyRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; //This isn't a good thing to do, but to keep the code simple i prefer doing this, it will be used only on mono
        }

        private class SVOSecureSession : HttpsSession
        {
            public SVOSecureSession(HttpsServer server) : base(server) { }

            protected override void OnReceivedRequest(HttpRequest request)
            {
                SendResponseAsync(SVOSecureRequestProcess(request, Response));
            }

            protected override void OnReceivedRequestError(HttpRequest request, string error)
            {
                LoggerAccessor.LogError($"[SVO_HTTPS] - Request error: {error}");
            }

            protected override void OnError(SocketError error)
            {
                LoggerAccessor.LogError($"[SVO_HTTPS] - Session caught an error: {error}");
            }
        }

        private class SVOSecureServer : HttpsServer
        {
            public SVOSecureServer(SslContext context, IPAddress address, int port) : base(context, address, port) { }

            protected override SslSession CreateSession() { return new SVOSecureSession(this); }

            protected override void OnError(SocketError error)
            {
                LoggerAccessor.LogError($"[SVO_HTTPS] - Server caught an error: {error}");
            }
        }
    }
}
