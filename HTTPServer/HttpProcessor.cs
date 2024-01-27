// Copyright (C) 2016 by David Jeske, Barend Erasmus and donated to the public domain
using BackendProject.MiscUtils;
using BackendProject.WebAPIs;
using BackendProject.WebAPIs.OHS;
using BackendProject.WebAPIs.PREMIUMAGENCY;
using CustomLogger;
using HTTPServer.API.JUGGERNAUT;
using HTTPServer.API.NDREAMS;
using HTTPServer.Extensions;
using HTTPServer.Models;
using HTTPServer.RouteHandlers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace HTTPServer
{
    public class HttpProcessor
    {
        #region Fields

        private List<Route> Routes = new();

        #endregion

        #region Constructors

        public HttpProcessor()
        {

        }

        #endregion

        #region Public Methods

        public bool IsIPBanned(string ipAddress, int? clientport)
        {
            if (HTTPServerConfiguration.BannedIPs != null && HTTPServerConfiguration.BannedIPs.Contains(ipAddress))
            {
                LoggerAccessor.LogError($"[SECURITY] - {ipAddress}:{clientport} Requested the HTTP server while being banned!");
                return true;
            }

            return false;
        }

        public bool IsIPAllowed(string ipAddress)
        {
            if ((HTTPServerConfiguration.AllowedIPs != null && HTTPServerConfiguration.AllowedIPs.Contains(ipAddress))
                || ipAddress == "127.0.0.1" || ipAddress.ToLower() == "localhost")
                return true;

            return false;
        }

        public void HandleClient(TcpClient tcpClient)
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

                using (Stream? inputStream = GetInputStream(tcpClient))
                {
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
                                                else if ((Host == "stats.outso-srv1.com" || Host == "www.outso-srv1.com") && request.getDataStream != null && absolutepath.EndsWith("/") && (absolutepath.Contains("/ohs") || absolutepath.Contains("/statistic/")))
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
                                                    OHSClass ohs = new(request.Method, absolutepath, version);
                                                    using (MemoryStream postdata = new())
                                                    {
                                                        request.getDataStream.CopyTo(postdata);

                                                        postdata.Position = 0;
                                                        // Find the number of bytes in the stream
                                                        int contentLength = (int)postdata.Length;
                                                        // Create a byte array
                                                        byte[] buffer = new byte[contentLength];
                                                        // Read the contents of the memory stream into the byte array
                                                        postdata.Read(buffer, 0, contentLength);
                                                        res = ohs.ProcessRequest(buffer, request.GetContentType(), apiPath);
                                                        postdata.Flush();
                                                    }
                                                    ohs.Dispose();
                                                    if (string.IsNullOrEmpty(res))
                                                        response = HttpBuilder.InternalServerError();
                                                    else
                                                        response = HttpResponse.Send($"<ohs>{res}</ohs>", "application/xml;charset=UTF-8");
                                                }
                                                else if (Host == "pshome.ndreams.net" && request.Method != null && absolutepath.EndsWith(".php"))
                                                {
                                                    LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested a NDREAMS method : {absolutepath}");

                                                    string? res = null;
                                                    NDREAMSClass ndreams = new(request.Method, absolutepath);
                                                    if (request.getDataStream != null)
                                                    {
                                                        using (MemoryStream postdata = new())
                                                        {
                                                            request.getDataStream.CopyTo(postdata);

                                                            postdata.Position = 0;
                                                            // Find the number of bytes in the stream
                                                            int contentLength = (int)postdata.Length;
                                                            // Create a byte array
                                                            byte[] buffer = new byte[contentLength];
                                                            // Read the contents of the memory stream into the byte array
                                                            postdata.Read(buffer, 0, contentLength);
                                                            res = ndreams.ProcessRequest(request.QueryParameters, buffer, request.GetContentType());
                                                            postdata.Flush();
                                                        }
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
                                                    if (request.getDataStream != null)
                                                    {
                                                        using (MemoryStream postdata = new())
                                                        {
                                                            request.getDataStream.CopyTo(postdata);

                                                            postdata.Position = 0;
                                                            // Find the number of bytes in the stream
                                                            int contentLength = (int)postdata.Length;
                                                            // Create a byte array
                                                            byte[] buffer = new byte[contentLength];
                                                            // Read the contents of the memory stream into the byte array
                                                            postdata.Read(buffer, 0, contentLength);
                                                            res = juggernaut.ProcessRequest(request.QueryParameters, buffer, request.GetContentType());
                                                            postdata.Flush();
                                                        }
                                                    }
                                                    else
                                                        res = juggernaut.ProcessRequest(request.QueryParameters);
                                                    juggernaut.Dispose();
                                                    if (res == null)
                                                        response = HttpBuilder.InternalServerError();
                                                    else if (res == string.Empty)
                                                        response = HttpBuilder.Ok();
                                                    else
                                                        response = HttpResponse.Send(res, "text/xml");
                                                }
                                                else if ((Host == "test.playstationhome.jp" || Host == "playstationhome.jp" || Host == "scej-home.playstation.net" || Host == "homeec.scej-nbs.jp") && request.Method != null && request.GetContentType().StartsWith("multipart/form-data") && absolutepath.Contains("/eventController/") && absolutepath.EndsWith(".do"))
                                                {
                                                    LoggerAccessor.LogInfo($"[HTTP] - {clientip}:{clientport} Requested a PREMIUMAGENCY method : {absolutepath}");

                                                    string? res = null;
                                                    PREMIUMAGENCYClass agency = new(request.Method, absolutepath, HTTPServerConfiguration.APIStaticFolder);
                                                    if (request.getDataStream != null)
                                                    {
                                                        using (MemoryStream postdata = new())
                                                        {
                                                            request.getDataStream.CopyTo(postdata);

                                                            postdata.Position = 0;
                                                            // Find the number of bytes in the stream
                                                            int contentLength = (int)postdata.Length;
                                                            // Create a byte array
                                                            byte[] buffer = new byte[contentLength];
                                                            // Read the contents of the memory stream into the byte array
                                                            postdata.Read(buffer, 0, contentLength);
                                                            res = agency.ProcessRequest(buffer, request.GetContentType());
                                                            postdata.Flush();
                                                        }
                                                    }
                                                    if (string.IsNullOrEmpty(res))
                                                        response = HttpBuilder.InternalServerError();
                                                    else
                                                        response = HttpResponse.Send(res, "text/xml");
                                                }
                                                else
                                                {
                                                    switch (request.Method)
                                                    {
                                                        case "GET":
                                                            switch (absolutepath)
                                                            {
                                                                case "/networktest/get_2m":
                                                                    response = HttpResponse.Send(new byte[2097152]);
                                                                    break;
                                                                case "/robots.txt":
                                                                    response = HttpResponse.Send("User-agent: *\nDisallow: / "); // Get Away Google.
                                                                    break;
                                                                default:
                                                                    if (absolutepath.ToLower().EndsWith(".php") && !string.IsNullOrEmpty(HTTPServerConfiguration.PHPRedirectUrl))
                                                                        response = HttpBuilder.PermanantRedirect($"{HTTPServerConfiguration.PHPRedirectUrl}{request.Url}");
                                                                    else if (absolutepath.ToLower().EndsWith(".php") && Directory.Exists(HTTPServerConfiguration.PHPStaticFolder) && File.Exists(filePath))
                                                                    {
                                                                        (byte[]?, string[][]) CollectPHP = PHP.ProcessPHPPage(filePath, HTTPServerConfiguration.PHPStaticFolder, HTTPServerConfiguration.PHPVersion, clientip, clientport.ToString(), request);
                                                                        string? encoding = request.GetHeaderValue("Accept-Encoding");
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
                                                                            response = FileSystemRouteHandler.Handle(request, filePath, $"http://{VariousUtils.GetPublicIPAddress(true, true)}{absolutepath[..^1]}", clientip, clientport.ToString());
                                                                    }
                                                                    break;
                                                            }
                                                            break;
                                                        case "POST":
                                                            switch (absolutepath)
                                                            {
                                                                case "/networktest/post_128":
                                                                    response = HttpBuilder.Ok();
                                                                    break;
                                                                case "/!HomeTools/MakeBarSdat/":
                                                                    if (IsIPAllowed(clientip))
                                                                    {
                                                                        var makeres = HomeToolsInterface.MakeBarSdat(request.getDataStream, request.GetContentType());
                                                                        if (makeres != null)
                                                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(makeres.Value.Item1, makeres.Value.Item2);
                                                                        else
                                                                            response = HttpBuilder.InternalServerError();
                                                                    }
                                                                    else
                                                                        response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                    break;
                                                                case "/!HomeTools/UnBar/":
                                                                    if (IsIPAllowed(clientip))
                                                                    {
                                                                        var unbarres = HomeToolsInterface.UnBarAsync(request.getDataStream, request.GetContentType(), HTTPServerConfiguration.HomeToolsHelperStaticFolder).Result;
                                                                        if (unbarres != null)
                                                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(unbarres.Value.Item1, unbarres.Value.Item2);
                                                                        else
                                                                            response = HttpBuilder.InternalServerError();
                                                                    }
                                                                    else
                                                                        response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                    break;
                                                                case "/!HomeTools/CDS/":
                                                                    if (IsIPAllowed(clientip))
                                                                    {
                                                                        var cdsres = HomeToolsInterface.CDS(request.getDataStream, request.GetContentType());
                                                                        if (cdsres != null)
                                                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(cdsres.Value.Item1, cdsres.Value.Item2);
                                                                        else
                                                                            response = HttpBuilder.InternalServerError();
                                                                    }
                                                                    else
                                                                        response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                    break;
                                                                case "/!HomeTools/CDSBruteforce/":
                                                                    if (IsIPAllowed(clientip))
                                                                    {
                                                                        var cdsres = HomeToolsInterface.CDSBruteforceAsync(request.getDataStream, request.GetContentType(), HTTPServerConfiguration.HomeToolsHelperStaticFolder).Result;
                                                                        if (cdsres != null)
                                                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(cdsres.Value.Item1, cdsres.Value.Item2);
                                                                        else
                                                                            response = HttpBuilder.InternalServerError();
                                                                    }
                                                                    else
                                                                        response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                    break;
                                                                case "/!HomeTools/HCDBUnpack/":
                                                                    if (IsIPAllowed(clientip))
                                                                    {
                                                                        var cdsres = HomeToolsInterface.HCDBUnpack(request.getDataStream, request.GetContentType());
                                                                        if (cdsres != null)
                                                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(cdsres.Value.Item1, cdsres.Value.Item2);
                                                                        else
                                                                            response = HttpBuilder.InternalServerError();
                                                                    }
                                                                    else
                                                                        response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                    break;
                                                                case "/!HomeTools/TicketList/":
                                                                    if (IsIPAllowed(clientip))
                                                                    {
                                                                        var ticketlistres = HomeToolsInterface.TicketList(request.getDataStream, request.GetContentType());
                                                                        if (ticketlistres != null)
                                                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(ticketlistres.Value.Item1, ticketlistres.Value.Item2);
                                                                        else
                                                                            response = HttpBuilder.InternalServerError();
                                                                    }
                                                                    else
                                                                        response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                    break;
                                                                case "/!HomeTools/INF/":
                                                                    if (IsIPAllowed(clientip))
                                                                    {
                                                                        var infres = HomeToolsInterface.INF(request.getDataStream, request.GetContentType());
                                                                        if (infres != null)
                                                                            response = FileSystemRouteHandler.Handle_ByteSubmit_Download(infres.Value.Item1, infres.Value.Item2);
                                                                        else
                                                                            response = HttpBuilder.InternalServerError();
                                                                    }
                                                                    else
                                                                        response = HttpBuilder.InternalServerError(); // We are vague on the status code.
                                                                    break;
                                                                case "/!HomeTools/ChannelID/":
                                                                    if (IsIPAllowed(clientip))
                                                                    {
                                                                        string? channelres = HomeToolsInterface.ChannelID(request.getDataStream, request.GetContentType());
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
                                                                        string? sceneres = HomeToolsInterface.SceneID(request.getDataStream, request.GetContentType());
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
                                                                        string? encoding = request.GetHeaderValue("Accept-Encoding");
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
                                                            response = HttpBuilder.NotAllowed();
                                                            break;
                                                        case "DELETE":
                                                            response = HttpBuilder.NotAllowed();
                                                            break;
                                                        case "HEAD":
                                                            response = FileSystemRouteHandler.HandleHEAD(filePath);
                                                            break;
                                                        case "OPTIONS":
                                                            response = HttpBuilder.Ok();
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

        private string Readline(Stream stream)
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

        private void WriteResponse(Stream stream, HttpRequest? request, HttpResponse? response, string filePath)
        {
            try
            {
                if (response != null && request != null)
                {
                    if (response.ContentStream == null)
                        response.ContentAsUTF8 = string.Empty;

                    if (response.ContentStream != null) // Safety.
                    {
                        if (request.Method == "OPTIONS")
                        {
                            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, HEAD");
                            response.Headers.Add("Access-Control-Max-Age", "1728000");
                        }

                        response.Headers.Add("Access-Control-Allow-Origin", "*");

                        response.Headers.Add("Server", VariousUtils.GenerateServerSignature());

                        if (!response.Headers.ContainsKey("Content-Type"))
                            response.Headers.Add("Content-Type", "text/plain");

                        if (!response.Headers.ContainsKey("Content-Length"))
                            response.Headers.Add("Content-Length", response.ContentStream.Length.ToString());

                        if (response.HttpStatusCode == Models.HttpStatusCode.Ok || response.HttpStatusCode == Models.HttpStatusCode.PartialContent)
                        {
                            response.Headers.Add("Date", DateTime.Now.ToString("r"));
                            response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                            if (File.Exists(filePath))
                                response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                        }

                        WriteLineToStream(stream, response.ToHeader());

                        stream.Flush();

                        int buffersize = HTTPServerConfiguration.BufferSize;
                        long totalBytes = response.ContentStream.Length;
                        long bytesLeft = totalBytes;

                        if (totalBytes > 8000000 && buffersize < 500000) // We optimize large file handling.
                            buffersize = 500000;

                        while (bytesLeft > 0)
                        {
                            Span<byte> buffer = new byte[bytesLeft > buffersize ? buffersize : bytesLeft];
                            int n = response.ContentStream.Read(buffer);

                            stream.Write(buffer);

                            bytesLeft -= n;
                        }

                        stream.Flush();

                        if (response.HttpStatusCode == Models.HttpStatusCode.Ok || response.HttpStatusCode == Models.HttpStatusCode.PartialContent
                                || response.HttpStatusCode == Models.HttpStatusCode.MovedPermanently)
                            LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                        else
                        {
                            if (response.HttpStatusCode == Models.HttpStatusCode.NotFound)
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
                if (response != null)
                    response.ContentStream?.Close();
            }
            catch (ObjectDisposedException)
            {
                // ContentStream has been disposed already.
            }

            response?.Dispose();
        }

        private void Handle_LocalFile_Stream(Stream stream, HttpRequest request, string local_path)
        {
            // This method directly communicate with the wire to handle, normally, imposible transfers.
            // If a part of the code sounds weird to you, it's normal... So does curl tests...
            const int rangebuffersize = 32768;

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
                        using (MemoryStream ms = new())
                        {
                            int buffersize = HTTPServerConfiguration.BufferSize;
                            Span<byte> Separator = new byte[] { 0x0D, 0x0A };
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
                                    ms.Flush();
                                    ms.Close();
                                    response = new(false)
                                    {
                                        HttpStatusCode = Models.HttpStatusCode.RangeNotSatisfiable
                                    };
                                    response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                                    response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                                    response.ContentAsUTF8 = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n" +
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
                                    goto shortcut; // Do we really have the choice?
                                }
                                else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                                {
                                    ms.Flush();
                                    ms.Close();
                                    response = new(false)
                                    {
                                        HttpStatusCode = Models.HttpStatusCode.Ok
                                    };
                                    response.Headers.Add("Accept-Ranges", "bytes");
                                    response.Headers.Add("Content-Type", ContentType);
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
                            response = new(true)
                            {
                                HttpStatusCode = Models.HttpStatusCode.PartialContent
                            };
                            response.Headers.Add("Content-Type", "multipart/byteranges; boundary=multiserver_separator");
                            response.Headers.Add("Accept-Ranges", "bytes");
                            response.Headers.Add("Access-Control-Allow-Origin", "*");
                            response.Headers.Add("Server", VariousUtils.GenerateServerSignature());
                            response.Headers.Add("Content-Length", ms.Length.ToString());
                            response.Headers.Add("Date", DateTime.Now.ToString("r"));
                            response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                            response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));

                            WriteLineToStream(stream, response.ToHeader());

                            stream.Flush();

                            long totalBytes = ms.Length;
                            long bytesLeft = totalBytes;

                            if (totalBytes > 8000000 && buffersize < 500000) // We optimize large file handling.
                                buffersize = 500000;

                            while (bytesLeft > 0)
                            {
                                Span<byte> buffer = new byte[bytesLeft > buffersize ? buffersize : bytesLeft];
                                int n = ms.Read(buffer);

                                stream.Write(buffer);

                                bytesLeft -= n;
                            }

                            stream.Flush();

                            ms.Flush();
                            ms.Close();

                            if (response.HttpStatusCode == Models.HttpStatusCode.Ok || response.HttpStatusCode == Models.HttpStatusCode.PartialContent
                                        || response.HttpStatusCode == Models.HttpStatusCode.MovedPermanently)
                                LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                            else
                            {
                                if (response.HttpStatusCode == Models.HttpStatusCode.NotFound)
                                    LoggerAccessor.LogWarn(string.Format("{0} Requested a non-existant file -> {1}", local_path, response.HttpStatusCode));
                                else if (response.HttpStatusCode == Models.HttpStatusCode.NotImplemented || response.HttpStatusCode == Models.HttpStatusCode.RangeNotSatisfiable)
                                    LoggerAccessor.LogWarn(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                                else
                                    LoggerAccessor.LogError(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                            }

                            try
                            {
                                if (response != null)
                                    response.ContentStream?.Close();
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
                        response = new(false)
                        {
                            HttpStatusCode = Models.HttpStatusCode.RangeNotSatisfiable
                        };
                        response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                        response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                        response.ContentAsUTF8 = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n" +
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
                    }
                    else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                    {
                        response = new(false)
                        {
                            HttpStatusCode = Models.HttpStatusCode.Ok
                        };
                        response.Headers.Add("Accept-Ranges", "bytes");
                        string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
                        if (ContentType == "application/octet-stream")
                        {
                            bool matched = false;
                            byte[] VerificationChunck = VariousUtils.ReadSmallFileChunck(local_path, 10);
                            foreach (var entry in HTTPUtils.PathernDictionary)
                            {
                                if (VariousUtils.FindbyteSequence(VerificationChunck, entry.Value))
                                {
                                    matched = true;
                                    response.Headers["Content-Type"] = entry.Key;
                                    break;
                                }
                            }
                            if (!matched)
                                response.Headers["Content-Type"] = ContentType;
                        }
                        else
                            response.Headers["Content-Type"] = ContentType;
                        response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
                    else
                    {
                        response = new(true)
                        {
                            HttpStatusCode = Models.HttpStatusCode.PartialContent
                        };
                        long TotalBytes = endByte - startByte; // Todo : Curl showed that we should load TotalBytes - 1, but VLC and Chrome complains about it...
                        fs.Position = startByte;
                        string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
                        if (ContentType == "application/octet-stream")
                        {
                            bool matched = false;
                            foreach (var entry in HTTPUtils.PathernDictionary)
                            {
                                if (VariousUtils.FindbyteSequence(VariousUtils.ReadSmallFileChunck(local_path, 10), entry.Value))
                                {
                                    matched = true;
                                    response.Headers["Content-Type"] = entry.Key;
                                    break;
                                }
                            }
                            if (!matched)
                                response.Headers["Content-Type"] = ContentType;
                        }
                        else
                            response.Headers["Content-Type"] = ContentType;
                        response.Headers.Add("Accept-Ranges", "bytes");
                        response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", startByte, endByte - 1, filesize));
                        response.Headers.Add("Access-Control-Allow-Origin", "*");
                        response.Headers.Add("Server", VariousUtils.GenerateServerSignature());
                        response.Headers.Add("Content-Length", TotalBytes.ToString());
                        response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                        response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));

                        WriteLineToStream(stream, response.ToHeader());

                        stream.Flush();

                        int buffersize = HTTPServerConfiguration.BufferSize;
                        long bytesLeft = TotalBytes;

                        if (TotalBytes > 8000000 && buffersize < 500000) // We optimize large file handling.
                            buffersize = 500000;

                        while (bytesLeft > 0)
                        {
                            Span<byte> buffer = new byte[bytesLeft > buffersize ? buffersize : bytesLeft];
                            int n = fs.Read(buffer);

                            stream.Write(buffer);

                            bytesLeft -= n;
                        }

                        stream.Flush();

                        if (response.HttpStatusCode == Models.HttpStatusCode.Ok || response.HttpStatusCode == Models.HttpStatusCode.PartialContent
                                        || response.HttpStatusCode == Models.HttpStatusCode.MovedPermanently)
                            LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                        else
                        {
                            if (response.HttpStatusCode == Models.HttpStatusCode.NotFound)
                                LoggerAccessor.LogWarn(string.Format("{0} Requested a non-existant file -> {1}", local_path, response.HttpStatusCode));
                            else if (response.HttpStatusCode == Models.HttpStatusCode.NotImplemented || response.HttpStatusCode == Models.HttpStatusCode.RangeNotSatisfiable)
                                LoggerAccessor.LogWarn(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                            else
                                LoggerAccessor.LogError(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                        }

                        try
                        {
                            if (response != null)
                                response.ContentStream?.Close();
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
                            response.Headers.Add("Access-Control-Allow-Origin", "*");
                            response.Headers.Add("Server", VariousUtils.GenerateServerSignature());
                            response.Headers.Add("Content-Length", response.ContentStream.Length.ToString());
                            response.Headers.Add("Date", DateTime.Now.ToString("r"));
                            response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                            response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));

                            WriteLineToStream(stream, response.ToHeader());

                            stream.Flush();

                            int buffersize = HTTPServerConfiguration.BufferSize;
                            long totalBytes = response.ContentStream.Length;
                            long bytesLeft = totalBytes;

                            if (totalBytes > 8000000 && buffersize < 500000) // We optimize large file handling.
                                buffersize = 500000;

                            while (bytesLeft > 0)
                            {
                                Span<byte> buffer = new byte[bytesLeft > buffersize ? buffersize : bytesLeft];
                                int n = response.ContentStream.Read(buffer);

                                stream.Write(buffer);

                                bytesLeft -= n;
                            }

                            stream.Flush();

                            if (response.HttpStatusCode == Models.HttpStatusCode.Ok || response.HttpStatusCode == Models.HttpStatusCode.PartialContent
                                        || response.HttpStatusCode == Models.HttpStatusCode.MovedPermanently)
                                LoggerAccessor.LogInfo(string.Format("{0} -> {1}", request.Url, response.HttpStatusCode));
                            else
                            {
                                if (response.HttpStatusCode == Models.HttpStatusCode.NotFound)
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
                        LoggerAccessor.LogError($"[HTTP] - WriteResponse - IO-Socket thrown an exception : {ex}");
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[HTTP] - WriteResponse thrown an assertion : {ex}");
                }

                try
                {
                    if (response != null)
                        response.ContentStream?.Close();
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

        private void WriteLineToStream(Stream stream, string text)
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

        private HttpRequest? GetRequest(Stream inputStream, string clientip, string? clientport)
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
                long bytesLeft = Convert.ToInt32(headers["Content-Length"]);

                using (MemoryStream contentStream = new())
                {
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
            }

            return req;
        }
        #endregion
    }
}
