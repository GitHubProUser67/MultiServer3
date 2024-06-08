using CyberBackendLibrary.HTTP;
using CustomLogger;
using System.Net;
using System.Text;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;
using System.Security.Authentication;

namespace Horizon.MUM
{
    public class MumServerHandler
    {
        private WebserverLite? _Server;
        private string ip;
        private int port;

        public MumServerHandler(string ip, int port, string certpath = "")
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
                settings.Ssl.PfxCertificatePassword = "qwerty";
                settings.Ssl.Enable = true;
            }

            _Server = new WebserverLite(settings, DefaultRoute);

            StartServer();
        }

        public void StopServer()
        {
            _Server?.Stop();
            _Server?.Dispose();

            LoggerAccessor.LogWarn($"MumHandler Server on port: {port} stopped...");
        }

        public void StartServer()
        {
            if (_Server != null && !_Server.IsListening)
            {
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

                        if (!string.IsNullOrEmpty(encoding))
                        {
                            if (encoding.Contains("zstd"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "zstd");
                                string? base64json = MumChannelHandler.JsonSerializeChannelsList();

                                if (!string.IsNullOrEmpty(base64json))
                                    await ctx.Response.Send(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(base64json)));
                                else
                                    await ctx.Response.Send();
                            }
                            else if (encoding.Contains("br"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "br");
                                string? base64json = MumChannelHandler.JsonSerializeChannelsList();

                                if (!string.IsNullOrEmpty(base64json))
                                    await ctx.Response.Send(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(base64json)));
                                else
                                    await ctx.Response.Send();
                            }
                            else if (encoding.Contains("gzip"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                string? base64json = MumChannelHandler.JsonSerializeChannelsList();

                                if (!string.IsNullOrEmpty(base64json))
                                    await ctx.Response.Send(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(base64json)));
                                else
                                    await ctx.Response.Send();
                            }
                            else if (encoding.Contains("deflate"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "deflate");
                                string? base64json = MumChannelHandler.JsonSerializeChannelsList();

                                if (!string.IsNullOrEmpty(base64json))
                                    await ctx.Response.Send(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(base64json)));
                                else
                                    await ctx.Response.Send();
                            }
                            else
                                await ctx.Response.Send(MumChannelHandler.JsonSerializeChannelsList());
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

                        if (!string.IsNullOrEmpty(encoding))
                        {
                            if (encoding.Contains("zstd"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "zstd");
                                string? base64xml = MumChannelHandler.XMLSerializeChannelsList();

                                if (!string.IsNullOrEmpty(base64xml))
                                    await ctx.Response.Send(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(base64xml)));
                                else
                                    await ctx.Response.Send();
                            }
                            else if (encoding.Contains("br"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "br");
                                string? base64xml = MumChannelHandler.XMLSerializeChannelsList();

                                if (!string.IsNullOrEmpty(base64xml))
                                    await ctx.Response.Send(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(base64xml)));
                                else
                                    await ctx.Response.Send();
                            }
                            else if (encoding.Contains("gzip"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                string? base64xml = MumChannelHandler.XMLSerializeChannelsList();

                                if (!string.IsNullOrEmpty(base64xml))
                                    await ctx.Response.Send(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(base64xml)));
                                else
                                    await ctx.Response.Send();
                            }
                            else if (encoding.Contains("deflate"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "deflate");
                                string? base64xml = MumChannelHandler.XMLSerializeChannelsList();

                                if (!string.IsNullOrEmpty(base64xml))
                                    await ctx.Response.Send(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(base64xml)));
                                else
                                    await ctx.Response.Send();
                            }
                            else
                                await ctx.Response.Send(MumChannelHandler.XMLSerializeChannelsList());
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
                        if (!string.IsNullOrEmpty(encoding))
                        {
                            if (encoding.Contains("zstd"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "zstd");
                                await ctx.Response.Send(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(MumChannelHandler.GetCRC32ChannelsList())));
                            }
                            else if (encoding.Contains("br"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "br");
                                await ctx.Response.Send(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(MumChannelHandler.GetCRC32ChannelsList())));
                            }
                            else if (encoding.Contains("gzip"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                await ctx.Response.Send(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(MumChannelHandler.GetCRC32ChannelsList())));
                            }
                            else if (encoding.Contains("deflate"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "deflate");
                                await ctx.Response.Send(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(MumChannelHandler.GetCRC32ChannelsList())));
                            }
                            else
                                await ctx.Response.Send(MumChannelHandler.GetCRC32ChannelsList());
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
