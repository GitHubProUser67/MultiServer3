using System.Net.Sockets;
using System.Text;
using SRVEmu.Arcadia.EA;
using SRVEmu.Arcadia.EA.Constants;
using SRVEmu.Arcadia.Storage;

namespace SRVEmu.Arcadia.Handlers;

public class TheaterHandler
{
    private NetworkStream _network = null!;
    private string _clientEndpoint = null!;

    private readonly SharedCounters _sharedCounters;
    private readonly SharedCache _sharedCache;

    private readonly Dictionary<string, Func<Packet, Task>> _handlers;

    private readonly Dictionary<string, object> _sessionCache = new();

    private int _brackets = 0;

    public TheaterHandler(SharedCounters sharedCounters, SharedCache sharedCache)
    {
        _sharedCounters = sharedCounters;
        _sharedCache = sharedCache;

        _handlers = new Dictionary<string, Func<Packet, Task>>
        {
            ["CONN"] = HandleCONN,
            ["USER"] = HandleUSER,
            ["CGAM"] = HandleCGAM,
            ["ECNL"] = HandleECNL,
            ["EGAM"] = HandleEGAM,
            ["EGRS"] = HandleEGRS,
            ["PENT"] = HandlePENT,
            ["GDAT"] = HandleGDAT,
            ["UBRA"] = HandleUBRA,
            ["UGAM"] = HandleUGAM,
        };
    }

    public async Task HandleClientConnection(NetworkStream network, string clientEndpoint)
    {
        _network = network;
        _clientEndpoint = clientEndpoint;

        while (_network.CanRead)
        {
            int read;
            byte[] readBuffer = new byte[8096];

            try
            {
                read = await _network.ReadAsync(readBuffer.AsMemory());
            }
            catch
            {
                CustomLogger.LoggerAccessor.LogWarn("[TheatreHandler] - Connection has been closed: {endpoint}", _clientEndpoint);
                break;
            }

            if (read == 0)
            {
                continue;
            }

            Packet packet = new(readBuffer[..read]);
            string type = packet.Type;

            CustomLogger.LoggerAccessor.LogInfo("[TheatreHandler] - Incoming Type: {type}", type);
            CustomLogger.LoggerAccessor.LogInfo("[TheatreHandler] - data:{data}", Encoding.ASCII.GetString(readBuffer[..read]));

            _handlers.TryGetValue(type, out var handler);

            if (handler is null)
            {
                CustomLogger.LoggerAccessor.LogWarn("[TheatreHandler] - Unknown packet type: {type}", type);
                continue;
            }

            await handler(packet);
        }
    }

    private async Task HandleCONN(Packet request)
    {
        object? tid = request.DataDict["TID"];

        CustomLogger.LoggerAccessor.LogInfo("[TheatreHandler] - CONN: {tid}", tid);

        var response = new Dictionary<string, object>
        {
            ["TIME"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["TID"] = tid,
            ["activityTimeoutSecs"] = 0,
            ["PROT"] = request.DataDict["PROT"]
        };

        Packet packet = new("CONN", TheaterTransmissionType.OkResponse, 0, response);
        byte[] data = await packet.Serialize();

        await _network.WriteAsync(data);
    }

    private async Task HandleUSER(Packet request)
    {
        object? lkey = request.DataDict["LKEY"];
        string username = _sharedCache.GetUsernameByKey((string)lkey);

        CustomLogger.LoggerAccessor.LogInfo("[TheatreHandler] - USER: {name} {lkey}", username, lkey);

        var response = new Dictionary<string, object>
        {
            ["NAME"] = username,
            ["TID"] = request.DataDict["TID"]
        };

        Packet packet = new("USER", TheaterTransmissionType.OkResponse, 0, response);
        byte[] data = await packet.Serialize();

        await _network.WriteAsync(data);
    }

    // CreateGame
    private async Task HandleCGAM(Packet request)
    {
        var response = new Dictionary<string, object>
        {
            ["TID"] = request.DataDict["TID"],
            ["MAX-PLAYERS"] = request.DataDict["MAX-PLAYERS"],
            ["EKEY"] = "",
            ["UGID"] = Guid.NewGuid().ToString(),
            ["JOIN"] = request.DataDict["JOIN"],
            ["SECRET"] = "",
            ["LID"] = 255,
            ["J"] = request.DataDict["JOIN"],
            ["GID"] = _sharedCounters.GetNextGameId()
        };

        Packet packet = new("CGAM", TheaterTransmissionType.OkResponse, 0, response);
        byte[] data = await packet.Serialize();

        await _network.WriteAsync(data);
    }

    // LeaveGame
    private async Task HandleECNL(Packet request)
    {
        // !TODO: set gid to a valid game id
        // !TODO: set lid to a valid lobby id

        var response = new Dictionary<string, object>
        {
            ["TID"] = request.DataDict["TID"],
            ["LID"] = request.DataDict["LID"],
            ["GID"] = request.DataDict["GID"],
        };

        Packet packet = new("ECNL", TheaterTransmissionType.OkResponse, 0, response);
        byte[] data = await packet.Serialize();

        await _network.WriteAsync(data);
    }

    // EnterGameRequest
    private async Task HandleEGAM(Packet request)
    {
        var response = new Dictionary<string, object>
        {
            ["TID"] = request.DataDict["TID"],
            ["LID"] = request.DataDict["LID"],
            ["GID"] = request.DataDict["GID"]
        };

        Packet packet = new("EGAM", TheaterTransmissionType.OkResponse, 0, response);
        byte[] data = await packet.Serialize();

        await _network.WriteAsync(data);

        await SendEGEG(request);
    }

    private async Task HandleEGRS(Packet request)
    {
        var serverInfo = new Dictionary<string, object>
        {
            ["TID"] = request.DataDict["TID"]
        };

        Packet packet = new("EGRS", TheaterTransmissionType.OkResponse, 0, serverInfo);
        byte[] data = await packet.Serialize();

        await _network.WriteAsync(data);

        await SendEGEG(request);
    }

    private async Task HandleGDAT(Packet request)
    {
        var serverInfo = new Dictionary<string, object>
        {
            ["JP"] = 1,
            ["B-U-location"] = "nrt",
            ["HN"] = "beach.server.p",
            ["B-U-level"] = "levels/coral_sea",
            ["N"] = "nrtps3313601",
            ["I"] = SRVEmuServerConfiguration.GameServerBindAddress,
            ["J"] = 0,
            ["HU"] = 201104017,
            ["B-U-Time"] = "T%3a0.00 S%3a 6.65 L%3a 0.00",
            ["V"] = "1.0",
            ["B-U-gamemode"] = "CONQUEST",
            ["B-U-trial"] = "RETAIL",
            ["P"] = "38681",
            ["B-U-balance"] = "NORMAL",
            ["B-U-hash"] = "2AC3F219-3614-F46A-843B-A02E03E849E1",
            ["B-numObservers"] = 0,
            ["TYPE"] = "G",
            ["LID"] = request.DataDict["LID"],
            ["B-U-Frames"] = "T%3a 300 B%3a 0",
            ["B-version"] = "RETAIL421378",
            ["QP"] = 0,
            ["MP"] = 24,
            ["B-U-type"] = "RANKED",
            ["B-U-playgroup"] = "YES",
            ["B-U-public"] = "YES",
            ["GID"] = request.DataDict["GID"],
            ["PL"] = "PS3",
            ["B-U-elo"] = 1520,
            ["B-maxObservers"] = 0,
            ["PW"] = 0,
            ["TID"] = request.DataDict["TID"],
            ["B-U-coralsea"] = "YES",
            ["AP"] = 5
        };

        Packet packet = new("GDAT", TheaterTransmissionType.OkResponse, 0, serverInfo);
        byte[] data = await packet.Serialize();
        await _network.WriteAsync(data);

        await SendGDET(request);
    }

    private async Task SendGDET(Packet request)
    {
        _sessionCache["UGID"] = Guid.NewGuid().ToString();

        var serverInfo = new Dictionary<string, object>
        {
            ["LID"] = request.DataDict["LID"],
            ["UGID"] = _sessionCache["UGID"],
            ["GID"] = request.DataDict["GID"],
            ["TID"] = request.DataDict["TID"]
        };

        for (var i = 0; i < 24; i++)
        {
            serverInfo.Add($"D-pdat{i}", "|0|0|0|0");
        }

        Packet packet = new("GDET", TheaterTransmissionType.OkResponse, 0, serverInfo);
        byte[] data = await packet.Serialize();

        CustomLogger.LoggerAccessor.LogInfo("[TheatreHandler] - Sending GDET to client at {endpoint}", _clientEndpoint);
        await _network.WriteAsync(data);
    }

    private async Task HandlePENT(Packet request)
    {
        var serverInfo = new Dictionary<string, object>
        {
            ["TID"] = request.DataDict["TID"],
            ["PID"] = request.DataDict["PID"],
        };

        Packet packet = new("PENT", TheaterTransmissionType.OkResponse, 0, serverInfo);
        byte[] data = await packet.Serialize();

        await _network.WriteAsync(data);
    }

    private async Task HandleUBRA(Packet request)
    {
        if (request["START"] != "1")
        {
            var originalTid = (request.DataDict["TID"] as int?) - (_brackets / 2) ?? 0;
            for (var i = 0; i < _brackets; i++)
            {
                var data = new Dictionary<string, object>
                {
                    ["TID"] = originalTid + i
                };

                var packet = new Packet(request.Type, TheaterTransmissionType.OkResponse, 0, data);
                await _network.WriteAsync(await packet.Serialize());
                Interlocked.Decrement(ref _brackets);
            }
        }
        else
            Interlocked.Add(ref _brackets, 2);
    }

    private Task HandleUGAM(Packet request)
    {
        CustomLogger.LoggerAccessor.LogInfo("[TheatreHandler] - Server is trying to update information");
        return Task.CompletedTask;
    }

    private async Task SendEGEG(Packet request)
    {
        string serverIp = SRVEmuServerConfiguration.GameServerBindAddress;
        int serverPort = SRVEmuServerConfiguration.GameServerPort;

        var serverInfo = new Dictionary<string, object>
        {
            ["PL"] = "ps3",
            ["TICKET"] = _sharedCounters.GetNextTicket(),
            ["PID"] = _sharedCounters.GetNextPid(),
            ["HUID"] = "201104017",
            ["EKEY"] = "",
            ["UGID"] = _sessionCache["UGID"],

            ["INT-IP"] = serverIp,
            ["INT-PORT"] = serverPort,

            ["I"] = serverIp,
            ["P"] = serverPort,

            ["LID"] = request.DataDict["LID"],
            ["GID"] = request.DataDict["GID"]
        };

        Packet packet = new("EGEG", TheaterTransmissionType.OkResponse, 0, serverInfo);
        byte[] data = await packet.Serialize();

        CustomLogger.LoggerAccessor.LogInfo("[TheatreHandler] - Sending EGEG to client at {endpoint}", _clientEndpoint);
        await _network.WriteAsync(data);
    }

    private async Task SendEGRQ()
    {
        string serverIp = SRVEmuServerConfiguration.GameServerBindAddress;
        int serverPort = SRVEmuServerConfiguration.GameServerPort;

        var serverInfo = new Dictionary<string, object>
        {
            ["R-INT-PORT"] = serverPort,
            ["R-INT-IP"] = serverIp,
            ["PORT"] = serverPort,
            ["IP"] = serverIp,
            ["NAME"] = "arcadia-ps3",
            ["PTYPE"] = "P",
            ["TICKET"] = "-479505973",
            ["PID"] = 1,
            ["PID"] = 1,
            ["UID"] = 1000000000000,
            ["LID"] = 255,
            ["GID"] = 801000
        };

        Packet packet = new("EGRQ", TheaterTransmissionType.OkResponse, 0, serverInfo);
        byte[] data = await packet.Serialize();

        CustomLogger.LoggerAccessor.LogInfo("[TheatreHandler] - Sending EGRQ to client at {endpoint}", _clientEndpoint);
        await _network.WriteAsync(data);
    }
}