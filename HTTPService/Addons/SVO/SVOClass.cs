using System.Net;
using HttpMultipartParser;
using MultiServer.HTTPService.Addons.SVO.Games;
using System.Text;

namespace MultiServer.HTTPService.Addons.SVO
{
    public class SVOClass
    {

        private static volatile bool _keepGoing = true;

        private static bool stopserver = false;

        public static bool svostarted = false;

        // Create and start the HttpListener
        private static HttpListener listener = new();

        private static Task? _mainLoop;

        private static DateTime? _lastSuccessfulDbAuth;

        public static SVOManager Manager = new();

        private static DateTime _lastComponentLog = MultiServer.Addons.Horizon.LIBRARY.Common.Utils.GetHighPrecisionUtcTime();

        private static async Task TickAsync()
        {
            try
            {
                // Attempt to authenticate with the db middleware
                // We do this every 24 hours to get a fresh new token
                if (_lastSuccessfulDbAuth == null || (MultiServer.Addons.Horizon.LIBRARY.Common.Utils.GetHighPrecisionUtcTime() - _lastSuccessfulDbAuth.Value).TotalHours > 24)
                {
                    if (!await ServerConfiguration.Database.Authenticate())
                    {
                        // Log and exit when unable to authenticate
                        ServerConfiguration.LogError($"Unable to authenticate connection to Cache Server.");
                        return;
                    }
                    else
                    {
                        _lastSuccessfulDbAuth = MultiServer.Addons.Horizon.LIBRARY.Common.Utils.GetHighPrecisionUtcTime();

                        // pass to manager
                        await Manager.OnDatabaseAuthenticated();

                        #region Check Cache Server Simulated
                        if (ServerConfiguration.Database._settings.SimulatedMode != true)
                            ServerConfiguration.LogInfo("Connected to Cache Server");
                        else
                            ServerConfiguration.LogInfo("Connected to Cache Server (Simulated)");
                        #endregion
                    }
                }

                if ((MultiServer.Addons.Horizon.LIBRARY.Common.Utils.GetHighPrecisionUtcTime() - _lastComponentLog).TotalSeconds > 15f)
                    _lastComponentLog = MultiServer.Addons.Horizon.LIBRARY.Common.Utils.GetHighPrecisionUtcTime();
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }
        }

        public static async Task StartTickPooling()
        {
            // iterate
            while (true)
            {
                // tick
                await TickAsync();

                await Task.Delay(100);
            }
        }

        public static Task SVOstart(bool ssl)
        {
            svostarted = true;

            PrepareFolder.Prepare();

            stopserver = false;
            _keepGoing = true;
            if (_mainLoop != null && !_mainLoop.IsCompleted) return Task.CompletedTask; //Already started
            _mainLoop = loopserver(ssl);

            return Task.CompletedTask;
        }

        private async static Task loopserver(bool ssl)
        {
            listener.Prefixes.Add("http://*:10060/");

            ServerConfiguration.LogInfo($"SVO Server started - Listening for requests...");

            listener.Start();

            if (ssl && !SVOHTTPSClass.httpsstarted)
                _ = Task.Run(SVOHTTPSClass.StartSVOHTTPSServer);

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

                            ServerConfiguration.LogError($"[SVO] - FATAL ERROR OCCURED - {ex} - Trying to listen for requests again...");

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

        private static async Task ProcessRequest(HttpListenerContext ctx)
        {
            try
            {
                string clientip = ctx.Request.RemoteEndPoint.Address.ToString();

                if (ctx.Request.Url == null || ServerConfiguration.IsIPBanned(clientip))
                {
                    ServerConfiguration.LogError($"[BSECURITY] - Client - {clientip} Requested the SVO server while being banned!");

                    ctx.Response.StatusCode = 403;
                    ctx.Response.Close();
                    return;
                }
                else
                    ServerConfiguration.LogInfo($"[SECURITY] - Client - {clientip} Requested the SVO server.");
            }
            catch (Exception)
            {
                ctx.Response.StatusCode = 403;
                ctx.Response.Close();
                return;
            }

            if (ctx.Request.UserAgent == null || ctx.Request.UserAgent == "")
                ServerConfiguration.LogInfo($"[SVO] - Client Requested {ctx.Request.Url.AbsolutePath}");
            else
                ServerConfiguration.LogInfo($"[SVO] - {ctx.Request.UserAgent} Requested {ctx.Request.Url.AbsolutePath}");

            string absolutepath = ctx.Request.Url.AbsolutePath;

            switch (absolutepath)
            {
                case "/dataloaderweb/queue":
                    if (ctx.Request.ContentType != null && ctx.Request.ContentType.StartsWith("multipart/form-data"))
                    {
                        switch (ctx.Request.HttpMethod)
                        {
                            case "POST":
                                ctx.Response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                ctx.Response.Headers.Set("Content-Language", "");

                                string boundary = Extensions.ExtractBoundary(ctx.Request.ContentType);

                                var data = MultipartFormDataParser.Parse(ctx.Request.InputStream, boundary);

                                byte[] datatooutput = Encoding.UTF8.GetBytes(data.GetParameterValue("body"));

                                DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}/dataloaderweb/queue");

                                FileInfo[] files = directory.GetFiles();

                                if (files.Length >= 20)
                                {
                                    FileInfo oldestFile = files.OrderBy(file => file.CreationTime).First();
                                    ServerConfiguration.LogInfo("[SVO] - Replacing log file: " + oldestFile.Name);

                                    File.WriteAllBytes(oldestFile.FullName, datatooutput);
                                }
                                else
                                    File.WriteAllBytes(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SVOStaticFolder}/dataloaderweb/queue/{Guid.NewGuid()}.xml", datatooutput);

                                ctx.Response.StatusCode = 200;
                                ctx.Response.SendChunked = true;
                                ctx.Response.ContentLength64 = datatooutput.Length;

                                if (ctx.Response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        ctx.Response.OutputStream.Write(datatooutput, 0, datatooutput.Length);
                                        ctx.Response.OutputStream.Close();
                                    }
                                    catch (Exception)
                                    {
                                        // Not Important.
                                    }
                                }
                                break;
                            default:
                                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                break;
                        }
                        break;
                    }
                    break;

                default:
                    if (absolutepath.Contains("/HUBPS3_SVML/"))
                        await Games.PlayStationHome.Home_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/motorstorm3ps3_xml/"))
                        await MotorStormApocalypse.MSApocalypse_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/wox_ws/"))
                        await WipeoutHD.WipeoutHD_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/BUZZPS3_SVML/"))
                        await BuzzQuizGame.BuzzQuizGame_SVO(ctx.Request, ctx.Response);
                    else if (absolutepath.Contains("/BOURBON_XML/"))
                        await Starhawk.Starhawk_SVO(ctx.Request, ctx.Response);
                    else
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
            }

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

        public static void SVOstop()
        {
            stopserver = true;
            listener.Stop();
            _keepGoing = false;

            svostarted = false;
        }
    }
}
