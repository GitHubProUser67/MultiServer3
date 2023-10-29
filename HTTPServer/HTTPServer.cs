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
        private string path;
        private string phppath;
        private string ip;
        private string phpver;
        private int port;

        public Processor(string path, string phppath, string ip, string phpver, int port)
        {
            this.path = path;
            this.phppath = phppath;
            this.ip = ip;
            this.phpver = phpver;
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

            try
            {
                clientip = ctx.Request.RemoteEndPoint.Address.ToString();

                if (string.IsNullOrEmpty(ctx.Request.UserAgent) || string.IsNullOrEmpty(ctx.Request.Headers["Host"]) || IsIPBanned(clientip))
                    LoggerAccessor.LogError($"[SECURITY] - Client - {clientip} Requested the HTTP server while being banned!");
                else
                {
                    if (ctx.Request.Url != null && ctx.Request.Url.AbsolutePath != null && ctx.Request.Url.LocalPath != null)
                    {
                        // get filename path
                        absolutepath = ctx.Request.Url.AbsolutePath;
                        url = ctx.Request.Url.LocalPath;
                        statusCode = HttpStatusCode.Continue;
                    }
                }
            }
            catch (Exception)
            {

            }

            if (statusCode == HttpStatusCode.Continue)
            {
                // Split the URL into segments
                string[] segments = url.Trim('/').Split('/');

                // Combine the folder segments into a directory path
                string directoryPath = Path.Combine(path, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                // Process the request based on the HTTP method
                string filePath = Path.Combine(path, url.Substring(1));

                if (ctx.Request.HttpMethod == "OPTIONS")
                {
                    ctx.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                    ctx.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, HEAD");
                    ctx.Response.AddHeader("Access-Control-Max-Age", "1728000");
                }

                switch (ctx.Request.Headers["Host"])
                {
                    case "sonyhome.thqsandbox.com":
                        switch (ctx.Request.HttpMethod)
                        {
                            case "POST":
                                switch (absolutepath)
                                {
                                    case "/index.php":
                                        LoggerAccessor.LogInfo($"[HTTP] - {clientip} Requested a UFC Connection");

                                        try
                                        {
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
                                            LoggerAccessor.LogError("[HTTP] - UFC POST ERROR: " + e.Message);
                                            statusCode = HttpStatusCode.InternalServerError;
                                        }
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
                                // send file
                                if (File.Exists(filePath) && filePath.ToLower().EndsWith(".php") && Directory.Exists(phppath))
                                {
                                    try
                                    {
                                        if (ctx.Response.OutputStream.CanWrite && ctx.Request.Url != null && ctx.Request.Url.Scheme != null)
                                        {
                                            PHPClass php = new();
                                            byte[]? PHPBytes = php.ProcessPHPPage($"{path}{absolutepath}", phppath, phpver, $"{ctx.Request.Url.Scheme}://{ctx.Request.Url.Authority}{ctx.Request.RawUrl}", ctx.Request, ctx.Response);
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
                                    catch (HttpListenerException e) when (e.ErrorCode == 64)
                                    {
                                        // Unfortunately, some client side implementation of HTTP (like RPCS3) freeze the interface at regular interval.
                                        // This will cause server to throw error 64 (network interface not openned anymore)
                                        // In that case, we send internalservererror so client try again.

                                        statusCode = HttpStatusCode.InternalServerError;
                                    }
                                    catch (Exception e)
                                    {
                                        LoggerAccessor.LogError("[HTTP] - PHP GET ERROR: " + e.Message);
                                        statusCode = HttpStatusCode.InternalServerError;
                                    }
                                }
                                else if (File.Exists(filePath))
                                {
                                    LoggerAccessor.LogInfo($"[HTTP] - {clientip} Requested a file : " + filePath);

                                    try
                                    {
                                        if (ctx.Response.OutputStream.CanWrite)
                                        {
                                            string mimetype = CryptoSporidium.HTTPUtils.mimeTypes[Path.GetExtension(filePath)];

                                            if (!Path.HasExtension(filePath))
                                            {
                                                CryptoSporidium.Utils? utils = new();
                                                if (utils.FindbyteSequence(File.ReadAllBytes(filePath), new byte[] { 0x3c, 0x21, 0x44, 0x4f, 0x43, 0x54, 0x59, 0x50, 0x45, 0x20, 0x68, 0x74, 0x6d, 0x6c, 0x3e }))
                                                    mimetype = "text/html";
                                                utils = null;
                                            }

                                            if ((mimetype.Contains("video/") || mimetype.Contains("audio/")) && ctx.Request.Headers.AllKeys.Contains("Range"))
                                            {
                                                byte[] buffer = File.ReadAllBytes(filePath);
                                                int startByte = -1;
                                                int endByte = -1;
                                                if (ctx.Request.Headers.GetValues("Range") != null)
                                                {
                                                    string rangeHeader = ctx.Request.Headers.GetValues("Range")[0].Replace("bytes=", "");
                                                    string[] range = rangeHeader.Split('-');
                                                    startByte = int.Parse(range[0]);
                                                    if (range[1].Trim().Length > 0) int.TryParse(range[1], out endByte);
                                                    if (endByte == -1) endByte = buffer.Length;
                                                }
                                                else
                                                {
                                                    startByte = 0;
                                                    endByte = buffer.Length;
                                                }
                                                if (endByte > buffer.Length || startByte < 0)
                                                    statusCode = HttpStatusCode.RequestedRangeNotSatisfiable;
                                                else
                                                {
                                                    statusCode = HttpStatusCode.PartialContent;
                                                    ctx.Response.StatusDescription = "Partial Content";
                                                    ctx.Response.ContentType = mimetype;
                                                    ctx.Response.AddHeader("Accept-Ranges", "bytes");
                                                    ctx.Response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", startByte, endByte - 1, buffer.Length));
                                                    ctx.Response.ContentLength64 = buffer.Length;
                                                    ctx.Response.KeepAlive = true;
                                                    try
                                                    {
                                                        ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                                    }
                                                    catch (HttpListenerException e) when (e.ErrorCode == 995)
                                                    {
                                                        statusCode = HttpStatusCode.InternalServerError;
                                                    }

                                                    ctx.Response.OutputStream.Flush();
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
                                    catch (HttpListenerException e) when (e.ErrorCode == 64)
                                    {
                                        // Unfortunately, some client side implementation of HTTP (like RPCS3) freeze the interface at regular interval.
                                        // This will cause server to throw error 64 (network interface not openned anymore)
                                        // In that case, we send internalservererror so client try again.

                                        statusCode = HttpStatusCode.InternalServerError;
                                    }
                                    catch (Exception e)
                                    {
                                        LoggerAccessor.LogError("[HTTP] - GET ERROR: " + e.Message);
                                        statusCode = HttpStatusCode.InternalServerError;
                                    }
                                }
                                else
                                {
                                    LoggerAccessor.LogWarn($"[HTTP] - {clientip} Requested a non-existant file: " + filePath);
                                    statusCode = HttpStatusCode.NotFound;
                                }
                                break;
                            case "POST":
                                switch (absolutepath)
                                {
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
                                        if (File.Exists(filePath) && filePath.ToLower().EndsWith(".php") && Directory.Exists(phppath))
                                        {
                                            try
                                            {
                                                if (ctx.Response.OutputStream.CanWrite && ctx.Request.Url != null && ctx.Request.Url.Scheme != null)
                                                {
                                                    PHPClass php = new();
                                                    byte[]? PHPBytes = php.ProcessPHPPage($"{path}{absolutepath}", phppath, phpver, $"{ctx.Request.Url.Scheme}://{ctx.Request.Url.Authority}{ctx.Request.RawUrl}", ctx.Request, ctx.Response);
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
                                            catch (HttpListenerException e) when (e.ErrorCode == 64)
                                            {
                                                // Unfortunately, some client side implementation of HTTP (like RPCS3) freeze the interface at regular interval.
                                                // This will cause server to throw error 64 (network interface not openned anymore)
                                                // In that case, we send internalservererror so client try again.

                                                statusCode = HttpStatusCode.InternalServerError;
                                            }
                                            catch (Exception e)
                                            {
                                                LoggerAccessor.LogError("[HTTP] - PHP POST ERROR: " + e.Message);
                                                statusCode = HttpStatusCode.InternalServerError;
                                            }
                                        }
                                        else
                                            statusCode = HttpStatusCode.Forbidden;
                                        break;
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

            // finish
            ctx.Response.StatusCode = (int)statusCode;
            if (statusCode == HttpStatusCode.OK)
            {
                ctx.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                ctx.Response.AddHeader("Last-Modified", File.GetLastWriteTime(absolutepath).ToString("r"));
                ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
            }

            ctx.Response.OutputStream.Close();
            ctx.Response.Close();
        }
    }
}
