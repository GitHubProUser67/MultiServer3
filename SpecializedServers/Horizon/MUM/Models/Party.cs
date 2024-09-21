using CustomLogger;
using Horizon.RT.Common;
using Horizon.RT.Models;
using Horizon.LIBRARY.Common;
using Horizon.LIBRARY.Database.Models;
using Horizon.SERVER.PluginArgs;
using System.Data;
using Horizon.PluginManager;
using System.Text.Json.Serialization;
using Horizon.SERVER;

namespace Horizon.MUM.Models
{
    public class Party
    {
        [JsonIgnore]
        private static int IdCounter = 1;

        private object _Lock = new();
        public class PartyClient
        {
            public ClientObject? Client;

            public int DmeId;
            public bool InGame;
        }

        public int MediusVersion = 0;
        public uint MediusWorldID = 0;
        public int ApplicationId = 0;
        public List<PartyClient> LocalClients = new();
        public string? PartyName;
        public string? PartyPassword;
        public MGCL_GAME_HOST_TYPE PartyHostType;
        public int MinPlayers;
        public int MaxPlayers;
        public string? Metadata;
        public int GenericField1;
        public int GenericField2;
        public int GenericField3;
        public int GenericField4;
        public int GenericField5;
        public int GenericField6;
        public int GenericField7;
        public int GenericField8;
        public MediusWorldAttributesType Attributes;
        public ClientObject DMEServer;
        public Channel? GameChannel;
        public ClientObject? Host;

        public string? AccountIdsAtStart => accountIdsAtStart;
        public DateTime UtcTimeCreated => utcTimeCreated;
        public DateTime? UtcTimeStarted => utcTimeStarted;
        public DateTime? UtcTimeEnded => utcTimeEnded;

        public MediusWorldStatus WorldStatus = MediusWorldStatus.WorldPendingCreation;
        public bool hasHostJoined = false;
        public DateTime utcTimeCreated;
        public DateTime? utcLastJoined;
        public DateTime? utcTimeStarted;
        public DateTime? utcTimeEnded;
        public DateTime? utcTimeEmpty;

        protected string? accountIdsAtStart;

        public uint Time => (uint)(Utils.GetHighPrecisionUtcTime() - utcTimeCreated).TotalMilliseconds;

        public int PlayerCount
        {
            get
            {
                lock (LocalClients)
                    return LocalClients.Count(x => x != null && x.Client != null && x.Client.IsConnected && x.InGame);
            }
        }

        public bool Destroyed = false;

        public virtual bool ReadyToDestroy => !Destroyed && WorldStatus == MediusWorldStatus.WorldClosed && utcTimeEmpty.HasValue && (Utils.GetHighPrecisionUtcTime() - utcTimeEmpty)?.TotalSeconds > 1f;

        public Party(ClientObject client, IMediusRequest partyCreate, ClientObject dmeServer)
        {
            MediusVersion = client.MediusVersion;

            if (partyCreate is MediusPartyCreateRequest r)
                FromPartyCreateRequest(r);

            utcTimeCreated = Utils.GetHighPrecisionUtcTime();
            utcTimeEmpty = null;
            DMEServer = dmeServer;
            GameChannel?.RegisterParty(this);
            Host = client;

            LoggerAccessor.LogInfo($"Party {MediusWorldID}: {PartyName}: Created by {client}");
        }

        public PartyDTO ToPartyDTO()
        {
            return new PartyDTO()
            {
                AppId = ApplicationId,
                PartyCreateDt = utcTimeCreated,
                PartyEndDt = utcTimeEnded,
                PartyStartDt = utcTimeStarted,
                GameHostType = PartyHostType.ToString(),
                PartyId = MediusWorldID,
                PartyName = PartyName,
                PartyPassword = PartyPassword,
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
                Metadata = Metadata,
                Destroyed = Destroyed
            };
        }

        private void FromPartyCreateRequest(MediusPartyCreateRequest partyCreate)
        {
            Channel gameChannel = new(partyCreate.ApplicationID, MediusVersion)
            {
                ApplicationId = partyCreate.ApplicationID,
                Name = partyCreate.PartyName,
                MinPlayers = partyCreate.MinPlayers,
                MaxPlayers = partyCreate.MaxPlayers,
                GenericField1 = (ulong)partyCreate.GenericField1,
                GenericField2 = (ulong)partyCreate.GenericField2,
                GenericField3 = (ulong)partyCreate.GenericField3,
                GenericField4 = (ulong)partyCreate.GenericField4,
                Password = partyCreate.PartyPassword,
                SecurityLevel = string.IsNullOrEmpty(partyCreate.PartyPassword) ? MediusWorldSecurityLevelType.WORLD_SECURITY_NONE : MediusWorldSecurityLevelType.WORLD_SECURITY_PLAYER_PASSWORD,
                GameHostType = partyCreate.PartyHostType,
                Type = ChannelType.Game
            };

            ApplicationId = partyCreate.ApplicationID;
            PartyName = partyCreate.PartyName;
            PartyPassword = partyCreate.PartyPassword;
            MinPlayers = partyCreate.MinPlayers;
            MaxPlayers = partyCreate.MaxPlayers;
            GenericField1 = partyCreate.GenericField1;
            GenericField2 = partyCreate.GenericField2;
            GenericField3 = partyCreate.GenericField3;
            GenericField4 = partyCreate.GenericField4;
            GenericField5 = partyCreate.GenericField5;
            GenericField6 = partyCreate.GenericField6;
            GenericField7 = partyCreate.GenericField7;
            GenericField8 = partyCreate.GenericField8;
            PartyHostType = partyCreate.PartyHostType;
            MediusWorldID = gameChannel.Id;

            GameChannel = gameChannel;

            MediusClass.Manager.AddChannel(gameChannel).Wait();
        }

        public string GetActivePlayerList()
        {
            IEnumerable<string?>? playlist;

            lock (LocalClients)
                playlist = LocalClients?.Select(x => x.Client?.AccountId.ToString()).Where(x => x != null);

            if (playlist != null)
                return string.Join(",", playlist);

            return string.Empty;
        }

        public virtual Task Tick()
        {
            // Remove timedout clients
            lock (LocalClients)
            {
                for (int i = 0; i < LocalClients.Count; ++i)
                {
                    var client = LocalClients[i];

                    if (client == null || client.Client == null || !client.Client.IsConnected || client.Client.CurrentGame?.MediusWorldId != MediusWorldID)
                    {
                        LocalClients.RemoveAt(i);
                        --i;
                    }
                }

                // Auto close when everyone leaves or if host fails to connect after timeout time
                if (!utcTimeEmpty.HasValue && !LocalClients.Any(x => x.InGame) && (hasHostJoined || (Utils.GetHighPrecisionUtcTime() - utcTimeCreated).TotalSeconds > MediusClass.GetAppSettingsOrDefault(ApplicationId).GameTimeoutSeconds))
                    utcTimeEmpty = Utils.GetHighPrecisionUtcTime();
            }

            return Task.CompletedTask;
        }

        public virtual async Task OnMediusServerConnectNotification(MediusServerConnectNotification notification)
        {
            PartyClient? player;

            lock (LocalClients)
                player = LocalClients.FirstOrDefault(x => x.Client?.SessionKey == notification.PlayerSessionKey);

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

        protected virtual Task OnPlayerJoined(PartyClient player)
        {
            utcLastJoined = DateTime.UtcNow;

            player.InGame = true;

            if (player.Client == Host)
                hasHostJoined = true;

            return Task.CompletedTask;
        }

        public virtual void AddPlayer(ClientObject client)
        {
            lock (LocalClients)
            {
                // Don't add again
                if (LocalClients.Any(x => x.Client == client))
                    return;

                LoggerAccessor.LogInfo($"Party {MediusWorldID}: {PartyName}: {client} added.");

                LocalClients.Add(new PartyClient()
                {
                    Client = client,
                    DmeId = client.DmeId
                });
            }

            // Inform the client of any custom game mode
            //client.CurrentChannel?.SendSystemMessage(client, $"Gamemode is {CustomGamemode?.FullName ?? "default"}.");
        }

        protected virtual async Task OnPlayerLeft(PartyClient player)
        {
            LoggerAccessor.LogInfo($"Party {MediusWorldID}: {PartyName}: {player.Client} left.");

            player.InGame = false;

            if (player.Client != null)
            {
                // Update player object
                await player.Client.LeaveParty(this);

                // Remove from collection
                RemovePlayer(player.Client);
            }
        }

        public virtual void RemovePlayer(ClientObject client)
        {
            LoggerAccessor.LogInfo($"Party {MediusWorldID}: {PartyName}: {client} removed.");

            // Remove host
            if (Host == client)
                Host = null;

            // Remove from clients list
            lock (LocalClients)
                LocalClients.RemoveAll(x => x.Client == client);
        }

        public virtual void OnPartyPlayerReport(MediusPartyPlayerReport report)
        {
            // Ensure report is for correct game world
            if (report.MediusWorldID != MediusWorldID)
                return;
        }

        public virtual Task PartyCreated()
        {
            return Task.CompletedTask;
        }

        public virtual Task EndParty(int appid)
        {
            lock (_Lock)
            {
                if (Destroyed)
                    return Task.CompletedTask;

                LoggerAccessor.LogInfo($"Party {MediusWorldID}: {PartyName}: EndParty() called.");

                // Send to plugins
                MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_DESTROYED, new OnPartyArgs() { Party = this }).Wait();

                // Remove players from game world
                lock (LocalClients)
                {
                    while (LocalClients.Count > 0)
                    {
                        ClientObject? client = LocalClients[0].Client;
                        if (client == null)
                            LocalClients.RemoveAt(0);
                        else
                            client.LeaveParty(this).Wait();
                    }
                }

                // Unregister from channel
                GameChannel?.UnregisterParty(this);

                // Send end game
                DMEServer?.Queue(new MediusServerEndGameRequest()
                {
                    MediusWorldID = MediusWorldID,
                    BrutalFlag = false
                });

                // destroy flag
                Destroyed = true;

                // Delete db entry if game hasn't started
                // Otherwise do a final update
                if (!utcTimeStarted.HasValue)
                    _ = HorizonServerConfiguration.Database.DeleteParty(MediusWorldID);
                else
                    _ = HorizonServerConfiguration.Database.UpdateParty(ToPartyDTO());
            }

            return Task.CompletedTask;
        }
    }
}
