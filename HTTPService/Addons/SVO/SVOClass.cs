using System.Net;
using MultiServer.HTTPService.Addons.SVO.Games;

namespace MultiServer.HTTPService.Addons.SVO
{
    public class SVOClass
    {

        private static volatile bool _keepGoing = true;

        private static bool stopserver = false;

        public static bool svostarted = false;

        // Create and start the HttpListener
        private static HttpListener listener = new();
#nullable enable
        private static Task? _mainLoop;
#nullable disable

        public static Task SVOstart()
        {
            svostarted = true;

            stopserver = false;
            _keepGoing = true;
            if (_mainLoop != null && !_mainLoop.IsCompleted) return Task.CompletedTask; //Already started
            _mainLoop = loopserver();

            return Task.CompletedTask;
        }

        private async static Task loopserver()
        {
            listener.Prefixes.Add("http://*:10060/");

            ServerConfiguration.LogInfo($"SVO Server started - Listening for requests...");

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
                default:
                    if (absolutepath.Contains("/HUBPS3_SVML/"))
                        await Games.PlayStationHome.Home_SVO(ctx.Request, ctx.Response);
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
