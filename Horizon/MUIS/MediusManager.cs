using CustomLogger;
using CryptoSporidium.Horizon.RT.Common;
using CryptoSporidium.Horizon.RT.Models;
using CryptoSporidium.Horizon.LIBRARY.Common;
using CryptoSporidium.Horizon.LIBRARY.Database.Models;
using Horizon.MUIS.Models;
using System.Collections.Concurrent;
using System.Net;

namespace Horizon.MUIS
{
    public class MediusManager
    {
        private class QuickLookup
        {
            public Dictionary<int, ClientObject> AccountIdToClient = new();
            public Dictionary<string, ClientObject> AccountNameToClient = new();
            public Dictionary<string, ClientObject> AccessTokenToClient = new();
            public Dictionary<string, ClientObject> SessionKeyToClient = new();

            public Dictionary<int, AccountDTO> BuddyInvitationsToClient = new();

            public Dictionary<string, DMEObject> AccessTokenToDmeClient = new();
            public Dictionary<string, DMEObject> SessionKeyToDmeClient = new();


            public Dictionary<int, Channel> ChannelIdToChannel = new();
            public Dictionary<string, Channel> ChanneNameToChannel = new();
            public Dictionary<int, Game> GameIdToGame = new();
        }

        private Dictionary<string, int[]> _appIdGroups = new();
        private Dictionary<int, QuickLookup> _lookupsByAppId = new();

        private List<MediusFile> _mediusFiles = new List<MediusFile>();
        private List<MediusFileMetaData> _mediusFilesToUpdateMetaData = new();

        private ConcurrentQueue<ClientObject> _addQueue = new();

        #region Clients
        public List<ClientObject> GetClients(int appId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            return _lookupsByAppId.Where(x => appIdsInGroup.Contains(x.Key)).SelectMany(x => x.Value.AccountIdToClient.Select(x => x.Value)).ToList();
        }

        public ClientObject? GetClientByAccountId(int accountId, int appId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    if (quickLookup.AccountIdToClient.TryGetValue(accountId, out var result))
                        return result;
                }
            }

            return null;
        }

        public ClientObject? GetClientByAccountName(string accountName, int appId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);
            accountName = accountName.ToLower();

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    if (quickLookup.AccountNameToClient.TryGetValue(accountName, out var result))
                        return result;
                }
            }

            return null;
        }

        public ClientObject? GetClientByAccessToken(string accessToken, int appId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    if (quickLookup.AccessTokenToClient.TryGetValue(accessToken, out var result))
                        return result;
                }
            }

            return null;
        }

        public ClientObject? GetClientBySessionKey(string sessionKey, int appId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    if (quickLookup.SessionKeyToDmeClient.TryGetValue(sessionKey, out var result))
                        return result;
                }
            }

            return null;
        }

        public DMEObject? GetDmeByAccessToken(string accessToken, int appId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    if (quickLookup.AccessTokenToDmeClient.TryGetValue(accessToken, out var result))
                        return result;
                }
            }

            return null;
        }

        public DMEObject? GetDmeBySessionKey(string sessionKey, int appId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    if (quickLookup.SessionKeyToDmeClient.TryGetValue(sessionKey, out var result))
                        return result;
                }
            }

            return null;
        }

        public void AddDmeClient(DMEObject dmeClient)
        {
            if (!dmeClient.IsLoggedIn)
                throw new InvalidOperationException($"Attempting to add DME client {dmeClient} to MediusManager but client has not yet logged in.");

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

                LoggerAccessor.LogError($"[DME] - AddDmeClient thrown an exception {ex}");
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

        public Game? GetGameByDmeWorldId(string dmeSessionKey, int dmeWorldId)
        {
            foreach (var lookupByAppId in _lookupsByAppId)
            {
                lock (lookupByAppId.Value.GameIdToGame)
                {
                    var game = lookupByAppId.Value.GameIdToGame.FirstOrDefault(x => x.Value?.DMEServer?.SessionKey == dmeSessionKey && x.Value?.DMEWorldId == dmeWorldId).Value;
                    if (game != null)
                        return game;
                }
            }

            return null;
        }

        public Channel? GetWorldByName(string worldName)
        {
            foreach (var lookupByAppId in _lookupsByAppId)
            {
                lock (lookupByAppId.Value.ChanneNameToChannel)
                {
                    if (lookupByAppId.Value.ChanneNameToChannel.TryGetValue(worldName, out var channel))
                        return channel;
                }
            }

            return null;
        }

        public Game? GetGameByGameId(ClientObject client, int gameId)
        {
            var appIdsInGroup = GetAppIdsInGroup(client.ApplicationId);

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    lock (quickLookup.GameIdToGame)
                    {
                        return quickLookup.GameIdToGame.FirstOrDefault(x => x.Value.Id == gameId && appIdsInGroup.Contains(x.Value.ApplicationId)).Value;
                    }
                }
            }

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

            quickLookup.GameIdToGame.Add(game.Id, game);
            await HorizonServerConfiguration.Database.CreateGame(game.ToGameDTO());
        }

        public int GetGameCountAppId(int appId)
        {
            if (!_lookupsByAppId.TryGetValue(appId, out var quickLookup))
                _lookupsByAppId.Add(appId, quickLookup = new QuickLookup());

            int gameCount = quickLookup.GameIdToGame.Count;

            return gameCount;
        }


        #region Channels

        public Channel? GetChannelByChannelId(int channelId, int appId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    lock (quickLookup.ChannelIdToChannel)
                    {
                        if (quickLookup.ChannelIdToChannel.TryGetValue(channelId, out var result))
                            return result;
                    }
                }
            }

            return null;
        }

        public Channel? GetChannelByChannelName(string channelName, int appId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    lock (quickLookup.ChannelIdToChannel)
                    {
                        return quickLookup.ChannelIdToChannel.FirstOrDefault(x => x.Value.Name == channelName && appIdsInGroup.Contains(x.Value.ApplicationId)).Value;
                    }
                }
            }

            return null;
        }

        public uint GetChannelCount(ChannelType type, int appId)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);
            uint count = 0;

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    lock (quickLookup.ChannelIdToChannel)
                    {
                        count += (uint)quickLookup.ChannelIdToChannel.Count(x => x.Value.Type == type);
                    }
                }
            }

            return count;
        }

        public Channel GetOrCreateDefaultLobbyChannel(int appId)
        {
            Channel? channel = null;
            var appIdsInGroup = GetAppIdsInGroup(appId);

            foreach (var appIdInGroup in appIdsInGroup)
            {
                if (_lookupsByAppId.TryGetValue(appIdInGroup, out var quickLookup))
                {
                    lock (quickLookup.ChannelIdToChannel)
                    {
                        //, x => x.Value.Type == ChannelType.Lobby
                        channel = quickLookup.ChannelIdToChannel.FirstOrDefault(x => x.Value.ApplicationId == appId).Value;
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
            if (!_lookupsByAppId.TryGetValue(channel.ApplicationId, out var quickLookup))
                _lookupsByAppId.Add(channel.ApplicationId, quickLookup = new QuickLookup());

            lock (quickLookup.ChannelIdToChannel)
            {
                quickLookup.ChannelIdToChannel.Add(channel.Id, channel);
            }

            lock (quickLookup.ChanneNameToChannel)
            {

                quickLookup.ChanneNameToChannel.Add(channel.Name, channel);
            }

            await channel.OnChannelCreate(channel);
        }

        public IEnumerable<Channel> GetChannelList(int appId, int pageIndex, int pageSize, ChannelType type)
        {
            var appIdsInGroup = GetAppIdsInGroup(appId);

            return _lookupsByAppId
                .Where(x => appIdsInGroup.Contains(x.Key))
                .SelectMany(x => x.Value.ChannelIdToChannel.Select(x => x.Value))
                .Where(x => x.Type == type)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);
        }
        #endregion

        #endregion

        #region MFS
        public IEnumerable<MediusFile> GetFilesList(string path, string filenameBeginsWith, uint pageSize, uint startingEntryNumber)
        {
            lock (_mediusFiles)
            {

                string[]? files = null;
                int counter = 0;

                if (filenameBeginsWith.ToString() == "*")
                    files = Directory.GetFiles(path).Select(file => Path.GetFileName(file)).ToArray();
                else
                    files = Directory.GetFiles(path, Convert.ToString(filenameBeginsWith));

                if (files.Length < pageSize)
                    counter = files.Count();
                else
                    counter = (int)pageSize - 1;

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

        public IEnumerable<MediusFile> GetFilesListExt(string path, string filenameBeginsWith, uint pageSize, uint startingEntryNumber)
        {
            lock (_mediusFiles)
            {
                if (startingEntryNumber == 0)
                    return _mediusFiles;

                int counter = 0;
                string filenameBeginsWithAppended = filenameBeginsWith.Remove(filenameBeginsWith.Length - 1);

                string[] filesArray = Directory.GetFiles(path);

                //files = Directory.GetFiles(path).Select(file => Path.GetFileName(filenameBeginsWith)).ToArray();

                if (filesArray.Length < pageSize)
                    counter = filesArray.Count() - 1;
                else
                    counter = (int)pageSize - 1;

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
                    LoggerAccessor.LogWarn($"MFS UpdateMetaData Exception:\n{e}");
                }

                return _mediusFilesToUpdateMetaData;
            }
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

        #region Tick

        public async Task Tick()
        {
            await TickClients();

            await TickChannels();

            await TickGames();
        }

        private async Task TickChannels()
        {
            Queue<(QuickLookup, int)> channelsToRemove = new Queue<(QuickLookup, int)>();

            // Tick channels
            foreach (var quickLookup in _lookupsByAppId)
            {
                foreach (var channelKeyPair in quickLookup.Value.ChannelIdToChannel)
                {
                    if (channelKeyPair.Value.ReadyToDestroy)
                    {
                        LoggerAccessor.LogInfo($"Destroying Channel {channelKeyPair.Value}");
                        channelsToRemove.Enqueue((quickLookup.Value, channelKeyPair.Key));
                    }
                    else
                        await channelKeyPair.Value.Tick();
                }
            }

            // Remove channels
            while (channelsToRemove.TryDequeue(out var lookupAndChannelId))
                lookupAndChannelId.Item1.ChannelIdToChannel.Remove(lookupAndChannelId.Item2);
        }

        private async Task TickGames()
        {
            Queue<(QuickLookup, int)> gamesToRemove = new Queue<(QuickLookup, int)>();

            // Tick games
            foreach (var quickLookup in _lookupsByAppId)
            {
                foreach (var gameKeyPair in quickLookup.Value.GameIdToGame)
                {
                    if (gameKeyPair.Value.ReadyToDestroy)
                    {
                        LoggerAccessor.LogInfo($"Destroying Game {gameKeyPair.Value}");
                        await gameKeyPair.Value.EndGame();
                        gamesToRemove.Enqueue((quickLookup.Value, gameKeyPair.Key));
                    }
                    else
                        await gameKeyPair.Value.Tick();
                }
            }

            // Remove games
            while (gamesToRemove.TryDequeue(out var lookupAndGameId))
                lookupAndGameId.Item1.GameIdToGame.Remove(lookupAndGameId.Item2);
        }

        private async Task TickClients()
        {
            Queue<(int, string)> clientsToRemove = new Queue<(int, string)>();


            while (_addQueue.TryDequeue(out var newClient))
            {
                if (!_lookupsByAppId.TryGetValue(newClient.ApplicationId, out var quickLookup))
                    _lookupsByAppId.Add(newClient.ApplicationId, quickLookup = new QuickLookup());

                try
                {
                    quickLookup.AccountIdToClient.Add(newClient.AccountId, newClient);
                    quickLookup.AccountNameToClient.Add(newClient.AccountName.ToLower(), newClient);
                    quickLookup.AccessTokenToClient.Add(newClient.Token, newClient);
                    quickLookup.SessionKeyToClient.Add(newClient.SessionKey, newClient);
                }
                catch (Exception ex)
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

                    LoggerAccessor.LogError(ex);
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

            /*
            try
            {

            } catch (Exception e)
            {
                Logger.Warn(e);
            }
            */
        }

        private void TickDme()
        {
            Queue<(int, string)> dmeToRemove = new Queue<(int, string)>();

            foreach (var quickLookup in _lookupsByAppId)
            {
                foreach (var dmeKeyPair in quickLookup.Value.SessionKeyToDmeClient)
                {
                    if (dmeKeyPair.Value != null && !dmeKeyPair.Value.IsConnected)
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
            var appids = await HorizonServerConfiguration.Database.GetAppIds();

            // build dictionary of app ids from response
            _appIdGroups = appids.ToDictionary(x => x.Name, x => x.AppIds.ToArray());
        }

        public bool IsAppIdSupported(int appId)
        {
            return _appIdGroups.Any(x => x.Value.Contains(appId));
        }

        public int[] GetAppIdsInGroup(int appId)
        {
            return _appIdGroups.FirstOrDefault(x => x.Value.Contains(appId)).Value ?? new int[0];
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
        /// Generates a Random Anonymous AccountID for MediusAnonymouseAccountRequest
        /// </summary>
        /// <param name="AnonymousIDRangeSeed">Config Value for changing the MAS</param>
        /// <returns></returns>
        public int AnonymousAccountIDGenerator(int AnonymousIDRangeSeed)
        {
            int result; // eax

            //for integers
            Random r = new Random();
            int rInt = r.Next(-80000000, 0);

            result = rInt;
            return result;
        }
        #endregion

        public string UsingStringJoin(string[] array)
        {
            return string.Join(string.Empty, array);
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
                                            result = DME_SERVER_RESULT.DME_SERVER_CLIENT_ALREADY_DISCONNECTED;
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
                                        result = DME_SERVER_RESULT.DME_SERVER_ENCRYPTED_ERROR;
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
                                            result = DME_SERVER_RESULT.DME_SERVER_CIRC_BUF_ERROR;
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
                                            result = DME_SERVER_RESULT.DME_SERVER_SOCKET_CLOSED;
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
                                        result = DME_SERVER_RESULT.DME_SERVER_SOCKET_WRITE_ERROR;
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
                                        result = DME_SERVER_RESULT.DME_SERVER_SOCKET_OPT_ERROR;
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
                                result = DME_SERVER_RESULT.DME_SERVER_UNKNOWN_CONN_ERROR;
                        }
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
