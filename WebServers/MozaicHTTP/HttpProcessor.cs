// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain

using CustomLogger;
using CyberBackendLibrary.Crypto;
using CyberBackendLibrary.DataTypes;
using CyberBackendLibrary.Extension;
using CyberBackendLibrary.GeoLocalization;
using CyberBackendLibrary.HTTP;
using CyberBackendLibrary.HTTP.PluginManager;
using HttpMultipartParser;
using MozaicHTTP.Extensions;
using MozaicHTTP.Models;
using MozaicHTTP.RouteHandlers;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Ocsp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebAPIService.CAPONE;
using WebAPIService.CDM;
using WebAPIService.FROMSOFTWARE;
using WebAPIService.HELLFIRE;
using WebAPIService.JUGGERNAUT;
using WebAPIService.LOOT;
using WebAPIService.MultiMedia;
using WebAPIService.NDREAMS;
using WebAPIService.OHS;
using WebAPIService.OUWF;
using WebAPIService.PREMIUMAGENCY;
using WebAPIService.UBISOFT.gsconnect;
using WebAPIService.UBISOFT.HERMES_API;
using WebAPIService.VEEMEE;

namespace MozaicHTTP
{
    public partial class HttpProcessor
    {
        #region Fields

        const int ProcbufferSize = 16 * 1024;

        private readonly List<Route> Routes = new();
        private string serverIP = "127.0.0.1";
        public X509Certificate2? sslCertificate = null;

        #endregion

        #region Constructors

        public HttpProcessor(X509Certificate2? sslCertificate = null)
        {
            if (sslCertificate != null)
                this.sslCertificate = sslCertificate;
        }

        #endregion

        #region Public Methods

        public static bool IsIPBanned(string ipAddress, int? clientport)
        {
            if (MozaicHTTPConfiguration.BannedIPs != null && MozaicHTTPConfiguration.BannedIPs.Contains(ipAddress))
            {
                LoggerAccessor.LogError($"[SECURITY] - {ipAddress}:{clientport} Requested the HTTP server while being banned!");
                return true;
            }

            return false;
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

        public void HandleClient(TcpClient tcpClient, ushort ListenerPort)
        {
            try
            {
                int? clientport = ((IPEndPoint?)tcpClient.Client.RemoteEndPoint)?.Port;
                string? clientip = ((IPEndPoint?)tcpClient.Client.RemoteEndPoint)?.Address.ToString();

                if (clientport == null || string.IsNullOrEmpty(clientip) || IsIPBanned(clientip, clientport))
                {
                    tcpClient.Close();
                    tcpClient.Dispose();
                    return;
                }

                HttpRequest? request = null;

                using (Stream? ClientStream = GetStream(tcpClient))
                {
                    while (tcpClient.IsConnected())
                    {
                        if (tcpClient.Available > 0 && ClientStream.CanWrite)
                        {
                            DateTime CurrentDate = DateTime.UtcNow;

                            if (request == null)
                                request = GetRequest(ClientStream, clientip, clientport.ToString(), ListenerPort);
                            else
                                request = AppendRequestOrInputStream(ClientStream, request, clientip, clientport.ToString(), ListenerPort);

                            if (request != null && !string.IsNullOrEmpty(request.Url) && !request.RetrieveHeaderValue("User-Agent").ToLower().Contains("bytespider")) // Get Away TikTok.
                            {
                                HttpResponse? response = null;
                                bool KeepAlive = request.RetrieveHeaderValue("Connection") == "keep-alive";
                                string Method = request.Method;
                                string Host = request.RetrieveHeaderValue("Host");
                                if (string.IsNullOrEmpty(Host))
                                    Host = request.RetrieveHeaderValue("HOST");
                                string Accept = request.RetrieveHeaderValue("Accept");
                                string SuplementalMessage = string.Empty;
                                string fullurl = HTTPProcessor.DecodeUrl(request.Url);
                                string absolutepath = HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + HTTPProcessor.RemoveQueryString(fullurl);
                                string fulluripath = HTTPProcessor.ExtractDirtyProxyPath(request.RetrieveHeaderValue("Referer")) + fullurl;
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

                                        if (MozaicHTTPConfiguration.DateTimeOffset != null && MozaicHTTPConfiguration.DateTimeOffset.ContainsKey(CountryCode))
                                            CurrentDate = CurrentDate.AddDays(MozaicHTTPConfiguration.DateTimeOffset[CountryCode]);
                                        else if (MozaicHTTPConfiguration.DateTimeOffset != null && MozaicHTTPConfiguration.DateTimeOffset.ContainsKey(string.Empty))
                                            CurrentDate = CurrentDate.AddDays(MozaicHTTPConfiguration.DateTimeOffset.Where(entry => entry.Key == string.Empty).FirstOrDefault().Value);
                                    }
                                }
                                else if (MozaicHTTPConfiguration.DateTimeOffset != null && MozaicHTTPConfiguration.DateTimeOffset.ContainsKey(string.Empty))
                                    CurrentDate = CurrentDate.AddDays(MozaicHTTPConfiguration.DateTimeOffset.Where(entry => entry.Key == string.Empty).FirstOrDefault().Value);

                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport}{SuplementalMessage} Requested the HTTP Server with URL : {fullurl}");

                                if (MozaicHTTPConfiguration.RedirectRules != null)
                                {
                                    foreach (string? rule in MozaicHTTPConfiguration.RedirectRules)
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
                                                        response = HttpBuilder.RedirectFromApacheRules(KeepAlive, match.Groups[3].Value, int.Parse(match.Groups[1].Value));
                                                }
                                            }
                                            else if (RouteRule.StartsWith("Permanent "))
                                            {
                                                string[] parts = RouteRule.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                                                if (parts.Length == 3 && (parts[1] == "/" || parts[1] == absolutepath))
                                                    response = HttpBuilder.PermanantRedirect(KeepAlive, parts[2]);
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
                                                        response = HttpBuilder.Found(KeepAlive, parts[2]);
                                                    // Check if the input string contains a status code
#if NET6_0
                                                    else if (new Regex(@"\\b\\d{3}\\b").Match(parts[0]).Success && int.TryParse(parts[0], out int statuscode))
#elif NET7_0_OR_GREATER
                                                    else if (HttpStatusCodeRegex().Match(parts[0]).Success && int.TryParse(parts[0], out int statuscode))
#endif
                                                        response = HttpBuilder.RedirectFromApacheRules(KeepAlive, parts[2], statuscode);
                                                    else if (parts[1] == "permanent")
                                                        response = HttpBuilder.PermanantRedirect(KeepAlive, parts[2]);
                                                }
                                                else if (parts.Length == 2 && (parts[0] == "/" || parts[0] == absolutepath))
                                                    response = HttpBuilder.Found(KeepAlive, parts[1]);
                                            }
                                        }
                                    }
                                }

                                // Split the URL into segments
                                string[] segments = absolutepath.Trim('/').Split('/');

                                // Combine the folder segments into a directory path
                                string directoryPath = Path.Combine(MozaicHTTPConfiguration.HTTPStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                                // Process the request based on the HTTP method
                                string filePath = Path.Combine(MozaicHTTPConfiguration.HTTPStaticFolder, absolutepath[1..]);

                                string apiPath = Path.Combine(MozaicHTTPConfiguration.APIStaticFolder, absolutepath[1..]);

                                if (response == null && MozaicHTTPConfiguration.plugins.Count > 0)
                                {
                                    foreach (HTTPPlugin plugin in MozaicHTTPConfiguration.plugins.Values)
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

                                response ??= RouteRequest(ClientStream, ClientStream, request, absolutepath, Host);

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

                                if (response == null)
                                {
                                    switch (Host)
                                    {
                                        default:

                                            #region Outso OHS API
                                            if ((Host == "stats.outso-srv1.com" 
                                                || Host == "www.outso-srv1.com") &&
                                                request.DataAsBytes != null &&
                                                (absolutepath.Contains("/ohs_") ||
                                                absolutepath.Contains("/ohs/") ||
                                                absolutepath.Contains("/statistic/") ||
                                                absolutepath.Contains("/tracker/") || 
                                                absolutepath.Contains("/Konami/")) && absolutepath.EndsWith('/'))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a OHS method : {absolutepath}");

                                                string? res = null;
												
                                                #region OHS API Version
                                                int version = 0;

                                                if (sslCertificate == null) // No crypto for https.
                                                {
                                                    if (absolutepath.Contains("/Insomniac/4BarrelsOfFury/"))
                                                        version = 2;
                                                    else if (absolutepath.Contains("/SCEA/SaucerPop/"))
                                                        version = 2;
                                                    else if (absolutepath.Contains("/AirRace/"))
                                                        version = 2;
                                                    else if (absolutepath.Contains("/Uncharted2/"))
                                                        version = 1;
                                                    else if (absolutepath.Contains("/Infamous/"))
                                                        version = 1;
                                                    else if (absolutepath.Contains("/warhawk_shooter/"))
                                                        version = 1;
                                                }
                                                #endregion

                                                byte[]? dataAsBytes = request.DataAsBytes;

                                                if (dataAsBytes != null)
                                                    res = new OHSClass(Method, absolutepath, version).ProcessRequest(dataAsBytes, request.GetContentType(), apiPath);

                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError(KeepAlive);
                                                else
                                                    response = HttpResponse.Send(KeepAlive, $"<ohs>{res}</ohs>", "application/xml;charset=UTF-8");
                                            }
                                            #endregion

                                            #region Outso OUWF Debug API
                                            else if (Host == "ouwf.outso-srv1.com" 
                                                && request.DataAsBytes != null 
                                                && !string.IsNullOrEmpty(Method) 
                                                && request.GetContentType().StartsWith("multipart/form-data"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip} Identified a OuWF method : {absolutepath}");

                                                string? res = null;
                                                res = new OuWFClass(Method, absolutepath, MozaicHTTPConfiguration.HTTPStaticFolder).ProcessRequest(request.DataAsBytes, request.GetContentType());
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError(KeepAlive);
                                                else
                                                    response = HttpResponse.Send(KeepAlive, res, "text/xml");
                                            }
                                            #endregion

                                            #region VEEMEE API
                                            else if ((Host == "away.veemee.com"
                                                || Host == "home.veemee.com") &&
                                                !string.IsNullOrEmpty(Method) &&
                                                absolutepath.EndsWith(".php"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a VEEMEE  method : {absolutepath}");

                                                byte[]? dataAsBytes = request.DataAsBytes;

                                                if (dataAsBytes != null)
                                                {
                                                    (string?, string?) res = new VEEMEEClass(Method, absolutepath).ProcessRequest(dataAsBytes, request.GetContentType(), MozaicHTTPConfiguration.APIStaticFolder);

                                                    if (string.IsNullOrEmpty(res.Item1))
                                                        response = HttpBuilder.InternalServerError(KeepAlive);
                                                    else
                                                    {
                                                        if (!string.IsNullOrEmpty(res.Item2))
                                                            response = HttpResponse.Send(KeepAlive, res.Item1, res.Item2);
                                                        else
                                                            response = HttpResponse.Send(KeepAlive, res.Item1, "text/plain");
                                                    }
                                                }
                                                else
                                                {
                                                    (string?, string?) res = new VEEMEEClass(Method, absolutepath).ProcessRequest(null, request.GetContentType(), MozaicHTTPConfiguration.APIStaticFolder);

                                                    if (string.IsNullOrEmpty(res.Item1))
                                                        response = HttpBuilder.InternalServerError(KeepAlive);
                                                    else
                                                    {
                                                        if (!string.IsNullOrEmpty(res.Item2))
                                                            response = HttpResponse.Send(KeepAlive, res.Item1, res.Item2);
                                                        else
                                                            response = HttpResponse.Send(KeepAlive, res.Item1, "text/plain");
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
                                                byte[]? dataAsBytes = request.DataAsBytes;
                                                NDREAMSClass ndreams = new(CurrentDate, Method, apiPath, $"http{(sslCertificate == null ? string.Empty : 's')}://nDreams-multiserver-cdn/", $"http{(sslCertificate == null ? string.Empty : 's')}://{Host}{fullurl}",
                                                    absolutepath, MozaicHTTPConfiguration.APIStaticFolder, Host);

                                                if (dataAsBytes != null)
                                                    res = ndreams.ProcessRequest(request.QueryParameters, dataAsBytes, request.GetContentType());
                                                else
                                                    res = ndreams.ProcessRequest(request.QueryParameters);

                                                ndreams.Dispose();
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError(KeepAlive);
                                                else
                                                    response = HttpResponse.Send(KeepAlive, res, "text/xml");
                                            }
                                            #endregion

                                            #region Hellfire Games API
                                            else if (Host == "game2.hellfiregames.com" && absolutepath.EndsWith(".php"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested a HELLFIRE method : {absolutepath}");

                                                string? res = string.Empty;
                                                byte[]? dataAsBytes = request.DataAsBytes;

                                                if (dataAsBytes != null)
                                                    res = new HELLFIREClass(request.Method.ToString(), HTTPProcessor.RemoveQueryString(absolutepath), MozaicHTTPConfiguration.APIStaticFolder).ProcessRequest(dataAsBytes, request.GetContentType(), sslCertificate != null);

                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError(KeepAlive);
                                                else
                                                    response = HttpResponse.Send(KeepAlive, res, "application/xml;charset=UTF-8");
                                            }
                                            #endregion

                                            #region Juggernaut Games API
                                            else if (Host == "juggernaut-games.com" 
                                                && !string.IsNullOrEmpty(Method) 
                                                && absolutepath.EndsWith(".php"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a JUGGERNAUT method : {absolutepath}");

                                                string? res = null;
                                                byte[]? dataAsBytes = request.DataAsBytes;
                                                JUGGERNAUTClass juggernaut = new(Method, absolutepath);
                                                if (dataAsBytes != null)
                                                    res = juggernaut.ProcessRequest(request.QueryParameters, MozaicHTTPConfiguration.APIStaticFolder, dataAsBytes, request.GetContentType());
                                                else
                                                    res = juggernaut.ProcessRequest(request.QueryParameters, MozaicHTTPConfiguration.APIStaticFolder);
                                                juggernaut.Dispose();
                                                if (res == null)
                                                    response = HttpBuilder.InternalServerError(KeepAlive);
                                                else if (res == string.Empty)
                                                    response = HttpBuilder.OK(KeepAlive);
                                                else
                                                    response = HttpResponse.Send(KeepAlive, res, "text/xml");
                                            }
                                            #endregion

                                            #region LOOT API
                                            else if ((Host == "server.lootgear.com" 
                                                || Host == "alpha.lootgear.com") 
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a LOOT method : {absolutepath}");

                                                string? res = null;
                                                byte[]? dataAsBytes = request.DataAsBytes;
                                                LOOTClass loot = new(Method, absolutepath, apiPath);
                                                if (dataAsBytes != null)
                                                    res = loot.ProcessRequest(request.QueryParameters, dataAsBytes, request.GetContentType());
                                                else
                                                    res = loot.ProcessRequest(request.QueryParameters);

                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError(KeepAlive);
                                                else
                                                    response = HttpResponse.Send(KeepAlive, res, "application/xml;charset=UTF-8");
                                            }
                                            #endregion

                                            #region PREMIUMAGENCY API
                                            else if ((Host == "test.playstationhome.jp" ||
                                                Host == "playstationhome.jp" ||
                                                Host == "homeec.scej-nbs.jp" ||
                                                Host == "homeecqa.scej-nbs.jp" ||
                                                Host == "homect-scej.jp" ||
                                                Host == "qa-homect-scej.jp") 
                                                && !string.IsNullOrEmpty(Method)
                                                && absolutepath.Contains("/eventController/"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a PREMIUMAGENCY method : {absolutepath}");

                                                string? res = null;
                                                byte[]? dataAsBytes = request.DataAsBytes;
                                                if (dataAsBytes != null)
                                                    res = new PREMIUMAGENCYClass(Method, absolutepath, MozaicHTTPConfiguration.APIStaticFolder, fulluripath).ProcessRequest(dataAsBytes, request.GetContentType());
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError(KeepAlive);
                                                else
                                                    response = HttpResponse.Send(KeepAlive, res, "text/xml");
                                            }
                                            #endregion

                                            #region FROMSOFTWARE API
                                            else if (Host == "acvd-ps3ww-cdn.fromsoftware.jp" && Method != null)
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a FROMSOFTWARE method : {absolutepath}");

                                                (byte[]?, string?, string[][]?) res = new();
                                                byte[]? dataAsBytes = request.DataAsBytes;
                                                if (dataAsBytes != null)
                                                    res = new FROMSOFTWAREClass(Method, absolutepath, MozaicHTTPConfiguration.APIStaticFolder).ProcessRequest(dataAsBytes, request.GetContentType());
                                                if (res.Item1 == null || string.IsNullOrEmpty(res.Item2) || res.Item3?.Length == 0)
                                                    response = HttpBuilder.InternalServerError(KeepAlive);
                                                else
                                                    response = HttpResponse.Send(KeepAlive, res.Item1, res.Item2, res.Item3);
                                            }
                                            #endregion

                                            #region gsconnect API
                                            else if (Host == "gsconnect.ubisoft.com" && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a gsconnect method : {absolutepath}");

                                                (string?, string?, Dictionary<string, string>?) res;
                                                byte[]? dataAsBytes = request.DataAsBytes;
                                                gsconnectClass gsconn = new(Method, absolutepath, MozaicHTTPConfiguration.APIStaticFolder);
                                                if (dataAsBytes != null)
                                                    res = gsconn.ProcessRequest(request.QueryParameters, dataAsBytes, request.GetContentType());
                                                else
                                                    res = gsconn.ProcessRequest(request.QueryParameters);

                                                if (string.IsNullOrEmpty(res.Item1) || string.IsNullOrEmpty(res.Item2))
                                                    response = HttpBuilder.InternalServerError(false);
                                                else
                                                {
                                                    response = new(false, "1.0")
                                                    {
                                                        HttpStatusCode = Models.HttpStatusCode.OK
                                                    };
                                                    response.Headers["Content-Type"] = res.Item2;
                                                    response.ContentAsUTF8 = res.Item1;

                                                    if (res.Item3 != null)
                                                    {
                                                        foreach (KeyValuePair<string, string> headertoadd in  res.Item3)
                                                        {
                                                            response.Headers.TryAdd(headertoadd.Key, headertoadd.Value);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region Ubisoft API
                                            else if (Host.Contains("api-ubiservices.ubi.com") 
                                                && request.RetrieveHeaderValue("User-Agent").Contains("UbiServices_SDK_HTTP_Client") 
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a UBISOFT method : {absolutepath}");

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

                                                    (string?, string?) res = new HERMESClass(Method, absolutepath, request.RetrieveHeaderValue("Ubi-AppId"), request.RetrieveHeaderValue("Ubi-RequestedPlatformType"),
                                                            request.RetrieveHeaderValue("ubi-appbuildid"), clientip, GeoIP.GetISOCodeFromIP(IPAddress.Parse(clientip)), Authorization.Replace("psn t=", string.Empty), MozaicHTTPConfiguration.APIStaticFolder)
                                                            .ProcessRequest(request.DataAsBytes, request.GetContentType());

                                                    if (string.IsNullOrEmpty(res.Item1))
                                                        response = HttpBuilder.InternalServerError(KeepAlive);
                                                    else
                                                    {
                                                        if (!string.IsNullOrEmpty(res.Item2))
                                                            response = HttpResponse.Send(KeepAlive, res.Item1, res.Item2);
                                                        else
                                                            response = HttpResponse.Send(KeepAlive, res.Item1, "text/plain");

                                                        response.Headers.Add("Ubi-Forwarded-By", "ue1-p-us-public-nginx-056b582ac580ba328");
                                                        response.Headers.Add("Ubi-TransactionId", Guid.NewGuid().ToString());

                                                    }
                                                }
                                                else
                                                    response = HttpBuilder.NotAllowed(KeepAlive);
                                            }
                                            #endregion

                                            #region CentralDispatchManager API
                                            else if (HPDDomains.Contains(Host) 
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a CentralDispatchManager method : {absolutepath}");

                                                string? res = null;
                                                byte[]? dataAsBytes = request.DataAsBytes;

                                                if (Method == "POST" && dataAsBytes != null)
                                                    res = new CDMClass(request.Method, absolutepath, MozaicHTTPConfiguration.APIStaticFolder).ProcessRequest(dataAsBytes, request.GetContentType(), apiPath);
                                                else
                                                    res = new CDMClass(request.Method, absolutepath, MozaicHTTPConfiguration.APIStaticFolder).ProcessRequest(Array.Empty<byte>(), request.GetContentType(), apiPath);

                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError(KeepAlive);
                                                else
                                                    response = HttpResponse.Send(KeepAlive, res, "text/xml");
                                            }
                                            #endregion

                                            #region CAPONE GriefReporter API
                                            else if (CAPONEDomains.Contains(Host) 
                                                && !string.IsNullOrEmpty(Method))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Identified a CAPONE method : {absolutepath}");

                                                string? res = null;
                                                byte[]? dataAsBytes = request.DataAsBytes;
                                                if (dataAsBytes != null)
                                                    res = new CAPONEClass(request.Method, absolutepath, MozaicHTTPConfiguration.APIStaticFolder).ProcessRequest(dataAsBytes, request.GetContentType(), apiPath);
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError(KeepAlive);
                                                else
                                                    response = HttpResponse.Send(KeepAlive, res, "text/xml");
                                            }
                                            #endregion

                                            #region PSH Central
                                            else if (Host == "apps.pshomecentral.net" && absolutepath == "/PrivateRTE/checkAuth.php")
                                            {
                                                LoggerAccessor.LogError($"[HTTPS] - {clientip}:{clientport} Requested a PS Home Central method : {absolutepath}");
                                                response = HttpResponse.Send(KeepAlive, "false");
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
                                                                response = HttpResponse.Send(false, new byte[2097152]);
                                                                break;
                                                            #endregion

                                                            #region MoreLife
                                                            case "/!morelife":
                                                                response = HttpResponse.Send(false, "{}", "text/json");
                                                                break;
                                                            #endregion

                                                            #region WebVideo Player
                                                            case "/!player":
                                                            case "/!player/":
                                                                string ServerIP = serverIP;
                                                                if (ServerIP.Length > 15)
                                                                    ServerIP = "[" + ServerIP + "]"; // Format the hostname if it's a IPV6 url format.
                                                                WebVideoPlayer? WebPlayer = new((request.QueryParameters ?? new Dictionary<string, string>()).Aggregate(new NameValueCollection(),
                                                                    (seed, current) => {
                                                                        seed.Add(current.Key, current.Value);
                                                                        return seed;
                                                                    }), $"http://{ServerIP}/!webvideo/?");
                                                                response = HttpResponse.Send(KeepAlive, WebPlayer.HtmlPage, "text/html", WebPlayer.HeadersToSet);
                                                                WebPlayer = null;
                                                                break;
                                                            #endregion

                                                            #region WebVideo
                                                            case "/!webvideo":
                                                            case "/!webvideo/":
                                                                if (request.RetrieveHeaderValue("User-Agent").Contains("PSHome")) // The game is imcompatible with the webvideo, and it can even spam request it, so we forbid.
                                                                    response = HttpBuilder.NotAllowed(KeepAlive);
                                                                else
                                                                {
                                                                    Dictionary<string, string>? QueryDic = request.QueryParameters;
                                                                    if (QueryDic != null && QueryDic.Count > 0 && QueryDic.TryGetValue("url", out string? value) && !string.IsNullOrEmpty(value))
                                                                    {
                                                                        WebVideo? vid = WebVideoConverter.ConvertVideo(QueryDic, MozaicHTTPConfiguration.ConvertersFolder);
                                                                        if (vid != null && vid.Available)
                                                                            response = HttpResponse.Send(KeepAlive, vid.VideoStream, vid.ContentType, new string[][] { new string[] { "Content-Disposition", "attachment; filename=\"" + vid.FileName + "\"" } },
                                                                                Models.HttpStatusCode.OK);
                                                                        else
                                                                            response = new HttpResponse(KeepAlive)
                                                                            {
                                                                                HttpStatusCode = Models.HttpStatusCode.OK,
                                                                                ContentAsUTF8 = "<p>" + vid?.ErrorMessage + "</p>" +
                                                                                        "<p>Make sure that parameters are correct, and both <i>yt-dlp</i> and <i>ffmpeg</i> are properly installed on the server.</p>",
                                                                                Headers = { { "Content-Type", "text/html" } }
                                                                            };
                                                                    }
                                                                    else
                                                                        response = new HttpResponse(KeepAlive)
                                                                        {
                                                                            HttpStatusCode = Models.HttpStatusCode.OK,
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
                                                                break;
                                                            #endregion

                                                            default:
                                                                if ((absolutepath.EndsWith(".asp", StringComparison.InvariantCultureIgnoreCase) || absolutepath.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(MozaicHTTPConfiguration.ASPNETRedirectUrl))
                                                                    response = HttpBuilder.PermanantRedirect(KeepAlive, $"{MozaicHTTPConfiguration.ASPNETRedirectUrl}{fullurl}");
                                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(MozaicHTTPConfiguration.PHPRedirectUrl))
                                                                    response = HttpBuilder.PermanantRedirect(KeepAlive, $"{MozaicHTTPConfiguration.PHPRedirectUrl}{fullurl}");
                                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && Directory.Exists(MozaicHTTPConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                                                {
                                                                    (byte[]?, string[][]) CollectPHP = PHP.ProcessPHPPage(filePath, MozaicHTTPConfiguration.PHPStaticFolder, MozaicHTTPConfiguration.PHPVersion, clientip, clientport.ToString(), request);
                                                                    if (MozaicHTTPConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(encoding) && CollectPHP.Item1 != null)
                                                                    {
                                                                        if (encoding.Contains("zstd"))
                                                                            response = HttpResponse.Send(KeepAlive, HTTPProcessor.CompressZstd(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "zstd" }));
                                                                        else if (encoding.Contains("br"))
                                                                            response = HttpResponse.Send(KeepAlive, HTTPProcessor.CompressBrotli(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "br" }));
                                                                        else if (encoding.Contains("gzip"))
                                                                            response = HttpResponse.Send(KeepAlive, HTTPProcessor.CompressGzip(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "gzip" }));
                                                                        else if (encoding.Contains("deflate"))
                                                                            response = HttpResponse.Send(KeepAlive, HTTPProcessor.Inflate(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "deflate" }));
                                                                        else
                                                                            response = HttpResponse.Send(KeepAlive, CollectPHP.Item1, "text/html", CollectPHP.Item2);
                                                                    }
                                                                    else
                                                                        response = HttpResponse.Send(KeepAlive, CollectPHP.Item1, "text/html", CollectPHP.Item2);
                                                                }
                                                                else
                                                                {
                                                                    if (File.Exists(filePath) && request.Headers.Keys.Count(x => x == "Range") == 1) // Mmm, is it possible to have more?
                                                                        Handle_LocalFile_Stream(ClientStream, request, filePath, KeepAlive);
                                                                    else
                                                                        response = FileSystemRouteHandler.Handle(KeepAlive, request, absolutepath, Host, filePath, Accept,
                                                                            serverIP, ListenerPort, $"http://example.com{absolutepath[..^1]}", clientip, clientport.ToString());
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                    case "POST":
                                                        switch (absolutepath)
                                                        {
                                                            #region PSN Network Test
                                                            case "/networktest/post_128":
                                                                response = HttpBuilder.OK(false);
                                                                break;
                                                            #endregion
															
                                                            default:
                                                                if ((absolutepath.EndsWith(".asp", StringComparison.InvariantCultureIgnoreCase) || absolutepath.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase)) && !string.IsNullOrEmpty(MozaicHTTPConfiguration.ASPNETRedirectUrl))
                                                                    response = HttpBuilder.PermanantRedirect(KeepAlive, $"{MozaicHTTPConfiguration.ASPNETRedirectUrl}{fullurl}");
                                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(MozaicHTTPConfiguration.PHPRedirectUrl))
                                                                    response = HttpBuilder.PermanantRedirect(KeepAlive, $"{MozaicHTTPConfiguration.PHPRedirectUrl}{fullurl}");
                                                                else if (absolutepath.EndsWith(".php", StringComparison.InvariantCultureIgnoreCase) && Directory.Exists(MozaicHTTPConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                                                {
                                                                    var CollectPHP = PHP.ProcessPHPPage(filePath, MozaicHTTPConfiguration.PHPStaticFolder, MozaicHTTPConfiguration.PHPVersion, clientip, clientport.ToString(), request);
                                                                    if (MozaicHTTPConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(encoding) && CollectPHP.Item1 != null)
                                                                    {
                                                                        if (encoding.Contains("zstd"))
                                                                            response = HttpResponse.Send(KeepAlive, HTTPProcessor.CompressZstd(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "zstd" }));
                                                                        else if (encoding.Contains("br"))
                                                                            response = HttpResponse.Send(KeepAlive, HTTPProcessor.CompressBrotli(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "br" }));
                                                                        else if (encoding.Contains("gzip"))
                                                                            response = HttpResponse.Send(KeepAlive, HTTPProcessor.CompressGzip(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "gzip" }));
                                                                        else if (encoding.Contains("deflate"))
                                                                            response = HttpResponse.Send(KeepAlive, HTTPProcessor.Inflate(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "deflate" }));
                                                                        else
                                                                            response = HttpResponse.Send(KeepAlive, CollectPHP.Item1, "text/html", CollectPHP.Item2);
                                                                    }
                                                                    else
                                                                        response = HttpResponse.Send(KeepAlive, CollectPHP.Item1, "text/html", CollectPHP.Item2);
                                                                }
                                                                else
                                                                {
                                                                    if (File.Exists(filePath) && request.Headers.Keys.Count(x => x == "Range") == 1) // Mmm, is it possible to have more?
                                                                        Handle_LocalFile_Stream(ClientStream, request, filePath, KeepAlive);
                                                                    else
                                                                        response = FileSystemRouteHandler.Handle(KeepAlive, request, absolutepath, Host,
                                                                            filePath, Accept, serverIP, ListenerPort, $"http://example.com{absolutepath[..^1]}", clientip, clientport.ToString());
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                    case "PUT":
                                                        if (MozaicHTTPConfiguration.EnablePUTMethod)
                                                        {
                                                            string ContentType = request.GetContentType();
                                                            byte[]? dataAsBytes = request.DataAsBytes;
                                                            if (dataAsBytes != null && !string.IsNullOrEmpty(ContentType))
                                                            {
                                                                string? boundary = HTTPProcessor.ExtractBoundary(ContentType);
                                                                if (!string.IsNullOrEmpty(boundary))
                                                                {
                                                                    string UploadDirectoryPath = MozaicHTTPConfiguration.HTTPTempFolder + $"/DataUpload/{absolutepath[1..]}";
                                                                    Directory.CreateDirectory(UploadDirectoryPath);
                                                                    foreach (FilePart? multipartfile in MultipartFormDataParser.Parse(new MemoryStream(dataAsBytes), boundary).Files)
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

                                                                    response = HttpBuilder.OK(KeepAlive);
                                                                }
                                                                else
                                                                    response = HttpBuilder.NotAllowed(KeepAlive);
                                                            }
                                                            else
                                                                response = HttpBuilder.NotAllowed(KeepAlive);
                                                        }
                                                        else
                                                            response = HttpBuilder.NotAllowed(KeepAlive);
                                                        break;
                                                    case "DELETE":
                                                        response = HttpBuilder.NotAllowed(KeepAlive);
                                                        break;
                                                    case "HEAD":
                                                        switch (absolutepath)
                                                        {
                                                            #region WebVideo
                                                            case "/!webvideo":
                                                            case "/!webvideo/":
                                                                if (request.RetrieveHeaderValue("User-Agent").Contains("PSHome")) // The game is imcompatible with the webvideo, and it can even spam request it, so we forbid.
                                                                    response = HttpBuilder.NotAllowed(KeepAlive);
                                                                else
                                                                {
                                                                    Dictionary<string, string>? QueryDic = request.QueryParameters;
                                                                    if (QueryDic != null && QueryDic.Count > 0 && QueryDic.TryGetValue("url", out string? value) && !string.IsNullOrEmpty(value))
                                                                    {
                                                                        WebVideo? vid = WebVideoConverter.ConvertVideo(QueryDic, MozaicHTTPConfiguration.ConvertersFolder);
                                                                        if (vid != null && vid.Available && vid.VideoStream != null)
                                                                        {
                                                                            using HugeMemoryStream ms = new(vid.VideoStream, MozaicHTTPConfiguration.BufferSize);
                                                                            vid.VideoStream.Close();
                                                                            vid.VideoStream.Dispose();
                                                                            ms.Position = 0;
                                                                            response = new(KeepAlive)
                                                                            {
                                                                                HttpStatusCode = Models.HttpStatusCode.OK
                                                                            };
                                                                            response.Headers.Add("Content-Type", vid.ContentType);
                                                                            response.Headers.Add("Content-Length", ms.Length.ToString());
                                                                        }
                                                                        else
                                                                            response = HttpBuilder.InternalServerError(KeepAlive);
                                                                    }
                                                                    else
                                                                        response = HttpBuilder.MissingParameters(KeepAlive);
                                                                }
                                                                break;
                                                            #endregion

                                                            default:
                                                                response = FileSystemRouteHandler.HandleHEAD(request, absolutepath, Host, filePath, Accept, serverIP, ListenerPort);
                                                                break;
                                                        }
                                                        break;
                                                    case "OPTIONS":
                                                        response = HttpBuilder.OK(KeepAlive);
                                                        response.Headers.Add("Allow", "OPTIONS, GET, HEAD, POST");
                                                        break;
                                                    case "PROPFIND":
                                                        if (File.Exists(filePath))
                                                        {
                                                            string ServerIP = serverIP;
                                                            if (ServerIP.Length > 15)
                                                                ServerIP = "[" + ServerIP + "]"; // Format the hostname if it's a IPV6 url format.

                                                            string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(filePath), MozaicHTTPConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
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

                                                            response = HttpResponse.Send(KeepAlive, "<?xml version=\"1.0\"?>\r\n" +
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
                                                                "</a:multistatus>", "text/xml", null, Models.HttpStatusCode.MultiStatus);
                                                        }
                                                        else
                                                            response = HttpBuilder.NotFound(KeepAlive, request, absolutepath, Host, serverIP, ListenerPort.ToString(), !string.IsNullOrEmpty(Accept) && Accept.Contains("html"));
                                                        break;
                                                    default:
                                                        response = HttpBuilder.NotAllowed(KeepAlive);
                                                        break;
                                                }
                                            }
                                            break;
                                    }
                                }

                                if (response != null)
                                    WriteResponse(ClientStream, request, response, filePath);
                            }
                        }
                    }
                    ClientStream.Flush();
                }

                request?.Dispose();
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
            catch (AuthenticationException ex)
            {
                LoggerAccessor.LogWarn($"[HTTP] - HandleClient thrown an AuthenticationException, make sure the Root Certificate Authority is being trusted by the client (Exception: {ex})");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[HTTP] - HandleClient thrown an exception : {ex}");
            }

            tcpClient.Close();
            tcpClient.Dispose();
        }

        public void AddRoute(Route route)
        {
            Routes.Add(route);
        }

#endregion

        #region Private Methods

        private static string Readline(Stream stream)
        {
            int next_char = 0;
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

        private static void WriteResponse(Stream stream, HttpRequest? request, HttpResponse? response, string local_path)
        {
            try
            {
                if (response != null && request != null)
                {
                    if (response.ContentStream == null)
                        response.ContentAsUTF8 = string.Empty;

                    if (response.ContentStream != null) // Safety.
                    {
                        string EtagMD5 = ComputeStreamChecksum(response.ContentStream);

                        response.ContentStream.Position = 0;

                        if (!string.IsNullOrEmpty(request.Method) && request.Method == "OPTIONS")
                        {
                            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, HEAD");
                            response.Headers.Add("Access-Control-Max-Age", "1728000");
                        }

                        if (request.Headers.TryGetValue("If-None-Match", out string? value1) && value1.Equals(EtagMD5))
                        {
                            response.Headers.Clear();

                            response.Headers.Add("ETag", EtagMD5);
                            response.Headers.Add("expires", DateTime.Now.AddMinutes(30).ToString("r"));
                            response.Headers.Add("age", "1800");

                            response.HttpStatusCode = Models.HttpStatusCode.Not_Modified;

                            WriteLineToStream(stream, response.ToHeader());

                            stream.Flush();
                        }
                        else
                        {
                            int buffersize = MozaicHTTPConfiguration.BufferSize;
                            string? encoding = null;

                            response.Headers.Add("Access-Control-Allow-Origin", "*");

                            if (!response.Headers.ContainsKey("Content-Type"))
                                response.Headers.Add("Content-Type", "text/plain");

                            if (response.HttpStatusCode == Models.HttpStatusCode.OK || response.HttpStatusCode == Models.HttpStatusCode.Partial_Content)
                            {
                                response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                response.Headers.Add("ETag", EtagMD5);
                                response.Headers.Add("expires", DateTime.Now.AddMinutes(30).ToString("r"));
                                response.Headers.Add("age", "1800");
                                if (File.Exists(local_path))
                                    response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));
                            }

                            if (!response.Headers.ContainsKey("Content-Length"))
                            {
                                if (response.Headers.TryGetValue("Transfer-Encoding", out encoding) && !string.IsNullOrEmpty(encoding) && encoding.Contains("chunked"))
                                {

                                }
                                else
                                    response.Headers.Add("Content-Length", response.ContentStream.Length.ToString());
                            }

                            WriteLineToStream(stream, response.ToHeader());

                            stream.Flush();

                            long totalBytes = response.ContentStream.Length;
                            long bytesLeft = totalBytes;

                            if (totalBytes > 8000000 && buffersize < 500000) // We optimize large file handling.
                                buffersize = 500000;

                            HttpResponseContentStream ctwire = new(stream, response.Headers.ContainsKey("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked"));

                            while (bytesLeft > 0)
                            {
                                Span<byte> buffer = new byte[bytesLeft > buffersize ? buffersize : bytesLeft];
                                int n = response.ContentStream.Read(buffer);

                                ctwire.Write(buffer);

                                bytesLeft -= n;
                            }

                            ctwire.WriteTerminator();

                            ctwire.Flush();
                        }

                        if ((int)response.HttpStatusCode < 400)
                            LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                        else
                        {
                            if (response.HttpStatusCode == Models.HttpStatusCode.Not_Found)
                                LoggerAccessor.LogWarn(string.Format("[HTTP] - {0}:{1} Requested a non-existant file: {2} -> {3}", request.IP, request.Port, local_path, response.HttpStatusCode));
                            else if (response.HttpStatusCode == Models.HttpStatusCode.NotImplemented || response.HttpStatusCode == Models.HttpStatusCode.RangeNotSatisfiable)
                                LoggerAccessor.LogWarn(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                            else
                                LoggerAccessor.LogError(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                        }

                    }
                    else
                        response = null; // If null, simply not respond to client.
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

            try
            {
                response?.ContentStream?.Close();
            }
            catch (ObjectDisposedException)
            {
                // ContentStream has been disposed already.
            }

            response?.Dispose();
        }

        private static void Handle_LocalFile_Stream(Stream stream, HttpRequest request, string local_path, bool KeepAlive)
        {
            // This method directly communicate with the wire to handle, normally, imposible transfers.
            // If a part of the code sounds weird to you, it's normal... So does curl tests...

            string? acceptencoding = request.RetrieveHeaderValue("Accept-Encoding");

            HttpResponse? response = null;

            using FileStream fs = new(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            long startByte = -1;
            long endByte = -1;
            try
            {
                long filesize = fs.Length;
                string HeaderString = request.RetrieveHeaderValue("Range").Replace("bytes=", string.Empty);
                if (HeaderString.Contains(','))
                {
                    using HugeMemoryStream ms = new();
                    int buffersize = MozaicHTTPConfiguration.BufferSize;
                    Span<byte> Separator = new byte[] { 0x0D, 0x0A };
                    string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(local_path), MozaicHTTPConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                    if (ContentType == "application/octet-stream")
                    {
                        byte[] VerificationChunck = DataTypesUtils.ReadSmallFileChunck(local_path, 10);
                        foreach (var entry in HTTPProcessor._PathernDictionary)
                        {
                            if (DataTypesUtils.FindBytePattern(VerificationChunck, entry.Value) != -1)
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
                            response = new(KeepAlive)
                            {
                                HttpStatusCode = Models.HttpStatusCode.RangeNotSatisfiable
                            };
                            response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                            response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                            if (MozaicHTTPConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(acceptencoding))
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
                            if (request.RetrieveHeaderValue("User-Agent").Contains("PSHome") && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                                response = new(KeepAlive, "1.0") // Home has a game bug where media files do not play well in screens/jukboxes with http 1.1.
                                {
                                    HttpStatusCode = Models.HttpStatusCode.OK
                                };
                            else
                                response = new(KeepAlive)
                                {
                                    HttpStatusCode = Models.HttpStatusCode.OK
                                };
                            response.Headers.Add("Accept-Ranges", "bytes");
                            response.Headers.Add("Content-Type", ContentType);

                            long FileLength = new FileInfo(local_path).Length;

                            if (MozaicHTTPConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(acceptencoding) && ContentType.StartsWith("text/") && FileLength <= 10 * 1024 * 1024)
                            {
                                bool Optimize = FileLength > 5000000;

                                if (acceptencoding.Contains("zstd"))
                                {
                                    response.Headers.Add("Content-Encoding", "zstd");
                                    response.ContentStream = HTTPProcessor.ZstdCompressStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Optimize);
                                }
                                else if (acceptencoding.Contains("br"))
                                {
                                    response.Headers.Add("Content-Encoding", "br");
                                    response.ContentStream = HTTPProcessor.BrotliCompressStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Optimize);
                                }
                                else if (acceptencoding.Contains("gzip"))
                                {
                                    response.Headers.Add("Content-Encoding", "gzip");
                                    response.ContentStream = HTTPProcessor.GzipCompressStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Optimize);
                                }
                                else if (acceptencoding.Contains("deflate"))
                                {
                                    response.Headers.Add("Content-Encoding", "deflate");
                                    response.ContentStream = HTTPProcessor.InflateStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Optimize);
                                }
                                else
                                    response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            }
                            else
                                response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                            goto shortcut; // Do we really have the choice?
                        }
                        else
                        {
                            int bytesRead = 0;
                            long TotalBytes = endByte - startByte - 1;
                            long totalBytesCopied = 0;
                            byte[] buffer = new byte[ProcbufferSize];
                            fs.Position = startByte;
                            ms.Write(Encoding.UTF8.GetBytes("Content-Range: " + string.Format("bytes {0}-{1}/{2}", startByte, endByte - 1, filesize)).AsSpan());
                            ms.Write(Separator);
                            ms.Write(Separator);
                            while (totalBytesCopied < TotalBytes && (bytesRead = fs.Read(buffer, 0, ProcbufferSize)) > 0)
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
                    if (request.RetrieveHeaderValue("User-Agent").Contains("PSHome") && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                        response = new(true, "1.0") // Home has a game bug where media files do not play well in screens/jukboxes with http 1.1.
                        {
                            HttpStatusCode = Models.HttpStatusCode.Partial_Content
                        };
                    else
                        response = new(true)
                        {
                            HttpStatusCode = Models.HttpStatusCode.Partial_Content
                        };
                    response.Headers.Add("Content-Type", "multipart/byteranges; boundary=multiserver_separator");
                    response.Headers.Add("Accept-Ranges", "bytes");
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                    response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));

                    string? encoding = null;

                    if (!response.Headers.ContainsKey("Content-Length"))
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
                    long bytesLeft = totalBytes;

                    if (totalBytes > 8000000 && buffersize < 500000) // We optimize large file handling.
                        buffersize = 500000;

                    HttpResponseContentStream ctwire = new(stream, response.Headers.ContainsKey("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked"));

                    while (bytesLeft > 0)
                    {
                        Span<byte> buffer = new byte[bytesLeft > buffersize ? buffersize : bytesLeft];
                        int n = ms.Read(buffer);

                        ctwire.Write(buffer);

                        bytesLeft -= n;
                    }

                    ctwire.WriteTerminator();

                    ctwire.Flush();

                    ms.Flush();
                    ms.Close();

                    LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));

                    try
                    {
                        response?.ContentStream?.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                        // ContentStream has been disposed already.
                    }

                    response?.Dispose();

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

                    response = new(KeepAlive)
                    {
                        HttpStatusCode = Models.HttpStatusCode.RangeNotSatisfiable
                    };
                    response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                    response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                    if (MozaicHTTPConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(acceptencoding))
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
                    string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(local_path), MozaicHTTPConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                    if (ContentType == "application/octet-stream")
                    {
                        byte[] VerificationChunck = DataTypesUtils.ReadSmallFileChunck(local_path, 10);
                        foreach (var entry in HTTPProcessor._PathernDictionary)
                        {
                            if (DataTypesUtils.FindBytePattern(VerificationChunck, entry.Value) != -1)
                            {
                                ContentType = entry.Key;
                                break;
                            }
                        }
                    }
                    if (request.RetrieveHeaderValue("User-Agent").Contains("PSHome") && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                        response = new(KeepAlive, "1.0") // Home has a game bug where media files do not play well in screens/jukboxes with http 1.1.
                        {
                            HttpStatusCode = Models.HttpStatusCode.OK
                        };
                    else
                        response = new(KeepAlive)
                        {
                            HttpStatusCode = Models.HttpStatusCode.OK
                        };
                    response.Headers.Add("Accept-Ranges", "bytes");
                    response.Headers.Add("Content-Type", ContentType);

                    long FileLength = new FileInfo(local_path).Length;

                    if (MozaicHTTPConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(acceptencoding) && ContentType.StartsWith("text/") && FileLength <= 10 * 1024 * 1024)
                    {
                        bool Optimize = FileLength > 5000000;

                        if (acceptencoding.Contains("zstd"))
                        {
                            response.Headers.Add("Content-Encoding", "zstd");
                            response.ContentStream = HTTPProcessor.ZstdCompressStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Optimize);
                        }
                        else if (acceptencoding.Contains("br"))
                        {
                            response.Headers.Add("Content-Encoding", "br");
                            response.ContentStream = HTTPProcessor.BrotliCompressStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Optimize);
                        }
                        else if (acceptencoding.Contains("gzip"))
                        {
                            response.Headers.Add("Content-Encoding", "gzip");
                            response.ContentStream = HTTPProcessor.GzipCompressStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Optimize);
                        }
                        else if (acceptencoding.Contains("deflate"))
                        {
                            response.Headers.Add("Content-Encoding", "deflate");
                            response.ContentStream = HTTPProcessor.InflateStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Optimize);
                        }
                        else
                            response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
                    else
                        response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                else
                {
                    long TotalBytes = endByte - startByte; // Todo : Curl showed that we should load TotalBytes - 1, but VLC and Chrome complains about it...
                    fs.Position = startByte;

                    string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(local_path), MozaicHTTPConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                    if (ContentType == "application/octet-stream")
                    {
                        foreach (var entry in HTTPProcessor._PathernDictionary)
                        {
                            if (DataTypesUtils.FindBytePattern(DataTypesUtils.ReadSmallFileChunck(local_path, 10), entry.Value) != -1)
                            {
                                ContentType = entry.Key;
                                break;
                            }
                        }
                    }
                    if (request.RetrieveHeaderValue("User-Agent").Contains("PSHome") && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                        response = new(true, "1.0") // Home has a game bug where media files do not play well in screens/jukboxes with http 1.1.
                        {
                            HttpStatusCode = Models.HttpStatusCode.Partial_Content
                        };
                    else
                        response = new(true)
                        {
                            HttpStatusCode = Models.HttpStatusCode.Partial_Content
                        };
                    response.Headers.Add("Content-Type", ContentType);
                    response.Headers.Add("Accept-Ranges", "bytes");
                    response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", startByte, endByte - 1, filesize));
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.Headers.Add("Date", DateTime.Now.ToString("r"));
                    response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));

                    int buffersize = MozaicHTTPConfiguration.BufferSize;

                    string? encoding = null;

                    if (!response.Headers.ContainsKey("Content-Length"))
                    {
                        if (response.Headers.TryGetValue("Transfer-Encoding", out encoding) && !string.IsNullOrEmpty(encoding) && encoding.Contains("chunked"))
                        {

                        }
                        else
                            response.Headers.Add("Content-Length", TotalBytes.ToString());
                    }

                    WriteLineToStream(stream, response.ToHeader());

                    stream.Flush();

                    long bytesLeft = TotalBytes;

                    if (TotalBytes > 8000000 && buffersize < 500000) // We optimize large file handling.
                        buffersize = 500000;

                    HttpResponseContentStream ctwire = new(stream, response.Headers.ContainsKey("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked"));

                    while (bytesLeft > 0)
                    {
                        Span<byte> buffer = new byte[bytesLeft > buffersize ? buffersize : bytesLeft];
                        int n = fs.Read(buffer);

                        ctwire.Write(buffer);

                        bytesLeft -= n;
                    }

                    ctwire.WriteTerminator();

                    ctwire.Flush();

                    LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));

                    try
                    {
                        response?.ContentStream?.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                        // ContentStream has been disposed already.
                    }

                    response?.Dispose();

                    fs.Flush();
                    fs.Close();

                    return;
                }

            shortcut: // Necessary evil.

                if (response != null && request != null)
                {
                    if (response.ContentStream == null)
                        response.ContentAsUTF8 = string.Empty;

                    if (response.ContentStream != null) // Safety.
                    {
                        string EtagMD5 = ComputeStreamChecksum(response.ContentStream);

                        response.ContentStream.Position = 0;

                        if (request.Headers.TryGetValue("If-None-Match", out string? value1) && value1.Equals(EtagMD5))
                        {
                            response.Headers.Clear();

                            response.Headers.Add("ETag", EtagMD5);
                            response.Headers.Add("expires", DateTime.Now.AddMinutes(30).ToString("r"));
                            response.Headers.Add("age", "1800");

                            response.HttpStatusCode = Models.HttpStatusCode.Not_Modified;

                            WriteLineToStream(stream, response.ToHeader());

                            stream.Flush();
                        }
                        else
                        {
                            int buffersize = MozaicHTTPConfiguration.BufferSize;
                            string? encoding = null;

                            response.Headers.Add("Access-Control-Allow-Origin", "*");
                            response.Headers.Add("Date", DateTime.Now.ToString("r"));
                            response.Headers.Add("ETag", EtagMD5);
                            response.Headers.Add("expires", DateTime.Now.AddMinutes(30).ToString("r"));
                            response.Headers.Add("age", "1800");
                            response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));

                            if (!response.Headers.ContainsKey("Content-Length"))
                            {
                                if (response.Headers.TryGetValue("Transfer-Encoding", out encoding) && !string.IsNullOrEmpty(encoding) && encoding.Contains("chunked"))
                                {

                                }
                                else
                                    response.Headers.Add("Content-Length", response.ContentStream.Length.ToString());
                            }

                            WriteLineToStream(stream, response.ToHeader());

                            stream.Flush();

                            long totalBytes = response.ContentStream.Length;
                            long bytesLeft = totalBytes;

                            if (totalBytes > 8000000 && buffersize < 500000) // We optimize large file handling.
                                buffersize = 500000;

                            HttpResponseContentStream ctwire = new(stream, response.Headers.ContainsKey("Transfer-Encoding") && response.Headers["Transfer-Encoding"].Contains("chunked"));

                            while (bytesLeft > 0)
                            {
                                Span<byte> buffer = new byte[bytesLeft > buffersize ? buffersize : bytesLeft];
                                int n = response.ContentStream.Read(buffer);

                                ctwire.Write(buffer);

                                bytesLeft -= n;
                            }

                            ctwire.WriteTerminator();

                            ctwire.Flush();

                        }

                        if (response.HttpStatusCode == Models.HttpStatusCode.OK || response.HttpStatusCode == Models.HttpStatusCode.Partial_Content
                                    || response.HttpStatusCode == Models.HttpStatusCode.MovedPermanently || response.HttpStatusCode == Models.HttpStatusCode.Not_Modified)
                            LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                        else
                        {
                            if (response.HttpStatusCode == Models.HttpStatusCode.Not_Found)
                                LoggerAccessor.LogWarn(string.Format("[HTTP] - {0}:{1} Requested a non-existant file: {2} -> {3}", request.IP, request.Port, local_path, response.HttpStatusCode));
                            else if (response.HttpStatusCode == Models.HttpStatusCode.NotImplemented || response.HttpStatusCode == Models.HttpStatusCode.RangeNotSatisfiable)
                                LoggerAccessor.LogWarn(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                            else
                                LoggerAccessor.LogError(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                        }
                    }
                    else
                        response = null; // If null, simply not respond to client.
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

            try
            {
                response?.ContentStream?.Close();
            }
            catch (ObjectDisposedException)
            {
                // ContentStream has been disposed already.
            }

            response?.Dispose();

            fs.Flush();
            fs.Close();
        }

        private static void WriteLineToStream(Stream stream, string text)
        {
            stream.Write(Encoding.UTF8.GetBytes(text).AsSpan());
        }

        protected virtual Stream GetStream(TcpClient tcpClient)
        {
            if (sslCertificate != null)
            {
                SslStream sslStream;

                // Get client stream
                NetworkStream stream = tcpClient.GetStream();

                // Wrap client in SSLstream
                if (MozaicHTTPConfiguration.AcceptInvalidSSLCertificates)
                    sslStream = new(stream, false, new RemoteCertificateValidationCallback(AcceptCertificate));
                else
                    sslStream = new(stream, false);

                // Authenticate server with self-signed certificate, false (for not requiring client authentication), SSL protocol type, ...
                sslStream.AuthenticateAsServer(sslCertificate,
                                                    false,
                                                    GetSslProtocol,
                                                    !MozaicHTTPConfiguration.AcceptInvalidSSLCertificates);

                return sslStream;
            }

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

        protected virtual HttpRequest AppendRequestOrInputStream(Stream inputStream, HttpRequest req, string clientip, string? clientport, ushort ListenerPort)
		{
			HttpRequest? newRequest = GetRequest(inputStream, clientip, clientport?.ToString(), ListenerPort);

			if (newRequest != null)
			{
				req.Dispose();
				return newRequest;
			}
			else
			{
                if (req.DataAsBytes != null)
                    req.Data = DataTypesUtils.CombineByteArray(req.DataAsBytes, ReadStreamFully(inputStream));
                else
                    req.Data = ReadStreamFully(inputStream);
            }

            return req;
		}

        protected virtual HttpRequest? GetRequest(Stream inputStream, string clientip, string? clientport, ushort ListenerPort)
		{
			// Read Request Line and check if valid.
			string[] tokens = Readline(inputStream).Split(' ');

			if (tokens.Length == 3 && tokens[2].Contains("HTTP/"))
			{
				string line = string.Empty;
				// string protocolVersion = tokens[2]; // Unused.

				// Read Headers
				Dictionary<string, string> headers = new();

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

					headers.TryAdd(line[..separator], line[pos..]);
				}

				HttpRequest req = new()
				{
					Method = tokens[0].ToUpper(),
					Url = HTTPProcessor.DecodeUrl(tokens[1]),
					Headers = headers,
					IP = clientip,
					Port = clientport,
					ServerPort = ListenerPort
				};

				if (TryGetContentLength(headers, out string? value))
                {
                    req.ContentLength = Convert.ToInt32(value);
                    req.Data = ReadStreamPartially(inputStream, req.ContentLength);
                }

                return req;
			}

			return null;
		}

        private byte[] ReadStreamPartially(Stream input, long sizeToCopy)
        {
            int bytesRead = 0;
            long totalBytesCopied = 0;
            byte[] buffer = new byte[ProcbufferSize];

            using MemoryStream ms = new();

            while (totalBytesCopied < sizeToCopy && (bytesRead = input.Read(buffer, 0, (int)Math.Min(ProcbufferSize, sizeToCopy - totalBytesCopied))) > 0)
            {
                ms.Write(buffer, 0, bytesRead);
                totalBytesCopied += bytesRead;
            }

            return ms.ToArray();
        }

        private byte[] ReadStreamFully(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!input.CanRead) throw new InvalidOperationException("Input stream is not readable");

            int read = 0;
            byte[] buffer = new byte[16 * 1024];
            using MemoryStream ms = new();

            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }

            return ms.ToArray();
        }

        private bool AcceptCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static bool TryGetContentLength(Dictionary<string, string> headers, out string? value)
        {
            foreach (KeyValuePair<string, string> header in headers)
            {
                if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                {
                    value = header.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Compute the checksum of a stream.
        /// <para>Calcul la somme des contr�les d'un stream.</para>
        /// </summary>
        /// <param name="input">The input stream (must be seekable).</param>
        /// <returns>A string.</returns>
        private static string ComputeStreamChecksum(Stream input)
        {
            if (!input.CanSeek)
                return string.Empty;

            byte[] bytes = MD5.Create().ComputeHash(input);

            StringBuilder builder = new();
            for (int i = 0; i < bytes.Length; i++)
                builder.Append(bytes[i].ToString("x2"));

            return builder.ToString();
        }

        internal static SslProtocols GetSslProtocol
        {
            get
            {
#pragma warning disable
                SslProtocols protocols = SslProtocols.Ssl2 | SslProtocols.Default | SslProtocols.Tls11 | SslProtocols.Tls12;
#pragma warning restore
#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER

                protocols |= SslProtocols.Tls13;

#endif

                return protocols;
            }
        }


#if NET7_0_OR_GREATER
        [GeneratedRegex("Match (\\d+) (.*) (.*)$")]
        private static partial Regex ApacheMatchRegex();
        [GeneratedRegex("^(GET|POST|PUT|DELETE|HEAD|OPTIONS|PATCH)")]
        private static partial Regex HttpMethodRegex();
        [GeneratedRegex("\\b\\d{3}\\b")]
        private static partial Regex HttpStatusCodeRegex();
#endif
        #endregion
    }
}