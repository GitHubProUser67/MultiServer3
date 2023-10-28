using CustomLogger;
using HttpMultipartParser;
using System.Net;
using System.Text;

namespace SVO
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
                    if (ctx.Request.Url != null && ctx.Request.Url.AbsolutePath != null && ctx.Request.Url.LocalPath != null)
                    {
                        // get filename path
                        absolutepath = ctx.Request.Url.AbsolutePath;
                        isok = true;
                    }
                }
            }
            catch (Exception)
            {

            }

            if (isok)
            {
                LoggerAccessor.LogInfo($"[SVO] - {clientip} Requested : {absolutepath}");

                if (absolutepath == "/dataloaderweb/queue")
                {
                    switch (ctx.Request.HttpMethod)
                    {
                        case "POST":
                            if (!string.IsNullOrEmpty(ctx.Request.ContentType))
                            {
                                ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                ctx.Response.Headers.Set("Content-Language", string.Empty);
                                string? boundary = ExtractBoundary(ctx.Request.ContentType);

                                var data = MultipartFormDataParser.Parse(ctx.Request.InputStream, boundary);

                                byte[] datatooutput = Encoding.UTF8.GetBytes(data.GetParameterValue("body"));

                                Directory.CreateDirectory($"{SVOServerConfiguration.SVOStaticFolder}/dataloaderweb/queue");

                                DirectoryInfo directory = new DirectoryInfo($"{SVOServerConfiguration.SVOStaticFolder}/dataloaderweb/queue");

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
                else
                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            else
                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            ctx.Response.Close();
        }

        public string? ExtractBoundary(string contentType)
        {
            int boundaryIndex = contentType.IndexOf("boundary=", StringComparison.InvariantCultureIgnoreCase);
            if (boundaryIndex != -1)
                return contentType.Substring(boundaryIndex + 9);
            return null;
        }
    }
}
