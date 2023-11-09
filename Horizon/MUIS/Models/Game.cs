using CustomLogger;
using DotNetty.Common.Internal.Logging;
using CryptoSporidium.Horizon.RT.Common;
using CryptoSporidium.Horizon.RT.Models;
using CryptoSporidium.Horizon.LIBRARY.Common;
using CryptoSporidium.Horizon.LIBRARY.Database.Models;
using Horizon.MUIS.PluginArgs;
using System.Data;
using Horizon.PluginManager;

namespace Horizon.MUIS.Models
{
    public class Game
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Game>();

        public static int IdCounter = 1;

        public class GameClient
        {
            public ClientObject Client;

            public int DmeId;
            public bool InGame;
        }

        public int Id = 0;
        public int DMEWorldId = -1;
        public int ApplicationId = 0;
        public ChannelType ChannelType = ChannelType.Game;
        public List<GameClient> Clients = new List<GameClient>();
        public string GameName;
        public string GamePassword;
        public string SpectatorPassword;
        public byte[] GameStats = new byte[Constants.GAMESTATS_MAXLEN];
        public MediusGameHostType GameHostType;
        public MGCL_GAME_HOST_TYPE GAME_HOST_TYPE;
        public NetAddressList netAddressList;
        public int WorldID;
        public int AccountID;
        public int MinPlayers;
        public int MaxPlayers;
        public int GameLevel;
        public int PlayerSkillLevel;
        public int RulesSet;
        public string Metadata;
        public int GenericField1;
        public int GenericField2;
        public int GenericField3;
        public int GenericField4;
        public int GenericField5;
        public int GenericField6;
        public int GenericField7;
        public int GenericField8;
        public MediusWorldStatus WorldStatus => _worldStatus;
        public MediusWorldAttributesType Attributes;
        public DMEObject DMEServer;
        public Channel ChatChannel;
        public ClientObject Host;

        public string AccountIdsAtStart => accountIdsAtStart;
        public DateTime UtcTimeCreated => utcTimeCreated;
        public DateTime? UtcTimeStarted => utcTimeStarted;
        public DateTime? UtcTimeEnded => utcTimeEnded;

        protected MediusWorldStatus _worldStatus = MediusWorldStatus.WorldPendingCreation;
        public bool hasHostJoined = false;
        protected string accountIdsAtStart;
        protected DateTime utcTimeCreated;
        protected DateTime? utcTimeStarted;
        protected DateTime? utcTimeEnded;
        protected DateTime? utcTimeEmpty;
        protected bool destroyed = false;

        public uint Time => (uint)(Utils.GetHighPrecisionUtcTime() - utcTimeCreated).TotalMilliseconds;

        public int PlayerCount => Clients.Count(x => x != null && x.Client.IsConnected || x.InGame);

        public virtual bool ReadyToDestroy => WorldStatus == MediusWorldStatus.WorldClosed && utcTimeEmpty.HasValue && (Utils.GetHighPrecisionUtcTime() - utcTimeEmpty)?.TotalSeconds > 1f;

        public Game(ClientObject client, IMediusRequest createGame, Channel chatChannel, DMEObject dmeServer)
        {
            if (createGame is MediusCreateGameRequest r)
                FromCreateGameRequest(r);
            else if (createGame is MediusCreateGameRequest0 r0)
                FromCreateGameRequest0(r0);
            else if (createGame is MediusCreateGameRequest1 r1)
                FromCreateGameRequest1(r1);
            else if (createGame is MediusServerCreateGameOnMeRequest r2)
                FromCreateGameOnMeRequest(r2);
            else if (createGame is MediusServerCreateGameOnSelfRequest r3)
                FromCreateGameOnSelfRequest(r3);
            else if (createGame is MediusServerCreateGameOnSelfRequest0 r4)
                FromCreateGameOnSelfRequest0(r4);

            Id = IdCounter++;

            utcTimeCreated = Utils.GetHighPrecisionUtcTime();
            utcTimeEmpty = null;
            DMEServer = dmeServer;
            ChatChannel = chatChannel;
            ChatChannel?.RegisterGame(this);
            Host = client;
            SetWorldStatus(MediusWorldStatus.WorldPendingCreation).Wait();

            LoggerAccessor.LogInfo($"Game {Id}: {GameName}: Created by {client} | Host: {Host}");
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
                GameId = Id,
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

        private void FromCreateGameRequest(MediusCreateGameRequest createGame)
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
        }

        private void FromCreateGameRequest0(MediusCreateGameRequest0 createGame)
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
        }


        private void FromCreateGameRequest1(MediusCreateGameRequest1 createGame)
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
            Attributes = createGame.WorldAttributesType;
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

        public string GetActivePlayerList()
        {
            return string.Join(",", Clients?.Select(x => x.Client.AccountId.ToString()).Where(x => x != null));
        }

        public virtual async Task Tick()
        {
            // Remove timedout clients
            for (int i = 0; i < Clients.Count; ++i)
            {
                var client = Clients[i];

                if (client == null || client.Client == null || !client.Client.IsConnected || client.Client.CurrentGame?.Id != Id)
                {
                    //Logger.Warn($"REMOVING CLIENT: {client}\n IS: {client.Client}\n IS Connected?: {client.Client.IsConnected}\nClient CurrentGame ID: {client.Client.CurrentGame?.Id}\nGameId: {Id}\nMatch?: {client.Client.CurrentGame?.Id != Id}");
                    Clients.RemoveAt(i);
                    --i;
                }
            }

            // Auto close when everyone leaves or if host fails to connect after timeout time
            if (!utcTimeEmpty.HasValue && Clients.Count(x => x.InGame) == 0 && (hasHostJoined || (Utils.GetHighPrecisionUtcTime() - utcTimeCreated).TotalSeconds > MuisClass.GetAppSettingsOrDefault(ApplicationId).GameTimeoutSeconds))
            {
                //Logger.Warn($"utcTimeEmpty: {utcTimeEmpty.HasValue}");
                //Logger.Warn($"Clients.Count: {Clients.Count()} | utcTimeCreated TotalSeconds: {(Utils.GetHighPrecisionUtcTime() - utcTimeCreated).TotalSeconds}");
                //Logger.Warn($"hasHostJoined: {hasHostJoined}");

                utcTimeEmpty = Utils.GetHighPrecisionUtcTime();
                await SetWorldStatus(MediusWorldStatus.WorldClosed);
            }
        }

        public virtual async Task OnMediusServerConnectNotification(MediusServerConnectNotification notification)
        {
            var player = Clients.FirstOrDefault(x => x.Client.SessionKey == notification.PlayerSessionKey);

            //Logger.Warn($"CLIENTS LEN {Clients.Count()}");
            //Logger.Warn($"player: {player} \n CHECK!!!!");

            if (player == null)
                return;

            switch (notification.ConnectEventType)
            {
                case MGCL_EVENT_TYPE.MGCL_EVENT_CLIENT_CONNECT:
                    {
                        await OnPlayerJoined(player);
                        break;
                    }
                case MGCL_EVENT_TYPE.MGCL_EVENT_CLIENT_DISCONNECT:
                    {
                        await OnPlayerLeft(player);
                        break;
                    }
            }
        }

        public virtual async Task OnMediusJoinGameResponse(string Sessionkey)
        {
            GameClient player = Clients.FirstOrDefault(x => x.Client.SessionKey == Sessionkey);

            if (player == null)
                return;

            await OnPlayerJoined(player);
        }

        public virtual async Task OnMediusServerCreateGameOnMeRequest(IMediusRequest createGameOnMeRequest)
        {
            GameClient player = Clients.FirstOrDefault(x => x != null && x.Client.IsConnected);
            if (player == null)
                return;

            await OnPlayerJoined(player);
        }

        public virtual async Task OnPlayerJoined(GameClient player)
        {
            player.InGame = true;
            Logger.Warn("Player joined!");

            if (player.Client == Host)
            {
                Logger.Warn("Player added as HOST!");
                hasHostJoined = true;
            }

            // Send to plugins
            await MuisClass.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_JOINED_GAME, new OnPlayerGameArgs() { Player = player.Client, Game = this });
        }

        public virtual void AddPlayer(ClientObject client)
        {
            // Don't add again
            if (Clients.Any(x => x.Client == client))
                return;

            // 
            LoggerAccessor.LogInfo($"Game {Id}: {GameName}: {client} added.");

            Clients.Add(new GameClient()
            {
                Client = client,
                DmeId = client.DmeClientId ?? -1
            });

            // Inform the client of any custom game mode
            //client.CurrentChannel?.SendSystemMessage(client, $"Gamemode is {CustomGamemode?.FullName ?? "default"}.");
        }

        protected virtual async Task OnPlayerLeft(GameClient player)
        {
            // 
            LoggerAccessor.LogInfo($"Game {Id}: {GameName}: {player.Client} left.");

            // 
            player.InGame = false;

            // Update player object
            await player.Client.LeaveGame(this);
            // player.Client.LeaveChannel(ChatChannel);

            // Remove from collection
            await RemovePlayer(player.Client);
        }

        public virtual async Task RemovePlayer(ClientObject client)
        {
            // 
            LoggerAccessor.LogInfo($"Game {Id}: {GameName}: {client} removed.");

            // Remove host
            if (Host == client)
            {
                // Send to plugins
                await MuisClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_HOST_LEFT, new OnPlayerGameArgs() { Player = client, Game = this });

                Host = null;
            }

            // Send to plugins
            await MuisClass.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_LEFT_GAME, new OnPlayerGameArgs() { Player = client, Game = this });

            // Remove from clients list
            Clients.RemoveAll(x => x.Client == client);
        }

        public virtual async Task OnEndGameReport(MediusEndGameReport report)
        {
            await SetWorldStatus(MediusWorldStatus.WorldClosed);

            ///Send database EndGameReport info

            await EndGame();
        }

        public virtual async Task OnWorldReport(MediusWorldReport report)
        {
            // Ensure report is for correct game world
            if (report.MediusWorldID != Id)
                return;


            //Id = report.MediusWorldID;
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
            {
                await SetWorldStatus(report.WorldStatus);
            }
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
            if (report.MediusWorldID != Id)
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
            {
                await SetWorldStatus(report.WorldStatus);
            }
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

        public virtual async Task EndGame()
        {
            // destroy flag
            destroyed = true;

            // 
            LoggerAccessor.LogInfo($"Game {Id}: {GameName}: EndGame() called.");

            // Send to plugins
            await MuisClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_DESTROYED, new OnGameArgs() { Game = this });

            // Remove players from game world
            while (Clients.Count > 0)
            {
                var client = Clients[0].Client;
                if (client == null)
                {
                    Clients.RemoveAt(0);
                }
                else
                {
                    await client.LeaveGame(this);
                    // client.LeaveChannel(ChatChannel);
                }
            }


            // Unregister from channel
            ChatChannel?.UnregisterGame(this);

            // Send end game
            if (DMEWorldId > 0)
            {
                DMEServer?.Queue(new MediusServerEndGameRequest()
                {
                    WorldID = this.DMEWorldId,
                    BrutalFlag = false
                });
            }

            // Delete db entry if game hasn't started
            // Otherwise do a final update
            if (!utcTimeStarted.HasValue)
                _ = HorizonServerConfiguration.Database.DeleteGame(Id);
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
                        await MuisClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_STARTED, new OnGameArgs() { Game = this });
                        break;
                    }
                case MediusWorldStatus.WorldClosed:
                    {
                        utcTimeEnded = Utils.GetHighPrecisionUtcTime();

                        // Send to plugins
                        await MuisClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_ENDED, new OnGameArgs() { Game = this });

                        return;
                    }
            }

            // Update db
            if (!utcTimeEnded.HasValue)
                _ = HorizonServerConfiguration.Database.UpdateGame(ToGameDTO());
        }
    }
}
