using CustomLogger;
using NetworkLibrary.GeoLocalization;
using NetworkLibrary.HTTP;
using Horizon.DME.Extension.PlayStationHome;
using Horizon.SERVER;
using Horizon.SERVER.Extension.PlayStationHome;
using System.Net;
using System.Text;
using WatsonWebserver;
using WatsonWebserver.Core;
using Horizon.MUM.Models;

namespace Horizon.HTTPSERVICE
{
    public class CrudServerHandler
    {
        private Webserver? _Server;
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

            _Server = new Webserver(settings, DefaultRoute);

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
                _Server.Events.ExceptionEncountered += ExceptionEncountered;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/GetRooms/", async (HttpContextBase ctx) =>
                {
                    string userAgent = ctx.Request.Useragent;

                    if (!string.IsNullOrEmpty(userAgent) && userAgent.Contains("bytespider", StringComparison.InvariantCultureIgnoreCase)) // Get Away TikTok.
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    }
                    else
                    {
                        bool IsAdmin = false;

                        string clientip = ctx.Request.Source.IpAddress;

                        if (!string.IsNullOrEmpty(clientip) && (clientip.Equals("127.0.0.1", StringComparison.InvariantCultureIgnoreCase)
                        || clientip.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) || MediusClass.Settings.PlaystationHomeUsersServersAccessList.Any(entry => entry.Key.Contains($":{clientip}") && entry.Value.Equals("ADMIN"))))
                            IsAdmin = true;

                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.ContentType = "application/json; charset=UTF-8";
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                        if (!string.IsNullOrEmpty(encoding))
                        {
                            if (encoding.Contains("zstd"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "zstd");
                                await ctx.Response.Send(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(RoomManager.ToJson())));
                            }
                            else if (encoding.Contains("br"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "br");
                                await ctx.Response.Send(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(RoomManager.ToJson())));
                            }
                            else if (encoding.Contains("gzip"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                await ctx.Response.Send(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(RoomManager.ToJson())));
                            }
                            else if (encoding.Contains("deflate"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "deflate");
                                await ctx.Response.Send(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(RoomManager.ToJson())));
                            }
                            else
                                await ctx.Response.Send(RoomManager.ToJson());
                        }
                        else
                            await ctx.Response.Send(RoomManager.ToJson());
                    }
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/GetCIDsList/", async (HttpContextBase ctx) =>
                {
                    string userAgent = ctx.Request.Useragent;

                    if (!string.IsNullOrEmpty(userAgent) && userAgent.Contains("bytespider", StringComparison.InvariantCultureIgnoreCase)) // Get Away TikTok.
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
                                await ctx.Response.Send(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(CIDManager.ToJson(!localhost))));
                            }
                            else if (encoding.Contains("br"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "br");
                                await ctx.Response.Send(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(CIDManager.ToJson(!localhost))));
                            }
                            else if (encoding.Contains("gzip"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                await ctx.Response.Send(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(CIDManager.ToJson(!localhost))));
                            }
                            else if (encoding.Contains("deflate"))
                            {
                                ctx.Response.Headers.Add("Content-Encoding", "deflate");
                                await ctx.Response.Send(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(CIDManager.ToJson(!localhost))));
                            }
                            else
                                await ctx.Response.Send(CIDManager.ToJson(!localhost));
                        }
                        else
                            await ctx.Response.Send(CIDManager.ToJson(!localhost));
                    }
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/HomeIGA/{command}/", async (HttpContextBase ctx) =>
                {
                    string? Command = ctx.Request.Url.Parameters["command"];
                    string userAgent = ctx.Request.Useragent;

                    if (!string.IsNullOrEmpty(userAgent) && userAgent.Contains("bytespider", StringComparison.InvariantCultureIgnoreCase)) // Get Away TikTok.
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    }
                    else
                    {
                        string clientip = ctx.Request.Source.IpAddress;

                        if (!string.IsNullOrEmpty(clientip) && (clientip.Equals("127.0.0.1", StringComparison.InvariantCultureIgnoreCase)
                        || clientip.Equals("localhost", StringComparison.InvariantCultureIgnoreCase) || MediusClass.Settings.PlaystationHomeUsersServersAccessList.Any(entry => entry.Key.Contains($":{clientip}") && entry.Value.Equals("ADMIN"))))
                        {
                            if (!string.IsNullOrEmpty(Command) && ctx.Request.QuerystringExists("DmeId") && short.TryParse(ctx.Request.RetrieveQueryValue("DmeId"), out short DmeId)
                             && ctx.Request.QuerystringExists("WorldId") && int.TryParse(ctx.Request.RetrieveQueryValue("WorldId"), out int WorldId)
                             && ctx.Request.QuerystringExists("DmeWorldId") && int.TryParse(ctx.Request.RetrieveQueryValue("DmeWorldId"), out int DmeWorldId))
                            {
                                bool Retail = true;
                                string result = "Command Unknown!";

                                if (ctx.Request.QuerystringExists("Retail") && bool.TryParse(ctx.Request.RetrieveQueryValue("Retail"), out Retail))
                                {
                                    
                                }

                                LoggerAccessor.LogWarn($"[CrudServerHandler] - client:{clientip}:{ctx.Request.Source.Port} issued command: {Command}");

                                switch (Command)
                                {
                                    case "Kick":
                                        result = NewIGA.KickClient(DmeId, WorldId, DmeWorldId, Retail);
                                        break;
                                    default:
                                        LoggerAccessor.LogWarn($"[CrudServerHandler] - Unknown Home IGA command: {Command}");
                                        break;
                                }

                                ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                ctx.Response.ContentType = "text/plain";
                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                                if (!string.IsNullOrEmpty(encoding))
                                {
                                    if (encoding.Contains("zstd"))
                                    {
                                        ctx.Response.Headers.Add("Content-Encoding", "zstd");
                                        await ctx.Response.Send(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(result)));
                                    }
                                    else if (encoding.Contains("br"))
                                    {
                                        ctx.Response.Headers.Add("Content-Encoding", "br");
                                        await ctx.Response.Send(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(result)));
                                    }
                                    else if (encoding.Contains("gzip"))
                                    {
                                        ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                        await ctx.Response.Send(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(result)));
                                    }
                                    else if (encoding.Contains("deflate"))
                                    {
                                        ctx.Response.Headers.Add("Content-Encoding", "deflate");
                                        await ctx.Response.Send(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(result)));
                                    }
                                    else
                                        await ctx.Response.Send(result);
                                }
                                else
                                    await ctx.Response.Send(result);
                            }
                            else
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                ctx.Response.ContentType = "text/plain";
                                await ctx.Response.Send();
                            }
                        }
                        else
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = "text/html";
                            await ctx.Response.Send("<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n" +
                                "    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
                                "    <title>DARK WEB</title>\r\n    <style>\r\n        body {\r\n            margin: 0;\r\n            padding: 0;\r\n" +
                                "            display: flex;\r\n            justify-content: center;\r\n            align-items: center;\r\n" +
                                "            height: 100vh;\r\n            background-image: url('https://media1.tenor.com/m/IKo-c45o9XUAAAAC/horror-gif.gif'); /* Example scary background */\r\n" +
                                "            background-size: cover;\r\n            background-position: center;\r\n        }\r\n\r\n        h1 {\r\n" +
                                "            color: red;\r\n            font-size: 100px;\r\n            font-family: 'Creepster', cursive; /* You can link to a scary font if you want */\r\n" +
                                "            text-shadow: 4px 4px 8px black;\r\n        }\r\n    </style>\r\n</head>\r\n<body>\r\n" +
                                "    <iframe width=\"0\" height=\"0\" src=\"https://www.youtube.com/embed/XfQrgDbisAo?autoplay=1&loop=1\"\r\n    frameborder=\"0\" allowfullscreen></iframe>" +
                                $"    <h1>BEWARE! {$"We know your IP {clientip} and where you live {GeoIP.GetGeoCodeFromIP(IPAddress.Parse(clientip)) ?? "Earth"}"}</h1>\r\n</body>\r\n" +
                                "</html>");
                        }
                    }
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/HomeRTM/{command}/", async (HttpContextBase ctx) =>
                {
                    string? Command = ctx.Request.Url.Parameters["command"];
                    string userAgent = ctx.Request.Useragent;

                    if (!string.IsNullOrEmpty(userAgent) && userAgent.Contains("bytespider", StringComparison.InvariantCultureIgnoreCase)) // Get Away TikTok.
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(Command))
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send();
                            return;
                        }
                        else
                            Command = HTTPProcessor.DecodeUrl(Command);

                        bool Retail = true;
                        string? AccessToken = null;

                        if (ctx.Request.QuerystringExists("Retail") && bool.TryParse(ctx.Request.RetrieveQueryValue("Retail"), out Retail))
                        {

                        }

                        if (ctx.Request.QuerystringExists("AccessToken"))
                            AccessToken = HTTPProcessor.DecodeUrl(ctx.Request.RetrieveQueryValue("AccessToken"));

                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send(await HomeRTMTools.SendRemoteCommand(ctx.Request.Source.IpAddress, AccessToken, Command, Retail) ? "Requested Command sent successfully!" : "Error while sending the Requested Command!");
                    }
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/HomeSSFW/{command}/", async (HttpContextBase ctx) =>
                {
                    string? Command = ctx.Request.Url.Parameters["command"];
                    string userAgent = ctx.Request.Useragent;

                    if (!string.IsNullOrEmpty(userAgent) && userAgent.Contains("bytespider", StringComparison.InvariantCultureIgnoreCase)) // Get Away TikTok.
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(Command))
                        {
                            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send();
                            return;
                        }
                        else
                            Command = HTTPProcessor.DecodeUrl(Command);

                        bool Retail = true;
                        string? AccessToken = null;

                        if (ctx.Request.QuerystringExists("Retail") && bool.TryParse(ctx.Request.RetrieveQueryValue("Retail"), out Retail))
                        {

                        }

                        if (ctx.Request.QuerystringExists("AccessToken"))
                            AccessToken = HTTPProcessor.DecodeUrl(ctx.Request.RetrieveQueryValue("AccessToken"));

                        switch (Command)
                        {
                            case "GetUserIds":
                                bool AccessTokenProvided = !string.IsNullOrEmpty(AccessToken);
                                StringBuilder sb = new("[");
                                List<string> userIds = new();
                                List<ClientObject>? clients = null;

                                if (AccessTokenProvided)
                                {
                                    ClientObject? client = MediusClass.Manager.GetClientByAccessToken(AccessToken, Retail ? 20374 : 20371);
                                    if (client != null)
                                    {
                                        clients = new()
                                        {
                                            client
                                        };
                                    }
                                }
                                else
                                    clients = MediusClass.Manager.GetClientsByIp(ctx.Request.Source.IpAddress, Retail ? 20374 : 20371);

                                if (clients != null)
                                {
                                    foreach (ClientObject client in clients)
                                    {
                                        string? userId = client.SSFWid;

                                        if (!string.IsNullOrEmpty(userId) && !userIds.Contains(userId))
                                            userIds.Add(userId);
                                    }
                                }

                                foreach (string userId in userIds)
                                {
                                    if (sb.Length > 1)
                                        sb.Append($",\"{userId}\"");
                                    else
                                        sb.Append($"\"{userId}\"");
                                }

                                sb.Append(']');

                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                ctx.Response.ContentType = "application/json; charset=utf-8";
                                await ctx.Response.Send(sb.ToString());
                                break;
                            default:
                                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                ctx.Response.ContentType = "text/plain";
                                await ctx.Response.Send();
                                return;
                        }
                    }
                });

                _Server.Routes.PostAuthentication.Parameter.Add(WatsonWebserver.Core.HttpMethod.GET, "/favicon.ico", async (HttpContextBase ctx) =>
                {
                    string userAgent = ctx.Request.Useragent;

                    if (!string.IsNullOrEmpty(userAgent) && userAgent.Contains("bytespider", StringComparison.InvariantCultureIgnoreCase)) // Get Away TikTok.
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

                _Server.Start();

                LoggerAccessor.LogInfo($"CrudHandler Server initiated on port:{port}...");
            }
        }

        private void ExceptionEncountered(object? sender, ExceptionEventArgs args)
        {
            LoggerAccessor.LogError(args.Exception);
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 403;
            ctx.Response.ContentType = "text/plain";
            await ctx.Response.Send();
        }
    }
}
