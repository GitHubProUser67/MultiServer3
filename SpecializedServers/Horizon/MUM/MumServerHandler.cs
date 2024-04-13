using CyberBackendLibrary.HTTP;
using CustomLogger;
using System.Net;
using System.Text;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace Horizon.MUM
{
    public class MumServerHandler
    {
        public static bool IsStarted = false;
        private static WebserverLite? _Server;
        private string ip;
        private int port;

        public MumServerHandler(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public void StartServer(string certpath = "")
        {
            if (_Server != null && _Server.IsListening)
                LoggerAccessor.LogWarn("MumHandler Server already initiated");
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

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/GetChannelsJson/", async (HttpContextBase ctx) =>
                {
                    if (!string.IsNullOrEmpty(ctx.Request.Useragent) && ctx.Request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    }
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                        string? query = ctx.Request.Query.Querystring;
                        ctx.Response.ContentType = "text/xml; charset=UTF-8";

                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                        {
                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                            string? base64json = MumChannelHandler.JsonSerializeChannelsList();

                            if (!string.IsNullOrEmpty(base64json))
                                await ctx.Response.Send(HTTPProcessor.Compress(Encoding.UTF8.GetBytes(base64json)));
                            else
                                await ctx.Response.Send();
                        }
                        else
                            await ctx.Response.Send(MumChannelHandler.JsonSerializeChannelsList());
                    }
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/GetChannelsXML/", async (HttpContextBase ctx) =>
                {
                    if (!string.IsNullOrEmpty(ctx.Request.Useragent) && ctx.Request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    }
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.ContentType = "text/xml; charset=UTF-8";
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                        string? query = ctx.Request.Query.Querystring;

                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                        {
                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                            string? base64xml = MumChannelHandler.XMLSerializeChannelsList();

                            if (!string.IsNullOrEmpty(base64xml))
                                await ctx.Response.Send(HTTPProcessor.Compress(Encoding.UTF8.GetBytes(base64xml)));
                            else
                                await ctx.Response.Send();
                        }
                        else
                            await ctx.Response.Send(MumChannelHandler.XMLSerializeChannelsList());
                    }
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/GetChannelsCRC/", async (HttpContextBase ctx) =>
                {
                    if (!string.IsNullOrEmpty(ctx.Request.Useragent) && ctx.Request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    }
                    else
                    {
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.ContentType = "text/xml; charset=UTF-8";
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                        if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                        {
                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                            await ctx.Response.Send(HTTPProcessor.Compress(Encoding.UTF8.GetBytes(MumChannelHandler.GetCRC32ChannelsList())));
                        }
                        else
                            await ctx.Response.Send(MumChannelHandler.GetCRC32ChannelsList());
                    }
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/favicon.ico", async (HttpContextBase ctx) =>
                {
                    if (!string.IsNullOrEmpty(ctx.Request.Useragent) && ctx.Request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
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
                            ctx.Response.ContentType = "image/x-icon";
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                await ctx.Response.Send(HTTPProcessor.Compress(File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico")));
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
                LoggerAccessor.LogInfo($"MumHandler Server initiated on port:{port}...");
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
