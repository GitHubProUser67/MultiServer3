// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using WebAPIService.OHS;
using WebAPIService.OUWF;
using WebAPIService.PREMIUMAGENCY;
using WebAPIService.VEEMEE;
using WebAPIService.JUGGERNAUT;
using WebAPIService.NDREAMS;
using WebAPIService.LOOT;
using WebAPIService.UBISOFT.HERMES_API;
using WebAPIService.FROMSOFTWARE;
using WebAPIService.CAPONE;
using WebAPIService.CDM;
using WebAPIService.MultiMedia;
using WebAPIService.UBISOFT.gsconnect;
using WebAPIService.HELLFIRE;
using NetworkLibrary.GeoLocalization;
using NetworkLibrary.HTTP;
using CustomLogger;
using HttpMultipartParser;
using HTTPServer.Extensions;
using HTTPServer.Models;
using HTTPServer.RouteHandlers;
using System;
using System.Buffers;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using NetworkLibrary.HTTP.PluginManager;
using NetworkLibrary.Extension;
using Newtonsoft.Json;
using WebAPIService.HTS;
using WebAPIService.ILoveSony;
using WebAPIService.CCPGames;
using WebAPIService.DEMANGLER;
using WebAPIService.UBISOFT.BuildAPI;
using NetworkLibrary.Extension.Csharp;

namespace HTTPServer
{
    public partial class HttpProcessor
    {
        #region Fields

        private readonly List<Route> Routes = new();
        private int KeepAliveClients = 0;

        public string ServerIp = "127.0.0.1";

        #endregion

        #region Domains
        private readonly static List<string> HPDDomains = new() {
                                    "prd.destinations.scea.com",
                                    "pre.destinations.scea.com",
                                    "qa.destinations.scea.com",
                                    "dev.destinations.scea.com"
                                };

        private readonly static List<string> CAPONEDomains = new() {
                                    "collector.gr.online.scea.com",
                                    "collector-nonprod.gr.online.scea.com",
                                    "collector-dev.gr.online.scea.com",
                                    "content.gr.online.scea.com",
                                    "content-nonprod.gr.online.scea.com",
                                    "content-dev.gr.online.scea.com",
                                };

        private readonly static List<string> nDreamsDomains = new()
                                {
                                    "pshome.ndreams.net",
                                    "www.ndreamshs.com",
                                    "www.ndreamsportal.com",
                                    "nDreams-multiserver-cdn"
                                };

        private readonly static List<string> HellFireGamesDomains = new()
                                {
                                    "game.hellfiregames.com",
                                    "game2.hellfiregames.com",
                                    "holdemqa.destinations.scea.com",
                                    "holdemeu.destinations.scea.com",
                                    "holdemna.destinations.scea.com",
                                    "c93f2f1d-3946-4f37-b004-1196acf599c5.scalr.ws"
                                };

        private readonly static List<string> HTSDomains = new() {
                                    "samples.hdk.scee.net",
                                };


        private readonly static List<string> CCPGamesDomains = new() {
                                    //"dust.ccpgamescdn.com",
                                    "dust514.online"
                                };
        private readonly static List<string> ILoveSonyDomains = new() {
                                    "www.myresistance.net",
                                };

        #endregion

        #region Constructors

        public HttpProcessor()
        {
            
        }

        #endregion

        #region Public Methods

        public static bool IsIPBanned(string ipAddress, int? clientport)
        {
            if (HTTPServerConfiguration.BannedIPs != null && HTTPServerConfiguration.BannedIPs.Contains(ipAddress))
            {
                LoggerAccessor.LogError($"[SECURITY] - {ipAddress}:{clientport} Requested the HTTP server while being banned!");
                return true;
            }

            return false;
        }

        public void HandleClient(TcpClient tcpClient, ushort ListenerPort)
        {
            bool IsInterlocked = false;
            HttpRequest? request = null;
            HttpResponse? response = null;

            try
            {
                string? clientip = ((IPEndPoint?)tcpClient.Client.RemoteEndPoint)?.Address.ToString();

                int? clientport = ((IPEndPoint?)tcpClient.Client.RemoteEndPoint)?.Port;

                if (clientport == null || string.IsNullOrEmpty(clientip) || IsIPBanned(clientip, clientport))
                    return;

                IsInterlocked = Interlocked.Increment(ref KeepAliveClients) > 0;
                bool AllowKeepAlive = KeepAliveClients < HTTPServerConfiguration.MaximumAllowedKeepAliveClients;

                using Stream? inputStream = GetInputStream(tcpClient);
                using (Stream? outputStream = GetOutputStream(tcpClient))
                {
                    while (tcpClient.IsConnected())
                    {
                        if (tcpClient.Available > 0 && outputStream.CanWrite)
                        {
                            if (request == null)
                                request = GetRequest(inputStream, clientip, clientport.ToString()!, ListenerPort);
                            else
                                request = AppendRequestOrInputStream(inputStream, request, clientip, clientport.ToString()!, ListenerPort);

                            if (request != null && !string.IsNullOrEmpty(request.RawUrlWithQuery))
                            {
                                string UserAgent = request.RetrieveHeaderValue("User-Agent");

                                if (UserAgent.Contains("bytespider", StringComparison.InvariantCultureIgnoreCase)) // Get Away TikTok.
                                {
                                    WriteResponse(outputStream, request, HttpBuilder.NotAllowed(), null, false, false);
                                    continue;
                                }
                                else if (response != null)
                                {
                                    response.Dispose();
                                    response = null;
                                }

                                bool EtagCompatible = false;
                                DateTime CurrentDate = DateTime.UtcNow;
                                string Method = request.Method;
                                string Host = request.RetrieveHeaderValue("host", false);
                                string Accept = request.RetrieveHeaderValue("Accept");
                                string cacheControl = request.RetrieveHeaderValue("Cache-Control");
                                bool noCompressCacheControl = !string.IsNullOrEmpty(cacheControl) && cacheControl == "no-transform";
                                string SuplementalMessage = string.Empty;
                                string fullurl = HTTPProcessor.DecodeUrl(request.RawUrlWithQuery);
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

                                        if (HTTPServerConfiguration.DateTimeOffset != null && HTTPServerConfiguration.DateTimeOffset.ContainsKey(CountryCode))
                                            CurrentDate = CurrentDate.AddDays(HTTPServerConfiguration.DateTimeOffset[CountryCode]);
                                        else if (HTTPServerConfiguration.DateTimeOffset != null && HTTPServerConfiguration.DateTimeOffset.ContainsKey(string.Empty))
                                            CurrentDate = CurrentDate.AddDays(HTTPServerConfiguration.DateTimeOffset.Where(entry => entry.Key == string.Empty).FirstOrDefault().Value);
                                    }
                                }
                                else if (HTTPServerConfiguration.DateTimeOffset != null && HTTPServerConfiguration.DateTimeOffset.ContainsKey(string.Empty))
                                    CurrentDate = CurrentDate.AddDays(HTTPServerConfiguration.DateTimeOffset.Where(entry => entry.Key == string.Empty).FirstOrDefault().Value);

#if DEBUG
                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport}{SuplementalMessage} Requested the HTTP Server with URL : {fullurl} (Details: \n" + JsonConvert.SerializeObject(request, Formatting.Indented) + ')');
#else
									LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport}{SuplementalMessage} Requested the HTTP Server with URL : {fullurl}");
#endif

                                string absolutepath = HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + HTTPProcessor.RemoveQueryString(fullurl);
                                string fulluripath = HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + fullurl;

                                if (HTTPServerConfiguration.RedirectRules != null)
                                {
                                    foreach (string? rule in HTTPServerConfiguration.RedirectRules)
                                    {
                                        if (!string.IsNullOrEmpty(rule) && rule.StartsWith("Redirect") && rule.Length >= 9) // Redirect + whitespace is minimum 9 in length.
                                        {
                                            string RouteRule = rule[8..];

                                            if (RouteRule.StartsWith("Match "))
                                            {
#if NET6_0
                                                Match match = new Regex(@"Match (\d{3}) (\S+) (\S+)$").Match(RouteRule);
#elif NET7_0_OR_GREATER
													Match match = ApacheMatchRegex().Match(RouteRule);
#endif
                                                if (match.Success && match.Groups.Count == 3)
                                                {
                                                    // Compare the regex rule against the test URL
                                                    if (Regex.IsMatch(absolutepath, match.Groups[2].Value))
                                                        response = HttpBuilder.RedirectFromApacheRules(match.Groups[3].Value, int.Parse(match.Groups[1].Value));
                                                }
                                            }
                                            else if (RouteRule.StartsWith("Permanent "))
                                            {
                                                string[] parts = RouteRule.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                                if (parts.Length == 3 && (parts[1] == "/" || parts[1] == absolutepath))
                                                    response = HttpBuilder.PermanantRedirect(parts[2]);
                                            }
                                            else if (RouteRule.StartsWith(' '))
                                            {
                                                string[] parts = RouteRule[1..].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                                if (parts.Length == 3 && (parts[1] == "/" || parts[1] == absolutepath))
                                                {
                                                    // Check if the input string contains an HTTP method
#if NET6_0
                                                    if (new Regex(@"^(GET|POST|PUT|DELETE|HEAD|OPTIONS|PATCH)").Match(parts[0]).Success && Method == parts[0])
#elif NET7_0_OR_GREATER
														if (HttpMethodRegex().Match(parts[0]).Success && Method == parts[0])
#endif
                                                        response = HttpBuilder.Found(parts[2]);
                                                    // Check if the input string contains a status code
#if NET6_0
                                                    else if (new Regex(@"\\b\\d{3}\\b").Match(parts[0]).Success && int.TryParse(parts[0], out int statuscode))
#elif NET7_0_OR_GREATER
														else if (HttpStatusCodeRegex().Match(parts[0]).Success && int.TryParse(parts[0], out int statuscode))
#endif
                                                        response = HttpBuilder.RedirectFromApacheRules(parts[2], statuscode);
                                                    else if (parts[1] == "permanent")
                                                        response = HttpBuilder.PermanantRedirect(parts[2]);
                                                }
                                                else if (parts.Length == 2 && (parts[0] == "/" || parts[0] == absolutepath))
                                                    response = HttpBuilder.Found(parts[1]);
                                            }
                                        }
                                    }
                                }

                                // Split the URL into segments
                                string[] segments = absolutepath.Trim('/').Split('/');

                                // Combine the folder segments into a directory path
                                string directoryPath = Path.Combine(!HTTPServerConfiguration.DomainFolder ? HTTPServerConfiguration.HTTPStaticFolder : HTTPServerConfiguration.HTTPStaticFolder + '/' + Host, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                                // Process the request based on the HTTP method
                                string filePath = Path.Combine(!HTTPServerConfiguration.DomainFolder ? HTTPServerConfiguration.HTTPStaticFolder : HTTPServerConfiguration.HTTPStaticFolder + '/' + Host, absolutepath[1..]);

                                //For HF to trim the url path out the combine, we don't need it for that api
                                string apiRootPath = HTTPServerConfiguration.APIStaticFolder;

                                string apiRootPathWithURIPath = Path.Combine(apiRootPath, absolutepath[1..]);

                                if (response == null && HTTPServerConfiguration.plugins.Count > 0)
                                {
                                    foreach (HTTPPlugin plugin in HTTPServerConfiguration.plugins.Values)
                                    {
                                        try
                                        {
                                            object? objReturn = plugin.ProcessPluginMessage(request);
                                            if (objReturn != null && objReturn is HttpResponse v)
                                                response = v;
                                            if (response != null)
                                                break;
                                        }
                                        catch (Exception ex)
                                        {
                                            LoggerAccessor.LogError($"[HTTP] - Plugin {plugin.GetHashCode()} thrown an assertion: {ex}");
                                        }
                                    }
                                }

                                response ??= RouteRequest(inputStream, outputStream, request, absolutepath, Host);

                                if (response == null)
                                {
                                    switch (Host)
                                    {
                                        default:

                                            #region Dust 514 dcrest
                                            if (request.ServerPort == 26004 //Check for Dust514 specific Port!!
                                                && !string.IsNullOrEmpty(Method)
                                                && request.RetrieveHeaderValue("X-CCP-User-Agent", true).Contains("CCPGamesCrestClient"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a Dust514  method : {absolutepath}");

                                                string? res = null;
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new Dust514Class(request.Method, absolutepath, HTTPServerConfiguration.APIStaticFolder).ProcessRequest(postdata.ToArray(), request.GetContentType(), request.Headers, false);
                                                    postdata.Flush();
                                                }
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "text/plain");
                                            }

                                            #endregion

                                            #region Outso OHS API
                                            else if ((Host == "stats.outso-srv1.com"
                                                || Host == "www.outso-srv1.com") &&
                                                request.GetDataStream != null &&
                                                (absolutepath.Contains("/ohs_") ||
                                                absolutepath.Contains("/ohs/") ||
                                                absolutepath.Contains("/statistic/") ||
                                                absolutepath.Contains("/Konami/") ||
                                                absolutepath.Contains("/tracker/")) && absolutepath.EndsWith("/"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a OHS method : {absolutepath}");

                                                string? res = null;

                                                #region OHS API Version
                                                int version = 0;
                                                if (absolutepath.Contains("/Insomniac/4BarrelsOfFury/"))
                                                    version = 2;
                                                else if (absolutepath.Contains("/SCEA/SaucerPop/"))
                                                    version = 2;
                                                else if (absolutepath.Contains("/AirRace/"))
                                                    version = 2;
                                                else if (absolutepath.Contains("/Flugtag/"))
                                                    version = 2;
                                                else if (absolutepath.Contains("/SCEA/op4_"))
                                                    version = 1;
                                                else if (absolutepath.Contains("/uncharted2"))
                                                    version = 1;
                                                else if (absolutepath.Contains("/Uncharted2"))
                                                    version = 1;
                                                else if (absolutepath.Contains("/Infamous/"))
                                                    version = 1;
                                                else if (absolutepath.Contains("/warhawk_shooter/"))
                                                    version = 1;
                                                #endregion

                                                using (MemoryStream postdata = new())
                                                {
                                                    request.GetDataStream?.CopyTo(postdata);
                                                    res = new OHSClass(Method, absolutepath, version).ProcessRequest(postdata.ToArray(), request.GetContentType(), apiRootPathWithURIPath);
                                                    postdata.Flush();
                                                }

                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send($"<ohs>{res}</ohs>", "application/xml;charset=UTF-8");
                                            }
                                            #endregion

                                            #region Outso OUWF Debug API
                                            else if (Host == "ouwf.outso-srv1.com"
                                                && request.GetDataStream != null
                                                && !string.IsNullOrEmpty(Method)
                                                && request.GetContentType().StartsWith("multipart/form-data"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip} Identified a OuWF method : {absolutepath}");

                                                string? res = null;
                                                using (MemoryStream postdata = new())
                                                {
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new OuWFClass(Method, absolutepath, HTTPServerConfiguration.HTTPStaticFolder).ProcessRequest(postdata.ToArray(), request.GetContentType());
                                                    postdata.Flush();
                                                }
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "text/xml");
                                            }
                                            #endregion

                                            #region Ubisoft Build API
                                            else if (Host == "builddatabasepullapi"
                                                && request.GetDataStream != null
                                                && !string.IsNullOrEmpty(Method)
                                                && request.GetContentType().StartsWith("application/soap+xml"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip} Identified a Ubisoft Build API method : {absolutepath}");

                                                string? res = null;
                                                using (MemoryStream postdata = new())
                                                {
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new SoapBuildAPIClass(Method, absolutepath, HTTPServerConfiguration.HTTPStaticFolder).ProcessRequest(postdata.ToArray(), request.GetContentType());
                                                    postdata.Flush();
                                                }
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "text/xml");
                                            }
                                            #endregion

                                            #region VEEMEE API
                                            else if ((Host == "away.veemee.com"
                                                || Host == "home.veemee.com"
                                                || Host == "ww-prod-sec.destinations.scea.com"
                                                || Host == "ww-prod.destinations.scea.com") &&
                                                !string.IsNullOrEmpty(Method) &&
                                                (absolutepath.EndsWith(".php") || absolutepath.EndsWith(".xml")))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a VEEMEE  method : {absolutepath}");

                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    (byte[]?, string?) res = new VEEMEEClass(Method, absolutepath).ProcessRequest(postdata.ToArray(), request.GetContentType(), HTTPServerConfiguration.APIStaticFolder);
                                                    postdata.Flush();

                                                    if (res.Item1 == null || res.Item1.Length == 0)
                                                        response = HttpBuilder.InternalServerError();
                                                    else
                                                    {
                                                        if (!string.IsNullOrEmpty(res.Item2))
                                                            response = HttpResponse.Send(res.Item1, res.Item2);
                                                        else
                                                            response = HttpResponse.Send(res.Item1, "text/plain");
                                                    }
                                                }
                                                else
                                                {
                                                    (byte[]?, string?) res = new VEEMEEClass(Method, absolutepath).ProcessRequest(null, request.GetContentType(), HTTPServerConfiguration.APIStaticFolder);

                                                    if (res.Item1 == null || res.Item1.Length == 0)
                                                        response = HttpBuilder.InternalServerError();
                                                    else
                                                    {
                                                        if (!string.IsNullOrEmpty(res.Item2))
                                                            response = HttpResponse.Send(res.Item1, res.Item2);
                                                        else
                                                            response = HttpResponse.Send(res.Item1, "text/plain");
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region nDreams API
                                            else if (nDreamsDomains.Contains(Host)
                                                && !string.IsNullOrEmpty(Method)
                                                && (absolutepath.EndsWith(".php")
                                                || absolutepath.Contains("/NDREAMS/")
                                                || absolutepath.Contains("/gateway/")))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a NDREAMS method : {absolutepath}");

                                                string? res = null;
                                                NDREAMSClass ndreams = new(CurrentDate, Method, apiRootPathWithURIPath, $"http://nDreams-multiserver-cdn/", $"http://{Host}{fullurl}", absolutepath, apiRootPath, Host);
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = ndreams.ProcessRequest(request.QueryParameters, postdata.ToArray(), request.GetContentType());
                                                    postdata.Flush();
                                                }
                                                else
                                                    res = ndreams.ProcessRequest(request.QueryParameters);

                                                ndreams.Dispose();
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "text/xml");
                                            }
                                            #endregion

                                            #region Hellfire Games API
                                            else if (HellFireGamesDomains.Contains(Host) && absolutepath.EndsWith(".php"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested a HELLFIRE method : {absolutepath}");

                                                string? res = string.Empty;

                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new HELLFIREClass(request.Method.ToString(), HTTPProcessor.RemoveQueryString(absolutepath), apiRootPath).ProcessRequest(postdata.ToArray(), request.GetContentType(), false);
                                                    postdata.Flush();
                                                }

                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                {
                                                    //response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                                    response = HttpResponse.Send(res, "application/xml;charset=UTF-8");
                                                }
                                            }
                                            #endregion

                                            #region Juggernaut Games API
                                            else if (Host == "juggernaut-games.com"
                                                && !string.IsNullOrEmpty(Method)
                                                && absolutepath.EndsWith(".php"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a JUGGERNAUT method : {absolutepath}");

                                                string? res = null;
                                                JUGGERNAUTClass juggernaut = new(Method, absolutepath);
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = juggernaut.ProcessRequest(request.QueryParameters, HTTPServerConfiguration.APIStaticFolder, postdata.ToArray(), request.GetContentType());
                                                    postdata.Flush();
                                                }
                                                else
                                                    res = juggernaut.ProcessRequest(request.QueryParameters, HTTPServerConfiguration.APIStaticFolder);
                                                juggernaut.Dispose();
                                                if (res == null)
                                                    response = HttpBuilder.InternalServerError();
                                                else if (res == string.Empty)
                                                    response = HttpBuilder.OK();
                                                else
                                                    response = HttpResponse.Send(res, "text/xml");
                                            }
                                            #endregion

                                            #region LOOT API
                                            else if ((Host == "server.lootgear.com"
                                                || Host == "alpha.lootgear.com")
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a LOOT method : {absolutepath}");

                                                string? res = null;
                                                LOOTClass loot = new(Method, absolutepath, apiRootPath);
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = loot.ProcessRequest(request.QueryParameters, postdata.ToArray(), request.GetContentType());
                                                    postdata.Flush();
                                                }
                                                else
                                                    res = loot.ProcessRequest(request.QueryParameters);

                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "application/xml;charset=UTF-8");
                                            }
                                            #endregion

                                            #region PREMIUMAGENCY API
                                            else if ((Host == "test.playstationhome.jp" ||
                                                Host == "playstationhome.jp" ||
                                                Host == "homeec.scej-nbs.jp" ||
                                                Host == "homeecqa.scej-nbs.jp" ||
                                                Host == "homect-scej.jp" ||
                                                Host == "qa-homect-scej.jp" ||
                                                Host == "home-eas.jp.playstation.com")
                                                && !string.IsNullOrEmpty(Method)
                                                && absolutepath.Contains("/eventController/"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a PREMIUMAGENCY method : {absolutepath}");

                                                string? res = null;
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new PREMIUMAGENCYClass(Method, absolutepath, apiRootPath, fulluripath).ProcessRequest(postdata.ToArray(), request.GetContentType());
                                                    postdata.Flush();
                                                }
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "text/xml");
                                            }
                                            #endregion

                                            #region FROMSOFTWARE API
                                            else if (Host == "acvd-ps3ww-cdn.fromsoftware.jp" && Method != null)
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a FROMSOFTWARE method : {absolutepath}");

                                                (byte[]?, string?, string[][]?) res = new();
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new FROMSOFTWAREClass(Method, absolutepath, apiRootPath).ProcessRequest(postdata.ToArray(), request.GetContentType());
                                                    postdata.Flush();
                                                }
                                                if (res.Item1 == null || string.IsNullOrEmpty(res.Item2) || res.Item3?.Length == 0)
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res.Item1, res.Item2, res.Item3);
                                            }
                                            #endregion

                                            #region Ubisoft API
                                            else if (Host.Contains("api-ubiservices.ubi.com")
                                                && UserAgent.Contains("UbiServices_SDK_HTTP_Client")
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a UBISOFT method : {absolutepath}");

                                                string Authorization = request.RetrieveHeaderValue("Authorization");

                                                if (!string.IsNullOrEmpty(Authorization))
                                                {
                                                    // TODO, verify ticket data for every platforms.

                                                    if (Authorization.StartsWith("psn t="))
                                                    {
                                                        (bool, byte[]) base64Data = Authorization.Replace("psn t=", string.Empty).IsBase64();

                                                        if (base64Data.Item1)
                                                        {
                                                            byte[] PSNTicket = base64Data.Item2;

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

                                                            if (ByteUtils.FindBytePattern(PSNTicket, new byte[] { 0x52, 0x50, 0x43, 0x4E }, 184) != -1)
                                                                LoggerAccessor.LogInfo($"[HERMES] : User {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on RPCN");
                                                            else
                                                                LoggerAccessor.LogInfo($"[HERMES] : {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on PSN");
                                                        }
                                                    }
                                                    else if (Authorization.StartsWith("Ubi_v1 t="))
                                                    {
                                                        // Our JWT token is fake for now.
                                                    }

                                                    if (request.GetDataStream != null)
                                                    {
                                                        using MemoryStream postdata = new();
                                                        request.GetDataStream.CopyTo(postdata);
                                                        (string?, string?) res = new HERMESClass(Method, absolutepath, request.RetrieveHeaderValue("Ubi-AppId"), request.RetrieveHeaderValue("Ubi-RequestedPlatformType"),
                                                            request.RetrieveHeaderValue("ubi-appbuildid"), clientip, GeoIP.GetISOCodeFromIP(IPAddress.Parse(clientip)), Authorization.Replace("psn t=", string.Empty), apiRootPath)
                                                            .ProcessRequest(postdata.ToArray(), request.GetContentType());
                                                        postdata.Flush();

                                                        if (string.IsNullOrEmpty(res.Item1))
                                                            response = HttpBuilder.InternalServerError();
                                                        else
                                                        {
                                                            if (!string.IsNullOrEmpty(res.Item2))
                                                                response = HttpResponse.Send(res.Item1, res.Item2);
                                                            else
                                                                response = HttpResponse.Send(res.Item1, "text/plain");

                                                            response.Headers.Add("Ubi-Forwarded-By", "ue1-p-us-public-nginx-056b582ac580ba328");
                                                            response.Headers.Add("Ubi-TransactionId", Guid.NewGuid().ToString());

                                                        }
                                                    }
                                                    else
                                                    {
                                                        (string?, string?) res = new HERMESClass(Method, absolutepath, request.RetrieveHeaderValue("Ubi-AppId"), request.RetrieveHeaderValue("Ubi-RequestedPlatformType"),
                                                            request.RetrieveHeaderValue("ubi-appbuildid"), clientip, GeoIP.GetISOCodeFromIP(IPAddress.Parse(clientip)), Authorization.Replace("psn t=", string.Empty), apiRootPath)
                                                            .ProcessRequest(null, request.GetContentType());

                                                        if (string.IsNullOrEmpty(res.Item1))
                                                            response = HttpBuilder.InternalServerError();
                                                        else
                                                        {
                                                            if (!string.IsNullOrEmpty(res.Item2))
                                                                response = HttpResponse.Send(res.Item1, res.Item2);
                                                            else
                                                                response = HttpResponse.Send(res.Item1, "text/plain");

                                                            response.Headers.Add("Ubi-Forwarded-By", "ue1-p-us-public-nginx-056b582ac580ba328");
                                                            response.Headers.Add("Ubi-TransactionId", Guid.NewGuid().ToString());

                                                        }
                                                    }
                                                }
                                                else
                                                    response = HttpBuilder.NotAllowed();
                                            }
                                            #endregion

                                            #region gsconnect API
                                            else if (Host == "gsconnect.ubisoft.com"
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a gsconnect method : {absolutepath}");

                                                (string?, string?, Dictionary<string, string>?) res;
                                                gsconnectClass gsconn = new(Method, absolutepath, apiRootPath);
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = gsconn.ProcessRequest(request.QueryParameters, postdata.ToArray(), request.GetContentType());
                                                    postdata.Flush();
                                                }
                                                else
                                                    res = gsconn.ProcessRequest(request.QueryParameters);

                                                if (string.IsNullOrEmpty(res.Item1) || string.IsNullOrEmpty(res.Item2))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                {
                                                    response = new("1.0")
                                                    {
                                                        HttpStatusCode = HttpStatusCode.OK
                                                    };
                                                    response.Headers["Content-Type"] = res.Item2;
                                                    response.ContentAsUTF8 = res.Item1;

                                                    if (res.Item3 != null)
                                                    {
                                                        foreach (KeyValuePair<string, string> headertoadd in res.Item3)
                                                        {
                                                            response.Headers.TryAdd(headertoadd.Key, headertoadd.Value);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region CentralDispatchManager API
                                            else if (HPDDomains.Contains(Host)
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a CentralDispatchManager method : {absolutepath}");

                                                string? res = null;

                                                if (Method == "POST")
                                                {
                                                    if (request.GetDataStream != null)
                                                    {
                                                        using MemoryStream postdata = new();
                                                        request.GetDataStream.CopyTo(postdata);
                                                        res = new CDMClass(request.Method, absolutepath, apiRootPath).ProcessRequest(postdata.ToArray(), request.GetContentType(), apiRootPathWithURIPath);
                                                        postdata.Flush();
                                                    }
                                                }
                                                else
                                                {

                                                    using (MemoryStream postdata = new())
                                                    {
                                                        res = new CDMClass(request.Method, absolutepath, apiRootPath).ProcessRequest(postdata.ToArray(), request.GetContentType(), apiRootPathWithURIPath);
                                                        postdata.Flush();
                                                    }
                                                }

                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "text/xml");
                                            }
                                            #endregion

                                            #region CAPONE GriefReporter API
                                            else if (CAPONEDomains.Contains(Host)
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a CAPONE method : {absolutepath}");

                                                string? res = null;
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new CAPONEClass(request.Method, absolutepath, apiRootPath).ProcessRequest(postdata.ToArray(), request.GetContentType(), false);
                                                    postdata.Flush();
                                                }
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "text/xml");
                                            }
                                            #endregion

                                            #region HTS Samples API
                                            else if (HTSDomains.Contains(Host)
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a HTS Samples method : {absolutepath}");

                                                string? res = null;
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new HTSClass(request.Method, absolutepath, apiRootPath).ProcessRequest(postdata.ToArray(), request.GetContentType(), false);
                                                    postdata.Flush();
                                                }
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "text/xml");
                                            }
                                            #endregion

                                            #region ILoveSony API
                                            else if (ILoveSonyDomains.Contains(Host)
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a IloveSony EULA method : {absolutepath}");

                                                string? res = null;
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new ILoveSonyClass(request.Method, absolutepath, apiRootPath).ProcessRequest(postdata.ToArray(), request.GetContentType(), false);
                                                    postdata.Flush();
                                                }
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "text/plain");
                                            }
                                            #endregion

                                            #region EA Demangler
                                            else if (Host.Contains("demangler.ea.com")
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a EA Demangler method : {absolutepath}");

                                                (string?, string?)? res = null;
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = DemanglerClass.ProcessDemanglerRequest(request.QueryParameters, absolutepath, clientip, postdata.ToArray());
                                                    postdata.Flush();
                                                }
                                                if (res == null)
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res.Value.Item1, res.Value.Item2!, new string[][] { new string[] { "x-envoy-upstream-service-time", "0" }, new string[] { "server", "istio-envoy" }
                                                        , new string[] { "content-length", res.Value.Item1!.Length.ToString() } }, HttpStatusCode.OK, true);
                                            }
                                            #endregion

                                            else
                                            {
                                                string? encoding = request.RetrieveHeaderValue("Accept-Encoding");

                                                switch (Method)
                                                {
                                                    case "GET":
                                                        switch (absolutepath)
                                                        {
                                                            #region PSN Network Test
                                                            case "/networktest/get_2m":
                                                                response = HttpResponse.Send(new byte[2097152]);
                                                                break;
                                                            #endregion

                                                            #region WebVideo Player
                                                            case "/!player":
                                                            case "/!player/":
                                                                string ServerIP = this.ServerIp;
                                                                if (ServerIP.Length > 15)
                                                                    ServerIP = "[" + ServerIP + "]"; // Format the hostname if it's a IPV6 url format.
                                                                WebVideoPlayer? WebPlayer = new((request.QueryParameters ?? new Dictionary<string, string>()).Aggregate(new NameValueCollection(),
                                                                    (seed, current) =>
                                                                    {
                                                                        seed.Add(current.Key, current.Value);
                                                                        return seed;
                                                                    }), $"http://{ServerIP}/!webvideo/?");
                                                                response = HttpResponse.Send(WebPlayer.HtmlPage, "text/html", WebPlayer.HeadersToSet);
                                                                WebPlayer = null;
                                                                break;
                                                            #endregion

                                                            #region WebVideo
                                                            case "/!webvideo":
                                                            case "/!webvideo/":
                                                                if (UserAgent.Contains("firefox", StringComparison.InvariantCultureIgnoreCase)
                                                                    || UserAgent.Contains("trident", StringComparison.InvariantCultureIgnoreCase)
                                                                    || UserAgent.Contains("chrome", StringComparison.InvariantCultureIgnoreCase))
                                                                {
                                                                    Dictionary<string, string>? QueryDic = request.QueryParameters;
                                                                    if (QueryDic != null && QueryDic.Count > 0 && QueryDic.TryGetValue("url", out string? value) && !string.IsNullOrEmpty(value))
                                                                    {
                                                                        WebVideo? vid = WebVideoConverter.ConvertVideo(QueryDic, HTTPServerConfiguration.ConvertersFolder);
                                                                        if (vid != null && vid.Available)
                                                                            response = HttpResponse.Send(vid.VideoStream, vid.ContentType, new string[][] { new string[] { "Content-Disposition", "attachment; filename=\"" + vid.FileName + "\"" } },
                                                                                HttpStatusCode.OK);
                                                                        else
                                                                            response = new HttpResponse()
                                                                            {
                                                                                HttpStatusCode = HttpStatusCode.OK,
                                                                                ContentAsUTF8 = "<p>" + vid?.ErrorMessage + "</p>" +
                                                                                        "<p>Make sure that parameters are correct, and both <i>yt-dlp</i> and <i>ffmpeg</i> are properly installed on the server.</p>",
                                                                                Headers = { { "Content-Type", "text/html" } }
                                                                            };
                                                                    }
                                                                    else
                                                                        response = new HttpResponse()
                                                                        {
                                                                            HttpStatusCode = HttpStatusCode.OK,
                                                                            ContentAsUTF8 = "<p>MultiServer can help download videos from popular sites in preferred format.</p>" +
                                                                                    "<p>Manual use parameters:" +
                                                                                    "<ul>" +
                                                                                    "<li><b>url</b> - Address of the video (e.g. https://www.youtube.com/watch?v=fPnO26CwqYU or similar)</li>" +
                                                                                    "<li><b>f</b> - Target format of the file (e.g. avi)</li>" +
                                                                                    "<li><b>vcodec</b> - Codec for video (e.g. mpeg4)</li>" +
                                                                                    "<li><b>acodec</b> - Codec for audio (e.g. mp3)</li>" +
                                                                                    "<li><b>content-type</b> - override MIME content type for the file (optional).</li>" +
                                                                                    "<li>Also you can use many <i>yt-dlp" +
                                                                                    "</i> and <i>ffmpeg" +
                                                                                    "</i> options like <b>aspect</b>, <b>b</b>, <b>no-mark-watched</b> and other.</li>" +
                                                                                    "</ul></p>",
                                                                            Headers = { { "Content-Type", "text/html" } }
                                                                        };
                                                                }
                                                                else
                                                                    response = HttpBuilder.NotAllowed();
                                                                break;
                                                            #endregion

                                                            default:
                                                                if ((absolutepath.EndsWith(".asp", StringComparison.InvariantCultureIgnoreCase) || absolutepath.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(HTTPServerConfiguration.ASPNETRedirectUrl))
                                                                    response = HttpBuilder.PermanantRedirect($"{HTTPServerConfiguration.ASPNETRedirectUrl}{fullurl}");
                                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(HTTPServerConfiguration.PHPRedirectUrl))
                                                                    response = HttpBuilder.PermanantRedirect($"{HTTPServerConfiguration.PHPRedirectUrl}{fullurl}");
                                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && Directory.Exists(HTTPServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                                                {
                                                                    (byte[]?, string[][]) CollectPHP = PHP.ProcessPHPPage(filePath, HTTPServerConfiguration.PHPStaticFolder, HTTPServerConfiguration.PHPVersion, request);
                                                                    if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(encoding) && CollectPHP.Item1 != null)
                                                                    {
                                                                        if (encoding.Contains("zstd"))
                                                                            response = HttpResponse.Send(HTTPProcessor.CompressZstd(CollectPHP.Item1), "text/html", CollectPHP.Item2.AddElementToLastPosition(new string[] { "Content-Encoding", "zstd" }));
                                                                        else if (encoding.Contains("br"))
                                                                            response = HttpResponse.Send(HTTPProcessor.CompressBrotli(CollectPHP.Item1), "text/html", CollectPHP.Item2.AddElementToLastPosition(new string[] { "Content-Encoding", "br" }));
                                                                        else if (encoding.Contains("gzip"))
                                                                            response = HttpResponse.Send(HTTPProcessor.CompressGzip(CollectPHP.Item1), "text/html", CollectPHP.Item2.AddElementToLastPosition(new string[] { "Content-Encoding", "gzip" }));
                                                                        else if (encoding.Contains("deflate"))
                                                                            response = HttpResponse.Send(HTTPProcessor.Inflate(CollectPHP.Item1), "text/html", CollectPHP.Item2.AddElementToLastPosition(new string[] { "Content-Encoding", "deflate" }));
                                                                        else
                                                                            response = HttpResponse.Send(CollectPHP.Item1, "text/html", CollectPHP.Item2);
                                                                    }
                                                                    else
                                                                        response = HttpResponse.Send(CollectPHP.Item1, "text/html", CollectPHP.Item2);
                                                                }
                                                                else
                                                                {
                                                                    bool fileExists = File.Exists(filePath);

                                                                    if (fileExists && request.Headers != null && request.Headers.Count(header => header.Key.Equals("Range")) == 1) // Mmm, is it possible to have more?
                                                                    {
                                                                        Handle_LocalFile_Stream(outputStream, request, filePath, UserAgent, AllowKeepAlive, noCompressCacheControl);
                                                                        continue;
                                                                    }
                                                                    else
                                                                    {
                                                                        (bool, HttpResponse) handleResponse = FileSystemRouteHandler.Handle(request, absolutepath, fullurl, filePath, UserAgent
                                                                            , Host, Accept, $"http://{request.ServerIP}:{request.ServerPort}{absolutepath[..^1]}", fileExists, true, noCompressCacheControl);
                                                                        EtagCompatible = handleResponse.Item1;
                                                                        response = handleResponse.Item2;
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                    case "POST":
                                                        switch (absolutepath)
                                                        {
                                                            #region PSN Network Test
                                                            case "/networktest/post_128":
                                                                response = HttpBuilder.OK();
                                                                break;
                                                            #endregion

                                                            default:
                                                                if ((absolutepath.EndsWith(".asp", StringComparison.InvariantCultureIgnoreCase) || absolutepath.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(HTTPServerConfiguration.ASPNETRedirectUrl))
                                                                    response = HttpBuilder.PermanantRedirect($"{HTTPServerConfiguration.ASPNETRedirectUrl}{fullurl}");
                                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(HTTPServerConfiguration.PHPRedirectUrl))
                                                                    response = HttpBuilder.PermanantRedirect($"{HTTPServerConfiguration.PHPRedirectUrl}{fullurl}");
                                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && Directory.Exists(HTTPServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                                                {
                                                                    var CollectPHP = PHP.ProcessPHPPage(filePath, HTTPServerConfiguration.PHPStaticFolder, HTTPServerConfiguration.PHPVersion, request);
                                                                    if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(encoding) && CollectPHP.Item1 != null)
                                                                    {
                                                                        if (encoding.Contains("zstd"))
                                                                            response = HttpResponse.Send(HTTPProcessor.CompressZstd(CollectPHP.Item1), "text/html", CollectPHP.Item2.AddElementToLastPosition(new string[] { "Content-Encoding", "zstd" }));
                                                                        else if (encoding.Contains("br"))
                                                                            response = HttpResponse.Send(HTTPProcessor.CompressBrotli(CollectPHP.Item1), "text/html", CollectPHP.Item2.AddElementToLastPosition(new string[] { "Content-Encoding", "br" }));
                                                                        else if (encoding.Contains("gzip"))
                                                                            response = HttpResponse.Send(HTTPProcessor.CompressGzip(CollectPHP.Item1), "text/html", CollectPHP.Item2.AddElementToLastPosition(new string[] { "Content-Encoding", "gzip" }));
                                                                        else if (encoding.Contains("deflate"))
                                                                            response = HttpResponse.Send(HTTPProcessor.Inflate(CollectPHP.Item1), "text/html", CollectPHP.Item2.AddElementToLastPosition(new string[] { "Content-Encoding", "deflate" }));
                                                                        else
                                                                            response = HttpResponse.Send(CollectPHP.Item1, "text/html", CollectPHP.Item2);
                                                                    }
                                                                    else
                                                                        response = HttpResponse.Send(CollectPHP.Item1, "text/html", CollectPHP.Item2);
                                                                }
                                                                else
                                                                {
                                                                    bool fileExists = File.Exists(filePath);

                                                                    if (fileExists && request.Headers != null && request.Headers.Count(header => header.Key.Equals("Range")) == 1) // Mmm, is it possible to have more?
                                                                    {
                                                                        Handle_LocalFile_Stream(outputStream, request, filePath, UserAgent, AllowKeepAlive, noCompressCacheControl);
                                                                        continue;
                                                                    }
                                                                    else
                                                                    {
                                                                        (bool, HttpResponse) handleResponse = FileSystemRouteHandler.Handle(request, absolutepath, fullurl, filePath, UserAgent
                                                                            , Host, Accept, $"http://{request.ServerIP}:{request.ServerPort}{absolutepath[..^1]}", fileExists, false, noCompressCacheControl);
                                                                        EtagCompatible = handleResponse.Item1;
                                                                        response = handleResponse.Item2;
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                    case "PUT":
                                                        if (HTTPServerConfiguration.EnablePUTMethod)
                                                        {
                                                            string ContentType = request.GetContentType();
                                                            if (request.GetDataStream != null && !string.IsNullOrEmpty(ContentType))
                                                            {
                                                                string? boundary = HTTPProcessor.ExtractBoundary(ContentType);
                                                                if (!string.IsNullOrEmpty(boundary))
                                                                {
                                                                    string UploadDirectoryPath = HTTPServerConfiguration.HTTPTempFolder + $"/DataUpload/{absolutepath[1..]}";
                                                                    Directory.CreateDirectory(UploadDirectoryPath);
                                                                    foreach (FilePart? multipartfile in MultipartFormDataParser.Parse(request.GetDataStream, boundary).Files)
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

                                                                    response = HttpBuilder.OK();
                                                                }
                                                                else
                                                                    response = HttpBuilder.BadRequest();
                                                            }
                                                            else
                                                                response = HttpBuilder.BadRequest();
                                                        }
                                                        else
                                                            response = HttpBuilder.NotAllowed();
                                                        break;
                                                    case "DELETE":
                                                        response = HttpBuilder.NotAllowed();
                                                        break;
                                                    case "HEAD":
                                                        switch (absolutepath)
                                                        {
                                                            #region WebVideo
                                                            case "/!webvideo":
                                                            case "/!webvideo/":
                                                                if (UserAgent.Contains("firefox", StringComparison.InvariantCultureIgnoreCase)
                                                                    || UserAgent.Contains("trident", StringComparison.InvariantCultureIgnoreCase)
                                                                    || UserAgent.Contains("chrome", StringComparison.InvariantCultureIgnoreCase))
                                                                {
                                                                    Dictionary<string, string>? QueryDic = request.QueryParameters;
                                                                    if (QueryDic != null && QueryDic.Count > 0 && QueryDic.TryGetValue("url", out string? value) && !string.IsNullOrEmpty(value))
                                                                    {
                                                                        WebVideo? vid = WebVideoConverter.ConvertVideo(QueryDic, HTTPServerConfiguration.ConvertersFolder);
                                                                        if (vid != null && vid.Available && vid.VideoStream != null)
                                                                        {
                                                                            using HugeMemoryStream ms = new(vid.VideoStream, HTTPServerConfiguration.BufferSize);
                                                                            response = new()
                                                                            {
                                                                                HttpStatusCode = HttpStatusCode.OK,
                                                                                ContentAsUTF8 = string.Empty
                                                                            };
                                                                            response.Headers.Add("Content-Type", vid.ContentType);
                                                                            response.Headers.Add("Content-Length", ms.Length.ToString());
                                                                            ms.Flush();
                                                                        }
                                                                        else
                                                                            response = HttpBuilder.InternalServerError();
                                                                    }
                                                                    else
                                                                        response = HttpBuilder.BadRequest();
                                                                }
                                                                else
                                                                    response = HttpBuilder.NotAllowed();
                                                                break;
                                                            #endregion

                                                            default:
                                                                response = FileSystemRouteHandler.HandleHEAD(request, absolutepath, filePath, Host, Accept);
                                                                break;
                                                        }
                                                        break;
                                                    case "OPTIONS":
                                                        response = HttpBuilder.OK();
                                                        response.Headers.Add("Allow", "OPTIONS, GET, HEAD, POST, PROPFIND");
                                                        break;
                                                    case "PROPFIND":
                                                        if (File.Exists(filePath))
                                                        {
                                                            string ServerIP = this.ServerIp;
                                                            if (ServerIP.Length > 15)
                                                                ServerIP = "[" + ServerIP + "]"; // Format the hostname if it's a IPV6 url format.

                                                            string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(filePath), HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                                                            if (ContentType == "application/octet-stream")
                                                            {
                                                                byte[] VerificationChunck = FileSystemUtils.ReadFileChunck(filePath, 10);
                                                                foreach (var entry in HTTPProcessor._PathernDictionary)
                                                                {
                                                                    if (ByteUtils.FindBytePattern(VerificationChunck, entry.Value) != -1)
                                                                    {
                                                                        ContentType = entry.Key;
                                                                        break;
                                                                    }
                                                                }
                                                            }

                                                            response = HttpResponse.Send("<?xml version=\"1.0\"?>\r\n" +
                                                                "<a:multistatus\r\n" +
                                                                $"  xmlns:b=\"urn:uuid:{Guid.NewGuid()}/\"\r\n" +
                                                                "  xmlns:a=\"DAV:\">\r\n" +
                                                                " <a:response>\r\n" +
                                                                $"   <a:href>http://{ServerIP}:{request.ServerPort}{absolutepath}</a:href>\r\n" +
                                                                "   <a:propstat>\r\n" +
                                                                "    <a:status>HTTP/1.1 200 OK</a:status>\r\n" +
                                                                "       <a:prop>\r\n" +
                                                                $"        <a:getcontenttype>{ContentType}</a:getcontenttype>\r\n" +
                                                                $"        <a:getcontentlength b:dt=\"int\">{new FileInfo(filePath).Length}</a:getcontentlength>\r\n" +
                                                                "       </a:prop>\r\n" +
                                                                "   </a:propstat>\r\n" +
                                                                " </a:response>\r\n" +
                                                                "</a:multistatus>", "text/xml", null, HttpStatusCode.MultiStatus);
                                                        }
                                                        else
                                                            response = HttpBuilder.NotFound(request, absolutepath, Host, !string.IsNullOrEmpty(Accept) && Accept.Contains("html"));
                                                        break;
                                                    default:
                                                        response = HttpBuilder.NotAllowed();
                                                        break;
                                                }
                                            }
                                            break;
                                    }
                                }

                                WriteResponse(outputStream, request, response, filePath, AllowKeepAlive, EtagCompatible);
                            }
                        }
                    }
                    outputStream.Flush();
                }
                inputStream.Flush();
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException socketException && socketException.ErrorCode != 995 &&
                    socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted
                    && socketException.SocketErrorCode != SocketError.ConnectionRefused)
                    LoggerAccessor.LogError($"[HTTP] - HandleClient - IO-Socket thrown an exception : {ex}");
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 995 && ex.SocketErrorCode != SocketError.ConnectionReset && ex.SocketErrorCode != SocketError.ConnectionAborted && ex.SocketErrorCode != SocketError.ConnectionRefused)
                    LoggerAccessor.LogError($"[HTTP] - HandleClient - Socket thrown an exception : {ex}");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[HTTP] - HandleClient thrown an exception : {ex}");
            }
            finally
            {
                if (IsInterlocked)
                    Interlocked.Decrement(ref KeepAliveClients);

                request?.Dispose();
                response?.Dispose();
            }
        }

        public void AddRoute(Route route)
        {
            if (!Routes.Contains(route))
                Routes.Add(route);
        }

#endregion

        #region Private Methods

        private static string Readline(Stream stream)
        {
            int next_char;
            string data = string.Empty;
            while (true)
            {
                next_char = stream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        private static void WriteResponse(Stream stream, HttpRequest request, HttpResponse response, string? filePath, bool AllowKeepAlive, bool EtagCompatible)
        {
            try
            {
                bool hasFilePath = !string.IsNullOrEmpty(filePath);

                if (response.ContentStream != null)
                {
                    bool KeepAlive = AllowKeepAlive && request.RetrieveHeaderValue("Connection").Equals("keep-alive");
                    string NoneMatch = EtagCompatible ? request.RetrieveHeaderValue("If-None-Match") : string.Empty;
                    string? EtagMD5 = EtagCompatible ? HTTPProcessor.ComputeStreamMD5(response.ContentStream) : null;
                    bool isNoneMatchValid = EtagCompatible && !string.IsNullOrEmpty(NoneMatch) && NoneMatch.Equals(EtagMD5);
                    bool isModifiedSinceValid = hasFilePath && EtagCompatible && HTTPProcessor.CheckLastWriteTime(filePath, request.RetrieveHeaderValue("If-Modified-Since"));

                    if (EtagCompatible && ((isNoneMatchValid && isModifiedSinceValid) ||
                        (isNoneMatchValid && string.IsNullOrEmpty(request.RetrieveHeaderValue("If-Modified-Since"))) ||
                        (isModifiedSinceValid && string.IsNullOrEmpty(NoneMatch))))
                    {
                        response.Headers.Clear();

                        if (!response.Headers.Keys.Any(key => key.Equals("server", StringComparison.OrdinalIgnoreCase)))
                            response.Headers.Add("Server", "Apache");

                        if (KeepAlive)
                        {
                            response.Headers.Add("Connection", "Keep-Alive");
                            response.Headers.Add("Keep-Alive", "timeout=15, max=98");
                        }
                        else
                            response.Headers.Add("Connection", "close");

                        if (!string.IsNullOrEmpty(EtagMD5))
                        {
                            response.Headers.Add("ETag", EtagMD5);
                            response.Headers.Add("Expires", DateTime.Now.AddMinutes(30).ToString("r"));
                        }

                        response.HttpStatusCode = HttpStatusCode.NotModified;

                        WriteLineToStream(stream, response.ToHeader());

                        stream.Flush();
                    }
                    else
                    {
                        int bufferSize = HTTPServerConfiguration.BufferSize;
                        long totalBytes = response.ContentStream.Length;
                        string? encoding = null;

                        if (!response.Headers.Keys.Any(key => key.Equals("server", StringComparison.OrdinalIgnoreCase)))
                            response.Headers.Add("Server", "Apache");

                        if (KeepAlive)
                        {
                            response.Headers.Add("Connection", "Keep-Alive");
                            response.Headers.Add("Keep-Alive", "timeout=15, max=98");
                        }
                        else
                            response.Headers.Add("Connection", "close");

                        response.Headers.Add("Access-Control-Allow-Origin", "*");
                        response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, HEAD, GET, PUT, POST, PROPFIND");
                        response.Headers.Add("Access-Control-Allow-Headers", "*");
                        response.Headers.Add("Access-Control-Expose-Headers", string.Empty);

                        if (!response.Headers.Keys.Any(key => key.Equals("content-type", StringComparison.OrdinalIgnoreCase)))
                            response.Headers.Add("Content-Type", "text/plain");

                        if (response.HttpStatusCode == HttpStatusCode.OK)
                        {
                            response.Headers.TryAdd("Date", DateTime.Now.ToString("r"));
                            if (!string.IsNullOrEmpty(EtagMD5))
                            {
                                response.Headers.Add("ETag", EtagMD5);
                                response.Headers.Add("Expires", DateTime.Now.AddMinutes(30).ToString("r"));
                            }
                            if (hasFilePath)
                                response.Headers.TryAdd("Last-Modified", File.GetLastWriteTime(filePath!).ToString("r"));
                        }

                        if (!response.Headers.Keys.Any(key => key.Equals("content-length", StringComparison.OrdinalIgnoreCase)))
                        {
                            if (response.Headers.TryGetValue("Transfer-Encoding", out encoding) && !string.IsNullOrEmpty(encoding) && encoding.Contains("chunked"))
                            {

                            }
                            else
                                response.Headers.Add("Content-Length", totalBytes.ToString());
                        }

                        WriteLineToStream(stream, response.ToHeader());

                        stream.Flush();

                        // We override the bufferSize for large content, else, we monster the CPU.
                        if (totalBytes > 8000000 && bufferSize < 500000)
                            bufferSize = 500000;

                        using (HttpResponseContentStream ctwire = new(stream, response.Headers.ContainsKey("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked")))
                        {
                            HTTPProcessor.CopyStream(response.ContentStream, ctwire, bufferSize, totalBytes, false);

                            ctwire.WriteTerminator();

                            ctwire.Flush();
                        }
                    }
                }
                else
                {
                    response.Headers.Clear();

                    if (!response.Headers.Keys.Any(key => key.Equals("server", StringComparison.OrdinalIgnoreCase)))
                            response.Headers.Add("Server", "Apache");
                    response.Headers.Add("Connection", "close");

                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    WriteLineToStream(stream, response.ToHeader());

                    stream.Flush();
                }

                if ((int)response.HttpStatusCode < 400)
                    LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.RawUrlWithQuery, response.HttpStatusCode));
                else
                {
                    if (response.HttpStatusCode == HttpStatusCode.NotFound)
                        LoggerAccessor.LogWarn(string.Format("[HTTP] - {0}:{1} Requested a non-existant file: {2} -> {3}", request.IP, request.Port, hasFilePath ? filePath : "Invalid Path", response.HttpStatusCode));
                    else if (response.HttpStatusCode == HttpStatusCode.NotImplemented || response.HttpStatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
                        LoggerAccessor.LogWarn(string.Format("{0} -> {1}", request.RawUrlWithQuery, response.HttpStatusCode));
                    else
                        LoggerAccessor.LogError(string.Format("{0} -> {1}", request.RawUrlWithQuery, response.HttpStatusCode));
                }
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException socketException &&
                    socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted)
                    LoggerAccessor.LogError($"[HTTP] - WriteResponse - IO-Socket thrown an exception : {ex}");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[HTTP] - WriteResponse thrown an assertion : {ex}");
            }

            response.Dispose();
        }

        private static void Handle_LocalFile_Stream(Stream stream, HttpRequest request, string filePath, string UserAgent, bool AllowKeepAlive, bool noCompressCacheControl)
        {
            // This method directly communicate with the wire to handle, normally, imposible transfers.
            // If a part of the code sounds weird to you, it's normal... So does curl tests...

            HttpResponse? response = null;

            try
            {
                string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(filePath), HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);

                if (HTTPProcessor.CheckLastWriteTime(filePath, request.RetrieveHeaderValue("If-Unmodified-Since"), true))
                    response = new()
                    {
                        HttpStatusCode = HttpStatusCode.PreconditionFailed,
                        ContentAsUTF8 = string.Empty
                    };
                else
                {
                    const int rangebuffersize = 32768;

                    string? acceptencoding = request.RetrieveHeaderValue("Accept-Encoding");

                    using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    long startByte = -1;
                    long endByte = -1;
                    long filesize = fs.Length;
                    string HeaderString = request.RetrieveHeaderValue("Range").Replace("bytes=", string.Empty);
                    if (HeaderString.Contains(','))
                    {
                        using HugeMemoryStream ms = new();
                        int bufferSize = HTTPServerConfiguration.BufferSize;
                        Span<byte> Separator = new byte[] { 0x0D, 0x0A };
                        if (ContentType == "application/octet-stream")
                        {
                            byte[] VerificationChunck = FileSystemUtils.ReadFileChunck(filePath, 10);
                            foreach (var entry in HTTPProcessor._PathernDictionary)
                            {
                                if (ByteUtils.FindBytePattern(VerificationChunck, entry.Value) != -1)
                                {
                                    ContentType = entry.Key;
                                    break;
                                }
                            }
                        }
                        // Split the ranges based on the comma (',') separator
                        foreach (string RangeSelect in HeaderString.Split(','))
                        {
                            ms.Write(Separator);
                            ms.Write(Encoding.UTF8.GetBytes("--multiserver_separator").AsSpan());
                            ms.Write(Separator);
                            ms.Write(Encoding.UTF8.GetBytes($"Content-Type: {ContentType}").AsSpan());
                            ms.Write(Separator);
                            fs.Position = 0;
                            startByte = -1;
                            endByte = -1;
                            string[] range = RangeSelect.Split('-');
                            if (range[0].Trim().Length > 0) _ = long.TryParse(range[0], out startByte);
                            if (range[1].Trim().Length > 0) _ = long.TryParse(range[1], out endByte);
                            if (endByte == -1) endByte = filesize;
                            else if (endByte != filesize) endByte++;
                            if (startByte == -1)
                            {
                                startByte = filesize - endByte;
                                endByte = filesize;
                            }
                            if (endByte > filesize) // Curl test showed this behaviour.
                                endByte = filesize;
                            if (startByte >= filesize && endByte == filesize) // Curl test showed this behaviour.
                            {
                                string payload = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n" +
                                    "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\r\n" +
                                    "         \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n" +
                                    "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\r\n" +
                                    "        <head>\r\n" +
                                    "                <title>416 - Requested Range Not Satisfiable</title>\r\n" +
                                    "        </head>\r\n" +
                                    "        <body>\r\n" +
                                    "                <h1>416 - Requested Range Not Satisfiable</h1>\r\n" +
                                    "        </body>\r\n" +
                                    "</html>";

                                ms.Flush();
                                ms.Close();
                                response = new()
                                {
                                    HttpStatusCode = HttpStatusCode.RequestedRangeNotSatisfiable
                                };
                                response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                                response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                                if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(acceptencoding))
                                {
                                    if (acceptencoding.Contains("zstd"))
                                    {
                                        response.Headers.Add("Content-Encoding", "zstd");
                                        response.ContentStream = new MemoryStream(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(payload)));
                                    }
                                    else if (acceptencoding.Contains("br"))
                                    {
                                        response.Headers.Add("Content-Encoding", "br");
                                        response.ContentStream = new MemoryStream(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(payload)));
                                    }
                                    else if (acceptencoding.Contains("gzip"))
                                    {
                                        response.Headers.Add("Content-Encoding", "gzip");
                                        response.ContentStream = new MemoryStream(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(payload)));
                                    }
                                    else if (acceptencoding.Contains("deflate"))
                                    {
                                        response.Headers.Add("Content-Encoding", "deflate");
                                        response.ContentStream = new MemoryStream(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(payload)));
                                    }
                                    else
                                        response.ContentAsUTF8 = payload;
                                }
                                else
                                    response.ContentAsUTF8 = payload;

                                goto shortcut; // Do we really have the choice?
                            }
                            else if (startByte >= endByte || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                            {
                                ms.Flush();
                                ms.Close();
                                // Hotfix PSHome videos not being displayed in HTTP using chunck encoding (game bug).
                                response = new(null, !string.IsNullOrEmpty(UserAgent) && UserAgent.Contains("PSHome")
                                    && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                                {
                                    HttpStatusCode = HttpStatusCode.OK
                                };
                                response.Headers.Add("Accept-Ranges", "bytes");
                                response.Headers.Add("Content-Type", ContentType);

                                long FileLength = new FileInfo(filePath).Length;

                                if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(acceptencoding)
                                    && (ContentType.StartsWith("text/") || ContentType.StartsWith("application/") || ContentType.StartsWith("font/")
                                    || ContentType == "image/svg+xml" || ContentType == "image/x-icon"))
                                {
                                    if (acceptencoding.Contains("zstd"))
                                    {
                                        response.Headers.Add("Content-Encoding", "zstd");
                                        response.ContentStream = HTTPProcessor.ZstdCompressStream(File.OpenRead(filePath));
                                    }
                                    else if (acceptencoding.Contains("br"))
                                    {
                                        response.Headers.Add("Content-Encoding", "br");
                                        response.ContentStream = HTTPProcessor.BrotliCompressStream(File.OpenRead(filePath));
                                    }
                                    else if (acceptencoding.Contains("gzip"))
                                    {
                                        response.Headers.Add("Content-Encoding", "gzip");
                                        response.ContentStream = HTTPProcessor.GzipCompressStream(File.OpenRead(filePath));
                                    }
                                    else if (acceptencoding.Contains("deflate"))
                                    {
                                        response.Headers.Add("Content-Encoding", "deflate");
                                        response.ContentStream = HTTPProcessor.InflateStream(File.OpenRead(filePath));
                                    }
                                    else
                                        response.ContentStream = File.OpenRead(filePath);
                                }
                                else
                                    response.ContentStream = File.OpenRead(filePath);

                                goto shortcut; // Do we really have the choice?
                            }
                            else
                            {
                                int bytesRead = 0;
                                long TotalBytes = endByte - startByte;
                                long totalBytesCopied = 0;
                                byte[] buffer = new byte[rangebuffersize];
                                fs.Position = startByte;
                                ms.Write(Encoding.UTF8.GetBytes("Content-Range: " + string.Format("bytes {0}-{1}/{2}", startByte, endByte - 1, filesize)).AsSpan());
                                ms.Write(Separator);
                                ms.Write(Separator);
                                while (totalBytesCopied < TotalBytes && (bytesRead = fs.Read(buffer, 0, rangebuffersize)) > 0)
                                {
                                    int bytesToWrite = (int)Math.Min(TotalBytes - totalBytesCopied, bytesRead);
                                    ms.Write(buffer, 0, bytesToWrite);
                                    totalBytesCopied += bytesToWrite;
                                }
                            }
                        }
                        ms.Write(Separator);
                        ms.Write(Encoding.UTF8.GetBytes("--multiserver_separator--").AsSpan());
                        ms.Write(Separator);
                        ms.Position = 0;
                        // Hotfix PSHome videos not being displayed in HTTP using chunck encoding (game bug).
                        response = new(null, !string.IsNullOrEmpty(UserAgent) && UserAgent.Contains("PSHome")
                            && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                        {
                            HttpStatusCode = HttpStatusCode.PartialContent
                        };
                        response.Headers.Add("Server", "Apache");
                        response.Headers.Add("Content-Type", "multipart/byteranges; boundary=multiserver_separator");
                        response.Headers.Add("Accept-Ranges", "bytes");
                        response.Headers.Add("Access-Control-Allow-Origin", "*");
                        response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, HEAD, GET, PUT, POST, PROPFIND");
                        response.Headers.Add("Access-Control-Allow-Headers", "*");
                        response.Headers.Add("Access-Control-Expose-Headers", string.Empty);
                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));

                        string? encoding = null;

                        if (!response.Headers.Keys.Any(key => key.Equals("content-length", StringComparison.OrdinalIgnoreCase)))
                        {
                            if (response.Headers.TryGetValue("Transfer-Encoding", out encoding) && !string.IsNullOrEmpty(encoding) && encoding.Contains("chunked"))
                            {

                            }
                            else
                                response.Headers.Add("Content-Length", ms.Length.ToString());
                        }

                        WriteLineToStream(stream, response.ToHeader());

                        stream.Flush();

                        long totalBytes = ms.Length;

                        // We override the bufferSize for large content, else, we monster the CPU.
                        if (totalBytes > 8000000 && bufferSize < 500000)
                            bufferSize = 500000;

                        using (HttpResponseContentStream ctwire = new(stream, response.Headers.ContainsKey("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked")))
                        {
                            HTTPProcessor.CopyStream(ms, ctwire, bufferSize, totalBytes, false);

                            ctwire.WriteTerminator();

                            ctwire.Flush();
                        }

                        ms.Flush();
                        ms.Close();

                        LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.RawUrlWithQuery, HttpStatusCode.PartialContent));

                        response.Dispose();

                        fs.Flush();
                        fs.Close();

                        return;
                    }
                    else
                    {
                        string[] range = HeaderString.Split('-');
                        if (range[0].Trim().Length > 0) _ = long.TryParse(range[0], out startByte);
                        if (range[1].Trim().Length > 0) _ = long.TryParse(range[1], out endByte);
                        if (endByte == -1) endByte = filesize;
                        else if (endByte != filesize) endByte++;
                        if (startByte == -1)
                        {
                            startByte = filesize - endByte;
                            endByte = filesize;
                        }
                    }
                    if (endByte > filesize) // Curl test showed this behaviour.
                        endByte = filesize;
                    if (startByte >= filesize && endByte == filesize) // Curl test showed this behaviour.
                    {
                        string payload = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n" +
                            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\r\n" +
                            "         \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n" +
                            "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\r\n" +
                            "        <head>\r\n" +
                            "                <title>416 - Requested Range Not Satisfiable</title>\r\n" +
                            "        </head>\r\n" +
                            "        <body>\r\n" +
                            "                <h1>416 - Requested Range Not Satisfiable</h1>\r\n" +
                            "        </body>\r\n" +
                            "</html>";

                        response = new()
                        {
                            HttpStatusCode = HttpStatusCode.RequestedRangeNotSatisfiable
                        };
                        response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                        response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                        if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(acceptencoding))
                        {
                            if (acceptencoding.Contains("zstd"))
                            {
                                response.Headers.Add("Content-Encoding", "zstd");
                                response.ContentStream = new MemoryStream(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(payload)));
                            }
                            else if (acceptencoding.Contains("br"))
                            {
                                response.Headers.Add("Content-Encoding", "br");
                                response.ContentStream = new MemoryStream(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(payload)));
                            }
                            else if (acceptencoding.Contains("gzip"))
                            {
                                response.Headers.Add("Content-Encoding", "gzip");
                                response.ContentStream = new MemoryStream(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(payload)));
                            }
                            else if (acceptencoding.Contains("deflate"))
                            {
                                response.Headers.Add("Content-Encoding", "deflate");
                                response.ContentStream = new MemoryStream(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(payload)));
                            }
                            else
                                response.ContentAsUTF8 = payload;
                        }
                        else
                            response.ContentAsUTF8 = payload;
                    }
                    else if (startByte >= endByte || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                    {
                        if (ContentType == "application/octet-stream")
                        {
                            byte[] VerificationChunck = FileSystemUtils.ReadFileChunck(filePath, 10);
                            foreach (var entry in HTTPProcessor._PathernDictionary)
                            {
                                if (ByteUtils.FindBytePattern(VerificationChunck, entry.Value) != -1)
                                {
                                    ContentType = entry.Key;
                                    break;
                                }
                            }
                        }
                        // Hotfix PSHome videos not being displayed in HTTP using chunck encoding (game bug).
                        response = new(null, !string.IsNullOrEmpty(UserAgent) && UserAgent.Contains("PSHome")
                            && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                        {
                                HttpStatusCode = HttpStatusCode.OK
                        };
                        response.Headers.Add("Accept-Ranges", "bytes");
                        response.Headers.Add("Content-Type", ContentType);

                        long FileLength = new FileInfo(filePath).Length;

                        if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(acceptencoding)
                            && (ContentType.StartsWith("text/") || ContentType.StartsWith("application/") || ContentType.StartsWith("font/")
                                    || ContentType == "image/svg+xml" || ContentType == "image/x-icon"))
                        {
                            if (acceptencoding.Contains("zstd"))
                            {
                                response.Headers.Add("Content-Encoding", "zstd");
                                response.ContentStream = HTTPProcessor.ZstdCompressStream(File.OpenRead(filePath));
                            }
                            else if (acceptencoding.Contains("br"))
                            {
                                response.Headers.Add("Content-Encoding", "br");
                                response.ContentStream = HTTPProcessor.BrotliCompressStream(File.OpenRead(filePath));
                            }
                            else if (acceptencoding.Contains("gzip"))
                            {
                                response.Headers.Add("Content-Encoding", "gzip");
                                response.ContentStream = HTTPProcessor.GzipCompressStream(File.OpenRead(filePath));
                            }
                            else if (acceptencoding.Contains("deflate"))
                            {
                                response.Headers.Add("Content-Encoding", "deflate");
                                response.ContentStream = HTTPProcessor.InflateStream(File.OpenRead(filePath));
                            }
                            else
                                response.ContentStream = File.OpenRead(filePath);
                        }
                        else
                            response.ContentStream = File.OpenRead(filePath);
                    }
                    else
                    {
                        int bufferSize = HTTPServerConfiguration.BufferSize;
                        long totalBytes = endByte - startByte;
                        string? encoding = null;

                        fs.Position = startByte;

                        if (ContentType == "application/octet-stream")
                        {
                            foreach (var entry in HTTPProcessor._PathernDictionary)
                            {
                                if (ByteUtils.FindBytePattern(FileSystemUtils.ReadFileChunck(filePath, 10), entry.Value) != -1)
                                {
                                    ContentType = entry.Key;
                                    break;
                                }
                            }
                        }
                        // Hotfix PSHome videos not being displayed in HTTP using chunck encoding (game bug).
                        response = new(null, !string.IsNullOrEmpty(UserAgent) && UserAgent.Contains("PSHome")
                            && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                            {
                                HttpStatusCode = HttpStatusCode.PartialContent
                            };
                        response.Headers.Add("Server", "Apache");
                        response.Headers.Add("Content-Type", ContentType);
                        response.Headers.Add("Accept-Ranges", "bytes");
                        response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", startByte, endByte - 1, filesize));
                        response.Headers.Add("Access-Control-Allow-Origin", "*");
                        response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, HEAD, GET, PUT, POST, PROPFIND");
                        response.Headers.Add("Access-Control-Allow-Headers", "*");
                        response.Headers.Add("Access-Control-Expose-Headers", string.Empty);
                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));

                        if (!response.Headers.Keys.Any(key => key.Equals("content-length", StringComparison.OrdinalIgnoreCase)))
                        {
                            if (response.Headers.TryGetValue("Transfer-Encoding", out encoding) && !string.IsNullOrEmpty(encoding) && encoding.Contains("chunked"))
                            {

                            }
                            else
                                response.Headers.Add("Content-Length", totalBytes.ToString());
                        }

                        WriteLineToStream(stream, response.ToHeader());

                        stream.Flush();

                        // We override the bufferSize for large content, else, we monster the CPU.
                        if (totalBytes > 8000000 && bufferSize < 500000)
                            bufferSize = 500000;

                        using (HttpResponseContentStream ctwire = new(stream, response.Headers.ContainsKey("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked")))
                        {
                            HTTPProcessor.CopyStream(fs, ctwire, bufferSize, totalBytes, false);

                            ctwire.WriteTerminator();

                            ctwire.Flush();
                        }

                        LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.RawUrlWithQuery, HttpStatusCode.PartialContent));

                        response.Dispose();

                        fs.Flush();
                        fs.Close();

                        return;
                    }

                shortcut: // Necessary evil.

                    fs.Flush();
                    fs.Close();
                }

                if (response != null)
                {
                    if (response.ContentStream != null)
                    {
                        bool KeepAlive = AllowKeepAlive && request.RetrieveHeaderValue("Connection").Equals("keep-alive");
                        string NoneMatch = request.RetrieveHeaderValue("If-None-Match");
                        string? EtagMD5 = HTTPProcessor.ComputeStreamMD5(response.ContentStream);
                        bool isNoneMatchValid = !string.IsNullOrEmpty(NoneMatch) && NoneMatch.Equals(EtagMD5);
                        bool isModifiedSinceValid = HTTPProcessor.CheckLastWriteTime(filePath, request.RetrieveHeaderValue("If-Modified-Since"));

                        if ((isNoneMatchValid && isModifiedSinceValid) ||
                            (isNoneMatchValid && string.IsNullOrEmpty(request.RetrieveHeaderValue("If-Modified-Since"))) ||
                            (isModifiedSinceValid && string.IsNullOrEmpty(NoneMatch)))
                        {
                            response.Headers.Clear();

                            if (!response.Headers.Keys.Any(key => key.Equals("server", StringComparison.OrdinalIgnoreCase)))
                                response.Headers.Add("Server", "Apache");

                            if (KeepAlive)
                            {
                                response.Headers.Add("Connection", "Keep-Alive");
                                response.Headers.Add("Keep-Alive", "timeout=15, max=98");
                            }
                            else
                                response.Headers.Add("Connection", "close");

                            if (!string.IsNullOrEmpty(EtagMD5))
                            {
                                response.Headers.Add("ETag", EtagMD5);
                                response.Headers.Add("Expires", DateTime.Now.AddMinutes(30).ToString("r"));
                            }

                            response.HttpStatusCode = HttpStatusCode.NotModified;

                            WriteLineToStream(stream, response.ToHeader());

                            stream.Flush();
                        }
                        else
                        {
                            int bufferSize = HTTPServerConfiguration.BufferSize;
                            long totalBytes = response.ContentStream.Length;
                            string? encoding = null;

                            if (!response.Headers.Keys.Any(key => key.Equals("server", StringComparison.OrdinalIgnoreCase)))
                                response.Headers.Add("Server", "Apache");

                            if (KeepAlive)
                            {
                                response.Headers.Add("Connection", "Keep-Alive");
                                response.Headers.Add("Keep-Alive", "timeout=15, max=98");
                            }
                            else
                                response.Headers.Add("Connection", "close");

                            response.Headers.Add("Access-Control-Allow-Origin", "*");
                            response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, HEAD, GET, PUT, POST, PROPFIND");
                            response.Headers.Add("Access-Control-Allow-Headers", "*");
                            response.Headers.Add("Access-Control-Expose-Headers", string.Empty);

                            if (!response.Headers.Keys.Any(key => key.Equals("content-type", StringComparison.OrdinalIgnoreCase)))
                                response.Headers.Add("Content-Type", "text/plain");

                            if (response.HttpStatusCode == HttpStatusCode.OK)
                            {
                                response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                if (!string.IsNullOrEmpty(EtagMD5))
                                {
                                    response.Headers.Add("ETag", EtagMD5);
                                    response.Headers.Add("Expires", DateTime.Now.AddMinutes(30).ToString("r"));
                                }
                                response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                            }

                            if (!response.Headers.Keys.Any(key => key.Equals("content-length", StringComparison.OrdinalIgnoreCase)))
                            {
                                if (response.Headers.TryGetValue("Transfer-Encoding", out encoding) && !string.IsNullOrEmpty(encoding) && encoding.Contains("chunked"))
                                {

                                }
                                else
                                    response.Headers.Add("Content-Length", totalBytes.ToString());
                            }

                            WriteLineToStream(stream, response.ToHeader());

                            stream.Flush();

                            // We override the bufferSize for large content, else, we monster the CPU.
                            if (totalBytes > 8000000 && bufferSize < 500000)
                                bufferSize = 500000;

                            using (HttpResponseContentStream ctwire = new(stream, response.Headers.ContainsKey("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked")))
                            {
                                HTTPProcessor.CopyStream(response.ContentStream, ctwire, bufferSize, totalBytes, false);

                                ctwire.WriteTerminator();

                                ctwire.Flush();
                            }
                        }
                    }
                    else
                    {
                        response.Headers.Clear();

                        if (!response.Headers.Keys.Any(key => key.Equals("server", StringComparison.OrdinalIgnoreCase)))
                            response.Headers.Add("Server", "Apache");
                        response.Headers.Add("Connection", "close");

                        response.HttpStatusCode = HttpStatusCode.InternalServerError;

                        WriteLineToStream(stream, response.ToHeader());

                        stream.Flush();
                    }

                    if ((int)response.HttpStatusCode < 400)
                        LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.RawUrlWithQuery, response.HttpStatusCode));
                    else
                    {
                        if (response.HttpStatusCode == HttpStatusCode.NotFound)
                            LoggerAccessor.LogWarn(string.Format("[HTTP] - {0}:{1} Requested a non-existant file: {2} -> {3}", request.IP, request.Port, filePath, response.HttpStatusCode));
                        else if (response.HttpStatusCode == HttpStatusCode.NotImplemented || response.HttpStatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
                            LoggerAccessor.LogWarn(string.Format("{0} -> {1}", request.RawUrlWithQuery, response.HttpStatusCode));
                        else
                            LoggerAccessor.LogError(string.Format("{0} -> {1}", request.RawUrlWithQuery, response.HttpStatusCode));
                    }
                }
            }
            catch (IOException ex)
            {
                if (ex.InnerException is SocketException socketException &&
                    socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted)
                    LoggerAccessor.LogError($"[HTTP] - Handle_LocalFile_Stream - IO-Socket thrown an exception : {ex}");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[HTTP] - Handle_LocalFile_Stream thrown an assertion : {ex}");
            }

            response?.Dispose();
        }

        private static void WriteLineToStream(Stream stream, string text)
        {
            stream.Write(Encoding.UTF8.GetBytes(text).AsSpan());
        }

        protected virtual Stream GetOutputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected virtual Stream GetInputStream(TcpClient tcpClient)
        {
            return tcpClient.GetStream();
        }

        protected virtual HttpResponse? RouteRequest(Stream inputStream, Stream outputStream, HttpRequest request, string url, string Host)
        {
            List<Route> routes = Routes.Where(x => x.UrlRegex != null && Regex.Match(url, x.UrlRegex).Success).ToList();

            if (!routes.Any())
                return null;

            Route? route = routes.SingleOrDefault(x => x.Method == request.Method && (x.Host == Host || x.Host == string.Empty));

            if (route == null)
                return null;

            request.Route = route;

            try
            {
                if (route.Callable != null)
                {
                    HttpResponse? result = route.Callable(request);
                    if (result != null && result.IsValid())
                        return result;
                }
            }
            catch
            {
                // Not Important
            }

            return null;
        }

        protected virtual HttpRequest AppendRequestOrInputStream(Stream inputStream, HttpRequest request, string clientip, string clientport, ushort ListenerPort)
		{
            HttpRequest? newRequest = GetRequest(inputStream, clientip, clientport, ListenerPort);

            if (newRequest != null)
            {
                request.Dispose();
                return newRequest;
            }

            if (request.Data != null && request.Data.CanSeek)
            {
                // Seek to the end of the target stream, and copy from there.
                long CurrentPosition = request.Data.Seek(0, SeekOrigin.End);
                inputStream.CopyTo(request.Data);
                request.Data.Position = CurrentPosition;
            }
            else
            {
                const int bufferSize = 16 * 1024;

                int bytesRead;
                byte[] buffer = new byte[bufferSize];

                request.Data = new HugeMemoryStream(); // We can't predict stream size, so take safer option.

                while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    request.Data.Write(buffer, 0, bytesRead);
                }

                request.Data.Position = 0;
            }

            return request;
        }


        protected virtual HttpRequest? GetRequest(Stream inputStream, string clientip, string clientport, ushort ListenerPort)
		{
            // Read Request Line and check if valid.
            string[] tokens = Readline(inputStream).Split(' ');

            if (tokens.Length == 3 && !string.IsNullOrEmpty(tokens[2]) && tokens[2].Contains("HTTP/"))
            {
                string line;
                // string protocolVersion = tokens[2]; // Unused.

                // Read Headers
                List<KeyValuePair<string, string>> headers = new();

                while ((line = Readline(inputStream)) != null)
                {
                    if (line.Equals(string.Empty))
                        break;

                    int separator = line.IndexOf(':');
                    if (separator == -1)
                        return null;

                    int pos = separator + 1;
                    while (pos < line.Length && line[pos] == ' ')
                    {
                        pos++;
                    }

                    headers.Add(new KeyValuePair<string, string>(line[..separator], line[pos..]));
                }

                HttpRequest request = new()
                {
                    Method = tokens[0].ToUpper(),
                    RawUrlWithQuery = tokens[1],
                    Headers = headers,
                    IP = clientip,
                    Port = clientport,
                    ServerPort = ListenerPort,
                    ServerIP = ServerIp
                };

                string ContentLength = request.GetContentLength();

                if (!string.IsNullOrEmpty(ContentLength))
                {
                    const int bufferSize = 16 * 1024;

                    int bytesRead;
                    long totalBytesCopied = 0;
                    long bytesToCopy = Convert.ToInt32(ContentLength);
                    byte[] buffer = new byte[bufferSize];

                    request.Data = new MemoryStream();

                    while (totalBytesCopied < bytesToCopy && (bytesRead = inputStream.Read(buffer, 0, (int)Math.Min(bufferSize, bytesToCopy - totalBytesCopied))) > 0)
                    {
                        request.Data.Write(buffer, 0, bytesRead);
                        totalBytesCopied += bytesRead;
                    }

                    request.Data.Position = 0;
                }

                return request;
            }

            return null;
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex(@"Match (\d{3}) (\S+) (\S+)$")]
        private static partial Regex ApacheMatchRegex();
        [GeneratedRegex("^(GET|POST|PUT|DELETE|HEAD|OPTIONS|PATCH)")]
        private static partial Regex HttpMethodRegex();
        [GeneratedRegex("\\b\\d{3}\\b")]
        private static partial Regex HttpStatusCodeRegex();
#endif
#endregion
    }
}
