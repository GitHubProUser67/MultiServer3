// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain
using BackendProject.FileHelper.Utils;
using BackendProject.MiscUtils;
using BackendProject.WebAPIs;
using BackendProject.WebAPIs.OHS;
using BackendProject.WebAPIs.OUWF;
using BackendProject.WebAPIs.PREMIUMAGENCY;
using BackendProject.WeBAPIs.VEEMEE;
using BackendProject.WebAPIs.JUGGERNAUT;
using BackendProject.WebAPIs.NDREAMS;
using BackendProject.WebTools;
using CustomLogger;
using HttpMultipartParser;
using HTTPServer.Extensions;
using HTTPServer.Models;
using HTTPServer.RouteHandlers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using BackendProject.WebAPIs.HOMECORE;

namespace HTTPServer
{
    public class HttpProcessor
    {
        #region Fields

        private readonly List<Route> Routes = new();

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

        public static bool IsIPAllowed(string ipAddress)
        {
            if ((HTTPServerConfiguration.AllowedIPs != null && HTTPServerConfiguration.AllowedIPs.Contains(ipAddress))
                || ipAddress == "127.0.0.1" || ipAddress.ToLower() == "localhost"
                || ipAddress.ToLower() == VariousUtils.GetLocalIPAddress().ToString().ToLower() 
                || ipAddress.ToLower() == VariousUtils.GetLocalIPAddress(true).ToString().ToLower())
                return true;

            return false;
        }

        public void HandleClient(TcpClient tcpClient, int ListenerPort)
        {
            try
            {
                string? clientip = ((IPEndPoint?)tcpClient.Client.RemoteEndPoint)?.Address.ToString();

                int? clientport = ((IPEndPoint?)tcpClient.Client.RemoteEndPoint)?.Port;

                if (clientport == null || string.IsNullOrEmpty(clientip) || IsIPBanned(clientip, clientport))
                {
                    tcpClient.Close();
                    tcpClient.Dispose();
                    return;
                }

                using Stream? inputStream = GetInputStream(tcpClient);
                using (Stream? outputStream = GetOutputStream(tcpClient))
                {
                    while (tcpClient.IsConnected())
                    {
                        if (tcpClient.Available > 0 && outputStream.CanWrite)
                        {
                            HttpRequest? request = GetRequest(inputStream, clientip, clientport.ToString());

                            if (request != null && !string.IsNullOrEmpty(request.Url) && !request.GetHeaderValue("User-Agent").ToLower().Contains("bytespider")) // Get Away TikTok.
                            {
                                string Host = request.GetHeaderValue("Host");

                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested the HTTP Server with URL : {request.Url}");

                                string absolutepath = HTTPUtils.ExtractDirtyProxyPath(request.GetHeaderValue("Referer")) + HTTPUtils.RemoveQueryString(request.Url);

                                // Split the URL into segments
                                string[] segments = absolutepath.Trim('/').Split('/');

                                // Combine the folder segments into a directory path
                                string directoryPath = Path.Combine(HTTPServerConfiguration.HTTPStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                                // Process the request based on the HTTP method
                                string filePath = Path.Combine(HTTPServerConfiguration.HTTPStaticFolder, absolutepath[1..]);

                                string apiPath = Path.Combine(HTTPServerConfiguration.APIStaticFolder, absolutepath[1..]);

                                HttpResponse? response = RouteRequest(inputStream, outputStream, request, absolutepath, Host);

                                if (response == null)
                                {
                                    switch (Host)
                                    {
                                        default:
                                            // A little bit out of the scope of Routes.
                                            if (absolutepath.Contains("/!plugin/") && HTTPServerConfiguration.plugins.Count > 0)
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested a Plugin method : {absolutepath}");
                                                int i = 0;
                                                foreach (PluginManager.HTTPPlugin plugin in HTTPServerConfiguration.plugins)
                                                {
                                                    response = plugin.ProcessPluginMessage(request);
                                                    if (response != null)
                                                        break;
                                                    i++;
                                                }
                                            }

                                            else if ((Host == "stats.outso-srv1.com" || Host == "www.outso-srv1.com") && request.GetDataStream != null && absolutepath.EndsWith("/") && (absolutepath.Contains("/ohs") || absolutepath.Contains("/statistic/")))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested a OHS method : {absolutepath}");

                                                string? res = null;
                                                int version = 0;
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
                                                using (MemoryStream postdata = new())
                                                {
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new OHSClass(request.Method, absolutepath, version).ProcessRequest(postdata.ToArray(), request.GetContentType(), apiPath);
                                                    postdata.Flush();
                                                }
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send($"<ohs>{res}</ohs>", "application/xml;charset=UTF-8");
                                            }
                                            else if ((Host == "ouwf.outso-srv1.com") && request.GetDataStream != null && request.Method != null && request.GetContentType().StartsWith("multipart/form-data"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip} Requested a OuWF method : {absolutepath}");

                                                string? res = null;
                                                using (MemoryStream postdata = new())
                                                {
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new OuWFClass(request.Method, absolutepath, HTTPServerConfiguration.HTTPStaticFolder).ProcessRequest(postdata.ToArray(), request.GetContentType());
                                                    postdata.Flush();
                                                }
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "text/xml");
                                            }
                                            else if ((Host == "away.veemee.com" || Host == "home.veemee.com") && request.Method != null && absolutepath.EndsWith(".php"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested a VEEMEE  method : {absolutepath}");

                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    var res = new VEEMEEClass(request.Method, absolutepath).ProcessRequest(postdata.ToArray(), request.GetContentType(), absolutepath);
                                                    postdata.Flush();

                                                    if (string.IsNullOrEmpty(res.Item1))
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
                                            else if (Host == "pshome.ndreams.net" && request.Method != null && absolutepath.EndsWith(".php"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested a NDREAMS method : {absolutepath}");

                                                string? res = null;
                                                NDREAMSClass ndreams = new NDREAMSClass(request.Method, absolutepath);
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
                                            else if (Host == "juggernaut-games.com" && request.Method != null && absolutepath.EndsWith(".php"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested a JUGGERNAUT method : {absolutepath}");

                                                string? res = null;
                                                JUGGERNAUTClass juggernaut = new(request.Method, absolutepath);
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = juggernaut.ProcessRequest(request.QueryParameters, postdata.ToArray(), request.GetContentType());
                                                    postdata.Flush();
                                                }
                                                else
                                                    res = juggernaut.ProcessRequest(request.QueryParameters);
                                                juggernaut.Dispose();
                                                if (res == null)
                                                    response = HttpBuilder.InternalServerError();
                                                else if (res == string.Empty)
                                                    response = HttpBuilder.OK();
                                                else
                                                    response = HttpResponse.Send(res, "text/xml");
                                            }
                                            else if ((Host == "test.playstationhome.jp" || Host == "playstationhome.jp" || Host == "scej-home.playstation.net" || Host == "homeec.scej-nbs.jp") && request.Method != null && request.GetContentType().StartsWith("multipart/form-data") && absolutepath.Contains("/eventController/") && absolutepath.EndsWith(".do"))
                                            {
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested a PREMIUMAGENCY method : {absolutepath}");

                                                string? res = null;
                                                if (request.GetDataStream != null)
                                                {
                                                    using MemoryStream postdata = new();
                                                    request.GetDataStream.CopyTo(postdata);
                                                    res = new PREMIUMAGENCYClass(request.Method, absolutepath, HTTPServerConfiguration.APIStaticFolder).ProcessRequest(postdata.ToArray(), request.GetContentType());
                                                    postdata.Flush();
                                                }
                                                if (string.IsNullOrEmpty(res))
                                                    response = HttpBuilder.InternalServerError();
                                                else
                                                    response = HttpResponse.Send(res, "text/xml");
                                            }
                                            else
                                            {
                                                string? encoding = request.GetHeaderValue("Accept-Encoding");

                                                switch (request.Method)
                                                {
                                                    case "GET":
                                                        switch (absolutepath)
                                                        {
                                                            case "/publisher/list/":
                                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested a HOMECORE method : {absolutepath}");

                                                                string? res = null;
                                                                HOMECOREClass homecore = new(request.Method, absolutepath);
                                                                if (request.GetDataStream != null)
                                                                {
                                                                    using MemoryStream postdata = new();
                                                                    request.GetDataStream.CopyTo(postdata);
                                                                    res = homecore.ProcessRequest(postdata.ToArray(), request.GetContentType(), HTTPServerConfiguration.APIStaticFolder);
                                                                    postdata.Flush();
                                                                }
                                                                homecore.Dispose();
                                                                if (string.IsNullOrEmpty(res))
                                                                    response = HttpBuilder.InternalServerError();
                                                                else
                                                                    response = HttpResponse.Send(res, "text/xml");
                                                                break;
                                                            case "/networktest/get_2m":
                                                                response = HttpResponse.Send(new byte[2097152]);
                                                                break;
                                                            case "/robots.txt":
                                                                response = HttpResponse.Send("User-agent: *\nDisallow: / "); // Get Away Google.
                                                                break;
                                                            case "/!player":
                                                            case "/!player/":
                                                                // We want to check if the router allows external IPs first.
                                                                string ServerIP = VariousUtils.GetPublicIPAddress(true);
                                                                try
                                                                {
                                                                    using TcpClient client = new(ServerIP, ListenerPort);
                                                                    client.Close();
                                                                }
                                                                catch (Exception) // Failed to connect, so we fallback to local IP.
                                                                {
                                                                    ServerIP = VariousUtils.GetLocalIPAddress(true).ToString();
                                                                }
                                                                if (ServerIP.Length > 15)
                                                                    ServerIP = "[" + ServerIP + "]"; // Format the hostname if it's a IPV6 url format.
                                                                WebVideoPlayer? WebPlayer = new((request.QueryParameters ?? new Dictionary<string, string>()).Aggregate(new NameValueCollection(),
                                                                    (seed, current) => {
                                                                        seed.Add(current.Key, current.Value);
                                                                        return seed;
                                                                    }), $"http://{ServerIP}/!webvideo/?");
                                                                response = HttpResponse.Send(WebPlayer.HtmlPage, "text/html", WebPlayer.HeadersToSet);
                                                                WebPlayer = null;
                                                                break;
                                                            case "/!webvideo":
                                                            case "/!webvideo/":
                                                                if (request.GetHeaderValue("User-Agent").Contains("PSHome")) // The game is imcompatible with the webvideo, and it can even spam request it, so we forbid.
                                                                    response = HttpBuilder.NotAllowed();
                                                                else
                                                                {
                                                                    Dictionary<string, string>? QueryDic = request.QueryParameters;
                                                                    if (QueryDic != null && QueryDic.Count > 0 && QueryDic.ContainsKey("url") && !string.IsNullOrEmpty(QueryDic["url"]))
                                                                    {
                                                                        WebVideo? vid = WebVideoConverter.ConvertVideo(QueryDic, HTTPServerConfiguration.ConvertersFolder);
                                                                        if (vid != null && vid.Available)
                                                                            response = HttpResponse.Send(vid.VideoStream, vid.ContentType, new string[][] { new string[] { "Content-Disposition", "attachment; filename=\"" + vid.FileName + "\"" } },
                                                                                Models.HttpStatusCode.OK);
                                                                        else
                                                                            response = new HttpResponse(request.GetHeaderValue("Connection") == "keep-alive")
                                                                            {
                                                                                HttpStatusCode = Models.HttpStatusCode.OK,
                                                                                ContentAsUTF8 = "<p>" + vid?.ErrorMessage + "</p>" +
                                                                                        "<p>Make sure that parameters are correct, and both <i>yt-dlp</i> and <i>ffmpeg</i> are properly installed on the server.</p>",
                                                                                Headers = { { "Content-Type", "text/html" } }
                                                                            };
                                                                    }
                                                                    else
                                                                        response = new HttpResponse(request.GetHeaderValue("Connection") == "keep-alive")
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
                                                            case "/!GetMediaList":
                                                            case "/!GetMediaList/":
                                                                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                                                    response = HttpResponse.Send(HTTPUtils.Compress(Encoding.UTF8.GetBytes(FileStructureToJson.GetMediaFilesAsJson(HTTPServerConfiguration.HTTPStaticFolder + "/!MediaPath"))), "application/json", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                                                                else
                                                                    response = HttpResponse.Send(FileStructureToJson.GetMediaFilesAsJson(HTTPServerConfiguration.HTTPStaticFolder + "/!MediaPath"), "application/json");
                                                                break;
                                                            default:
                                                                if (absolutepath.ToLower().EndsWith(".php") && !string.IsNullOrEmpty(HTTPServerConfiguration.PHPRedirectUrl))
                                                                    response = HttpBuilder.PermanantRedirect($"{HTTPServerConfiguration.PHPRedirectUrl}{request.Url}");
                                                                else if (absolutepath.ToLower().EndsWith(".php") && Directory.Exists(HTTPServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                                                {
                                                                    (byte[]?, string[][]) CollectPHP = PHP.ProcessPHPPage(filePath, HTTPServerConfiguration.PHPStaticFolder, HTTPServerConfiguration.PHPVersion, clientip, clientport.ToString(), request);
                                                                    if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && CollectPHP.Item1 != null)
                                                                        response = HttpResponse.Send(HTTPUtils.Compress(CollectPHP.Item1), "text/html", VariousUtils.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "gzip" }));
                                                                    else
                                                                        response = HttpResponse.Send(CollectPHP.Item1, "text/html", CollectPHP.Item2);
                                                                }
                                                                else
                                                                {
                                                                    if (File.Exists(filePath) && request.Headers.Keys.Count(x => x == "Range") == 1) // Mmm, is it possible to have more?
                                                                        Handle_LocalFile_Stream(outputStream, request, filePath);
                                                                    else
                                                                        response = FileSystemRouteHandler.Handle(request, filePath, $"http://example.com{absolutepath[..^1]}", clientip, clientport.ToString());
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                    case "POST":
                                                        switch (absolutepath)
                                                        {
                                                            case "/networktest/post_128":
                                                                response = HttpBuilder.OK();
                                                                break;
                                                            case "/!HomeTools/MakeBarSdat/":
                                                                if (IsIPAllowed(clientip))
                                                                {
                                                                    var makeres = HomeToolsInterface.MakeBarSdat(request.GetDataStream, request.GetContentType());
                                                                    if (makeres != null)
                                                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, makeres.Value.Item1, makeres.Value.Item2);
                                                                    else
                                                                        response = HttpBuilder.InternalServerError();
                                                                }
                                                                else
                                                                    response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                break;
                                                            case "/!HomeTools/UnBar/":
                                                                if (IsIPAllowed(clientip))
                                                                {
                                                                    var unbarres = HomeToolsInterface.UnBarAsync(request.GetDataStream, request.GetContentType(), HTTPServerConfiguration.HomeToolsHelperStaticFolder).Result;
                                                                    if (unbarres != null)
                                                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, unbarres.Value.Item1, unbarres.Value.Item2);
                                                                    else
                                                                        response = HttpBuilder.InternalServerError();
                                                                }
                                                                else
                                                                    response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                break;
                                                            case "/!HomeTools/CDS/":
                                                                if (IsIPAllowed(clientip))
                                                                {
                                                                    var cdsres = HomeToolsInterface.CDS(request.GetDataStream, request.GetContentType());
                                                                    if (cdsres != null)
                                                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, cdsres.Value.Item1, cdsres.Value.Item2);
                                                                    else
                                                                        response = HttpBuilder.InternalServerError();
                                                                }
                                                                else
                                                                    response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                break;
                                                            case "/!HomeTools/CDSBruteforce/":
                                                                if (IsIPAllowed(clientip))
                                                                {
                                                                    var cdsres = HomeToolsInterface.CDSBruteforceAsync(request.GetDataStream, request.GetContentType(), HTTPServerConfiguration.HomeToolsHelperStaticFolder).Result;
                                                                    if (cdsres != null)
                                                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, cdsres.Value.Item1, cdsres.Value.Item2);
                                                                    else
                                                                        response = HttpBuilder.InternalServerError();
                                                                }
                                                                else
                                                                    response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                break;
                                                            case "/!HomeTools/HCDBUnpack/":
                                                                if (IsIPAllowed(clientip))
                                                                {
                                                                    var cdsres = HomeToolsInterface.HCDBUnpack(request.GetDataStream, request.GetContentType());
                                                                    if (cdsres != null)
                                                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, cdsres.Value.Item1, cdsres.Value.Item2);
                                                                    else
                                                                        response = HttpBuilder.InternalServerError();
                                                                }
                                                                else
                                                                    response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                break;
                                                            case "/!HomeTools/TicketList/":
                                                                if (IsIPAllowed(clientip))
                                                                {
                                                                    var ticketlistres = HomeToolsInterface.TicketList(request.GetDataStream, request.GetContentType());
                                                                    if (ticketlistres != null)
                                                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, ticketlistres.Value.Item1, ticketlistres.Value.Item2);
                                                                    else
                                                                        response = HttpBuilder.InternalServerError();
                                                                }
                                                                else
                                                                    response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                break;
                                                            case "/!HomeTools/INF/":
                                                                if (IsIPAllowed(clientip))
                                                                {
                                                                    var infres = HomeToolsInterface.INF(request.GetDataStream, request.GetContentType());
                                                                    if (infres != null)
                                                                        response = FileSystemRouteHandler.Handle_ByteSubmit_Download(request, infres.Value.Item1, infres.Value.Item2);
                                                                    else
                                                                        response = HttpBuilder.InternalServerError();
                                                                }
                                                                else
                                                                    response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                break;
                                                            case "/!HomeTools/ChannelID/":
                                                                if (IsIPAllowed(clientip))
                                                                {
                                                                    string? channelres = HomeToolsInterface.ChannelID(request.GetDataStream, request.GetContentType());
                                                                    if (!string.IsNullOrEmpty(channelres))
                                                                        response = HttpResponse.Send(channelres);
                                                                    else
                                                                        response = HttpBuilder.InternalServerError();
                                                                }
                                                                else
                                                                    response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                break;
                                                            case "/!HomeTools/SceneID/":
                                                                if (IsIPAllowed(clientip))
                                                                {
                                                                    string? sceneres = HomeToolsInterface.SceneID(request.GetDataStream, request.GetContentType());
                                                                    if (!string.IsNullOrEmpty(sceneres))
                                                                        response = HttpResponse.Send(sceneres);
                                                                    else
                                                                        response = HttpBuilder.InternalServerError();
                                                                }
                                                                else
                                                                    response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                break;
                                                            default:
                                                                if (absolutepath.ToLower().EndsWith(".php") && !string.IsNullOrEmpty(HTTPServerConfiguration.PHPRedirectUrl))
                                                                    response = HttpBuilder.PermanantRedirect($"{HTTPServerConfiguration.PHPRedirectUrl}{request.Url}");
                                                                else if (absolutepath.ToLower().EndsWith(".php") && Directory.Exists(HTTPServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                                                {
                                                                    var CollectPHP = PHP.ProcessPHPPage(filePath, HTTPServerConfiguration.PHPStaticFolder, HTTPServerConfiguration.PHPVersion, clientip, clientport.ToString(), request);
                                                                    if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && CollectPHP.Item1 != null)
                                                                        response = HttpResponse.Send(HTTPUtils.Compress(CollectPHP.Item1), "text/html", VariousUtils.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "gzip" }));
                                                                    else
                                                                        response = HttpResponse.Send(CollectPHP.Item1, "text/html", CollectPHP.Item2);
                                                                }
                                                                else
                                                                    response = HttpBuilder.NotFound();
                                                                break;
                                                        }
                                                        break;
                                                    case "PUT":
                                                        if (HTTPServerConfiguration.EnablePUTMethod)
                                                        {
                                                            string ContentType = request.GetContentType();
                                                            if (request.GetDataStream != null && !string.IsNullOrEmpty(ContentType))
                                                            {
                                                                string? boundary = HTTPUtils.ExtractBoundary(ContentType);
                                                                if (!string.IsNullOrEmpty(boundary))
                                                                {
                                                                    string UploadDirectoryPath = HTTPServerConfiguration.HTTPTempFolder + $"/DataUpload/{absolutepath[1..]}";
                                                                    Directory.CreateDirectory(UploadDirectoryPath);
                                                                    var data = MultipartFormDataParser.Parse(request.GetDataStream, boundary);
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

                                                                    response = HttpBuilder.OK();
                                                                }
                                                                else
                                                                    response = HttpBuilder.NotAllowed();
                                                            }
                                                            else
                                                                response = HttpBuilder.NotAllowed();
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
                                                            case "/!webvideo":
                                                            case "/!webvideo/":
                                                                if (request.GetHeaderValue("User-Agent").Contains("PSHome")) // The game is imcompatible with the webvideo, and it can even spam request it, so we forbid.
                                                                    response = HttpBuilder.NotAllowed();
                                                                else
                                                                {
                                                                    Dictionary<string, string>? QueryDic = request.QueryParameters;
                                                                    if (QueryDic != null && QueryDic.Count > 0 && QueryDic.ContainsKey("url") && !string.IsNullOrEmpty(QueryDic["url"]))
                                                                    {
                                                                        WebVideo? vid = WebVideoConverter.ConvertVideo(QueryDic, HTTPServerConfiguration.ConvertersFolder);
                                                                        if (vid != null && vid.Available && vid.VideoStream != null)
                                                                        {
                                                                            using HugeMemoryStream ms = new(vid.VideoStream, HTTPServerConfiguration.BufferSize);
                                                                            response = new(request.GetHeaderValue("Connection") == "keep-alive")
                                                                            {
                                                                                HttpStatusCode = Models.HttpStatusCode.OK
                                                                            };
                                                                            response.Headers.Add("Content-Type", vid.ContentType);
                                                                            response.Headers.Add("Content-Length", ms.Length.ToString());
                                                                            ms.Flush();
                                                                        }
                                                                        else
                                                                            response = HttpBuilder.InternalServerError();
                                                                    }
                                                                    else
                                                                        response = HttpBuilder.MissingParameters();
                                                                }
                                                                break;
                                                            default:
                                                                response = FileSystemRouteHandler.HandleHEAD(request, filePath);
                                                                break;
                                                        }
                                                        break;
                                                    case "OPTIONS":
                                                        response = HttpBuilder.OK();
                                                        response.Headers.Add("Allow", "OPTIONS, GET, HEAD, POST");
                                                        break;
                                                    case "PROPFIND":
                                                        response = HttpBuilder.NotImplemented();
                                                        break;
                                                    default:
                                                        response = HttpBuilder.NotAllowed();
                                                        break;
                                                }
                                            }
                                            break;
                                    }
                                }

                                if (response != null)
                                    WriteResponse(outputStream, request, response, filePath);

                                request.Dispose();
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

        private static void WriteResponse(Stream stream, HttpRequest? request, HttpResponse? response, string filePath)
        {
            try
            {
                if (response != null && request != null)
                {
                    if (response.ContentStream == null)
                        response.ContentAsUTF8 = string.Empty;

                    if (response.ContentStream != null) // Safety.
                    {
                        string EtagMD5 = VariousUtils.ComputeMD5(response.ContentStream);

                        if (request.Method == "OPTIONS")
                        {
                            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, HEAD");
                            response.Headers.Add("Access-Control-Max-Age", "1728000");
                        }

                        if (request.Headers.ContainsKey("If-Modified-Since") && DateTime.TryParse(request.Headers["If-Modified-Since"], out DateTime HeaderTimeCheck) && HeaderTimeCheck >= new FileInfo(filePath).LastWriteTimeUtc)
                        {
                            response.Headers.Clear();

                            response.Headers.Add("ETag", EtagMD5);

                            response.HttpStatusCode = Models.HttpStatusCode.Not_Modified;

                            WriteLineToStream(stream, response.ToHeader());

                            stream.Flush();
                        }
                        else if (request.Headers.ContainsKey("If-None-Match") && request.Headers["If-None-Match"] == EtagMD5)
                        {
                            response.Headers.Clear();

                            response.Headers.Add("ETag", EtagMD5);

                            response.HttpStatusCode = Models.HttpStatusCode.Not_Modified;

                            WriteLineToStream(stream, response.ToHeader());

                            stream.Flush();
                        }
                        else
                        {
                            int buffersize = HTTPServerConfiguration.BufferSize;
                            string? encoding = null;

                            response.Headers.Add("Access-Control-Allow-Origin", "*");
                            response.Headers.Add("Server", VariousUtils.GenerateServerSignature());

                            if (!response.Headers.ContainsKey("Content-Type"))
                                response.Headers.Add("Content-Type", "text/plain");

                            if (response.HttpStatusCode == Models.HttpStatusCode.OK || response.HttpStatusCode == Models.HttpStatusCode.Partial_Content)
                            {
                                response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                response.Headers.Add("ETag", EtagMD5);
                                if (File.Exists(filePath))
                                    response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
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

                        if (response.HttpStatusCode == Models.HttpStatusCode.OK || response.HttpStatusCode == Models.HttpStatusCode.Partial_Content
                                || response.HttpStatusCode == Models.HttpStatusCode.MovedPermanently || response.HttpStatusCode == Models.HttpStatusCode.Not_Modified)
                            LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                        else
                        {
                            if (response.HttpStatusCode == Models.HttpStatusCode.Not_Found)
                                LoggerAccessor.LogWarn(string.Format("{0} Requested a non-existant file -> {1}", filePath, response.HttpStatusCode));
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

        private static void Handle_LocalFile_Stream(Stream stream, HttpRequest request, string local_path)
        {
            // This method directly communicate with the wire to handle, normally, imposible transfers.
            // If a part of the code sounds weird to you, it's normal... So does curl tests...

            const int rangebuffersize = 32768;

            string? acceptencoding = request.GetHeaderValue("Accept-Encoding");

            HttpResponse? response = null;

            using (FileStream fs = new(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                long startByte = -1;
                long endByte = -1;
                try
                {
                    long filesize = fs.Length;
                    string HeaderString = request.GetHeaderValue("Range").Replace("bytes=", string.Empty);
                    if (HeaderString.Contains(','))
                    {
                        using HugeMemoryStream ms = new();
                        int buffersize = HTTPServerConfiguration.BufferSize;
                        Span<byte> Separator = new byte[] { 0x0D, 0x0A };
                        string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
                        if (ContentType == "application/octet-stream")
                        {
                            byte[] VerificationChunck = VariousUtils.ReadSmallFileChunck(local_path, 10);
                            foreach (var entry in HTTPUtils.PathernDictionary)
                            {
                                if (VariousUtils.FindbyteSequence(VerificationChunck, entry.Value))
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
                                response = new(request.GetHeaderValue("Connection") == "keep-alive")
                                {
                                    HttpStatusCode = Models.HttpStatusCode.RangeNotSatisfiable
                                };
                                response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                                response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                                if (!string.IsNullOrEmpty(acceptencoding) && acceptencoding.Contains("gzip"))
                                {
                                    response.Headers.Add("Content-Encoding", "gzip");
                                    response.ContentStream = new MemoryStream(HTTPUtils.Compress(Encoding.UTF8.GetBytes(payload)));
                                }
                                else
                                    response.ContentAsUTF8 = payload;
                                goto shortcut; // Do we really have the choice?
                            }
                            else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                            {
                                ms.Flush();
                                ms.Close();
                                if (request.GetHeaderValue("User-Agent").Contains("PSHome") && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                                    response = new(request.GetHeaderValue("Connection") == "keep-alive", "1.0") // Home has a game bug where media files do not play well in screens/jukboxes with http 1.1.
                                    {
                                        HttpStatusCode = Models.HttpStatusCode.OK
                                    };
                                else
                                    response = new(request.GetHeaderValue("Connection") == "keep-alive")
                                    {
                                        HttpStatusCode = Models.HttpStatusCode.OK
                                    };
                                response.Headers.Add("Accept-Ranges", "bytes");
                                response.Headers.Add("Content-Type", ContentType);
                                if (!string.IsNullOrEmpty(acceptencoding) && acceptencoding.Contains("deflate") && new FileInfo(local_path).Length <= 80000000) // We must be reasonable on the file-size here (80 Mb).
                                {
                                    response.Headers.Add("Content-Encoding", "deflate");
                                    response.ContentStream = HTTPUtils.InflateStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
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
                        if (request.GetHeaderValue("User-Agent").Contains("PSHome") && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
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
                        response.Headers.Add("Server", VariousUtils.GenerateServerSignature());
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

                        response = new(request.GetHeaderValue("Connection") == "keep-alive")
                        {
                            HttpStatusCode = Models.HttpStatusCode.RangeNotSatisfiable
                        };
                        response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                        response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                        if (!string.IsNullOrEmpty(acceptencoding) && acceptencoding.Contains("gzip"))
                        {
                            response.Headers.Add("Content-Encoding", "gzip");
                            response.ContentStream = new MemoryStream(HTTPUtils.Compress(Encoding.UTF8.GetBytes(payload)));
                        }
                        else
                            response.ContentAsUTF8 = payload;
                    }
                    else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                    {
                        string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
                        if (ContentType == "application/octet-stream")
                        {
                            byte[] VerificationChunck = VariousUtils.ReadSmallFileChunck(local_path, 10);
                            foreach (var entry in HTTPUtils.PathernDictionary)
                            {
                                if (VariousUtils.FindbyteSequence(VerificationChunck, entry.Value))
                                {
                                    ContentType = entry.Key;
                                    break;
                                }
                            }
                        }
                        if (request.GetHeaderValue("User-Agent").Contains("PSHome") && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                            response = new(request.GetHeaderValue("Connection") == "keep-alive", "1.0") // Home has a game bug where media files do not play well in screens/jukboxes with http 1.1.
                            {
                                HttpStatusCode = Models.HttpStatusCode.OK
                            };
                        else
                            response = new(request.GetHeaderValue("Connection") == "keep-alive")
                            {
                                HttpStatusCode = Models.HttpStatusCode.OK
                            };
                        response.Headers.Add("Accept-Ranges", "bytes");
                        response.Headers.Add("Content-Type", ContentType);
                        if (!string.IsNullOrEmpty(acceptencoding) && acceptencoding.Contains("deflate") && new FileInfo(local_path).Length <= 80000000) // We must be reasonable on the file-size here (80 Mb).
                        {
                            response.Headers.Add("Content-Encoding", "deflate");
                            response.ContentStream = HTTPUtils.InflateStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        }
                        else
                            response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
                    else
                    {
                        long TotalBytes = endByte - startByte; // Todo : Curl showed that we should load TotalBytes - 1, but VLC and Chrome complains about it...
                        fs.Position = startByte;
						
                        string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
                        if (ContentType == "application/octet-stream")
                        {
                            foreach (var entry in HTTPUtils.PathernDictionary)
                            {
                                if (VariousUtils.FindbyteSequence(VariousUtils.ReadSmallFileChunck(local_path, 10), entry.Value))
                                {
                                    ContentType = entry.Key;
                                    break;
                                }
                            }
                        }
                        if (request.GetHeaderValue("User-Agent").Contains("PSHome") && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
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
                        response.Headers.Add("Server", VariousUtils.GenerateServerSignature());
                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));

                        int buffersize = HTTPServerConfiguration.BufferSize;

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
                            string EtagMD5 = VariousUtils.ComputeMD5(response.ContentStream);

                            if (request.Headers.ContainsKey("If-Modified-Since") && DateTime.TryParse(request.Headers["If-Modified-Since"], out DateTime HeaderTimeCheck) && HeaderTimeCheck >= new FileInfo(local_path).LastWriteTimeUtc)
                            {
                                response.Headers.Clear();

                                response.Headers.Add("ETag", EtagMD5);

                                response.HttpStatusCode = Models.HttpStatusCode.Not_Modified;

                                WriteLineToStream(stream, response.ToHeader());

                                stream.Flush();
                            }
                            else if (request.Headers.ContainsKey("If-None-Match") && request.Headers["If-None-Match"] == EtagMD5)
                            {
                                response.Headers.Clear();

                                response.Headers.Add("ETag", EtagMD5);

                                response.HttpStatusCode = Models.HttpStatusCode.Not_Modified;

                                WriteLineToStream(stream, response.ToHeader());

                                stream.Flush();
                            }
                            else
                            {
                                int buffersize = HTTPServerConfiguration.BufferSize;
                                string? encoding = null;

                                response.Headers.Add("Access-Control-Allow-Origin", "*");
                                response.Headers.Add("Server", VariousUtils.GenerateServerSignature());
                                response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                response.Headers.Add("ETag", EtagMD5);
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
                                    LoggerAccessor.LogWarn(string.Format("{0} Requested a non-existant file -> {1}", local_path, response.HttpStatusCode));
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
                    HttpResponse result = route.Callable(request);
                    if (result.IsValid())
                        return result;
                }
            }
            catch (Exception)
            {
                // Not Important
            }

            return null;
        }

        private static HttpRequest? GetRequest(Stream inputStream, string clientip, string? clientport)
        {
            string line = string.Empty;

            // Read Request Line
            string[] tokens = Readline(inputStream).Split(' ');
            if (tokens.Length != 3)
                return null;
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
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                headers.Add(line[..separator], line[pos..]);
            }

            HttpRequest req = new()
            {
                Method = tokens[0].ToUpper(),
                Url = HTTPUtils.DecodeUrl(tokens[1]),
                Headers = headers,
                Data = null,
                IP = clientip,
                PORT = clientport
            };

            if (headers.ContainsKey("Content-Length"))
            {
                int bytesLeft = Convert.ToInt32(headers["Content-Length"]); // No more than 2gb

                using MemoryStream contentStream = new();
                while (bytesLeft > 0)
                {
                    Span<byte> buffer = new byte[bytesLeft > HTTPServerConfiguration.BufferSize ? HTTPServerConfiguration.BufferSize : bytesLeft];
                    int n = inputStream.Read(buffer);

                    contentStream.Write(buffer);

                    bytesLeft -= n;
                }

                contentStream.Position = 0;

                if (req.Data == null)
                {
                    req.Data = new MemoryStream();
                    contentStream.CopyTo(req.Data);
                    req.Data.Position = 0;
                }

                contentStream.Flush();
            }

            return req;
        }
        #endregion
    }
}
