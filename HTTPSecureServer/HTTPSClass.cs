using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using CustomLogger;
using System.Text.RegularExpressions;
using NetCoreServer;
using System.Text;

namespace HTTPSecureServer
{
    public class HTTPSClass
    {
        public static bool IsStarted = false;
        private static string[] AllowedFileTypes = { ".txt", ".csv", ".plain", ".richtext", ".rtf",
            ".rtx", ".sgml", ".strings", ".url", ".ttf", ".css", ".js", ".vue", ".atom", ".soap", ".fastsoap",
            ".sql", ".tss", ".xml", ".xslt", ".sdc", ".odc", ".htm", ".html", ".pdf", ".json", ".map", ".ico", ".png", 
            ".dds", ".jpg", ".jpeg", ".bmp", ".flick", ".emf", ".gif", ".jpm", ".jpx", ".jrx", ".svg", ".tiff", ".wmf", string.Empty };
        private string certpath;
        private string certpass;

        public HTTPSClass(string certpath, string certpass)
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
            string pattern = @"^(.*?):\s(.*)$"; // Make a GITHUB ticket for netcoreserver, the header tuple can get out of sync with null values, we mitigate the problem.

            foreach ((string HeaderIndex, string HeaderItem) header in headers)
            {
                Match match = Regex.Match(header.HeaderItem, pattern);

                if (header.HeaderIndex == requestedHeaderIndex)
                    return header.HeaderItem;
                else if (header.HeaderItem.Contains(requestedHeaderIndex) && match.Success)
                    return match.Groups[2].Value;
            }
            return string.Empty; // Return empty string if the header index is not found, why not null, because in this case it prevents us
                                 // from doing extensive checks everytime we want to display the User-Agent in particular.
        }

        public static string RemoveQueryString(string input)
        {
            int indexOfQuestionMark = input.IndexOf('?');

            if (indexOfQuestionMark >= 0)
                return input.Substring(0, indexOfQuestionMark);
            else
                return input;
        }

        public static bool IsIPBanned(string ipAddress)
        {
            if (HTTPSServerConfiguration.BannedIPs != null && HTTPSServerConfiguration.BannedIPs.Contains(ipAddress))
                return true;

            return false;
        }

        public Task StartHTTPS()
        {
            // Create and prepare a new SSL server context
            var context = new SslContext(SslProtocols.Tls12, new X509Certificate2(certpath, certpass));

            // Create a new HTTP server
            var server = new HttpsCacheServer(context, IPAddress.Any, 443);

            // Start the server
            server.Start();
            IsStarted = true;
            LoggerAccessor.LogInfo("[HTTPS] - Server started...");

            return Task.CompletedTask;
        }

        private class HttpsCacheSession : HttpsSession
        {
            public HttpsCacheSession(HttpsServer server) : base(server) { }

            protected override void OnReceivedRequest(HttpRequest request)
            {
                (string HeaderIndex, string HeaderItem)[] Headers = CollectHeaders(request);
                HttpStatusCode statusCode = HttpStatusCode.Forbidden;
                string absolutepath = string.Empty;
                string UserAgent = GetHeaderValue(Headers, "User-Agent");
                string clientip = GetHeaderValue(Headers, "Host");

                try
                {
                    if (string.IsNullOrEmpty(UserAgent) || string.IsNullOrEmpty(clientip) || IsIPBanned(clientip))
                        LoggerAccessor.LogError($"[SECURITY] - Client - {clientip} Requested the HTTPS server while being banned!");
                    else
                    {
                        if (request.Url != null)
                        {
                            LoggerAccessor.LogInfo($"[HTTPS] - Client - {clientip} Requested the HTTPS Server with URL : {request.Url}");

                            // get filename path
                            absolutepath = request.Url;
                            statusCode = HttpStatusCode.Continue;
                        }
                        else
                            LoggerAccessor.LogInfo($"[HTTPS] - Client - {clientip} Requested the HTTPS Server with invalid parameters!");
                    }
                }
                catch (Exception)
                {

                }

                if (statusCode == HttpStatusCode.Continue)
                {
                    string host = GetHeaderValue(Headers, "Host");

                    // Split the URL into segments
                    string[] segments = absolutepath.Trim('/').Split('/');

                    // Combine the folder segments into a directory path
                    string directoryPath = Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                    // Process the request based on the HTTP method
                    string filePath = Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, absolutepath.Substring(1));

                    if ((absolutepath == "/" || absolutepath == "\\") && request.Method == "GET")
                    {
                        foreach (string indexFile in CryptoSporidium.HTTPUtils.DefaultDocuments)
                        {
                            if (File.Exists(Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, indexFile)))
                            {
                                string? encoding = GetHeaderValue(Headers, "Content-Encoding");

                                using (FileStream stream = new(indexFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    byte[]? buffer = null;

                                    using (MemoryStream ms = new())
                                    {
                                        stream.CopyTo(ms);
                                        buffer = ms.ToArray();
                                        ms.Flush();
                                    }

                                    if (buffer != null)
                                    {
                                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                        {
                                            buffer = CryptoSporidium.HTTPUtils.Compress(buffer);

                                            if (buffer != null)
                                                SendResponseAsync(Response.MakeGetResponse(buffer, CryptoSporidium.HTTPUtils.mimeTypes[Path.GetExtension(indexFile)]));
                                            else
                                                SendResponseAsync(Response.MakeErrorResponse());
                                        }
                                        else
                                            SendResponseAsync(Response.MakeGetResponse(buffer, CryptoSporidium.HTTPUtils.mimeTypes[Path.GetExtension(indexFile)]));
                                    }
                                    else
                                        SendResponseAsync(Response.MakeErrorResponse());

                                    stream.Flush();
                                }
                                break;
                            }
                        }
                    }
                    else if ((host == "away.veemee.com" || host == "home.veemee.com") && RemoveQueryString(absolutepath).ToLower().EndsWith(".php"))
                    {
                        LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a VEEMEE method : {absolutepath}");

                        API.VEEMEE.VEEMEEClass veemee = new(request.Method, RemoveQueryString(absolutepath)); // Todo, loss of GET informations.
                        string? res = veemee.ProcessRequest(request.BodyBytes, GetHeaderValue(Headers, "Content-type"));
                        veemee.Dispose();
                        if (string.IsNullOrEmpty(res))
                            statusCode = HttpStatusCode.InternalServerError;
                        else
                            statusCode = HttpStatusCode.OK;
                        Response.Clear();
                        Response.SetBegin((int)statusCode);
                        Response.SetBody(res);
                        SendResponseAsync(Response);
                    }
                    else if (host == "game2.hellfiregames.com" && RemoveQueryString(absolutepath).ToLower().EndsWith(".php"))
                    {
                        LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a HELLFIRE method : {absolutepath}");

                        API.HELLFIRE.HELLFIREClass hellfire = new(request.Method, RemoveQueryString(absolutepath));
                        string? res = hellfire.ProcessRequest(request.BodyBytes, GetHeaderValue(Headers, "Content-type"));
                        hellfire.Dispose();
                        if (string.IsNullOrEmpty(res))
                            statusCode = HttpStatusCode.InternalServerError;
                        else
                            statusCode = HttpStatusCode.OK;
                        Response.Clear();
                        Response.SetBegin((int)statusCode);
                        Response.SetBody(res);
                        SendResponseAsync(Response);
                    }
                    else if ((host == "stats.outso-srv1.com" || host == "www.outso-srv1.com") && absolutepath.Contains("/ohs") && absolutepath.EndsWith("/"))
                    {
                        LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a OHS method : {absolutepath}");

                        CryptoSporidium.OHS.OHSClass ohs = new(request.Method, absolutepath, 0);
                        string? res = ohs.ProcessRequest(request.BodyBytes, GetHeaderValue(Headers, "Content-type"), filePath);
                        ohs.Dispose();
                        if (string.IsNullOrEmpty(res))
                            statusCode = HttpStatusCode.InternalServerError;
                        else
                        {
                            res = $"<ohs>{res}</ohs>";
                            statusCode = HttpStatusCode.OK;
                        }
                        Response.Clear();
                        Response.SetBegin((int)statusCode);
                        if (!string.IsNullOrEmpty(res))
                            Response.SetContentType("application/xml;charset=UTF-8");
                        Response.SetBody(res);
                        SendResponseAsync(Response);
                    }
                    else
                    {
                        switch (request.Method)
                        {
                            case "GET":
                                if (filePath.EndsWith("/"))
                                {
                                    string? encoding = GetHeaderValue(Headers, "Content-Encoding");
                                    if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                    {
                                        byte[]? buffer = CryptoSporidium.HTTPUtils.Compress(Encoding.UTF8.GetBytes(CryptoSporidium.FileStructureToJson.GetFileStructureAsJson(filePath.Substring(0, filePath.Length - 1))));

                                        if (buffer != null)
                                            SendResponseAsync(Response.MakeGetResponse(buffer, "application/json"));
                                        else
                                            SendResponseAsync(Response.MakeErrorResponse());
                                    }
                                    else
                                        SendResponseAsync(Response.MakeGetResponse(CryptoSporidium.FileStructureToJson.GetFileStructureAsJson(filePath.Substring(0, filePath.Length - 1)), "application/json"));
                                }
                                else
                                {
                                    FileInfo? getfileInfo = new(filePath);

                                    // send file
                                    if (getfileInfo.Exists)
                                    {
                                        // Workaround for lack of stream support in NetCoreServer.
                                        // Reading very large files causes a big memory leak, a github ticket has already been issued : https://github.com/chronoxor/NetCoreServer/issues/176
                                        // And : https://github.com/chronoxor/NetCoreServer/issues/248
                                        if (AllowedFileTypes.Contains(Path.GetExtension(filePath)) && getfileInfo.Length / (1024 * 1024) < 10) // Arbitrary, 9.9 mb limit.
                                        {
                                            LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a file : {absolutepath}");

                                            string? encoding = GetHeaderValue(Headers, "Content-Encoding");

                                            using (FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                            {
                                                byte[]? buffer = null;

                                                using (MemoryStream ms = new())
                                                {
                                                    stream.CopyTo(ms);
                                                    buffer = ms.ToArray();
                                                    ms.Flush();
                                                }

                                                if (buffer != null)
                                                {
                                                    if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                                    {
                                                        buffer = CryptoSporidium.HTTPUtils.Compress(buffer);

                                                        if (buffer != null)
                                                            SendResponseAsync(Response.MakeGetResponse(buffer, CryptoSporidium.HTTPUtils.mimeTypes[Path.GetExtension(filePath)]));
                                                        else
                                                            SendResponseAsync(Response.MakeErrorResponse());
                                                    }
                                                    else
                                                        SendResponseAsync(Response.MakeGetResponse(buffer, CryptoSporidium.HTTPUtils.mimeTypes[Path.GetExtension(filePath)]));
                                                }
                                                else
                                                    SendResponseAsync(Response.MakeErrorResponse());

                                                stream.Flush();
                                            }
                                        }
                                        else
                                        {
                                            LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a file redirection to HTTP : {absolutepath}");
                                            statusCode = HttpStatusCode.PermanentRedirect;
                                            Response.Clear();
                                            Response.SetBegin((int)statusCode);
                                            Response.SetHeader("Location", $"http://{Misc.GetPublicIPAddress()}{absolutepath}");
                                            Response.SetBody();
                                            SendResponseAsync(Response);
                                        }
                                    }
                                    else
                                    {
                                        LoggerAccessor.LogWarn($"[HTTPS] - {clientip} Requested a non-existant file : {filePath}");
                                        statusCode = HttpStatusCode.NotFound;
                                        Response.Clear();
                                        Response.SetBegin((int)statusCode);
                                        Response.SetBody();
                                        SendResponseAsync(Response);
                                    }

                                    getfileInfo = null;
                                }
                                break;
                            case "POST":
                                statusCode = HttpStatusCode.Forbidden;
                                Response.Clear();
                                Response.SetBegin((int)statusCode);
                                Response.SetBody();
                                SendResponseAsync(Response);
                                break;
                            case "PUT":
                                statusCode = HttpStatusCode.Forbidden;
                                Response.Clear();
                                Response.SetBegin((int)statusCode);
                                Response.SetBody();
                                SendResponseAsync(Response);
                                break;
                            case "DELETE":
                                statusCode = HttpStatusCode.Forbidden;
                                Response.Clear();
                                Response.SetBegin((int)statusCode);
                                Response.SetBody();
                                SendResponseAsync(Response);
                                break;
                            case "HEAD":
                                Response.Clear();
                                FileInfo? fileInfo = new(filePath);
                                if (fileInfo.Exists)
                                {
                                    statusCode = HttpStatusCode.OK;
                                    Response.SetBegin((int)statusCode);
                                    Response.SetContentType(CryptoSporidium.HTTPUtils.mimeTypes[Path.GetExtension(filePath)]);
                                    Response.SetHeader("Content-Length", fileInfo.Length.ToString());
                                    Response.SetHeader("Date", DateTime.Now.ToString("r"));
                                    Response.SetHeader("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                }
                                else
                                {
                                    LoggerAccessor.LogWarn($"[HTTPS] - {clientip} Requested a non-existant file: {filePath}");
                                    statusCode = HttpStatusCode.NotFound;
                                    Response.SetBegin((int)statusCode);
                                }
                                fileInfo = null;
                                Response.SetBody();
                                SendResponseAsync(Response);
                                break;
                            default:
                                statusCode = HttpStatusCode.Forbidden;
                                Response.Clear();
                                Response.SetBegin((int)statusCode);
                                Response.SetBody();
                                SendResponseAsync(Response);
                                break;
                        }
                    }
                }
                else
                {
                    statusCode = HttpStatusCode.Forbidden;
                    Response.Clear();
                    Response.SetBegin((int)statusCode);
                    Response.SetBody();
                    SendResponseAsync(Response);
                }
            }

            protected override void OnReceivedRequestError(HttpRequest request, string error)
            {
                LoggerAccessor.LogError($"[HTTPS] - Request error: {error}");
            }

            protected override void OnError(SocketError error)
            {
                LoggerAccessor.LogError($"[HTTPS] - Session caught an error: {error}");
            }
        }

        private class HttpsCacheServer : HttpsServer
        {
            public HttpsCacheServer(SslContext context, IPAddress address, int port) : base(context, address, port) { }

            protected override SslSession CreateSession() { return new HttpsCacheSession(this); }

            protected override void OnError(SocketError error)
            {
                LoggerAccessor.LogError($"[HTTPS] - Server caught an error: {error}");
            }
        }
    }
}
