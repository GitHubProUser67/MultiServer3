using System.Globalization;
using System.Text;
using SRVEmu.Arcadia.EA;
using SRVEmu.Arcadia.EA.Constants;
using SRVEmu.Arcadia.PSN;
using SRVEmu.Arcadia.Storage;
using Org.BouncyCastle.Tls;

namespace SRVEmu.Arcadia.Handlers;

public class FeslHandler
{
    private readonly SharedCounters _sharedCounters;
    private readonly SharedCache _sharedCache;

    public FeslHandler(SharedCounters sharedCounters, SharedCache sharedCache)
    {
        _sharedCounters = sharedCounters;
        _sharedCache = sharedCache;
    }

    private readonly Dictionary<string, object> _sessionCache = new();
    private TlsServerProtocol _network = null!;
    private string _clientEndpoint = null!;

    private uint _feslTicketId;

    public async Task HandleClientConnection(TlsServerProtocol network, string clientEndpoint)
    {
        _network = network;
        _clientEndpoint = clientEndpoint;

        while (_network.IsConnected)
        {
            int read;
            byte[]? readBuffer;

            try
            {
                (read, readBuffer) = await PacketUtils.ReadApplicationDataAsync(_network);
            }
            catch
            {
                CustomLogger.LoggerAccessor.LogWarn("Connection has been closed with {endpoint}", _clientEndpoint);
                break;
            }

            if (read == 0)
            {
                continue;
            }

            Packet reqPacket = new(readBuffer[..read]);
            string reqTxn = reqPacket.TXN;

            CustomLogger.LoggerAccessor.LogInfo("Incoming Type: {type} | TXN: {txn}", reqPacket.Type, reqTxn);
            CustomLogger.LoggerAccessor.LogInfo("data:{data}", Encoding.ASCII.GetString(readBuffer[..read]));

            if (reqPacket.Type == "fsys" && reqTxn == "Hello")
                await HandleHello(reqPacket);
            else if (reqPacket.Type == "pnow" && reqTxn == "Start")
                await HandlePlayNow(reqPacket);
            else if (reqPacket.Type == "fsys" && reqTxn == "MemCheck")
                await HandleMemCheck();
            else if (reqPacket.Type == "fsys" && reqTxn == "GetPingSites")
                await HandleGetPingSites(reqPacket);
            else if (reqPacket.Type == "acct" && reqTxn == "NuPS3Login")
                await HandleNuPs3Login(reqPacket);
            else if (reqPacket.Type == "acct" && reqTxn == "NuLogin")
                await HandleNuLogin(reqPacket);
            else if (reqPacket.Type == "acct" && reqTxn == "NuGetPersonas")
                await HandleNuGetPersonas(reqPacket);
            else if (reqPacket.Type == "acct" && reqTxn == "NuLoginPersona")
                await HandleNuLoginPersona(reqPacket);
            else if (reqPacket.Type == "acct" && reqTxn == "NuGetTos")
                await HandleGetTos(reqPacket);
            else if (reqPacket.Type == "acct" && reqTxn == "GetTelemetryToken")
                await HandleTelemetryToken(reqPacket);
            else if (reqPacket.Type == "acct" && reqTxn == "NuPS3AddAccount")
                await HandleAddAccount(reqPacket);
            else if (reqPacket.Type == "acct" && reqTxn == "NuLookupUserInfo")
                await HandleLookupUserInfo(reqPacket);
            else if (reqPacket.Type == "acct" && reqTxn == "NuGetEntitlements")
                await HandleNuGetEntitlements(reqPacket);
            else if (reqPacket.Type == "acct" && reqTxn == "GetLockerURL")
                await HandleGetLockerUrl(reqPacket);
            else if (reqPacket.Type == "asso" && reqTxn == "GetAssociations")
                await HandleGetAssociations(reqPacket);
            else if (reqPacket.Type == "pres" && reqTxn == "PresenceSubscribe")
                await HandlePresenceSubscribe(reqPacket);
            else if (reqPacket.Type == "pres" && reqTxn == "SetPresenceStatus")
                await HandleSetPresenceStatus(reqPacket);
            else if (reqPacket.Type == "rank" && reqTxn == "GetStats")
                await HandleGetStats(reqPacket);
            else
            {
                CustomLogger.LoggerAccessor.LogWarn("[feslHandler] - Unknown packet type: {type}, TXN: {txn}", reqPacket.Type, reqTxn);
                Interlocked.Increment(ref _feslTicketId);
            }
        }
    }

    private async Task HandleTelemetryToken(Packet request)
    {
        var responseData = new Dictionary<string, object>
        {
            { "TXN", "GetTelemetryToken" },
        };

        Packet packet = new("acct", FeslTransmissionType.SinglePacketResponse, request.Id, responseData);
        await SendPacket(packet);
    }

    private async Task HandlePlayNow(Packet request)
    {
        long pnowId = _sharedCounters.GetNextPnowId();
        long gid = _sharedCounters.GetNextGameId();
        long lid = _sharedCounters.GetNextLobbyId();

        var data1 = new Dictionary<string, object>
        {
            { "TXN", "Start" },
            { "id.id", pnowId },
            { "id.partition", "/ps3/BEACH" },
        };

        Packet packet1 = new("pnow", FeslTransmissionType.SinglePacketResponse, request.Id, data1);
        await SendPacket(packet1);

        var data2 = new Dictionary<string, object>
        {
            { "TXN", "Status" },
            { "id.id", pnowId },
            { "id.partition", "/ps3/BEACH" },
            { "sessionState", "COMPLETE" },
            { "props.{}", 3 },
            { "props.{resultType}", "JOIN" },
            { "props.{avgFit}", "0.8182313914386985" },
            { "props.{games}.[]", 1 },
            { "props.{games}.0.gid", gid },
            { "props.{games}.0.lid", lid }
        };

        Packet packet2 = new("pnow", FeslTransmissionType.SinglePacketResponse, request.Id, data2);
        await SendPacket(packet2);
    }

    private async Task HandleGetStats(Packet request)
    {
        // TODO Not entirely sure if this works well with the game, since stats requests are usually sent as multi-packet queries with base64 encoded data
        var responseData = new Dictionary<string, object>
        {
            { "TXN", "GetStats" },
            {"stats.[]", 0 }
        };

        // TODO: Add some stats
        // var keysStr = request.DataDict["keys.[]"] as string ?? string.Empty;
        // var reqKeys = int.Parse(keysStr, CultureInfo.InvariantCulture);
        // for (var i = 0; i < reqKeys; i++)
        // {
        //     var key = request.DataDict[$"keys.{i}"];

        //     responseData.Add($"stats.{i}.key", key);
        //     responseData.Add($"stats.{i}.value", 0.0);
        // }

        Packet packet = new("rank", FeslTransmissionType.SinglePacketResponse, request.Id, responseData);
        await SendPacket(packet);
    }

    private async Task HandlePresenceSubscribe(Packet request)
    {
        var responseData = new Dictionary<string, object>
        {
            { "TXN", "PresenceSubscribe" },
            { "responses.0.outcome", "0" },
            { "responses.[]", "1" },
            { "responses.0.owner.type", "1" },
            { "responses.0.owner.id", _sessionCache["UID"] },
        };

        Packet packet = new("pres", FeslTransmissionType.SinglePacketResponse, request.Id, responseData);
        await SendPacket(packet);
    }

    private async Task HandleSetPresenceStatus(Packet request)
    {
        var responseData = new Dictionary<string, object>
        {
            { "TXN", "SetPresenceStatus" },
        };

        Packet packet = new("pres", FeslTransmissionType.SinglePacketResponse, request.Id, responseData);
        await SendPacket(packet);
    }

    private async Task HandleLookupUserInfo(Packet request)
    {
        var responseData = new Dictionary<string, object>
        {
            { "TXN", "NuLookupUserInfo" },
            { "userInfo.[]", "1" },
            { "userInfo.0.userName", _sessionCache["personaName"] },
        };

        Packet packet = new("acct", FeslTransmissionType.SinglePacketResponse, request.Id, responseData);
        await SendPacket(packet);
    }

    private async Task HandleGetAssociations(Packet request)
    {
        string assoType = request.DataDict["type"] as string ?? string.Empty;
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
            CustomLogger.LoggerAccessor.LogWarn("[feslHandler] - Unknown association type: {assoType}", assoType);

        Packet packet = new("asso", FeslTransmissionType.SinglePacketResponse, request.Id, responseData);
        await SendPacket(packet);
    }

    private async Task HandleGetPingSites(Packet request)
    {
        const string serverIp = "127.0.0.1";

        var responseData = new Dictionary<string, object>
        {
            { "TXN", "GetPingSites" },
            { "pingSite.[]", "4"},
            { "pingSite.0.addr", serverIp },
            { "pingSite.0.type", "0"},
            { "pingSite.0.name", "eu1"},
            { "minPingSitesToPing", "0"}
        };

        Packet packet = new("fsys", FeslTransmissionType.SinglePacketResponse, request.Id, responseData);
        await SendPacket(packet);
    }

    private async Task HandleHello(Packet request)
    {
        var serverHelloData = new Dictionary<string, object>
                {
                    { "domainPartition.domain", "ps3" },
                    { "messengerIp", "127.0.0.1" },
                    { "messengerPort", 0 },
                    { "domainPartition.subDomain", "BEACH" },
                    { "TXN", "Hello" },
                    { "activityTimeoutSecs", 0 },
                    { "curTime", DateTime.UtcNow.ToString("MMM-dd-yyyy HH:mm:ss 'UTC'", CultureInfo.InvariantCulture)},
                    { "theaterIp", SRVEmuServerConfiguration.TheaterBindAddress },
                    { "theaterPort", Beach.TheaterPort }
                };

        Packet helloPacket = new("fsys", FeslTransmissionType.SinglePacketResponse, request.Id, serverHelloData);
        await SendPacket(helloPacket);
        await SendMemCheck();
    }

    private async Task HandleGetTos(Packet request)
    {
        // TODO Same as with stats, usually sent as multi-packed response
        const string tos = "Welcome to SRVEmu.Arcadia!\nBeware, here be dragons!";

        var data = new Dictionary<string, object>
        {
            { "TXN", "NuGetTos" },
            { "version", "20426_17.20426_17" },
            { "tos", $"{System.Net.WebUtility.UrlEncode(tos).Replace('+', ' ')}" },
        };

        Packet packet = new("acct", FeslTransmissionType.SinglePacketResponse, request.Id, data);
        await SendPacket(packet);
    }

    private async Task HandleNuLogin(Packet request)
    {
        _sessionCache["personaName"] = request["nuid"];

        var loginResponseData = new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "encryptedLoginInfo", "Ciyvab0tregdVsBtboIpeChe4G6uzC1v5_-SIxmvSL" + "bjbfvmobxvmnawsthtgggjqtoqiatgilpigaqqzhejglhbaokhzltnstufrfouwrvzyphyrspmnzprxcocyodg" }
        };

        Packet loginPacket = new("acct", FeslTransmissionType.SinglePacketResponse, request.Id, loginResponseData);
        await SendPacket(loginPacket);
    }

    private async Task HandleNuGetPersonas(Packet request)
    {
        var loginResponseData = new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "personas.[]", 1 },
            { "personas.0", _sessionCache["personaName"] },
        };

        Packet packet = new(request.Type, FeslTransmissionType.SinglePacketResponse, request.Id, loginResponseData);
        await SendPacket(packet);
    }
    
    private async Task HandleNuLoginPersona(Packet request)
    {
        _sessionCache["LKEY"] = _sharedCounters.GetNextLkey();

        long uid = _sharedCounters.GetNextUserId();
        _sessionCache["UID"] = uid;

        _sharedCache.AddUserWithKey((string)_sessionCache["LKEY"], (string)_sessionCache["personaName"]);
        var loginResponseData = new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "lkey", _sessionCache["LKEY"] },
            { "profileId", uid },
            { "userId", uid },
        };

        Packet packet = new(request.Type, FeslTransmissionType.SinglePacketResponse, request.Id, loginResponseData);
        await SendPacket(packet);
    }

    private async Task HandleNuGetEntitlements(Packet request)
    {
        var loginResponseData = new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "entitlements.[]", 0 }
        };

        Packet packet = new(request.Type, FeslTransmissionType.SinglePacketResponse, request.Id, loginResponseData);
        await SendPacket(packet);
    }

    private async Task HandleGetLockerUrl(Packet request)
    {
        var loginResponseData = new Dictionary<string, object>
        {
            { "TXN", request.TXN },
            { "url", "http://127.0.0.1/arcadia.jsp" }
        };

        Packet packet = new(request.Type, FeslTransmissionType.SinglePacketResponse, request.Id, loginResponseData);
        await SendPacket(packet);
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
        string onlineId = (ticketData[5] as BStringData).Value.TrimEnd('\0');

        _sessionCache["personaName"] = onlineId;
        _sessionCache["LKEY"] = _sharedCounters.GetNextLkey();
        _sessionCache["UID"] = _sharedCounters.GetNextUserId();

        _sharedCache.AddUserWithKey((string)_sessionCache["LKEY"], (string)_sessionCache["personaName"]);

        var loginResponseData = new Dictionary<string, object>
        {
            { "TXN", "NuPS3Login" },
            { "lkey", _sessionCache["LKEY"] },
            { "userId", _sessionCache["UID"] },
            { "personaName", _sessionCache["personaName"] }
        };

        Packet loginPacket = new("acct", FeslTransmissionType.SinglePacketResponse, request.Id, loginResponseData);
        await SendPacket(loginPacket);
    }

    private async Task SendInvalidLogin(Packet request)
    {
        var loginResponseData = new Dictionary<string, object>
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
        };

        Packet loginPacket = new("acct", FeslTransmissionType.SinglePacketResponse, request.Id, loginResponseData);
        await SendPacket(loginPacket);
    }

    private async Task HandleAddAccount(Packet request)
    {
        var data = new Dictionary<string, object>
        {
            {"TXN", "NuPS3AddAccount"}
        };

        string? email = request.DataDict["nuid"] as string;
        string? pass = request.DataDict["password"] as string;

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(pass))
            CustomLogger.LoggerAccessor.LogInfo("[feslHandler] - Trying to register user {email} with password {pass}", email, pass);

        Packet resultPacket = new("acct", FeslTransmissionType.SinglePacketResponse, request.Id, data);
        await SendPacket(resultPacket );
    }

    private static Task HandleMemCheck()
    {
        return Task.CompletedTask;
    }

    private async Task SendMemCheck()
    {
        var memCheckData = new Dictionary<string, object>
                {
                    { "TXN", "MemCheck" },
                    { "memcheck.[]", "0" },
                    { "type", "0" },
                    { "salt", PacketUtils.GenerateSalt() }
                };

        // FESL backend is requesting the client to respond to the memcheck, so this is a request
        // But since memchecks are not part of the meaningful conversation with the client, they don't have a packed id
        Packet memcheckPacket = new("fsys", FeslTransmissionType.SinglePacketRequest, 0, memCheckData);
        await SendPacket(memcheckPacket);
    }

    private async Task SendPacket(Packet packet)
    {
        byte[] serializedData = await packet.Serialize();
        _network.WriteApplicationData(serializedData.AsSpan());
    }
}