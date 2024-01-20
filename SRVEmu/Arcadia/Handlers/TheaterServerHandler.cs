using System.Net.Sockets;
using CustomLogger;
using SRVEmu.Arcadia.EA;
using SRVEmu.Arcadia.EA.Constants;
using SRVEmu.Arcadia.Storage;

namespace SRVEmu.Arcadia.Handlers;

public class TheaterServerHandler
{
    private static int joiningPlayers = 0;

    private readonly SharedCounters _sharedCounters;
    private readonly SharedCache _sharedCache;
    private readonly IEAConnection _conn;

    private readonly Dictionary<string, Func<Packet, Task>> _handlers;

    private readonly Dictionary<string, object> _sessionCache = new();

    private int _brackets = 0;

    public TheaterServerHandler(IEAConnection conn, SharedCounters sharedCounters, SharedCache sharedCache)
    {
        _sharedCounters = sharedCounters;
        _sharedCache = sharedCache;
        _conn = conn;

        _handlers = new Dictionary<string, Func<Packet, Task>>
        {
            ["CONN"] = HandleCONN,
            ["USER"] = HandleUSER,
            ["CGAM"] = HandleCGAM,
            ["UBRA"] = HandleUBRA,
            ["UGAM"] = HandleUGAM,
            ["EGRS"] = HandleEGRS,
            ["UGDE"] = HandleUGDE
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
        var packetType = packet.Type;
        _handlers.TryGetValue(packetType, out var handler);

        if (handler is null)
        {
            LoggerAccessor.LogWarn("[Arcadia] - TheaterServerHandler-HandlePacket Unknown packet type: {type}", packetType);
            return;
        }

        LoggerAccessor.LogDebug("[Arcadia] - TheaterServerHandler-HandlePacket Incoming Type: {type}", packetType);
        await handler(packet);
    }

    private async Task HandleCONN(Packet request)
    {
        object? tid = request.DataDict["TID"];

        LoggerAccessor.LogInfo("[Arcadia] - TheaterServerHandler-HandleCONN CONN: {tid}", tid);

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
        object? lkey = request.DataDict["LKEY"];
        string username = _sharedCache.GetUsernameByLKey((string)lkey);

        _sessionCache["NAME"] = username;
        _sessionCache["UID"] = _sharedCounters.GetNextUserId();
        LoggerAccessor.LogInfo("[Arcadia] - TheaterServerHandler-HandleUSER USER: {name} {lkey}", username, lkey);

        await _conn.SendPacket(new Packet("USER", TheaterTransmissionType.OkResponse, 0, new Dictionary<string, object>
        {
            ["NAME"] = username,
            ["TID"] = request.DataDict["TID"]
        }));
    }

    // CreateGame
    private async Task HandleCGAM(Packet request)
    {
        long gid = _sharedCounters.GetNextGameId();
        long lid = _sharedCounters.GetNextLid();
        string ugid = Guid.NewGuid().ToString();

        _sharedCache.UpsertGameServerDataByGid(gid, request.DataDict);
        _sharedCache.UpsertGameServerValueByGid(gid, "UGID", ugid);

        await _conn.SendPacket(new Packet("CGAM", TheaterTransmissionType.OkResponse, 0, new Dictionary<string, object>
        {
            ["TID"] = request["TID"],
            ["MAX-PLAYERS"] = request["MAX-PLAYERS"],
            ["EKEY"] = "AIBSgPFqRDg0TfdXW1zUGa4%3d",
            ["UGID"] = ugid,
            ["JOIN"] = request["JOIN"],
            ["SECRET"] = request["SECRET"],
            ["LID"] = lid,
            ["J"] = request["JOIN"],
            ["GID"] = gid
        }));
    }

    private async Task HandleEGRS(Packet request)
    {
        var serverInfo = new Dictionary<string, object>
        {
            ["TID"] = request.DataDict["TID"],
        };

        if (request["ALLOWED"] == "1")
            Interlocked.Increment(ref joiningPlayers);

        await _conn.SendPacket(new Packet("EGRS", TheaterTransmissionType.OkResponse, 0, serverInfo));
    }

    private async Task HandleUBRA(Packet request)
    {
        if (request["START"] != "1")
        {
            int originalTid = (request.DataDict["TID"] as int?) - (_brackets / 2) ?? 0;
            for (int i = 0; i < _brackets; i++)
            {
                await _conn.SendPacket(new Packet(request.Type, TheaterTransmissionType.OkResponse, 0, new Dictionary<string, object>
                {
                    //TODO: Server responds with unknown if tid=i+1?
                    ["TID"] = request["TID"]
                }));
                Interlocked.Decrement(ref _brackets);
            }
        }
        else
            Interlocked.Add(ref _brackets, 2);
    }

    // UpdateGameDetails
    private Task HandleUGDE(Packet request)
    {
        var gid = long.Parse(request["GID"]);
        LoggerAccessor.LogInfo("[Arcadia] - TheaterServerHandler-HandleUGDE Server GID={gid} updating details!", gid);
        _sharedCache.UpsertGameServerDataByGid(gid, request.DataDict);
        return Task.CompletedTask;
    }

    // UpdateGameData
    private Task HandleUGAM(Packet request)
    {
        var gid = long.Parse(request["GID"]);
        LoggerAccessor.LogInfo("[Arcadia] - TheaterServerHandler-HandleUGAM Server GID={gid} updating data!", gid);
        _sharedCache.UpsertGameServerDataByGid(gid, request.DataDict);
        return Task.CompletedTask;
    }
}