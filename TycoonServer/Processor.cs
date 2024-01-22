using CustomLogger;
using HttpServerLite;
using System.Web;

namespace TycoonServer
{
    public class Processor
    {
        public static bool IsStarted = false;
        private static Webserver? _Server;
        private int port;

        public Processor(int port)
        {
            this.port = port;
        }

        private static bool AuthorizeConnection(string arg1, int arg2)
        {
            if (TycoonServerConfiguration.BannedIPs != null && TycoonServerConfiguration.BannedIPs.Contains(arg1))
                return false;

            return true;
        }

        public void StartServer()
        {
            if (_Server != null && _Server.IsListening)
                LoggerAccessor.LogWarn("Tycoon Server already initiated");
            else
            {
                _Server = new Webserver("*", port, false, null, null, DefaultRoute);
                _Server.Settings.Headers.Host = "http://*:" + port;
                _Server.Callbacks.AuthorizeConnection = AuthorizeConnection;
                _Server.Events.Logger = LoggerAccessor.LogInfo;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;
                _Server.Start();
                IsStarted = true;
                LoggerAccessor.LogInfo("Tycoon Server initiated...");
            }
        }

        private static Task DefaultRoute(HttpContext ctx)
        {
            ctx.Response.StatusCode = 403;
            ctx.Response.ContentType = "text/plain";
            ctx.Response.Send(true);

            return Task.CompletedTask;
        }

        [StaticRoute(HttpServerLite.HttpMethod.POST, "/HomeTycoon/Main_SCEE.php")]
        public static async Task SCEERoute(HttpContext ctx)
        {
            string? res = null;
            if (ctx.Request.Url.Full.ToLower().EndsWith(".php"))
                res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType, null);
            else
            {
                // Parse the query string from the URL
                res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType,
                    HttpUtility.ParseQueryString(ctx.Request.Url.Full)["PHPSESSID"]);
            }
            if (string.IsNullOrEmpty(res))
            {
                ctx.Response.ContentType = "text/plain";
                ctx.Response.StatusCode = 500;
            }
            else
            {
                ctx.Response.ContentType = "application/xml;charset=UTF-8";
                ctx.Response.StatusCode = 200;
            }
            await ctx.Response.SendAsync(res);
        }

        [StaticRoute(HttpServerLite.HttpMethod.POST, "/HomeTycoon/Main_SCEJ.php")]
        public static async Task SCEJRoute(HttpContext ctx)
        {
            string? res = null;
            if (ctx.Request.Url.Full.ToLower().EndsWith(".php"))
                res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType, null);
            else
            {
                // Parse the query string from the URL
                res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType,
                    HttpUtility.ParseQueryString(ctx.Request.Url.Full)["PHPSESSID"]);
            }
            if (string.IsNullOrEmpty(res))
            {
                ctx.Response.ContentType = "text/plain";
                ctx.Response.StatusCode = 500;
            }
            else
            {
                ctx.Response.ContentType = "application/xml;charset=UTF-8";
                ctx.Response.StatusCode = 200;
            }
            await ctx.Response.SendAsync(res);
        }

        [StaticRoute(HttpServerLite.HttpMethod.POST, "/HomeTycoon/Main_SCEAsia.php")]
        public static async Task SCEAsiaRoute(HttpContext ctx)
        {
            string? res = null;
            if (ctx.Request.Url.Full.ToLower().EndsWith(".php"))
                res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType, null);
            else
            {
                // Parse the query string from the URL
                res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType,
                    HttpUtility.ParseQueryString(ctx.Request.Url.Full)["PHPSESSID"]);
            }
            if (string.IsNullOrEmpty(res))
            {
                ctx.Response.ContentType = "text/plain";
                ctx.Response.StatusCode = 500;
            }
            else
            {
                ctx.Response.ContentType = "application/xml;charset=UTF-8";
                ctx.Response.StatusCode = 200;
            }
            await ctx.Response.SendAsync(res);
        }

        [StaticRoute(HttpServerLite.HttpMethod.POST, "/HomeTycoon/Main.php")]
        public static async Task HomelessRoute(HttpContext ctx)
        {
            string? res = null;
            if (ctx.Request.Url.Full.ToLower().EndsWith(".php"))
                res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType, null);
            else
            {
                // Parse the query string from the URL
                res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType,
                    HttpUtility.ParseQueryString(ctx.Request.Url.Full)["PHPSESSID"]);
            }
            if (string.IsNullOrEmpty(res))
            {
                ctx.Response.ContentType = "text/plain";
                ctx.Response.StatusCode = 500;
            }
            else
            {
                ctx.Response.ContentType = "application/xml;charset=UTF-8";
                ctx.Response.StatusCode = 200;
            }
            await ctx.Response.SendAsync(res);
        }

        [StaticRoute(HttpServerLite.HttpMethod.POST, "/Postcards/")]
        public static async Task Postcards(HttpContext ctx)
        {
            string? res = TycoonRequestProcessor.ProcessPostCards(ctx.Request.DataAsBytes, ctx.Request.ContentType);
            if (string.IsNullOrEmpty(res))
            {
                ctx.Response.ContentType = "text/plain";
                ctx.Response.StatusCode = 500;
            }
            else
            {
                ctx.Response.ContentType = "application/xml;charset=UTF-8";
                ctx.Response.StatusCode = 200;
            }
            await ctx.Response.SendAsync(res);
        }
    }
}
