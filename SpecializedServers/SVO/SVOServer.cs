using CyberBackendLibrary.HTTP;
using CustomLogger;
using HttpMultipartParser;
using SVO.Games;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace SVO
{
    public class SVOServer
    {
        public static bool IsStarted = false;

        private Thread? thread;
        private volatile bool threadActive;

        private HttpListener? listener;
        private readonly string ip;

        public SVOServer(string ip)
        {
            this.ip = ip;

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
                LoggerAccessor.LogWarn("SVO Server already active.");
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
				listener.Prefixes.Add(string.Format("http://{0}:{1}/", ip, 10058));
                listener.Prefixes.Add(string.Format("http://{0}:{1}/", ip, 10060));
                listener.Prefixes.Add(string.Format("http://{0}:{1}/", ip, 10061));
                listener.Start();
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError("[SVO] - An Exception Occured while starting the http server: " + e.Message);
                threadActive = false;
                return;
            }

            HashSet<Task> requests = new();
            for (int i = 0; i < Environment.ProcessorCount; i++)
                requests.Add(listener.GetContextAsync());

            // wait for requests
            while (threadActive)
            {
                try
                {
                    if (!threadActive) break;

                    Task t = await Task.WhenAny(requests);
                    requests.Remove(t);

                    if (t is Task<HttpListenerContext>)
                    {
                        HttpListenerContext? ctx = (t as Task<HttpListenerContext>)?.Result;
                        requests.Add(ProcessContext(ctx));
                        requests.Add(listener.GetContextAsync());
                    }
                }
                catch (HttpListenerException e)
                {
                    if (e.ErrorCode != 995) LoggerAccessor.LogError("[SVO] - A HttpListenerException Occured: " + e.Message);
                    listener.Stop();

                    if (!listener.IsListening) // Check if server is closed, then, start it again.
                        listener.Start();
                    else
                        threadActive = false;
                }
                catch (Exception e)
                {
                    LoggerAccessor.LogError("[SVO] - An Exception Occured: " + e.Message);
                    listener.Stop();

                    if (!listener.IsListening) // Check if server is closed, then, start it again.
                        listener.Start();
                    else
                        threadActive = false;
                }
            }
        }

        private async Task ProcessContext(HttpListenerContext? ctx)
        {
            if (ctx == null)
                return;

            bool isok = false;
            string clientip = string.Empty;
            string absolutepath = string.Empty;

            try
            {
                clientip = ctx.Request.RemoteEndPoint.Address.ToString();

                if (IsIPBanned(clientip))
                    LoggerAccessor.LogError($"[SECURITY] - Client - {clientip} Requested the SVO server while being banned!");
                else
                {
                    string? UserAgent = null;

                    if (!string.IsNullOrEmpty(ctx.Request.UserAgent))
                        UserAgent = ctx.Request.UserAgent.ToLower();

                    if (!string.IsNullOrEmpty(UserAgent) && (UserAgent.Contains("firefox") || UserAgent.Contains("chrome") || UserAgent.Contains("trident") || UserAgent.Contains("bytespider"))) // Get Away TikTok.
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        LoggerAccessor.LogInfo($"[SVO] - Client - {clientip} Requested the SVO Server while not being allowed!");
                    }
                    else
                    {
                        if (ctx.Request.Url != null && !string.IsNullOrEmpty(ctx.Request.Url.AbsolutePath))
                        {
#if DEBUG
                            try
                            {
                                LoggerAccessor.LogJson(JsonConvert.SerializeObject(new
                                {
                                    HttpMethod = ctx.Request.HttpMethod,
                                    Url = ctx.Request.Url.ToString(),
                                    Headers = ctx.Request.Headers,
                                    HeadersValues = ctx.Request.Headers.AllKeys.SelectMany(key => ctx.Request.Headers.GetValues(key) ?? Enumerable.Empty<string>()),
                                    UserAgent = ctx.Request.UserAgent,
                                    ClientAddress = ctx.Request.RemoteEndPoint.ToString(),
                                }, Formatting.Indented), $"[[SVO]] - Client - {clientip} Requested the SVO Server with URL : {ctx.Request.Url}");
                            }
                            catch (Exception ex)
                            {
                                LoggerAccessor.LogError($"[SVO] - Thrown an exception while trying to generate DEBUG json data: {ex}");

                                LoggerAccessor.LogInfo($"[SVO] - Client - {clientip} Requested the SVO Server with URL : {ctx.Request.Url}");
                            }
#else
                            LoggerAccessor.LogInfo($"[SVO] - Client - {clientip} Requested the SVO Server with URL : {ctx.Request.Url}");
#endif

                            // get filename path
                            absolutepath = ctx.Request.Url.AbsolutePath;
                            isok = true;
                        }
                        else
                            LoggerAccessor.LogInfo($"[SVO] - Client - {clientip} Requested the SVO Server with invalid parameters!");
                    }
                }
            }
            catch
            {
                // Not Important.
            }

            if (isok)
            {
                try
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

                                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
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
                                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                break;
                            default:
                                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                break;
                        }
                    }
                    else if (absolutepath.Contains("/HUBPS3_SVML/"))
                        await PlayStationHome.Home_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/WARHAWK_SVML/"))
                        await Warhawk.Warhawk_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/motorstorm3ps3_xml/"))
                        await MotorStormApocalypse.MSApocalypse_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/BUZZPS3_SVML/"))
                        await BuzzQuizGame.BuzzQuizGame_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/BOURBON_XML/"))
                        await Starhawk.Starhawk_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/SOCOMCF_SVML/"))
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
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
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
                            ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                catch (HttpListenerException e) when (e.ErrorCode == 64)
                {
                    // Unfortunately, some client side implementation of HTTP (like RPCS3) freeze the interface at regular interval.
                    // This will cause server to throw error 64 (network interface not openned anymore)
                    // In that case, we send internalservererror so client try again.

                    ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
                catch (Exception e)
                {
                    LoggerAccessor.LogError("[SVO] - REQUEST ERROR: " + e.Message);
                    ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;

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
