using System.Collections.Concurrent;

namespace SRVEmu.Arcadia.Storage;

public class SharedCache
{
    private readonly ConcurrentDictionary<string, string> _lkeyUsernames = new();
    private readonly ConcurrentBag<GameServerListing> _gameServers = new();

    public void AddUserWithKey(string lkey, string username)
    {
        _lkeyUsernames.TryAdd(lkey, username);
    }

    public string GetUsernameByLKey(string lkey)
    {
        return _lkeyUsernames[lkey];
    }

    public void UpsertGameServerValueByGid(long serverGid, string key, object value)
    {
        if (serverGid < 1)
        {
            CustomLogger.LoggerAccessor.LogWarn("[Arcadia] - SharedCache-UpsertGameServerValueByGid() : Tried to update server with GID=0");
            return;
        }

        GameServerListing? server = _gameServers.SingleOrDefault(x => x.GameId == serverGid);
        if (server is null)
            throw new NotImplementedException();

        var serverData = server.InfoData;
        if (serverData is null)
            throw new NotImplementedException();

        serverData.Remove(key, out var _);
        serverData.TryAdd(key, value);
    }

    public void UpsertGameServerDataByGid(long serverGid, Dictionary<string, object> data)
    {
        if (serverGid < 1)
        {
            CustomLogger.LoggerAccessor.LogWarn("[Arcadia] - SharedCache-UpsertGameServerDataByGid() : Tried to update server with GID=0");
            return;
        }

        GameServerListing? server = _gameServers.SingleOrDefault(x => x.GameId == serverGid);
        if (server is null)
        {
            _gameServers.Add(new()
            {
                GameId = serverGid,
                InfoData = new ConcurrentDictionary<string, object>(data)
            });

            return;
        }

        var serverData = server.InfoData;
        if (serverData is null)
        {
            server.InfoData = new ConcurrentDictionary<string, object>(data);
            return;
        }

        foreach (var line in data)
        {
            bool removed = serverData.Remove(line.Key, out var _);
            serverData.TryAdd(line.Key, line.Value);
        }
    }

    public IDictionary<string, object>? GetGameServerDataByGid(long serverGid)
    {
        return _gameServers.SingleOrDefault(x => x.GameId == serverGid)?.InfoData;
    }

    public long[] ListServersGIDs()
    {
        return _gameServers.Select(x => x.GameId).ToArray();
    }

    public GameServerListing? GetGameByGid(long serverGid)
    {
        return _gameServers.SingleOrDefault(x => x.GameId == serverGid);
    }
}

public class GameServerListing
{
    public long HostPersonaId { get; set; }
    public long GameId { get; set; }
    public ConcurrentDictionary<string, object> InfoData { get; set; } = new();
}