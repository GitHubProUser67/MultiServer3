using CustomLogger;
using HttpMultipartParser;
using SVO.Games;
using System.Net;
using System.Text;

namespace SVO
{
    public class SVOServer
    {
        public static bool IsStarted = false;

        private Thread? thread;
        private volatile bool threadActive;

        private HttpListener? listener;
        private string ip;

        public SVOServer(string ip)
        {
            this.ip = ip;
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
                LoggerAccessor.LogError("SVO Server already active.");
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

        private void Listen()
        {
            threadActive = true;

            // start listener
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(string.Format("http://{0}:{1}/", ip, 10060));
                listener.Prefixes.Add(string.Format("http://{0}:{1}/", ip, 10058));
                listener.Start();
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError("[SVO] - ERROR: " + e.Message);
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
                    if (e.ErrorCode != 995) LoggerAccessor.LogError("[SVO] - ERROR: " + e.Message);
                    threadActive = false;
                }
                catch (Exception e)
                {
                    LoggerAccessor.LogError("[SVO] - ERROR: " + e.Message);
                    threadActive = false;
                }
            }
        }

        private async void ProcessContext(HttpListenerContext ctx)
        {
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
                    string? UserAgent = ctx.Request.UserAgent;
                    if (!string.IsNullOrEmpty(UserAgent) && (UserAgent.ToLower().Contains("firefox") || UserAgent.ToLower().Contains("chrome") || UserAgent.ToLower().Contains("trident")))
                        LoggerAccessor.LogInfo($"[SVO] - Client - {clientip} Requested the SVO Server while not being allowed!");
                    else
                    {
                        if (ctx.Request.Url != null && ctx.Request.Url.AbsolutePath != null && ctx.Request.Url.LocalPath != null)
                        {
                            LoggerAccessor.LogInfo($"[SVO] - Client - {clientip} Requested the SVO Server with URL : {ctx.Request.Url}");
                            // get filename path
                            absolutepath = ctx.Request.Url.AbsolutePath;
                            isok = true;
                        }
                        else
                            LoggerAccessor.LogInfo($"[SVO] - Client - {clientip} Requested the SVO Server with invalid parameters!");
                    }
                }
            }
            catch (Exception)
            {

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
                                    string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ctx.Request.ContentType);

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
                                            ctx.Response.OutputStream.Close();
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
                    else if (absolutepath.Contains("/motorstorm3ps3_xml/"))
                        await MotorStormApocalypse.MSApocalypse_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/wox_ws/"))
                        await WipeoutHD.WipeoutHD_OTG(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/BUZZPS3_SVML/"))
                        await BuzzQuizGame.BuzzQuizGame_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/BOURBON_XML/"))
                        await Starhawk.Starhawk_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/SOCOMCF_SVML/"))
                        await SocomConfrontation.SocomConfrontation_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/TWISTEDMETALX_XML/"))
                        await TwistedMetalX.TwistedMetalX_SVO(ctx.Request, ctx.Response);
                    else
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
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
