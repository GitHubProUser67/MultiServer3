using System.Collections.Specialized;
using System.Text;
using CyberBackendLibrary.HTTP;
using CyberBackendLibrary.DNS;
using CyberBackendLibrary.GeoLocalization;
using WebAPIService.LOOT;
using WebAPIService.NDREAMS;
using WebAPIService.OHS;
using WebAPIService.PREMIUMAGENCY;
using WebAPIService.HELLFIRE;
using WebAPIService.VEEMEE;
using CustomLogger;
using HttpMultipartParser;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;
using WebAPIService.UBISOFT.HERMES_API;
using WebAPIService.CAPONE;
using WebAPIService.CDM;
using WebAPIService.MultiMedia;
using CyberBackendLibrary.DataTypes;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CyberBackendLibrary.FileSystem;
using CyberBackendLibrary.HTTP.PluginManager;
using Newtonsoft.Json;
using System.Security.Authentication;

namespace HTTPSecureServerLite
{
    public partial class HttpsProcessor
    {
        private static string serverIP = "127.0.0.1";
        private WebserverLite? _Server;
        private readonly string ip;
        private readonly ushort port;
        private readonly SslProtocols protocols;

        public HttpsProcessor(string certpath, string certpass, string ip, ushort port, SslProtocols protocols)
        {
            this.ip = ip;
            this.port = port;
            this.protocols = protocols;
            WebserverSettings settings = new()
            {
                Hostname = ip,
                Port = port,
            };
            settings.Ssl.PfxCertificateFile = certpath;
            settings.Ssl.PfxCertificatePassword = certpass;
            settings.Ssl.Enable = true;
            _Server = new WebserverLite(settings, DefaultRoute);

            StartServer();
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

        public Task TryGetServerIP(ushort Port)
        {
            // We want to check if the router allows external IPs first.
            string ServerIP = CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress(true);
            try
            {
                using TcpClient client = new(ServerIP, Port);
                client.Close();
            }
            catch // Failed to connect to public ip, so we fallback to local IP.
            {
                ServerIP = CyberBackendLibrary.TCP_IP.IPUtils.GetLocalIPAddress(true).ToString();

                try
                {
                    using TcpClient client = new(ServerIP, Port);
                    client.Close();
                }
                catch // Failed to connect to local ip, trying IPV4 only as a last resort.
                {
                    ServerIP = CyberBackendLibrary.TCP_IP.IPUtils.GetLocalIPAddress(false).ToString();
                }
            }

            serverIP = ServerIP;

            return Task.CompletedTask;
        }

        public void StopServer()
        {
            _Server?.Stop();
            _Server?.Dispose();

            LoggerAccessor.LogWarn($"HTTPS Server on port: {port} stopped...");
        }

        public void StartServer()
        {
			_ = TryGetServerIP(port);

            if (_Server != null && !_Server.IsListening)
            {
                _Server.Routes.AuthenticateRequest = AuthorizeConnection;
                _Server.Events.Logger = LoggerAccessor.LogInfo;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;

                _Server.Routes.PostAuthentication.Parameter.Add(HttpMethod.GET, "/objects/D2CDD8B2-DE444593-A64C68CB-0B5EDE23/{id}.xml", async (ctx) =>
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

                _Server.Start(protocols);
                LoggerAccessor.LogInfo($"HTTPS Server initiated on port: {port}...");
            }
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            try
            {
                HttpRequestBase request = ctx.Request;
                DateTime CurrentDate = request.Timestamp.Start;
                HttpResponseBase response = ctx.Response;
                HttpStatusCode statusCode = HttpStatusCode.Forbidden;
                string fullurl = string.Empty;
                string absolutepath = string.Empty;
                string fulluripath = string.Empty;
                string Host = request.RetrieveHeaderValue("Host");
                if (string.IsNullOrEmpty(Host))
                    Host = request.RetrieveHeaderValue("HOST");
                string Accept = request.RetrieveHeaderValue("Accept");
                string clientip = request.Source.IpAddress;
                string clientport = request.Source.Port.ToString();
                string ServerIP = request.Destination.IpAddress;
                if (string.IsNullOrEmpty(ServerIP))
                    ServerIP = serverIP;
                int ServerPort = request.Destination.Port;
                if (ServerPort == 0)
                    ServerPort = 443; // Happens for some reasons (API bug?)
                bool sent = false;

                try
                {
                    if (!string.IsNullOrEmpty(request.Url.RawWithQuery))
                    {
                        if (!string.IsNullOrEmpty(request.Useragent) && request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
                            LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested the HTTPS Server with a ByteDance crawler!");
                        else
                        {
                            fullurl = HTTPProcessor.DecodeUrl(request.Url.RawWithQuery);

                            string SuplementalMessage = string.Empty;
                            string? GeoCodeString = GeoIP.GetGeoCodeFromIP(IPAddress.Parse(clientip));

                            if (!string.IsNullOrEmpty(GeoCodeString))
                            {
                                // Split the input string by the '-' character
                                string[] parts = GeoCodeString.Split('-');

                                // Check if there are exactly two parts
                                if (parts.Length == 2)
                                {
                                    string CountryCode = parts[0];

                                    SuplementalMessage = " Located at " + CountryCode + (bool.Parse(parts[1]) ? " Situated in Europe " : string.Empty);

                                    if (HTTPSServerConfiguration.DateTimeOffset != null && HTTPSServerConfiguration.DateTimeOffset.ContainsKey(CountryCode))
                                        CurrentDate = CurrentDate.AddDays(HTTPSServerConfiguration.DateTimeOffset[CountryCode]);
                                    else if (HTTPSServerConfiguration.DateTimeOffset != null && HTTPSServerConfiguration.DateTimeOffset.ContainsKey(string.Empty))
                                        CurrentDate = CurrentDate.AddDays(HTTPSServerConfiguration.DateTimeOffset.Where(entry => entry.Key == string.Empty).FirstOrDefault().Value);
                                }
                            }
                            else if (HTTPSServerConfiguration.DateTimeOffset != null && HTTPSServerConfiguration.DateTimeOffset.ContainsKey(string.Empty))
                                CurrentDate = CurrentDate.AddDays(HTTPSServerConfiguration.DateTimeOffset.Where(entry => entry.Key == string.Empty).FirstOrDefault().Value);

#if DEBUG
                            LoggerAccessor.LogJson(JsonConvert.SerializeObject(new
                            {
                                HttpMethod = request.Method,
                                Url = request.Url.Full,
                                Headers = request.Headers,
                                HeadersValues = ctx.Request.Headers.AllKeys.SelectMany(key => ctx.Request.Headers.GetValues(key) ?? Enumerable.Empty<string>()),
                                UserAgent = request.Useragent,
                                ClientAddress = request.Source.IpAddress + ":" + request.Source.Port,
                            }), $"[[HTTPS]] - {clientip}:{clientport}{SuplementalMessage} Requested the HTTPS Server with URL : {fullurl}" + " (" + ctx.Timestamp.TotalMs + "ms)");
#else
                        LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport}{SuplementalMessage} Requested the HTTPS Server with URL : {fullurl}" + " (" + ctx.Timestamp.TotalMs + "ms)");
#endif

                            absolutepath = HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + HTTPProcessor.RemoveQueryString(fullurl);
                            fulluripath = HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + fullurl;
                            statusCode = HttpStatusCode.Continue;
                        }
                    }
                    else
                        LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested the HTTPS Server with invalid parameters!");
                }
                catch
                {

                }

                #region Domains

                List<string> HPDDomains = new() {
                                    "prd.destinations.scea.com",
                                    "pre.destinations.scea.com",
                                    "qa.destinations.scea.com",
                                    "dev.destinations.scea.com",
                                    "holdemeu.destinations.scea.com",
                                    "holdemna.destinations.scea.com",
                                    "c93f2f1d-3946-4f37-b004-1196acf599c5.scalr.ws"
                                };

                List<string> CAPONEDomains = new() {
                                    "collector.gr.online.scea.com",
                                    "collector-nonprod.gr.online.scea.com",
                                    "collector-dev.gr.online.scea.com",
                                    "content.gr.online.scea.com",
                                    "content-nonprod.gr.online.scea.com",
                                    "content-dev.gr.online.scea.com",
                                };


                List<string> nDreamsDomains = new()
                                {
                                    "pshome.ndreams.net",
                                    "www.ndreamshs.com",
                                    "www.ndreamsportal.com",
                                    "nDreams-multiserver-cdn"
                                };

                #endregion

                if (statusCode == HttpStatusCode.Continue)
                {
                    if (HTTPSServerConfiguration.RedirectRules != null)
                    {
                        foreach (string? rule in HTTPSServerConfiguration.RedirectRules)
                        {
                            if (!string.IsNullOrEmpty(rule) && rule.StartsWith("Redirect") && rule.Length >= 9) // Redirect + whitespace is minimum 9 in length.
                            {
                                string RouteRule = rule[8..];

                                if (RouteRule.StartsWith("Match "))
                                {
#if NET6_0
                                Match match = new Regex(@"Match (\\d+) (.*) (.*)$").Match(RouteRule);
#elif NET7_0_OR_GREATER
                                    Match match = ApacheMatchRegex().Match(RouteRule);
#endif
                                    if (match.Success && match.Groups.Count == 3)
                                    {
                                        // Compare the regex rule against the test URL
                                        if (Regex.IsMatch(absolutepath, match.Groups[2].Value))
                                        {
                                            statusCode = (HttpStatusCode)int.Parse(match.Groups[1].Value);
                                            response.Headers.Add("Location", match.Groups[3].Value);
                                            response.StatusCode = (int)statusCode;
                                            response.ContentType = "text/plain";
                                            sent = await response.Send();
                                        }
                                    }
                                }
                                else if (RouteRule.StartsWith("Permanent "))
                                {
                                    string[] parts = RouteRule.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                    if (parts.Length == 3 && (parts[1] == "/" || parts[1] == absolutepath))
                                    {
                                        statusCode = HttpStatusCode.PermanentRedirect;
                                        response.Headers.Add("Location", parts[2]);
                                        response.StatusCode = (int)statusCode;
                                        response.ContentType = "text/plain";
                                        sent = await response.Send();
                                    }
                                }
                                else if (RouteRule.StartsWith(' '))
                                {
                                    string[] parts = RouteRule[1..].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                    if (parts.Length == 3 && (parts[1] == "/" || parts[1] == absolutepath))
                                    {
                                        // Check if the input string contains an HTTP method
#if NET6_0
                                    if (new Regex(@"^(GET|POST|PUT|DELETE|HEAD|OPTIONS|PATCH)").Match(parts[0]).Success && request.Method.ToString() == parts[0])
#elif NET7_0_OR_GREATER
                                        if (HttpMethodRegex().Match(parts[0]).Success && request.Method.ToString() == parts[0])
#endif
                                        {
                                            statusCode = HttpStatusCode.Found;
                                            response.Headers.Add("Location", parts[2]);
                                            response.StatusCode = (int)statusCode;
                                            response.ContentType = "text/plain";
                                            sent = await response.Send();
                                        }
                                        // Check if the input string contains a status code
#if NET6_0
                                    else if (new Regex(@"\\b\\d{3}\\b").Match(parts[0]).Success && int.TryParse(parts[0], out int statuscode))
#elif NET7_0_OR_GREATER
                                        else if (HttpStatusCodeRegex().Match(parts[0]).Success && int.TryParse(parts[0], out int statuscode))
#endif
                                        {
                                            statusCode = (HttpStatusCode)statuscode;
                                            response.Headers.Add("Location", parts[2]);
                                            response.StatusCode = (int)statusCode;
                                            response.ContentType = "text/plain";
                                            sent = await response.Send();
                                        }
                                        else if (parts[1] == "permanent")
                                        {
                                            statusCode = HttpStatusCode.PermanentRedirect;
                                            response.Headers.Add("Location", parts[2]);
                                            response.StatusCode = (int)statusCode;
                                            response.ContentType = "text/plain";
                                            sent = await response.Send();
                                        }
                                    }
                                    else if (parts.Length == 2 && (parts[0] == "/" || parts[0] == absolutepath))
                                    {
                                        statusCode = HttpStatusCode.Found;
                                        response.Headers.Add("Location", parts[1]);
                                        response.StatusCode = (int)statusCode;
                                        response.ContentType = "text/plain";
                                        sent = await response.Send();
                                    }
                                }
                            }
                        }
                    }

                    if (!sent)
                    {
                        if (HTTPSServerConfiguration.plugins.Count > 0)
                        {
                            foreach (HTTPPlugin plugin in HTTPSServerConfiguration.plugins.Values)
                            {
                                try
                                {
                                    object? objReturn = plugin.ProcessPluginMessage(ctx);
                                    if (objReturn != null && objReturn is bool v)
                                        sent = v;
                                    if (sent)
                                        break;
                                }
                                catch (Exception ex)
                                {
                                    LoggerAccessor.LogError($"[HTTPS] - Plugin {plugin.GetHashCode()} thrown an assertion: {ex}");
                                }
                            }
                        }

                        if (sent)
                        {

                        }
                        else
                        {
                            // Split the URL into segments
                            string[] segments = absolutepath.Trim('/').Split('/');

                            // Combine the folder segments into a directory path
                            string directoryPath = Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                            // Process the request based on the HTTP method
                            string filePath = Path.Combine(HTTPSServerConfiguration.HTTPSStaticFolder, absolutepath[1..]);

                            string apiPath = Path.Combine(HTTPSServerConfiguration.APIStaticFolder, absolutepath[1..]);

                            if ((absolutepath == "/" || absolutepath == "\\") && request.Method.ToString() == "GET")
                            {
                                bool handled = false;

                                foreach (string indexFile in HTTPProcessor._DefaultFiles)
                                {
                                    if (File.Exists(HTTPSServerConfiguration.HTTPSStaticFolder + $"/{indexFile}"))
                                    {
                                        handled = true;

                                        string? encoding = request.RetrieveHeaderValue("Accept-Encoding");

                                        if (indexFile.EndsWith(".php") && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder))
                                        {
                                            var CollectPHP = Extensions.PHP.ProcessPHPPage(HTTPSServerConfiguration.HTTPSStaticFolder + $"/{indexFile}", HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx);
                                            statusCode = HttpStatusCode.OK;
                                            if (CollectPHP.Item2 != null)
                                            {
                                                foreach (var innerArray in CollectPHP.Item2)
                                                {
                                                    // Ensure the inner array has at least two elements
                                                    if (innerArray.Length >= 2)
                                                        // Extract two values from the inner array
                                                        response.Headers.Add(innerArray[0], innerArray[1]);
                                                }
                                            }
                                            response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                            response.Headers.Add("Last-Modified", File.GetLastWriteTime(HTTPSServerConfiguration.HTTPSStaticFolder + $"/{indexFile}").ToString("r"));
                                            response.StatusCode = (int)statusCode;
                                            response.ContentType = "text/html";
                                            sent = await response.Send(CollectPHP.Item1);
                                        }
                                        else
                                        {
                                            using FileStream stream = new(HTTPSServerConfiguration.HTTPSStaticFolder + $"/{indexFile}", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                            byte[]? buffer = null;

                                            using (MemoryStream ms = new())
                                            {
                                                stream.CopyTo(ms);
                                                buffer = ms.ToArray();
                                                ms.Flush();
                                            }

                                            if (buffer != null)
                                            {
                                                statusCode = HttpStatusCode.OK;
                                                response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                response.Headers.Add("Last-Modified", File.GetLastWriteTime(HTTPSServerConfiguration.HTTPSStaticFolder + $"/{indexFile}").ToString("r"));
                                                response.StatusCode = (int)statusCode;
                                                response.ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(HTTPSServerConfiguration.HTTPSStaticFolder + $"/{indexFile}"), HTTPSServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                                                sent = await response.Send(buffer);
                                            }
                                            else
                                            {
                                                statusCode = HttpStatusCode.InternalServerError;
                                                response.StatusCode = (int)statusCode;
                                                response.ContentType = "text/plain";
                                                sent = await response.Send();
                                            }

                                            stream.Flush();
                                        }
                                        break;
                                    }
                                }

                                if (!handled)
                                {
                                    statusCode = HttpStatusCode.NotFound;
                                    response.StatusCode = (int)statusCode;

                                    if (!string.IsNullOrEmpty(Accept) && Accept.Contains("html"))
                                    {
                                        response.ContentType = "text/html";
                                        sent = await response.Send(await DefaultHTMLPages.GenerateNotFound(absolutepath, $"https://{(string.IsNullOrEmpty(Host) ? (ServerIP.Length > 15 ? "[" + ServerIP + "]" : ServerIP) : Host)}",
                                            HTTPSServerConfiguration.HTTPSStaticFolder, "Apache 2.2.22 (Unix) DAV/2", ServerPort.ToString(), HTTPSServerConfiguration.NotFoundSuggestions));
                                    }
                                    else
                                    {
                                        response.ContentType = "text/plain";
                                        sent = await response.Send();
                                    }
                                }
                            }

                            #region VEEMEE API
                            else if ((Host == "away.veemee.com" || Host == "home.veemee.com") && absolutepath.EndsWith(".php"))
                            {
                                LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a VEEMEE method : {absolutepath}");

                                (string?, string?) res = new VEEMEEClass(request.Method.ToString(), absolutepath).ProcessRequest(request.DataAsBytes, request.ContentType, HTTPSServerConfiguration.APIStaticFolder);
                                if (string.IsNullOrEmpty(res.Item1))
                                    statusCode = HttpStatusCode.InternalServerError;
                                else
                                {
                                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                    statusCode = HttpStatusCode.OK;
                                }
                                response.StatusCode = (int)statusCode;
                                if (!string.IsNullOrEmpty(res.Item2))
                                    response.ContentType = res.Item2;
                                else
                                    response.ContentType = "text/plain";
                                sent = await response.Send(res.Item1);
                            }
                            #endregion

                            #region nDreams API
                            else if (nDreamsDomains.Contains(Host)
                            && !string.IsNullOrEmpty(request.Method.ToString())
                            && (absolutepath.EndsWith(".php")
                            || absolutepath.Contains("/NDREAMS/")
                            || absolutepath.Contains("/gateway/")))
                            {
                                LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a NDREAMS method : {absolutepath}");
                                string? res = new NDREAMSClass(CurrentDate, request.Method.ToString(), apiPath, $"https://nDreams-multiserver-cdn/", $"https://{Host}{request.Url.RawWithQuery}", absolutepath,
                                    HTTPSServerConfiguration.APIStaticFolder, Host).ProcessRequest(null, request.DataAsBytes, request.ContentType);
                                if (string.IsNullOrEmpty(res))
                                {
                                    response.ContentType = "text/plain";
                                    statusCode = HttpStatusCode.InternalServerError;
                                }
                                else
                                {
                                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                    response.ContentType = "text/xml";
                                    statusCode = HttpStatusCode.OK;
                                }
                                response.StatusCode = (int)statusCode;
                                sent = await response.Send(res);
                            }
                            #endregion

                            #region Hellfire Games API
                            else if (Host == "game2.hellfiregames.com" && absolutepath.EndsWith(".php"))
                            {
                                LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a HELLFIRE method : {absolutepath}");

                                string? res = new HELLFIREClass(request.Method.ToString(), HTTPProcessor.RemoveQueryString(absolutepath), HTTPSServerConfiguration.APIStaticFolder).ProcessRequest(request.DataAsBytes, request.ContentType, true);
                                if (string.IsNullOrEmpty(res))
                                {
                                    response.ContentType = "text/plain";
                                    statusCode = HttpStatusCode.InternalServerError;
                                }
                                else
                                {
                                    response.ContentType = "application/xml;charset=UTF-8";
                                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                    statusCode = HttpStatusCode.OK;
                                }
                                response.StatusCode = (int)statusCode;
                                sent = await response.Send(res);
                            }
                            #endregion

                            #region Outso OHS API
                            else if ((Host == "stats.outso-srv1.com" || Host == "www.outso-srv1.com") &&
                                                        absolutepath.EndsWith("/") ||
                                                        absolutepath.Contains("/ohs_") ||
                                                        absolutepath.Contains("/ohs/") ||
                                                        absolutepath.Contains("/statistic/") ||
                                                        absolutepath.Contains("/Konami/"))
                            {
                                LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a OHS method : {absolutepath}");

                                string? res = new OHSClass(request.Method.ToString(), absolutepath, 0).ProcessRequest(request.DataAsBytes, request.ContentType, apiPath);
                                if (string.IsNullOrEmpty(res))
                                {
                                    response.ContentType = "text/plain";
                                    statusCode = HttpStatusCode.InternalServerError;
                                }
                                else
                                {
                                    res = $"<ohs>{res}</ohs>";
                                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                    response.ContentType = "application/xml;charset=UTF-8";
                                    statusCode = HttpStatusCode.OK;
                                }
                                response.StatusCode = (int)statusCode;
                                sent = await response.Send(res);
                            }
                            #endregion

                            #region LOOT API
                            else if (Host == "server.lootgear.com" || Host == "alpha.lootgear.com")
                            {
                                LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a LOOT method : {absolutepath}");

                                NameValueCollection QueryElements = request.Query.Elements;
                                Dictionary<string, string> QueryElementsList = new();

                                foreach (string? k in QueryElements.AllKeys)
                                {
                                    QueryElements.Add(k, QueryElements[k]);
                                }

                                string? res = new LOOTClass(request.Method.ToString(), absolutepath, apiPath).ProcessRequest(QueryElementsList, request.DataAsBytes, request.ContentType);
                                if (string.IsNullOrEmpty(res))
                                {
                                    response.ContentType = "text/plain";
                                    statusCode = HttpStatusCode.InternalServerError;
                                }
                                else
                                {
                                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                    response.ContentType = "application/xml;charset=UTF-8";
                                    statusCode = HttpStatusCode.OK;
                                }
                                response.StatusCode = (int)statusCode;
                                sent = await response.Send(res);
                            }
                            #endregion

                            #region PREMIUMAGENCY API
                            else if ((Host == "test.playstationhome.jp" ||
                                                        Host == "playstationhome.jp" ||
                                                        Host == "homeec.scej-nbs.jp" ||
                                                        Host == "homeecqa.scej-nbs.jp" ||
                                                        Host == "homect-scej.jp" ||
                                                        Host == "qa-homect-scej.jp")
                                                        && absolutepath.Contains("/eventController/"))
                            {
                                LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a PREMIUMAGENCY method : {absolutepath}");

                                string? res = new PREMIUMAGENCYClass(request.Method.ToString(), absolutepath, HTTPSServerConfiguration.APIStaticFolder, fullurl).ProcessRequest(request.DataAsBytes, request.ContentType);
                                if (string.IsNullOrEmpty(res))
                                {
                                    response.ContentType = "text/plain";
                                    statusCode = HttpStatusCode.InternalServerError;
                                }
                                else
                                {
                                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                    response.ContentType = "text/xml";
                                    statusCode = HttpStatusCode.OK;
                                }
                                response.StatusCode = (int)statusCode;
                                sent = await response.Send(res);
                            }
                            #endregion

                            #region UBISOFT API
                            else if (Host.Contains("api-ubiservices.ubi.com") && request.RetrieveHeaderValue("User-Agent").Contains("UbiServices_SDK_HTTP_Client"))
                            {
                                LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a UBISOFT method : {absolutepath}");

                                string Authorization = request.RetrieveHeaderValue("Authorization");

                                if (!string.IsNullOrEmpty(Authorization))
                                {
                                    // TODO, verify ticket data for every platforms.

                                    if (Authorization.StartsWith("psn t=") && DataTypesUtils.IsBase64String(Authorization))
                                    {
                                        byte[] PSNTicket = Convert.FromBase64String(Authorization.Replace("psn t=", string.Empty));

                                        // Extract the desired portion of the binary data
                                        byte[] extractedData = new byte[0x63 - 0x54 + 1];

                                        // Copy it
                                        Array.Copy(PSNTicket, 0x54, extractedData, 0, extractedData.Length);

                                        // Convert 0x00 bytes to 0x48 so FileSystem can support it
                                        for (int i = 0; i < extractedData.Length; i++)
                                        {
                                            if (extractedData[i] == 0x00)
                                                extractedData[i] = 0x48;
                                        }

                                        if (DataTypesUtils.FindBytePattern(PSNTicket, new byte[] { 0x52, 0x50, 0x43, 0x4E }) != -1)
                                            LoggerAccessor.LogInfo($"[HERMES] : User {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on RPCN");
                                        else
                                            LoggerAccessor.LogInfo($"[HERMES] : {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on PSN");
                                    }
                                    else if (Authorization.StartsWith("Ubi_v1 t="))
                                    {
                                        // Our JWT token is fake for now.
                                    }

                                    (string?, string?) res = new HERMESClass(request.Method.ToString(), absolutepath, request.RetrieveHeaderValue("Ubi-AppId"), request.RetrieveHeaderValue("Ubi-RequestedPlatformType"),
                                            request.RetrieveHeaderValue("ubi-appbuildid"), clientip, GeoIP.GetISOCodeFromIP(IPAddress.Parse(clientip)), Authorization.Replace("psn t=", string.Empty), HTTPSServerConfiguration.APIStaticFolder)
                                        .ProcessRequest(request.DataAsBytes, request.ContentType);
                                    if (string.IsNullOrEmpty(res.Item1))
                                        statusCode = HttpStatusCode.InternalServerError;
                                    else
                                    {
                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                        response.Headers.Add("Ubi-Forwarded-By", "ue1-p-us-public-nginx-056b582ac580ba328");
                                        response.Headers.Add("Ubi-TransactionId", Guid.NewGuid().ToString());
                                        statusCode = HttpStatusCode.OK;
                                    }
                                    response.StatusCode = (int)statusCode;
                                    if (!string.IsNullOrEmpty(res.Item2))
                                        response.ContentType = res.Item2;
                                    else
                                        response.ContentType = "text/plain";
                                    sent = await response.Send(res.Item1);
                                }
                                else
                                {
                                    statusCode = HttpStatusCode.Forbidden;
                                    response.StatusCode = (int)statusCode;
                                    response.ContentType = "text/plain";
                                    sent = await response.Send();
                                }
                            }
                            #endregion

                            #region CentralDispatchManager API
                            else if (HPDDomains.Contains(Host))
                            {
                                LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a CentralDispatchManager method : {absolutepath}");

                                string? res = new CDMClass(request.Method.ToString(), absolutepath, HTTPSServerConfiguration.APIStaticFolder).ProcessRequest(request.DataAsBytes, request.ContentType, apiPath);
                                if (string.IsNullOrEmpty(res))
                                {
                                    response.ContentType = "text/plain";
                                    statusCode = HttpStatusCode.InternalServerError;
                                }
                                else
                                {
                                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                    response.ContentType = "text/xml";
                                    statusCode = HttpStatusCode.OK;
                                }
                                response.StatusCode = (int)statusCode;
                                sent = await response.Send(res);
                            }
                            #endregion

                            #region CAPONE GriefReporter API
                            else if (CAPONEDomains.Contains(Host))
                            {
                                LoggerAccessor.LogInfo($"[HTTPS] - {clientip}:{clientport} Requested a CAPONE method : {absolutepath}");

                                string? res = new CAPONEClass(request.Method.ToString(), absolutepath, HTTPSServerConfiguration.APIStaticFolder).ProcessRequest(request.DataAsBytes, request.ContentType, apiPath);
                                if (string.IsNullOrEmpty(res))
                                {
                                    response.ContentType = "text/plain";
                                    statusCode = HttpStatusCode.InternalServerError;
                                }
                                else
                                {
                                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                    response.ContentType = "text/xml";
                                    statusCode = HttpStatusCode.OK;
                                }
                                response.StatusCode = (int)statusCode;
                                sent = await response.Send(res);
                            }
                            #endregion

                            #region PSH Central
                            else if (Host == "apps.pshomecentral.net" && absolutepath == "/PrivateRTE/checkAuth.php")
                            {
                                LoggerAccessor.LogError($"[HTTPS] - {clientip}:{clientport} Requested a PS Home Central method : {absolutepath}");

                                statusCode = HttpStatusCode.OK;
                                response.StatusCode = (int)statusCode;
                                response.ContentType = "text/plain";
                                sent = await response.Send("false");
                            }
                            #endregion

                            else
                            {
                                string? encoding = request.RetrieveHeaderValue("Accept-Encoding");

                                switch (request.Method.ToString())
                                {
                                    case "GET":
                                        switch (absolutepath)
                                        {
                                            case "/dns-query":
                                                bool acceptsDoH = false;

                                                if (string.IsNullOrEmpty(Accept))
                                                    acceptsDoH = true;
                                                else
                                                {
                                                    foreach (string mediaType in Accept.Split(','))
                                                    {
                                                        if (mediaType.Equals("application/dns-message", StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            acceptsDoH = true;
                                                            break;
                                                        }
                                                    }
                                                }

                                                if (!SecureDNSConfigProcessor.Initiated)
                                                {
                                                    statusCode = HttpStatusCode.MethodNotAllowed;
                                                    response.StatusCode = (int)statusCode;
                                                    response.ContentType = "text/plain";
                                                    sent = await response.Send("DNS system not enabled or initializing");
                                                }
                                                else if (!acceptsDoH)
                                                {
                                                    statusCode = HttpStatusCode.BadRequest;
                                                    response.StatusCode = (int)statusCode;
                                                    response.ContentType = "text/plain";
                                                    sent = await response.Send("Bad Request");
                                                }
                                                else
                                                {
                                                    string? dnsRequestBase64Url = request.Query.Elements["dns"];
                                                    if (string.IsNullOrEmpty(dnsRequestBase64Url))
                                                    {
                                                        statusCode = HttpStatusCode.BadRequest;
                                                        response.StatusCode = (int)statusCode;
                                                        response.ContentType = "text/plain";
                                                        sent = await response.Send("Bad Request");
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
                                                            bool treated = false;

                                                            byte[]? DnsReq = Convert.FromBase64String(dnsRequestBase64Url);

                                                            string fullname = string.Join(".", DNSProcessor.GetDnsName(DnsReq).ToArray());

                                                            LoggerAccessor.LogInfo($"[HTTPS_DNS] - Host: {fullname} was Requested.");

                                                            string? url = null;

                                                            if (fullname.EndsWith("in-addr.arpa") && IPAddress.TryParse(fullname[..^13], out IPAddress? arparuleaddr)) // IPV4 Only.
                                                            {
                                                                if (arparuleaddr != null)
                                                                {
                                                                    if (arparuleaddr.AddressFamily == AddressFamily.InterNetwork)
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
                                                                if (SecureDNSConfigProcessor.DicRules != null && SecureDNSConfigProcessor.DicRules.TryGetValue(fullname, out DnsSettings value))
                                                                {
                                                                    if (value.Mode == HandleMode.Allow) url = fullname;
                                                                    else if (value.Mode == HandleMode.Redirect) url = value.Address ?? "127.0.0.1";
                                                                    else if (value.Mode == HandleMode.Deny) url = "NXDOMAIN";
                                                                    treated = true;
                                                                }

                                                                if (!treated && SecureDNSConfigProcessor.StarRules != null)
                                                                {
                                                                    foreach (KeyValuePair<string, DnsSettings> rule in SecureDNSConfigProcessor.StarRules)
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
                                                                url = CyberBackendLibrary.TCP_IP.IPUtils.GetFirstActiveIPAddress(fullname, CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress(true));

                                                            IPAddress ip = IPAddress.None; // NXDOMAIN
                                                            if (!string.IsNullOrEmpty(url) && url != "NXDOMAIN")
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

                                                                DnsReq = DNSProcessor.MakeDnsResponsePacket(DnsReq, ip);
                                                            }
                                                            else if (url == "NXDOMAIN")
                                                                DnsReq = DNSProcessor.MakeDnsResponsePacket(DnsReq, ip);

                                                            if (DnsReq != null && DnsReq.Length <= 512) // Https wire expect padding.
                                                            {
                                                                // Create a new byte array with size 512
                                                                byte[] paddedArray = new byte[512];

                                                                // Copy the original array content to the padded array
                                                                Array.Copy(DnsReq, paddedArray, DnsReq.Length);

                                                                statusCode = HttpStatusCode.OK;
                                                                response.StatusCode = (int)statusCode;
                                                                response.ContentType = "application/dns-message";
                                                                sent = await response.Send(DnsReq);
                                                            }
                                                            else
                                                            {
                                                                statusCode = HttpStatusCode.InternalServerError;
                                                                response.StatusCode = (int)statusCode;
                                                                response.ContentType = "text/plain";
                                                                sent = await response.Send();
                                                            }
                                                        }
                                                        catch (Exception)
                                                        {
                                                            statusCode = HttpStatusCode.InternalServerError;
                                                            response.StatusCode = (int)statusCode;
                                                            response.ContentType = "text/plain";
                                                            sent = await response.Send();
                                                        }
                                                    }
                                                }
                                                break;
                                            case "/!player":
                                            case "/!player/":
                                                if (ServerIP.Length > 15)
                                                    ServerIP = "[" + ServerIP + "]"; // Format the hostname if it's a IPV6 url format.
                                                WebVideoPlayer? WebPlayer = new(request.Query.Elements, $"http://{ServerIP}/!webvideo/?");
                                                statusCode = HttpStatusCode.OK;
                                                response.StatusCode = (int)statusCode;
                                                response.ContentType = "text/html";
                                                foreach (string[] HeaderCollection in WebPlayer.HeadersToSet)
                                                {
                                                    response.Headers.Add(HeaderCollection[0], HeaderCollection[1]);
                                                }
                                                sent = await response.Send(WebPlayer.HtmlPage);
                                                WebPlayer = null;
                                                break;
                                            default:
                                                if (Directory.Exists(filePath) && filePath.EndsWith("/"))
                                                {
                                                    if (request.RetrieveQueryValue("directory") == "on")
                                                    {
                                                        statusCode = HttpStatusCode.OK;
                                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                        response.StatusCode = (int)statusCode;
                                                        response.ContentType = "application/json";
                                                        sent = await response.Send(FileStructureToJson.GetFileStructureAsJson(filePath[..^1], $"https://example.com{absolutepath[..^1]}", HTTPSServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes));
                                                    }
                                                    else if (request.RetrieveQueryValue("m3u") == "on")
                                                    {
                                                        string? m3ufile = StaticFileSystem.GetM3UStreamFromDirectory(filePath[..^1], $"https://example.com{absolutepath[..^1]}");
                                                        if (!string.IsNullOrEmpty(m3ufile))
                                                        {
                                                            statusCode = HttpStatusCode.OK;
                                                            response.StatusCode = (int)statusCode;
                                                            response.ContentType = "audio/x-mpegurl";
                                                            sent = await response.Send(m3ufile);
                                                        }
                                                        else
                                                        {
                                                            statusCode = HttpStatusCode.NoContent;
                                                            response.StatusCode = (int)statusCode;
                                                            sent = await response.Send();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        bool handled = false;

                                                        foreach (string indexFile in HTTPProcessor._DefaultFiles)
                                                        {
                                                            if (File.Exists(filePath + $"/{indexFile}"))
                                                            {
                                                                handled = true;

                                                                if (indexFile.EndsWith(".php") && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder))
                                                                {
                                                                    var CollectPHP = Extensions.PHP.ProcessPHPPage(filePath + $"/{indexFile}", HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx);
                                                                    statusCode = HttpStatusCode.OK;
                                                                    if (CollectPHP.Item2 != null)
                                                                    {
                                                                        foreach (var innerArray in CollectPHP.Item2)
                                                                        {
                                                                            // Ensure the inner array has at least two elements
                                                                            if (innerArray.Length >= 2)
                                                                                // Extract two values from the inner array
                                                                                response.Headers.Add(innerArray[0], innerArray[1]);
                                                                        }
                                                                    }
                                                                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                                    response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath + $"/{indexFile}").ToString("r"));
                                                                    response.StatusCode = (int)statusCode;
                                                                    response.ContentType = "text/html";
                                                                    sent = await response.Send(CollectPHP.Item1);
                                                                }
                                                                else
                                                                {
                                                                    using FileStream stream = new(filePath + $"/{indexFile}", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                                                    byte[]? buffer = null;

                                                                    using (MemoryStream ms = new())
                                                                    {
                                                                        stream.CopyTo(ms);
                                                                        buffer = ms.ToArray();
                                                                        ms.Flush();
                                                                    }

                                                                    if (buffer != null)
                                                                    {
                                                                        statusCode = HttpStatusCode.OK;
                                                                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                                        response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath + $"/{indexFile}").ToString("r"));
                                                                        response.StatusCode = (int)statusCode;
                                                                        response.ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(filePath + $"/{indexFile}"), HTTPSServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                                                                        sent = await response.Send(buffer);
                                                                    }
                                                                    else
                                                                    {
                                                                        statusCode = HttpStatusCode.InternalServerError;
                                                                        response.StatusCode = (int)statusCode;
                                                                        response.ContentType = "text/plain";
                                                                        sent = await response.Send();
                                                                    }

                                                                    stream.Flush();
                                                                }
                                                                break;
                                                            }
                                                        }

                                                        if (!handled)
                                                        {
                                                            statusCode = HttpStatusCode.NotFound;
                                                            response.StatusCode = (int)statusCode;

                                                            if (!string.IsNullOrEmpty(Accept) && Accept.Contains("html"))
                                                            {
                                                                response.ContentType = "text/html";
                                                                sent = await response.Send(await DefaultHTMLPages.GenerateNotFound(absolutepath, $"https://{(string.IsNullOrEmpty(Host) ? (ServerIP.Length > 15 ? "[" + ServerIP + "]" : ServerIP) : Host)}",
                                                                    HTTPSServerConfiguration.HTTPSStaticFolder, "Apache 2.2.22 (Unix) DAV/2", ServerPort.ToString(), HTTPSServerConfiguration.NotFoundSuggestions));
                                                            }
                                                            else
                                                            {
                                                                response.ContentType = "text/plain";
                                                                sent = await response.Send();
                                                            }
                                                        }
                                                    }
                                                }
                                                else if ((absolutepath.EndsWith(".asp", StringComparison.InvariantCultureIgnoreCase) || absolutepath.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(HTTPSServerConfiguration.ASPNETRedirectUrl))
                                                {
                                                    statusCode = HttpStatusCode.PermanentRedirect;
                                                    response.Headers.Add("Location", $"{HTTPSServerConfiguration.ASPNETRedirectUrl}{request.Url.RawWithQuery}");
                                                    response.StatusCode = (int)statusCode;
                                                    response.ContentType = "text/html";
                                                    sent = await response.Send();
                                                }
                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(HTTPSServerConfiguration.PHPRedirectUrl))
                                                {
                                                    statusCode = HttpStatusCode.PermanentRedirect;
                                                    response.Headers.Add("Location", $"{HTTPSServerConfiguration.PHPRedirectUrl}{request.Url.RawWithQuery}");
                                                    response.StatusCode = (int)statusCode;
                                                    response.ContentType = "text/html";
                                                    sent = await response.Send();
                                                }
                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                                {
                                                    var CollectPHP = Extensions.PHP.ProcessPHPPage(filePath, HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx);
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
                                                                response.Headers.Add(value1, value2);
                                                            }
                                                        }
                                                    }
                                                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                    response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                                    response.StatusCode = (int)statusCode;
                                                    response.ContentType = "text/html";
                                                    sent = await response.Send(CollectPHP.Item1);
                                                }
                                                else
                                                {
                                                    if (File.Exists(filePath))
                                                    {
                                                        string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(filePath), HTTPSServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                                                        if (ContentType == "application/octet-stream")
                                                        {
                                                            byte[] VerificationChunck = DataTypesUtils.ReadSmallFileChunck(filePath, 10);
                                                            foreach (var entry in HTTPProcessor._PathernDictionary)
                                                            {
                                                                if (DataTypesUtils.FindBytePattern(VerificationChunck, entry.Value) != -1)
                                                                {
                                                                    ContentType = entry.Key;
                                                                    break;
                                                                }
                                                            }
                                                        }

                                                        string? UserAgent = null;

                                                        if (!string.IsNullOrEmpty(request.Useragent))
                                                            UserAgent = request.Useragent.ToLower();

                                                        if (HTTPSServerConfiguration.EnableLiveTranscoding
                                                            && ((ContentType.Contains("video") && !ContentType.Contains("mp4"))
                                                            || ContentType.Contains("audio")) && !ContentType.Contains("mpeg")
                                                            && !string.IsNullOrEmpty(UserAgent) && (UserAgent.Contains("firefox")
                                                            || UserAgent.Contains("chrome") || UserAgent.Contains("trident")))
                                                            sent = await new Extensions.Mp4TranscodeHandler(filePath, HTTPSServerConfiguration.ConvertersFolder).ProcessVideoTranscode(ctx);
                                                        else if (!string.IsNullOrEmpty(request.RetrieveHeaderValue("Range"))) // Mmm, is it possible to have more?
                                                            sent = new LocalFileStreamHelper().Handle_LocalFile_Stream(ctx, filePath, ContentType);
                                                        else
                                                        {
                                                            // send file
                                                            LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a file : {absolutepath}");

                                                            sent = await SendFile(ctx, encoding, filePath, ContentType);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        LoggerAccessor.LogWarn($"[HTTPS] - {clientip} Requested a non-existant file : {filePath}");

                                                        statusCode = HttpStatusCode.NotFound;
                                                        response.StatusCode = (int)statusCode;

                                                        if (!string.IsNullOrEmpty(Accept) && Accept.Contains("html"))
                                                        {
                                                            response.ContentType = "text/html";
                                                            sent = await response.Send(await DefaultHTMLPages.GenerateNotFound(absolutepath, $"https://{(string.IsNullOrEmpty(Host) ? (ServerIP.Length > 15 ? "[" + ServerIP + "]" : ServerIP) : Host)}",
                                                                HTTPSServerConfiguration.HTTPSStaticFolder, "Apache 2.2.22 (Unix) DAV/2", ServerPort.ToString(), HTTPSServerConfiguration.NotFoundSuggestions));
                                                        }
                                                        else
                                                        {
                                                            response.ContentType = "text/plain";
                                                            sent = await response.Send();
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
                                                if (!SecureDNSConfigProcessor.Initiated)
                                                {
                                                    statusCode = HttpStatusCode.MethodNotAllowed;
                                                    response.StatusCode = (int)statusCode;
                                                    response.ContentType = "text/plain";
                                                    sent = await response.Send("DNS system not enabled or initializing");
                                                }
                                                else if (!string.Equals(request.ContentType, "application/dns-message", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    statusCode = HttpStatusCode.UnsupportedMediaType;
                                                    response.StatusCode = (int)statusCode;
                                                    response.ContentType = "text/plain";
                                                    sent = await response.Send("Unsupported Media Type");
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        byte[]? DnsReq = request.DataAsBytes;

                                                        string fullname = string.Join(".", DNSProcessor.GetDnsName(DnsReq).ToArray());

                                                        LoggerAccessor.LogInfo($"[HTTPS_DNS] - Host: {fullname} was Requested.");

                                                        string? url = null;
                                                        bool treated = false;

                                                        if (fullname.EndsWith("in-addr.arpa") && IPAddress.TryParse(fullname[..^13], out IPAddress? arparuleaddr)) // IPV4 Only.
                                                        {
                                                            if (arparuleaddr != null)
                                                            {
                                                                if (arparuleaddr.AddressFamily == AddressFamily.InterNetwork)
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
                                                            if (SecureDNSConfigProcessor.DicRules != null && SecureDNSConfigProcessor.DicRules.TryGetValue(fullname, out DnsSettings value))
                                                            {
                                                                if (value.Mode == HandleMode.Allow) url = fullname;
                                                                else if (value.Mode == HandleMode.Redirect) url = value.Address ?? "127.0.0.1";
                                                                else if (value.Mode == HandleMode.Deny) url = "NXDOMAIN";
                                                                treated = true;
                                                            }

                                                            if (!treated && SecureDNSConfigProcessor.StarRules != null)
                                                            {
                                                                foreach (KeyValuePair<string, DnsSettings> rule in SecureDNSConfigProcessor.StarRules)
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
                                                            url = CyberBackendLibrary.TCP_IP.IPUtils.GetFirstActiveIPAddress(fullname, CyberBackendLibrary.TCP_IP.IPUtils.GetPublicIPAddress(true));

                                                        IPAddress ip = IPAddress.None; // NXDOMAIN
                                                        if (!string.IsNullOrEmpty(url) && url != "NXDOMAIN")
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

                                                            DnsReq = DNSProcessor.MakeDnsResponsePacket(DnsReq, ip);
                                                        }
                                                        else if (url == "NXDOMAIN")
                                                            DnsReq = DNSProcessor.MakeDnsResponsePacket(DnsReq, ip);

                                                        if (DnsReq != null && DnsReq.Length <= 512) // Https wire expect padding.
                                                        {
                                                            // Create a new byte array with size 512
                                                            byte[] paddedArray = new byte[512];

                                                            // Copy the original array content to the padded array
                                                            Array.Copy(DnsReq, paddedArray, DnsReq.Length);

                                                            statusCode = HttpStatusCode.OK;
                                                            response.StatusCode = (int)statusCode;
                                                            response.ContentType = "application/dns-message";
                                                            sent = await response.Send(DnsReq);
                                                        }
                                                        else
                                                        {
                                                            statusCode = HttpStatusCode.InternalServerError;
                                                            response.StatusCode = (int)statusCode;
                                                            response.ContentType = "text/plain";
                                                            sent = await response.Send();
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {
                                                        statusCode = HttpStatusCode.InternalServerError;
                                                        response.StatusCode = (int)statusCode;
                                                        response.ContentType = "text/plain";
                                                        sent = await response.Send();
                                                    }
                                                }
                                                break;
                                            default:
                                                if ((absolutepath.EndsWith(".asp", StringComparison.InvariantCultureIgnoreCase) || absolutepath.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(HTTPSServerConfiguration.ASPNETRedirectUrl))
                                                {
                                                    statusCode = HttpStatusCode.PermanentRedirect;
                                                    response.Headers.Add("Location", $"{HTTPSServerConfiguration.ASPNETRedirectUrl}{request.Url.RawWithQuery}");
                                                    response.StatusCode = (int)statusCode;
                                                    response.ContentType = "text/html";
                                                    sent = await response.Send();
                                                }
                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(HTTPSServerConfiguration.PHPRedirectUrl))
                                                {
                                                    statusCode = HttpStatusCode.PermanentRedirect;
                                                    response.Headers.Add("Location", $"{HTTPSServerConfiguration.PHPRedirectUrl}{request.Url.RawWithQuery}");
                                                    response.StatusCode = (int)statusCode;
                                                    response.ContentType = "text/html";
                                                    sent = await response.Send();
                                                }
                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && Directory.Exists(HTTPSServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                                {
                                                    (byte[]?, string[][]) CollectPHP = Extensions.PHP.ProcessPHPPage(filePath, HTTPSServerConfiguration.PHPStaticFolder, HTTPSServerConfiguration.PHPVersion, clientip, clientport, ctx);
                                                    statusCode = HttpStatusCode.OK;
                                                    if (CollectPHP.Item2 != null)
                                                    {
                                                        foreach (string[] innerArray in CollectPHP.Item2)
                                                        {
                                                            // Ensure the inner array has at least two elements
                                                            if (innerArray.Length >= 2)
                                                                // Extract two values from the inner array
                                                                response.Headers.Add(innerArray[0], innerArray[1]);
                                                        }
                                                    }
                                                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                    response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                                    response.StatusCode = (int)statusCode;
                                                    response.ContentType = "text/html";
                                                    sent = await response.Send(CollectPHP.Item1);
                                                }
                                                else
                                                {
                                                    if (File.Exists(filePath))
                                                    {
                                                        string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(filePath), HTTPSServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                                                        if (ContentType == "application/octet-stream")
                                                        {
                                                            byte[] VerificationChunck = DataTypesUtils.ReadSmallFileChunck(filePath, 10);
                                                            foreach (var entry in HTTPProcessor._PathernDictionary)
                                                            {
                                                                if (DataTypesUtils.FindBytePattern(VerificationChunck, entry.Value) != -1)
                                                                {
                                                                    ContentType = entry.Key;
                                                                    break;
                                                                }
                                                            }
                                                        }

                                                        string UserAgent = request.Useragent.ToLower();

                                                        if (!string.IsNullOrEmpty(request.RetrieveHeaderValue("Range"))) // Mmm, is it possible to have more?
                                                            sent = new LocalFileStreamHelper().Handle_LocalFile_Stream(ctx, filePath, ContentType);
                                                        else
                                                        {
                                                            // send file
                                                            LoggerAccessor.LogInfo($"[HTTPS] - {clientip} Requested a file : {absolutepath}");

                                                            sent = await SendFile(ctx, encoding, filePath, ContentType);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        LoggerAccessor.LogWarn($"[HTTPS] - {clientip} Requested a non-existant file : {filePath}");

                                                        statusCode = HttpStatusCode.NotFound;
                                                        response.StatusCode = (int)statusCode;

                                                        if (!string.IsNullOrEmpty(Accept) && Accept.Contains("html"))
                                                        {
                                                            response.ContentType = "text/html";
                                                            sent = await response.Send(await DefaultHTMLPages.GenerateNotFound(absolutepath, $"https://{(string.IsNullOrEmpty(Host) ? (ServerIP.Length > 15 ? "[" + ServerIP + "]" : ServerIP) : Host)}",
                                                                HTTPSServerConfiguration.HTTPSStaticFolder, "Apache 2.2.22 (Unix) DAV/2", ServerPort.ToString(), HTTPSServerConfiguration.NotFoundSuggestions));
                                                        }
                                                        else
                                                        {
                                                            response.ContentType = "text/plain";
                                                            sent = await response.Send();
                                                        }
                                                    }
                                                }
                                                break;
                                        }
                                        break;
                                    case "PUT":
                                        if (HTTPSServerConfiguration.EnablePUTMethod)
                                        {
                                            string ContentType = request.ContentType;
                                            byte[] PostData = request.DataAsBytes;
                                            if (PostData != Array.Empty<byte>() && !string.IsNullOrEmpty(ContentType))
                                            {
                                                string? boundary = HTTPProcessor.ExtractBoundary(ContentType);
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
                                        response.StatusCode = (int)statusCode;
                                        response.ContentType = "text/plain";
                                        sent = await response.Send();
                                        break;
                                    case "DELETE":
                                        statusCode = HttpStatusCode.Forbidden;
                                        response.StatusCode = (int)statusCode;
                                        response.ContentType = "text/plain";
                                        sent = await response.Send();
                                        break;
                                    case "HEAD":
                                        FileInfo? fileInfo = new(filePath);
                                        if (fileInfo.Exists)
                                        {
                                            statusCode = HttpStatusCode.OK;
                                            string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(filePath), HTTPSServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                                            if (ContentType == "application/octet-stream")
                                            {
                                                bool matched = false;
                                                byte[] VerificationChunck = DataTypesUtils.ReadSmallFileChunck(filePath, 10);
                                                foreach (var entry in HTTPProcessor._PathernDictionary)
                                                {
                                                    if (DataTypesUtils.FindBytePattern(VerificationChunck, entry.Value) != -1)
                                                    {
                                                        matched = true;
                                                        response.ContentType = entry.Key;
                                                        break;
                                                    }
                                                }
                                                if (!matched)
                                                    response.ContentType = ContentType;
                                            }
                                            else
                                                response.ContentType = ContentType;

                                            response.Headers.Set("Content-Length", fileInfo.Length.ToString());
                                            response.Headers.Set("Date", DateTime.Now.ToString("r"));
                                            response.Headers.Set("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                                            response.ContentLength = fileInfo.Length;
                                            response.StatusCode = (int)statusCode;
                                            response.ContentType = "text/plain";
                                            sent = await response.Send();
                                        }
                                        else
                                        {
                                            LoggerAccessor.LogWarn($"[HTTPS] - {clientip} Requested a non-existant file: {filePath}");

                                            statusCode = HttpStatusCode.NotFound;
                                            response.StatusCode = (int)statusCode;

                                            if (!string.IsNullOrEmpty(Accept) && Accept.Contains("html"))
                                            {
                                                response.ContentType = "text/html";
                                                sent = await response.Send(await DefaultHTMLPages.GenerateNotFound(absolutepath, $"https://{(string.IsNullOrEmpty(Host) ? (ServerIP.Length > 15 ? "[" + ServerIP + "]" : ServerIP) : Host)}",
                                                    HTTPSServerConfiguration.HTTPSStaticFolder, "Apache 2.2.22 (Unix) DAV/2", ServerPort.ToString(), HTTPSServerConfiguration.NotFoundSuggestions));
                                            }
                                            else
                                            {
                                                response.ContentType = "text/plain";
                                                sent = await response.Send();
                                            }
                                        }
                                        fileInfo = null;
                                        break;
                                    case "OPTIONS":
                                        statusCode = HttpStatusCode.OK;
                                        response.StatusCode = (int)statusCode;
                                        response.ContentType = "text/plain";
                                        response.Headers.Set("Allow", "OPTIONS, GET, HEAD, POST");
                                        sent = await response.Send();
                                        break;
                                    case "PROPFIND":
                                        if (File.Exists(filePath))
                                        {
                                            string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(filePath), HTTPSServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                                            if (ContentType == "application/octet-stream")
                                            {
                                                byte[] VerificationChunck = DataTypesUtils.ReadSmallFileChunck(filePath, 10);
                                                foreach (var entry in HTTPProcessor._PathernDictionary)
                                                {
                                                    if (DataTypesUtils.FindBytePattern(VerificationChunck, entry.Value) != -1)
                                                    {
                                                        ContentType = entry.Key;
                                                        break;
                                                    }
                                                }
                                            }

                                            if (ServerIP.Length > 15)
                                                ServerIP = "[" + ServerIP + "]"; // Format the hostname if it's a IPV6 url format.

                                            statusCode = HttpStatusCode.MultiStatus;
                                            response.StatusCode = (int)statusCode;
                                            response.ContentType = "text/xml";
                                            sent = await response.Send("<?xml version=\"1.0\"?>\r\n" +
                                                "<a:multistatus\r\n" +
                                                $"  xmlns:b=\"urn:uuid:{Guid.NewGuid()}/\"\r\n" +
                                                "  xmlns:a=\"DAV:\">\r\n" +
                                                " <a:response>\r\n" +
                                                $"   <a:href>https://{ServerIP}:{ServerPort}{absolutepath}</a:href>\r\n" +
                                                "   <a:propstat>\r\n" +
                                                "    <a:status>HTTP/1.1 200 OK</a:status>\r\n" +
                                                "       <a:prop>\r\n" +
                                                $"        <a:getcontenttype>{ContentType}</a:getcontenttype>\r\n" +
                                                $"        <a:getcontentlength b:dt=\"int\">{new FileInfo(filePath).Length}</a:getcontentlength>\r\n" +
                                                "       </a:prop>\r\n" +
                                                "   </a:propstat>\r\n" +
                                                " </a:response>\r\n" +
                                                "</a:multistatus>");
                                        }
                                        else
                                        {
                                            statusCode = HttpStatusCode.NotFound;
                                            response.StatusCode = (int)statusCode;

                                            if (!string.IsNullOrEmpty(Accept) && Accept.Contains("html"))
                                            {
                                                response.ContentType = "text/html";
                                                sent = await response.Send(await DefaultHTMLPages.GenerateNotFound(absolutepath, $"https://{(string.IsNullOrEmpty(Host) ? (ServerIP.Length > 15 ? "[" + ServerIP + "]" : ServerIP) : Host)}",
                                                    HTTPSServerConfiguration.HTTPSStaticFolder, "Apache 2.2.22 (Unix) DAV/2", ServerPort.ToString(), HTTPSServerConfiguration.NotFoundSuggestions));
                                            }
                                            else
                                            {
                                                response.ContentType = "text/plain";
                                                sent = await response.Send();
                                            }
                                        }
                                        break;
                                    default:
                                        statusCode = HttpStatusCode.Forbidden;
                                        response.StatusCode = (int)statusCode;
                                        response.ContentType = "text/plain";
                                        sent = await response.Send();
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    response.StatusCode = (int)statusCode; // Send the other status.
                    response.ContentType = "text/plain";
                    sent = await response.Send();
                }

#if DEBUG
                if (!sent)
                    LoggerAccessor.LogWarn($"[HTTPS] - {clientip}:{clientport} Failed to receive the response! Client might have closed the wire.");
#endif
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException socketException && socketException.ErrorCode != 995 &&
                    socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted
                    && socketException.SocketErrorCode != SocketError.ConnectionRefused)
                    LoggerAccessor.LogError($"[HTTPS] - DefaultRoute - IO-Socket thrown an exception : {ex}");
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 995 && ex.SocketErrorCode != SocketError.ConnectionReset && ex.SocketErrorCode != SocketError.ConnectionAborted && ex.SocketErrorCode != SocketError.ConnectionRefused)
                    LoggerAccessor.LogError($"[HTTPS] - DefaultRoute - Socket thrown an exception : {ex}");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[HTTPS] - DefaultRoute - thrown an exception: {ex}");
            }
        }

        private static async Task<bool> SendFile(HttpContextBase ctx, string encoding, string filePath, string contentType)
        {
            Stream? st = null;
            bool sent = false;
            long fileSize = new FileInfo(filePath).Length;

            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
            ctx.Response.ContentType = contentType;
            ctx.Response.StatusCode = 200;
            if (HTTPSServerConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(encoding))
            {
                if (encoding.Contains("zstd") && fileSize <= 8000000)
                {
                    ctx.Response.Headers.Add("Content-Encoding", "zstd");
                    st = HTTPProcessor.ZstdCompressStream(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), fileSize > 8000000);
                }
                else if (encoding.Contains("br") && fileSize <= 8000000)
                {
                    ctx.Response.Headers.Add("Content-Encoding", "br");
                    st = HTTPProcessor.BrotliCompressStream(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), fileSize > 8000000);
                }
                else if (encoding.Contains("gzip") && fileSize <= 8000000)
                {
                    ctx.Response.Headers.Add("Content-Encoding", "gzip");
                    st = HTTPProcessor.GzipCompressStream(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), fileSize > 8000000);
                }
                else if (encoding.Contains("deflate") && fileSize <= 8000000)
                {
                    ctx.Response.Headers.Add("Content-Encoding", "deflate");
                    st = HTTPProcessor.InflateStream(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), fileSize > 8000000);
                }
                else
                    st = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            else
                st = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            sent = await ctx.Response.Send(st.Length, st);

            st.Flush();
            st.Dispose();

            return sent;
        }
#if NET7_0_OR_GREATER
        [GeneratedRegex("Match (\\d+) (.*) (.*)$")]
        private static partial Regex ApacheMatchRegex();
        [GeneratedRegex("^(GET|POST|PUT|DELETE|HEAD|OPTIONS|PATCH)")]
        private static partial Regex HttpMethodRegex();
        [GeneratedRegex("\\b\\d{3}\\b")]
        private static partial Regex HttpStatusCodeRegex();
#endif
    }
}
