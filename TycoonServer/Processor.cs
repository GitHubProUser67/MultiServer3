using CustomLogger;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;
using System.Web;

namespace TycoonServer
{
    public class Processor
    {
        public static bool IsStarted = false;
        private static WebserverLite? _Server;
        private int port;

        public Processor(int port)
        {
            this.port = port;
        }

        private static async Task AuthorizeConnection(HttpContextBase ctx)
        {
            if (TycoonServerConfiguration.BannedIPs != null && TycoonServerConfiguration.BannedIPs.Contains(ctx.Request.Source.IpAddress))
            {
                LoggerAccessor.LogError($"[SECURITY] - Client - {ctx.Request.Source.IpAddress} Requested the Tycoon server while being banned!");
                ctx.Response.StatusCode = 403;
                await ctx.Response.Send();
            }
        }

        public void StartServer()
        {
            if (_Server != null && _Server.IsListening)
                LoggerAccessor.LogWarn("Tycoon Server already initiated");
            else
            {
                _Server = new WebserverLite(new WebserverSettings()
                {
                    Hostname = "*",
                    Port = port,
                }, DefaultRoute);
                _Server.Routes.AuthenticateRequest = AuthorizeConnection;
                _Server.Events.Logger = LoggerAccessor.LogInfo;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.POST, "/HomeTycoon/Main_SCEE.php", async (HttpContextBase ctx) =>
                {
                    string? res = null;
                    if (ctx.Request.Url.RawWithQuery.ToLower().EndsWith(".php"))
                        res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType, null);
                    else
                        // Parse the query string from the URL
                        res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType,
                            HttpUtility.ParseQueryString(ctx.Request.Url.RawWithQuery)["PHPSESSID"]);
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
                    await ctx.Response.Send(res);
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.POST, "/HomeTycoon/Main_SCEJ.php", async (HttpContextBase ctx) =>
                {
                    string? res = null;
                    if (ctx.Request.Url.RawWithQuery.ToLower().EndsWith(".php"))
                        res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType, null);
                    else
                        // Parse the query string from the URL
                        res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType,
                            HttpUtility.ParseQueryString(ctx.Request.Url.RawWithQuery)["PHPSESSID"]);
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
                    await ctx.Response.Send(res);
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.POST, "/HomeTycoon/Main_SCEAsia.php", async (HttpContextBase ctx) =>
                {
                    string? res = null;
                    if (ctx.Request.Url.RawWithQuery.ToLower().EndsWith(".php"))
                        res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType, null);
                    else
                        // Parse the query string from the URL
                        res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType,
                            HttpUtility.ParseQueryString(ctx.Request.Url.RawWithQuery)["PHPSESSID"]);
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
                    await ctx.Response.Send(res);
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.POST, "/HomeTycoon/Main.php", async (HttpContextBase ctx) =>
                {
                    string? res = null;
                    if (ctx.Request.Url.RawWithQuery.ToLower().EndsWith(".php"))
                        res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType, null);
                    else
                        // Parse the query string from the URL
                        res = TycoonRequestProcessor.ProcessMainPHP(ctx.Request.DataAsBytes, ctx.Request.ContentType,
                            HttpUtility.ParseQueryString(ctx.Request.Url.RawWithQuery)["PHPSESSID"]);
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
                    await ctx.Response.Send(res);
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.POST, "/Postcards/", async (HttpContextBase ctx) =>
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
                    await ctx.Response.Send(res);
                });

                _Server.Start();
                IsStarted = true;
                LoggerAccessor.LogInfo("Tycoon Server initiated...");
            }
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 403;
            ctx.Response.ContentType = "text/plain";
            await ctx.Response.Send();
        }
    }
}
