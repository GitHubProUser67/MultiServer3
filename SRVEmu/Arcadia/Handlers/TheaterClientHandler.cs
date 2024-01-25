using System.Net.Sockets;
using CustomLogger;
using SRVEmu.Arcadia.EA;
using SRVEmu.Arcadia.EA.Constants;
using SRVEmu.Arcadia.Storage;

namespace SRVEmu.Arcadia.Handlers;

public class TheaterClientHandler
{

    private static string UGID = string.Empty;
    private static int activePlayers = 0;
    private static int joiningPlayers = 0;

    private readonly SharedCounters _sharedCounters;
    private readonly SharedCache _sharedCache;
    private readonly IEAConnection _conn;

    private readonly Dictionary<string, Func<Packet, Task>> _handlers;

    private readonly Dictionary<string, object> _sessionCache = new();

    public TheaterClientHandler(IEAConnection conn, SharedCounters sharedCounters, SharedCache sharedCache)
    {
        _sharedCounters = sharedCounters;
        _sharedCache = sharedCache;
        _conn = conn;

        _handlers = new Dictionary<string, Func<Packet, Task>>
        {
            ["CONN"] = HandleCONN,
            ["USER"] = HandleUSER,
            ["ECNL"] = HandleECNL,
            ["EGAM"] = HandleEGAM,
            ["GDAT"] = HandleGDAT
        };
    }

    public async Task HandleClientConnection(NetworkStream network, string clientEndpoint)
    {
        _conn.InitializeInsecure(network, clientEndpoint);
        await foreach (var packet in _conn.StartConnection())
        {
            await HandlePacket(packet);
        }
    }

    public async Task HandlePacket(Packet packet)
    {
        string packetType = packet.Type;
        _handlers.TryGetValue(packetType, out var handler);

        if (handler is null)
        {
            LoggerAccessor.LogWarn("[Arcadia] - TheaterClientHandler-HandlePacket Unknown packet type: {type}", packetType);
            return;
        }

        LoggerAccessor.LogDebug("[Arcadia] - TheaterClientHandler-HandlePacket Incoming Type: {type}", packetType);
        await handler(packet);
    }

    private async Task HandleCONN(Packet request)
    {
        var tid = request.DataDict["TID"];

        LoggerAccessor.LogInfo("[Arcadia] - TheaterClientHandler-HandleCONN CONN: {tid}", tid);

        if (request["PLAT"] == "PC")
            _sharedCounters.SetServerTheaterNetworkStream(_conn.NetworkStream!);

        await _conn.SendPacket(new Packet("CONN", TheaterTransmissionType.OkResponse, 0, new Dictionary<string, object>
        {
            ["TIME"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["TID"] = tid,
            ["activityTimeoutSecs"] = 0,
            ["PROT"] = request.DataDict["PROT"]
        }));
    }

    private async Task HandleUSER(Packet request)
    {
        object lkey = request.DataDict["LKEY"];
        string username = _sharedCache.GetUsernameByLKey((string)lkey);

        _sessionCache["NAME"] = username;
        _sessionCache["UID"] = _sharedCounters.GetNextUserId();
        LoggerAccessor.LogInfo("[Arcadia] - TheaterClientHandler-HandleUSER USER: {name} {lkey}", username, lkey);

        await _conn.SendPacket(new Packet("USER", TheaterTransmissionType.OkResponse, 0, new Dictionary<string, object>
        {
            ["NAME"] = username,
            ["TID"] = request.DataDict["TID"]
        }));
    }
    
    // LeaveGame
    private async Task HandleECNL(Packet request)
    {
        // !TODO: set gid to a valid game id
        // !TODO: set lid to a valid lobby id

        await _conn.SendPacket(new Packet("ECNL", TheaterTransmissionType.OkResponse, 0, new Dictionary<string, object>
        {
            ["TID"] = request.DataDict["TID"],
            ["LID"] = request.DataDict["LID"],
            ["GID"] = request.DataDict["GID"],
        }));
    }

    // EnterGameRequest
    private async Task HandleEGAM(Packet request)
    {
        long ticket = _sharedCounters.GetNextTicket();
        _sessionCache["TICKET"] = ticket;

        var srvData = _sharedCache.GetGameServerDataByGid(long.Parse(request["GID"])) ?? throw new NotImplementedException();
        _sessionCache["UGID"] = srvData["UGID"];

        await SendEGRQToHost(request, ticket);
        await _conn.SendPacket(new Packet("EGAM", TheaterTransmissionType.OkResponse, 0, new Dictionary<string, object>
        {
            ["TID"] = request.DataDict["TID"],
            ["LID"] = request.DataDict["LID"],
            ["GID"] = request.DataDict["GID"],
        }));
        await SendEGEG(request, ticket, srvData);
    }

    private async Task SendEGRQToHost(Packet request, long ticket)
    {
        await _sharedCounters.GetServerTheaterNetworkStream()!.WriteAsync(await new Packet("EGRQ", TheaterTransmissionType.OkResponse, 0, new Dictionary<string, object>
        {
            ["R-INT-PORT"] = request["R-INT-PORT"],
            ["R-INT-IP"] = request["R-INT-IP"],
            ["PORT"] = request["PORT"],
            ["NAME"] = _sessionCache["NAME"],
            ["PTYPE"] = request["PTYPE"],
            ["TICKET"] = ticket,
            ["PID"] = _sessionCache["UID"],
            ["UID"] = _sessionCache["UID"],
            ["IP"] = _conn.ClientEndpoint.Split(":")[0],

            ["LID"] = request.DataDict["LID"],
            ["GID"] = request.DataDict["GID"],
        }).Serialize());
    }

    private async Task HandleGDAT(Packet request)
    {
        long serverGid = long.Parse(request["GID"]);
        var serverInfo = _sharedCache.GetGameServerDataByGid(serverGid) ?? throw new NotImplementedException();

        await _conn.SendPacket(new Packet("GDAT", TheaterTransmissionType.OkResponse, 0, new Dictionary<string, object>
        {
            ["TID"] = request["TID"],
            ["LID"] = serverInfo["LID"],
            ["GID"] = serverInfo["GID"],

            ["HU"] = "1000000000001",
            ["HN"] = "bfbc.server.ps3@ea.com",

            ["I"] = SRVEmuServerConfiguration.GameServerAddress,
            ["P"] = serverInfo["PORT"],

            ["N"] = serverInfo["NAME"],
            ["AP"] = 0,
            ["MP"] = serverInfo["MAX-PLAYERS"],
            ["JP"] = 1,
            ["PL"] = "ps3",

            ["PW"] = 0,
            ["TYPE"] = serverInfo["TYPE"],
            ["J"] = serverInfo["JOIN"],

            ["B-U-balance"] = serverInfo["B-U-balance"],
            ["B-U-Hardcore"] = serverInfo["B-U-Hardcore"],
            ["B-U-HasPassword"] = serverInfo["B-U-HasPassword"],
            ["B-U-Punkbuster"] = serverInfo["B-U-Punkbuster"],
            ["B-version"] = serverInfo["B-version"],
            ["V"] = "530204",
            ["B-U-level"] = serverInfo["B-U-level"],
            ["B-U-gamemode"] = serverInfo["B-U-gamemode"],
            ["B-U-sguid"] = serverInfo["B-U-sguid"],
            ["B-U-Time"] = serverInfo["B-U-Time"],
            ["B-U-hash"] = serverInfo["B-U-hash"],
            ["B-U-type"] = serverInfo["B-U-type"],
            ["B-U-region"] = serverInfo["B-U-region"],
            ["B-U-public"] = serverInfo["B-U-public"],
            ["B-U-elo"] = serverInfo["B-U-elo"],
            ["B-numObservers"] = serverInfo["B-numObservers"],
            ["B-maxObservers"] = serverInfo["B-maxObservers"]
        }));

        await SendGDET(request, serverInfo);
    }

    private async Task SendGDET(Packet request, IDictionary<string, object> serverInfo)
    {
        var responseData = new Dictionary<string, object>
        {
            ["LID"] = request.DataDict["LID"],
            ["GID"] = request.DataDict["GID"],
            ["TID"] = request.DataDict["TID"],

            ["UGID"] = serverInfo["UGID"],
            ["D-AutoBalance"] = serverInfo["D-AutoBalance"],
            ["D-Crosshair"] = serverInfo["D-Crosshair"],
            ["D-FriendlyFire"] = serverInfo["D-FriendlyFire"],
            ["D-KillCam"] = serverInfo["D-KillCam"],
            ["D-Minimap"] = serverInfo["D-Minimap"],
            ["D-MinimapSpotting"] = serverInfo["D-MinimapSpotting"],
            ["D-ServerDescriptionCount"] = 0,
            ["D-ThirdPersonVehicleCameras"] = serverInfo["D-ThirdPersonVehicleCameras"],
            ["D-ThreeDSpotting"] = serverInfo["D-ThreeDSpotting"]
        };

        for (var i = 0; i < 32; i++)
        {
            var pdatId = $"D-pdat{i:D2}";
            var validId = serverInfo.TryGetValue(pdatId, out var pdat);
            if (!validId | string.IsNullOrEmpty(pdat as string)) break;
            responseData.Add(pdatId, pdat!);
        }

        LoggerAccessor.LogDebug("[Arcadia] - TheaterClientHandler-SendGDET Sending GDET to client at {endpoint}", _conn.ClientEndpoint);
        await _conn.SendPacket(new Packet("GDET", TheaterTransmissionType.OkResponse, 0, responseData));
    }

    private async Task SendEGEG(Packet request, long ticket, IDictionary<string, object> serverData)
    {
        string? serverIp = SRVEmuServerConfiguration.GameServerAddress;
        int serverPort = SRVEmuServerConfiguration.GameServerPort;

        LoggerAccessor.LogDebug("[Arcadia] - TheaterClientHandler-SendEGEG Sending EGEG to client at {endpoint}", _conn.ClientEndpoint);

        await _conn.SendPacket(new Packet("EGEG", TheaterTransmissionType.OkResponse, 0, new Dictionary<string, object>
        {
            ["PL"] = "ps3",
            ["TICKET"] = ticket,
            ["PID"] = _sessionCache["UID"],
            ["HUID"] = "1000000000001",
            ["EKEY"] = "AIBSgPFqRDg0TfdXW1zUGa4%3d",
            ["UGID"] = _sessionCache["UGID"],

            ["INT-IP"] = serverIp,
            ["INT-PORT"] = serverPort,

            ["I"] = serverIp,
            ["P"] = serverData["PORT"],

            ["LID"] = request.DataDict["LID"],
            ["GID"] = request.DataDict["GID"]
        }));
    }
}