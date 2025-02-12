using NetworkLibrary.HTTP;
using CustomLogger;
using HttpMultipartParser;
using System.Text;
using SpaceWizards.HttpListener;
using System.Security.Cryptography.X509Certificates;
using SVO.Games.PS3;

namespace SVO
{
    public class SVOServer
    {
        public static bool IsStarted = false;

        const ushort securePort = 10061;

        private X509Certificate2? certificate;
        private Thread? thread;
        private volatile bool threadActive;

        private HttpListener? listener;
        private readonly string ip;

        private int MaxConcurrentListeners;

        public SVOServer(string ip, X509Certificate2? certificate = null, int MaxConcurrentListeners = 10)
        {
            this.ip = ip;
            this.certificate = certificate;
            this.MaxConcurrentListeners = MaxConcurrentListeners;

            Start();
        }

        public static bool IsIPBanned(string ipAddress)
        {
            if (SVOServerConfiguration.BannedIPs != null && SVOServerConfiguration.BannedIPs.Contains(ipAddress))
                return true;

            return false;
        }

        public void Start()
        {
            if (thread != null)
            {
                LoggerAccessor.LogWarn("[SVO] - Server already active.");
                return;
            }
            thread = new Thread(Listen);
            thread.Start();
            LoggerAccessor.LogInfo("[SVO] - Server started...");
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
                RemoveAllPrefixes(listener);
                listener.Close();
                listener = null;
            }
            IsStarted = false;
        }

        private bool RemoveAllPrefixes(HttpListener listener)
        {
            try
            {
                // Get the prefixes that the Web server is listening to.
                listener.Prefixes.Clear();
            }
            // If the operation failed, return false.
            catch
            {
                return false;
            }

            return true;
        }

        private async void Listen()
        {
            threadActive = true;

            // start listener
            try
            {
                listener = new HttpListener();
                if (certificate != null)
                    listener.SetCertificate(securePort, certificate);
				listener.Prefixes.Add(string.Format("http://{0}:{1}/", ip, 10058));
                listener.Prefixes.Add(string.Format("http://{0}:{1}/", ip, 10060));
                listener.Prefixes.Add(string.Format("https://{0}:{1}/", ip, securePort));
                listener.Start();

                HashSet<Task> requests = new();
                for (int i = 0; i < MaxConcurrentListeners; i++)
                    requests.Add(listener.GetContextAsync());

                // wait for requests
                while (threadActive)
                {
                    try
                    {
                        if (!threadActive) break;

                        Task t = await Task.WhenAny(requests);

                        if (t is Task<HttpListenerContext>)
                        {
                            HttpListenerContext? ctx = null;

                            try
                            {
                                ctx = (t as Task<HttpListenerContext>)?.Result;
                            }
                            catch (AggregateException ex)
                            {
                                ex.Handle(innerEx =>
                                {
                                    if (innerEx is TaskCanceledException)
                                        return true; // Indicate that the exception was handled

                                    LoggerAccessor.LogWarn($"[SVO] - HttpListenerContext Task thrown an AggregateException: {ex}");

                                    return false;
                                });
                            }

                            _ = ProcessContext(ctx);
                        }

                        requests.Remove(t);
                        requests.Add(listener.GetContextAsync());
                    }
                    catch (HttpListenerException e)
                    {
                        if (e.ErrorCode != 995) LoggerAccessor.LogError("[SVO] - A HttpListenerException Occured: " + e.Message);
                    }
                    catch (Exception e)
                    {
                        LoggerAccessor.LogError("[SVO] - An Exception Occured: " + e.Message);
                    }
                }
				
                requests.Clear();
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError("[SVO] - An Exception Occured while starting the http server: " + e.Message);
                threadActive = false;
                return;
            }
        }

        private async Task ProcessContext(HttpListenerContext? ctx)
        {
            if (ctx == null)
                return;

            try
            {

                bool isAllowed = false;
                string absolutepath = ctx.Request.Url.AbsolutePath;
                string clientip = ctx.Request.RemoteEndPoint?.Address.ToString() ?? string.Empty;

                if (IsIPBanned(clientip))
                    LoggerAccessor.LogError($"[SECURITY] - Client - {clientip} Requested the SVO server while being banned!");
                else
                {
                    string? UserAgent = null;

                    if (!string.IsNullOrEmpty(ctx.Request.UserAgent))
                        UserAgent = ctx.Request.UserAgent.ToLower();

                    if (!string.IsNullOrEmpty(UserAgent) && UserAgent.Contains("bytespider")) // Get Away TikTok.
                        LoggerAccessor.LogInfo($"[SVO] - Client - {clientip} Requested the SVO Server while not being allowed!");
                    else if (!string.IsNullOrEmpty(absolutepath))
                    {
                        LoggerAccessor.LogInfo($"[SVO] - Client - {clientip} Requested the SVO Server with URL : {ctx.Request.Url}");
                        isAllowed = true;
                    }
                    else
                        LoggerAccessor.LogInfo($"[SVO] - Client - {clientip} Requested the SVO Server with invalid url!");
                }

                if (isAllowed)
                {
                    if (absolutepath == "/dataloaderweb/queue")
                    {
                        switch (ctx.Request.HttpMethod)
                        {
                            case "POST":
                                if (!string.IsNullOrEmpty(ctx.Request.ContentType))
                                {
                                    ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                    ctx.Response.Headers.Set("Content-Language", string.Empty);
                                    string? boundary = HTTPProcessor.ExtractBoundary(ctx.Request.ContentType);

                                    var data = MultipartFormDataParser.Parse(ctx.Request.InputStream, boundary);

                                    byte[] datatooutput = Encoding.UTF8.GetBytes(data.GetParameterValue("body"));

                                    Directory.CreateDirectory($"{SVOServerConfiguration.SVOStaticFolder}/dataloaderweb/queue");

                                    DirectoryInfo directory = new($"{SVOServerConfiguration.SVOStaticFolder}/dataloaderweb/queue");

                                    FileInfo[] files = directory.GetFiles();

                                    if (files.Length >= 20)
                                    {
                                        FileInfo oldestFile = files.OrderBy(file => file.CreationTime).First();
                                        LoggerAccessor.LogInfo("[SVO] - Replacing Home Debug log file: " + oldestFile.Name);
                                        if (File.Exists(oldestFile.FullName))
                                            File.Delete(oldestFile.FullName);
                                    }

                                    File.WriteAllBytes($"{SVOServerConfiguration.SVOStaticFolder}/dataloaderweb/queue/{Guid.NewGuid()}.xml", datatooutput);

                                    ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                                    ctx.Response.SendChunked = true;

                                    if (ctx.Response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            ctx.Response.ContentLength64 = datatooutput.Length;
                                            ctx.Response.OutputStream.Write(datatooutput, 0, datatooutput.Length);
                                        }
                                        catch (Exception)
                                        {
                                            // Not Important.
                                        }
                                    }
                                }
                                else
                                    ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                                break;
                            default:
                                ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                                break;
                        }
                    }
                    else if (absolutepath.Contains("/HUBPS3_SVML/"))
                        await PlayStationHome.Home_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/WARHAWK_SVML/"))
                        await Warhawk.Warhawk_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/MOTORSTORM2PS3_SVML/") || absolutepath.Contains("/MOTORSTORM2PS3_XML/"))
                        await MotorstormPR2.MotorStormPR_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/motorstorm3ps3_xml/"))
                        await MotorStormApocalypse.MSApocalypse_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/BUZZPS3_SVML/"))
                        await BuzzQuizGame.BuzzQuizGame_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/BOURBON_XML/"))
                        await Starhawk.Starhawk_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/CONFRONTATION_XML/"))
                        await SocomConfrontation.SocomConfrontation_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/SINGSTARPS3_SVML/"))
                        await SingStar.Singstar_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/TWISTEDMETALX_XML/"))
                        await TwistedMetalX.TwistedMetalX_SVO(ctx.Request, ctx.Response);
                    else
                    {
                        // Only meant to be used with fairly small files.
                        string filePath = Path.Combine(SVOServerConfiguration.SVOStaticFolder, absolutepath[1..]);

                        if (File.Exists(filePath))
                        {
                            ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                            ctx.Response.ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(filePath), HTTPProcessor._mimeTypes);

                            ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));

                            byte[] FileContent = File.ReadAllBytes(filePath);

                            if (ctx.Response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    ctx.Response.ContentLength64 = FileContent.Length;
                                    ctx.Response.OutputStream.Write(FileContent, 0, FileContent.Length);
                                }
                                catch (Exception)
                                {
                                    // Not Important;
                                }
                            }
                        }
                        else
                            ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    }
                }
                else
                    ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
            }
            catch (HttpListenerException e) when (e.ErrorCode == 64)
            {
                // Unfortunately, some client side implementation of HTTP (like RPCS3) freeze the interface at regular interval.
                // This will cause server to throw error 64 (network interface not openned anymore)
                // In that case, we send internalservererror so client try again.

                ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError("[SVO] - REQUEST ERROR: " + e.Message);
                ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
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
