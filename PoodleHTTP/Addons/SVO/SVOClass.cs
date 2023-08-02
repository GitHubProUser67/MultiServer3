using HttpMultipartParser;
using System.Net;
using System.Text;
using PSMultiServer.Addons.Horizon.Server.Database;
using System.Diagnostics;

namespace PSMultiServer.PoodleHTTP.Addons.SVO
{
    public class SVOClass
    {
        private static int _ticks = 0;

        private static int sleepMS = 0;

        private static Stopwatch _sw = new Stopwatch();

        private static DateTime? _lastSuccessfulDbAuth;

        public static DbController? Database;

        public static SVOManager Manager = new SVOManager();

        private static DateTime _lastComponentLog = PSMultiServer.Addons.Horizon.Server.Common.Utils.GetHighPrecisionUtcTime();

        private static PSMultiServer.Addons.Timer.HighResolutionTimer _timer;

        public static void setupdatabase()
        {
            if (!File.Exists(Directory.GetCurrentDirectory() + $"/{ServerConfiguration.SVODatabaseConfig}"))
            {
                Database = new DbController("{\r\n  \"SimulatedMode\": true,\r\n  \"DatabaseUrl\": null,\r\n  \"DatabaseUsername\": null,\r\n  \"DatabasePassword\": null\r\n}");
            }
            else
            {
                Database = new DbController(Directory.GetCurrentDirectory() + $"/{ServerConfiguration.SVODatabaseConfig}");
            }
        }

        public static async Task TickAsync()
        {
            try
            {
                if (!_sw.IsRunning)
                    _sw.Start();

                ++_ticks;
                if (_sw.Elapsed.TotalSeconds > 5f)
                {
                    // 
                    _sw.Stop();
                    float tps = _ticks / (float)_sw.Elapsed.TotalSeconds;
                    float error = MathF.Abs(ServerConfiguration.SVODBTickrate - tps) / ServerConfiguration.SVODBTickrate;

                    if (error > 0.1f)
                        ServerConfiguration.LogError($"Average TickRate Per Second: {tps} is {error * 100}% off of target {ServerConfiguration.SVODBTickrate}");
                    //var dt = DateTime.UtcNow - Utils.GetHighPrecisionUtcTime();
                    //if (Math.Abs(dt.TotalMilliseconds) > 50)
                    //    ServerConfiguration.LogError($"System clock and local clock are out of sync! delta ms: {dt.TotalMilliseconds}");

                    _sw.Restart();
                    _ticks = 0;
                }

                // Attempt to authenticate with the db middleware
                // We do this every 24 hours to get a fresh new token
                if ((_lastSuccessfulDbAuth == null || (PSMultiServer.Addons.Horizon.Server.Common.Utils.GetHighPrecisionUtcTime() - _lastSuccessfulDbAuth.Value).TotalHours > 24))
                {
                    if (!await Database.Authenticate())
                    {
                        // Log and exit when unable to authenticate
                        ServerConfiguration.LogError($"Unable to authenticate connection to Cache Server.");
                        return;
                    }
                    else
                    {
                        _lastSuccessfulDbAuth = PSMultiServer.Addons.Horizon.Server.Common.Utils.GetHighPrecisionUtcTime();

                        // pass to manager
                        await Manager.OnDatabaseAuthenticated();

                        #region Check Cache Server Simulated
                        if (Database._settings.SimulatedMode != true)
                        {
                            ServerConfiguration.LogInfo("Connected to Cache Server");
                        }
                        else
                        {
                            ServerConfiguration.LogInfo("Connected to Cache Server (Simulated)");
                        }
                        #endregion
                    }
                }

                // 
                if ((PSMultiServer.Addons.Horizon.Server.Common.Utils.GetHighPrecisionUtcTime() - _lastComponentLog).TotalSeconds > 15f)
                {
                    _lastComponentLog = PSMultiServer.Addons.Horizon.Server.Common.Utils.GetHighPrecisionUtcTime();
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }
        }

        public static async Task StartTickPooling()
        {
            int waitMs = sleepMS;

            ServerConfiguration.LogInfo($"[SVO] - Tick Pooling Started. . .");

            #region Timer
            // start timer
            _timer.SetPeriod(waitMs);
            _timer.Start();

            // iterate
            while (true)
            {
                // handle tick rate change
                if (sleepMS != waitMs)
                {
                    waitMs = sleepMS;
                    _timer.Stop();
                    _timer.SetPeriod(waitMs);
                    _timer.Start();
                }

                // tick
                await TickAsync();

                // wait for next tick
                _timer.WaitForTrigger();
            }

            #endregion
        }

        public static Middleware<Context> StaticSVORoot(string route, string userAgentdrm)
        {
            return async (ctx, next) =>
            {
                string userAgent = ctx.Request.Headers["User-Agent"];

                if (userAgent == null)
                {
                    userAgent = "Medius Client"; // Medius Client can hide userAgent.
                }
                else if (userAgent == "")
                {
                    await next();
                    return;
                }

                if (userAgentdrm != null && !userAgent.Contains(userAgentdrm))
                {
                    await next();
                    return;
                }

                if (ctx.Request.Url != null)
                {
                    // Don't use Request.RawUrl, because it contains url parameters. (e.g. '?a=1&b=2')
                    string relativePath = ctx.Request.Url.AbsolutePath;

                    bool handled = relativePath.StartsWith(route);
                    if (!handled)
                    {
                        await next();
                        return;
                    }

                    await processrequest(ctx.Request, ctx.Response);
                }
            };
        }

        public static async Task processrequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.Url.LocalPath == null)
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return;
            }
            string absolutepath = request.Url.AbsolutePath;

            switch (absolutepath)
            {
                case "/dataloaderweb/queue":
                    if (request.ContentType.StartsWith("multipart/form-data"))
                    {
                        switch (request.HttpMethod)
                        {
                            case "POST":
                                response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                                response.Headers.Set("Content-Language", "");

                                string boundary = Misc.ExtractBoundary(request.ContentType);

                                var data = MultipartFormDataParser.Parse(request.InputStream, boundary);

                                byte[] datatooutput = Encoding.UTF8.GetBytes(data.GetParameterValue("body"));

                                response.StatusCode = 200;
                                response.SendChunked = true;

                                if (response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        response.OutputStream.Write(datatooutput, 0, datatooutput.Length);
                                        response.OutputStream.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        // Not Important.
                                    }
                                }
                                break;
                            default:
                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                                break;
                        }
                        break;
                    }
                    break;
                default:
                    if (absolutepath.Contains("/HUBPS3_SVML"))
                        await Games.PlayStationHome.Home_SVO(request, response);
                    else
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
            }
        }
    }
}
