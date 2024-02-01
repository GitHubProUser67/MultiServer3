using BackendProject.MiscUtils;
using CustomLogger;
using System.Net;
using System.Text;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;


namespace Horizon.HTTPSERVICE
{
    public class CrudServerHandler
    {
        public static bool IsStarted = false;
        private static WebserverLite? _Server;
        private string ip;
        private int port;

        public CrudServerHandler(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public void StartServer(string certpath = "")
        {
            if (_Server != null && _Server.IsListening)
                LoggerAccessor.LogWarn("CrudHandler Server already initiated");
            else
            {
                WebserverSettings settings = new()
                {
                    Hostname = ip,
                    Port = port,
                };

                if (!string.IsNullOrEmpty(certpath))
                {
                    settings.Ssl.PfxCertificateFile = certpath;
                    settings.Ssl.PfxCertificatePassword = "qwerty";
                    settings.Ssl.Enable = true;

                    _Server = new WebserverLite(settings, DefaultRoute);
                }
                else
                    _Server = new WebserverLite(settings, DefaultRoute);
                _Server.Events.Logger = LoggerAccessor.LogInfo;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/GetRooms/", async (HttpContextBase ctx) =>
                {
                    if (ctx.Request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    }
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                        ctx.Response.ContentType = "application/json";
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                        {
                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                            await ctx.Response.Send(HTTPUtils.Compress(Encoding.UTF8.GetBytes(CrudRoomManager.ToJson())));
                        }
                        else
                            await ctx.Response.Send(CrudRoomManager.ToJson());
                    }
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/favicon.ico", async (HttpContextBase ctx) =>
                {
                    if (ctx.Request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    }
                    else
                    {
                        if (File.Exists(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico"))
                        {
                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                            ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                            ctx.Response.ContentType = "image/x-icon";
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                await ctx.Response.Send(HTTPUtils.Compress(File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico")));
                            }
                            else
                                await ctx.Response.Send(File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico"));
                        }
                        else
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send();
                        }
                    }
                });

                _Server.Start();
                IsStarted = true;
                LoggerAccessor.LogInfo($"CrudHandler Server initiated on port:{port}...");
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
