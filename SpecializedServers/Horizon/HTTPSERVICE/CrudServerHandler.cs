using CustomLogger;
using CyberBackendLibrary.HTTP;
using System.Net;
using System.Security.Authentication;
using System.Text;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;

namespace Horizon.HTTPSERVICE
{
    public class CrudServerHandler
    {
        private WebserverLite? _Server;
        private string ip;
        private int port;

        public CrudServerHandler(string ip, int port, string certpath = "", string certpass = "")
        {
            this.ip = ip;
            this.port = port;

            WebserverSettings settings = new()
            {
                Hostname = ip,
                Port = port,
            };

            if (!string.IsNullOrEmpty(certpath))
            {
                settings.Ssl.PfxCertificateFile = certpath;
                settings.Ssl.PfxCertificatePassword = certpass;
                settings.Ssl.Enable = true;
            }

            _Server = new WebserverLite(settings, DefaultRoute);

            StartServer();
        }

        public void StopServer()
        {
            _Server?.Stop();
            _Server?.Dispose();

            LoggerAccessor.LogWarn($"CrudHandler Server on port: {port} stopped...");
        }

        public void StartServer()
        {
            if (_Server != null && !_Server.IsListening)
            {
                _Server.Events.Logger = LoggerAccessor.LogInfo;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/GetRooms/", async (HttpContextBase ctx) =>
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
                        ctx.Response.ContentType = "application/json; charset=UTF-8";
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                        if (!string.IsNullOrEmpty(encoding))
                        {
                            if (encoding.Contains("zstd"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "zstd");
                                await ctx.Response.Send(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(CrudRoomManager.ToJson())));
                            }
                            else if (encoding.Contains("br"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "br");
                                await ctx.Response.Send(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(CrudRoomManager.ToJson())));
                            }
                            else if (encoding.Contains("gzip"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                await ctx.Response.Send(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(CrudRoomManager.ToJson())));
                            }
                            else if (encoding.Contains("deflate"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "deflate");
                                await ctx.Response.Send(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(CrudRoomManager.ToJson())));
                            }
                            else
                                await ctx.Response.Send(CrudRoomManager.ToJson());
                        }
                        else
                            await ctx.Response.Send(CrudRoomManager.ToJson());
                    }
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/GetCIDsList/", async (HttpContextBase ctx) =>
                {
                    if (!string.IsNullOrEmpty(ctx.Request.Useragent) && ctx.Request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    }
                    else
                    {
                        string clientip = ctx.Request.Source.IpAddress;
                        bool localhost = false;

                        if (!string.IsNullOrEmpty(clientip) && (clientip.Equals("127.0.0.1", StringComparison.InvariantCultureIgnoreCase) || clientip.Equals("localhost", StringComparison.InvariantCultureIgnoreCase)))
                            localhost = true;

                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.ContentType = localhost ? "application/json; charset=UTF-8" : "text/plain";
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                        if (!string.IsNullOrEmpty(encoding))
                        {
                            if (encoding.Contains("zstd"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "zstd");
                                await ctx.Response.Send(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(CrudCIDManager.ToJson(localhost ? false : true))));
                            }
                            else if (encoding.Contains("br"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "br");
                                await ctx.Response.Send(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(CrudCIDManager.ToJson(localhost ? false : true))));
                            }
                            else if (encoding.Contains("gzip"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                await ctx.Response.Send(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(CrudCIDManager.ToJson(localhost ? false : true))));
                            }
                            else if (encoding.Contains("deflate"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "deflate");
                                await ctx.Response.Send(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(CrudCIDManager.ToJson(localhost ? false : true))));
                            }
                            else
                                await ctx.Response.Send(CrudCIDManager.ToJson(localhost ? false : true));
                        }
                        else
                            await ctx.Response.Send(CrudCIDManager.ToJson(localhost ? false : true));
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
                            if (!string.IsNullOrEmpty(encoding))
                            {
                                if (encoding.Contains("zstd"))
                                {
                                    ctx.Response.Headers.Add("Content-Encoding", "zstd");
                                    await ctx.Response.Send(HTTPProcessor.CompressZstd(File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico")));
                                }
                                else if (encoding.Contains("br"))
                                {
                                    ctx.Response.Headers.Add("Content-Encoding", "br");
                                    await ctx.Response.Send(HTTPProcessor.CompressBrotli(File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico")));
                                }
                                else if (encoding.Contains("gzip"))
                                {
                                    ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                    await ctx.Response.Send(HTTPProcessor.CompressGzip(File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico")));
                                }
                                else if (encoding.Contains("deflate"))
                                {
                                    ctx.Response.Headers.Add("Content-Encoding", "deflate");
                                    await ctx.Response.Send(HTTPProcessor.Inflate(File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico")));
                                }
                                else
                                    await ctx.Response.Send(File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico"));
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

                _Server.Start(SslProtocols.Tls12 | SslProtocols.Tls13);

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
