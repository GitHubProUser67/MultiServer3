using System.Globalization;
using SRVEmu.Arcadia.EA;
using SRVEmu.Arcadia.EA.Constants;
using SRVEmu.Arcadia.EA.Ports;
using SRVEmu.Arcadia.Storage;
using Org.BouncyCastle.Tls;
using CustomLogger;

namespace SRVEmu.Arcadia.Handlers;

public class FeslServerHandler
{
    private readonly SharedCounters _sharedCounters;
    private readonly SharedCache _sharedCache;
    private readonly IEAConnection _conn;

    private readonly Dictionary<string, Func<Packet, Task>> _handlers;

    private readonly Dictionary<string, object> _sessionCache = new();
    private uint _feslTicketId;

    public FeslServerHandler(IEAConnection conn, SharedCounters sharedCounters, SharedCache sharedCache)
    {
        _sharedCounters = sharedCounters;
        _sharedCache = sharedCache;
        _conn = conn;

        _handlers = new Dictionary<string, Func<Packet, Task>>()
        {
            ["fsys/Hello"] = HandleHello,
            ["fsys/MemCheck"] = HandleMemCheck,
            ["fsys/GetPingSites"] = HandleGetPingSites,
            ["acct/NuLogin"] = HandleNuLogin,
            ["acct/NuGetPersonas"] = HandleNuGetPersonas,
            ["acct/NuLoginPersona"] = HandleNuLoginPersona,
            ["asso/GetAssociations"] = HandleGetAssociations,
        };
    }

    public async Task HandleClientConnection(TlsServerProtocol tlsProtocol, string clientEndpoint)
    {
        _conn.InitializeSecure(tlsProtocol, clientEndpoint);
        await foreach (var packet in _conn.StartConnection())
        {
            await HandlePacket(packet);
        }
    }

    public async Task HandlePacket(Packet packet)
    {
        string reqTxn = packet.TXN;
        string packetType = packet.Type;
        _handlers.TryGetValue($"{packetType}/{reqTxn}", out var handler);

        if (handler is null)
        {
            LoggerAccessor.LogWarn("[Arcadia] - FeslServerHandler-HandlePacket Unknown packet type: {type}, TXN: {txn}", packet.Type, reqTxn);
            Interlocked.Increment(ref _feslTicketId);
            return;
        }

        LoggerAccessor.LogDebug("[Arcadia] - FeslServerHandler-HandlePacket Incoming Type: {type} | TXN: {txn}", packet.Type, reqTxn);
        await handler(packet);
    }

    private async Task HandleHello(Packet request)
    {
        if (request["clientType"] != "server")
            throw new NotSupportedException("Client tried connecting to a server port!");

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
                    { "theaterPort", (int)TheaterServerPort.RomePC }
                }));
        await SendMemCheck();
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
            { "maxListSize", 100 }
        };

        LoggerAccessor.LogInfo("[Arcadia] - FeslServerHandler-HandleGetAssociations Association type: {assoType}", assoType);

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

        long uid = _sharedCounters.GetNextUserId();
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