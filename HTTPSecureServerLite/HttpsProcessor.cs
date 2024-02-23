using BackendProject.FileHelper.Utils;
using BackendProject.MiscUtils;
using BackendProject.SSDP_DLNA;
using BackendProject.WebAPIs;
using BackendProject.WebAPIs.HOMECORE;
using BackendProject.WebAPIs.NDREAMS;
using BackendProject.WebAPIs.OHS;
using BackendProject.WebAPIs.PREMIUMAGENCY;
using BackendProject.WeBAPIs.HELLFIRE;
using BackendProject.WeBAPIs.VEEMEE;
using BackendProject.WebTools;
using CustomLogger;
using HttpMultipartParser;
using Newtonsoft.Json;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace HTTPSecureServerLite
{
    public class HttpsProcessor
    {
        private static Dictionary<string, DnsSettings>? DicRules = null;
        private static List<KeyValuePair<string, DnsSettings>>? StarRules = null;
        public static bool IsStarted = false;
        private static WebserverLite? _Server;
        private readonly string ip;
        private readonly string certpath;
        private readonly string certpass;
        private readonly int port;

        public HttpsProcessor(string certpath, string certpass, string ip, int port)
        {
            this.certpath = certpath;
            this.certpass = certpass;
            this.ip = ip;
            this.port = port;
        }

        private static async Task AuthorizeConnection(HttpContextBase ctx)
        {
            if (HTTPSServerConfiguration.BannedIPs != null && HTTPSServerConfiguration.BannedIPs.Contains(ctx.Request.Source.IpAddress))
            {
                LoggerAccessor.LogError($"[SECURITY] - Client - {ctx.Request.Source.IpAddress}:{ctx.Request.Source.Port} Requested the HTTPS server while being banned!");
                ctx.Response.StatusCode = 403;
                await ctx.Response.Send();
            }
        }

        private static bool IsIPAllowed(string ipAddress)
        {
            if ((HTTPSServerConfiguration.AllowedIPs != null && HTTPSServerConfiguration.AllowedIPs.Contains(ipAddress))
                || ipAddress == "127.0.0.1" || ipAddress.ToLower() == "localhost"
                || ipAddress.ToLower() == VariousUtils.GetLocalIPAddress().ToString().ToLower()
                || ipAddress.ToLower() == VariousUtils.GetLocalIPAddress(true).ToString().ToLower())
                return true;

            return false;
        }

        public void StartServer()
        {
            if (_Server != null && _Server.IsListening)
                LoggerAccessor.LogWarn("HTTPS Server already initiated");
            else
            {
                _ = Task.Run(() => {
                    if (!string.IsNullOrEmpty(HTTPSServerConfiguration.DNSOnlineConfig))
                    {
                        LoggerAccessor.LogInfo("[HTTPS_DNS] - Downloading Configuration File...");
                        if (VariousUtils.IsWindows()) ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                        try
                        {
#if NET7_0
                        HttpResponseMessage response = new HttpClient().GetAsync(HTTPSServerConfiguration.DNSOnlineConfig).Result;
                        response.EnsureSuccessStatusCode();
                        ParseRules(response.Content.ReadAsStringAsync().Result, false);
#else
#pragma warning disable // NET 6.0 and lower has a bug where GetAsync() is EXTREMLY slow to operate (https://github.com/dotnet/runtime/issues/65375).
                            ParseRules(new WebClient().DownloadStringTaskAsync(HTTPSServerConfiguration.DNSOnlineConfig).Result, false);
#pragma warning restore
#endif
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
                });

                WebserverSettings settings = new()
                {
                    Hostname = ip,
                    Port = port,
                };

                settings.Ssl.PfxCertificateFile = certpath;
                settings.Ssl.PfxCertificatePassword = certpass;
                settings.Ssl.Enable = true;

                _Server = new WebserverLite(settings, DefaultRoute);
                _Server.Routes.AuthenticateRequest = AuthorizeConnection;
                _Server.Events.Logger = LoggerAccessor.LogInfo;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/objects/D2CDD8B2-DE444593-A64C68CB-0B5EDE23/{id}.xml", async (HttpContextBase ctx) =>
                {
                    string? QuizID = ctx.Request.Url.Parameters["id"];

                    if (!string.IsNullOrEmpty(QuizID))
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        ctx.Response.ContentType = "text/xml";
                        await ctx.Response.Send("<Root></Root>"); // TODO - Figure out this complicated LUAC in object : D2CDD8B2-DE444593-A64C68CB-0B5EDE23
                    }
                    else
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    }
                });

                _Server.Start();
                IsStarted = true;
                LoggerAccessor.LogInfo("HTTPS Server initiated...");
            }
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            HttpStatusCode statusCode = HttpStatusCode.Forbidden;
            string fullurl = string.Empty;
            string absolutepath = string.Empty;
            string Host = ctx.Request.RetrieveHeaderValue("Host");
            string clientip = ctx.Request.Source.IpAddress;
            string clientport = ctx.Request.Source.Port.ToString();
            bool sent = false;

            try
            {
                if (!string.IsNullOrEmpty(ctx.Request.Url.RawWithQuery))
                {
                    if (!string.IsNullOrEmpty(ctx.Request.Useragent) && ctx.Request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
                        LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested the HTTPS Server with a ByteDance crawler!");
                    else
                    {
                        fullurl = HTTPUtils.DecodeUrl(ctx.Request.Url.RawWithQuery);

                        string SuplementalMessage = string.Empty;
                        string? GeoCodeString = GeoIPUtils.GetGeoCodeFromIP(IPAddress.Parse(clientip));

                        if (!string.IsNullOrEmpty(GeoCodeString))
                        {
                            // Split the input string by the '-' character
                            string[] parts = GeoCodeString.Split('-');

                            // Check if there are exactly two parts
                            if (parts.Length == 2)
                                SuplementalMessage = " Located at " + parts[0] + (bool.Parse(parts[1]) ? " Situated in Europe " : string.Empty);
                        }

                        LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport}{SuplementalMessage} Requested the HTTPS Server with URL : {fullurl}");

                        absolutepath = HTTPUtils.ExtractDirtyProxyPath(ctx.Request.RetrieveHeaderValue("Referer")) + HTTPUtils.RemoveQueryString(fullurl);
                        statusCode = HttpStatusCode.Continue;
                    }
                }
                else
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested the HTTPS Server with invalid parameters!");
            }
            catch (Exception)
            {

            }

#if DEBUG
            foreach (string? key in ctx.Request.Headers.AllKeys)
            {
                string? value = ctx.Request.Headers[key];
                LoggerAccessor.LogInfo($"[CollectHeaders] - Debug Headers : HeaderIndex -> {key} | HeaderItem -> {value}");
            }
#endif

            ctx.Response.Headers.Add("Server", VariousUtils.GenerateServerSignature());

            if (statusCode == HttpStatusCode.Continue)
            {
                // Split the URL into segments
                string[] segments = absolutepath.Trim('/').Split('/');

                // Combine the folder segments into a directory path
                string directoryPath = Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                // Process the request based on the HTTP method
                string filePath = Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, absolutepath[1..]);

                string apiPath = Path.Combine(HTTPSServerConfiguration.APIStaticFolder, absolutepath[1..]);

                if ((absolutepath == "/" || absolutepath == "\\") && ctx.Request.Method.ToString() == "GET")
                {
                    bool handled = false;

                    foreach (string indexFile in HTTPUtils.DefaultDocuments)
                    {
                        if (File.Exists(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile))
                        {
                            handled = true;

                            string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");

                            if (indexFile.EndsWith(".php") && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder))
                            {
                                var CollectPHP = Extensions.PHP.ProcessPHPPage(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile, HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx);
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
                                    ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile).ToString("r"));
                                    ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "text/html";
                                    sent = await ctx.Response.Send(HTTPUtils.Compress(CollectPHP.Item1));
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
                                    ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile).ToString("r"));
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "text/html";
                                    sent = await ctx.Response.Send(CollectPHP.Item1);
                                }
                            }
                            else
                            {
                                using FileStream stream = new(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
                                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile).ToString("r"));
                                        ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile));
                                        sent = await ctx.Response.Send(HTTPUtils.Compress(buffer));
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.OK;
                                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile).ToString("r"));
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(HTTPSServerConfiguration.HTTPSStaticFolder + indexFile));
                                        sent = await ctx.Response.Send(buffer);
                                    }
                                }
                                else
                                {
                                    statusCode = HttpStatusCode.InternalServerError;
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "text/plain";
                                    sent = await ctx.Response.Send();
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
                        sent = await ctx.Response.Send();
                    }
                }
                else if ((Host == "away.veemee.com" || Host == "home.veemee.com") && absolutepath.EndsWith(".php"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a VEEMEE method : {absolutepath}");

                    (string?, string?) res = new VEEMEEClass(ctx.Request.Method.ToString(), absolutepath).ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType, HTTPSServerConfiguration.APIStaticFolder);
                    if (string.IsNullOrEmpty(res.Item1))
                        statusCode = HttpStatusCode.InternalServerError;
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    if (!string.IsNullOrEmpty(res.Item2))
                        ctx.Response.ContentType = res.Item2;
                    else
                        ctx.Response.ContentType = "text/plain";
                    sent = await ctx.Response.Send(res.Item1);
                }
                else if (Host == "pshome.ndreams.net" && absolutepath.EndsWith(".php"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a NDREAMS method : {absolutepath}");

                    string? res = new NDREAMSClass(ctx.Request.Method.ToString(), absolutepath).ProcessRequest(null, ctx.Request.DataAsBytes, ctx.Request.ContentType);
                    if (string.IsNullOrEmpty(res))
                    {
                        ctx.Response.ContentType = "text/plain";
                        statusCode = HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.ContentType = "text/xml";
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    sent = await ctx.Response.Send(res);
                }
                else if (Host == "game2.hellfiregames.com" && absolutepath.EndsWith(".php"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a HELLFIRE method : {absolutepath}");

                    string? res = new HELLFIREClass(ctx.Request.Method.ToString(), HTTPUtils.RemoveQueryString(absolutepath)).ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType);
                    if (string.IsNullOrEmpty(res))
                        statusCode = HttpStatusCode.InternalServerError;
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    ctx.Response.ContentType = "text/plain";
                    sent = await ctx.Response.Send(res);
                }
                else if ((Host == "stats.outso-srv1.com" || Host == "www.outso-srv1.com") && absolutepath.EndsWith("/") && (absolutepath.Contains("/ohs") || absolutepath.Contains("/statistic/")))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a OHS method : {absolutepath}");

                    string? res = new OHSClass(ctx.Request.Method.ToString(), absolutepath, 0).ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType, apiPath);
                    if (string.IsNullOrEmpty(res))
                    {
                        ctx.Response.ContentType = "text/plain";
                        statusCode = HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        res = $"<ohs>{res}</ohs>";
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.ContentType = "application/xml;charset=UTF-8";
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    sent = await ctx.Response.Send(res);
                }
                else if ((Host == "test.playstationhome.jp" || Host == "playstationhome.jp" || Host == "scej-home.playstation.net" || Host == "homeec.scej-nbs.jp") && ctx.Request.ContentType.StartsWith("multipart/form-data") && absolutepath.Contains("/eventController/") && absolutepath.EndsWith(".do"))
                {
                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a PREMIUMAGENCY method : {absolutepath}");

                    string? res = new PREMIUMAGENCYClass(ctx.Request.Method.ToString(), absolutepath, HTTPSServerConfiguration.APIStaticFolder).ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType);
                    if (string.IsNullOrEmpty(res))
                    {
                        ctx.Response.ContentType = "text/plain";
                        statusCode = HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.ContentType = "text/xml";
                        statusCode = HttpStatusCode.OK;
                    }
                    ctx.Response.StatusCode = (int)statusCode;
                    sent = await ctx.Response.Send(res);
                }
                else
                {
                    string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");

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
                                        sent = await ctx.Response.Send("Bad Request");
                                    }
                                    else
                                    {
                                        string? dnsRequestBase64Url = ctx.Request.Query.Elements["dns"];
                                        if (string.IsNullOrEmpty(dnsRequestBase64Url))
                                        {
                                            statusCode = HttpStatusCode.BadRequest;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send("Bad Request");
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


                                                if (fullname.EndsWith("in-addr.arpa") && IPAddress.TryParse(fullname[..^13], out IPAddress? arparuleaddr)) // IPV4 Only.
                                                {
                                                    if (arparuleaddr != null)
                                                    {
                                                        if (arparuleaddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                                        {
                                                            // Split the IP address into octets
                                                            string[] octets = arparuleaddr.ToString().Split('.');

                                                            // Reverse the order of octets
                                                            Array.Reverse(octets);

                                                            // Join the octets back together
                                                            url = string.Join(".", octets);

                                                            treated = true;
                                                        }
                                                    }
                                                }
                                                else
                                                {
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
                                                }

                                                if (!treated && HTTPSServerConfiguration.DNSAllowUnsafeRequests)
                                                    url = VariousUtils.GetFirstActiveIPAddress(fullname, VariousUtils.GetPublicIPAddress(true));

                                                IPAddress ip = IPAddress.None; // NXDOMAIN
                                                if (url != string.Empty && url != "NXDOMAIN")
                                                {
                                                    try
                                                    {
                                                        if (!IPAddress.TryParse(url, out IPAddress? address))
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
                                                    sent = await ctx.Response.Send(DnsReq);
                                                }
                                                else
                                                {
                                                    statusCode = HttpStatusCode.InternalServerError;
                                                    ctx.Response.StatusCode = (int)statusCode;
                                                    ctx.Response.ContentType = "text/plain";
                                                    sent = await ctx.Response.Send();
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                statusCode = HttpStatusCode.InternalServerError;
                                                ctx.Response.StatusCode = (int)statusCode;
                                                ctx.Response.ContentType = "text/plain";
                                                sent = await ctx.Response.Send();
                                            }
                                        }
                                    }
                                    break;
                                case "/publisher/list/":
                                    LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a HOMECORE method : {absolutepath}");

                                    string? res = new HOMECOREClass(ctx.Request.Method.ToString(), absolutepath).ProcessRequest(ctx.Request.DataAsBytes, ctx.Request.ContentType, HTTPSServerConfiguration.APIStaticFolder);
                                    if (string.IsNullOrEmpty(res))
                                    {
                                        ctx.Response.ContentType = "text/plain";
                                        statusCode = HttpStatusCode.InternalServerError;
                                    }
                                    else
                                    {
                                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        ctx.Response.ContentType = "text/xml";
                                        statusCode = HttpStatusCode.OK;
                                    }
                                    ctx.Response.StatusCode = (int)statusCode;
                                    sent = await ctx.Response.Send(res);
                                    break;
                                case "/robots.txt": // Get Away Google.
                                    statusCode = HttpStatusCode.OK;
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "text/plain";
                                    sent = await ctx.Response.Send("User-agent: *\nDisallow: / ");
                                    break;
                                case "/!player":
                                case "/!player/":
                                    // We want to check if the router allows external IPs first.
                                    string ServerIP = VariousUtils.GetPublicIPAddress(true);
                                    try
                                    {
                                        using TcpClient client = new(ServerIP, ctx.Request.Destination.Port);
                                        client.Close();
                                    }
                                    catch (Exception) // Failed to connect, so we fallback to local IP.
                                    {
                                        ServerIP = VariousUtils.GetLocalIPAddress(true).ToString();
                                    }
                                    if (ServerIP.Length > 15)
                                        ServerIP = "[" + ServerIP + "]"; // Format the hostname if it's a IPV6 url format.
                                    WebVideoPlayer? WebPlayer = new(ctx.Request.Query.Elements, $"http://{ServerIP}/!webvideo/?");
                                    statusCode = HttpStatusCode.OK;
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "text/html";
                                    foreach (string[] HeaderCollection in WebPlayer.HeadersToSet)
                                    {
                                        ctx.Response.Headers.Add(HeaderCollection[0], HeaderCollection[1]);
                                    }
                                    sent = await ctx.Response.Send(WebPlayer.HtmlPage);
                                    WebPlayer = null;
                                    break;
                                case "/!GetMediaList":
                                case "/!GetMediaList/":
                                    statusCode = HttpStatusCode.OK;
                                    ctx.Response.StatusCode = (int)statusCode;
                                    ctx.Response.ContentType = "application/json";
                                    if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                    {
                                        ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                        sent = await ctx.Response.Send(HTTPUtils.Compress(Encoding.UTF8.GetBytes(FileStructureToJson.GetMediaFilesAsJson(HTTPSServerConfiguration.HTTPSStaticFolder, "mp4"))));
                                    }
                                    else
                                        sent = await ctx.Response.Send(FileStructureToJson.GetMediaFilesAsJson(HTTPSServerConfiguration.HTTPSStaticFolder, "mp4"));
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
                                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "application/json;charset=UTF-8";
                                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                        {
                                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                            sent = await ctx.Response.Send(HTTPUtils.Compress(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(devices, Formatting.Indented))));
                                        }
                                        else
                                            sent = await ctx.Response.Send(JsonConvert.SerializeObject(devices, Formatting.Indented));
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.Forbidden;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
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
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                            ctx.Response.StatusCode = (int)statusCode;
                                            DLNADevice Device = new(src);
                                            if (Device.IsConnected())
                                                sent = await ctx.Response.Send($"DLNA Player returned {Device.TryToPlayFile(dst)}");
                                            else
                                                sent = await ctx.Response.Send("Failed to send to TV");
                                        }
                                        else
                                        {
                                            statusCode = HttpStatusCode.Forbidden;
                                            ctx.Response.StatusCode = (int)statusCode;
											ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send();
                                        }
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.Forbidden;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
                                    }
                                    break;
                                default:
                                    if (Directory.Exists(filePath) && filePath.EndsWith("/"))
                                    {
                                        if (ctx.Request.RetrieveQueryValue("directory") == "on")
                                        {
                                            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                            {
                                                byte[]? buffer = HTTPUtils.Compress(Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(filePath[..^1], $"https://example.com{absolutepath[..^1]}")));

                                                if (buffer != null)
                                                {
                                                    statusCode = HttpStatusCode.OK;
                                                    ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                    ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                                    ctx.Response.StatusCode = (int)statusCode;
                                                    ctx.Response.ContentType = "application/json";
                                                    sent = await ctx.Response.Send(buffer);
                                                }
                                                else
                                                {
                                                    statusCode = HttpStatusCode.InternalServerError;
                                                    ctx.Response.StatusCode = (int)statusCode;
                                                    ctx.Response.ContentType = "text/plain";
                                                    sent = await ctx.Response.Send();
                                                }
                                            }
                                            else
                                            {
                                                statusCode = HttpStatusCode.OK;
                                                ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                ctx.Response.StatusCode = (int)statusCode;
                                                ctx.Response.ContentType = "application/json";
                                                sent = await ctx.Response.Send(FileStructureToJson.GetFileStructureAsJson(filePath[..^1], $"https://example.com{absolutepath[..^1]}"));
                                            }
                                        }
                                        else
                                        {
                                            bool handled = false;

                                            foreach (string indexFile in HTTPUtils.DefaultDocuments)
                                            {
                                                if (File.Exists(filePath + indexFile))
                                                {
                                                    handled = true;

                                                    if (indexFile.EndsWith(".php") && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder))
                                                    {
                                                        var CollectPHP = Extensions.PHP.ProcessPHPPage(filePath + indexFile, HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx);
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
                                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath + indexFile).ToString("r"));
                                                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                                            ctx.Response.StatusCode = (int)statusCode;
                                                            ctx.Response.ContentType = "text/html";
                                                            sent = await ctx.Response.Send(HTTPUtils.Compress(CollectPHP.Item1));
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
                                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath + indexFile).ToString("r"));
                                                            ctx.Response.StatusCode = (int)statusCode;
                                                            ctx.Response.ContentType = "text/html";
                                                            sent = await ctx.Response.Send(CollectPHP.Item1);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        using FileStream stream = new(filePath + indexFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
                                                                ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath + indexFile).ToString("r"));
                                                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                                                ctx.Response.StatusCode = (int)statusCode;
                                                                ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath + indexFile));
                                                                sent = await ctx.Response.Send(HTTPUtils.Compress(buffer));
                                                            }
                                                            else
                                                            {
                                                                statusCode = HttpStatusCode.OK;
                                                                ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                                ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath + indexFile).ToString("r"));
                                                                ctx.Response.StatusCode = (int)statusCode;
                                                                ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath + indexFile));
                                                                sent = await ctx.Response.Send(buffer);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            statusCode = HttpStatusCode.InternalServerError;
                                                            ctx.Response.StatusCode = (int)statusCode;
                                                            ctx.Response.ContentType = "text/plain";
                                                            sent = await ctx.Response.Send();
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
                                                sent = await ctx.Response.Send();
                                            }
                                        }
                                    }
                                    else if (absolutepath.ToLower().EndsWith(".php") && !string.IsNullOrEmpty(HTTPSServerConfiguration.PHPRedirectUrl))
                                    {
                                        statusCode = HttpStatusCode.PermanentRedirect;
                                        ctx.Response.Headers.Add("Location", $"{HTTPSServerConfiguration.PHPRedirectUrl}{ctx.Request.Url.RawWithQuery}");
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/html";
                                        sent = await ctx.Response.Send();
                                    }
                                    else if (absolutepath.ToLower().EndsWith(".php") && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                    {
                                        var CollectPHP = Extensions.PHP.ProcessPHPPage(filePath, HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx);
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
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/html";
                                            sent = await ctx.Response.Send(HTTPUtils.Compress(CollectPHP.Item1));
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
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/html";
                                            sent = await ctx.Response.Send(CollectPHP.Item1);
                                        }
                                    }
                                    else
                                    {
                                        if (File.Exists(filePath) && !string.IsNullOrEmpty(ctx.Request.RetrieveHeaderValue("Range"))) // Mmm, is it possible to have more?
                                            sent = new LocalFileStreamHelper().Handle_LocalFile_Stream(ctx, filePath);
                                        else
                                        {
                                            // send file
                                            if (File.Exists(filePath))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a file : {absolutepath}");

                                                string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                                if (ContentType == "application/octet-stream")
                                                {
                                                    byte[] VerificationChunck = VariousUtils.ReadSmallFileChunck(filePath, 10);
                                                    foreach (var entry in HTTPUtils.PathernDictionary)
                                                    {
                                                        if (VariousUtils.FindbyteSequence(VerificationChunck, entry.Value))
                                                        {
                                                            ContentType = entry.Key;
                                                            break;
                                                        }
                                                    }
                                                }
                                                sent = SendFile(ctx, filePath, ContentType);
                                            }
                                            else
                                            {
                                                LoggerAccessor.LogWarn($"[HTTPS] - {clientip} Requested a non-existant file : {filePath}");
                                                statusCode = HttpStatusCode.NotFound;
                                                ctx.Response.StatusCode = (int)statusCode;
                                                ctx.Response.ContentType = "text/plain";
                                                sent = await ctx.Response.Send();
                                            }
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
                                        sent = await ctx.Response.Send("Unsupported Media Type");
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


                                            if (fullname.EndsWith("in-addr.arpa") && IPAddress.TryParse(fullname[..^13], out IPAddress? arparuleaddr)) // IPV4 Only.
                                            {
                                                if (arparuleaddr != null)
                                                {
                                                    if (arparuleaddr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                                    {
                                                        // Split the IP address into octets
                                                        string[] octets = arparuleaddr.ToString().Split('.');

                                                        // Reverse the order of octets
                                                        Array.Reverse(octets);

                                                        // Join the octets back together
                                                        url = string.Join(".", octets);

                                                        treated = true;
                                                    }
                                                }
                                            }
                                            else
                                            {
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
                                            }

                                            if (!treated && HTTPSServerConfiguration.DNSAllowUnsafeRequests)
                                                url = VariousUtils.GetFirstActiveIPAddress(fullname, VariousUtils.GetPublicIPAddress(true));

                                            IPAddress ip = IPAddress.None; // NXDOMAIN
                                            if (url != string.Empty && url != "NXDOMAIN")
                                            {
                                                try
                                                {
                                                    if (!IPAddress.TryParse(url, out IPAddress? address))
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
                                                sent = await ctx.Response.Send(DnsReq);
                                            }
                                            else
                                            {
                                                statusCode = HttpStatusCode.InternalServerError;
                                                ctx.Response.StatusCode = (int)statusCode;
                                                ctx.Response.ContentType = "text/plain";
                                                sent = await ctx.Response.Send();
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send();
                                        }
                                    }
                                    break;
                                case "/!HomeTools/MakeBarSdat/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var makeres = HomeToolsInterface.MakeBarSdat(new MemoryStream(ctx.Request.DataAsBytes), ctx.Request.ContentType);
                                        if (makeres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={makeres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            sent = await ctx.Response.Send(makeres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send();
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
                                    }
                                    break;
                                case "/!HomeTools/UnBar/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var unbarres = await HomeToolsInterface.UnBar(new MemoryStream(ctx.Request.DataAsBytes), ctx.Request.ContentType, HTTPSServerConfiguration.HomeToolsHelperStaticFolder);
                                        if (unbarres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={unbarres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            sent = await ctx.Response.Send(unbarres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send();
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
                                    }
                                    break;
                                case "/!HomeTools/CDS/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var cdsres = HomeToolsInterface.CDS(new MemoryStream(ctx.Request.DataAsBytes), ctx.Request.ContentType);
                                        if (cdsres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={cdsres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            sent = await ctx.Response.Send(cdsres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send();
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
                                    }
                                    break;
                                case "/!HomeTools/CDSBruteforce/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var cdsres = HomeToolsInterface.CDSBruteforce(new MemoryStream(ctx.Request.DataAsBytes), ctx.Request.ContentType);
                                        if (cdsres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={cdsres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            sent = await ctx.Response.Send(cdsres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send();
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
                                    }
                                    break;
                                case "/!HomeTools/HCDBUnpack/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var cdsres = HomeToolsInterface.HCDBUnpack(new MemoryStream(ctx.Request.DataAsBytes), ctx.Request.ContentType);
                                        if (cdsres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={cdsres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            sent = await ctx.Response.Send(cdsres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send();
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
                                    }
                                    break;
                                case "/!HomeTools/TicketList/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var ticketlistres = HomeToolsInterface.TicketList(new MemoryStream(ctx.Request.DataAsBytes), ctx.Request.ContentType);
                                        if (ticketlistres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={ticketlistres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            sent = await ctx.Response.Send(ticketlistres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send();
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
                                    }
                                    break;
                                case "/!HomeTools/INF/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        var infres = HomeToolsInterface.INF(new MemoryStream(ctx.Request.DataAsBytes), ctx.Request.ContentType);
                                        if (infres != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("Content-disposition", $"attachment; filename={infres.Value.Item2}");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = HTTPUtils.GetMimeType(Path.GetExtension(filePath));
                                            sent = await ctx.Response.Send(infres.Value.Item1);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send();
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
                                    }
                                    break;
                                case "/!HomeTools/ChannelID/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        string? channelres = HomeToolsInterface.ChannelID(new MemoryStream(ctx.Request.DataAsBytes), ctx.Request.ContentType);
                                        if (!string.IsNullOrEmpty(channelres))
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send(channelres);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send();
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
                                    }
                                    break;
                                case "/!HomeTools/SceneID/":
                                    if (IsIPAllowed(clientip))
                                    {
                                        string? sceneres = HomeToolsInterface.SceneID(new MemoryStream(ctx.Request.DataAsBytes), ctx.Request.ContentType);
                                        if (!string.IsNullOrEmpty(sceneres))
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send(sceneres);
                                        }
                                        else // We are vague on the status code.
                                        {
                                            statusCode = HttpStatusCode.InternalServerError;
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/plain";
                                            sent = await ctx.Response.Send();
                                        }
                                    }
                                    else // We are vague on the status code.
                                    {
                                        statusCode = HttpStatusCode.InternalServerError;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
                                    }
                                    break;
                                default:
                                    if (absolutepath.ToLower().EndsWith(".php") && !string.IsNullOrEmpty(HTTPSServerConfiguration.PHPRedirectUrl))
                                    {
                                        statusCode = HttpStatusCode.PermanentRedirect;
                                        ctx.Response.Headers.Add("Location", $"{HTTPSServerConfiguration.PHPRedirectUrl}{ctx.Request.Url.RawWithQuery}");
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/html";
                                        sent = await ctx.Response.Send();
                                    }
                                    else if (absolutepath.ToLower().EndsWith(".php") && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                    {
                                        (byte[]?, string[][]) CollectPHP = Extensions.PHP.ProcessPHPPage(filePath, HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx);
                                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && CollectPHP.Item1 != null)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            if (CollectPHP.Item2 != null)
                                            {
                                                foreach (string[] innerArray in CollectPHP.Item2)
                                                {
                                                    // Ensure the inner array has at least two elements
                                                    if (innerArray.Length >= 2)
                                                        ctx.Response.Headers.Add(innerArray[0], innerArray[1]);
                                                }
                                            }
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/html";
                                            sent = await ctx.Response.Send(HTTPUtils.Compress(CollectPHP.Item1));
                                        }
                                        else
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            if (CollectPHP.Item2 != null)
                                            {
                                                foreach (string[] innerArray in CollectPHP.Item2)
                                                {
                                                    // Ensure the inner array has at least two elements
                                                    if (innerArray.Length >= 2)
                                                        // Extract two values from the inner array
                                                        ctx.Response.Headers.Add(innerArray[0], innerArray[1]);
                                                }
                                            }
                                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                            ctx.Response.StatusCode = (int)statusCode;
                                            ctx.Response.ContentType = "text/html";
                                            sent = await ctx.Response.Send(CollectPHP.Item1);
                                        }
                                    }
                                    else
                                    {
                                        statusCode = HttpStatusCode.NotFound;
                                        ctx.Response.StatusCode = (int)statusCode;
                                        ctx.Response.ContentType = "text/plain";
                                        sent = await ctx.Response.Send();
                                    }
                                    break;
                            }
                            break;
                        case "PUT":
                            if (HTTPSServerConfiguration.EnablePUTMethod)
                            {
                                string ContentType = ctx.Request.ContentType;
                                byte[] PostData = ctx.Request.DataAsBytes;
                                if (PostData != Array.Empty<byte>() && !string.IsNullOrEmpty(ContentType))
                                {
                                    string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                                    if (!string.IsNullOrEmpty(boundary))
                                    {
                                        string UploadDirectoryPath = HTTPSServerConfiguration.HTTPSTempFolder + $"/DataUpload/{absolutepath[1..]}";
                                        Directory.CreateDirectory(UploadDirectoryPath);
                                        var data = MultipartFormDataParser.Parse(new MemoryStream(PostData), boundary);
                                        foreach (FilePart? multipartfile in data.Files)
                                        {
                                            if (multipartfile.Data.Length > 0)
                                            {
                                                using Stream filedata = multipartfile.Data;
                                                int copyNumber = 0;
                                                string UploadFilePath = UploadDirectoryPath + $"/{multipartfile.FileName}";

                                                while (File.Exists(UploadFilePath))
                                                {
                                                    copyNumber++;
                                                    UploadFilePath = Path.Combine(UploadDirectoryPath,
                                                        $"{Path.GetFileNameWithoutExtension(multipartfile.FileName)} (Copy {copyNumber}){Path.GetExtension(multipartfile.FileName)}");
                                                }

                                                using (FileStream fileStream = File.Create(UploadFilePath))
                                                {
                                                    filedata.Seek(0, SeekOrigin.Begin);
                                                    filedata.CopyTo(fileStream);
                                                }

                                                filedata.Flush();
                                            }
                                        }

                                        statusCode = HttpStatusCode.OK;
                                    }
                                    else
                                        statusCode = HttpStatusCode.Forbidden;
                                }
                                else
                                    statusCode = HttpStatusCode.Forbidden;
                            }
                            else
                                statusCode = HttpStatusCode.Forbidden;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            sent = await ctx.Response.Send();
                            break;
                        case "DELETE":
                            statusCode = HttpStatusCode.Forbidden;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            sent = await ctx.Response.Send();
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
                                    byte[] VerificationChunck = VariousUtils.ReadSmallFileChunck(filePath, 10);
                                    foreach (var entry in HTTPUtils.PathernDictionary)
                                    {
                                        if (VariousUtils.FindbyteSequence(VerificationChunck, entry.Value))
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
                            sent = await ctx.Response.Send();
                            break;
                        case "OPTIONS":
                            statusCode = HttpStatusCode.OK;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            ctx.Response.Headers.Set("Allow", "OPTIONS, GET, HEAD, POST");
                            sent = await ctx.Response.Send();
                            break;
                        case "PROPFIND":
                            statusCode = HttpStatusCode.NotImplemented;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            sent = await ctx.Response.Send();
                            break;
                        default:
                            statusCode = HttpStatusCode.Forbidden;
                            ctx.Response.StatusCode = (int)statusCode;
                            ctx.Response.ContentType = "text/plain";
                            sent = await ctx.Response.Send();
                            break;
                    }
                }
            }
            else
            {
                ctx.Response.StatusCode = (int)statusCode; // Send the other status.
                ctx.Response.ContentType = "text/plain";
                sent = await ctx.Response.Send();
            }

#if DEBUG
            if (!sent)
                LoggerAccessor.LogWarn($"[HTTPS] - {clientip}:{clientport} Failed to receive the response! Client might have closed the wire.");
#endif
        }

        private static bool SendFile(HttpContextBase ctx, string filePath, string contentType)
        {
            bool sent = false;

            long contentLen = new FileInfo(filePath).Length;

            string? acceptencoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");

            if (!string.IsNullOrEmpty(acceptencoding) && acceptencoding.Contains("deflate") && contentLen <= 80000000) // We must be reasonable on the file-size here (80 Mb).
            {
                using Stream st = HTTPUtils.InflateStream(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                ctx.Response.Headers.Add("Content-Encoding", "deflate");
                ctx.Response.ContentType = contentType;
                ctx.Response.StatusCode = 200;
                sent = ctx.Response.Send(st.Length, st).Result;

                st.Flush();
                st.Close();
            }
            else
            {
                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                ctx.Response.ContentType = contentType;
                ctx.Response.StatusCode = 200;
                sent = ctx.Response.Send(contentLen, fs).Result;

                fs.Flush();
                fs.Close();
            }

            return sent;
        }

        private static void ParseRules(string Filename, bool IsFilename = true)
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
                                    dns.Address = VariousUtils.GetLocalIPAddress().ToString();
                                break;
                            default:
                                LoggerAccessor.LogWarn($"[HTTPS_DNS] - Rule : {s} is not a formated properly, skipping...");
                                break;
                        }

                        string domain = split[0].Trim();

                        // Check if the domain has been processed before
                        if (!processedDomains.Contains(domain))
                        {
                            processedDomains.Add(domain);

                            if (domain.Contains('*'))
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

        private static Dictionary<string, DnsSettings> ParseSimpleDNSRules(string Filename, Dictionary<string, DnsSettings> DicRules)
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
                    // Add the hostname to the list
                    hostnames.Add(parts[1].Trim());
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
                                    dns.Address = VariousUtils.GetLocalIPAddress().ToString();
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
            public string? Address; // For redirect to
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
