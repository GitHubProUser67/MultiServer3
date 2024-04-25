using CustomLogger;
using Horizon.RT.Common;
using Horizon.RT.Models;
using Horizon.LIBRARY.Common;
using Horizon.LIBRARY.Database.Models;
using Horizon.MEDIUS.PluginArgs;
using System.Data;
using Horizon.PluginManager;
using System.Text.Json.Serialization;
using Horizon.MEDIUS;
using Horizon.MEDIUS.Medius.Models;

namespace Horizon.MUM
{
    public class Party
    {
        [JsonIgnore]
        private static int IdCounter = 1;

        public class PartyClient
        {
            public ClientObject? Client;

            public int DmeId;
            public bool InGame;
        }

        public int MediusWorldId = 0;
        public int ApplicationId = 0;
        public int WorldID;
        public List<PartyClient> LocalClients = new();
        public string? PartyName;
        public string? PartyPassword;
        public MediusGameHostType PartyHostType;
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
        public DMEObject DMEServer;
        public Channel? ChatChannel;
        public ClientObject? Host;

        public string? AccountIdsAtStart => accountIdsAtStart;
        public DateTime UtcTimeCreated => utcTimeCreated;
        public DateTime? UtcTimeStarted => utcTimeStarted;
        public DateTime? UtcTimeEnded => utcTimeEnded;

        public MediusWorldStatus WorldStatus = MediusWorldStatus.WorldPendingCreation;
        public bool hasHostJoined = false;
        public DateTime utcTimeCreated;
        public DateTime? utcTimeStarted;
        public DateTime? utcTimeEnded;
        public DateTime? utcTimeEmpty;

        protected string? accountIdsAtStart;
        protected bool destroyed = false;

        public uint Time => (uint)(Utils.GetHighPrecisionUtcTime() - utcTimeCreated).TotalMilliseconds;

        public int PlayerCount => LocalClients.Count(x => x != null && x.Client != null && x.Client.IsConnected && x.InGame);

        public Party(ClientObject client, IMediusRequest partyCreate, Channel? chatChannel, DMEObject dmeServer, int WorldId)
        {
            if (partyCreate is MediusPartyCreateRequest r)
                FromPartyCreateRequest(r);

            MediusWorldId = IdCounter++;

            utcTimeCreated = Utils.GetHighPrecisionUtcTime();
            utcTimeEmpty = null;
            ChatChannel = chatChannel;
            DMEServer = dmeServer;
            ChatChannel?.RegisterParty(this);
            Host = client;
            WorldID = WorldId;

            LoggerAccessor.LogInfo($"Party {MediusWorldId}: {PartyName}: Created by {client}");
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
                PartyId = MediusWorldId,
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
                Destroyed = destroyed
            };
        }

        private void FromPartyCreateRequest(MediusPartyCreateRequest partyCreate)
        {
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
        }

        public string GetActivePlayerList()
        {
            var playlist = LocalClients?.Select(x => x.Client?.AccountId.ToString()).Where(x => x != null);
            if (playlist != null)
                return string.Join(",", playlist);

            return string.Empty;
        }

        public virtual Task Tick()
        {
            // Remove timedout clients
            for (int i = 0; i < LocalClients.Count; ++i)
            {
                var client = LocalClients[i];

                if (client == null || client.Client == null || !client.Client.IsConnected || client.Client.CurrentGame?.MediusWorldId != MediusWorldId)
                {
                    LocalClients.RemoveAt(i);
                    --i;
                }
            }

            // Auto close when everyone leaves or if host fails to connect after timeout time
            if (!utcTimeEmpty.HasValue && !LocalClients.Any(x => x.InGame) && (hasHostJoined || (Utils.GetHighPrecisionUtcTime() - utcTimeCreated).TotalSeconds > MediusClass.GetAppSettingsOrDefault(ApplicationId).GameTimeoutSeconds))
                utcTimeEmpty = Utils.GetHighPrecisionUtcTime();

            return Task.CompletedTask;
        }

        public virtual async Task OnMediusServerConnectNotification(MediusServerConnectNotification notification)
        {
            var player = LocalClients.FirstOrDefault(x => x.Client?.SessionKey == notification.PlayerSessionKey);
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
            player.InGame = true;

            if (player.Client == Host)
                hasHostJoined = true;

            return Task.CompletedTask;
        }

        public virtual void AddPlayer(ClientObject client)
        {
            // Don't add again
            if (LocalClients.Any(x => x.Client == client))
                return;

            LoggerAccessor.LogInfo($"Party {MediusWorldId}: {PartyName}: {client} added.");

            LocalClients.Add(new PartyClient()
            {
                Client = client,
                DmeId = client.DmeClientId != null ? (int)client.DmeClientId : 0
            });

            // Inform the client of any custom game mode
            //client.CurrentChannel?.SendSystemMessage(client, $"Gamemode is {CustomGamemode?.FullName ?? "default"}.");
        }

        protected virtual async Task OnPlayerLeft(PartyClient player)
        {
            LoggerAccessor.LogInfo($"Party {MediusWorldId}: {PartyName}: {player.Client} left.");

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
            LoggerAccessor.LogInfo($"Party {MediusWorldId}: {PartyName}: {client} removed.");

            // Remove host
            if (Host == client)
                Host = null;

            // Remove from clients list
            LocalClients.RemoveAll(x => x.Client == client);
        }

        public virtual void OnPartyPlayerReport(MediusPartyPlayerReport report)
        {
            // Ensure report is for correct game world
            if (report.MediusWorldID != MediusWorldId)
                return;
        }

        public virtual Task PartyCreated()
        {
            return Task.CompletedTask;
        }

        public virtual async Task EndParty(int appid)
        {
            // destroy flag
            destroyed = true;

            LoggerAccessor.LogInfo($"Party {MediusWorldId}: {PartyName}: EndParty() called.");

            // Send to plugins
            await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_GAME_ON_DESTROYED, new OnPartyArgs() { Party = this });

            // Remove players from game world
            while (LocalClients.Count > 0)
            {
                ClientObject? client = LocalClients[0].Client;
                if (client == null)
                    LocalClients.RemoveAt(0);
                else
                    await client.LeaveParty(this);
                // client.LeaveChannel(ChatChannel);
            }

            // Unregister from channel
            ChatChannel?.UnregisterParty(this);

            // Send end game
            DMEServer?.Queue(new MediusServerEndGameRequest()
            {
                MediusWorldID = MediusWorldId,
                BrutalFlag = false
            });

            // Delete db entry if game hasn't started
            // Otherwise do a final update
            if (!utcTimeStarted.HasValue)
                _ = HorizonServerConfiguration.Database.DeleteParty(MediusWorldId);
            else
                _ = HorizonServerConfiguration.Database.UpdateParty(ToPartyDTO());
        }
    }
}
