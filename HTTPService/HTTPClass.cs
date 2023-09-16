using MultiServer.HTTPService.Addons.PlayStationHome.HELLFIREGAMES;
using MultiServer.HTTPService.Addons.PlayStationHome.NDREAMS;
using MultiServer.HTTPService.Addons.PlayStationHome.OHS;
using MultiServer.HTTPService.Addons.PlayStationHome.POTTERMORE;
using MultiServer.HTTPService.Addons.PlayStationHome.UFC;
using MultiServer.HTTPService.Addons.PlayStationHome.VEEMEE;
using System.Net;
using System.Text;

namespace MultiServer.HTTPService
{
    public class HTTPClass
    {
        private static volatile bool _keepGoing = true;

        private static bool stopserver = false;

        public static bool httpstarted = false;

        // Create and start the HttpListener
        private static HttpListener listener = new();

        private static Task? _mainLoop;

        public static Task HTTPstart(int port)
        {
            httpstarted = true;

            HTTPPrivateKey.setup();

            CryptoSporidium.AFSBLOWFISH.InitiateINFCryptoContext();

            Addons.PlayStationHome.PrepareFolder.Prepare();

            stopserver = false;
            _keepGoing = true;
            if (_mainLoop != null && !_mainLoop.IsCompleted) return Task.CompletedTask; //Already started
            _mainLoop = loopserver(port);

            return Task.CompletedTask;
        }

        private async static Task loopserver(int port)
        {
            listener.Prefixes.Add($"http://*:{port}/");

            listener.Prefixes.Add("http://*:10010/");

            ServerConfiguration.LogInfo($"HTTP Server started - Listening for requests...");

            listener.Start();

            while (_keepGoing)
            {
                try
                {
                    //GetContextAsync() returns when a new request come in
                    var context = await listener.GetContextAsync();

                    lock (listener)
                    {
                        if (_keepGoing)
                            Task.Run(() => ProcessRequest(context));
                    }
                }
                catch (Exception ex)
                {
                    if (ex is HttpListenerException)
                    {
                        _keepGoing = false;

                        if (stopserver)
                        {
                            RemoveAllPrefixes(listener);
                            stopserver = false;
                        }
                        else
                        {
                            _keepGoing = false;

                            ServerConfiguration.LogError($"[HTTP] - FATAL ERROR OCCURED - {ex} - Trying to listen for requests again...");

                            if (!listener.IsListening)
                            {
                                _keepGoing = true;
                                listener.Start();
                            }
                            else
                                _keepGoing = true;
                        }
                    }
                }
            }
        }

        private async static Task ProcessRequest(HttpListenerContext ctx)
        {
            string clientip = string.Empty;

            try
            {
                clientip = ctx.Request.RemoteEndPoint.Address.ToString();

                if (ctx.Request.UserAgent == null || ctx.Request.UserAgent == "" || ServerConfiguration.IsIPBanned(ctx.Request.RemoteEndPoint.Address.ToString()))
                {
                    ServerConfiguration.LogError($"[SECURITY] - Client - {clientip} Requested the HTTP server while being banned!");

                    ctx.Response.StatusCode = 403;
                    ctx.Response.Close();
                    return;
                }
                else
                    ServerConfiguration.LogInfo($"[SECURITY] - Client - {clientip} Requested the HTTP server.");
            }
            catch (Exception)
            {
                ctx.Response.StatusCode = 403;
                ctx.Response.Close();
                return;
            }

            if (Server.plugins.Count > 0)
            {
                foreach (var plugin in Server.plugins)
                {
                    _ = plugin.HTTPExecute(ctx.Request, ctx.Response, ctx.Request.UserAgent);
                }
            }

            if (ctx.Request.Url != null && ctx.Request.RawUrl != null)
            {
                int num = 0;
                foreach (char value in ctx.Request.Url.AbsolutePath.ToLower().Replace(Path.DirectorySeparatorChar, '/'))
                {
                    num *= 37;
                    num += Convert.ToInt32(value);
                }

                string crc32 = num.ToString("X8");

                ServerConfiguration.LogInfo($"[HTTP] - {ctx.Request.UserAgent} Requested {ctx.Request.Url.AbsolutePath} with CRC32 {crc32}");

                // Don't use Request.RawUrl, because it contains url parameters. (e.g. '?a=1&b=2')
                string absolutepath = ctx.Request.Url.AbsolutePath;

                if (absolutepath == null || absolutepath.Length <= 0)
                {
                    ctx.Response.StatusCode = 403;
                    ctx.Response.Close();
                    return;
                }

                bool specialrequest = true;

                if (absolutepath == "/" || absolutepath == "\\")
                {
                    byte[] fileBuffer = Encoding.UTF8.GetBytes(PreMadeWebPages.homepage(clientip));

                    ctx.Response.ContentType = "text/html";
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentLength64 = fileBuffer.Length;

                    if (ctx.Response.OutputStream.CanWrite)
                    {
                        try
                        {
                            ctx.Response.OutputStream.Write(fileBuffer, 0, fileBuffer.Length);
                            ctx.Response.OutputStream.Close();
                        }
                        catch (Exception)
                        {
                            // Not Important.
                        }
                    }
                }
                else if (ctx.Request.UserAgent.Contains("PSHome")) // Home has subservers running on Port 80.
                {
                    string requesthost = ctx.Request.Headers["Host"];

                    if (requesthost != null && requesthost == "sonyhome.thqsandbox.com")
                        await UFCClass.processrequest(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/ohs") || requesthost == "stats.outso-srv1.com")
                        await OHSClass.processrequest(crc32, ctx.Request, ctx.Response);
                    else if ((requesthost == "away.veemee.com" || requesthost == "home.veemee.com") && absolutepath.EndsWith(".php"))
                        await VEEMEEClass.processrequest(ctx.Request, ctx.Response);
                    else if ((requesthost == "game2.hellfiregames.com" && absolutepath.EndsWith(".php")) || absolutepath == "/Postcards/")
                        await HELLFIREGAMESClass.processrequest(ctx.Request, ctx.Response);
                    else if (requesthost == "pshome.ndreams.net" && absolutepath.EndsWith(".php"))
                        await NDREAMSClass.processrequest(ctx.Request, ctx.Response);
                    else if (requesthost == "api.pottermore.com")
                        await POTTERMOREClass.processrequest(ctx.Request, ctx.Response);
                    else
                        specialrequest = false;
                }
                else if (absolutepath.Contains("/!yt/")) // Todo make it more safe
                {
                    // Extract the URL after "/!yt/"
                    string[] segments = ctx.Request.RawUrl.Split(new[] { "/!yt/" }, StringSplitOptions.None);

                    if (segments.Length >= 2)
                    {
                        byte[] ytData = await Extensions.YoutubeDLP(segments[1], ctx.Request.RawUrl);

                        if (ytData == null)
                            ctx.Response.StatusCode = 500;
                        else
                        {
                            ctx.Response.AddHeader("Content-disposition", "attachment; filename=youtube-video.mp4");
                            ctx.Response.ContentType = "video/mp4";
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentLength64 = ytData.Length;

                            if (ctx.Response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    ctx.Response.OutputStream.Write(ytData, 0, ytData.Length);
                                    ctx.Response.OutputStream.Close();
                                }
                                catch (Exception)
                                {
                                    // Not Important.
                                }
                            }
                        }
                    }
                    else
                        ctx.Response.StatusCode = 403;
                }
                else
                    specialrequest = false;

                if (!specialrequest)
                {
                    if (absolutepath.ToLower().EndsWith(".php"))
                    {
                        if (!Directory.Exists(Directory.GetCurrentDirectory() + ServerConfiguration.PHPStaticFolder))
                        {
                            byte[] fileBuffer = Encoding.UTF8.GetBytes(PreMadeWebPages.phpnotenabled);

                            ctx.Response.ContentType = "text/html";
                            ctx.Response.StatusCode = 404;
                            ctx.Response.ContentLength64 = fileBuffer.Length;

                            if (ctx.Response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    ctx.Response.OutputStream.Write(fileBuffer, 0, fileBuffer.Length);
                                    ctx.Response.OutputStream.Close();
                                }
                                catch (Exception)
                                {
                                    // Not Important.
                                }
                            }
                        }
                        else
                        {
                            byte[] fileBuffer = FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}{absolutepath}", HTTPPrivateKey.HTTPPrivatekey); ;

                            if (fileBuffer != null)
                            {
                                if (Misc.FindbyteSequence(fileBuffer, new byte[] { 0x3c, 0x3f, 0x70, 0x68, 0x70 }))
                                {
                                    fileBuffer = Extensions.ProcessPHPPage(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}{absolutepath}", ServerConfiguration.PHPVersion, $"{ctx.Request.Url.Scheme}://{ctx.Request.Url.Authority}{ctx.Request.RawUrl}", ctx.Request, ctx.Response);
                                    ctx.Response.ContentType = "text/html";
                                    ctx.Response.StatusCode = 200;
                                    ctx.Response.ContentLength64 = fileBuffer.Length;

                                    if (ctx.Response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            ctx.Response.OutputStream.Write(fileBuffer, 0, fileBuffer.Length);
                                            ctx.Response.OutputStream.Close();
                                        }
                                        catch (Exception)
                                        {
                                            // Not Important.
                                        }
                                    }
                                }
                                else
                                {
                                    ctx.Response.ContentType = "text/plain";
                                    ctx.Response.StatusCode = 200;
                                    ctx.Response.ContentLength64 = fileBuffer.Length;

                                    if (ctx.Response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            ctx.Response.OutputStream.Write(fileBuffer, 0, fileBuffer.Length);
                                            ctx.Response.OutputStream.Close();
                                        }
                                        catch (Exception)
                                        {
                                            // Not Important.
                                        }
                                    }
                                }
                            }
                            else
                            {
                                ctx.Response.StatusCode = 500;
                            }
                        }
                    }
                    else
                    {
                        string url = ctx.Request.Url.LocalPath;

                        // Split the URL into segments
                        string[] segments = url.Trim('/').Split('/');

                        // Combine the folder segments into a directory path
                        string directoryPath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

                        // Process the request based on the HTTP method
                        string filePath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, url.Substring(1));

                        switch (ctx.Request.HttpMethod)
                        {
                            case "GET":
                                if (ServerConfiguration.EnableHomeTools && ServerConfiguration.IsIPAllowed(clientip))
                                {
                                    switch (absolutepath)
                                    {
                                        case "/!HomeTools/MakeBarSdat/":
                                            byte[] MakeBarSdatData = Encoding.UTF8.GetBytes(PreMadeWebPages.MakeBarSdat);

                                            ctx.Response.ContentType = "text/html";
                                            ctx.Response.StatusCode = 200;
                                            ctx.Response.ContentLength64 = MakeBarSdatData.Length;

                                            if (ctx.Response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    ctx.Response.OutputStream.Write(MakeBarSdatData, 0, MakeBarSdatData.Length);
                                                    ctx.Response.OutputStream.Close();
                                                }
                                                catch (Exception)
                                                {
                                                    // Not Important.
                                                }
                                            }
                                            break;
                                        case "/!HomeTools/UnBar/":
                                            byte[] UnBarData = Encoding.UTF8.GetBytes(PreMadeWebPages.UnBar);

                                            ctx.Response.ContentType = "text/html";
                                            ctx.Response.StatusCode = 200;
                                            ctx.Response.ContentLength64 = UnBarData.Length;

                                            if (ctx.Response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    ctx.Response.OutputStream.Write(UnBarData, 0, UnBarData.Length);
                                                    ctx.Response.OutputStream.Close();
                                                }
                                                catch (Exception)
                                                {
                                                    // Not Important.
                                                }
                                            }
                                            break;
                                        case "/!HomeTools/ChannelID/":
                                            byte[] ChannelIDData = Encoding.UTF8.GetBytes(PreMadeWebPages.ChannelID.Replace("PUT_GUID_HERE", "00000000-0000-0000-0000-000000000000"));

                                            ctx.Response.ContentType = "text/html";
                                            ctx.Response.StatusCode = 200;
                                            ctx.Response.ContentLength64 = ChannelIDData.Length;

                                            if (ctx.Response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    ctx.Response.OutputStream.Write(ChannelIDData, 0, ChannelIDData.Length);
                                                    ctx.Response.OutputStream.Close();
                                                }
                                                catch (Exception)
                                                {
                                                    // Not Important.
                                                }
                                            }
                                            break;
                                        case "/!HomeTools/SceneID/":
                                            byte[] SceneIDData = Encoding.UTF8.GetBytes(PreMadeWebPages.SceneID.Replace("PUT_SCENEID_HERE", ""));

                                            ctx.Response.ContentType = "text/html";
                                            ctx.Response.StatusCode = 200;
                                            ctx.Response.ContentLength64 = SceneIDData.Length;

                                            if (ctx.Response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    ctx.Response.OutputStream.Write(SceneIDData, 0, SceneIDData.Length);
                                                    ctx.Response.OutputStream.Close();
                                                }
                                                catch (Exception)
                                                {
                                                    // Not Important.
                                                }
                                            }
                                            break;
                                        case "/!HomeTools/INF/":
                                            byte[] INFData = Encoding.UTF8.GetBytes(PreMadeWebPages.INF);

                                            ctx.Response.ContentType = "text/html";
                                            ctx.Response.StatusCode = 200;
                                            ctx.Response.ContentLength64 = INFData.Length;

                                            if (ctx.Response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    ctx.Response.OutputStream.Write(INFData, 0, INFData.Length);
                                                    ctx.Response.OutputStream.Close();
                                                }
                                                catch (Exception)
                                                {
                                                    // Not Important.
                                                }
                                            }
                                            break;
                                        case "/!HomeTools/CDS/":
                                            byte[] CDSData = Encoding.UTF8.GetBytes(PreMadeWebPages.CDS);

                                            ctx.Response.ContentType = "text/html";
                                            ctx.Response.StatusCode = 200;
                                            ctx.Response.ContentLength64 = CDSData.Length;

                                            if (ctx.Response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    ctx.Response.OutputStream.Write(CDSData, 0, CDSData.Length);
                                                    ctx.Response.OutputStream.Close();
                                                }
                                                catch (Exception)
                                                {
                                                    // Not Important.
                                                }
                                            }
                                            break;
                                        case "/!videoplayer/":
                                            byte[] videoplayerData = Encoding.UTF8.GetBytes(PreMadeWebPages.videoplayer);

                                            ctx.Response.ContentType = "text/html";
                                            ctx.Response.StatusCode = 200;
                                            ctx.Response.ContentLength64 = videoplayerData.Length;

                                            if (ctx.Response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    ctx.Response.OutputStream.Write(videoplayerData, 0, videoplayerData.Length);
                                                    ctx.Response.OutputStream.Close();
                                                }
                                                catch (Exception)
                                                {
                                                    // Not Important.
                                                }
                                            }
                                            break;
                                        default:
                                            await RootProcess.ProcessRootRequest(filePath, directoryPath, crc32, ctx.Request, ctx.Response);
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (absolutepath)
                                    {
                                        case "/!videoplayer/":
                                            byte[] videoplayerData = Encoding.UTF8.GetBytes(PreMadeWebPages.videoplayer);

                                            ctx.Response.ContentType = "text/html";
                                            ctx.Response.StatusCode = 200;
                                            ctx.Response.ContentLength64 = videoplayerData.Length;

                                            if (ctx.Response.OutputStream.CanWrite)
                                            {
                                                try
                                                {
                                                    ctx.Response.OutputStream.Write(videoplayerData, 0, videoplayerData.Length);
                                                    ctx.Response.OutputStream.Close();
                                                }
                                                catch (Exception)
                                                {
                                                    // Not Important.
                                                }
                                            }
                                            break;
                                        default:
                                            await RootProcess.ProcessRootRequest(filePath, directoryPath, crc32, ctx.Request, ctx.Response);
                                            break;
                                    }
                                }
                                break;
                            case "POST":
                                if (ServerConfiguration.EnableHomeTools && ServerConfiguration.IsIPAllowed(clientip))
                                {
                                    switch (absolutepath)
                                    {
                                        case "/!HomeTools/Packaging/":
                                            await HomeTools.MakeBarSdat(ctx.Request, ctx.Response);
                                            break;
                                        case "/!HomeTools/UnBarPackaging/":
                                            await HomeTools.UnBARProcess(ctx.Request, ctx.Response);
                                            break;
                                        case "/!HomeTools/ChannelIDHandling/":
                                            await HomeTools.ChannelIDCalculator(ctx.Request, ctx.Response);
                                            break;
                                        case "/!HomeTools/SceneIDHandling/":
                                            await HomeTools.SceneIDCalculator(ctx.Request, ctx.Response);
                                            break;
                                        case "/!HomeTools/INFProcess/":
                                            await HomeTools.INFProcess(ctx.Request, ctx.Response);
                                            break;
                                        case "/!HomeTools/CDSProcess/":
                                            await HomeTools.CDSProcess(ctx.Request, ctx.Response);
                                            break;
                                        default:
                                            ctx.Response.StatusCode = 403;
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (absolutepath)
                                    {
                                        default:
                                            ctx.Response.StatusCode = 403;
                                            break;
                                    }
                                }
                                break;
                            case "HEAD":
                                if (File.Exists(filePath))
                                {
                                    byte[] output = FileHelper.CryptoReadAsync(filePath, HTTPPrivateKey.HTTPPrivatekey);

                                    if (output != null)
                                    {
                                        ctx.Response.ContentType = MimeTypes.GetMimeType(filePath);
                                        ctx.Response.StatusCode = 200;
                                        ctx.Response.ContentLength64 = output.Length;
                                    }
                                    else
                                        ctx.Response.StatusCode = 500;
                                }
                                else
                                {
                                    ServerConfiguration.LogWarn($"[HTTP] : {ctx.Request.UserAgent} Requested a non-existing file: '{filePath}'.");
                                    ctx.Response.StatusCode = 404;
                                }
                                break;
                            default:
                                ctx.Response.StatusCode = 403;
                                break;
                        }
                    }
                }
            }
            else
                ctx.Response.StatusCode = 403;

            ctx.Response.Close();
        }

        private static bool RemoveAllPrefixes(HttpListener listener)
        {
            // Get the prefixes that the Web server is listening to.
            HttpListenerPrefixCollection prefixes = listener.Prefixes;
            try
            {
                prefixes.Clear();
            }
            // If the operation failed, return false.
            catch
            {
                return false;
            }

            return true;
        }

        public static void HTTPstop()
        {
            stopserver = true;
            listener.Stop();
            _keepGoing = false;

            httpstarted = false;
        }
    }
}
