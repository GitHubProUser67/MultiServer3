using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.RT.Models;
using BackendProject.Horizon.LIBRARY.Common;
using Horizon.MEDIUS;
using Horizon.MEDIUS.Medius.Models;
using Newtonsoft.Json;

namespace Horizon.MUM
{
    public enum ChannelType
    {
        Lobby,
        Game
    }

    public class Channel
    {
        [JsonIgnore]
        public static int IdCounter = 0;

        public List<ClientObject> LocalClients = new();
        public List<Channel> LocalChannels = new();

        public string LobbyIp = MediusClass.SERVER_IP.ToString();
        public string RegionCode = BackendProject.MiscUtils.GeoIPUtils.GetGeoCodeFromIP(MediusClass.SERVER_IP) ?? string.Empty;
        public int LobbyPort = MediusClass.LobbyServer.TCPPort;
        public int Id = 0;
        public int ApplicationId = 0;
        public ChannelType Type = ChannelType.Lobby;
        public string Name = "MediusLobby";
        public string? Password = null;
        public int MinPlayers = 1;
        public int MaxPlayers = 10;
        public int GameLevel = 0;
        public int PlayerSkillLevel = 0;
        public int RuleSet = 0;
        public MediusApplicationType AppType = MediusApplicationType.LobbyChatChannel;
        public MediusWorldSecurityLevelType SecurityLevel = MediusWorldSecurityLevelType.WORLD_SECURITY_NONE;
        public MediusLobbyFilterMaskLevelType LobbyFilterMaskLevelType = MediusLobbyFilterMaskLevelType.MediusLobbyFilterMaskLevel0;
        public ulong GenericField1 = 0;
        public ulong GenericField2 = 0;
        public ulong GenericField3 = 0;
        public ulong GenericField4 = 0;
        public MediusWorldGenericFieldLevelType GenericFieldLevel = MediusWorldGenericFieldLevelType.MediusWorldGenericFieldLevel0;
        public MediusGameHostType GameHostType;
        public MediusWorldStatus WorldStatus;

        public virtual bool ReadyToDestroy => Type == ChannelType.Game && (_removeChannel || (Utils.GetHighPrecisionUtcTime() - _timeCreated).TotalSeconds > MediusClass.GetAppSettingsOrDefault(ApplicationId).GameTimeoutSeconds && GameCount == 0 && PartyCount == 0);
        public virtual int PlayerCount => LocalClients.Count;
        public int GameCount => _games.Count;
        public int PartyCount => _parties.Count;

        public List<Game> _games = new();
        public List<Party> _parties = new();
        public DateTime _timeCreated = Utils.GetHighPrecisionUtcTime();

        protected bool _removeChannel = false;

        public Channel()
        {
            Id = IdCounter++;
        }

        public Channel(MediusCreateChannelRequest request)
        {
            Id = IdCounter++;

            ApplicationId = request.ApplicationID;
            Name = request.LobbyName;
            Password = request.LobbyPassword;
            SecurityLevel = string.IsNullOrEmpty(Password) ? MediusWorldSecurityLevelType.WORLD_SECURITY_NONE : MediusWorldSecurityLevelType.WORLD_SECURITY_PLAYER_PASSWORD;
            MaxPlayers = request.MaxPlayers;
            GenericField1 = request.GenericField1;
            GenericField2 = request.GenericField2;
            GenericField3 = request.GenericField3;
            GenericField4 = request.GenericField4;
            GenericFieldLevel = request.GenericFieldLevel;
        }

        public Channel(MediusCreateChannelRequest0 request)
        {
            Id = IdCounter++;

            ApplicationId = request.ApplicationID;
            MinPlayers = request.MinPlayers;
            MaxPlayers = request.MaxPlayers;
            GameLevel = request.GameLevel;
            Password = request.GamePassword;
            PlayerSkillLevel = request.PlayerSkillLevel;
            RuleSet = request.RuleSet;
            GenericField1 = request.GenericField1;
            GenericField2 = request.GenericField2;
            GenericField3 = request.GenericField3;
            GameHostType = request.GameHostType;
        }

        public Channel(MediusCreateChannelRequest1 request)
        {
            Id = IdCounter++;

            ApplicationId = request.ApplicationID;
            MaxPlayers = request.MaxPlayers;
            Name = request.LobbyName;
            Password = request.LobbyPassword;
        }

        private Task UpdateMumReport()
        {
            int index = MumChannelHandler.GetIndexOfLocalChannelByIdAndAppId(Id, ApplicationId);

            if (index != -1)
                _ = MumChannelHandler.UpdateMumChannels(index, this);
            else
                MumChannelHandler.AddMumChannelsList(this);

            foreach (Game game in _games)
            {
                index = MumGameHandler.GetIndexOfLocalGameByNameAndAppId(Name, game.GameName, ApplicationId);

                if (index != -1)
                    _ = MumGameHandler.UpdateMumGame(index, game);
            }

            foreach (Party party in _parties)
            {
                index = MumPartyHandler.GetIndexOfLocalPartyByNameAndAppId(Name, party.PartyName, ApplicationId);

                if (index != -1)
                    _ = MumPartyHandler.UpdateMumParty(index, party);
            }

            return Task.CompletedTask;
        }

        public virtual Task Tick()
        {
            // Remove inactive clients
            for (int i = 0; i < LocalClients.Count; ++i)
            {
                if (!LocalClients[i].IsConnected)
                {
                    LocalClients.RemoveAt(i);
                    --i;
                }
            }

            _ = UpdateMumReport();

            return Task.CompletedTask;
        }

        public virtual Task OnChannelCreate(Channel channel)
        {
            LocalChannels.Add(channel);

            // Also update MUM.
            _ = UpdateMumReport();

            return Task.CompletedTask;
        }

        public virtual Task OnPlayerJoined(ClientObject client)
        {
            LocalClients.Add(client);

            _ = UpdateMumReport();

            return Task.CompletedTask;
        }

        public virtual void OnPlayerLeft(ClientObject client)
        {
            LocalClients.RemoveAll(x => x == client);

            _ = UpdateMumReport();
        }

        #region Parties

        public virtual void RegisterParty(Party party)
        {
            _parties.Add(party);

            // Also update MUM.
            _ = MumPartyHandler.AddMumParty(party);

            _ = UpdateMumReport();
        }

        public virtual void UnregisterParty(Party party)
        {
            // Remove Party
            _parties.Remove(party);

            // If empty, just end channel
            if (_parties.Count == 0)
                _removeChannel = true;

            _ = UpdateMumReport();
        }
        #endregion

        #region Games
        public virtual void RegisterGame(Game game)
        {
            _games.Add(game);

            // Also update MUM.
            _ = MumGameHandler.AddMumGame(game);

            _ = UpdateMumReport();
        }

        public virtual void UnregisterGame(Game game)
        {
            // Remove game
            _games.Remove(game);

            // If empty, just end channel
            if (_games.Count == 0)
                _removeChannel = true;

            _ = UpdateMumReport();
        }
        #endregion

        #region Buddies
        /// <summary>
        /// Add to buddy list confirmation for only them to appear on your friends list
        /// </summary>
        /// <param name="source">The client making the request for confirmation</param>
        /// <param name="accountToAdd">The account to add as a buddy on the client</param>
        /// <param name="msg"></param>
        public void AddToBuddyListConfirmationSingleRequest(ClientObject source, ClientObject accountToAdd, MediusAddToBuddyListConfirmationRequest msg)
        {
            accountToAdd?.Queue(new MediusAddToBuddyListFwdConfirmationRequest()
            {
                MessageID = msg.MessageID,
                OriginatorAccountID = source.AccountId,
                OriginatorAccountName = source.AccountName,
                AddType = MediusBuddyAddType.AddSingle,
            });
        }

        /// <summary>
        /// Add to buddy list confirmation for both you and them to appear on both clients friends list
        /// </summary>
        /// <param name="source">The client making the request for confirmation</param>
        /// <param name="accountToAdd">The account to add as a buddy on the client</param>
        /// <param name="msg"></param>
        public void AddToBuddyListConfirmationSymmetricRequest(ClientObject source, ClientObject accountToAdd, MediusAddToBuddyListConfirmationRequest msg)
        {
            accountToAdd?.Queue(new MediusAddToBuddyListFwdConfirmationRequest()
            {
                MessageID = msg.MessageID,
                OriginatorAccountID = source.AccountId,
                OriginatorAccountName = source.AccountName,
                AddType = MediusBuddyAddType.AddSymmetric,
            });
        }

        /// <summary>
        /// Add to buddy list confirmation for only them to appear on your friends list
        /// </summary>
        /// <param name="source">The client making the request for confirmation</param>
        /// <param name="accountToAdd">The account to add as a buddy on the client</param>
        /// <param name="msg"></param>
        public void AddToBuddyListConfirmationSingleResponse(ClientObject source, ClientObject accountToAdd, MediusAddToBuddyListFwdConfirmationResponse msg)
        {
            accountToAdd?.Queue(new MediusAddToBuddyListConfirmationResponse()
            {
                MessageID = msg.MessageID,
                StatusCode = msg.StatusCode,
                TargetAccountID = source.AccountId,
                TargetAccountName = source.AccountName,
            });
        }

        /// <summary>
        /// Add to buddy list confirmation for both you and them to appear on both clients friends list
        /// </summary>
        /// <param name="source">The client making the request for confirmation</param>
        /// <param name="accountToAdd">The account to add as a buddy on the client</param>
        /// <param name="msg"></param>
        public void AddToBuddyListConfirmationSymmetricResponse(ClientObject source, ClientObject accountToAdd, MediusAddToBuddyListFwdConfirmationResponse msg)
        {
            accountToAdd?.Queue(new MediusAddToBuddyListConfirmationResponse()
            {
                MessageID = msg.MessageID,
                StatusCode = msg.StatusCode,
                TargetAccountID = source.AccountId,
                TargetAccountName = source.AccountName,
            });
        }

        #endregion

        #region Messages
        public void BroadcastBinaryMessage(ClientObject source, MediusBinaryMessage msg)
        {
            foreach (var client in LocalClients.Where(x => x != source))
            {
                client?.Queue(new MediusBinaryFwdMessage()
                {
                    MessageType = msg.MessageType,
                    OriginatorAccountID = source.AccountId,
                    Message = msg.Message
                });
            }
        }

        public void BroadcastBinaryMessage(ClientObject source, MediusBinaryMessage1 msg)
        {
            foreach (var client in LocalClients.Where(x => x != source))
            {
                client?.Queue(new MediusBinaryFwdMessage1()
                {
                    MessageID = msg.MessageID,
                    MessageType = msg.MessageType,
                    OriginatorAccountID = source.AccountId,
                    MessageSize = msg.MessageSize,
                    Message = msg.Message
                });
            }
        }

        #region GenericChatMessages
        public void BroadcastChatMessage(IEnumerable<ClientObject> targets, ClientObject source, string message)
        {
            foreach (var target in targets)
            {
                if (target.MediusVersion >= 112)
                {
                    target?.Queue(new MediusGenericChatFwdMessage1()
                    {
                        OriginatorAccountID = source.AccountId,
                        OriginatorAccountName = source.AccountName,
                        Message = message,
                        MessageType = MediusChatMessageType.Broadcast,
                        TimeStamp = Utils.GetUnixTime()
                    });
                }
                else
                {
                    target?.Queue(new MediusGenericChatFwdMessage()
                    {
                        OriginatorAccountID = source.AccountId,
                        OriginatorAccountName = source.AccountName,
                        Message = message,
                        MessageType = MediusChatMessageType.Broadcast,
                        TimeStamp = Utils.GetUnixTime()
                    });
                }
            }
        }

        public void WhisperChatMessage(IEnumerable<ClientObject> targets, ClientObject source, string message)
        {
            foreach (var target in targets)
            {
                if (target.MediusVersion >= 112)
                {
                    target?.Queue(new MediusGenericChatFwdMessage1()
                    {
                        OriginatorAccountID = source.AccountId,
                        OriginatorAccountName = source.AccountName,
                        Message = message,
                        MessageType = MediusChatMessageType.Whisper,
                        TimeStamp = Utils.GetUnixTime()
                    });
                }
                else
                {
                    target?.Queue(new MediusGenericChatFwdMessage()
                    {
                        OriginatorAccountID = source.AccountId,
                        OriginatorAccountName = source.AccountName,
                        Message = message,
                        MessageType = MediusChatMessageType.Whisper,
                        TimeStamp = Utils.GetUnixTime()
                    });
                }
            }
        }

        public void ClanChatMessage(IEnumerable<ClientObject> targets, ClientObject source, string message)
        {
            foreach (var target in targets)
            {
                if (target.MediusVersion >= 112)
                {
                    target?.Queue(new MediusGenericChatFwdMessage1()
                    {
                        OriginatorAccountID = source.AccountId,
                        OriginatorAccountName = source.AccountName,
                        Message = message,
                        MessageType = MediusChatMessageType.Clan,
                        TimeStamp = Utils.GetUnixTime()
                    });
                }
                else
                {
                    target?.Queue(new MediusGenericChatFwdMessage()
                    {
                        OriginatorAccountID = source.AccountId,
                        OriginatorAccountName = source.AccountName,
                        Message = message,
                        MessageType = MediusChatMessageType.Clan,
                        TimeStamp = Utils.GetUnixTime()
                    });
                }
            }
        }
        #endregion

        /// <summary>
        /// Send Server System Message 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void SendSystemMessage(ClientObject client, string message)
        {
            if (client.MediusVersion >= 112)
            {
                client.Queue(new MediusGenericChatFwdMessage1()
                {
                    OriginatorAccountID = 0,
                    OriginatorAccountName = "SYSTEM",
                    Message = message,
                    MessageType = MediusChatMessageType.Broadcast,
                    TimeStamp = Utils.GetUnixTime()
                });
            }
            else
            {
                client.Queue(new MediusGenericChatFwdMessage()
                {
                    OriginatorAccountID = 0,
                    OriginatorAccountName = "SYSTEM",
                    Message = message,
                    MessageType = MediusChatMessageType.Broadcast,
                    TimeStamp = Utils.GetUnixTime()
                });
            }
        }

        /// <summary>
        /// Send Server System Message 
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="message"></param>
        public void BroadcastSystemMessage(IEnumerable<ClientObject> targets, string message)
        {
            foreach (var target in targets)
            {
                if (target.MediusVersion >= 112)
                {
                    target?.Queue(new MediusGenericChatFwdMessage1()
                    {
                        OriginatorAccountID = 0,
                        OriginatorAccountName = "SYSTEM",
                        Message = message,
                        MessageType = MediusChatMessageType.Broadcast,
                        TimeStamp = Utils.GetUnixTime()
                    });
                }
                else
                {
                    target?.Queue(new MediusGenericChatFwdMessage()
                    {
                        OriginatorAccountID = 0,
                        OriginatorAccountName = "SYSTEM",
                        Message = message,
                        MessageType = MediusChatMessageType.Broadcast,
                        TimeStamp = Utils.GetUnixTime()
                    });
                }
            }
        }
        #endregion
    }
}