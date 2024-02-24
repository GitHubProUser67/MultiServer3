using CustomLogger;
using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.RT.Models;
using BackendProject.Horizon.LIBRARY.Common;
using BackendProject.Horizon.LIBRARY.Database.Models;
using Horizon.MEDIUS.Medius.Models;
using System.Collections.Concurrent;
using System.Net;
using IChannel = DotNetty.Transport.Channels.IChannel;
using Horizon.MUM;

namespace Horizon.MEDIUS.Medius
{
    public class MediusManager
    {
        public class QuickLookup
        {
            public Dictionary<int, ClientObject> AccountIdToClient = new();
            public Dictionary<string, ClientObject> AccountNameToClient = new();
            public Dictionary<string, ClientObject> AccessTokenToClient = new();
            public Dictionary<string, ClientObject> SessionKeyToClient = new();

            public Dictionary<int, AccountDTO> BuddyInvitationsToClient = new();

            public Dictionary<string, DMEObject> AccessTokenToDmeClient = new();
            public Dictionary<string, DMEObject> SessionKeyToDmeClient = new();


            public Dictionary<int, List<Channel>> AppIdToChannel = new();
            public Dictionary<int, Game> GameIdToGame = new();
            public Dictionary<int, Party> PartyIdToGame = new();

            public Dictionary<int, Clan> ClanIdToClan = new();
            public Dictionary<string, Clan> ClanNameToClan = new();
        }

        private Dictionary<string, int[]> _appIdGroups = new();
        private readonly Dictionary<int, QuickLookup> _lookupsByAppId = new();

        private readonly List<MediusFile> _mediusFiles = new();
        private readonly List<MediusFileMetaData> _mediusFilesToUpdateMetaData = new();

        private readonly ConcurrentQueue<ClientObject> _addQueue = new();

        #region Clients
        public List<ClientObject> GetClients(int appId)
        {
            return _lookupsByAppId.Where(x => GetAppIdsInGroup(appId).Contains(x.Key)).SelectMany(x => x.Value.AccountIdToClient.Select(x => x.Value)).ToList();
        }

        public ClientObject? GetClientByAccountId(int accountId, int appId)
        {
            foreach (int appIdInGroup in GetAppIdsInGroup(appId))
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out QuickLookup? quickLookup))
                {
                    if (quickLookup.AccountIdToClient.TryGetValue(accountId, out ClientObject? result))
                        return result;
                }
            }

            return null;
        }

        public ClientObject? GetClientByAccountName(string accountName, int appId)
        {
            accountName = accountName.ToLower();

            foreach (int appIdInGroup in GetAppIdsInGroup(appId))
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out QuickLookup? quickLookup))
                {
                    if (quickLookup.AccountNameToClient.TryGetValue(accountName, out ClientObject? result))
                        return result;
                }
            }

            return null;
        }

        public ClientObject? GetClientByAccessToken(string? accessToken, int appId)
        {
            if (string.IsNullOrEmpty(accessToken))
                return null;

            foreach (int appIdInGroup in GetAppIdsInGroup(appId))
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out QuickLookup? quickLookup))
                {
                    if (quickLookup.AccessTokenToClient.TryGetValue(accessToken, out ClientObject? result))
                        return result;
                }
            }

            return null;
        }

        public ClientObject? GetClientBySessionKey(string sessionKey, int appId)
        {
            foreach (int appIdInGroup in GetAppIdsInGroup(appId))
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out QuickLookup? quickLookup))
                {
                    if (quickLookup.SessionKeyToDmeClient.TryGetValue(sessionKey, out DMEObject? result))
                        return result;
                }
            }

            return null;
        }

        public DMEObject? GetDmeByAccessToken(string accessToken, int appId)
        {
            foreach (int appIdInGroup in GetAppIdsInGroup(appId))
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out QuickLookup? quickLookup))
                {
                    if (quickLookup.AccessTokenToDmeClient.TryGetValue(accessToken, out DMEObject? result))
                        return result;
                }
            }

            return null;
        }

        public DMEObject? GetDmeBySessionKey(string sessionKey, int appId)
        {
            foreach (int appIdInGroup in GetAppIdsInGroup(appId))
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out QuickLookup? quickLookup))
                {
                    if (quickLookup.SessionKeyToDmeClient.TryGetValue(sessionKey, out DMEObject? result))
                        return result;
                }
            }

            return null;
        }

        public void AddDmeClient(DMEObject dmeClient)
        {
            if (!dmeClient.IsLoggedIn)
            {
                LoggerAccessor.LogError($"Attempting to add DME client {dmeClient} to MediusManager but client has not yet logged in.");
                return;
            }

            if (!_lookupsByAppId.TryGetValue(dmeClient.ApplicationId, out var quickLookup))
                _lookupsByAppId.Add(dmeClient.ApplicationId, quickLookup = new QuickLookup());

            try
            {
                quickLookup.AccessTokenToDmeClient.Add(dmeClient.Token, dmeClient);
                quickLookup.SessionKeyToDmeClient.Add(dmeClient.SessionKey, dmeClient);
            }
            catch (Exception ex)
            {
                // clean up
                if (dmeClient != null)
                {
                    if (dmeClient.Token != null)
                        quickLookup.AccessTokenToDmeClient.Remove(dmeClient.Token);

                    if (dmeClient.SessionKey != null)
                        quickLookup.SessionKeyToDmeClient.Remove(dmeClient.SessionKey);
                }

                LoggerAccessor.LogError($"[DMECLIENT] - An Error was thrown {ex}");
            }
        }

        public void AddClient(ClientObject client)
        {
            if (!client.IsLoggedIn)
            {
                LoggerAccessor.LogError($"Attempting to add {client} to MediusManager but client has not yet logged in.");
                return;
            }

            _addQueue.Enqueue(client);
        }

        #endregion

        #region Games

        public uint GetGameCount(int appId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);
            uint count = 0;

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    lock (quickLookup.GameIdToGame)
                    {
                        count += (uint)quickLookup.GameIdToGame.Count();
                    }
                }
            }

            return count;
        }

        public Game? GetGameByWorldId(string dmeSessionKey, int WorldId)
        {
            if (string.IsNullOrEmpty(dmeSessionKey))
                return null;

            foreach (var lookupByAppId in _lookupsByAppId)
            {
                lock (lookupByAppId.Value.GameIdToGame)
                {
                    Game? game = lookupByAppId.Value.GameIdToGame.FirstOrDefault(x => x.Value?.DMEServer?.SessionKey == dmeSessionKey && x.Value?.WorldID == WorldId).Value;
                    if (game != null)
                        return game;
                }
            }

            return null;
        }

        public Party? GetPartyByWorldId(string dmeSessionKey, int WorldId) // Todo, add worldid property to Party.
        {
            foreach (var lookupByAppId in _lookupsByAppId)
            {
                lock (lookupByAppId.Value.PartyIdToGame)
                {
                    Party? party = lookupByAppId.Value.PartyIdToGame.FirstOrDefault(x => x.Value?.DMEServer?.SessionKey == dmeSessionKey).Value;
                    if (party != null)
                        return party;
                }
            }

            return null;
        }

        public Party? GetPartyAll(string name, int appId)
        {
            foreach (var lookupByAppId in _lookupsByAppId)
            {
                lock (lookupByAppId.Value.PartyIdToGame)
                {
                    Party? party = lookupByAppId.Value.PartyIdToGame.FirstOrDefault(x => x.Value?.ApplicationId == appId && x.Value.PartyName == name).Value;
                    if (party != null)
                        return party;
                }
            }
            return null;
        }

        public Channel? GetWorldByName(string worldName)
        {
            foreach (var lookupByAppId in _lookupsByAppId)
            {
                lock (lookupByAppId.Value.AppIdToChannel)
                {
                    Channel? channel = lookupByAppId.Value.AppIdToChannel.SelectMany(kv => kv.Value) // Flatten all channels
                                      .FirstOrDefault(c => c.Name == worldName); // Find the first channel with matching name
                    if (channel != null)
                        return channel;
                }
            }

            return null;
        }

        public Game? GetGameByName(string? gameName, ClientObject? data)
        {
            if (!string.IsNullOrEmpty(gameName) && data != null && data.CurrentGame != null && data.CurrentGame.GameName == gameName)
                return data.CurrentGame;

            return null;
        }

        public Game? GetGameByGameId(int gameId)
        {
            foreach (var lookupByAppId in _lookupsByAppId)
            {
                lock (lookupByAppId.Value.GameIdToGame)
                {
                    if (lookupByAppId.Value.GameIdToGame.TryGetValue(gameId, out Game? game))
                        return game;
                }
            }

            return null;
        }

        public List<Game> GetGamesByGameIdViaChannelFilter(int gameId)
        {
            List<Channel> channels = new();
            List<Game> Games = new();

            Parallel.ForEach(_appIdGroups.Values, appIds => {
                foreach (int appId in appIds)
                {
                    if (_lookupsByAppId.TryGetValue(appId, out QuickLookup? quickLookup))
                    {
                        lock (quickLookup.AppIdToChannel)
                        {
                            channels.AddRange(quickLookup.AppIdToChannel
                                .Where(pair => pair.Key == appId)
                                .SelectMany(pair => pair.Value)
                                .ToList());
                        }
                    }
                }
            });

            foreach (Channel channel in channels)
            {
                Parallel.ForEach(channel._games, game => {
                    lock (Games)
                    {
                        if (game.MediusWorldId == gameId)
                            Games.Add(game);
                    }
                });
            }

            return Games;
        }

        public List<Game> GetGamesByGameNameViaChannelFilter(string gameName)
        {
            List<Channel> channels = new();
            List<Game> Games = new();

            Parallel.ForEach(_appIdGroups.Values, appIds => {
                foreach (int appId in appIds)
                {
                    if (_lookupsByAppId.TryGetValue(appId, out QuickLookup? quickLookup))
                    {
                        lock (quickLookup.AppIdToChannel)
                        {
                            channels.AddRange(quickLookup.AppIdToChannel
                                .Where(pair => pair.Key == appId)
                                .SelectMany(pair => pair.Value)
                                .ToList());
                        }
                    }
                }
            });

            foreach (Channel channel in channels)
            {
                Parallel.ForEach(channel._games, game => {
                    lock (Games)
                    {
                        if (game.GameName == gameName)
                            Games.Add(game);
                    }
                });
            }

            return Games;
        }

        public Party? GetPartyByPartyId(int partyId)
        {
            foreach (var lookupByAppId in _lookupsByAppId)
            {
                lock (lookupByAppId.Value.PartyIdToGame)
                {
                    if (lookupByAppId.Value.PartyIdToGame.TryGetValue(partyId, out Party? party))
                        return party;
                }
            }

            return null;
        }

        public Party? GetPartyByName(string gameName, ClientObject? data)
        {
            if (data != null && data.CurrentParty != null && data.CurrentParty.PartyName == gameName)
                return data.CurrentParty;

            return null;
        }

        /*
        
        public Game GetGameByGameId(ClientObject client, int gameId)
        {
            foreach (var lookupByAppId in _lookupsByAppId)
            {
                if (client.ApplicationId == 20764 || client.ApplicationId == 20364)
                {
                    lock (lookupByAppId.Value.GameIdToGame)
                    {
                        if (lookupByAppId.Value.GameIdToGame.TryGetValue(gameId, out var game))
                            Logger.Warn($"GameIdToGame {game.Id}");
                        return game;
                    }
                } else if (client.ApplicationId == lookupByAppId.Key) {
                    lock (lookupByAppId.Value.GameIdToGame)
                    {

                        if (lookupByAppId.Value.GameIdToGame.TryGetValue(gameId, out var game))
                            Logger.Warn($"GameIdToGame {game.Id}");
                        return game;
                    }
                }

            }

            return null;
        }
        */
        public async Task AddGame(Game game)
        {
            if (!_lookupsByAppId.TryGetValue(game.ApplicationId, out var quickLookup))
                _lookupsByAppId.Add(game.ApplicationId, quickLookup = new QuickLookup());

            quickLookup.GameIdToGame.Add(game.MediusWorldId, game);
            await HorizonServerConfiguration.Database.CreateGame(game.ToGameDTO());
        }

        public int GetGameCountAppId(int appId)
        {
            if (!_lookupsByAppId.TryGetValue(appId, out var quickLookup))
                _lookupsByAppId.Add(appId, quickLookup = new QuickLookup());

            int gameCount = quickLookup.GameIdToGame.Count;

            return gameCount;
        }

        public IEnumerable<Game> GetGameList(int appId, int pageIndex, int pageSize, IEnumerable<GameListFilter> filters)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            return _lookupsByAppId.Where(x => appIdsInGroup.Contains(x.Key))
                            .SelectMany(x => x.Value.GameIdToGame.Select(x => x.Value))
                            .Where(x => (x.WorldStatus == MediusWorldStatus.WorldActive || x.WorldStatus == MediusWorldStatus.WorldStaging) &&
                                        (filters.Count() == 0 || filters.All(y => y.IsMatch(x))))
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize);
        }

        public IEnumerable<Game> GetGameListAppId(int appId, int pageIndex, int pageSize)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            return _lookupsByAppId.Where(x => appIdsInGroup.Contains(x.Key))
                            .SelectMany(x => x.Value.GameIdToGame.Select(x => x.Value))
                            .Where(x => x.WorldStatus == MediusWorldStatus.WorldActive || x.WorldStatus == MediusWorldStatus.WorldStaging)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize);
        }

        public IEnumerable<Game> GetGameListInLocalChannel(Channel channel, int pageIndex, int pageSize)
        {
            return channel._games.Where(x => x.WorldStatus == MediusWorldStatus.WorldActive || x.WorldStatus == MediusWorldStatus.WorldStaging)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize);
        }

        #region CreateGame
        public async Task CreateGame(ClientObject client, IMediusRequest request)
        {
            if (!_lookupsByAppId.TryGetValue(client.ApplicationId, out var quickLookup))
                _lookupsByAppId.Add(client.ApplicationId, quickLookup = new QuickLookup());

            var appIdsInGroup = GetAppIdsInGroup(client.ApplicationId);
            string? gameName = null;
            Game? game = null;
            Channel? gameChannel = null;
            if (request is MediusCreateGameRequest r)
            {
                gameName = r.GameName;
                if (client.ApplicationId == 23360)
                {
                    gameChannel = new Channel()
                    {
                        MaxPlayers = r.MaxPlayers,
                        MinPlayers = r.MinPlayers,
                        ApplicationId = r.ApplicationID,
                        Name = gameName,
                        Type = ChannelType.Game,
                    };

                    await MediusClass.Manager.AddChannel(gameChannel);
                }
            }
            else if (request is MediusCreateGameRequest1 r1)
                gameName = r1.GameName;

            var existingGames = _lookupsByAppId.Where(x => appIdsInGroup.Contains(client.ApplicationId)).SelectMany(x => x.Value.GameIdToGame.Select(g => g.Value));

            // Ensure the name is unique
            // If the host leaves then we unreserve the name
            if (existingGames.Any(x => x.WorldStatus != MediusWorldStatus.WorldClosed && x.WorldStatus != MediusWorldStatus.WorldInactive && x.GameName == gameName && x.Host != null && x.Host.IsConnected))
            {
                client.Queue(new RT_MSG_SERVER_APP()
                {
                    Message = new MediusCreateGameResponse()
                    {
                        MessageID = request.MessageID,
                        MediusWorldID = -1,
                        StatusCode = MediusCallbackStatus.MediusGameNameExists
                    }
                });
                return;
            }

            // Try to get next free dme server
            // If none exist, return error to clist
            DMEObject? dme = MediusClass.ProxyServer.GetFreeDme(client.ApplicationId);

            if (dme == null)
            {
                client.Queue(new MediusCreateGameResponse()
                {
                    MessageID = request.MessageID,
                    MediusWorldID = -1,
                    StatusCode = MediusCallbackStatus.MediusTransactionTimedOut
                });
                return;
            }

            // Create and add
            try
            {
                if (client.ApplicationId == 23360)
                    game = new Game(client, request, gameChannel, dme);
                else
                    game = new Game(client, request, client.CurrentChannel, dme);

                await AddGame(game);

                // Send create game request to dme server
                dme.Queue(new MediusServerCreateGameWithAttributesRequest()
                {
                    MessageID = new MessageId($"{game.MediusWorldId}-{client.AccountId}-{request.MessageID}-{0}"),
                    MediusWorldUID = (uint)game.MediusWorldId,
                    Attributes = game.Attributes,
                    ApplicationID = client.ApplicationId,
                    MaxClients = game.MaxPlayers
                });
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);

                // Failure adding game for some reason
                client.Queue(new MediusCreateGameResponse()
                {
                    MessageID = request.MessageID,
                    MediusWorldID = -1,
                    StatusCode = MediusCallbackStatus.MediusFail
                });
            }
        }
        #endregion

        #region CreateGame1
        public async Task CreateGame1(ClientObject client, IMediusRequest request)
        {
            if (!_lookupsByAppId.TryGetValue(client.ApplicationId, out var quickLookup))
                _lookupsByAppId.Add(client.ApplicationId, quickLookup = new QuickLookup());

            var appIdsInGroup = GetAppIdsInGroup(client.ApplicationId);
            string? gameName = null;
            if (request is MediusCreateGameRequest1 r)
                gameName = r.GameName;

            var existingGames = _lookupsByAppId.Where(x => appIdsInGroup.Contains(client.ApplicationId)).SelectMany(x => x.Value.GameIdToGame.Select(g => g.Value));

            // Ensure the name is unique
            // If the host leaves then we unreserve the name
            if (existingGames.Any(x => x.WorldStatus != MediusWorldStatus.WorldClosed && x.WorldStatus != MediusWorldStatus.WorldInactive && x.GameName == gameName && x.Host != null && x.Host.IsConnected))
            {
                client.Queue(new RT_MSG_SERVER_APP()
                {
                    Message = new MediusCreateGameResponse()
                    {
                        MessageID = request.MessageID,
                        MediusWorldID = -1,
                        StatusCode = MediusCallbackStatus.MediusGameNameExists
                    }
                });
                return;
            }

            // Try to get next free dme server
            // If none exist, return error to clist
            DMEObject? dme = MediusClass.ProxyServer.GetFreeDme(client.ApplicationId);
            if (dme == null)
            {
                client.Queue(new MediusCreateGameResponse()
                {
                    MessageID = request.MessageID,
                    MediusWorldID = -1,
                    StatusCode = MediusCallbackStatus.MediusTransactionTimedOut
                });
                return;
            }

            // Create and add
            try
            {
                Game game = new(client, request, client.CurrentChannel, dme);
                await AddGame(game);

                // Send create game request to dme server
                dme.Queue(new MediusServerCreateGameWithAttributesRequest()
                {
                    MessageID = new MessageId($"{game.MediusWorldId}-{client.AccountId}-{request.MessageID}-{0}"),
                    MediusWorldUID = (uint)game.MediusWorldId,
                    Attributes = game.Attributes,
                    ApplicationID = client.ApplicationId,
                    MaxClients = game.MaxPlayers
                });
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);

                // Failure adding game for some reason
                client.Queue(new MediusCreateGameResponse()
                {
                    MessageID = request.MessageID,
                    MediusWorldID = -1,
                    StatusCode = MediusCallbackStatus.MediusFail
                });
            }
        }
        #endregion

        #region MatchCreateGame
        public async Task MatchCreateGame(ClientObject client, MediusMatchCreateGameRequest matchCreateGameRequest, IChannel channel)
        {
            if (!_lookupsByAppId.TryGetValue(client.ApplicationId, out var quickLookup))
                _lookupsByAppId.Add(client.ApplicationId, quickLookup = new QuickLookup());

            var appIdsInGroup = GetAppIdsInGroup(client.ApplicationId);
            string? gameName = null;
            if (matchCreateGameRequest is MediusMatchCreateGameRequest r)
                gameName = r.GameName;

            var existingGames = _lookupsByAppId.Where(x => appIdsInGroup.Contains(client.ApplicationId)).SelectMany(x => x.Value.GameIdToGame.Select(g => g.Value));

            // Ensure the name is unique
            // If the host leaves then we unreserve the name
            if (existingGames.Any(x => x.WorldStatus != MediusWorldStatus.WorldClosed && x.WorldStatus != MediusWorldStatus.WorldInactive && x.GameName == gameName && x.Host != null && x.Host.IsConnected))
            {
                client.Queue(new RT_MSG_SERVER_APP()
                {
                    Message = new MediusMatchCreateGameResponse()
                    {
                        MessageID = matchCreateGameRequest.MessageID,
                        MediusWorldID = -1,
                        StatusCode = MediusCallbackStatus.MediusGameNameExists
                    }
                });
                return;
            }

            // P2P Matchmaking.
            if (matchCreateGameRequest.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer)
            {
                // Create and add
                try
                {
                    Game game = new(client, matchCreateGameRequest, client.CurrentChannel, null, client.WorldId)
                    {
                        MaxPlayers = matchCreateGameRequest.MaxPlayers,
                        GameHostType = matchCreateGameRequest.GameHostType,
                        GameName = matchCreateGameRequest.GameName,
                        RequestData = matchCreateGameRequest.RequestData,
                        AppDataSize = matchCreateGameRequest.ApplicationDataSize,
                        AppData = matchCreateGameRequest.ApplicationData
                    };

                    await AddGame(game);

                    // Try to get next free MPS server
                    // If none exist, return error to clist
                    //var dme = MediusStarter.ProxyServer.GetFreeDme(client.ApplicationId);
                    MPS mps = MediusClass.GetMPS();
                    if (mps == null)
                    {
                        client.Queue(new MediusMatchCreateGameResponse()
                        {
                            MessageID = matchCreateGameRequest.MessageID,
                            MediusWorldID = -1,
                            StatusCode = MediusCallbackStatus.MediusTransactionTimedOut
                        });
                        return;
                    }

                    mps.SendServerCreateGameWithAttributesRequestP2P(matchCreateGameRequest.MessageID.ToString(), client.AccountId, game.MediusWorldId, false, game, client);

                    /*
                    client.Queue(new MediusMatchCreateGameResponse()
                    {
                        MessageID = matchCreateGameRequest.MessageID,
                        StatusCode = MediusCallbackStatus.MediusSuccess,
                        MediusWorldID = game.Id,
                        SystemSpecificStatusCode = 0,
                        RequestData = matchCreateGameRequest.RequestData,
                        ApplicationDataSize = matchCreateGameRequest.ApplicationDataSize,
                        ApplicationData = matchCreateGameRequest.ApplicationData,
                    });
                    */
                }
                catch (Exception e)
                {
                    // 
                    LoggerAccessor.LogError(e);

                    // Failure creating match game for some reason
                    client.Queue(new MediusMatchCreateGameResponse()
                    {
                        MessageID = matchCreateGameRequest.MessageID,
                        MediusWorldID = -1,
                        StatusCode = MediusCallbackStatus.MediusMatchGameCreationFailed
                    });
                }
            }
            else
            //DME
            {
                // Try to get next free dme server
                // If none exist, return error to clist
                var dme = MediusClass.ProxyServer.GetFreeDme(client.ApplicationId);
                if (dme == null)
                {
                    client.Queue(new MediusMatchCreateGameResponse()
                    {
                        MessageID = matchCreateGameRequest.MessageID,
                        MediusWorldID = -1,
                        StatusCode = MediusCallbackStatus.MediusTransactionTimedOut
                    });
                    return;
                }

                // Create and add
                try
                {
                    Game game = new(client, matchCreateGameRequest, client.CurrentChannel, dme)
                    {
                        MaxPlayers = matchCreateGameRequest.MaxPlayers,
                        GameHostType = matchCreateGameRequest.GameHostType,
                        GameName = matchCreateGameRequest.GameName
                    };

                    await AddGame(game);

                    game.RequestData = matchCreateGameRequest.RequestData;
                    game.AppDataSize = matchCreateGameRequest.ApplicationDataSize;
                    game.AppData = matchCreateGameRequest.ApplicationData;

                    // Send create game request to dme server
                    dme.Queue(new MediusServerCreateGameWithAttributesRequest()
                    {
                        MessageID = new MessageId($"{game.MediusWorldId}-{client.AccountId}-{matchCreateGameRequest.MessageID}-{0}"),
                        MediusWorldUID = (uint)game.MediusWorldId,
                        Attributes = game.Attributes,
                        ApplicationID = client.ApplicationId,
                        MaxClients = game.MaxPlayers
                    });
                }
                catch (Exception e)
                {
                    // 
                    LoggerAccessor.LogError(e);

                    // Failure adding game for some reason
                    client.Queue(new MediusMatchCreateGameResponse()
                    {
                        MessageID = matchCreateGameRequest.MessageID,
                        MediusWorldID = -1,
                        StatusCode = MediusCallbackStatus.MediusMatchGameCreationFailed
                    });
                }
            }
        }
        #endregion

        #region Create Game P2P

        #region MediusServerCreateGameOnMeRequest / MediusServerCreateGameOnSelfRequest / MediusServerCreateGameOnSelfRequest0
        public async Task CreateGameP2P(ClientObject? client, IMediusRequest request, IChannel channel, DMEObject dme)
        {
            if (client != null && !_lookupsByAppId.TryGetValue(client.ApplicationId, out var quickLookup))
                _lookupsByAppId.Add(client.ApplicationId, quickLookup = new QuickLookup());

            var appIdsInGroup = GetAppIdsInGroup(client.ApplicationId);
            string? gameName = null;
            NetAddressList gameNetAddressList = new();
            int worldId = -1;

            string p2pHostAddress = ((IPEndPoint)channel.RemoteAddress).Address.ToString();
            string p2pHostAddressRemoved = p2pHostAddress.Remove(0, 7);

            if (request is MediusServerCreateGameOnMeRequest r)
            {
                gameName = r.GameName;

                if (r.AddressList.AddressList[0].AddressType == NetAddressType.NetAddressTypeBinaryExternalVport ||
                        r.AddressList.AddressList[1].AddressType == NetAddressType.NetAddressTypeBinaryInternalVport)
                {
                    gameNetAddressList.AddressList[0].IPBinaryBitOne = r.AddressList.AddressList[0].IPBinaryBitOne;
                    gameNetAddressList.AddressList[0].IPBinaryBitTwo = r.AddressList.AddressList[0].IPBinaryBitTwo;
                    gameNetAddressList.AddressList[0].IPBinaryBitThree = r.AddressList.AddressList[0].IPBinaryBitThree;
                    gameNetAddressList.AddressList[0].IPBinaryBitFour = r.AddressList.AddressList[0].IPBinaryBitFour;
                    gameNetAddressList.AddressList[0].BinaryPort = r.AddressList.AddressList[0].BinaryPort;


                    gameNetAddressList.AddressList[1].IPBinaryBitOne = r.AddressList.AddressList[1].IPBinaryBitOne;
                    gameNetAddressList.AddressList[1].IPBinaryBitTwo = r.AddressList.AddressList[1].IPBinaryBitTwo;
                    gameNetAddressList.AddressList[1].IPBinaryBitThree = r.AddressList.AddressList[1].IPBinaryBitThree;
                    gameNetAddressList.AddressList[1].IPBinaryBitFour = r.AddressList.AddressList[1].IPBinaryBitFour;
                    gameNetAddressList.AddressList[1].BinaryPort = r.AddressList.AddressList[1].BinaryPort;

                }
                else
                    gameNetAddressList = r.AddressList;

                worldId = r.WorldID;
            }
            else if (request is MediusServerCreateGameOnSelfRequest r1)
            {
                gameName = r1.GameName;
                gameNetAddressList = r1.AddressList;
                worldId = r1.WorldID;
            }
            else if (request is MediusServerCreateGameOnSelfRequest0 r2)
            {
                gameName = r2.GameName;
                gameNetAddressList = r2.AddressList;
                worldId = r2.WorldID;
            }

            // We make sure a game with the same name not already exist, if so, we tell client about.
            if (client.CurrentChannel != null)
            {
                foreach (Channel SubChannel in client.CurrentChannel.LocalChannels)
                {
                    if (SubChannel.GameCount > 0 && SubChannel._games.Any(game => game.GameName == gameName))
                    {
                        client.Queue(new RT_MSG_SERVER_APP()
                        {
                            Message = new MediusServerCreateGameOnMeResponse()
                            {
                                MessageID = request.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_GAME_NAME_EXISTS,
                                MediusWorldID = -1,
                            }
                        });

                        return;
                    }
                }
            }

            LoggerAccessor.LogDebug("NON-DME SUPPORTED CLIENT**\n  NOT CHECKING FOR FREE DME SERVER");

            // Try to get next free dme server
            // If none exist, return error to clist
            //var dme = Program.ProxyServer.GetFreeDme(client.ApplicationId);
            //svar dme = Program.ProxyServer.();

            //dme.IP = p2pHostAddressRemoved.ToString();
            /*
            if (dme == null)
            {
                client.Queue(new MediusCreateGameResponse()
                {
                    MessageID = request.MessageID,
                    MediusWorldID = -1,
                    StatusCode = MediusCallbackStatus.MediusTransactionTimedOut
                });
                return;
            }
            */

            // Create and add
            try
            {
                Game game = new(client, request, client.CurrentChannel, dme, worldId)
                {
                    //Set game host type to PeerToPeer for those speci
                    GameHostType = MediusGameHostType.MediusGameHostPeerToPeer,
                    ApplicationId = client.ApplicationId,
                    Host = client
                };

                // Join game
                await client.JoinGameP2P(game);
                await game.OnMediusServerCreateGameOnMeRequest(request, game.WorldID.ToString());

                await AddGame(game);

                //Send Success response
                client.Queue(new MediusServerCreateGameOnMeResponse()
                {
                    MessageID = request.MessageID,
                    Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                    MediusWorldID = worldId,
                });
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);

                // Failure adding game for some reason
                client.Queue(new MediusCreateGameResponse()
                {
                    MessageID = request.MessageID,
                    MediusWorldID = -1,
                    StatusCode = MediusCallbackStatus.MediusFail
                });
            }
        }
        #endregion

        #endregion

        #region JoinGameRequest
        public Task JoinGame(ClientObject client, MediusJoinGameRequest request)
        {
            #region Client
            /*
            if (client == null)
            {
                Logger.Warn($"Join Game Request Handler Error: Player is not priviliged [{client}]");
                client.Queue(new MediusJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusGameNotFound
                });
            }
            */
            #endregion
            /*
            if(client.ApplicationId == 10994)
            {
                Logger.Warn("JaKXO: Overriding request GameHostType"); //Change request for CLAN_CHAT in Jak X
                request.GameHostType = MediusGameHostType.MediusGameHostClientServerAuxUDP;
            }
            */
            Game? game = null;
            var gameList = GetGameListAppId(client.ApplicationId, 1, 100); // -1 means any?
            if (request.MediusWorldID == -1)
            {
                if (gameList == null)
                {
                    LoggerAccessor.LogWarn($"[MediusManager] - Join Game Request Handler Error: Error in retrieving game world info from MUM cache [{request.MediusWorldID}]");
                    client.Queue(new MediusJoinGameResponse()
                    {
                        MessageID = request.MessageID,
                        StatusCode = MediusCallbackStatus.MediusGameNotFound
                    });
                }
                else
                    game = gameList.FirstOrDefault();
            }
            else
                game = GetGameByGameId(request.MediusWorldID); // MUM original fetches GameWorldData

            if (game == null)
            {
                LoggerAccessor.LogWarn($"Join Game Request Handler Error: Error in retrieving game world info from MUM cache [{request.MediusWorldID}]");
                client.Queue(new MediusJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusGameNotFound
                });
            }

            #region Password
            else if (game.GamePassword != null && game.GamePassword != string.Empty && game.GamePassword != request.GamePassword)
            {
                LoggerAccessor.LogWarn($"Join Game Request Handler Error: This game's password {game.GamePassword} doesn't match the requested GamePassword {request.GamePassword}");
                client.Queue(new MediusJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusInvalidPassword
                });
            }
            #endregion

            #region MaxPlayers
            else if (game.PlayerCount >= game.MaxPlayers)
            {
                LoggerAccessor.LogWarn($"Join Game Request Handler Error: This game does not allow more than {game.MaxPlayers}. Current player count: {game.PlayerCount}");
                client.Queue(new MediusJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusWorldIsFull
                });
            }
            #endregion

            #region GameHostType check
            else if (request.GameHostType != game.GameHostType)
            {
                LoggerAccessor.LogWarn($"Join Game Request Handler Error: This games HostType {game.GameHostType} does not match the Requests HostType {request.GameHostType}");
                client.Queue(new MediusJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusRequestDenied
                });
            }
            #endregion

            #region JoinType.MediusJoinAsMassSpectator
            else if (request.JoinType == MediusJoinType.MediusJoinAsMassSpectator && (Convert.ToInt32(game.Attributes) & 2) == 0)
                LoggerAccessor.LogWarn($"Join Game Request Handler Error: This game does not allow mass spectators. Attributes: {game.Attributes}");

            #endregion

            else
            {
                //Program.AntiCheatPlugin.mc_anticheat_event_msg(AnticheatEventCode.anticheatJOINGAME, request.MediusWorldID, client.AccountId, Program.AntiCheatClient, request, 4);

                var dme = game.DMEServer;

                // if This is a Peer to Peer Player Host as DME we treat differently
                if (game.GAME_HOST_TYPE == MGCL_GAME_HOST_TYPE.MGCLGameHostPeerToPeer
                    && game.netAddressList?.AddressList[0].AddressType == NetAddressType.NetAddressTypeSignalAddress)
                {

                    game.Host?.Queue(new MediusServerJoinGameRequest()
                    {
                        MessageID = new MessageId($"{game.MediusWorldId}-{client.AccountId}-{request.MessageID}-{0}"),
                        ConnectInfo = new NetConnectionInfo()
                        {
                            Type = NetConnectionType.NetConnectionTypePeerToPeerUDP,
                            AccessKey = client.Token,
                            SessionKey = client.SessionKey,
                            WorldID = game.WorldID,
                            ServerKey = MediusClass.GlobalAuthPublic,
                            AddressList = new NetAddressList()
                            {
                                AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                {
                                    new NetAddress() { Address = request.AddressList.AddressList[0].Address, Port = request.AddressList.AddressList[0].Port, AddressType = NetAddressType.NetAddressTypeSignalAddress},
                                    new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                                }
                            },
                        }
                    });
                }
                else if (game.GAME_HOST_TYPE == MGCL_GAME_HOST_TYPE.MGCLGameHostPeerToPeer
                        && game.netAddressList?.AddressList[0].AddressType == NetAddressType.NetAddressTypeExternal
                        && game.netAddressList?.AddressList[1].AddressType == NetAddressType.NetAddressTypeInternal)
                {

                    game.Host?.Queue(new MediusServerJoinGameRequest()
                    {
                        MessageID = new MessageId($"{game.MediusWorldId}-{client.AccountId}-{request.MessageID}-{0}"),
                        ConnectInfo = new NetConnectionInfo()
                        {
                            Type = NetConnectionType.NetConnectionTypePeerToPeerUDP,
                            AccessKey = client.Token,
                            SessionKey = client.SessionKey,
                            WorldID = game.WorldID,
                            ServerKey = MediusClass.GlobalAuthPublic,
                            AddressList = new NetAddressList()
                            {
                                AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                {
                                    new NetAddress() { Address = request.AddressList.AddressList[0].Address, Port = request.AddressList.AddressList[0].Port, AddressType = NetAddressType.NetAddressTypeExternal},
                                    new NetAddress() { Address = request.AddressList.AddressList[1].Address, Port = request.AddressList.AddressList[1].Port, AddressType = NetAddressType.NetAddressTypeInternal},
                                }
                            },
                        }
                    });

                }

                else if (game.GAME_HOST_TYPE == MGCL_GAME_HOST_TYPE.MGCLGameHostPeerToPeer
                        && game.netAddressList?.AddressList[0].AddressType == NetAddressType.NetAddressTypeBinaryExternalVport
                        && game.netAddressList?.AddressList[1].AddressType == NetAddressType.NetAddressTypeBinaryInternalVport)
                {

                    game.Host?.Queue(new MediusServerJoinGameRequest()
                    {
                        MessageID = new MessageId($"{game.MediusWorldId}-{client.AccountId}-{request.MessageID}"),
                        ConnectInfo = new NetConnectionInfo()
                        {
                            Type = NetConnectionType.NetConnectionTypePeerToPeerUDP,
                            AccessKey = client.Token,
                            SessionKey = client.SessionKey,
                            WorldID = game.WorldID,
                            ServerKey = MediusClass.GlobalAuthPublic,
                            AddressList = new NetAddressList()
                            {
                                AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                                {
                                    new NetAddress() { IPBinaryBitOne = request.AddressList.AddressList[0].IPBinaryBitOne,
                                        IPBinaryBitTwo = request.AddressList.AddressList[0].IPBinaryBitTwo,
                                        IPBinaryBitThree = request.AddressList.AddressList[0].IPBinaryBitThree,
                                        IPBinaryBitFour = request.AddressList.AddressList[0].IPBinaryBitFour,
                                        BinaryPort = request.AddressList.AddressList[0].BinaryPort,
                                        AddressType = NetAddressType.NetAddressTypeBinaryExternalVport},
                                    new NetAddress() { IPBinaryBitOne = request.AddressList.AddressList[1].IPBinaryBitOne,
                                        IPBinaryBitTwo = request.AddressList.AddressList[1].IPBinaryBitTwo,
                                        IPBinaryBitThree = request.AddressList.AddressList[1].IPBinaryBitThree,
                                        IPBinaryBitFour = request.AddressList.AddressList[1].IPBinaryBitFour,
                                        BinaryPort = request.AddressList.AddressList[1].BinaryPort,
                                        AddressType = NetAddressType.NetAddressTypeBinaryInternalVport},
                                }
                            },
                        }
                    });

                }

                // Else send normal Connection type to DME
                else
                {
                    if (client.MediusVersion > 108 && client.ApplicationId != 10994)
                    {
                        dme.Queue(new MediusServerJoinGameRequest()
                        {
                            MessageID = new MessageId($"{game.MediusWorldId}-{client.AccountId}-{request.MessageID}-{0}"),
                            ConnectInfo = new NetConnectionInfo()
                            {
                                Type = NetConnectionType.NetConnectionTypeClientServerTCPAuxUDP,
                                WorldID = game.WorldID,
                                AccessKey = client.Token,
                                SessionKey = client.SessionKey,
                                ServerKey = MediusClass.GlobalAuthPublic
                            }
                        });
                    }
                    else
                    {
                        dme.Queue(new MediusServerJoinGameRequest()
                        {
                            MessageID = new MessageId($"{game.MediusWorldId}-{client.AccountId}-{request.MessageID}-{0}"),
                            ConnectInfo = new NetConnectionInfo()
                            {
                                Type = NetConnectionType.NetConnectionTypeClientServerTCP,
                                WorldID = game.WorldID,
                                AccessKey = client.Token,
                                SessionKey = client.SessionKey,
                                ServerKey = MediusClass.GlobalAuthPublic
                            }
                        });
                    }
                }

            }

            return Task.CompletedTask;
        }
        #endregion

        #region JoinGameRequest0
        public void JoinGame0(ClientObject client, MediusJoinGameRequest0 request)
        {
            Game? game = GetGameByGameId(request.MediusWorldID);
            if (game == null)
            {
                client.Queue(new MediusJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusGameNotFound
                });
            }
            else if (game.GamePassword != null && game.GamePassword != string.Empty && game.GamePassword != request.GamePassword)
            {
                client.Queue(new MediusJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusInvalidPassword
                });
            }
            else if (game.PlayerCount >= game.MaxPlayers)
            {
                client.Queue(new MediusJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusWorldIsFull
                });
            }
            else
            {
                var dme = game.DMEServer;
                // if This is a Peer to Peer Player Host as DME we treat differently
                if (game.GameHostType == MediusGameHostType.MediusGameHostPeerToPeer)
                {
                    game.Host?.Queue(new MediusServerJoinGameRequest()
                    {
                        MessageID = new MessageId($"{game.MediusWorldId}-{client.AccountId}-{request.MessageID}-{0}"),
                        ConnectInfo = new NetConnectionInfo()
                        {
                            Type = NetConnectionType.NetConnectionTypePeerToPeerUDP,
                            WorldID = game.WorldID,
                            AccessKey = client.Token,
                            SessionKey = client.SessionKey,
                            ServerKey = MediusClass.GlobalAuthPublic
                        }
                    });
                }
                // Else send normal Connection type
                else
                {
                    dme.Queue(new MediusServerJoinGameRequest()
                    {
                        MessageID = new MessageId($"{game.MediusWorldId}-{client.AccountId}-{request.MessageID}-{0}"),
                        ConnectInfo = new NetConnectionInfo()
                        {
                            Type = NetConnectionType.NetConnectionTypeClientServerTCP,
                            WorldID = game.WorldID,
                            AccessKey = client.Token,
                            SessionKey = client.SessionKey,
                            ServerKey = MediusClass.GlobalAuthPublic
                        }
                    });
                }
            }
        }
        #endregion

        #endregion

        #region Channels

        public List<Channel> GetAllChannels()
        {
            List<Channel> channels = new();

            Parallel.ForEach(_appIdGroups.Values, appIds => {
                foreach (int appId in appIds)
                {
                    if (_lookupsByAppId.TryGetValue(appId, out QuickLookup? quickLookup))
                    {
                        lock (quickLookup.AppIdToChannel)
                        {
                            channels.AddRange(quickLookup.AppIdToChannel
                                .Where(pair => pair.Key == appId)
                                .SelectMany(pair => pair.Value)
                                .ToList());
                        }
                    }
                }
            });

            return channels;
        }

        public Channel? GetChannelByChannelId(int channelId, int appId)
        {
            foreach (int appIdInGroup in GetAppIdsInGroup(appId))
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out QuickLookup? quickLookup))
                {
                    lock (quickLookup.AppIdToChannel)
                    {
                        Channel? channel = quickLookup.AppIdToChannel.SelectMany(kv => kv.Value).Where(c => c.Id == channelId && c.ApplicationId == appId).FirstOrDefault();
                        if (channel != null)
                            return channel;
                    }
                }
            }

            return null;
        }

        public Channel? GetChannelByChannelName(string channelName, int appId)
        {
            foreach (int appIdInGroup in GetAppIdsInGroup(appId))
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out QuickLookup? quickLookup))
                {
                    lock (quickLookup.AppIdToChannel)
                    {
                        Channel? channel = quickLookup.AppIdToChannel.SelectMany(kv => kv.Value).FirstOrDefault(x => x.Name == channelName && x.ApplicationId == appId);
                        if (channel != null)
                            return channel;
                    }
                }
            }

            return null;
        }

        public Channel? GetChannelByRequestFilter(int appId, ChannelType type, ulong FieldMask1, ulong FieldMask2, ulong FieldMask3, ulong FieldMask4, MediusLobbyFilterMaskLevelType filterMaskLevelType)
        {
            return _lookupsByAppId
                .Where(x => GetAppIdsInGroup(appId).Contains(x.Key))
                .SelectMany(x => x.Value.AppIdToChannel.SelectMany(x => x.Value))
                .Where(x => x.Type == type &&
                    x.ApplicationId == appId &&
                    x.GenericField1 == FieldMask1 &&
                    x.GenericField2 == FieldMask2 &&
                    x.GenericField3 == FieldMask3 &&
                    x.GenericField4 == FieldMask4 &&
                    x.GenericFieldLevel == (MediusWorldGenericFieldLevelType)filterMaskLevelType)
                .First();
        }

        public uint GetChannelCount(ChannelType type, int appId)
        {
            uint count = 0;

            foreach (int appIdInGroup in GetAppIdsInGroup(appId))
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out QuickLookup? quickLookup))
                {
                    lock (quickLookup.AppIdToChannel)
                    {
                        count += (uint)quickLookup.AppIdToChannel.SelectMany(kv => kv.Value).Count(x => x.Type == type && x.ApplicationId == appId);
                    }
                }
            }

            return count;
        }

        public Channel GetOrCreateDefaultLobbyChannel(int appId)
        {
            Channel? channel = null;

            foreach (int appIdInGroup in GetAppIdsInGroup(appId))
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out QuickLookup? quickLookup))
                {
                    lock (quickLookup.AppIdToChannel)
                    {
                        //, x => x.Value.Type == ChannelType.Lobby
                        channel = quickLookup.AppIdToChannel.SelectMany(kv => kv.Value).FirstOrDefault(x => x.ApplicationId == appId);
                        if (channel != null)
                            return channel;
                    }
                }
            }

            // create default
            channel = new Channel()
            {
                ApplicationId = appId,
                Name = "Default",
                Type = ChannelType.Lobby
            };

            _ = AddChannel(channel);

            return channel;
        }

        public async Task AddChannel(Channel channel)
        {
            if (!_lookupsByAppId.TryGetValue(channel.ApplicationId, out QuickLookup? quickLookup))
                _lookupsByAppId.Add(channel.ApplicationId, quickLookup = new QuickLookup());

            lock (quickLookup.AppIdToChannel)
            {
                if (!quickLookup.AppIdToChannel.ContainsKey(channel.ApplicationId))
                    quickLookup.AppIdToChannel.Add(channel.ApplicationId, new List<Channel>() { channel });
                else
                    quickLookup.AppIdToChannel[channel.ApplicationId].Add(channel);
            }

            await channel.OnChannelCreate(channel);
        }

        /// <summary>
        /// Filter Worlds by AppId, and if set by client WorldFilters
        /// </summary>
        /// <param name="appId">ApplicationId</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="type"></param>
        /// <param name="FieldMask1"></param>
        /// <param name="FieldMask2"></param>
        /// <param name="FieldMask3"></param>
        /// <param name="FieldMask4"></param>
        /// <param name="filterMaskLevelType"></param>
        /// <returns></returns>
        public IEnumerable<Channel> GetChannelListFiltered(int appId, int pageIndex, int pageSize, ChannelType type, ulong FieldMask1, ulong FieldMask2, ulong FieldMask3, ulong FieldMask4, MediusLobbyFilterMaskLevelType filterMaskLevelType)
        {
            return _lookupsByAppId
                .Where(x => GetAppIdsInGroup(appId).Contains(x.Key))
                .SelectMany(x => x.Value.AppIdToChannel.SelectMany(x => x.Value))
                .Where(x => x.Type == type &&
                    x.ApplicationId == appId &&
                    x.GenericField1 == FieldMask1 &&
                    x.GenericField2 == FieldMask2 &&
                    x.GenericField3 == FieldMask3 &&
                    x.GenericField4 == FieldMask4 &&
                    x.GenericFieldLevel == (MediusWorldGenericFieldLevelType)filterMaskLevelType)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);
        }

        /// <summary>
        /// Filter Worlds by AppId
        /// </summary>
        /// <param name="appId">ApplicationId</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<Channel> GetChannelListUnfiltered(int appId, int pageIndex, int pageSize)
        {
            return _lookupsByAppId
                .Where(x => GetAppIdsInGroup(appId).Contains(x.Key))
                .SelectMany(x => x.Value.AppIdToChannel.SelectMany(x => x.Value))
                .Where(x => x.ApplicationId == appId)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);
        }

        /// <summary>
        /// Filter Worlds by AppId
        /// </summary>
        /// <param name="appId">ApplicationId</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<Channel> GetChannelList(int appId, int pageIndex, int pageSize, ChannelType type)
        {
            return _lookupsByAppId
                .Where(x => GetAppIdsInGroup(appId).Contains(x.Key))
                .SelectMany(x => x.Value.AppIdToChannel.SelectMany(x => x.Value))
                .Where(x => x.Type == type &&
                    x.ApplicationId == appId)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);
        }

        /// <summary>
        /// Filter Worlds by AppId
        /// </summary>
        /// <param name="appId">ApplicationId</param>
        /// <returns></returns>
        public Channel GetChannelLeastPoplated(int appId)
        {
            return _lookupsByAppId
                .Where(x => GetAppIdsInGroup(appId).Contains(x.Key))
                .SelectMany(x => x.Value.AppIdToChannel.SelectMany(x => x.Value))
                .Where(x => x.ApplicationId == appId).OrderBy(kvp => kvp.PlayerCount).First();
        }
        #endregion

        #region Party

        public async Task joinParty(ClientObject client, MediusPartyJoinByIndexRequest request, IChannel channel)
        {
            #region Client
            /*
            if (client == null)
            {
                Logger.Warn($"Join Game Request Handler Error: Player is not priviliged [{client}]");
                client.Queue(new MediusJoinGameResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusGameNotFound
                });
            }
            */
            #endregion

            Party? party = GetPartyByPartyId(request.MediusWorldID); // MUM original fetches GameWorldData
            if (party == null)
            {
                LoggerAccessor.LogWarn($"Join Game Request Handler Error: Error in retrieving party info from MUM cache [{request.MediusWorldID}]");
                
                /*
                client.Queue(new MediusPartyJoinByIndexResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusNoResult
                });
                */
            }

            #region Password
            else if (party.PartyPassword != null && party.PartyPassword != string.Empty && party.PartyPassword != request.PartyPassword)
            {
                LoggerAccessor.LogWarn($"Join Game Request Handler Error: This party's password {party.PartyPassword} doesn't match the requested party Password {request.PartyPassword}");
                client.Queue(new MediusPartyJoinByIndexResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusInvalidPassword
                });
            }
            #endregion
            /*
            #region MaxPlayers
            else if (party.PlayerCount >= party.MaxPlayers)
            {
                Logger.Warn($"Join Game Request Handler Error: This game does not allow more than {party.MaxPlayers}. Current player count: {party.PlayerCount}");
                client.Queue(new MediusPartyJoinByIndexResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusWorldIsFull
                });
            }
            #endregion
            */
            #region GameHostType check
            else if (request.PartyHostType != party.PartyHostType)
            {
                LoggerAccessor.LogWarn($"PartyJoinByIndex Request Handler Error: This party's HostType {party.PartyHostType} does not match the Requests HostType {request.PartyHostType}");
                client.Queue(new MediusPartyJoinByIndexResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusRequestDenied
                });
            }
            #endregion

            else
            {
                var dme = party.DMEServer;

                // Join game DME
                await client.JoinParty(party, party.MediusWorldId);

                dme.Queue(new MediusServerJoinGameRequest()
                {
                    MessageID = new MessageId($"{party.MediusWorldId}-{client.AccountId}-{request.MessageID}-{1}"),
                    ConnectInfo = new NetConnectionInfo()
                    {
                        Type = NetConnectionType.NetConnectionTypeClientServerTCPAuxUDP,
                        WorldID = party.WorldID,
                        AccessKey = client.Token,
                        SessionKey = client.SessionKey,
                        ServerKey = request.pubKey
                    }
                });

                /* RESPONSE FOR MPS
                client?.Queue(new MediusPartyJoinByIndexResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusSuccess,
                    PartyHostType = party.PartyHostType,
                    ConnectionInfo = new NetConnectionInfo()
                    {
                        AccessKey = client.Token,
                        SessionKey = client.SessionKey,
                        WorldID = MediusClass.Manager.GetOrCreateDefaultLobbyChannel(client.ApplicationId).Id,
                        ServerKey = new RSA_KEY(),
                        AddressList = new NetAddressList()
                        {
                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                            {
                                new NetAddress() { Address = MediusClass.SERVER_IP.MapToIPv4().ToString(), Port = dme.Port, AddressType = NetAddressType.NetAddressTypeExternal },
                                new NetAddress() { AddressType = NetAddressType.NetAddressNone },
                            }
                        },
                        Type = NetConnectionType.NetConnectionTypeClientServerTCPAuxUDP
                    },
                    partyIndex = party.Id,
                    maxPlayers = party.MaxPlayers
                });
                */
                /*
                var p2pHostAddress = (channel as IPEndPoint).Address.ToString();
                var p2pHostPort = (channel as IPEndPoint).Port.ToString();
                string p2pHostAddressRemoved = p2pHostAddress.Remove(0, 7);


                // Join game DME
                client.JoinParty(party, party.Id);

                // 
                client?.Queue(new MediusPartyJoinByIndexResponse()
                {
                    MessageID = request.MessageID,
                    StatusCode = MediusCallbackStatus.MediusSuccess,
                    PartyHostType = party.PartyHostType,
                    ConnectionInfo = new NetConnectionInfo()
                    {
                        AccessKey = client.Token,
                        SessionKey = client.SessionKey,
                        WorldID = MediusClass.Manager.GetOrCreateDefaultLobbyChannel(client.ApplicationId).Id,
                        ServerKey = MediusClass.GlobalAuthPublic,
                        AddressList = new NetAddressList()
                        {
                            AddressList = new NetAddress[Constants.NET_ADDRESS_LIST_COUNT]
                            {
                                new NetAddress() { Address = p2pHostAddressRemoved, Port = Convert.ToUInt32(p2pHostPort), AddressType = NetAddressType.NetAddressTypeExternal},
                                new NetAddress() { AddressType = NetAddressType.NetAddressNone},
                            }
                        },
                        Type = NetConnectionType.NetConnectionTypeClientServerUDP
                    },
                    partyIndex = party.Id,
                    maxPlayers = party.MaxPlayers
                });
                */
            }
        }

        public async Task AddParty(Party party)
        {
            if (!_lookupsByAppId.TryGetValue(party.ApplicationId, out var quickLookup))
                _lookupsByAppId.Add(party.ApplicationId, quickLookup = new QuickLookup());

            quickLookup.PartyIdToGame.Add(party.MediusWorldId, party);
            await HorizonServerConfiguration.Database.CreateParty(party.ToPartyDTO());
        }

        public async Task CreateParty(ClientObject client, IMediusRequest request)
        {
            if (!_lookupsByAppId.TryGetValue(client.ApplicationId, out var quickLookup))
                _lookupsByAppId.Add(client.ApplicationId, quickLookup = new QuickLookup());

            var appIdsInGroup = GetAppIdsInGroup(client.ApplicationId);
            string? partyName = null;
            if (request is MediusPartyCreateRequest r)
                partyName = r.PartyName;

            var existingParties = _lookupsByAppId.Where(x => appIdsInGroup.Contains(client.ApplicationId)).SelectMany(x => x.Value.PartyIdToGame.Select(g => g.Value));
            /*
            // Ensure the name is unique
            // If the host leaves then we unreserve the name
            if (existingGames.Any(x => x.WorldStatus != MediusWorldStatus.WorldClosed && x.WorldStatus != MediusWorldStatus.WorldInactive && x. == partyName && x.Host != null && x.Host.IsConnected))
            {
                client.Queue(new RT_MSG_SERVER_APP()
                {
                    Message = new MediusCreateGameResponse()
                    {
                        MessageID = request.MessageID,
                        MediusWorldID = -1,
                        StatusCode = MediusCallbackStatus.MediusGameNameExists
                    }
                });
                return;
            }

            // Try to get next free dme server
            // If none exist, return error to clist
            var dme = Program.ProxyServer.GetFreeDme(client.ApplicationId);
            if (dme == null)
            {
                client.Queue(new MediusCreateGameResponse()
                {
                    MessageID = request.MessageID,
                    MediusWorldID = -1,
                    StatusCode = MediusCallbackStatus.MediusTransactionTimedOut
                });
                return;
            }
            */
            // Create and add

            // Try to get next free dme server
            // If none exist, return error to clist

            var dme = MediusClass.ProxyServer.GetFreeDme(client.ApplicationId);
            MPS mps = MediusClass.GetMPS();

            if (dme == null)
            {
                client.Queue(new MediusPartyCreateResponse()
                {
                    MessageID = request.MessageID,
                    MediusWorldID = -1,
                    StatusCode = MediusCallbackStatus.MediusTransactionTimedOut
                });
                return;
            }

            // Create and add
            try
            {
                Party? party = new(client, request, client.CurrentChannel, dme);
                await AddParty(party);

                await client.JoinParty(party, party.MediusWorldId);

                if (mps == null)
                {
                    client.Queue(new MediusPartyCreateResponse()
                    {
                        MessageID = request.MessageID,
                        MediusWorldID = party.MediusWorldId,
                        StatusCode = MediusCallbackStatus.MediusFail
                    });
                    return;
                }
                /*
                //TEMP
                client.Queue(new MediusPartyCreateResponse()
                {
                    MessageID = request.MessageID,
                    MediusWorldID = -1,
                    StatusCode = MediusCallbackStatus.MediusTransactionTimedOut
                });
                */
                mps.SendServerCreateGameWithAttributesRequest(request.MessageID.ToString(), client.AccountId, party.MediusWorldId, true, (int)party.Attributes, client.ApplicationId, party.MaxPlayers);
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);

                // Failure adding game for some reason
                client.Queue(new MediusPartyCreateResponse()
                {
                    MessageID = request.MessageID,
                    MediusWorldID = -1,
                    StatusCode = MediusCallbackStatus.MediusFail
                });
            }
        }

        #endregion

        #region MFS
        public IEnumerable<MediusFile> GetFilesList(string path, string filenameBeginsWith, uint pageSize, uint startingEntryNumber, int appId)
        {
            lock (_mediusFiles)
            {

                string[]? files = null;
                int counter = 0;
                /*
                if (appId == 11203 || appId == 10994 || appId == 11204)
                {

                    string JakXPath = path + filenameBeginsWith.Split("*");
                } else
                {

                }
                */
                if (filenameBeginsWith.ToString() == "*")
                {
                    files = Directory.GetFiles(path).Select(file => Path.GetFileName(file)).ToArray();
                }
                else
                {
                    files = Directory.GetFiles(path, Convert.ToString(filenameBeginsWith));
                }

                if (files.Length < pageSize)
                {
                    counter = files.Count();
                }
                else
                {
                    counter = (int)pageSize - 1;
                }

                for (int i = (int)startingEntryNumber - 1; i < counter; i++)
                {
                    string fileName = files[i];
                    FileInfo fi = new FileInfo(fileName);

                    try
                    {
                        _mediusFiles.Add(new MediusFile()
                        {
                            FileName = files[i],
                            FileID = (int)i,
                            FileSize = (int)fi.Length,
                            CreationTimeStamp = (int)Utils.ToUnixTime(fi.CreationTime),
                        });
                    }
                    catch (Exception e)
                    {
                        LoggerAccessor.LogWarn($"MFS FileList Exception:\n{e}");
                    }
                }
                return _mediusFiles;
            }
        }

        public IEnumerable<MediusFile> GetFilesListExt(string path, string filenameBeginsWith, uint pageSize, uint startingEntryNumber, int appId)
        {
            lock (_mediusFiles)
            {
                if (startingEntryNumber == 0)
                    return _mediusFiles;

                int counter = 0;

                //JakX FileNameBeginsWith requires extra handling for folders
                if (appId == 11203 || appId == 10994 || appId == 11204)
                {

                    string JakXPath = path + filenameBeginsWith.Split("*");

                    string[] filesArray = Directory.GetFiles(JakXPath);

                    //files = Directory.GetFiles(path).Select(file => Path.GetFileName(filenameBeginsWith)).ToArray();

                    if (filesArray.Length < pageSize)
                    {
                        counter = filesArray.Count() - 1;
                    }
                    else
                    {
                        counter = (int)pageSize - 1;
                    }

                    for (int i = (int)(startingEntryNumber - 1); i < counter; i++)
                    {
                        //string[] pathArray = path.TrimStart('[').TrimEnd(']').Split(',');

                        //string[] fileName = filesArray[i].Split(path, path.Length, options: StringSplitOptions.None);
                        //string FileNameAppended = UsingStringJoin(fileName);

                        try
                        {
                            string fileName = filesArray[i];
                            FileInfo fi = new FileInfo(fileName);

                            LoggerAccessor.LogInfo($"Medius Files Count: {_mediusFiles.Count}");

                            _mediusFiles.Add(new MediusFile()
                            {
                                FileName = fileName.ToString(),
                                FileID = i,
                                FileSize = (int)fi.Length,
                                CreationTimeStamp = (int)Utils.ToUnixTime(fi.CreationTime),
                            });
                        }
                        catch (Exception e)
                        {
                            LoggerAccessor.LogWarn($"MFS FileListExt Exception:\n{e}");
                        }
                    }
                    return _mediusFiles;
                }
                else
                {

                    string filenameBeginsWithAppended = filenameBeginsWith.Remove(filenameBeginsWith.Length - 1);


                    string[] filesArray = Directory.GetFiles(path);

                    //files = Directory.GetFiles(path).Select(file => Path.GetFileName(filenameBeginsWith)).ToArray();

                    if (filesArray.Length < pageSize)
                    {
                        counter = filesArray.Count() - 1;
                    }
                    else
                    {
                        counter = (int)pageSize - 1;
                    }

                    for (int i = (int)(startingEntryNumber - 1); i < counter; i++)
                    {
                        //string[] pathArray = path.TrimStart('[').TrimEnd(']').Split(',');

                        //string[] fileName = filesArray[i].Split(path, path.Length, options: StringSplitOptions.None);
                        //string FileNameAppended = UsingStringJoin(fileName);

                        try
                        {
                            string fileName = filesArray[i];
                            FileInfo fi = new FileInfo(fileName);

                            _mediusFiles.Where(x => x.FileName == fileName.StartsWith(filenameBeginsWithAppended).ToString()).ToList();

                            /*
                            _mediusFiles.Add(new MediusFile()
                            {
                                FileName = filenameBeginsWithAppended.ToString(),
                                FileID = (uint)i,
                                FileSize = (uint)fi.Length,
                                CreationTimeStamp = Utils.ToUnixTime(fi.CreationTime),
                            });
                            */
                        }
                        catch (Exception e)
                        {
                            LoggerAccessor.LogWarn($"MFS FileListExt Exception:\n{e}");
                        }
                    }
                    return _mediusFiles;
                }


            }
        }

        public IEnumerable<MediusFileMetaData> UpdateFileMetaData(string path, int appId, MediusFile mediusFile, MediusFileMetaData mediusFileMetaData)

        {
            lock (_mediusFilesToUpdateMetaData)
            {

                /*
                if (filename.ToString() != null)
                {
                    files = Directory.GetFiles(path).Select(file => Path.GetFileName(file)).ToArray();
                }
                else
                {
                    files = Directory.GetFiles(path, Convert.ToString(filename));
                }
                */
                try
                {
                    _mediusFilesToUpdateMetaData.Add(new MediusFileMetaData()
                    {
                        Key = mediusFileMetaData.Key,
                        Value = mediusFileMetaData.Value,
                    });
                }
                catch (Exception e)
                {
                    LoggerAccessor.LogError($"[MFS] - UpdateMetaData Exception:\n{e}");
                }

                return _mediusFilesToUpdateMetaData;
            }
        }

        public Task UploadMediusFile(MediusFileUploadResponse fileUploadResponse, ClientObject clientObject)
        {

            var uploadState = clientObject.Upload;

            if (fileUploadResponse.iXferStatus >= MediusFileXferStatus.End)
                return Task.CompletedTask;

            try
            {
                LoggerAccessor.LogInfo($"[MFS] - Bytes Received Total [{uploadState.BytesReceived}] < [{uploadState.TotalSize}]");
                uploadState.Stream.Seek(fileUploadResponse.iStartByteIndex, SeekOrigin.Begin);
                uploadState.Stream.Write(fileUploadResponse.Data, 0, fileUploadResponse.iDataSize);
                uploadState.BytesReceived += fileUploadResponse.iDataSize;
                uploadState.PacketNumber++;

                if (uploadState.BytesReceived < uploadState.TotalSize)
                {
                    clientObject.Queue(new MediusFileUploadServerRequest()
                    {
                        MessageID = fileUploadResponse.MessageID,
                        StatusCode = MediusCallbackStatus.MediusSuccess,
                        iPacketNumber = uploadState.PacketNumber,
                        iReqStartByteIndex = uploadState.BytesReceived,
                        iXferStatus = MediusFileXferStatus.Mid
                    });
                }
                else
                {
                    clientObject.Queue(new MediusFileUploadServerRequest()
                    {
                        MessageID = fileUploadResponse.MessageID,
                        StatusCode = MediusCallbackStatus.MediusSuccess,
                        iPacketNumber = uploadState.PacketNumber,
                        iReqStartByteIndex = uploadState.BytesReceived,
                        iXferStatus = MediusFileXferStatus.End
                    });
                }
            }
            catch
            {
                clientObject.Queue(new MediusFileUploadServerRequest()
                {
                    MessageID = fileUploadResponse.MessageID,
                    StatusCode = MediusCallbackStatus.MediusFileInternalAccessError,
                    iPacketNumber = uploadState.PacketNumber,
                    iReqStartByteIndex = uploadState.BytesReceived,
                    iXferStatus = MediusFileXferStatus.Error
                });
            }

            return Task.CompletedTask;
        }
        #endregion

        #region Buddies

        public List<AccountDTO> AddToBuddyInvitations(int appId, AccountDTO accountToAdd)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            if (!_lookupsByAppId.TryGetValue(appId, out var quickLookup))
                _lookupsByAppId.Add(appId, quickLookup = new QuickLookup());

            lock (quickLookup.BuddyInvitationsToClient)
            {
                quickLookup.BuddyInvitationsToClient.Add(accountToAdd.AccountId, accountToAdd);
            }

            return _lookupsByAppId.Where(x => appIdsInGroup.Contains(x.Key))
                .SelectMany(x => x.Value.BuddyInvitationsToClient.Select(x => x.Value))
                .ToList();
        }

        public List<AccountDTO> GetBuddyInvitations(int appId, int AccountId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            return _lookupsByAppId.Where(x => appIdsInGroup.Contains(x.Key))
                .SelectMany(x => x.Value.BuddyInvitationsToClient.Select(x => x.Value))
                .Where(x => x.AccountId == AccountId)
                .ToList();
        }

        #endregion

        #region Clans

        //public Clan GetClanByAccountId(int clanId, int appId)
        //{
        //    if (_clanIdToClan.TryGetValue(clanId, out var result))
        //        return result;

        //    return null;
        //}

        //public Clan GetClanByAccountName(string clanName, int appId)
        //{
        //    clanName = clanName.ToLower();
        //    if (_clanNameToClan.TryGetValue(clanName, out var result))
        //        return result;

        //    return null;
        //}

        //public void AddClan(Clan clan)
        //{
        //    if (!_lookupsByAppId.TryGetValue(clan.ApplicationId, out var quickLookup))
        //        _lookupsByAppId.Add(dmeClient.ApplicationId, quickLookup = new QuickLookup());

        //    _clanNameToClan.Add(clan.Name.ToLower(), clan);
        //    _clanIdToClan.Add(clan.Id, clan);
        //}

        #endregion

        #region Tick

        public async Task Tick()
        {
            await TickClients();

            await TickChannels();

            await TickGames();
        }

        private async Task TickChannels()
        {
            try
            {
                Queue<(QuickLookup, int)> channelsToRemove = new();

                // Tick channels
                foreach (var quickLookup in _lookupsByAppId)
                {
                    foreach (var channelKeyPair in quickLookup.Value.AppIdToChannel)
                    {
                        foreach (Channel channel in channelKeyPair.Value)
                        {
                            if (channel.ReadyToDestroy)
                            {
                                LoggerAccessor.LogInfo($"Destroying Channel {channelKeyPair.Value}");
                                channelsToRemove.Enqueue((quickLookup.Value, channelKeyPair.Key));
                            }
                            else
                                await channel.Tick();
                        }
                    }
                }

                // Remove channels
                while (channelsToRemove.TryDequeue(out (QuickLookup, int) lookupAndChannelApplicationId))
                    lookupAndChannelApplicationId.Item1.AppIdToChannel.Remove(lookupAndChannelApplicationId.Item2);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[MediusManager] - Error in TickChannels {ex}");
            }
        }

        private async Task TickGames()
        {
            try
            {
                Queue<(QuickLookup, int)> gamesToRemove = new Queue<(QuickLookup, int)>();

                // Tick games
                foreach (var quickLookup in _lookupsByAppId)
                {
                    int appid = quickLookup.Key;
                    foreach (var gameKeyPair in quickLookup.Value.GameIdToGame)
                    {
                        if (gameKeyPair.Value.ReadyToDestroy)
                        {
                            LoggerAccessor.LogInfo($"Destroying Game {gameKeyPair.Value}");
                            await gameKeyPair.Value.EndGame(appid);
                            gamesToRemove.Enqueue((quickLookup.Value, gameKeyPair.Key));
                        }
                        else
                        {
                            await gameKeyPair.Value.Tick();
                        }
                    }
                }

                // Remove games
                while (gamesToRemove.TryDequeue(out var lookupAndGameId))
                    lookupAndGameId.Item1.GameIdToGame.Remove(lookupAndGameId.Item2);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[MediusManager] - Error in TickGames {ex}");
            }
        }

        private async Task TickClients()
        {
            try
            {
                Queue<(int, string)> clientsToRemove = new();

                while (_addQueue.TryDequeue(out ClientObject? newClient))
                {
                    if (!_lookupsByAppId.TryGetValue(newClient.ApplicationId, out QuickLookup? quickLookup))
                        _lookupsByAppId.Add(newClient.ApplicationId, quickLookup = new QuickLookup());

                    try
                    {
                        if (!string.IsNullOrEmpty(newClient.AccountName) &&
                            !string.IsNullOrEmpty(newClient.Token) &&
                            !string.IsNullOrEmpty(newClient.SessionKey) &&
                            !quickLookup.AccountIdToClient.ContainsKey(newClient.AccountId) &&
                            !quickLookup.AccountNameToClient.ContainsKey(newClient.AccountName.ToLower()) &&
                            !quickLookup.AccessTokenToClient.ContainsKey(newClient.Token) &&
                            !quickLookup.SessionKeyToClient.ContainsKey(newClient.SessionKey))
                        {
                            quickLookup.AccountIdToClient.Add(newClient.AccountId, newClient);
                            quickLookup.AccountNameToClient.Add(newClient.AccountName.ToLower(), newClient);
                            quickLookup.AccessTokenToClient.Add(newClient.Token, newClient);
                            quickLookup.SessionKeyToClient.Add(newClient.SessionKey, newClient);
                        }
                    }
                    catch (Exception e)
                    {
                        // clean up
                        if (newClient != null)
                        {
                            quickLookup.AccountIdToClient.Remove(newClient.AccountId);

                            if (newClient.AccountName != null)
                                quickLookup.AccountNameToClient.Remove(newClient.AccountName.ToLower());

                            if (newClient.Token != null)
                                quickLookup.AccessTokenToClient.Remove(newClient.Token);

                            if (newClient.SessionKey != null)
                                quickLookup.SessionKeyToClient.Remove(newClient.SessionKey);
                        }

                        LoggerAccessor.LogError(e);
                    }
                }

                foreach (var quickLookup in _lookupsByAppId)
                {
                    foreach (var clientKeyPair in quickLookup.Value.SessionKeyToClient)
                    {
                        if (!clientKeyPair.Value.IsConnected)
                        {
                            if (clientKeyPair.Value.Timedout)
                                LoggerAccessor.LogWarn($"Timing out client {clientKeyPair.Value}");
                            else
                                LoggerAccessor.LogInfo($"Destroying Client {clientKeyPair.Value}");

                            // Logout and end session
                            await clientKeyPair.Value.Logout();
                            clientKeyPair.Value.EndSession();

                            clientsToRemove.Enqueue((quickLookup.Key, clientKeyPair.Key));
                        }
                    }
                }

                // Remove
                while (clientsToRemove.TryDequeue(out var appIdAndSessionKey))
                {
                    if (_lookupsByAppId.TryGetValue(appIdAndSessionKey.Item1, out var quickLookup))
                    {
                        if (quickLookup.SessionKeyToClient.Remove(appIdAndSessionKey.Item2, out var clientObject))
                        {
                            quickLookup.AccountIdToClient.Remove(clientObject.AccountId);
                            quickLookup.AccessTokenToClient.Remove(clientObject.Token);
                            quickLookup.AccountNameToClient.Remove(clientObject.AccountName.ToLower());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[MediusManager] - Error in TickClients {ex}");
            }
        }

        private void TickDme()
        {
            Queue<(int, string)> dmeToRemove = new Queue<(int, string)>();

            foreach (var quickLookup in _lookupsByAppId)
            {
                foreach (var dmeKeyPair in quickLookup.Value.SessionKeyToDmeClient)
                {
                    if (!dmeKeyPair.Value.IsConnected)
                    {
                        LoggerAccessor.LogInfo($"Destroying DME Client {dmeKeyPair.Value}");

                        // Logout and end session
                        dmeKeyPair.Value?.Logout();
                        dmeKeyPair.Value?.EndSession();

                        dmeToRemove.Enqueue((quickLookup.Key, dmeKeyPair.Key));
                    }
                }
            }

            // Remove
            while (dmeToRemove.TryDequeue(out var appIdAndSessionKey))
            {
                if (_lookupsByAppId.TryGetValue(appIdAndSessionKey.Item1, out var quickLookup))
                {
                    if (quickLookup.SessionKeyToDmeClient.Remove(appIdAndSessionKey.Item2, out var clientObject))
                    {
                        quickLookup.AccessTokenToDmeClient.Remove(clientObject.Token);
                    }
                }
            }
        }

        #endregion

        #region App Ids

        public async Task OnDatabaseAuthenticated()
        {
            // get supported app ids
            AppIdDTO[]? appids = await HorizonServerConfiguration.Database.GetAppIds();

            if (appids != null)
                // build dictionary of app ids from response
                _appIdGroups = appids.ToDictionary(x => x.Name, x => x.AppIds != null ? x.AppIds.ToArray() : Array.Empty<int>());
        }

        public bool IsAppIdSupported(int appId)
        {
            return _appIdGroups.Any(x => x.Value.Contains(appId));
        }

        public int[] GetAppIdsInGroup(int appId)
        {
            return _appIdGroups.FirstOrDefault(x => x.Value.Contains(appId)).Value ?? Array.Empty<int>();
        }

        #endregion

        #region Misc

        public SECURITY_MODE GetServerSecurityMode(SECURITY_MODE securityMode, RSA_KEY rsaKey)
        {
            int result;

            result = (int)securityMode;

            if (securityMode == SECURITY_MODE.MODE_UNKNOWN)
            {
                //result = (KM_GetLocalPublicKey(RSA_KEY, 0x80000000, 0) != 0) + 1;

                //securityMode = (SECURITY_MODE)result;
            }


            return (SECURITY_MODE)result;
        }

        public void rt_msg_server_check_protocol_compatibility(int clientVersion, byte p_compatible)
        {



        }

        #region AnonymouseAccountIdGenerator
        /// <summary>
        /// Generates a Random Anonymous AccountID for MediusAnonymouseAccountRequest <br></br>
        /// Or if one doesn't exist in Database
        /// </summary>
        /// <param name="AnonymousIDRangeSeed">Config Value for changing the MAS</param>
        /// <returns></returns>
        public int AnonymousAccountIDGenerator(int AnonymousIDRangeSeed)
        {
            // Anonymous login expect a negative id < 0.
            return new Random().Next(-80000000, AnonymousIDRangeSeed);
        }
        #endregion

        public string UsingStringJoin(string[] array)
        {
            return string.Join(string.Empty, array);
        }

        #endregion

        #region Vulgarity

        public void load_classifier(string filename)
        {
            int classifier = 0;
            var rootPath = Path.GetFullPath(MediusClass.Settings.MediusVulgarityRootPath);

            try
            {
                var stream = File.Open(rootPath + filename, FileMode.OpenOrCreate, FileAccess.Read);

                FileInfo fi = new FileInfo(filename);

                if (fi.Extension == "cl")
                    classifier = read_classifier(stream);
                else
                    LoggerAccessor.LogWarn($"Unknown file type in {rootPath}.\n");

            }
            catch (Exception)
            {
                LoggerAccessor.LogWarn($"Cannot open {rootPath + "/" + filename}.\n");
            }

        }

        public int read_classifier(FileStream fileClassifier)
        {
            if (fileClassifier.Length == 20398493)
            {

            }

            return 0;
        }
        #endregion

        #region DmeServerClient

        public Task DmeServerClientIpQuery(int WorldID, int TargetClient, IPAddress IP)
        {

            return Task.CompletedTask;
        }

        public DME_SERVER_RESULT DmeServerMapRtError(uint RtError)
        {
            DME_SERVER_RESULT result;
            if (RtError == 52518)
            {
                result = DME_SERVER_RESULT.DME_SERVER_UNKNOWN_MSG_TYPE;
                return result;
            }

            if (RtError > 0xCD26)
            {
                result = DME_SERVER_RESULT.DME_SERVER_MUTEX_ERROR;
                if (RtError != 52528)
                {
                    if (RtError > 0xCD30)
                    {
                        result = DME_SERVER_RESULT.DME_SERVER_UNSECURED_ERROR;
                        if (RtError != 52533)
                        {
                            if (RtError > 0xCD35)
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_CONFIG_ERROR;
                                if (RtError != 52535)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_BUFF_OVERFLOW_ERROR;
                                    if (RtError >= 0xCD37)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_PARTIAL_RW_ERROR;
                                        if (RtError != 52536)
                                        {
                                            result = DME_SERVER_RESULT.DME_SERVER_CLIENT_ALREADY_DISCONNECTED;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //if (RtError == 52530)
                                //return 30;
                                result = DME_SERVER_RESULT.DME_SERVER_NO_MORE_WORLDS;
                                if (RtError >= 0xCD32)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_CLIENT_LIMIT;
                                    if (RtError != 52531)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_ENCRYPTED_ERROR;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        result = DME_SERVER_RESULT.DME_SERVER_MSG_TOO_BIG;
                        if (RtError != 52523)
                        {
                            if (RtError > 0xCD2B)
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_PARTIAL_WRITE;
                                if (RtError != 52525)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_UNKNOWN_MSG_TYPE;
                                    if (RtError >= 0xCD2D)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_SOCKET_RESET_ERROR;
                                        if (RtError != 52526)
                                        {
                                            result = DME_SERVER_RESULT.DME_SERVER_CIRC_BUF_ERROR;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_TCP_GET_WORLD_INDEX;
                                if (RtError != 52520)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_WOULD_BLOCK;
                                    if (RtError >= 0xCD28)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_READ_ERROR;
                                        if (RtError != 52521)
                                        {
                                            result = DME_SERVER_RESULT.DME_SERVER_SOCKET_CLOSED;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (RtError == 52508)
                {
                    result = DME_SERVER_RESULT.DME_SERVER_CONN_MSG_ERROR;
                    return result;
                }
                if (RtError > 0xCD1C)
                {
                    result = DME_SERVER_RESULT.DME_SERVER_SOCKET_BIND_ERROR;
                    if (RtError != 52513)
                    {
                        if (RtError > 0xCD21)
                        {
                            result = DME_SERVER_RESULT.DME_SERVER_SOCKET_LISTEN_ERROR;
                            if (RtError != 52515)
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_SOCKET_POLL_ERROR;
                                if (RtError >= 0xCD23)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_SOCKET_READ_ERROR;
                                    if (RtError != 52516)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_SOCKET_WRITE_ERROR;
                                    }
                                }
                            }
                        }
                        else
                        {
                            result = DME_SERVER_RESULT.DME_SERVER_STACK_LOAD_ERROR;
                            if (RtError != 52510)
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_WORLD_FULL;
                                if (RtError >= 0xCD1E)
                                {
                                    result = DME_SERVER_RESULT.DME_SERVER_SOCKET_CREATE_ERROR;
                                    if (RtError != 52511)
                                    {
                                        result = DME_SERVER_RESULT.DME_SERVER_SOCKET_OPT_ERROR;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (RtError == 52503)
                    {
                        result = DME_SERVER_RESULT.DME_SERVER_NOT_INITIALIZED;
                        return result;
                    }

                    if (RtError <= 0xCD17)
                    {
                        if (RtError == 52501)
                        {
                            result = DME_SERVER_RESULT.DME_SERVER_INVALID_PARAM;
                            return result;
                        }
                        if (RtError > 0xCD15)
                        {
                            result = DME_SERVER_RESULT.DME_SERVER_NOT_IMPLEMENTED;
                            return result;
                        }
                    }

                    result = DME_SERVER_RESULT.DME_SERVER_MEM_ALLOC;
                    if (RtError != 52505)
                    {
                        result = DME_SERVER_RESULT.DME_SERVER_UNKNOWN_RESULT;
                        if (RtError >= 0xCD19)
                        {
                            result = DME_SERVER_RESULT.DME_SERVER_SOCKET_LIMIT;
                            if (RtError != 52506)
                            {
                                result = DME_SERVER_RESULT.DME_SERVER_UNKNOWN_CONN_ERROR;
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Matchmaking
        public int CalculateSizeOfMatchRoster(MediusMatchRosterInfo roster)
        {
            int rosterSize;
            uint v3;
            uint partySize;

            MediusMatchPartyInfo mediusMatchPartyInfo = new MediusMatchPartyInfo();

            if (roster == null)
                return 0;
            rosterSize = 4 * roster.NumParties + 8;
            partySize = (uint)roster.Parties;
            v3 = (uint)(4 * roster.NumParties + partySize - 4);
            while (partySize <= v3)
            {
                rosterSize += CalculateSizeOfMatchParty(mediusMatchPartyInfo);
                partySize += 4;
            }

            return rosterSize;
        }

        public int CalculateSizeOfMatchParty(MediusMatchPartyInfo party)
        {
            int MatchPartySize;
            if (party != null)
            {
                MatchPartySize = 8 * party.NumPlayers + 8;
            }
            else
            {
                MatchPartySize = 0;
            }

            return MatchPartySize;
        }
        #endregion
    }
}
