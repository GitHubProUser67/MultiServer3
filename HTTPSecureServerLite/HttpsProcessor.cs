using CryptoSporidium;
using CryptoSporidium.WebAPIs;
using CryptoSporidium.WebAPIs.OHS;
using CryptoSporidium.WebAPIs.PREMIUMAGENCY;
using CustomLogger;
using HttpServerLite;
using System.Net;
using System.Text;

namespace HTTPSecureServerLite
{
    public class HttpsProcessor
    {
        public static bool IsStarted = false;
        private static int _BufferSize = 65536;
        private static Webserver? _Server;
        private string ip;
        private string certpath;
        private string certpass;
        private int port;

        public HttpsProcessor(string certpath, string certpass, string ip, int port)
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

        private static bool IsIPAllowed(string ipAddress)
        {
            if ((HTTPSServerConfiguration.AllowedIPs != null && HTTPSServerConfiguration.AllowedIPs.Contains(ipAddress)) || ipAddress == "127.0.0.1" || ipAddress.ToLower() == "localhost")
                return true;

            return false;
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
            string Host = ctx.Request.RetrieveHeaderValue("Host");
            string clientip = ctx.Request.Source.IpAddress;
            string clientport = ctx.Request.Source.Port.ToString();

            try
            {
                if (string.IsNullOrEmpty(UserAgent) || string.IsNullOrEmpty(Host))
                    LoggerAccessor.LogInfo($"[HTTPS] - Client - {clientip} Requested the HTTPS Server with invalid parameters!");
                else
                {
                    if (ctx.Request.Url != null)
                    {
                        LoggerAccessor.LogInfo($"[HTTPS] - Client - {clientip} Requested the HTTPS Server with URL : {ctx.Request.Url.Full}");

                        // get filename path
                        absolutepath = HTTPUtils.RemoveQueryString(ctx.Request.Url.Full);
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
                string filePath = Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, absolutepath[1..]);

                if (ctx.Request.Method.ToString() == "OPTIONS")
                {
                    ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                    ctx.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, HEAD");
                    ctx.Response.Headers.Add("Access-Control-Max-Age", "1728000");
                }

                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                if ((absolutepath == "/" || absolutepath == "\\") && ctx.Request.Method.ToString() == "GET")
                {
                    foreach (string indexFile in HTTPUtils.DefaultDocuments)
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
                                        buffer = HTTPUtils.Compress(buffer);

                                        if (buffer != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(indexFile));
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
                                        ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(indexFile));
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
                else if ((Host == "away.veemee.com" || Host == "home.veemee.com") && absolutepath.EndsWith(".php"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a VEEMEE method : {absolutepath}");

                    API.VEEMEE.VEEMEEClass veemee = new(ctx.Request.Method.ToString(), absolutepath); // Todo, loss of GET informations.
                    var res = veemee.ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType);
                    veemee.Dispose();
                    if (string.IsNullOrEmpty(res.Item1))
                        statusCode = HttpStatusCode.InternalServerError;
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    if (!string.IsNullOrEmpty(res.Item2))
                        ctx.Response.ContentType = res.Item2;
                    else
                        ctx.Response.ContentType = "text/plain";
                    await ctx.Response.SendAsync(res.Item1);
                }
                else if (Host == "game2.hellfiregames.com" && absolutepath.EndsWith(".php"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a HELLFIRE method : {absolutepath}");

                    API.HELLFIRE.HELLFIREClass hellfire = new(ctx.Request.Method.ToString(), HTTPUtils.RemoveQueryString(absolutepath));
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
                else if ((Host == "stats.outso-srv1.com" || Host == "www.outso-srv1.com") && absolutepath.Contains("/ohs") && absolutepath.EndsWith("/"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a OHS method : {absolutepath}");

                    OHSClass ohs = new(ctx.Request.Method.ToString(), absolutepath, 0);
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
                else if ((Host == "test.playstationhome.jp" || Host == "playstationhome.jp") && ctx.Request.ContentType.StartsWith("multipart/form-data") && absolutepath.Contains("/eventController/") && absolutepath.EndsWith(".do"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a PREMIUMAGENCY method : {absolutepath}");

                    PREMIUMAGENCYClass agency = new(ctx.Request.Method.ToString(), absolutepath, HTTPSServerConfiguration.HTTPSStaticFolder);
                    string? res = agency.ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType);
                    agency.Dispose();
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
                    if (!string.IsNullOrEmpty(res))
                        ctx.Response.ContentType = "text/xml";
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
                                    byte[]? buffer = HTTPUtils.Compress(Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(filePath.Substring(0, filePath.Length - 1))));

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
                                    await ctx.Response.SendAsync(FileStructureToJson.GetFileStructureAsJson(filePath.Substring(0, filePath.Length - 1)));
                                }
                            }
                            else if (absolutepath.ToLower().EndsWith(".php") && !string.IsNullOrEmpty(HTTPSServerConfiguration.PHPRedirectUrl))
                            {
                                statusCode = HttpStatusCode.PermanentRedirect;
                                ctx.Response.Headers.Add("Location", $"{HTTPSServerConfiguration.PHPRedirectUrl}{ctx.Request.Url}");
                                ctx.Response.StatusCode = (int)statusCode;
                                ctx.Response.ContentType = "text/html";
                                ctx.Response.Send(true);
                            }
                            else if (absolutepath.ToLower().EndsWith(".php") && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                            {
                                var CollectPHP = Extensions.PHP.ProcessPHPPage(filePath, HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx.Request);
                                string? encoding = ctx.Request.RetrieveHeaderValue("Content-Encoding");
                                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && CollectPHP.Item1 != null)
                                {
                                    byte[]? buffer = HTTPUtils.Compress(CollectPHP.Item1);

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

                                            buffer = HTTPUtils.Compress(buffer);

                                            if (buffer != null)
                                            {
                                                statusCode = HttpStatusCode.OK;
                                                ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                                ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                                ctx.Response.StatusCode = (int)statusCode;
                                                ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
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
                                        SendFile(ctx, filePath, HTTPUtils.GetMimeType(Path.GetExtension(filePath)), _BufferSize);
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
                            switch (absolutepath)
                            {
                                case "/!HomeTools/MakeBarSdat/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var makeres = HomeTools.MakeBarSdat(ctx.Request.Data, ctx.Request.ContentType);
                                        if (makeres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={makeres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            await ctx.Response.SendAsync(makeres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            ctx.Response.Send(true);
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                    break;
                                case "/!HomeTools/UnBar/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var unbarres = HomeTools.UnBar(ctx.Request.Data, ctx.Request.ContentType, HTTPSServerConfiguration.HomeToolsHelperStaticFolder).Result;
                                        if (unbarres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={unbarres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            await ctx.Response.SendAsync(unbarres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            ctx.Response.Send(true);
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                    break;
                                case "/!HomeTools/CDS/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var cdsres = HomeTools.CDS(ctx.Request.Data, ctx.Request.ContentType);
                                        if (cdsres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={cdsres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            await ctx.Response.SendAsync(cdsres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            ctx.Response.Send(true);
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                    break;
                                case "/!HomeTools/TicketList/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var ticketlistres = HomeTools.TicketList(ctx.Request.Data, ctx.Request.ContentType);
                                        if (ticketlistres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={ticketlistres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            await ctx.Response.SendAsync(ticketlistres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            ctx.Response.Send(true);
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                    break;
                                case "/!HomeTools/INF/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var infres = HomeTools.INF(ctx.Request.Data, ctx.Request.ContentType);
                                        if (infres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={infres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            await ctx.Response.SendAsync(infres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            ctx.Response.Send(true);
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                    break;
                                case "/!HomeTools/ChannelID/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        string? channelres = HomeTools.ChannelID(ctx.Request.Data, ctx.Request.ContentType);
                                        if (!string.IsNullOrEmpty(channelres))
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            await ctx.Response.SendAsync(channelres);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            ctx.Response.Send(true);
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                    break;
                                case "/!HomeTools/SceneID/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        string? sceneres = HomeTools.SceneID(ctx.Request.Data, ctx.Request.ContentType);
                                        if (!string.IsNullOrEmpty(sceneres))
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            await ctx.Response.SendAsync(sceneres);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            ctx.Response.Send(true);
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                    break;
                                default:
                                    if (absolutepath.ToLower().EndsWith(".php") && !string.IsNullOrEmpty(HTTPSServerConfiguration.PHPRedirectUrl))
                                    {
                                        statusCode = HttpStatusCode.PermanentRedirect;
                                        ctx.Response.Headers.Add("Location", $"{HTTPSServerConfiguration.PHPRedirectUrl}{ctx.Request.Url}");
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/html";
                                        ctx.Response.Send(true);
                                    }
                                    else if (absolutepath.ToLower().EndsWith(".php") && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                    {
                                        var CollectPHP = Extensions.PHP.ProcessPHPPage(filePath, HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx.Request);
                                        string? encoding = ctx.Request.RetrieveHeaderValue("Content-Encoding");
                                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && CollectPHP.Item1 != null)
                                        {
                                            byte[]? buffer = HTTPUtils.Compress(CollectPHP.Item1);

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
                                ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
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
