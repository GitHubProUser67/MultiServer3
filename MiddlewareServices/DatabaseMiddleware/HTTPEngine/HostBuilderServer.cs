using BackendProject.MiscUtils;
using CustomLogger;
using DatabaseMiddleware.Controllers.Horizon;
using DatabaseMiddleware.Models;
using Horizon.LIBRARY.Database.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using WatsonWebserver.Core;
using WatsonWebserver.Lite;
using HttpMethod = WatsonWebserver.Core.HttpMethod;

namespace DatabaseMiddleware.HTTPEngine
{
    public partial class HostBuilderServer
    {
        public static bool IsStarted = false;
        private static WebserverLite? _Server;
        private readonly string ip;
        private readonly int port;

        public HostBuilderServer(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        private static async Task AuthorizeConnection(HttpContextBase ctx)
        {
            char[] charsToRemove = { '\"' };
            string? Token = ctx.Request.RetrieveHeaderValue("Authorization");

            if (DatabaseMiddlewareServerConfiguration.BannedIPs != null && DatabaseMiddlewareServerConfiguration.BannedIPs.Contains(ctx.Request.Source.IpAddress))
            {
                LoggerAccessor.LogError($"[SECURITY] - Client - {ctx.Request.Source.IpAddress}:{ctx.Request.Source.Port} Requested the Database server while being banned!");
                ctx.Response.StatusCode = 403;
                await ctx.Response.Send();
            }
            else if (!string.IsNullOrEmpty(Token))
            {
                MiddlewareUser? user = AuthenticationChannel.GetUserByToken(new string(WebCryptoUtils.Decrypt(Token, DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCryptoUtils.AuthIV)?.Where(c => !charsToRemove.Contains(c)).ToArray()));
                if (user != null)
                    ctx.Request.Headers.Add("MiddlewareRoles", JsonConvert.SerializeObject(user.Roles)); // I don't like this a lot, but it's still accetable.
            }
            else
            {
                ctx.Response.StatusCode = 511;
                await ctx.Response.Send();
            }
        }

        public void StartServer()
        {
            if (_Server != null && _Server.IsListening)
                LoggerAccessor.LogWarn("Database Server already initiated");
            else
            {
                _Server = new WatsonWebserver.Lite.Extensions.HostBuilderExtension.HostBuilder(ip, port, false, DefaultRoute)
                    .MapAuthenticationRoute(AuthorizeConnection)
                    .MapPreRoutingRoute(PreRoutingHandler)
                    .MapStaticRoute(HttpMethod.POST, "/Account/authenticate", async (ctx) =>
                    {
                        if (!string.IsNullOrEmpty(ctx.Request.Useragent) && ctx.Request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
                            ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        else
                        {
                            string? AuthToken = null;
                            MiddlewareUser? user = null;
                            AuthenticationRequest? req = await GetClassFromString<AuthenticationRequest>(ctx.Request.DataAsString);

                            if (req != null)
                            {
                                if (!string.IsNullOrEmpty(req.Password) && VariousUtils.IsBase64String(req.Password))
                                {
                                    char[] charsToRemove = { '\"' };
                                    req.Password = new string(WebCryptoUtils.Decrypt(req.Password, DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCryptoUtils.AuthIV)?.Where(c => !charsToRemove.Contains(c)).ToArray());
                                }

                                if (!string.IsNullOrEmpty(req.Password) && !string.IsNullOrEmpty(req.AccountName))
                                {
                                    user = AuthenticationChannel.GetUserByUsername(req.AccountName);
                                    if (user != null && user.Password == req.Password)
                                        AuthToken = AuthenticationChannel.GetTokenById(user.AccountId);
                                    else if (user == null)
                                    {
                                        Authentication auth = new(new MiddlewareUser(AuthenticationChannel.GetNextAvailableId(), req.AccountName, req.Password));
                                        AuthenticationChannel.AddAuthentificationData(auth);
                                        AuthToken = auth.Token;
                                        user = auth.User;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(AuthToken) && user != null)
                            {
                                ctx.Response.ChunkedTransfer = true;
                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                ctx.Response.ContentType = "application/json";
                                await ctx.Response.SendFinalChunk(WebCryptoUtils.EncryptToByteArray(new AuthenticationResponse()
                                { AccountId = user.AccountId, AccountName = user.AccountName, Token = AuthToken, Roles = user.Roles }
                                , DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCryptoUtils.AuthIV));
                                return;
                            }
                            else
                                ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        }

                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send();
                    })
                    .MapParameteRoute(HttpMethod.POST, "/api/{table}/{command}", async (ctx) =>
                    {
                        HttpRequestBase request = ctx.Request;
                        HttpResponseBase response = ctx.Response;

                        try
                        {
                            string? table = request.Url.Parameters["table"];
                            string? command = request.Url.Parameters["command"];
                            List<string>? Roles = JsonConvert.DeserializeObject<List<string>>(request.Headers["MiddlewareRoles"] ?? "[]");

                            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(command) || Roles == null)
                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                            else
                            {
                                object? Extractedclass = null;
                                string PostData = ctx.Request.DataAsString;

                                LoggerAccessor.LogInfo($"[POST_API] - Table:{table} was requested with Command:{command}");

                                switch (table)
                                {
                                    case "Keys":
                                        if (Roles.Contains("database"))
                                        {
                                            Keys? keys = new();
                                            switch (command)
                                            {
                                                case "setSettings":
                                                    if (request.QuerystringExists("appId"))
                                                    {
                                                        keys.setSettings(int.Parse(request.RetrieveQueryValue("appId")), await GetClassFromString<Dictionary<string, string>>(PostData) ?? new Dictionary<string, string> { });
                                                        Extractedclass = "[]";
                                                    }
                                                    break;
                                                default:
                                                    LoggerAccessor.LogWarn($"[POST_API] - Table:{table} - does not handle requested Command:{command} - Please report this on GITHUB if it's unexpected.");
                                                    break;
                                            }
                                            keys = null;
                                        }
                                        break;
                                    default:
                                        LoggerAccessor.LogWarn($"[POST_API] - Unknown Table was requested:{table} - Please report this on GITHUB if it's unexpected.");
                                        break;
                                }

                                if (Extractedclass != null)
                                {
#if DEBUG
                                    LoggerAccessor.LogInfo($"[POST_API] - Extracted Data -> {JsonConvert.SerializeObject(Extractedclass)}");
#endif
                                    response.ChunkedTransfer = true;
                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    response.ContentType = "application/json";
                                    await response.SendFinalChunk(WebCryptoUtils.EncryptNoPreserveToByteArray(Extractedclass
                                    , DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCryptoUtils.AuthIV));
                                    return;
                                }
                                else
                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            }
                        }
                        catch (Exception ex)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            LoggerAccessor.LogError($"[API_POST] - Thrown an exception:{ex}");
                        }

                        response.ContentType = "text/plain";
                        await response.Send();
                    }, true)
                    .MapParameteRoute(HttpMethod.GET, "/api/{table}/{command}", async (ctx) =>
                    {
                        HttpRequestBase request = ctx.Request;
                        HttpResponseBase response = ctx.Response;

                        try
                        {
                            string? table = request.Url.Parameters["table"];
                            string? command = request.Url.Parameters["command"];
                            List<string>? Roles = JsonConvert.DeserializeObject<List<string>>(request.Headers["MiddlewareRoles"] ?? "[]");

                            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(command) || Roles == null)
                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                            else
                            {
                                object? Extractedclass = null;

                                LoggerAccessor.LogInfo($"[GET_API] - Table:{table} was requested with Command:{command}");

                                switch (table)
                                {
                                    case "Keys":
                                        if (Roles.Contains("database"))
                                        {
                                            Keys? keys = new();
                                            switch (command)
                                            {
                                                case "getAppIds":
                                                case "isAppIdCompatible":
                                                    Extractedclass = await keys.getAppIds();
                                                    break;
                                                case "getSettings":
                                                    if (request.QuerystringExists("appId"))
                                                        Extractedclass = await keys.getSettings(int.Parse(request.RetrieveQueryValue("appId")));
                                                    break;
                                                case "getAnnouncements":
                                                    if (request.QuerystringExists("fromDt") && request.QuerystringExists("AppId"))
                                                        Extractedclass = await keys.getAnnouncements(null, null, DateTime.Parse(request.RetrieveQueryValue("fromDt")), null, int.Parse(request.RetrieveQueryValue("appId")));
                                                    break;
                                                case "getAnnouncementsList":
                                                    if (request.QuerystringExists("Dt") && request.QuerystringExists("TakeSize") && request.QuerystringExists("AppId"))
                                                        Extractedclass = await keys.getAnnouncementsList(int.Parse(request.RetrieveQueryValue("appId")), DateTime.Parse(request.RetrieveQueryValue("Dt")), int.Parse(request.RetrieveQueryValue("TakeSize")));
                                                    break;
                                                case "getEULA":
                                                    if (request.QuerystringExists("policyType") && request.QuerystringExists("appId") && request.QuerystringExists("fromDt"))
                                                        Extractedclass = await keys.getEULA(int.Parse(request.RetrieveQueryValue("policyType")), int.Parse(request.RetrieveQueryValue("appId")), null, DateTime.Parse(request.RetrieveQueryValue("fromDt")), null);
                                                    break;
                                                case "getLocations":
                                                    if (request.QuerystringExists("LocationId") && request.QuerystringExists("AppId") && request.QuerystringExists("fromDt"))
                                                        Extractedclass = await keys.getLocations(int.Parse(request.RetrieveQueryValue("AppId")));
                                                    break;
                                                case "getServerFlags":
                                                    Extractedclass = await keys.getServerFlags();
                                                    break;
                                                default:
                                                    LoggerAccessor.LogWarn($"[GET_API] - Table:{table} - does not handle requested Command:{command} - Please report this on GITHUB if it's unexpected.");
                                                    break;
                                            }
                                            keys = null;
                                        }
                                        break;
                                    default:
                                        LoggerAccessor.LogWarn($"[GET_API] - Unknown Table was requested:{table} - Please report this on GITHUB if it's unexpected.");
                                        break;
                                }

                                if (Extractedclass != null)
                                {
#if DEBUG
                                    LoggerAccessor.LogInfo($"[GET_API] - Extracted Data -> {JsonConvert.SerializeObject(Extractedclass)}");
#endif

                                    response.ChunkedTransfer = true;
                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    response.ContentType = "application/json";
                                    await response.SendFinalChunk(WebCryptoUtils.EncryptNoPreserveToByteArray(Extractedclass
                                    , DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCryptoUtils.AuthIV));
                                    return;
                                }
                                else
                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            }

                        }
                        catch (Exception ex)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            LoggerAccessor.LogError($"[API_GET] - Thrown an exception:{ex}");
                        }

                        response.ContentType = "text/plain";
                        await response.Send();
                    }, true)
                    .MapStaticRoute(HttpMethod.GET, "/favicon.ico", async (ctx) =>
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
                    })
                    .Build();

                _Server.Events.Logger = LoggerAccessor.LogInfo;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;

                _Server.Start();
                IsStarted = true;
                LoggerAccessor.LogInfo($"Database Server initiated on port: {port}...");
            }
        }

        private static Task PreRoutingHandler(HttpContextBase ctx)
        {
            string clientip = ctx.Request.Source.IpAddress;
            string clientport = ctx.Request.Source.Port.ToString();
            string SuplementalMessage = string.Empty;
            string? GeoCodeString = GeoIPUtils.GetGeoCodeFromIP(IPAddress.Parse(clientip));
            string fullurl = HTTPUtils.DecodeUrl(ctx.Request.Url.RawWithQuery);

            if (!string.IsNullOrEmpty(GeoCodeString))
            {
                // Split the input string by the '-' character
                string[] parts = GeoCodeString.Split('-');

                // Check if there are exactly two parts
                if (parts.Length == 2)
                    SuplementalMessage = " Located at " + parts[0] + (bool.Parse(parts[1]) ? " Situated in Europe " : string.Empty);
            }

            LoggerAccessor.LogInfo($"[Database] - {clientip}:{clientport}{SuplementalMessage} Requested the Database Server with URL : {fullurl} ({ctx.Timestamp.TotalMs}ms)");

            return Task.CompletedTask;
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 403;
            ctx.Response.ContentType = "text/plain";
            await ctx.Response.Send();
        }

        private static Task<T?> GetClassFromString<T>(string data)
        {
            T? result = default;

            try
            {
                result = JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception)
            {
                // Not Important.
            }

            return Task.FromResult(result);
        }
    }
}
