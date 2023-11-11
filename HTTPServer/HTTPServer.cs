using System.Net;
using System.Text;
using CustomLogger;
using HTTPServer.API;

namespace HTTPServer
{
    internal class Processor
    {
        public static bool IsStarted = false;

        private Thread? thread;
        private volatile bool threadActive;

        private HttpListener? listener;
        private string ip;
        private int port;

        public Processor(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public bool IsIPBanned(string ipAddress)
        {
            if (HTTPServerConfiguration.BannedIPs != null && HTTPServerConfiguration.BannedIPs.Contains(ipAddress))
                return true;

            return false;
        }

        public bool IsIPAllowed(string ipAddress)
        {
            if ((HTTPServerConfiguration.AllowedIPs != null && HTTPServerConfiguration.AllowedIPs.Contains(ipAddress)) || ipAddress == "127.0.0.1" || ipAddress.ToLower() == "localhost")
                return true;

            return false;
        }

        public void Start()
        {
            if (thread != null)
            {
                LoggerAccessor.LogError("HTTP Server already active.");
                return;
            }
            thread = new Thread(Listen);
            thread.Start();
            IsStarted = true;
        }

        public void Stop()
        {
            // stop thread and listener
            threadActive = false;
            if (listener != null && listener.IsListening) listener.Stop();

            // wait for thread to finish
            if (thread != null)
            {
                thread.Join();
                thread = null;
            }

            // finish closing listener
            if (listener != null)
            {
                listener.Close();
                listener = null;
            }
            IsStarted = false;
        }

        private void Listen()
        {
            threadActive = true;

            // start listener
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(string.Format("http://{0}:{1}/", ip, port));
                listener.Start();
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError("[HTTP] - ERROR: " + e.Message);
                threadActive = false;
                return;
            }

            // wait for requests
            while (threadActive)
            {
                try
                {
                    var context = listener.GetContextAsync().Result;
                    if (!threadActive) break;
                    Task.Run(() => ProcessContext(context));
                }
                catch (HttpListenerException e)
                {
                    if (e.ErrorCode != 995) LoggerAccessor.LogError("[HTTP] - ERROR: " + e.Message);
                    threadActive = false;
                }
                catch (Exception e)
                {
                    LoggerAccessor.LogError("[HTTP] - ERROR: " + e.Message);
                    threadActive = false;
                }
            }
        }

        private void ProcessContext(HttpListenerContext ctx)
        {
            HttpStatusCode statusCode = HttpStatusCode.Forbidden;
            string clientip = string.Empty;
            string url = string.Empty;
            string absolutepath = string.Empty;
            string? host = string.Empty;

            try
            {
                clientip = ctx.Request.RemoteEndPoint.Address.ToString();

                if (string.IsNullOrEmpty(ctx.Request.UserAgent) || string.IsNullOrEmpty(ctx.Request.Headers["Host"]) || IsIPBanned(clientip))
                    LoggerAccessor.LogError($"[SECURITY] - Client - {clientip} Requested the HTTP server while being banned!");
                else
                {
                    if (ctx.Request.Url != null && ctx.Request.Url.AbsolutePath != null && ctx.Request.Url.LocalPath != null && !string.IsNullOrEmpty(ctx.Request.Headers["Host"]))
                    {
                        LoggerAccessor.LogInfo($"[HTTP] - Client - {clientip} Requested the HTTP Server with URL : {ctx.Request.Url}");

                        // get filename path
                        absolutepath = ctx.Request.Url.AbsolutePath;
                        url = ctx.Request.Url.LocalPath;
                        host = ctx.Request.Headers["Host"];
                        statusCode = HttpStatusCode.Continue;
                    }
                    else
                        LoggerAccessor.LogInfo($"[HTTP] - Client - {clientip} Requested the HTTP Server with invalid parameters!");
                }
            }
            catch (Exception)
            {

            }

            if (statusCode == HttpStatusCode.Continue)
            {

                try
                {
                    // Split the URL into segments
                    string[] segments = url.Trim('/').Split('/');

                    // Combine the folder segments into a directory path
                    string directoryPath = Path.Combine(HTTPServerConfiguration.HTTPStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                    // Process the request based on the HTTP method
                    string filePath = Path.Combine(HTTPServerConfiguration.HTTPStaticFolder, url.Substring(1));

                    if (ctx.Request.HttpMethod == "OPTIONS")
                    {
                        ctx.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                        ctx.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, HEAD");
                        ctx.Response.AddHeader("Access-Control-Max-Age", "1728000");
                    }

                    ctx.Response.AppendHeader("Access-Control-Allow-Origin", "*");

                    if ((absolutepath == "/" || absolutepath == "\\") && ctx.Request.HttpMethod == "GET")
                    {
                        foreach (string indexFile in CryptoSporidium.HTTPUtils.DefaultDocuments)
                        {
                            if (ctx.Response.OutputStream.CanWrite && File.Exists(Path.Combine(HTTPServerConfiguration.HTTPStaticFolder, indexFile)))
                            {
                                string? encoding = ctx.Request.Headers["Content-Encoding"];

                                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                {
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
                                            byte[]? CompresedFileBytes = CryptoSporidium.HTTPUtils.Compress(buffer);
                                            if (CompresedFileBytes != null)
                                            {
                                                statusCode = HttpStatusCode.OK;

                                                ctx.Response.AddHeader("Content-Encoding", "gzip");
                                                ctx.Response.ContentType = "text/html";
                                                ctx.Response.ContentLength64 = CompresedFileBytes.Length;
                                                ctx.Response.OutputStream.Write(CompresedFileBytes, 0, CompresedFileBytes.Length);
                                                ctx.Response.OutputStream.Flush();
                                            }
                                            else
                                                statusCode = HttpStatusCode.InternalServerError;
                                        }
                                        else
                                            statusCode = HttpStatusCode.InternalServerError;

                                        stream.Flush();
                                    }
                                }
                                else
                                {
                                    statusCode = HttpStatusCode.OK;

                                    using (FileStream stream = new(indexFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        ctx.Response.SendChunked = true;

                                        // get mime type
                                        ctx.Response.ContentType = "text/html";
                                        ctx.Response.ContentLength64 = stream.Length;

                                        // copy file stream to response
                                        stream.CopyTo(ctx.Response.OutputStream);
                                        stream.Flush();
                                        ctx.Response.OutputStream.Flush();
                                    }
                                }
                                break;
                            }
                            else
                                statusCode = HttpStatusCode.NotFound;
                        }
                    }
                    else
                    {
                        switch (host)
                        {
                            case "sonyhome.thqsandbox.com":
                                switch (ctx.Request.HttpMethod)
                                {
                                    case "POST":
                                        switch (absolutepath)
                                        {
                                            case "/index.php":
                                                LoggerAccessor.LogInfo($"[HTTP] - {clientip} Requested a UFC Connection");

                                                if (ctx.Response.OutputStream.CanWrite && ctx.Request.ContentType != null)
                                                {
                                                    HTTPUtils? utils = new();
                                                    string? UFCResult = utils.ProcessUFCUserData(ctx.Request.InputStream, CryptoSporidium.HTTPUtils.ExtractBoundary(ctx.Request.ContentType));
                                                    if (UFCResult != null)
                                                    {
                                                        using (MemoryStream memoryStream = new(Encoding.UTF8.GetBytes(UFCResult)))
                                                        {
                                                            ctx.Response.SendChunked = true;

                                                            // get mime type
                                                            ctx.Response.ContentType = "text/xml";
                                                            ctx.Response.ContentLength64 = memoryStream.Length;

                                                            // copy file stream to response
                                                            memoryStream.CopyTo(ctx.Response.OutputStream);
                                                            memoryStream.Flush();
                                                            ctx.Response.OutputStream.Flush();
                                                        }

                                                        statusCode = HttpStatusCode.OK;
                                                    }
                                                    else
                                                        statusCode = HttpStatusCode.InternalServerError;

                                                    utils = null;
                                                }
                                                else
                                                    statusCode = HttpStatusCode.InternalServerError;
                                                break;
                                            default:
                                                statusCode = HttpStatusCode.Forbidden;
                                                break;
                                        }
                                        break;
                                    default:
                                        statusCode = HttpStatusCode.Forbidden;
                                        break;
                                }
                                break;
                            default:
                                switch (ctx.Request.HttpMethod)
                                {
                                    case "GET":
                                        if ((host == "stats.outso-srv1.com" || host == "www.outso-srv1.com") && absolutepath.Contains("/ohs") && absolutepath.EndsWith("/"))
                                        {
                                            LoggerAccessor.LogInfo($"[HTTP] - {clientip} Requested a OHS method : {absolutepath}");
                                            string? res = null;
                                            int version = 0;
                                            if (absolutepath.Contains("/Insomniac/4BarrelsOfFury"))
                                                version = 2;
                                            CryptoSporidium.OHS.OHSClass ohs = new(ctx.Request.HttpMethod, absolutepath, version);
                                            using (MemoryStream postdata = new())
                                            {
                                                ctx.Request.InputStream.CopyTo(postdata);

                                                postdata.Position = 0;
                                                // Find the number of bytes in the stream
                                                int contentLength = (int)postdata.Length;
                                                // Create a byte array
                                                byte[] buffer = new byte[contentLength];
                                                // Read the contents of the memory stream into the byte array
                                                postdata.Read(buffer, 0, contentLength);
                                                res = ohs.ProcessRequest(buffer, ctx.Request.ContentType, filePath);
                                                postdata.Flush();
                                            }
                                            ohs.Dispose();
                                            if (string.IsNullOrEmpty(res))
                                                statusCode = HttpStatusCode.InternalServerError;
                                            else
                                            {
                                                statusCode = HttpStatusCode.OK;
                                                ctx.Response.ContentType = "application/xml;charset=UTF-8";
                                                // Construct a response.
                                                byte[] buffer = Encoding.UTF8.GetBytes($"<ohs>{res}</ohs>");
                                                if (ctx.Response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        ctx.Response.ContentLength64 = buffer.Length;
                                                        ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                                        ctx.Response.OutputStream.Flush();
                                                    }
                                                    catch (Exception)
                                                    {
                                                        // Not Important;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            switch (absolutepath)
                                            {
                                                case "/networktest/get_2m":
                                                    statusCode = HttpStatusCode.OK;

                                                    byte[] NulledBytes = new byte[2097152];
                                                    if (ctx.Response.OutputStream.CanWrite)
                                                    {
                                                        try
                                                        {
                                                            ctx.Response.ContentLength64 = NulledBytes.Length;
                                                            ctx.Response.OutputStream.Write(NulledBytes, 0, NulledBytes.Length);
                                                            ctx.Response.OutputStream.Flush();
                                                        }
                                                        catch (Exception)
                                                        {
                                                            // Not Important;
                                                        }
                                                    }
                                                    break;
                                                default:
                                                    if (File.Exists(filePath) && filePath.ToLower().EndsWith(".php") && Directory.Exists(HTTPServerConfiguration.PHPStaticFolder))
                                                    {
                                                        if (ctx.Response.OutputStream.CanWrite && ctx.Request.Url != null && ctx.Request.Url.Scheme != null)
                                                        {
                                                            PHPClass php = new();
                                                            byte[]? PHPBytes = php.ProcessPHPPage($"{HTTPServerConfiguration.HTTPStaticFolder}{absolutepath}", HTTPServerConfiguration.PHPStaticFolder, HTTPServerConfiguration.PHPVersion, $"{ctx.Request.Url.Scheme}://{ctx.Request.Url.Authority}{ctx.Request.RawUrl}", ctx.Request, ctx.Response);
                                                            php.Dispose();
                                                            if (PHPBytes != null)
                                                            {
                                                                using (MemoryStream memoryStream = new(PHPBytes))
                                                                {
                                                                    ctx.Response.SendChunked = true;

                                                                    // get mime type
                                                                    ctx.Response.ContentType = CryptoSporidium.HTTPUtils.mimeTypes[Path.GetExtension(filePath)];
                                                                    ctx.Response.ContentLength64 = memoryStream.Length;

                                                                    // copy file stream to response
                                                                    memoryStream.CopyTo(ctx.Response.OutputStream);
                                                                    memoryStream.Flush();
                                                                    ctx.Response.OutputStream.Flush();
                                                                }

                                                                statusCode = HttpStatusCode.OK;
                                                            }
                                                            else
                                                                statusCode = HttpStatusCode.InternalServerError;
                                                        }
                                                        else
                                                            statusCode = HttpStatusCode.InternalServerError;
                                                    }
                                                    else if (filePath.EndsWith("/"))
                                                    {
                                                        string? encoding = ctx.Request.Headers["Content-Encoding"];

                                                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                                        {
                                                            byte[]? CompresedFileBytes = CryptoSporidium.HTTPUtils.Compress(Encoding.UTF8.GetBytes(CryptoSporidium.FileStructureToJson.GetFileStructureAsJson(filePath.Substring(0, filePath.Length - 1))));
                                                            if (CompresedFileBytes != null)
                                                            {
                                                                statusCode = HttpStatusCode.OK;

                                                                ctx.Response.AddHeader("Content-Encoding", "gzip");
                                                                ctx.Response.ContentType = "application/json";
                                                                if (ctx.Response.OutputStream.CanWrite)
                                                                {
                                                                    try
                                                                    {
                                                                        ctx.Response.ContentLength64 = CompresedFileBytes.Length;
                                                                        ctx.Response.OutputStream.Write(CompresedFileBytes, 0, CompresedFileBytes.Length);
                                                                        ctx.Response.OutputStream.Flush();
                                                                    }
                                                                    catch (Exception)
                                                                    {
                                                                        // Not Important;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                                statusCode = HttpStatusCode.InternalServerError;
                                                        }
                                                        else
                                                        {
                                                            statusCode = HttpStatusCode.OK;

                                                            byte[] FileBytes = Encoding.UTF8.GetBytes(CryptoSporidium.FileStructureToJson.GetFileStructureAsJson(filePath.Substring(0, filePath.Length - 1)));
                                                            ctx.Response.ContentType = "application/json";
                                                            if (ctx.Response.OutputStream.CanWrite)
                                                            {
                                                                try
                                                                {
                                                                    ctx.Response.ContentLength64 = FileBytes.Length;
                                                                    ctx.Response.OutputStream.Write(FileBytes, 0, FileBytes.Length);
                                                                    ctx.Response.OutputStream.Flush();
                                                                }
                                                                catch (Exception)
                                                                {
                                                                    // Not Important;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else if (File.Exists(filePath))
                                                    {
                                                        LoggerAccessor.LogInfo($"[HTTP] - {clientip} Requested a file : " + filePath);

                                                        if (ctx.Response.OutputStream.CanWrite)
                                                        {
                                                            string mimetype = CryptoSporidium.HTTPUtils.mimeTypes[Path.GetExtension(filePath)];

                                                            string? encoding = ctx.Request.Headers["Content-Encoding"];

                                                            if (!Path.HasExtension(filePath))
                                                            {
                                                                CryptoSporidium.MiscUtils? utils = new();
                                                                if (utils.FindbyteSequence(File.ReadAllBytes(filePath), "<!DOCTYPE html>"u8.ToArray()))
                                                                    mimetype = "text/html";
                                                                utils = null;
                                                            }

                                                            if (ctx.Request.Headers.AllKeys.Contains("Range") && ctx.Request.Headers.AllKeys.Count(x => x == "Range") == 1) // We not support multiple ranges.
                                                            {
                                                                long startByte = -1;
                                                                long endByte = -1;
                                                                string[]? RangeCollection = ctx.Request.Headers.GetValues("Range");

                                                                using (FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                                                {
                                                                    if (RangeCollection != null && !string.IsNullOrEmpty(RangeCollection[0]))
                                                                    {
                                                                        string rangeHeader = RangeCollection[0].Replace("bytes=", "");
                                                                        string[] range = rangeHeader.Split('-');
                                                                        startByte = long.Parse(range[0]);
                                                                        if (range[1].Trim().Length > 0) long.TryParse(range[1], out endByte);
                                                                        if (endByte == -1) endByte = stream.Length;
                                                                    }
                                                                    else
                                                                    {
                                                                        startByte = 0;
                                                                        endByte = stream.Length;
                                                                    }
                                                                    if (endByte > stream.Length || startByte < 0)
                                                                        statusCode = HttpStatusCode.RequestedRangeNotSatisfiable;
                                                                    else
                                                                    {
                                                                        statusCode = HttpStatusCode.PartialContent;
                                                                        ctx.Response.StatusDescription = "Partial Content";
                                                                        ctx.Response.ContentType = mimetype;
                                                                        ctx.Response.AddHeader("Accept-Ranges", "bytes");
                                                                        ctx.Response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", startByte, endByte - 1, stream.Length));
                                                                        ctx.Response.ContentLength64 = stream.Length;
                                                                        ctx.Response.KeepAlive = true;
                                                                        try
                                                                        {
                                                                            // copy file stream to response
                                                                            stream.CopyTo(ctx.Response.OutputStream);
                                                                        }
                                                                        catch (HttpListenerException e) when (e.ErrorCode == 995)
                                                                        {
                                                                            statusCode = HttpStatusCode.InternalServerError;
                                                                        }

                                                                        ctx.Response.OutputStream.Flush();
                                                                    }

                                                                    stream.Flush();
                                                                }
                                                            }
                                                            else if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                                            {
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
                                                                        byte[]? CompresedFileBytes = CryptoSporidium.HTTPUtils.Compress(buffer);
                                                                        if (CompresedFileBytes != null)
                                                                        {
                                                                            statusCode = HttpStatusCode.OK;

                                                                            ctx.Response.AddHeader("Content-Encoding", "gzip");
                                                                            ctx.Response.ContentType = mimetype;
                                                                            ctx.Response.ContentLength64 = CompresedFileBytes.Length;
                                                                            ctx.Response.OutputStream.Write(CompresedFileBytes, 0, CompresedFileBytes.Length);
                                                                            ctx.Response.OutputStream.Flush();
                                                                        }
                                                                        else
                                                                            statusCode = HttpStatusCode.InternalServerError;
                                                                    }
                                                                    else
                                                                        statusCode = HttpStatusCode.InternalServerError;

                                                                    stream.Flush();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                statusCode = HttpStatusCode.OK;

                                                                using (FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                                                                {
                                                                    ctx.Response.SendChunked = true;

                                                                    // get mime type
                                                                    ctx.Response.ContentType = mimetype;
                                                                    ctx.Response.ContentLength64 = stream.Length;

                                                                    // copy file stream to response
                                                                    stream.CopyTo(ctx.Response.OutputStream);
                                                                    stream.Flush();
                                                                    ctx.Response.OutputStream.Flush();
                                                                }
                                                            }
                                                        }
                                                        else
                                                            statusCode = HttpStatusCode.InternalServerError;
                                                    }
                                                    else
                                                    {
                                                        LoggerAccessor.LogWarn($"[HTTP] - {clientip} Requested a non-existant file: " + filePath);
                                                        statusCode = HttpStatusCode.NotFound;
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                    case "POST":
                                        if ((host == "stats.outso-srv1.com" || host == "www.outso-srv1.com") && absolutepath.Contains("/ohs") && absolutepath.EndsWith("/"))
                                        {
                                            LoggerAccessor.LogInfo($"[HTTP] - {clientip} Requested a OHS method : {absolutepath}");
                                            string? res = null;
                                            int version = 0;
                                            if (absolutepath.Contains("/Insomniac/4BarrelsOfFury"))
                                                version = 2;
                                            CryptoSporidium.OHS.OHSClass ohs = new(ctx.Request.HttpMethod, absolutepath, version);
                                            using (MemoryStream postdata = new())
                                            {
                                                ctx.Request.InputStream.CopyTo(postdata);

                                                postdata.Position = 0;
                                                // Find the number of bytes in the stream
                                                int contentLength = (int)postdata.Length;
                                                // Create a byte array
                                                byte[] buffer = new byte[contentLength];
                                                // Read the contents of the memory stream into the byte array
                                                postdata.Read(buffer, 0, contentLength);
                                                res = ohs.ProcessRequest(buffer, ctx.Request.ContentType, filePath);
                                                postdata.Flush();
                                            }
                                            ohs.Dispose();
                                            if (string.IsNullOrEmpty(res))
                                                statusCode = HttpStatusCode.InternalServerError;
                                            else
                                            {
                                                statusCode = HttpStatusCode.OK;
                                                ctx.Response.ContentType = "application/xml;charset=UTF-8";
                                                // Construct a response.
                                                byte[] buffer = Encoding.UTF8.GetBytes($"<ohs>{res}</ohs>");

                                                if (ctx.Response.OutputStream.CanWrite)
                                                {
                                                    try
                                                    {
                                                        ctx.Response.ContentLength64 = buffer.Length;
                                                        ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                                        ctx.Response.OutputStream.Flush();
                                                    }
                                                    catch (Exception)
                                                    {
                                                        // Not Important;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            switch (absolutepath)
                                            {
                                                case "/networktest/post_128":
                                                    statusCode = HttpStatusCode.OK;
                                                    break;
                                                case "/!HomeTools/MakeBarSdat/":
                                                    if (IsIPAllowed(clientip) && HomeTools.MakeBarSdat(ctx.Request.InputStream, ctx.Request.ContentType, ctx.Response))
                                                        statusCode = HttpStatusCode.OK;
                                                    else
                                                        statusCode = HttpStatusCode.InternalServerError;
                                                    break;
                                                case "/!HomeTools/UnBar/":
                                                    if (IsIPAllowed(clientip) && HomeTools.UnBar(ctx.Request.InputStream, ctx.Request.ContentType, ctx.Response).Result)
                                                        statusCode = HttpStatusCode.OK;
                                                    else
                                                        statusCode = HttpStatusCode.InternalServerError;
                                                    break;
                                                case "/!HomeTools/CDS/":
                                                    if (IsIPAllowed(clientip) && HomeTools.CDS(ctx.Request.InputStream, ctx.Request.ContentType, ctx.Response))
                                                        statusCode = HttpStatusCode.OK;
                                                    else
                                                        statusCode = HttpStatusCode.InternalServerError;
                                                    break;
                                                case "/!HomeTools/INF/":
                                                    if (IsIPAllowed(clientip) && HomeTools.INF(ctx.Request.InputStream, ctx.Request.ContentType, ctx.Response))
                                                        statusCode = HttpStatusCode.OK;
                                                    else
                                                        statusCode = HttpStatusCode.InternalServerError;
                                                    break;
                                                case "/!HomeTools/ChannelID/":
                                                    if (IsIPAllowed(clientip))
                                                    {
                                                        string? channelres = HomeTools.ChannelID(ctx.Request.InputStream, ctx.Request.ContentType);
                                                        if (string.IsNullOrEmpty(channelres))
                                                            statusCode = HttpStatusCode.InternalServerError;
                                                        else
                                                        {
                                                            statusCode = HttpStatusCode.OK;
                                                            // Construct a response.
                                                            byte[] buffer = Encoding.UTF8.GetBytes(channelres);

                                                            if (ctx.Response.OutputStream.CanWrite)
                                                            {
                                                                try
                                                                {
                                                                    ctx.Response.ContentLength64 = buffer.Length;
                                                                    ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                                                    ctx.Response.OutputStream.Flush();
                                                                }
                                                                catch (Exception)
                                                                {
                                                                    // Not Important;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                        statusCode = HttpStatusCode.InternalServerError;
                                                    break;
                                                case "/!HomeTools/SceneID/":
                                                    if (IsIPAllowed(clientip))
                                                    {
                                                        string? sceneres = HomeTools.SceneID(ctx.Request.InputStream, ctx.Request.ContentType);
                                                        if (string.IsNullOrEmpty(sceneres))
                                                            statusCode = HttpStatusCode.InternalServerError;
                                                        else
                                                        {
                                                            statusCode = HttpStatusCode.OK;
                                                            // Construct a response.
                                                            byte[] buffer = Encoding.UTF8.GetBytes(sceneres);

                                                            if (ctx.Response.OutputStream.CanWrite)
                                                            {
                                                                try
                                                                {
                                                                    ctx.Response.ContentLength64 = buffer.Length;
                                                                    ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                                                    ctx.Response.OutputStream.Flush();
                                                                }
                                                                catch (Exception)
                                                                {
                                                                    // Not Important;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                        statusCode = HttpStatusCode.InternalServerError;
                                                    break;
                                                default:
                                                    // send file
                                                    if (File.Exists(filePath) && filePath.ToLower().EndsWith(".php") && Directory.Exists(HTTPServerConfiguration.PHPStaticFolder))
                                                    {
                                                        if (ctx.Response.OutputStream.CanWrite && ctx.Request.Url != null && ctx.Request.Url.Scheme != null)
                                                        {
                                                            PHPClass php = new();
                                                            byte[]? PHPBytes = php.ProcessPHPPage($"{HTTPServerConfiguration.HTTPStaticFolder}{absolutepath}", HTTPServerConfiguration.PHPStaticFolder, HTTPServerConfiguration.PHPVersion, $"{ctx.Request.Url.Scheme}://{ctx.Request.Url.Authority}{ctx.Request.RawUrl}", ctx.Request, ctx.Response);
                                                            php.Dispose();
                                                            if (PHPBytes != null)
                                                            {
                                                                using (MemoryStream memoryStream = new(PHPBytes))
                                                                {
                                                                    ctx.Response.SendChunked = true;

                                                                    // get mime type
                                                                    ctx.Response.ContentType = CryptoSporidium.HTTPUtils.mimeTypes[Path.GetExtension(filePath)];
                                                                    ctx.Response.ContentLength64 = memoryStream.Length;

                                                                    // copy file stream to response
                                                                    memoryStream.CopyTo(ctx.Response.OutputStream);
                                                                    memoryStream.Flush();
                                                                    ctx.Response.OutputStream.Flush();
                                                                }

                                                                statusCode = HttpStatusCode.OK;
                                                            }
                                                            else
                                                                statusCode = HttpStatusCode.InternalServerError;
                                                        }
                                                        else
                                                            statusCode = HttpStatusCode.InternalServerError;
                                                    }
                                                    else
                                                        statusCode = HttpStatusCode.Forbidden;
                                                    break;
                                            }
                                        }
                                        break;
                                    case "PUT":
                                        statusCode = HttpStatusCode.Forbidden;
                                        break;
                                    case "DELETE":
                                        statusCode = HttpStatusCode.Forbidden;
                                        break;
                                    case "HEAD":
                                        if (File.Exists(filePath))
                                        {
                                            FileInfo? fileInfo = new(filePath);

                                            if (fileInfo.Exists)
                                            {
                                                long fileSizeInBytes = fileInfo.Length;
                                                // get mime type
                                                ctx.Response.ContentType = CryptoSporidium.HTTPUtils.mimeTypes[Path.GetExtension(filePath)];
                                                ctx.Response.ContentLength64 = fileSizeInBytes;

                                                statusCode = HttpStatusCode.OK;
                                            }
                                            else
                                            {
                                                LoggerAccessor.LogWarn($"[HTTP] - {clientip} Requested a non-existant file: " + filePath);
                                                statusCode = HttpStatusCode.NotFound;
                                            }

                                            fileInfo = null;
                                        }
                                        else
                                        {
                                            LoggerAccessor.LogWarn($"[HTTP] - {clientip} Requested a non-existant file: " + filePath);
                                            statusCode = HttpStatusCode.NotFound;
                                        }
                                        break;
                                    default:
                                        statusCode = HttpStatusCode.Forbidden;
                                        break;
                                }
                                break;
                        }
                    }
                }
                catch (HttpListenerException e) when (e.ErrorCode == 64)
                {
                    // Unfortunately, some client side implementation of HTTP (like RPCS3) freeze the interface at regular interval.
                    // This will cause server to throw error 64 (network interface not openned anymore)
                    // In that case, we send internalservererror so client try again.

                    statusCode = HttpStatusCode.InternalServerError;
                }
                catch (Exception e)
                {
                    LoggerAccessor.LogError("[HTTP] - REQUEST ERROR: " + e.Message);
                    statusCode = HttpStatusCode.InternalServerError;
                }
            }

            // finish
            ctx.Response.StatusCode = (int)statusCode;
            if (statusCode == HttpStatusCode.OK)
            {
                ctx.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                ctx.Response.AddHeader("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
            }

            try
            {
                ctx.Response.OutputStream.Close();
            }
            catch (ObjectDisposedException)
            {
                // outputstream has been disposed already.
            }
            ctx.Response.Close();
        }
    }
}
