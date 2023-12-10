// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using CryptoSporidium;
using CustomLogger;
using HttpServerLite;
using System.Net;
using System.Text;

namespace Horizon.HTTPSERVICE
{
    public class CrudServerHandler
    {
        public static bool IsStarted = false;
        private static Webserver? _Server;
        private string ip;
        private int port;

        public CrudServerHandler(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public void StartServer(string certpath = "", string certpass = "")
        {
            if (_Server != null && _Server.IsListening)
                LoggerAccessor.LogWarn("CrudHandler Server already initiated");
            else
            {
                if (!string.IsNullOrEmpty(certpath) && !string.IsNullOrEmpty(certpass))
                {
                    _Server = new Webserver(ip, port, true, certpath, certpass, DefaultRoute);
                    _Server.Settings.Headers.Host = "https://" + ip + ":" + port;
                }
                else
                {
                    _Server = new Webserver(ip, port, false, null, null, DefaultRoute);
                    _Server.Settings.Headers.Host = "http://" + ip + ":" + port;
                }
                _Server.Events.Logger = LoggerAccessor.LogInfo;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;
                _Server.Start();
                IsStarted = true;
                LoggerAccessor.LogInfo("CrudHandler Server initiated...");
            }
        }

        private static Task DefaultRoute(HttpContext ctx)
        {
            ctx.Response.StatusCode = 403;
            ctx.Response.ContentType = "text/plain";
            ctx.Response.Send(true);

            return Task.CompletedTask;
        }

        [StaticRoute(HttpServerLite.HttpMethod.GET, "/GetRooms/")]
        public static async Task CrudJsonRoute(HttpContext ctx)
        {
            if (ctx.Request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                ctx.Response.ContentType = "text/plain";
                ctx.Response.Send(true);
            }
            else
            {
                if (ctx.Request.Method.ToString() == "OPTIONS")
                {
                    ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                    ctx.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, HEAD");
                    ctx.Response.Headers.Add("Access-Control-Max-Age", "1728000");
                }

                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                {
                    ctx.Response.Headers.Add("Content-Encoding", "gzip");
                    await ctx.Response.SendAsync(HTTPUtils.Compress(Encoding.UTF8.GetBytes(CrudRoomManager.ToJson())));
                }
                else
                    await ctx.Response.SendAsync(CrudRoomManager.ToJson());
            }
        }

        [StaticRoute(HttpServerLite.HttpMethod.GET, "/favicon.ico")]
        public static async Task FaviconRoute(HttpContext ctx)
        {
            if (ctx.Request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                ctx.Response.ContentType = "text/plain";
                ctx.Response.Send(true);
            }
            else
            {
                if (ctx.Request.Method.ToString() == "OPTIONS")
                {
                    ctx.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                    ctx.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, HEAD");
                    ctx.Response.Headers.Add("Access-Control-Max-Age", "1728000");
                }

                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                if (File.Exists(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico"))
                {
                    ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                    ctx.Response.ContentType = "image/x-icon";
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                    string? encoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");
                    if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                    {
                        ctx.Response.Headers.Add("Content-Encoding", "gzip");
                        await ctx.Response.SendAsync(HTTPUtils.Compress(File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico")));
                    }
                    else
                        await ctx.Response.SendAsync(File.ReadAllBytes(Directory.GetCurrentDirectory() + "/static/wwwroot/favicon.ico"));
                }
                else
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    ctx.Response.ContentType = "text/plain";
                    ctx.Response.Send(true);
                }
            }
        }
    }
}
