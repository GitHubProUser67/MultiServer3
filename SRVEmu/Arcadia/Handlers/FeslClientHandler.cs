using System.Globalization;
using SRVEmu.Arcadia.EA;
using SRVEmu.Arcadia.EA.Constants;
using SRVEmu.Arcadia.EA.Ports;
using SRVEmu.Arcadia.PSN;
using SRVEmu.Arcadia.Storage;
using Org.BouncyCastle.Tls;
using CustomLogger;

namespace SRVEmu.Arcadia.Handlers;

public class FeslClientHandler
{
    private readonly SharedCounters _sharedCounters;
    private readonly SharedCache _sharedCache;
    private readonly IEAConnection _conn;

    private readonly Dictionary<string, Func<Packet, Task>> _handlers;

    private readonly Dictionary<string, object> _sessionCache = new();
    private FeslGamePort _servicePort;
    private uint _feslTicketId;

    public FeslClientHandler(IEAConnection conn, SharedCounters sharedCounters, SharedCache sharedCache)
    {
        _sharedCounters = sharedCounters;
        _sharedCache = sharedCache;
        _conn = conn;

        _handlers = new Dictionary<string, Func<Packet, Task>>()
        {
            ["fsys/Hello"] = HandleHello,
            ["fsys/MemCheck"] = HandleMemCheck,
            ["fsys/GetPingSites"] = HandleGetPingSites,
            ["pnow/Start"] = HandlePlayNow,
            ["acct/NuPS3Login"] = HandleNuPs3Login,
            ["acct/NuLogin"] = HandleNuLogin,
            ["acct/NuGetPersonas"] = HandleNuGetPersonas,
            ["acct/NuLoginPersona"] = HandleNuLoginPersona,
            ["acct/NuGetTos"] = HandleGetTos,
            ["acct/GetTelemetryToken"] = HandleTelemetryToken,
            ["acct/NuPS3AddAccount"] = HandleAddAccount,
            ["acct/NuLookupUserInfo"] = HandleLookupUserInfo,
            ["acct/NuGetEntitlements"] = HandleNuGetEntitlements,
            ["acct/NuGrantEntitlement"] = HandleNuGrantEntitlement,
            ["acct/GetLockerURL"] = HandleGetLockerUrl,
            ["recp/GetRecord"] = HandleGetRecord,
            ["recp/GetRecordAsMap"] = HandleGetRecordAsMap,
            ["asso/GetAssociations"] = HandleGetAssociations,
            ["pres/PresenceSubscribe"] = HandlePresenceSubscribe,
            ["pres/SetPresenceStatus"] = HandleSetPresenceStatus,
            ["rank/GetStats"] = HandleGetStats
        };
    }

    public async Task HandleClientConnection(TlsServerProtocol tlsProtocol, string clientEndpoint, FeslGamePort servicePort)
    {
        _servicePort = servicePort;
        _conn.InitializeSecure(tlsProtocol, clientEndpoint);
        await foreach (var packet in _conn.StartConnection())
        {
            await HandlePacket(packet);
        }
    }

    public async Task HandlePacket(Packet packet)
    {
        var reqTxn = packet.TXN;
        var packetType = packet.Type;
        _handlers.TryGetValue($"{packetType}/{reqTxn}", out var handler);

        if (handler is null)
        {
            LoggerAccessor.LogWarn("[Arcadia] - FeslClientHandler-HandlePacket Unknown packet type: {type}, TXN: {txn}", packet.Type, reqTxn);
            Interlocked.Increment(ref _feslTicketId);
            return;
        }

        LoggerAccessor.LogDebug("[Arcadia] - FeslClientHandler-HandlePacket Incoming Type: {type} | TXN: {txn}", packet.Type, reqTxn);
        await handler(packet);
    }

    private async Task HandleHello(Packet request)
    {
        if (request["clientType"] == "server")
            throw new NotSupportedException("Server tried connecting to a client port!");

        await _conn.SendPacket(new Packet("fsys", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
                {
                    { "domainPartition.domain", "ps3" },
                    { "messengerIp", "127.0.0.1" },
                    { "messengerPort", 0 },
                    { "domainPartition.subDomain", "BEACH" },
                    { "TXN", "Hello" },
                    { "activityTimeoutSecs", 0 },
                    { "curTime", DateTime.UtcNow.ToString("MMM-dd-yyyy HH:mm:ss 'UTC'", CultureInfo.InvariantCulture) },
                    { "theaterIp", SRVEmuServerConfiguration.TheaterAddress },
                    { "theaterPort", (int)GetTheaterPort() }
                }));
        await SendMemCheck();
    }

    private async Task HandleTelemetryToken(Packet request)
    {
        await _conn.SendPacket(new Packet("acct", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", "GetTelemetryToken" },
        }));
    }

    private async Task HandlePlayNow(Packet request)
    {
        long[] servers = _sharedCache.ListServersGIDs();
        var serverData = _sharedCache.GetGameServerDataByGid(servers.First());

        // Should return approprate error to user
        if (serverData is null) throw new NotImplementedException();

        long pnowId = _sharedCounters.GetNextPnowId();

        await _conn.SendPacket(new Packet("pnow", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", "Start" },
            { "id.id", pnowId },
            { "id.partition", "/ps3/BEACH" },
        }));

        await _conn.SendPacket(new Packet("pnow", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", "Status" },
            { "id.id", pnowId },
            { "id.partition", "/ps3/BEACH" },
            { "sessionState", "COMPLETE" },
            { "props.{}", 3 },
            { "props.{resultType}", "JOIN" },
            { "props.{avgFit}", "0.8182313914386985" },
            { "props.{games}.[]", 1 },
            { "props.{games}.0.gid", serverData["GID"] },
            { "props.{games}.0.lid", serverData["LID"] }
        }));
    }

    private async Task HandleGetRecord(Packet request)
    {
        await _conn.SendPacket(new Packet("recp", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "localizedMessage", "Nope" },
            { "errorContainer.[]", 0 },
            { "errorCode", 5000 },
        }));
    }

    private async Task HandleGetRecordAsMap(Packet request)
    {
        await _conn.SendPacket(new Packet("recp", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "TTL", 0 },
            { "state", 1 },
            { "values.{}", 0 }
        }));
    }

    private async Task HandleNuGrantEntitlement(Packet request)
    {
        await _conn.SendPacket(new Packet("acct", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", request.TXN }
        }));
    }


    private async Task HandleGetStats(Packet request)
    {
        // TODO Not entirely sure if this works well with the game, since stats requests are usually sent as multi-packet queries with base64 encoded data

        // TODO: Add some stats
        // var keysStr = request.DataDict["keys.[]"] as string ?? string.Empty;
        // var reqKeys = int.Parse(keysStr, CultureInfo.InvariantCulture);
        // for (var i = 0; i < reqKeys; i++)
        // {
        //     var key = request.DataDict[$"keys.{i}"];

        //     responseData.Add($"stats.{i}.key", key);
        //     responseData.Add($"stats.{i}.value", 0.0);
        // }

        await _conn.SendPacket(new Packet("rank", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", "GetStats" },
            { "stats.[]", 0 }
        }));
    }

    private async Task HandlePresenceSubscribe(Packet request)
    {
        await _conn.SendPacket(new Packet("pres", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", "PresenceSubscribe" },
            { "responses.0.outcome", "0" },
            { "responses.[]", "1" },
            { "responses.0.owner.type", "1" },
            { "responses.0.owner.id", _sessionCache["UID"] },
        }));
    }

    private async Task HandleSetPresenceStatus(Packet request)
    {
        await _conn.SendPacket(new Packet("pres", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", "SetPresenceStatus" },
        }));
    }

    private async Task HandleLookupUserInfo(Packet request)
    {
        await _conn.SendPacket(new Packet("acct", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", "NuLookupUserInfo" },
            { "userInfo.[]", "1" },
            { "userInfo.0.userName", _sessionCache["personaName"] },
        }));
    }

    private async Task HandleGetAssociations(Packet request)
    {
        var assoType = request.DataDict["type"] as string ?? string.Empty;
        var responseData = new Dictionary<string, object>
        {
            { "TXN", "GetAssociations" },
            { "domainPartition.domain", request.DataDict["domainPartition.domain"] },
            { "domainPartition.subDomain", request.DataDict["domainPartition.subDomain"] },
            { "owner.id", _sessionCache["UID"] },
            { "owner.type", "1" },
            { "type", assoType },
            { "members.[]", "0" },
        };

        if (assoType == "PlasmaMute")
        {
            responseData.Add("maxListSize", 100);
            responseData.Add("owner.name", _sessionCache["personaName"]);
        }
        else
            LoggerAccessor.LogWarn("[Arcadia] - FeslClientHandler-HandleGetAssociations Unknown association type: {assoType}", assoType);

        await _conn.SendPacket(new Packet("asso", FeslTransmissionType.SinglePacketResponse, request.Id, responseData));
    }

    private async Task HandleGetPingSites(Packet request)
    {
        const string serverIp = "127.0.0.1";

        await _conn.SendPacket(new Packet("fsys", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", "GetPingSites" },
            { "pingSite.[]", "4"},
            { "pingSite.0.addr", serverIp },
            { "pingSite.0.type", "0"},
            { "pingSite.0.name", "eu1"},
            { "minPingSitesToPing", "0"}
        }));
    }


    private TheaterGamePort GetTheaterPort()
    {
        switch (_servicePort)
        {
            case FeslGamePort.RomePS3:
                return TheaterGamePort.RomePS3;
            case FeslGamePort.BeachPS3:
                return TheaterGamePort.BeachPS3;
            case FeslGamePort.BadCompanyPS3:
                return TheaterGamePort.BadCompanyPS3;
            default:
                LoggerAccessor.LogWarn("[Arcadia] - FeslClientHandler-GetTheaterPort Unknown FESL service port: {port}", (int)_servicePort);
                return TheaterGamePort.BeachPS3;
        }
    }

    private async Task HandleGetTos(Packet request)
    {
        // TODO Same as with stats, usually sent as multi-packed response
        const string tos = "Welcome to Arcadia!\nBeware, here be dragons!";

        await _conn.SendPacket(new Packet("acct", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", "NuGetTos" },
            { "version", "20426_17.20426_17" },
            { "tos", $"{System.Net.WebUtility.UrlEncode(tos).Replace('+', ' ')}" },
        }));
    }

    private async Task HandleNuLogin(Packet request)
    {
        _sessionCache["personaName"] = request["nuid"];

        await _conn.SendPacket(new Packet("acct", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "encryptedLoginInfo", "Ciyvab0tregdVsBtboIpeChe4G6uzC1v5_-SIxmvSL" + "bjbfvmobxvmnawsthtgggjqtoqiatgilpigaqqzhejglhbaokhzltnstufrfouwrvzyphyrspmnzprxcocyodg" }
        }));
    }

    private async Task HandleNuGetPersonas(Packet request)
    {
        await _conn.SendPacket(new Packet(request.Type, FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "personas.[]", 1 },
            { "personas.0", _sessionCache["personaName"] },
        }));
    }
    
    private async Task HandleNuLoginPersona(Packet request)
    {
        _sessionCache["LKEY"] = SharedCounters.GetNextLkey();

        var uid = _sharedCounters.GetNextUserId();
        _sessionCache["UID"] = uid;

        _sharedCache.AddUserWithKey((string)_sessionCache["LKEY"], (string)_sessionCache["personaName"]);
        await _conn.SendPacket(new Packet(request.Type, FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "lkey", _sessionCache["LKEY"] },
            { "profileId", uid },
            { "userId", uid },
        }));
    }

    private async Task HandleNuGetEntitlements(Packet request)
    {
        var loginResponseData = new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "entitlements.[]", 0 }
        };

        var packet = new Packet(request.Type, FeslTransmissionType.SinglePacketResponse, request.Id, loginResponseData);
        await _conn.SendPacket(packet);
    }

    private async Task HandleGetLockerUrl(Packet request)
    {
        await _conn.SendPacket(new Packet(request.Type, FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "url", "http://127.0.0.1/arcadia.jsp" }
        }));
    }

    private async Task HandleNuPs3Login(Packet request)
    {
        // var tosAccepted = request.DataDict.TryGetValue("tosVersion", out var tosAcceptedValue);
        // if (false)
        // {
        //     loginResponseData.Add("TXN", request.Type);
        //     loginResponseData.Add("localizedMessage", "The password the user specified is incorrect");
        //     loginResponseData.Add("errorContainer.[]", "0");
        //     loginResponseData.Add("errorCode", "122");
        // }

        // if (!tosAccepted || string.IsNullOrEmpty(tosAcceptedValue as string))
        // {
        //     loginResponseData.Add("TXN", request.Type);
        //     loginResponseData.Add( "localizedMessage", "The user was not found" );
        //     loginResponseData.Add( "errorContainer.[]", 0 );
        //     loginResponseData.Add( "errorCode", 101 );
        // }
        // else

        string loginTicket = request.DataDict["ticket"] as string ?? string.Empty;
        TicketData[] ticketData = TicketDecoder.DecodeFromASCIIString(loginTicket);
        string? onlineId = (ticketData[5] as BStringData)?.Value?.TrimEnd('\0');

        _sessionCache["personaName"] = onlineId ?? throw new NotImplementedException();
        _sessionCache["LKEY"] = SharedCounters.GetNextLkey();
        _sessionCache["UID"] = _sharedCounters.GetNextUserId();

        _sharedCache.AddUserWithKey((string)_sessionCache["LKEY"], (string)_sessionCache["personaName"]);

        await _conn.SendPacket(new Packet("acct", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", "NuPS3Login" },
            { "lkey", _sessionCache["LKEY"] },
            { "userId", _sessionCache["UID"] },
            { "personaName", _sessionCache["personaName"] }
        }));
    }

    private async Task SendInvalidLogin(Packet request)
    {
        await _conn.SendPacket(new Packet("acct", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            { "TXN", "NuPS3Login" },
            { "localizedMessage", "Nope" },
            { "errorContainer.[]", 0 },
            { "errorCode", 101 }

            // 101: unknown user
            // 102: account disabled
            // 103: account banned
            // 120: account not entitled
            // 121: too many login attempts
            // 122: invalid password
            // 123: game has not been registered (?)
        }));
    }

    private async Task HandleAddAccount(Packet request)
    {
        string? email = request.DataDict["nuid"] as string;
        string? pass = request.DataDict["password"] as string;

        LoggerAccessor.LogDebug("[Arcadia] - FeslClientHandler-HandleAddAccount Trying to register user {email} with password {pass}", email, pass);

        await _conn.SendPacket(new Packet("acct", FeslTransmissionType.SinglePacketResponse, request.Id, new Dictionary<string, object>
        {
            {"TXN", "NuPS3AddAccount"}
        }));
    }

    private static Task HandleMemCheck(Packet _)
    {
        return Task.CompletedTask;
    }

    private async Task SendMemCheck()
    {
        // FESL backend is requesting the client to respond to the memcheck, so this is a request
        // But since memchecks are not part of the meaningful conversation with the client, they don't have a packed id
        await _conn.SendPacket(new Packet("fsys", FeslTransmissionType.SinglePacketRequest, 0, new Dictionary<string, object>
                {
                    { "TXN", "MemCheck" },
                    { "memcheck.[]", "0" },
                    { "type", "0" },
                    { "salt", PacketUtils.GenerateSalt() }
                }));
    }
}