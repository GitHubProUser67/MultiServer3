using CustomLogger;
using HttpMultipartParser;
using HttpServerLite;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Net;
using System.Text;

namespace HTTPSecureServerLite
{
    public class Processor
    {
        public static bool IsStarted = false;
        private static int _BufferSize = 65536;
        private static Webserver? _Server;
        private string ip;
        private string certpath;
        private string certpass;
        private int port;

        public Processor(string certpath, string certpass, string ip, int port)
        {
            this.certpath = certpath;
            this.certpass = certpass;
            this.ip = ip;
            this.port = port;
        }

        private static bool AuthorizeConnection(string arg1, int arg2)
        {
            if (HTTPSServerConfiguration.BannedIPs != null && HTTPSServerConfiguration.BannedIPs.Contains(arg1))
            {
                LoggerAccessor.LogError($"[SECURITY] - Client - {arg1} Requested the HTTPS server while being banned!");
                return false;
            }

            return true;
        }

        public void StartServer()
        {
            if (_Server != null && _Server.IsListening)
                LoggerAccessor.LogWarn("HTTPS Server already initiated");
            else
            {
                _Server = new Webserver(ip, port, true, certpath, certpass, DefaultRoute);
                _Server.Settings.Headers.Host = "https://" + ip + ":" + port;
                _Server.Callbacks.AuthorizeConnection = AuthorizeConnection;
                _Server.Events.Logger = LoggerAccessor.LogInfo;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;
                _Server.Start();
                IsStarted = true;
                LoggerAccessor.LogInfo("HTTPS Server initiated...");
            }
        }

        static async Task DefaultRoute(HttpContext ctx)
        {
            HttpStatusCode statusCode = HttpStatusCode.Forbidden;
            string absolutepath = string.Empty;
            string UserAgent = ctx.Request.RetrieveHeaderValue("User-Agent");
            string host = ctx.Request.RetrieveHeaderValue("Host");
            string clientip = ctx.Request.Source.IpAddress;
            string clientport = ctx.Request.Source.Port.ToString();

            try
            {
                if (string.IsNullOrEmpty(UserAgent) || string.IsNullOrEmpty(host))
                    LoggerAccessor.LogInfo($"[HTTPS] - Client - {clientip} Requested the HTTPS Server with invalid parameters!");
                else
                {
                    if (ctx.Request.Url != null)
                    {
                        LoggerAccessor.LogInfo($"[HTTPS] - Client - {clientip} Requested the HTTPS Server with URL : {ctx.Request.Url.Full}");

                        // get filename path
                        absolutepath = CryptoSporidium.HTTPUtils.RemoveQueryString(ctx.Request.Url.Full);
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
                // Split the URL into segments
                string[] segments = absolutepath.Trim('/').Split('/');

                // Combine the folder segments into a directory path
                string directoryPath = Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                // Process the request based on the HTTP method
                string filePath = Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, absolutepath.Substring(1));

                if (ctx.Request.Method.ToString() == "OPTIONS")
                {
                    ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                    ctx.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, HEAD");
                    ctx.Response.Headers.Add("Access-Control-Max-Age", "1728000");
                }

                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                if ((absolutepath == "/" || absolutepath == "\\") && ctx.Request.Method.ToString() == "GET")
                {
                    foreach (string indexFile in CryptoSporidium.HTTPUtils.DefaultDocuments)
                    {
                        if (File.Exists(Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, indexFile)))
                        {
                            string? encoding = ctx.Request.RetrieveHeaderValue("Content-Encoding");

                            using (FileStream stream = new(indexFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = CryptoSporidium.HTTPUtils.GetMimeType(Path.GetExtension(indexFile));
                                            await ctx.Response.SendAsync(buffer);
                                        }
                                        else
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            ctx.Response.Send(true);
                                        }
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.OK;
                                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = CryptoSporidium.HTTPUtils.GetMimeType(Path.GetExtension(indexFile));
                                        await ctx.Response.SendAsync(buffer);
                                    }
                                }
                                else
                                {
                                    statusCode = HttpStatusCode.InternalServerError;
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "text/plain";
                                    ctx.Response.Send(true);
                                }

                                stream.Flush();
                            }
                            break;
                        }
                        else
                        {
                            statusCode = HttpStatusCode.NotFound;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            ctx.Response.Send(true);
                        }
                    }
                }
                else if ((host == "away.veemee.com" || host == "home.veemee.com") && absolutepath.ToLower().EndsWith(".php"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a VEEMEE method : {absolutepath}");

                    API.VEEMEE.VEEMEEClass veemee = new(ctx.Request.Method.ToString(), absolutepath); // Todo, loss of GET informations.
                    string? res = veemee.ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType);
                    veemee.Dispose();
                    if (string.IsNullOrEmpty(res))
                        statusCode = HttpStatusCode.InternalServerError;
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.SendAsync(res);
                }
                else if (host == "game2.hellfiregames.com" && absolutepath.ToLower().EndsWith(".php"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a HELLFIRE method : {absolutepath}");

                    API.HELLFIRE.HELLFIREClass hellfire = new(ctx.Request.Method.ToString(), CryptoSporidium.HTTPUtils.RemoveQueryString(absolutepath));
                    string? res = hellfire.ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType);
                    hellfire.Dispose();
                    if (string.IsNullOrEmpty(res))
                        statusCode = HttpStatusCode.InternalServerError;
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.SendAsync(res);
                }
                else if ((host == "stats.outso-srv1.com" || host == "www.outso-srv1.com") && absolutepath.Contains("/ohs") && absolutepath.EndsWith("/"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a OHS method : {absolutepath}");

                    CryptoSporidium.OHS.OHSClass ohs = new(ctx.Request.Method.ToString(), absolutepath, 0);
                    string? res = ohs.ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType, filePath);
                    ohs.Dispose();
                    if (string.IsNullOrEmpty(res))
                        statusCode = HttpStatusCode.InternalServerError;
                    else
                    {
                        res = $"<ohs>{res}</ohs>";
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    if (!string.IsNullOrEmpty(res))
                        ctx.Response.ContentType = "application/xml;charset=UTF-8";
                    else
                        ctx.Response.ContentType = "text/plain";
                    await ctx.Response.SendAsync(res);
                }
                else
                {
                    switch (ctx.Request.Method.ToString())
                    {
                        case "GET":
                            if (filePath.EndsWith("/"))
                            {
                                string? encoding = ctx.Request.RetrieveHeaderValue("Content-Encoding");
                                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                {
                                    byte[]? buffer = CryptoSporidium.HTTPUtils.Compress(Encoding.UTF8.GetBytes(CryptoSporidium.FileStructureToJson.GetFileStructureAsJson(filePath.Substring(0, filePath.Length - 1))));

                                    if (buffer != null)
                                    {
                                        statusCode = HttpStatusCode.OK;
                                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                        ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "application/json";
                                        await ctx.Response.SendAsync(buffer);
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                }
                                else
                                {
                                    statusCode = HttpStatusCode.OK;
                                    ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                    ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                    ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "application/json";
                                    await ctx.Response.SendAsync(CryptoSporidium.FileStructureToJson.GetFileStructureAsJson(filePath.Substring(0, filePath.Length - 1)));
                                }
                            }
                            else if (absolutepath.EndsWith(".php") && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                            {
                                var CollectPHP = Extensions.PHP.ProcessPHPPage(filePath, HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx.Request);
                                string? encoding = ctx.Request.RetrieveHeaderValue("Content-Encoding");
                                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && CollectPHP.Item1 != null)
                                {
                                    byte[]? buffer = CryptoSporidium.HTTPUtils.Compress(CollectPHP.Item1);

                                    if (buffer != null)
                                    {
                                        statusCode = HttpStatusCode.OK;
                                        if (CollectPHP.Item2 != null)
                                        {
                                            foreach (var innerArray in CollectPHP.Item2)
                                            {
                                                // Ensure the inner array has at least two elements
                                                if (innerArray.Length >= 2)
                                                {
                                                    // Extract two values from the inner array
                                                    string value1 = innerArray[0];
                                                    string value2 = innerArray[1];
                                                    ctx.Response.Headers.Add(value1, value2);
                                                }
                                            }
                                        }
                                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                        ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/html";
                                        await ctx.Response.SendAsync(buffer);
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                }
                                else
                                {
                                    statusCode = HttpStatusCode.OK;
                                    if (CollectPHP.Item2 != null)
                                    {
                                        foreach (var innerArray in CollectPHP.Item2)
                                        {
                                            // Ensure the inner array has at least two elements
                                            if (innerArray.Length >= 2)
                                            {
                                                // Extract two values from the inner array
                                                string value1 = innerArray[0];
                                                string value2 = innerArray[1];
                                                ctx.Response.Headers.Add(value1, value2);
                                            }
                                        }
                                    }
                                    ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                    ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                    ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "text/html";
                                    await ctx.Response.SendAsync(CollectPHP.Item1);
                                }
                            }
                            else
                            {
                                // send file
                                if (File.Exists(filePath))
                                {
                                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a file : {absolutepath}");

                                    string? encoding = ctx.Request.RetrieveHeaderValue("Content-Encoding");
                                    if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                    {
                                        using (FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                        {
                                            byte[]? buffer = null;

                                            using (MemoryStream ms = new())
                                            {
                                                stream.CopyTo(ms);
                                                buffer = ms.ToArray();
                                                ms.Flush();
                                            }

                                            buffer = CryptoSporidium.HTTPUtils.Compress(buffer);

                                            if (buffer != null)
                                            {
                                                statusCode = HttpStatusCode.OK;
                                                ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                                ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                                ctx.Response.StatusCode = (int)statusCode;
                                                ctx.Response.ContentType = CryptoSporidium.HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                                await ctx.Response.SendAsync(buffer);
                                            }
                                            else
                                            {
                                                statusCode = HttpStatusCode.InternalServerError;
                                                ctx.Response.StatusCode = (int)statusCode;
                                                ctx.Response.ContentType = "text/plain";
                                                ctx.Response.Send(true);
                                            }

                                            stream.Flush();
                                        }
                                    }
                                    else
                                        SendFile(ctx, filePath, CryptoSporidium.HTTPUtils.GetMimeType(Path.GetExtension(filePath)), _BufferSize);
                                }
                                else
                                {
                                    LoggerAccessor.LogWarn($"[HTTPS] - {clientip} Requested a non-existant file : {filePath}");
                                    statusCode = HttpStatusCode.NotFound;
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "text/plain";
                                    ctx.Response.Send(true);
                                }
                            }
                            break;
                        case "POST":
                            if (absolutepath.EndsWith(".php") && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                            {
                                var CollectPHP = Extensions.PHP.ProcessPHPPage(filePath, HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx.Request);
                                string? encoding = ctx.Request.RetrieveHeaderValue("Content-Encoding");
                                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && CollectPHP.Item1 != null)
                                {
                                    byte[]? buffer = CryptoSporidium.HTTPUtils.Compress(CollectPHP.Item1);

                                    if (buffer != null)
                                    {
                                        statusCode = HttpStatusCode.OK;
                                        if (CollectPHP.Item2 != null)
                                        {
                                            foreach (var innerArray in CollectPHP.Item2)
                                            {
                                                // Ensure the inner array has at least two elements
                                                if (innerArray.Length >= 2)
                                                {
                                                    // Extract two values from the inner array
                                                    string value1 = innerArray[0];
                                                    string value2 = innerArray[1];
                                                    ctx.Response.Headers.Add(value1, value2);
                                                }
                                            }
                                        }
                                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                        ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/html";
                                        await ctx.Response.SendAsync(buffer);
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                }
                                else
                                {
                                    statusCode = HttpStatusCode.OK;
                                    if (CollectPHP.Item2 != null)
                                    {
                                        foreach (var innerArray in CollectPHP.Item2)
                                        {
                                            // Ensure the inner array has at least two elements
                                            if (innerArray.Length >= 2)
                                            {
                                                // Extract two values from the inner array
                                                string value1 = innerArray[0];
                                                string value2 = innerArray[1];
                                                ctx.Response.Headers.Add(value1, value2);
                                            }
                                        }
                                    }
                                    ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                    ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                    ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "text/html";
                                    await ctx.Response.SendAsync(CollectPHP.Item1);
                                }
                            }
                            else
                            {
                                ctx.Response.StatusCode = (int)statusCode;
                                ctx.Response.ContentType = "text/plain";
                                ctx.Response.Send(true);
                            }
                            break;
                        case "PUT":
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            ctx.Response.Send(true);
                            break;
                        case "DELETE":
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            ctx.Response.Send(true);
                            break;
                        case "HEAD":
                            FileInfo? fileInfo = new(filePath);
                            if (fileInfo.Exists)
                            {
                                statusCode = HttpStatusCode.OK;
                                ctx.Response.ContentType = CryptoSporidium.HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                ctx.Response.Headers.Set("Content-Length", fileInfo.Length.ToString());
                                ctx.Response.Headers.Set("Date", DateTime.Now.ToString("r"));
                                ctx.Response.Headers.Set("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                            }
                            else
                            {
                                LoggerAccessor.LogWarn($"[HTTPS] - {clientip} Requested a non-existant file: {filePath}");
                                statusCode = HttpStatusCode.NotFound;
                            }
                            fileInfo = null;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            ctx.Response.Send(true);
                            break;
                        default:
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            ctx.Response.Send(true);
                            break;
                    }
                }
            }
            else
            {
                ctx.Response.StatusCode = (int)statusCode;
                ctx.Response.ContentType = "text/plain";
                ctx.Response.Send(true);
            }
        }

        private static void SendFile(HttpContext ctx, string filePath, string contentType, int bufferSize)
        {
            long contentLen = new FileInfo(filePath).Length;

            using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ctx.Response.ContentType = contentType;
                ctx.Response.StatusCode = 200;
                ctx.Response.Send(contentLen, fs);
                fs.Flush();
            }
        }
    }
}
