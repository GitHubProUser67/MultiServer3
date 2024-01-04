using BackendProject;
using BackendProject.SSDP_DLNA;
using BackendProject.WebAPIs;
using BackendProject.WebAPIs.OHS;
using BackendProject.WebAPIs.PREMIUMAGENCY;
using CustomLogger;
using HttpServerLite;
using Newtonsoft.Json;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace HTTPSecureServerLite
{
    public class HttpsProcessor
    {
        private static Dictionary<string, DnsSettings>? DicRules = null;
        private static List<KeyValuePair<string, DnsSettings>>? StarRules = null;
        public static bool IsStarted = false;
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
                if (!string.IsNullOrEmpty(HTTPSServerConfiguration.DNSOnlineConfig))
                {
                    LoggerAccessor.LogInfo("[HTTPS_DNS] - Downloading Configuration File...");
                    if (MiscUtils.IsWindows()) ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                    string content = string.Empty;
                    try
                    {
                        HttpClient client = new();
                        HttpResponseMessage response = client.GetAsync(HTTPSServerConfiguration.DNSOnlineConfig).Result;
                        response.EnsureSuccessStatusCode();
                        content = response.Content.ReadAsStringAsync().Result;
                        ParseRules(content, false);
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[HTTPS_DNS] - Online Config failed to initialize! - {ex}");
                    }
                }
                else if (DicRules == null)
                {
                    if (File.Exists(HTTPSServerConfiguration.DNSConfig))
                        ParseRules(HTTPSServerConfiguration.DNSConfig);
                    else
                        LoggerAccessor.LogError("[HTTPS_DNS] - No config text file, so HTTPS_DNS server configuration is aborted!");
                }

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
            string fullurl = string.Empty;
            string absolutepath = string.Empty;
            string Host = ctx.Request.RetrieveHeaderValue("Host");
            string clientip = ctx.Request.Source.IpAddress;
            string clientport = ctx.Request.Source.Port.ToString();

            try
            {
                if (string.IsNullOrEmpty(Host)) // DNS HTTPS do not expose a UserAgent.
                    LoggerAccessor.LogInfo($"[HTTPS] - Client - {clientip} Requested the HTTPS Server with invalid parameters!");
                else
                {
                    if (ctx.Request.Url != null && !ctx.Request.RetrieveHeaderValue("User-Agent").ToLower().Contains("bytespider")) // Get Away TikTok.
                    {
                        fullurl = HTTPUtils.DecodeUrl(ctx.Request.Url.Full);

                        LoggerAccessor.LogInfo($"[HTTPS] - Client - {clientip} Requested the HTTPS Server with URL : {fullurl}");

                        // get filename path
                        absolutepath = HTTPUtils.RemoveQueryString(fullurl);
                        statusCode = HttpStatusCode.Continue;
                    }
                    else
                        LoggerAccessor.LogInfo($"[HTTPS] - Client - {clientip} Requested the HTTPS Server with invalid parameters!");
                }
            }
            catch (Exception)
            {

            }

            ctx.Response.Headers.Add("Server", MiscUtils.GenerateServerSignature());

            if (statusCode == HttpStatusCode.Continue)
            {
                // Split the URL into segments
                string[] segments = absolutepath.Trim('/').Split('/');

                // Combine the folder segments into a directory path
                string directoryPath = Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                // Process the request based on the HTTP method
                string filePath = Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, absolutepath[1..]);

                if ((absolutepath == "/" || absolutepath == "\\") && ctx.Request.Method.ToString() == "GET")
                {
                    bool handled = false;

                    foreach (string indexFile in HTTPUtils.DefaultDocuments)
                    {
                        if (File.Exists(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile))
                        {
                            handled = true;

                            string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");

                            using (FileStream stream = new(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
                                        statusCode = HttpStatusCode.OK;
                                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile).ToString("r"));
                                        ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile));
                                        await ctx.Response.SendAsync(HTTPUtils.Compress(buffer));
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.OK;
                                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile).ToString("r"));
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile));
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
                    }

                    if (!handled)
                    {
                        statusCode = HttpStatusCode.NotFound;
                        ctx.Response.StatusCode = (int)statusCode;
                        ctx.Response.ContentType = "text/plain";
                        ctx.Response.Send(true);
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
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.SendAsync(res);
                }
                else if ((Host == "stats.outso-srv1.com" || Host == "www.outso-srv1.com") && absolutepath.EndsWith("/") && (absolutepath.Contains("/ohs") || absolutepath.Contains("/statistic/")))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a OHS method : {absolutepath}");

                    OHSClass ohs = new(ctx.Request.Method.ToString(), absolutepath, 0);
                    string? res = ohs.ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType, filePath);
                    ohs.Dispose();
                    if (string.IsNullOrEmpty(res))
                    {
                        ctx.Response.ContentType = "text/plain";
                        statusCode = HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        res = $"<ohs>{res}</ohs>";
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                        ctx.Response.ContentType = "application/xml;charset=UTF-8";
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    await ctx.Response.SendAsync(res);
                }
                else if ((Host == "test.playstationhome.jp" || Host == "playstationhome.jp") && ctx.Request.ContentType.StartsWith("multipart/form-data") && absolutepath.Contains("/eventController/") && absolutepath.EndsWith(".do"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a PREMIUMAGENCY method : {absolutepath}");

                    PREMIUMAGENCYClass agency = new(ctx.Request.Method.ToString(), absolutepath, HTTPSServerConfiguration.HTTPSStaticFolder);
                    string? res = agency.ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType);
                    agency.Dispose();
                    if (string.IsNullOrEmpty(res))
                    {
                        ctx.Response.ContentType = "text/plain";
                        statusCode = HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                        ctx.Response.ContentType = "text/xml";
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    await ctx.Response.SendAsync(res);
                }
                else
                {
                    switch (ctx.Request.Method.ToString())
                    {
                        case "GET":
                            switch (absolutepath)
                            {
                                case "/dns-query":
                                    bool acceptsDoH = false;

                                    string? requestAccept = ctx.Request.Headers["Accept"];
                                    if (string.IsNullOrEmpty(requestAccept))
                                        acceptsDoH = true;
                                    else
                                    {
                                        foreach (string mediaType in requestAccept.Split(','))
                                        {
                                            if (mediaType.Equals("application/dns-message", StringComparison.OrdinalIgnoreCase))
                                            {
                                                acceptsDoH = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (!acceptsDoH)
                                    {
                                        statusCode = HttpStatusCode.BadRequest;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        await ctx.Response.SendAsync("Bad Request");
                                    }
                                    else
                                    {
                                        string? dnsRequestBase64Url = ctx.Request.Query.Elements["dns"];
                                        if (string.IsNullOrEmpty(dnsRequestBase64Url))
                                        {
                                            statusCode = HttpStatusCode.BadRequest;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            await ctx.Response.SendAsync("Bad Request");
                                        }
                                        else
                                        {
                                            //convert from base64url to base64
                                            dnsRequestBase64Url = dnsRequestBase64Url.Replace('-', '+');
                                            dnsRequestBase64Url = dnsRequestBase64Url.Replace('_', '/');

                                            //add padding
                                            int x = dnsRequestBase64Url.Length % 4;
                                            if (x > 0)
                                                dnsRequestBase64Url = dnsRequestBase64Url.PadRight(dnsRequestBase64Url.Length - x + 4, '=');

                                            try
                                            {
                                                byte[]? DnsReq = Convert.FromBase64String(dnsRequestBase64Url);

                                                string fullname = string.Join(".", HTTPUtils.GetDnsName(DnsReq).ToArray());

                                                LoggerAccessor.LogInfo($"[HTTPS_DNS] - Host: {fullname} was Requested.");

                                                string url = string.Empty;
                                                bool treated = false;

                                                if (DicRules != null && DicRules.ContainsKey(fullname))
                                                {
                                                    if (DicRules[fullname].Mode == HandleMode.Allow) url = fullname;
                                                    else if (DicRules[fullname].Mode == HandleMode.Redirect) url = DicRules[fullname].Address ?? "127.0.0.1";
                                                    else if (DicRules[fullname].Mode == HandleMode.Deny) url = "NXDOMAIN";
                                                    treated = true;
                                                }

                                                if (!treated && StarRules != null)
                                                {
                                                    foreach (KeyValuePair<string, DnsSettings> rule in StarRules)
                                                    {
                                                        Regex regex = new(rule.Key);
                                                        if (!regex.IsMatch(fullname))
                                                            continue;

                                                        if (rule.Value.Mode == HandleMode.Allow) url = fullname;
                                                        else if (rule.Value.Mode == HandleMode.Redirect) url = rule.Value.Address ?? "127.0.0.1";
                                                        else if (rule.Value.Mode == HandleMode.Deny) url = "NXDOMAIN";
                                                        treated = true;
                                                        break;
                                                    }
                                                }

                                                if (!treated && HTTPSServerConfiguration.DNSAllowUnsafeRequests)
                                                    url = MiscUtils.GetFirstActiveIPAddress(fullname, MiscUtils.GetPublicIPAddress(true));

                                                IPAddress ip = IPAddress.None;
                                                if (url != string.Empty && url != "NXDOMAIN")
                                                {
                                                    try
                                                    {
                                                        IPAddress? address;
                                                        if (!IPAddress.TryParse(url, out address))
                                                            ip = Dns.GetHostEntry(url).AddressList[0];
                                                        else ip = address;
                                                    }
                                                    catch (Exception)
                                                    {
                                                        ip = IPAddress.None;
                                                    }

                                                    LoggerAccessor.LogInfo($"[HTTPS_DNS] - Resolved: {fullname} to: {ip}");

                                                    DnsReq = HTTPUtils.MakeDnsResponsePacket(DnsReq, ip);
                                                }
                                                else if (url == "NXDOMAIN")
                                                    DnsReq = HTTPUtils.MakeDnsResponsePacket(DnsReq, ip);

                                                if (DnsReq != null && DnsReq.Length <= 512) // Https wire expect padding.
                                                {
                                                    // Create a new byte array with size 512
                                                    byte[] paddedArray = new byte[512];

                                                    // Copy the original array content to the padded array
                                                    Array.Copy(DnsReq, paddedArray, DnsReq.Length);

                                                    statusCode = HttpStatusCode.OK;
                                                    ctx.Response.StatusCode = (int)statusCode;
                                                    ctx.Response.ContentType = "application/dns-message";
                                                    await ctx.Response.SendAsync(DnsReq);
                                                }
                                                else
                                                {
                                                    statusCode = HttpStatusCode.InternalServerError;
                                                    ctx.Response.StatusCode = (int)statusCode;
                                                    ctx.Response.ContentType = "text/plain";
                                                    ctx.Response.Send(true);
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                statusCode = HttpStatusCode.InternalServerError;
                                                ctx.Response.StatusCode = (int)statusCode;
                                                ctx.Response.ContentType = "text/plain";
                                                ctx.Response.Send(true);
                                            }
                                        }
                                    }
                                    break;
                                case "/robots.txt": // Get Away Google.
                                    statusCode = HttpStatusCode.OK;
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "text/plain";
                                    await ctx.Response.SendAsync("User-agent: *\nDisallow: / ");
                                    break;
                                case "/!DLNADiscovery/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        statusCode = HttpStatusCode.OK;
                                        SSDP.Start(); // Start a service as this will take a long time
                                        Thread.Sleep(14000); // Wait for each TV/Device to reply to the broadcast
                                        SSDP.Stop(); // Stop the service if it has not stopped already
                                        List<DlnaDeviceInfo> devices = new();
                                        // 2 Threads only.
                                        Parallel.ForEach(SSDP.Servers.Split(' '), new ParallelOptions { MaxDegreeOfParallelism = 2 }, url => {
                                            string? xmlContent = FetchDLNARemote.FetchXmlContent(url);
                                            if (!string.IsNullOrEmpty(xmlContent))
                                                devices.Add(FetchDLNARemote.ParseXml(xmlContent, url));
                                        });
                                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "application/json;charset=UTF-8";
                                        string? encoding0 = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                                        if (!string.IsNullOrEmpty(encoding0) && encoding0.Contains("gzip"))
                                        {
                                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                            await ctx.Response.SendAsync(HTTPUtils.Compress(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(devices, Formatting.Indented))));
                                        }
                                        else
                                            await ctx.Response.SendAsync(JsonConvert.SerializeObject(devices, Formatting.Indented));
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.Forbidden;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                    break;
                                case "/!DLNAPlay/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        string? src = ctx.Request.RetrieveQueryValue("src");
                                        string? dst = ctx.Request.RetrieveQueryValue("dst");
                                        ctx.Response.ContentType = "text/plain";
                                        if (!string.IsNullOrEmpty(src) && !string.IsNullOrEmpty(dst))
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                            ctx.Response.StatusCode = (int)statusCode;
                                            DLNADevice Device = new(src);
                                            if (Device.IsConnected())
                                                await ctx.Response.SendAsync($"DLNA Player returned {Device.TryToPlayFile(dst)}");
                                            else
                                                await ctx.Response.SendAsync("Failed to send to TV");
                                        }
                                        else
                                        {
                                            statusCode = HttpStatusCode.Forbidden;
                                            ctx.Response.StatusCode = (int)statusCode;
											ctx.Response.ContentType = "text/plain";
                                            ctx.Response.Send(true);
                                        }
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.Forbidden;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                    break;
                                default:
                                    if (Directory.Exists(filePath) && filePath.EndsWith("/"))
                                    {
                                        string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                        {
                                            byte[]? buffer = HTTPUtils.Compress(Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(filePath[..^1])));

                                            if (buffer != null)
                                            {
                                                statusCode = HttpStatusCode.OK;
                                                ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                                ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
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
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "application/json";
                                            await ctx.Response.SendAsync(FileStructureToJson.GetFileStructureAsJson(filePath[..^1]));
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
                                        string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && CollectPHP.Item1 != null)
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
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/html";
                                            await ctx.Response.SendAsync(HTTPUtils.Compress(CollectPHP.Item1));
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
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
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

                                            string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            if (ContentType == "application/octet-stream")
                                            {
                                                byte[] VerificationChunck = MiscUtils.ReadSmallFileChunck(filePath, 10);
                                                foreach (var entry in HTTPUtils.PathernDictionary)
                                                {
                                                    if (MiscUtils.FindbyteSequence(VerificationChunck, entry.Value))
                                                    {
                                                        ContentType = entry.Key;
                                                        break;
                                                    }
                                                }
                                            }
                                            SendFile(ctx, filePath, ContentType);
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
                            }
                            break;
                        case "POST":
                            switch (absolutepath)
                            {
                                case "/dns-query":
                                    if (!string.Equals(ctx.Request.ContentType, "application/dns-message", StringComparison.OrdinalIgnoreCase))
                                    {
                                        statusCode = HttpStatusCode.UnsupportedMediaType;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        await ctx.Response.SendAsync("Unsupported Media Type");
                                    }
                                    else
                                    {
                                        try
                                        {
                                            byte[]? DnsReq = ctx.Request.DataAsBytes;

                                            string fullname = string.Join(".", HTTPUtils.GetDnsName(DnsReq).ToArray());

                                            LoggerAccessor.LogInfo($"[HTTPS_DNS] - Host: {fullname} was Requested.");

                                            string url = string.Empty;
                                            bool treated = false;

                                            if (DicRules != null && DicRules.ContainsKey(fullname))
                                            {
                                                if (DicRules[fullname].Mode == HandleMode.Allow) url = fullname;
                                                else if (DicRules[fullname].Mode == HandleMode.Redirect) url = DicRules[fullname].Address ?? "127.0.0.1";
                                                else if (DicRules[fullname].Mode == HandleMode.Deny) url = "NXDOMAIN";
                                                treated = true;
                                            }

                                            if (!treated && StarRules != null)
                                            {
                                                foreach (KeyValuePair<string, DnsSettings> rule in StarRules)
                                                {
                                                    Regex regex = new(rule.Key);
                                                    if (!regex.IsMatch(fullname))
                                                        continue;

                                                    if (rule.Value.Mode == HandleMode.Allow) url = fullname;
                                                    else if (rule.Value.Mode == HandleMode.Redirect) url = rule.Value.Address ?? "127.0.0.1";
                                                    else if (rule.Value.Mode == HandleMode.Deny) url = "NXDOMAIN";
                                                    treated = true;
                                                    break;
                                                }
                                            }

                                            if (!treated && HTTPSServerConfiguration.DNSAllowUnsafeRequests)
                                                url = MiscUtils.GetFirstActiveIPAddress(fullname, MiscUtils.GetPublicIPAddress(true));


                                            IPAddress ip = IPAddress.None;
                                            if (url != string.Empty && url != "NXDOMAIN")
                                            {
                                                try
                                                {
                                                    IPAddress? address;
                                                    if (!IPAddress.TryParse(url, out address))
                                                        ip = Dns.GetHostEntry(url).AddressList[0];
                                                    else ip = address;
                                                }
                                                catch (Exception)
                                                {
                                                    ip = IPAddress.None;
                                                }

                                                LoggerAccessor.LogInfo($"[HTTPS_DNS] - Resolved: {fullname} to: {ip}");

                                                DnsReq = HTTPUtils.MakeDnsResponsePacket(DnsReq, ip);
                                            }
                                            else if (url == "NXDOMAIN")
                                                DnsReq = HTTPUtils.MakeDnsResponsePacket(DnsReq, ip);

                                            if (DnsReq != null && DnsReq.Length <= 512) // Https wire expect padding.
                                            {
                                                // Create a new byte array with size 512
                                                byte[] paddedArray = new byte[512];

                                                // Copy the original array content to the padded array
                                                Array.Copy(DnsReq, paddedArray, DnsReq.Length);

                                                statusCode = HttpStatusCode.OK;
                                                ctx.Response.StatusCode = (int)statusCode;
                                                ctx.Response.ContentType = "application/dns-message";
                                                await ctx.Response.SendAsync(DnsReq);
                                            }
                                            else
                                            {
                                                statusCode = HttpStatusCode.InternalServerError;
                                                ctx.Response.StatusCode = (int)statusCode;
                                                ctx.Response.ContentType = "text/plain";
                                                ctx.Response.Send(true);
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            ctx.Response.Send(true);
                                        }
                                    }
                                    break;
                                case "/!HomeTools/MakeBarSdat/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var makeres = HomeToolsInterface.MakeBarSdat(ctx.Request.Data, ctx.Request.ContentType);
                                        if (makeres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
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
                                        var unbarres = HomeToolsInterface.UnBar(ctx.Request.Data, ctx.Request.ContentType, HTTPSServerConfiguration.HomeToolsHelperStaticFolder).Result;
                                        if (unbarres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
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
                                        var cdsres = HomeToolsInterface.CDS(ctx.Request.Data, ctx.Request.ContentType);
                                        if (cdsres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
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
                                case "/!HomeTools/CDSBruteforce/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var cdsres = HomeToolsInterface.CDSBruteforce(ctx.Request.Data, ctx.Request.ContentType, HTTPSServerConfiguration.HomeToolsHelperStaticFolder);
                                        if (cdsres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
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
                                case "/!HomeTools/HCDBUnpack/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var cdsres = HomeToolsInterface.HCDBUnpack(ctx.Request.Data, ctx.Request.ContentType);
                                        if (cdsres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
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
                                        var ticketlistres = HomeToolsInterface.TicketList(ctx.Request.Data, ctx.Request.ContentType);
                                        if (ticketlistres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
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
                                        var infres = HomeToolsInterface.INF(ctx.Request.Data, ctx.Request.ContentType);
                                        if (infres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
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
                                        string? channelres = HomeToolsInterface.ChannelID(ctx.Request.Data, ctx.Request.ContentType);
                                        if (!string.IsNullOrEmpty(channelres))
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
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
                                        string? sceneres = HomeToolsInterface.SceneID(ctx.Request.Data, ctx.Request.ContentType);
                                        if (!string.IsNullOrEmpty(sceneres))
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
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
                                        string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && CollectPHP.Item1 != null)
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
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/html";
                                            await ctx.Response.SendAsync(HTTPUtils.Compress(CollectPHP.Item1));
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
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/html";
                                            await ctx.Response.SendAsync(CollectPHP.Item1);
                                        }
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.Forbidden;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        ctx.Response.Send(true);
                                    }
                                    break;
                            }
                            break;
                        case "PUT":
                            statusCode = HttpStatusCode.Forbidden;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            ctx.Response.Send(true);
                            break;
                        case "DELETE":
                            statusCode = HttpStatusCode.Forbidden;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            ctx.Response.Send(true);
                            break;
                        case "HEAD":
                            FileInfo? fileInfo = new(filePath);
                            if (fileInfo.Exists)
                            {
                                statusCode = HttpStatusCode.OK;
                                string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                if (ContentType == "application/octet-stream")
                                {
                                    bool matched = false;
                                    byte[] VerificationChunck = MiscUtils.ReadSmallFileChunck(filePath, 10);
                                    foreach (var entry in HTTPUtils.PathernDictionary)
                                    {
                                        if (MiscUtils.FindbyteSequence(VerificationChunck, entry.Value))
                                        {
                                            matched = true;
                                            ctx.Response.ContentType = entry.Key;
                                            break;
                                        }
                                    }
                                    if (!matched)
                                        ctx.Response.ContentType = ContentType;
                                }
                                else
                                    ctx.Response.ContentType = ContentType;
                                ctx.Response.Headers.Set("Content-Length", fileInfo.Length.ToString());
                                ctx.Response.Headers.Set("Date", DateTime.Now.ToString("r"));
                                ctx.Response.Headers.Set("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                ctx.Response.ContentLength = fileInfo.Length;
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
                        case "OPTIONS":
                            statusCode = HttpStatusCode.OK;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            ctx.Response.Headers.Set("Allow", "OPTIONS, GET, HEAD, POST");
                            ctx.Response.Send(true);
                            break;
                        case "PROPFIND":
                            statusCode = HttpStatusCode.NotImplemented;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            ctx.Response.Send(true);
                            break;
                        default:
                            statusCode = HttpStatusCode.Forbidden;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            ctx.Response.Send(true);
                            break;
                    }
                }
            }
            else
            {
                ctx.Response.StatusCode = (int)statusCode; // Send the other status.
                ctx.Response.ContentType = "text/plain";
                ctx.Response.Send(true);
            }
        }

        [ParameterRoute(HttpServerLite.HttpMethod.GET, "/objects/D2CDD8B2-DE444593-A64C68CB-0B5EDE23/{id}.xml")]
        public static async Task SportWalkQuizRoute(HttpContext ctx)
        {
            string? QuizID = ctx.Request.Url.Parameters["id"];

            if (!string.IsNullOrEmpty(QuizID))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                ctx.Response.ContentType = "text/xml";
                await ctx.Response.SendAsync("<Root></Root>"); // TODO - Figure out this complicated LUAC in object : D2CDD8B2-DE444593-A64C68CB-0B5EDE23
            }
            else
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                ctx.Response.ContentType = "text/plain";
                ctx.Response.Send(true);
            }
        }

        private static void SendFile(HttpContext ctx, string filePath, string contentType)
        {
            long contentLen = new FileInfo(filePath).Length;

            using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                ctx.Response.ContentType = contentType;
                ctx.Response.StatusCode = 200;
                ctx.Response.Send(contentLen, fs);
                fs.Flush();
            }
        }

        private void ParseRules(string Filename, bool IsFilename = true)
        {
            DicRules = new Dictionary<string, DnsSettings>();
            StarRules = new List<KeyValuePair<string, DnsSettings>>();

            if (Path.GetFileNameWithoutExtension(Filename).ToLower() == "boot")
                DicRules = ParseSimpleDNSRules(Filename, DicRules);
            else
            {
                HashSet<string> processedDomains = new();
                string[] rules = IsFilename ? File.ReadAllLines(Filename) : Filename.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                Parallel.ForEach(rules, s => {
                    if (s.StartsWith(";") || s.Trim() == string.Empty)
                    {

                    }
                    else
                    {
                        string[] split = s.Split(',');
                        DnsSettings dns = new();
                        switch (split[1].Trim().ToLower())
                        {
                            case "deny":
                                dns.Mode = HandleMode.Deny;
                                break;
                            case "allow":
                                dns.Mode = HandleMode.Allow;
                                break;
                            case "redirect":
                                dns.Mode = HandleMode.Redirect;
                                string IpFromConfig = split[2].Trim();
                                if (Regex.IsMatch(IpFromConfig, @"^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,7}:$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,1}(:[0-9a-fA-F]{1,4}){1,6}$|"
                                                                + @"^:((:[0-9a-fA-F]{0,4}){0,6})?$"))
                                    dns.Address = IpFromConfig;
                                else
                                    dns.Address = MiscUtils.GetLocalIPAddress().ToString();
                                break;
                            default:
                                LoggerAccessor.LogWarn($"[DNS] - Rule : {s} is not a formated properly, skipping...");
                                break;
                        }

                        string domain = split[0].Trim();

                        // Check if the domain has been processed before
                        if (!processedDomains.Contains(domain))
                        {
                            processedDomains.Add(domain);

                            if (domain.Contains("*"))
                            {
                                // Escape all possible URI characters conflicting with Regex
                                domain = domain.Replace(".", "\\.");
                                domain = domain.Replace("$", "\\$");
                                domain = domain.Replace("[", "\\[");
                                domain = domain.Replace("]", "\\]");
                                domain = domain.Replace("(", "\\(");
                                domain = domain.Replace(")", "\\)");
                                domain = domain.Replace("+", "\\+");
                                domain = domain.Replace("?", "\\?");
                                // Replace "*" characters with ".*" which means any number of any character for Regexp
                                domain = domain.Replace("*", ".*");
                                StarRules.Add(new KeyValuePair<string, DnsSettings>(domain, dns));
                            }
                            else
                            {
                                DicRules.Add(domain, dns);
                                DicRules.Add("www." + domain, dns);
                            }
                        }
                    }
                });
            }

            LoggerAccessor.LogInfo("[HTTPS_DNS] - " + DicRules.Count.ToString() + " dictionary rules and " + StarRules.Count.ToString() + " star rules loaded");
        }

        private Dictionary<string, DnsSettings> ParseSimpleDNSRules(string Filename, Dictionary<string, DnsSettings> DicRules)
        {
            // Read all lines from the test file
            string[] lines = File.ReadAllLines(Filename);

            // Define a list to store extracted hostnames
            List<string> hostnames = new();

            Parallel.ForEach(lines, line => {
                // Split the line by tab character
                string[] parts = line.Split('\t');

                // Check if the line has enough parts and the primary entry is not empty
                if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    // Extract the hostname from the primary entry
                    string hostname = parts[1].Trim();

                    // Add the hostname to the list
                    hostnames.Add(hostname);
                }
            });

            DnsSettings dns = new();

            // Iterate through the extracted hostnames and search for corresponding .dns files
            Parallel.ForEach(hostnames, hostname => {
                string dnsFilePath = Path.GetDirectoryName(Filename) + $"/{hostname}.dns";

                // Check if the .dns file exists
                if (File.Exists(dnsFilePath))
                {
                    string[] dnsFileLines = File.ReadAllLines(dnsFilePath);

                    foreach (string line in dnsFileLines)
                    {
                        if (line.StartsWith("\t\tA"))
                        {
                            // Extract the IP address using a regular expression
                            Match match = Regex.Match(line, @"A\s+(\S+)");
                            if (match.Success)
                            {
                                dns.Mode = HandleMode.Redirect;
                                string IpFromConfig = match.Groups[1].Value;
                                if (Regex.IsMatch(IpFromConfig, @"^((25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,7}:$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}$|"
                                                                + @"^([0-9a-fA-F]{1,4}:){1,1}(:[0-9a-fA-F]{1,4}){1,6}$|"
                                                                + @"^:((:[0-9a-fA-F]{0,4}){0,6})?$"))
                                    dns.Address = IpFromConfig;
                                else
                                    dns.Address = MiscUtils.GetLocalIPAddress().ToString();
                                DicRules.Add(hostname, dns);
                                DicRules.Add("www." + hostname, dns);
                                break;
                            }
                        }
                    }
                }
            });

            return DicRules;
        }

        private bool MyRemoteCertificateValidationCallback(object? sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; //This isn't a good thing to do, but to keep the code simple i prefer doing this, it will be used only on mono
        }

        public struct DnsSettings
        {
            public string? Address; //For redirect to
            public HandleMode? Mode;
        }

        public enum HandleMode
        {
            Deny,
            Allow,
            Redirect
        }
    }
}
