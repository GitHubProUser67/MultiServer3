using CustomLogger;
using Horizon.RT.Common;
using Horizon.RT.Models;
using Horizon.LIBRARY.Common;
using Horizon.LIBRARY.Database.Models;
using Horizon.MEDIUS.PluginArgs;
using System.Data;
using Horizon.PluginManager;
using Horizon.HTTPSERVICE;
using Horizon.MEDIUS;
using Horizon.MEDIUS.Medius.Models;
using System.Text.Json.Serialization;

namespace Horizon.MUM
{
    public class Game
    {
        [JsonIgnore]
        private static int IdCounter = 1;

        public class GameClient
        {
            public ClientObject? Client;

            public int DmeId;
            public bool InGame;
        }

        public int MediusWorldId = 0;
        public int ApplicationId = 0;
        public ChannelType ChannelType = ChannelType.Game;
        public List<GameClient> LocalClients = new();
        public string? GameName;
        public string? GamePassword;
        public string? SpectatorPassword;
        public byte[] GameStats = new byte[Constants.GAMESTATS_MAXLEN];
        public MediusGameHostType GameHostType;
        public MGCL_GAME_HOST_TYPE GAME_HOST_TYPE;
        public NetAddressList? netAddressList;
        public RSA_KEY pubKey = new();
        public int WorldID;
        public int AccountID;
        public int MinPlayers;
        public int MaxPlayers;
        public int GameLevel;
        public int PlayerSkillLevel;
        public int RulesSet;
        public string? Metadata;
        public int GenericField1;
        public int GenericField2;
        public int GenericField3;
        public int GenericField4;
        public int GenericField5;
        public int GenericField6;
        public int GenericField7;
        public int GenericField8;
        public byte[]? RequestData;
        public uint GroupMemberListSize;
        public byte[]? GroupMemberList;
        public uint AppDataSize;
        public string? AppData;

        public MediusWorldStatus WorldStatus => _worldStatus;
        public MediusWorldAttributesType Attributes;
        public MediusMatchOptions MatchOptions;
        public DMEObject? DMEServer;
        public Channel? ChatChannel;
        public ClientObject? Host;

        public string? AccountIdsAtStart => accountIdsAtStart;
        public DateTime UtcTimeCreated => utcTimeCreated;
        public DateTime? UtcTimeStarted => utcTimeStarted;
        public DateTime? UtcTimeEnded => utcTimeEnded;

        public MediusWorldStatus _worldStatus = MediusWorldStatus.WorldPendingCreation;
        public bool hasHostJoined = false;
        public DateTime utcTimeCreated;
        public DateTime? utcLastJoined;
        public DateTime? utcTimeStarted;
        public DateTime? utcTimeEnded;
        public DateTime? utcTimeEmpty;

        protected string? accountIdsAtStart;
        protected bool destroyed = false;

        public uint Time => (uint)(Utils.GetHighPrecisionUtcTime() - utcTimeCreated).TotalMilliseconds;

        public int PlayerCount => LocalClients.Count(x => x != null && x.Client != null && x.Client.IsConnected || x != null && x.InGame);

        public virtual bool ReadyToDestroy => WorldStatus == MediusWorldStatus.WorldClosed && utcTimeEmpty.HasValue && (Utils.GetHighPrecisionUtcTime() - utcTimeEmpty)?.TotalSeconds > 1f;

        public Game(ClientObject client, IMediusRequest createGame, Channel? chatChannel, DMEObject? dmeServer, int WorldId = 0)
        {
            if (createGame is MediusCreateGameRequest r)
                FromCreateGameRequest(r, WorldId);
            else if (createGame is MediusCreateGameRequest0 r0)
                FromCreateGameRequest0(r0, WorldId);
            else if (createGame is MediusCreateGameRequest1 r1)
                FromCreateGameRequest1(r1, WorldId);
            else if (createGame is MediusMatchCreateGameRequest r2)
                FromMatchCreateGameRequest(r2, WorldId);
            else if (createGame is MediusServerCreateGameOnMeRequest r3)
                FromCreateGameOnMeRequest(r3);
            else if (createGame is MediusServerCreateGameOnSelfRequest r5)
                FromCreateGameOnSelfRequest(r5);
            else if (createGame is MediusServerCreateGameOnSelfRequest0 r6)
                FromCreateGameOnSelfRequest0(r6);

            MediusWorldId = IdCounter++;

            utcTimeCreated = Utils.GetHighPrecisionUtcTime();
            utcTimeEmpty = null;
            DMEServer = dmeServer;
            ChatChannel = chatChannel;
            ChatChannel?.RegisterGame(this);
            Host = client;
            SetWorldStatus(MediusWorldStatus.WorldStaging).Wait();

            LoggerAccessor.LogInfo($"Game {MediusWorldId}: {GameName}: Created by {client} | Host: {Host}");
        }

        public GameDTO ToGameDTO()
        {
            return new GameDTO()
            {
                AppId = ApplicationId,
                GameCreateDt = utcTimeCreated,
                GameEndDt = utcTimeEnded,
                GameStartDt = utcTimeStarted,
                GameHostType = GameHostType.ToString(),
                GameId = MediusWorldId,
                GameLevel = GameLevel,
                GameName = GameName,
                GameStats = GameStats,
                GenericField1 = GenericField1,
                GenericField2 = GenericField2,
                GenericField3 = GenericField3,
                GenericField4 = GenericField4,
                GenericField5 = GenericField5,
                GenericField6 = GenericField6,
                GenericField7 = GenericField7,
                GenericField8 = GenericField8,
                MaxPlayers = MaxPlayers,
                MinPlayers = MinPlayers,
                PlayerCount = PlayerCount,
                PlayerSkillLevel = PlayerSkillLevel,
                RuleSet = RulesSet,
                Metadata = Metadata,
                WorldStatus = WorldStatus.ToString(),
                PlayerListCurrent = GetActivePlayerList(),
                PlayerListStart = accountIdsAtStart,
                Destroyed = destroyed
            };
        }

        private void FromMatchCreateGameRequest(MediusMatchCreateGameRequest createGame, int WorldId)
        {
            ApplicationId = createGame.ApplicationID;
            GameName = createGame.GameName;
            MinPlayers = createGame.MinPlayers;
            MaxPlayers = createGame.MaxPlayers;
            GameLevel = createGame.GameLevel;
            PlayerSkillLevel = createGame.PlayerSkillLevel;
            RulesSet = createGame.RulesSet;
            GenericField1 = createGame.GenericField1;
            GenericField2 = createGame.GenericField2;
            GenericField3 = createGame.GenericField3;
            GenericField4 = createGame.GenericField4;
            GenericField5 = createGame.GenericField5;
            GenericField6 = createGame.GenericField6;
            GenericField7 = createGame.GenericField7;
            GenericField8 = createGame.GenericField8;
            GamePassword = createGame.GamePassword;
            SpectatorPassword = createGame.SpectatorPassword;
            GameHostType = createGame.GameHostType;
            Attributes = createGame.WorldAttributesType;
            MatchOptions = createGame.MatchOptions;
			WorldID = WorldId;
        }

        private void FromCreateGameRequest(MediusCreateGameRequest createGame, int WorldId)
        {
            ApplicationId = createGame.ApplicationID;
            GameName = createGame.GameName;
            MinPlayers = createGame.MinPlayers;
            MaxPlayers = createGame.MaxPlayers;
            GameLevel = createGame.GameLevel;
            PlayerSkillLevel = createGame.PlayerSkillLevel;
            RulesSet = createGame.RulesSet;
            GenericField1 = createGame.GenericField1;
            GenericField2 = createGame.GenericField2;
            GenericField3 = createGame.GenericField3;
            GenericField4 = createGame.GenericField4;
            GenericField5 = createGame.GenericField5;
            GenericField6 = createGame.GenericField6;
            GenericField7 = createGame.GenericField7;
            GenericField8 = createGame.GenericField8;
            GamePassword = createGame.GamePassword;
            SpectatorPassword = createGame.SpectatorPassword;
            GameHostType = createGame.GameHostType;
            Attributes = createGame.Attributes;
			WorldID = WorldId;
        }

        private void FromCreateGameRequest0(MediusCreateGameRequest0 createGame, int WorldId)
        {
            ApplicationId = createGame.ApplicationID;
            GameName = createGame.GameName;
            MinPlayers = createGame.MinPlayers;
            MaxPlayers = createGame.MaxPlayers;
            GameLevel = createGame.GameLevel;
            PlayerSkillLevel = createGame.PlayerSkillLevel;
            RulesSet = createGame.RulesSet;
            GenericField1 = createGame.GenericField1;
            GenericField2 = createGame.GenericField2;
            GenericField3 = createGame.GenericField3;
            GamePassword = createGame.GamePassword;
            GameHostType = createGame.GameHostType;
			WorldID = WorldId;
        }


        private void FromCreateGameRequest1(MediusCreateGameRequest1 createGame, int WorldId)
        {
            ApplicationId = createGame.ApplicationID;
            GameName = createGame.GameName;
            MinPlayers = createGame.MinPlayers;
            MaxPlayers = createGame.MaxPlayers;
            GameLevel = createGame.GameLevel;
            PlayerSkillLevel = createGame.PlayerSkillLevel;
            RulesSet = createGame.RulesSet;
            GenericField1 = createGame.GenericField1;
            GenericField2 = createGame.GenericField2;
            GenericField3 = createGame.GenericField3;
            GamePassword = createGame.GamePassword;
            SpectatorPassword = createGame.SpectatorPassword;
            GameHostType = createGame.GameHostType;
            WorldID = WorldId;
        }

        private void FromCreateGameOnMeRequest(MediusServerCreateGameOnMeRequest serverCreateGameOnMe)
        {
            GameHostType = MediusGameHostType.MediusGameHostPeerToPeer;

            GameName = serverCreateGameOnMe.GameName;
            GameStats = serverCreateGameOnMe.GameStats;
            GamePassword = serverCreateGameOnMe.GamePassword;
            ApplicationId = serverCreateGameOnMe.ApplicationID;
            MaxPlayers = serverCreateGameOnMe.MaxClients;
            MinPlayers = serverCreateGameOnMe.MinClients;
            GameLevel = serverCreateGameOnMe.GameLevel;
            PlayerSkillLevel = serverCreateGameOnMe.PlayerSkillLevel;
            RulesSet = serverCreateGameOnMe.RulesSet;
            GenericField1 = serverCreateGameOnMe.GenericField1;
            GenericField2 = serverCreateGameOnMe.GenericField2;
            GenericField3 = serverCreateGameOnMe.GenericField3;
            GenericField4 = serverCreateGameOnMe.GenericField4;
            GenericField5 = serverCreateGameOnMe.GenericField5;
            GenericField6 = serverCreateGameOnMe.GenericField6;
            GenericField7 = serverCreateGameOnMe.GenericField7;
            GenericField8 = serverCreateGameOnMe.GenericField8;
            GAME_HOST_TYPE = serverCreateGameOnMe.GameHostType;
            netAddressList = serverCreateGameOnMe.AddressList;
            WorldID = serverCreateGameOnMe.WorldID;
        }

        private void FromCreateGameOnSelfRequest(MediusServerCreateGameOnSelfRequest serverCreateGameOnSelf)
        {
            GameName = serverCreateGameOnSelf.GameName;
            GameStats = serverCreateGameOnSelf.GameStats;
            GamePassword = serverCreateGameOnSelf.GamePassword;
            ApplicationId = serverCreateGameOnSelf.ApplicationID;
            MaxPlayers = serverCreateGameOnSelf.MaxClients;
            MinPlayers = serverCreateGameOnSelf.MinClients;
            GameLevel = serverCreateGameOnSelf.GameLevel;
            PlayerSkillLevel = serverCreateGameOnSelf.PlayerSkillLevel;
            RulesSet = serverCreateGameOnSelf.RulesSet;
            GenericField1 = serverCreateGameOnSelf.GenericField1;
            GenericField2 = serverCreateGameOnSelf.GenericField2;
            GenericField3 = serverCreateGameOnSelf.GenericField3;
            GAME_HOST_TYPE = serverCreateGameOnSelf.GameHostType;
            netAddressList = serverCreateGameOnSelf.AddressList;
            WorldID = serverCreateGameOnSelf.WorldID;
            AccountID = serverCreateGameOnSelf.AccountID;
        }

        private void FromCreateGameOnSelfRequest0(MediusServerCreateGameOnSelfRequest0 serverCreateGameOnSelf0)
        {
            GameName = serverCreateGameOnSelf0.GameName;
            GameStats = serverCreateGameOnSelf0.GameStats;
            GamePassword = serverCreateGameOnSelf0.GamePassword;
            ApplicationId = serverCreateGameOnSelf0.ApplicationID;
            MaxPlayers = serverCreateGameOnSelf0.MaxClients;
            MinPlayers = serverCreateGameOnSelf0.MinClients;
            GameLevel = serverCreateGameOnSelf0.GameLevel;
            PlayerSkillLevel = serverCreateGameOnSelf0.PlayerSkillLevel;
            RulesSet = serverCreateGameOnSelf0.RulesSet;
            GenericField1 = serverCreateGameOnSelf0.GenericField1;
            GenericField2 = serverCreateGameOnSelf0.GenericField2;
            GenericField3 = serverCreateGameOnSelf0.GenericField3;
            GAME_HOST_TYPE = serverCreateGameOnSelf0.GameHostType;
            netAddressList = serverCreateGameOnSelf0.AddressList;
            WorldID = serverCreateGameOnSelf0.WorldID;
        }

        public virtual int ReassignGameMediusWorldID(MediusReassignGameMediusWorldID reassignGameMediusWorldID)
        {
            // Ensure reassignedGame Old MediusWorldID matches current Game
            if (reassignGameMediusWorldID.OldMediusWorldID != MediusWorldId)
                return 0;

            MediusWorldId = reassignGameMediusWorldID.NewMediusWorldID;

            return MediusWorldId;
        }

        public string GetActivePlayerList()
        {
            IEnumerable<string?>? playlist = LocalClients?.Select(x => x.Client?.AccountId.ToString()).Where(x => x != null);
            if (playlist != null)
                return string.Join(",", playlist);

            return string.Empty;
        }

        public virtual async Task Tick()
        {
            // Remove timedout clients
            for (int i = 0; i < LocalClients.Count; ++i)
            {
                var client = LocalClients[i];

                if (client == null || client.Client == null || !client.Client.IsConnected || client.Client.CurrentGame?.MediusWorldId != MediusWorldId)
                {
                    LoggerAccessor.LogWarn($"REMOVING CLIENT: {client}\n IS: {client?.Client}\nHasHostJoined: {hasHostJoined}\nIS Connected?: {client?.Client?.IsConnected}\nClient CurrentGame ID: {client?.Client?.CurrentGame?.MediusWorldId}\nGameId: {MediusWorldId}\nMatch?: {client?.Client?.CurrentGame?.MediusWorldId != MediusWorldId}");
                    lock (LocalClients)
                        LocalClients.RemoveAt(i);
                    --i;
                }
            }

            // Auto close when everyone leaves or if host fails to connect after timeout time
            if (!utcTimeEmpty.HasValue && !LocalClients.Any(x => x.InGame) && (hasHostJoined || (Utils.GetHighPrecisionUtcTime() - utcTimeCreated).TotalSeconds > MediusClass.GetAppSettingsOrDefault(ApplicationId).GameTimeoutSeconds))
            {
                LoggerAccessor.LogWarn("AUTO CLOSING WORLD");
                utcTimeEmpty = Utils.GetHighPrecisionUtcTime();
                await SetWorldStatus(MediusWorldStatus.WorldClosed);
            }
        }

        public virtual async Task OnMediusServerConnectNotification(MediusServerConnectNotification connectNotification)
        {
            var player = LocalClients.FirstOrDefault(x => x.Client?.SessionKey == connectNotification.PlayerSessionKey);

            if (player == null)
                return;

            switch (connectNotification.ConnectEventType)
            {
                case MGCL_EVENT_TYPE.MGCL_EVENT_CLIENT_CONNECT:
                    {
                        await OnPlayerJoined(player);
                        break;
                    }
                case MGCL_EVENT_TYPE.MGCL_EVENT_CLIENT_DISCONNECT:
                    {
                        await OnPlayerLeft(player, connectNotification);
                        break;
                    }
            }
        }

        public virtual async Task OnMediusJoinGameResponse(string? Sessionkey)
        {
            if (string.IsNullOrEmpty(Sessionkey))
                return;

            GameClient? player = LocalClients.FirstOrDefault(x => x.Client?.SessionKey == Sessionkey);

            if (player == null)
                return;

            await OnPlayerJoined(player);
        }

        public virtual async Task OnMediusServerCreateGameOnMeRequest(IMediusRequest createGameOnMeRequest)
        {
            GameClient? player = LocalClients.FirstOrDefault(x => x != null && x.Client != null && x.Client.IsConnected);
            if (player == null)
                return;

            await OnPlayerJoined(player);
        }

        public virtual async Task OnPlayerJoined(GameClient player)
        {
            utcLastJoined = DateTime.UtcNow;

            bool ishost = false;

            player.InGame = true;

            if (player.Client == Host)
            {
                LoggerAccessor.LogInfo($"[Game] -> OnHostJoined -> {player.Client?.ApplicationId} - {player.Client?.CurrentGame?.GameName} (id : {player.Client?.CurrentGame?.WorldID}) -> {player.Client?.AccountName} -> {player.Client?.LanguageType}");
                ishost = true;
                hasHostJoined = true;
            }
            else
                LoggerAccessor.LogInfo($"[Game] -> OnPlayerJoined -> {player.Client?.ApplicationId} - {player.Client?.CurrentGame?.GameName} (id : {player.Client?.CurrentGame?.WorldID}) -> {player.Client?.AccountName} -> {player.Client?.LanguageType}");

            try
            {
                if (player.Client != null)
                    RoomManager.UpdateOrCreateRoom(player.Client.ApplicationId.ToString(), player.Client.CurrentGame?.GameName, player.Client.CurrentGame?.MediusWorldId,
                        player.Client.CurrentGame?.WorldID.ToString(), player.Client.AccountName, player.Client.DmeClientId, player.Client.LanguageType.ToString(), ishost);
            }
            catch
            {
                // Not Important
            }

            // Send to plugins
            await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_JOINED_GAME, new OnPlayerGameArgs() { Player = player.Client, Game = this });
        }

        public virtual void AddPlayer(ClientObject client)
        {
            // Don't add again
            if (LocalClients.Any(x => x.Client == client))
                return;

            LoggerAccessor.LogInfo($"Game {MediusWorldId}: {GameName}: {client} added with sessionkey {client.SessionKey}.");

            lock (LocalClients)
            {
                LocalClients.Add(new GameClient()
                {
                    Client = client,
                    DmeId = client.DmeClientId != null ? (int)client.DmeClientId : 0
                });
            }

            // Inform the client of any custom game mode
            //client.CurrentChannel?.SendSystemMessage(client, $"Gamemode is {CustomGamemode?.FullName ?? "default"}.");
        }

        protected virtual async Task OnPlayerLeft(GameClient player, MediusServerConnectNotification connectNotification)
        {
            LoggerAccessor.LogInfo($"[Game] -> OnPlayerLeft -> {player.Client?.ApplicationId} - {player.Client?.CurrentGame?.GameName} (id : {player.Client?.CurrentGame?.WorldID}) -> {player.Client?.AccountName} -> {player.Client?.LanguageType}");

            player.InGame = false;

            if (player.Client != null)
            {
                // Update player object
                await player.Client.LeaveGame(this);

                // Remove from collection
                if (player.Client.CurrentGame != null)
                    await RemovePlayer(player.Client, player.Client.ApplicationId, player.Client.CurrentGame?.WorldID.ToString());
            }
        }

        public virtual async Task RemovePlayer(ClientObject client, int appid, string? WorldId)
        {
            LoggerAccessor.LogInfo($"Game {MediusWorldId}: {GameName}: {client} removed.");

            try
            {
                if (!string.IsNullOrEmpty(client.CurrentGame?.GameName) && !string.IsNullOrEmpty(client.AccountName) && !string.IsNullOrEmpty(WorldId))
                    RoomManager.RemoveUserFromGame(client.ApplicationId.ToString(), client.CurrentGame.GameName, WorldId, client.AccountName);

                if (PlayerCount <= 1)
                    RoomManager.RemoveGame(client.ApplicationId.ToString(), WorldId, GameName);
            }
            catch
            {
                // Not Important
            }

            // Send to plugins
            await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_LEFT_GAME, new OnPlayerGameArgs() { Player = client, Game = this });

            // Remove host
            if (Host == client)
            {
                // Send to plugins
                await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_HOST_LEFT, new OnPlayerGameArgs() { Player = client, Game = this });

                Host = null;
            }

            // Remove from clients list
            lock (LocalClients)
                LocalClients.RemoveAll(x => x.Client == client);
        }

        public virtual async Task OnEndGameReport(MediusEndGameReport report, int appid)
        {
            try
            {
                // Send database EndGameReport info
                await EndGame(appid);

                LoggerAccessor.LogInfo($"Successful local delete of game world [{report.MediusWorldID}]");
            }
            catch (Exception e)
            {
                LoggerAccessor.LogWarn($"Couldn't perform local delete of game world [{report.MediusWorldID}] with exception: {e}");
            }
        }

        public virtual async Task OnWorldReport(MediusWorldReport report, int appId)
        {
            // Ensure report is for correct game world
            if (report.MediusWorldID != MediusWorldId)
                return;

            if (appId == 24180)
                report.MaxPlayers = 10;

            GameName = report.GameName;
            GameStats = report.GameStats;
            MinPlayers = report.MinPlayers;
            MaxPlayers = report.MaxPlayers;
            GameLevel = report.GameLevel;
            PlayerSkillLevel = report.PlayerSkillLevel;
            RulesSet = report.RulesSet;
            GenericField1 = report.GenericField1;
            GenericField2 = report.GenericField2;
            GenericField3 = report.GenericField3;
            GenericField4 = report.GenericField4;
            GenericField5 = report.GenericField5;
            GenericField6 = report.GenericField6;
            GenericField7 = report.GenericField7;
            GenericField8 = report.GenericField8;

            // Once the world has been closed then we force it closed.
            // This is because when the host hits 'Play Again' they tell the server the world has closed (EndGameReport)
            // but the existing clients tell the server the world is still active.
            // This gives the host a "Game Name Already Exists" when they try to remake with the same name.
            // This just fixes that. At the cost of the game not showing after a host leaves a game.
            if (WorldStatus != MediusWorldStatus.WorldClosed && WorldStatus != report.WorldStatus)
                await SetWorldStatus(report.WorldStatus);
            else
            {
                // Update db
                if (!utcTimeEnded.HasValue)
                    _ = HorizonServerConfiguration.Database.UpdateGame(ToGameDTO());
            }
        }

        public virtual async Task OnWorldReport0(MediusWorldReport0 report)
        {
            // Ensure report is for correct game world
            if (report.MediusWorldID != MediusWorldId)
                return;

            GameName = report.GameName;
            GameStats = report.GameStats;
            MinPlayers = report.MinPlayers;
            MaxPlayers = report.MaxPlayers;
            GameLevel = report.GameLevel;
            PlayerSkillLevel = report.PlayerSkillLevel;
            RulesSet = report.RulesSet;
            GenericField1 = report.GenericField1;
            GenericField2 = report.GenericField2;
            GenericField3 = report.GenericField3;

            // Once the world has been closed then we force it closed.
            // This is because when the host hits 'Play Again' they tell the server the world has closed (EndGameReport)
            // but the existing clients tell the server the world is still active.
            // This gives the host a "Game Name Already Exists" when they try to remake with the same name.
            // This just fixes that. At the cost of the game not showing after a host leaves a game.
            if (WorldStatus != MediusWorldStatus.WorldClosed && WorldStatus != report.WorldStatus)
                await SetWorldStatus(report.WorldStatus);
            else
            {
                // Update db
                if (!utcTimeEnded.HasValue)
                    _ = HorizonServerConfiguration.Database.UpdateGame(ToGameDTO());
            }

            LoggerAccessor.LogInfo("[Medius Game] - World Updated from World Report");
        }

        public virtual async Task OnWorldReportOnMe(MediusServerWorldReportOnMe report)
        {
            // Ensure report is for correct game world
            if (report.MediusWorldID != MediusWorldId)
                return;

            ApplicationId = report.ApplicationID;
            GameName = report.GameName;
            GameStats = report.GameStats;
            MinPlayers = report.MinClients;
            MaxPlayers = report.MaxClients;
            //PlayerCount = report.PlayerCount; //Not Needed at this moment
            GameLevel = report.GameLevel;
            PlayerSkillLevel = report.PlayerSkillLevel;
            RulesSet = report.RulesSet;
            GenericField1 = report.GenericField1;
            GenericField2 = report.GenericField2;
            GenericField3 = report.GenericField3;
            GenericField4 = report.GenericField4;
            GenericField5 = report.GenericField5;
            GenericField6 = report.GenericField6;
            GenericField7 = report.GenericField7;
            GenericField8 = report.GenericField8;

            // Once the world has been closed then we force it closed.
            // This is because when the host hits 'Play Again' they tell the server the world has closed (EndGameReport)
            // but the existing clients tell the server the world is still active.
            // This gives the host a "Game Name Already Exists" when they try to remake with the same name.
            // This just fixes that. At the cost of the game not showing after a host leaves a game.
            if (WorldStatus != MediusWorldStatus.WorldClosed && WorldStatus != report.WorldStatus)
                await SetWorldStatus(report.WorldStatus);
            else
            {
                // Update db
                if (!utcTimeEnded.HasValue)
                    _ = HorizonServerConfiguration.Database.UpdateGame(ToGameDTO());
            }
        }

        public virtual Task GameCreated()
        {
            return Task.CompletedTask;
        }

        public virtual async Task EndGame(int appid)
        {
            // destroy flag
            destroyed = true;

            LoggerAccessor.LogInfo($"Game {MediusWorldId}: {GameName}: EndGame() called.");

            // Send to plugins
            await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_DESTROYED, new OnGameArgs() { Game = this });

            // Remove players from game world
            while (LocalClients.Count > 0)
            {
                ClientObject? client = LocalClients[0].Client;
                if (client == null)
                {
                    lock (LocalClients)
                        LocalClients.RemoveAt(0);
                }
                else
                {
                    await client.LeaveGame(this);
                    if (ChatChannel != null)
                        await client.LeaveChannel(ChatChannel);
                }
            }

            // Unregister from channel
            ChatChannel?.UnregisterGame(this);

            // Send end game
            DMEServer?.Queue(new MediusServerEndGameRequest()
            {
                MediusWorldID = MediusWorldId,
                BrutalFlag = false
            });

            try
            {
                RoomManager.RemoveGame(appid.ToString(), WorldID.ToString(), GameName);
            }
            catch
            {
                // Not Important
            }

            // Delete db entry if game hasn't started
            // Otherwise do a final update
            if (!utcTimeStarted.HasValue)
                _ = HorizonServerConfiguration.Database.DeleteGame(MediusWorldId);
            else
                _ = HorizonServerConfiguration.Database.UpdateGame(ToGameDTO());
        }

        public virtual async Task SetWorldStatus(MediusWorldStatus status)
        {
            if (WorldStatus == status)
                return;

            _worldStatus = status;

            switch (status)
            {
                case MediusWorldStatus.WorldActive:
                    {
                        utcTimeStarted = Utils.GetHighPrecisionUtcTime();
                        accountIdsAtStart = GetActivePlayerList();

                        // Send to plugins
                        await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_STARTED, new OnGameArgs() { Game = this });
                        break;
                    }
                case MediusWorldStatus.WorldClosed:
                    {
                        utcTimeEnded = Utils.GetHighPrecisionUtcTime();

                        // Send to plugins
                        await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_ENDED, new OnGameArgs() { Game = this });

                        return;
                    }
            }

            // Update db
            if (!utcTimeEnded.HasValue)
                _ = HorizonServerConfiguration.Database.UpdateGame(ToGameDTO());
        }
    }
}
