using CyberBackendLibrary.HTTP;
using CyberBackendLibrary.GeoLocalization;
using CyberBackendLibrary.Crypto;
using CyberBackendLibrary.DataTypes;
using CustomLogger;
using DatabaseMiddleware.Controllers.HorizonDatabase;
using DatabaseMiddleware.Models;
using Horizon.LIBRARY.Database.Models;
using Newtonsoft.Json;
using System.Net;
using WatsonWebserver.Core;
using HttpMethod = WatsonWebserver.Core.HttpMethod;
using WatsonWebserver;

namespace DatabaseMiddleware.HTTPEngine
{
    public partial class HostBuilderServer
    {
        private static Webserver? _Server;
        private readonly string ip;
        private readonly int port;

        public HostBuilderServer(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

            StartServer();
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
                MiddlewareUser? user = AuthenticationChannel.GetUserByToken(new string(WebCrypto.Decrypt(Token, DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCrypto.AuthIV)?.Where(c => !charsToRemove.Contains(c)).ToArray()));
                if (user != null)
                    ctx.Request.Headers.Add("MiddlewareRoles", JsonConvert.SerializeObject(user.Roles)); // I don't like this a lot, but it's still accetable.
            }
            else
            {
                ctx.Response.StatusCode = 511;
                await ctx.Response.Send();
            }
        }

        public void StopServer()
        {
            _Server?.Stop();
            _Server?.Dispose();

            LoggerAccessor.LogWarn($"Database Server on port: {port} stopped...");
        }

        public void StartServer()
        {
            if (_Server != null && _Server.IsListening)
                LoggerAccessor.LogWarn($"Database Server already initiated on port: {port}");
            else
            {
                _Server = new WatsonWebserver.Extensions.HostBuilderExtension.HostBuilder(ip, port, false, DefaultRoute)
                    .MapAuthenticationRoute(AuthorizeConnection)
                    .MapPreRoutingRoute(PreRoutingHandler)
                    .MapParameterRoute(HttpMethod.POST, "/Account/{command}", async (ctx) =>
                    {
                        HttpRequestBase request = ctx.Request;
                        HttpResponseBase response = ctx.Response;

                        try
                        {
                            string? command = request.Url.Parameters["command"];

                            if (command == "authenticate")
                            {
                                if (!string.IsNullOrEmpty(request.Useragent) && request.Useragent.ToLower().Contains("bytespider")) // Get Away TikTok.
                                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                                else
                                {
                                    string? AuthToken = null;
                                    MiddlewareUser? user = null;
                                    AuthenticationRequest? req = await GetClassFromString<AuthenticationRequest>(request.DataAsString);

                                    if (req != null)
                                    {
                                        if (!string.IsNullOrEmpty(req.Password) && DataTypesUtils.IsBase64String(req.Password))
                                        {
                                            char[] charsToRemove = { '\"' };
                                            req.Password = new string(WebCrypto.Decrypt(req.Password, DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCrypto.AuthIV)?.Where(c => !charsToRemove.Contains(c)).ToArray());
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
                                        response.ChunkedTransfer = true;
                                        response.StatusCode = (int)HttpStatusCode.OK;
                                        response.ContentType = "application/json";
                                        await response.SendFinalChunk(WebCrypto.EncryptToByteArray(new AuthenticationResponse()
                                        { AccountId = user.AccountId, AccountName = user.AccountName, Token = AuthToken, Roles = user.Roles }
                                        , DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCrypto.AuthIV));
                                        return;
                                    }
                                    else
                                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                                }
                            }
                            else
                            {
                                await AuthorizeConnection(ctx);

                                if (!response.ResponseSent) // Allowed if no response sent already.
                                {
                                    List<string>? Roles = JsonConvert.DeserializeObject<List<string>>(request.Headers["MiddlewareRoles"] ?? "[]");

                                    if (string.IsNullOrEmpty(command) || Roles == null)
                                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                                    else
                                    {
                                        object? Extractedclass = null;
                                        string PostData = ctx.Request.DataAsString;

                                        LoggerAccessor.LogInfo($"[ACCOUNT_API] - Account was requested with Command:{command}");

                                        if (Roles.Contains("database"))
                                        {
                                            switch (command)
                                            {
                                                default:
                                                    LoggerAccessor.LogWarn($"[ACCOUNT_API] - Account - does not handle requested Command:{command} - Please report this on GITHUB if it's unexpected.");
                                                    break;
                                            }
                                        }

                                        if (Extractedclass != null)
                                        {
#if DEBUG
                                            LoggerAccessor.LogInfo($"[ACCOUNT_API] - Extracted Data -> {JsonConvert.SerializeObject(Extractedclass)}");
#endif
                                            response.ChunkedTransfer = true;
                                            response.StatusCode = (int)HttpStatusCode.OK;
                                            response.ContentType = "application/json";
                                            await response.SendFinalChunk(WebCrypto.EncryptNoPreserveToByteArray(Extractedclass
                                            , DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCrypto.AuthIV));
                                            return;
                                        }
                                        else
                                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                    }
                                }
                                else
                                    return;
                            }
                        }
                        catch (Exception ex)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            LoggerAccessor.LogError($"[ACCOUNT_POST] - Thrown an exception:{ex}");
                        }

                        response.ContentType = "text/plain";
                        await response.Send();
                    })
                    .MapParameterRoute(HttpMethod.GET, "/MultiSpy/{command}", async (ctx) =>
                    {
                        HttpRequestBase request = ctx.Request;
                        HttpResponseBase response = ctx.Response;

                        try
                        {
                            string? command = request.Url.Parameters["command"];
                            List<string>? Roles = JsonConvert.DeserializeObject<List<string>>(request.Headers["MiddlewareRoles"] ?? "[]");

                            if (string.IsNullOrEmpty(command) || Roles == null)
                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                            else
                            {
                                object? Extractedclass = null;

                                LoggerAccessor.LogInfo($"[MULTISPY_GET] - Service was requested with Command:{command}");

                                if (Roles.Contains("database"))
                                {
                                    switch (command)
                                    {
                                        /*case "UserExists":
                                            if (request.QuerystringExists("username"))
                                                Extractedclass = LoginDatabase.Instance.UserExists(request.RetrieveQueryValue("username"));
                                            break;
                                        case "CreateUser":
                                            if (request.QuerystringExists("username") && request.QuerystringExists("passwordEncrypted") && request.QuerystringExists("email") && request.QuerystringExists("country") && request.QuerystringExists("address"))
                                            {
                                                LoginDatabase.Instance.CreateUser(request.RetrieveQueryValue("username"), request.RetrieveQueryValue("passwordEncrypted"),
                                                    request.RetrieveQueryValue("email"), request.RetrieveQueryValue("country"), IPAddress.Parse(request.RetrieveQueryValue("address")));
                                                Extractedclass = "OK";
                                            }
                                            break;
                                        case "LogLogin":
                                            if (request.QuerystringExists("name") && request.QuerystringExists("address"))
                                            {
                                                LoginDatabase.Instance.LogLogin(request.RetrieveQueryValue("name"), IPAddress.Parse(request.RetrieveQueryValue("address")));
                                                Extractedclass = "OK";
                                            }
                                            break;
                                        case "SetData":
                                            if (request.QuerystringExists("name") && request.QuerystringExists("data"))
                                            {
                                                string data = request.RetrieveQueryValue("data");
                                                if (DataTypesUtils.IsBase64String(data))
                                                {
                                                    LoginDatabase.Instance.SetData(request.RetrieveQueryValue("name"), JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(Convert.FromBase64String(data))));
                                                    Extractedclass = "OK";
                                                }
                                            }
                                            break;
                                        case "GetData":
                                            if (request.QuerystringExists("username"))
                                                Extractedclass = LoginDatabase.Instance.GetData(request.RetrieveQueryValue("username"));
                                            else if (request.QuerystringExists("email") && request.QuerystringExists("passwordEncrypted"))
                                                Extractedclass = LoginDatabase.Instance.GetData(request.RetrieveQueryValue("email"), request.RetrieveQueryValue("passwordEncrypted"));
                                            break;*/
                                        default:
                                            LoggerAccessor.LogWarn($"[MULTISPY_GET] - Service - does not handle requested Command:{command} - Please report this on GITHUB if it's unexpected.");
                                            break;
                                    }
                                }

                                if (Extractedclass != null)
                                {
#if DEBUG
                                    LoggerAccessor.LogInfo($"[MULTISPY_GET] - Extracted Data -> {JsonConvert.SerializeObject(Extractedclass)}");
#endif

                                    response.ChunkedTransfer = true;
                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    response.ContentType = "application/json";
                                    await response.SendFinalChunk(WebCrypto.EncryptNoPreserveToByteArray(Extractedclass
                                    , DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCrypto.AuthIV));
                                    return;
                                }
                                else
                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            }

                        }
                        catch (Exception ex)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            LoggerAccessor.LogError($"[MULTISPY_GET] - Thrown an exception:{ex}");
                        }

                        response.ContentType = "text/plain";
                        await response.Send();
                    }, null, true)
                    .MapParameterRoute(HttpMethod.POST, "/api/{table}/{command}", async (ctx) =>
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

                                LoggerAccessor.LogInfo($"[API_POST] - Table:{table} was requested with Command:{command}");

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
                                                    LoggerAccessor.LogWarn($"[API_POST] - Table:{table} - does not handle requested Command:{command} - Please report this on GITHUB if it's unexpected.");
                                                    break;
                                            }
                                            keys = null;
                                        }
                                        break;
                                    case "Logs":
                                        if (Roles.Contains("database"))
                                        {
                                            Logs? logs = new();
                                            switch (command)
                                            {
                                                case "submitLog":
                                                    Extractedclass = await logs.submitLog(await GetClassFromString<LogDTO>(PostData) ?? new LogDTO());
                                                    break;
                                                default:
                                                    LoggerAccessor.LogWarn($"[API_POST] - Table:{table} - does not handle requested Command:{command} - Please report this on GITHUB if it's unexpected.");
                                                    break;
                                            }
                                            logs = null;
                                        }
                                        break;
                                    default:
                                        LoggerAccessor.LogWarn($"[API_POST] - Unknown Table was requested:{table} - Please report this on GITHUB if it's unexpected.");
                                        break;
                                }

                                if (Extractedclass != null)
                                {
#if DEBUG
                                    LoggerAccessor.LogInfo($"[API_POST] - Extracted Data -> {JsonConvert.SerializeObject(Extractedclass)}");
#endif
                                    response.ChunkedTransfer = true;
                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    response.ContentType = "application/json";
                                    await response.SendFinalChunk(WebCrypto.EncryptNoPreserveToByteArray(Extractedclass
                                    , DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCrypto.AuthIV));
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
                    }, null, true)
                    .MapParameterRoute(HttpMethod.GET, "/api/{table}/{command}", async (ctx) =>
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

                                LoggerAccessor.LogInfo($"[API_GET] - Table:{table} was requested with Command:{command}");

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
                                                    LoggerAccessor.LogWarn($"[API_GET] - Table:{table} - does not handle requested Command:{command} - Please report this on GITHUB if it's unexpected.");
                                                    break;
                                            }
                                            keys = null;
                                        }
                                        break;
                                    case "World":
                                        if (Roles.Contains("database"))
                                        {
                                            World? World = new();
                                            switch (command)
                                            {
                                                case "getChannels":
                                                    Extractedclass = await World.getChannels();
                                                    break;
                                                case "getLocations":
                                                    Extractedclass = await World.getLocations();
                                                    break;
                                                default:
                                                    if (command.Contains('/'))
                                                    {
                                                        string[] parts = command.Split('/');

                                                        if (parts.Length == 2)
                                                        {
                                                            switch (parts[0])
                                                            {
                                                                case "getLocations":
                                                                    Extractedclass = await World.getLocations(int.Parse(parts[1]));
                                                                    break;
                                                            }
                                                        }
                                                    }

                                                    if (Extractedclass == null)
                                                        LoggerAccessor.LogWarn($"[API_GET] - Table:{table} - does not handle requested Command:{command} - Please report this on GITHUB if it's unexpected.");

                                                    break;
                                            }
                                            World = null;
                                        }
                                        break;
                                    default:
                                        LoggerAccessor.LogWarn($"[API_GET] - Unknown Table was requested:{table} - Please report this on GITHUB if it's unexpected.");
                                        break;
                                }

                                if (Extractedclass != null)
                                {
#if DEBUG
                                    LoggerAccessor.LogInfo($"[API_GET] - Extracted Data -> {JsonConvert.SerializeObject(Extractedclass)}");
#endif

                                    response.ChunkedTransfer = true;
                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    response.ContentType = "application/json";
                                    await response.SendFinalChunk(WebCrypto.EncryptNoPreserveToByteArray(Extractedclass
                                    , DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCrypto.AuthIV));
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
                    }, null, true)
                    .MapParameterRoute(HttpMethod.POST, "/FileServices/{command}", async (ctx) =>
                    {
                        HttpRequestBase request = ctx.Request;
                        HttpResponseBase response = ctx.Response;

                        try
                        {
                            string? command = request.Url.Parameters["command"];
                            List<string>? Roles = JsonConvert.DeserializeObject<List<string>>(request.Headers["MiddlewareRoles"] ?? "[]");

                            if (string.IsNullOrEmpty(command) || Roles == null)
                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                            else
                            {
                                object? Extractedclass = null;
                                string PostData = ctx.Request.DataAsString;

                                LoggerAccessor.LogInfo($"[FILESERVICES_API_POST] - FileServices was requested with Command:{command}");

                                if (Roles.Contains("database"))
                                {
                                    FileServices? files = new();
                                    switch (command)
                                    {
                                        case "addFile":
                                            if (request.QuerystringExists("AppId") && request.QuerystringExists("File"))
                                                Extractedclass = await files.addFile(await GetClassFromString<FileDTO>(PostData) ?? new FileDTO { });
                                            break;
                                        case "deleteFile":
                                            Extractedclass = await files.deleteFile(await GetClassFromString<FileDTO>(PostData) ?? new FileDTO { });
                                            break;
                                        case "updateFileAttributes":
                                            if (request.QuerystringExists("File"))
                                                Extractedclass = await files.updateFileAttributes(await GetClassFromString<FileAttributesDTO>(PostData) ?? new FileAttributesDTO { });
                                            break;
                                        case "updateFileMetaData":
                                            if (request.QuerystringExists("AppId"))
                                                Extractedclass = await files.updateFileMetaData(await GetClassFromString<FileMetaDataDTO>(PostData) ?? new FileMetaDataDTO { });
                                            break;
                                        default:
                                            LoggerAccessor.LogWarn($"[FILESERVICES_API_POST] - FileServices - does not handle requested Command:{command} - Please report this on GITHUB if it's unexpected.");
                                            break;
                                    }
                                    files = null;
                                }

                                if (Extractedclass != null)
                                {
#if DEBUG
                                    LoggerAccessor.LogInfo($"[FILESERVICES_API_POST] - Extracted Data -> {JsonConvert.SerializeObject(Extractedclass)}");
#endif
                                    response.ChunkedTransfer = true;
                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    response.ContentType = "application/json";
                                    await response.SendFinalChunk(WebCrypto.EncryptNoPreserveToByteArray(Extractedclass
                                    , DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCrypto.AuthIV));
                                    return;
                                }
                                else
                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            }
                        }
                        catch (Exception ex)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            LoggerAccessor.LogError($"[FILESERVICES_API_POST] - Thrown an exception:{ex}");
                        }

                        response.ContentType = "text/plain";
                        await response.Send();
                    }, null, true)
                    .MapParameterRoute(HttpMethod.GET, "/FileServices/{command}", async (ctx) =>
                    {
                        HttpRequestBase request = ctx.Request;
                        HttpResponseBase response = ctx.Response;

                        try
                        {
                            string? command = request.Url.Parameters["command"];
                            List<string>? Roles = JsonConvert.DeserializeObject<List<string>>(request.Headers["MiddlewareRoles"] ?? "[]");

                            if (string.IsNullOrEmpty(command) || Roles == null)
                                response.StatusCode = (int)HttpStatusCode.Forbidden;
                            else
                            {
                                object? Extractedclass = null;

                                LoggerAccessor.LogInfo($"[FILESERVICES_API_GET] - FileServices was requested with Command:{command}");

                                if (Roles.Contains("database"))
                                {
                                    FileServices? files = new();
                                    switch (command)
                                    {
                                        case "getFileList":
                                            if (request.QuerystringExists("AppId") && request.QuerystringExists("FileNameBeginsWith") && request.QuerystringExists("OwnerByID"))
                                                Extractedclass = await files.getFileList(int.Parse(request.RetrieveQueryValue("AppId")), request.RetrieveQueryValue("FileNameBeginsWith"), int.Parse(request.RetrieveQueryValue("OwnerByID")));
                                            break;
                                        case "getFileListExt":
                                            if (request.QuerystringExists("AppId") && request.QuerystringExists("FileNameBeginsWith") && request.QuerystringExists("OwnerByID") && request.QuerystringExists("metaKey") && request.QuerystringExists("metaValue"))
                                                Extractedclass = await files.getFileListExt(int.Parse(request.RetrieveQueryValue("AppId")), request.RetrieveQueryValue("FileNameBeginsWith"), int.Parse(request.RetrieveQueryValue("OwnerByID")), request.RetrieveQueryValue("metaKey"));
                                            break;
                                        case "getFileAttributes":
                                            if (request.QuerystringExists("AppId") && request.QuerystringExists("File"))
                                                Extractedclass = await files.getFileAttributes(await GetClassFromString<FileDTO>(request.RetrieveQueryValue("File")) ?? new FileDTO { });
                                            break;
                                        case "getFileMetaData":
                                            if (request.QuerystringExists("appId") && request.QuerystringExists("FileName") && request.QuerystringExists("Key"))
                                                Extractedclass = await files.getFileMetaData(int.Parse(request.RetrieveQueryValue("appId")), request.RetrieveQueryValue("FileName"), request.RetrieveQueryValue("Key"));
                                            break;
                                        default:
                                            LoggerAccessor.LogWarn($"[FILESERVICES_API_POST] - FileServices - does not handle requested Command:{command} - Please report this on GITHUB if it's unexpected.");
                                            break;
                                    }
                                    files = null;
                                }

                                if (Extractedclass != null)
                                {
#if DEBUG
                                    LoggerAccessor.LogInfo($"[FILESERVICES_API_GET] - Extracted Data -> {JsonConvert.SerializeObject(Extractedclass)}");
#endif

                                    response.ChunkedTransfer = true;
                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    response.ContentType = "application/json";
                                    await response.SendFinalChunk(WebCrypto.EncryptNoPreserveToByteArray(Extractedclass
                                    , DatabaseMiddlewareServerConfiguration.DatabaseAccessKey, WebCrypto.AuthIV));
                                    return;
                                }
                                else
                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            }

                        }
                        catch (Exception ex)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            LoggerAccessor.LogError($"[FILESERVICES_API_GET] - Thrown an exception:{ex}");
                        }

                        response.ContentType = "text/plain";
                        await response.Send();
                    }, null, true)
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
                    })
                    .Build();

                _Server.Events.Logger = LoggerAccessor.LogInfo;
                _Server.Events.ExceptionEncountered += ExceptionEncountered;
                _Server.Settings.Debug.Responses = true;
                _Server.Settings.Debug.Routing = true;

                _Server.Start();
                LoggerAccessor.LogInfo($"Database Server initiated on port: {port}...");
            }
        }

        private static Task PreRoutingHandler(HttpContextBase ctx)
        {
            string clientip = ctx.Request.Source.IpAddress;
            string clientport = ctx.Request.Source.Port.ToString();
            string SuplementalMessage = string.Empty;
            string? GeoCodeString = GeoIP.GetGeoCodeFromIP(IPAddress.Parse(clientip));
            string fullurl = HTTPProcessor.DecodeUrl(ctx.Request.Url.RawWithQuery);

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
