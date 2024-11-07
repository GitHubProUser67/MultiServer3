using CustomLogger;
using Horizon.LIBRARY.Common;
using Horizon.LIBRARY.Database.Models;
using Horizon.SERVER;
using Horizon.SERVER.PluginArgs;
using Horizon.PluginManager;
using Horizon.RT.Common;
using Horizon.RT.Models;
using System.Collections.Concurrent;
using System.Net;
using Horizon.SERVER.Extension.PlayStationHome;

namespace Horizon.MUM.Models
{
    public class ClientObject
    {
        protected static Random RNG = new();

        public IPAddress IP { get; protected set; } = IPAddress.Any;

        public IPAddress MuisIP { get; set; } = IPAddress.Any;

        public ConcurrentDictionary<string, Task> Tasks = new();

        public int MaxWorlds { get; protected set; } = 0;
        public int MaxPlayersPerWorld { get; protected set; } = 0;
        public int CurrentWorlds { get; protected set; } = 0;
        public int CurrentPlayers { get; protected set; } = 0;

        public MGCL_ALERT_LEVEL MGCL_ALERT_LEVEL { get; protected set; } = MGCL_ALERT_LEVEL.MGCL_ALERT_NONE;
        public MGCL_TRUST_LEVEL MGCL_TRUST_LEVEL { get; set; }
        public MGCL_SERVER_ATTRIBUTES MGCL_SERVER_ATTRIBUTES { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; } = 0;

        #region HOME
        /// <summary>
        /// 
        /// </summary>
        public HomeOffsetsJsonData? ClientHomeData { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public uint HomePointer = 0;

        /// <summary>
        /// 
        /// </summary>
        public string? SSFWid = null;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public bool IsOnRPCN = false;

        /// <summary>
        /// 
        /// </summary>
        public int DmeId { get; protected set; } = -1;

        /// <summary>
        /// 
        /// </summary>
        public ConcurrentQueue<BaseScertMessage> SendMessageQueue { get; } = new();

        /// <summary>
        /// 
        /// </summary>
        public MediusPlayerStatus PlayerStatus => GetStatus();

        /// <summary>
        /// 
        /// </summary>
        public MGCL_GAME_HOST_TYPE ServerType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? ServerVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MediusConnectionType MediusConnectionType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public NetConnectionType NetConnectionType { get; set; }

        /// <summary>
        /// Client's CharacterEncoding Type
        /// </summary>
        public MediusCharacterEncodingType CharacterEncoding { get; set; }

        /// <summary>
        /// Client's language type
        /// </summary>
        public MediusLanguageType LanguageType { get; set; }

        /// <summary>
        /// Client's Timezone if set
        /// </summary>
        public MediusTimeZone TimeZone { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int AccountId { get; protected set; } = -1;

        /// <summary>
        /// 
        /// </summary>
        public string? AccountName { get; protected set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public byte[] AccountStats { get; set; } = new byte[256];

        /// <summary>
        /// Anonymous Login Name for the duration of that session
        /// </summary>
        public string? AccountDisplayName { get; set; } = null;

        /// <summary>
        /// Current access token required to access the account.
        /// </summary>
        public string? AccessToken { get; protected set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public string? SessionKey { get; protected set; } = null;

        /// <summary>
        /// Unique MGCL hardcoded game identifer per Medius title
        /// </summary>
        public int ApplicationId { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public int LocationId { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public int MediusVersion { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public int? ClanId { get; set; } = -1;

        /// <summary>
        /// 
        /// </summary>
        public int MediusWorldID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int SignalId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? requestData { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int appDataSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? appData { get; set; }

        #region Lobby World Filters

        public uint FilterMask1;
        public uint FilterMask2;
        public uint FilterMask3;
        public uint FilterMask4;
        public MediusLobbyFilterType LobbyFilterType;
        public MediusLobbyFilterMaskLevelType FilterMaskLevel;

        public List<ChannelListFilter> ChannelListFilters = new List<ChannelListFilter>();

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public List<GameListFilter> GameListFilters = new();

        /// <summary>
        /// 
        /// </summary>
        public Channel? CurrentChannel { get; protected set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public Game? CurrentGame { get; protected set; } = null;

        /// <summary>
        /// Current Party
        /// </summary>
        public Party? CurrentParty { get; protected set; } = null;

        public int? PartyIndex { get; protected set; } = null;

        /// <summary>
        /// 
        /// </summary>
        public DateTime UtcLastServerEchoSent { get; set; } = Utils.GetHighPrecisionUtcTime();

        /// <summary>
        /// 
        /// </summary>
        public DateTime UtcLastMessageReceived { get; protected set; } = Utils.GetHighPrecisionUtcTime();

        /// <summary>
        /// 
        /// </summary>
        public string? Metadata { get; set; } = null;

        /// <summary>
        /// RTT (ms)
        /// </summary>
        public uint LatencyMs { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime TimeCreated { get; protected set; } = Utils.GetHighPrecisionUtcTime();

        /// <summary>
        /// 
        /// </summary>
        public List<string>? FriendsListPS3 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<int, string?>? FriendsList { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int[] Stats { get; set; } = new int[15];

        /// <summary>
        /// 
        /// </summary>
        public int[]? WideStats { get; set; } = new int[100];

        /// <summary>
        /// 
        /// </summary>
        public int[]? CustomWideStats { get; set; } = new int[0];

        /// <summary>
        /// 
        /// </summary>
        public UploadState? Upload { get; set; }

        /// <summary>
        /// File being Uploaded
        /// </summary>
        public MediusFile? mediusFileToUpload;

        /// <summary>
        /// 
        /// </summary>
        public bool HasJoined { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public int TimeoutSeconds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LongTimeoutSeconds { get; set; }

        public virtual bool IsLoggedIn => !_logoutTime.HasValue && _loginTime.HasValue && IsConnected;
        public virtual bool IsInGame => CurrentGame != null && CurrentChannel != null && CurrentChannel.Type == ChannelType.Game;
        public virtual bool IsActiveServer => _hasServerSession;
        public virtual bool Timedout => (Utils.GetHighPrecisionUtcTime() - UtcLastMessageReceived).TotalSeconds > TimeoutSeconds;
        public virtual bool LongTimedout => (Utils.GetHighPrecisionUtcTime() - UtcLastMessageReceived).TotalSeconds > LongTimeoutSeconds;
        public virtual bool IsConnected => KeepAlive || (_hasSocket && !LongTimedout);
        public bool KeepAlive => _keepAliveTime.HasValue && (Utils.GetHighPrecisionUtcTime() - _keepAliveTime).Value.TotalSeconds < MediusClass.GetAppSettingsOrDefault(ApplicationId).KeepAliveGracePeriodSeconds;

        /// <summary>
        /// 
        /// </summary>
        protected DateTime? _loginTime = null;

        /// <summary>
        /// 
        /// </summary>
        protected DateTime? _logoutTime = null;

        /// <summary>
        /// 
        /// </summary>
        protected bool _hasServerSession = false;

        /// <summary>
        /// 
        /// </summary>
        private uint _gameListFilterIdCounter = 1;

        /// <summary>
        /// 
        /// </summary>
        private bool _hasSocket = false;

        /// <summary>
        /// 
        /// </summary>
        protected DateTime? _keepAliveTime = null;

        /// <summary>
        /// 
        /// </summary>
        private DateTime _lastServerEchoValue = DateTime.UnixEpoch;

        /// <summary>
        /// 
        /// </summary>
        private DateTime? _lastForceDisconnect = null;

        public ClientObject(int MediusVersion)
        {
            this.MediusVersion = MediusVersion;

            // Generate new session key
            SessionKey = MediusClass.GenerateSessionKey();

            // Generate new token
            byte[] tokenBuf = new byte[12];
            RNG.NextBytes(tokenBuf);
            AccessToken = Convert.ToBase64String(tokenBuf);

            // default last echo to creation of client object
            if (MediusVersion <= 108)
            {
                // reply must be before sent for the timeout to work
                UtcLastServerEchoSent = Utils.GetHighPrecisionUtcTime().AddSeconds(1);
                UtcLastMessageReceived = Utils.GetHighPrecisionUtcTime();
            }
            else
                UtcLastMessageReceived = UtcLastServerEchoSent = Utils.GetHighPrecisionUtcTime();

            TimeoutSeconds = MediusClass.GetAppSettingsOrDefault(ApplicationId).ClientTimeoutSeconds;
            LongTimeoutSeconds = MediusClass.GetAppSettingsOrDefault(ApplicationId).ClientLongTimeoutSeconds;
        }

        public void QueueServerEcho()
        {
            SendMessageQueue.Enqueue(new RT_MSG_SERVER_ECHO());
            UtcLastServerEchoSent = Utils.GetHighPrecisionUtcTime();
        }

        public void OnRecvServerEcho(RT_MSG_SERVER_ECHO echo)
        {
            DateTime echoTime = echo.UnixTimestamp.ToUtcDateTime();
            if (echoTime > _lastServerEchoValue)
            {
                _lastServerEchoValue = echoTime;
                LatencyMs = (uint)(Utils.GetHighPrecisionUtcTime() - echoTime).TotalMilliseconds;
            }
        }

        public void OnRecvClientEcho(RT_MSG_CLIENT_ECHO echo)
        {
            // older medius doesn't use server echo
            // so instead we'll increment our timeout dates by the client echo
            if (MediusVersion <= 108)
                // reply must be before sent for the timeout to work
                UtcLastServerEchoSent = Utils.GetHighPrecisionUtcTime().AddSeconds(1);
        }

        public virtual void OnRecv(BaseScertMessage msg)
        {
            UtcLastMessageReceived = Utils.GetHighPrecisionUtcTime();
        }

        public virtual void OnFileDownloadResponse(MediusFileDownloadResponse statsRequest)
        {
            // Set Stats from stats_ file
            //AccountStats = statsRequest.Data; 
        }

        public virtual void OnPlayerReport(MediusPlayerReport report)
        {
            // Ensure report is for correct game world
            if (report.MediusWorldID != MediusWorldID)
                return;

            AccountStats = report.Stats;
        }

        public void OnServerReport(MediusServerReport report)
        {
            MaxWorlds = report.MaxWorlds;
            MaxPlayersPerWorld = report.MaxPlayersPerWorld;
            CurrentWorlds = report.ActiveWorldCount;
            CurrentPlayers = report.TotalActivePlayers;
            MGCL_ALERT_LEVEL = report.AlertLevel;
        }

        #region Session

        public void OnConnected()
        {
            _keepAliveTime = null;
            _hasSocket = true;
        }

        public void OnDisconnected()
        {
            _hasSocket = false;
        }

        /// <summary>
        /// Begin DME Session
        /// </summary>
        public void BeginServerSession()
        {
            _hasServerSession = true;
        }

        /// <summary>
        /// End DME Session
        /// </summary>
        public void EndServerSession()
        {
            _hasServerSession = false;
        }

        public void ForceDisconnect()
        {
            DateTime now = Utils.GetHighPrecisionUtcTime();
            if ((now - _lastForceDisconnect)?.TotalSeconds < 5)
                return;

            LoggerAccessor.LogWarn($"Force disconnecting client {this}");
            Queue(new RT_MSG_CLIENT_DISCONNECT_WITH_REASON() { Reason = 0 });
            _lastForceDisconnect = now;
        }

        public void KeepAliveUntilNextConnection()
        {
            _keepAliveTime = Utils.GetHighPrecisionUtcTime();
        }

        #endregion

        #region Status

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private MediusPlayerStatus GetStatus()
        {
            if (!IsConnected || !IsLoggedIn)
                return MediusPlayerStatus.MediusPlayerDisconnected;

            if (IsInGame)
                return MediusPlayerStatus.MediusPlayerInGameWorld;

            return MediusPlayerStatus.MediusPlayerInChatWorld;

            /* // Needs proper handling between Universes for MUIS
            if (IsInOtherUniverse)
                return MediusPlayerStatus.MediusPlayerInOtherUniverse;
            */
        }

        /// <summary>
        /// Posts current account status to database.
        /// </summary>
        protected virtual void PostStatus()
        {
            _ = HorizonServerConfiguration.Database.PostAccountStatus(new AccountStatusDTO()
            {
                AppId = ApplicationId,
                AccountId = AccountId,
                LoggedIn = IsLoggedIn,
                ChannelId = CurrentChannel?.Id,
                GameId = CurrentGame?.MediusWorldId,
                GameName = CurrentGame?.GameName,
                PartyId = CurrentParty?.MediusWorldID,
                PartyName = CurrentParty?.PartyName,
                WorldId = CurrentGame?.MediusWorldId ?? CurrentParty?.MediusWorldID
            });
        }

        #endregion

        #region Login / Logout

        /// <summary>
        /// 
        /// </summary>
        public async Task Logout()
        {
            // Prevent logout twice
            if (_logoutTime.HasValue || !_loginTime.HasValue)
                return;

            // Raise plugin event
            await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_LOGGED_OUT, new OnPlayerArgs() { Player = this });

            // Leave game
            await LeaveCurrentGame();

            // Leave channel
            await LeaveCurrentChannel();

            // Stop custom tasks
            await DisposeTasks();

            // Release home pointer
            HomePointer = 0;

            // Logout
            _logoutTime = Utils.GetHighPrecisionUtcTime();

            HTTPSERVICE.RoomManager.RemoveUser(AccountName);

            // Tell database
            PostStatus();
        }

        public async Task LoginAnonymous(MediusAnonymousLoginRequest anonymousLoginRequest, int iAccountID)
        {
            if (IsLoggedIn)
                throw new InvalidOperationException($"{this} attempting to log into temp session as {anonymousLoginRequest.SessionDisplayName} but is already logged in!");

            // Raise plugin event
            await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_LOGGED_IN, new OnPlayerArgs() { Player = this });

            AccountName = anonymousLoginRequest.SessionDisplayName;
            AccountDisplayName = anonymousLoginRequest.SessionDisplayName;
            AccountStats = anonymousLoginRequest.SessionDisplayStats;
            AccountId = iAccountID;
            SessionKey = anonymousLoginRequest?.SessionKey;

            // Login
            _loginTime = Utils.GetHighPrecisionUtcTime();

            // update timeout times
            TimeoutSeconds = MediusClass.GetAppSettingsOrDefault(ApplicationId).ClientTimeoutSeconds;
            LongTimeoutSeconds = MediusClass.GetAppSettingsOrDefault(ApplicationId).ClientLongTimeoutSeconds;

            // WE ARE ANONYMOUS SO DON'T POST TO DATABASE!!!!
        }

        public async Task Login(AccountDTO account)
        {
            if (!IsLoggedIn)
            {
                AccountId = account.AccountId;
                AccountName = account.AccountName;
                Metadata = account.Metadata;
                ClanId = account.ClanId;
                WideStats = account.AccountWideStats;
                CustomWideStats = account.AccountCustomWideStats;

                FriendsList = account.Friends?.ToDictionary(x => x.AccountId, x => x.AccountName) ?? new Dictionary<int, string>();

                // Raise plugin event
                await MediusClass.Plugins.OnEvent(PluginEvent.MEDIUS_PLAYER_ON_LOGGED_IN, new OnPlayerArgs() { Player = this });

                // Login
                _loginTime = Utils.GetHighPrecisionUtcTime();

                // update timeout times
                TimeoutSeconds = MediusClass.GetAppSettingsOrDefault(ApplicationId).ClientTimeoutSeconds;
                LongTimeoutSeconds = MediusClass.GetAppSettingsOrDefault(ApplicationId).ClientLongTimeoutSeconds;

                // Update last sign in date
                _ = HorizonServerConfiguration.Database.PostAccountSignInDate(AccountId, Utils.GetHighPrecisionUtcTime());

                HTTPSERVICE.RoomManager.AddOrUpdateUser(AccountName, ApplicationId);

                // Update database status
                PostStatus();
            }
            else
                LoggerAccessor.LogError($"{this} attempting to log into {account} but is already logged in!");
        }

        public async Task RefreshAccount()
        {
            var accountDto = await HorizonServerConfiguration.Database.GetAccountById(AccountId);
            if (accountDto != null)
            {
                FriendsList = accountDto.Friends?.ToDictionary(x => x.AccountId, x => x.AccountName);
                ClanId = accountDto.ClanId;
            }
        }

        #endregion

        #region Party

        public async Task JoinParty(Party party, int partyIndex)
        {
            // Leave current game
            await LeaveCurrentParty();

            CurrentParty = party;
            PartyIndex = partyIndex;
            CurrentParty.AddPlayer(this);

            // Tell database
            PostStatus();
        }

        public async Task LeaveParty(Party party)
        {
            if (CurrentParty != null && CurrentParty == party)
            {
                await LeaveCurrentParty();

                // Tell database
                PostStatus();
            }
        }

        private Task LeaveCurrentParty()
        {
            if (CurrentParty != null)
            {
                CurrentParty.RemovePlayer(this);
                CurrentParty = null;
            }
            PartyIndex = null;

            return Task.CompletedTask;
        }

        #endregion

        #region Game

        public async Task JoinGame(Game game, int dmeClientIndex)
        {
            // Leave current game
            await LeaveCurrentGame();

            CurrentGame = game;
            DmeId = dmeClientIndex;
            CurrentGame.AddPlayer(this);

            // Tell database
            PostStatus();
        }

        public async Task JoinGameP2P(Game game)
        {
            // Leave current game
            await LeaveCurrentGame();

            CurrentGame = game;
            CurrentGame.AddPlayer(this);

            // Tell database
            PostStatus();
        }

        public async Task LeaveGame(Game game)
        {
            if (CurrentGame != null && CurrentGame == game)
            {
                await LeaveCurrentGame();

                // Tell database
                PostStatus();
            }
        }

        private async Task LeaveCurrentGame()
        {
            if (CurrentGame != null)
            {
                await CurrentGame.RemovePlayer(this, CurrentGame.ApplicationId, CurrentGame.MediusWorldId.ToString());
                CurrentGame = null;
            }
            DmeId = -1;
        }

        #endregion

        #region Channel

        public async Task JoinChannel(Channel channel)
        {
#if DEBUG
            LoggerAccessor.LogInfo($"[ClientObject] - Leaving Channel: {CurrentChannel?.Id} | Joining Channel: {channel.Id}");
#endif
            // Leave current channel
            await LeaveCurrentChannel();

            CurrentChannel = channel;
            await CurrentChannel.OnPlayerJoined(this);

            // Tell database
            PostStatus();
        }

        public async Task LeaveChannel(Channel? channel)
        {
            if (CurrentChannel != null && CurrentChannel == channel)
            {
                await LeaveCurrentChannel();

                // Tell database
                PostStatus();
            }
        }

        private Task LeaveCurrentChannel()
        {
            if (CurrentChannel != null)
            {
                CurrentChannel.OnPlayerLeft(this);
                CurrentChannel = null;
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Game List Filter

        public Task SetLobbyWorldFilter(MediusSetLobbyWorldFilterRequest request)
        {
            FilterMask1 = request.FilterMask1;
            FilterMask2 = request.FilterMask2;
            FilterMask3 = request.FilterMask3;
            FilterMask4 = request.FilterMask4;
            LobbyFilterType = request.LobbyFilterType;
            FilterMaskLevel = request.FilterMaskLevel;

            return Task.CompletedTask;
        }

        public Task SetLobbyWorldFilter(MediusSetLobbyWorldFilterRequest1 request)
        {
            FilterMask1 = request.FilterMask1;
            FilterMask2 = request.FilterMask2;
            FilterMask3 = request.FilterMask3;
            FilterMask4 = request.FilterMask4;
            LobbyFilterType = request.LobbyFilterType;
            FilterMaskLevel = request.FilterMaskLevel;

            return Task.CompletedTask;
        }
        /*
        public GameListFilter SetLobbyWorldFilter(MediusSetLobbyWorldFilterRequest1 request)
        {
            GameListFilter result;

            GameListFilters.Add(result = new GameListFilter()
            {
                FieldID = _gameListFilterIdCounter++,
                Mask = request.Mask,
                BaselineValue = request.BaselineValue,
                ComparisonOperator = request.ComparisonOperator,
                FilterField = request.FilterField
            });

            /*
            if (request.FilterField == MediusGameListFilterField.MEDIUS_FILTER_LOBBY_WORLDID)
            {
                GameListFilters.Add(result = new GameListFilter()
                {
                    FieldID = _gameListFilterIdCounter++,
                    Mask = request.Mask,
                    BaselineValue = (int)WorldId,
                    ComparisonOperator = MediusComparisonOperator.EQUAL_TO,
                    FilterField = request.FilterField
                });
            }
            else
            {
                GameListFilters.Add(result = new GameListFilter()
                {
                    FieldID = _gameListFilterIdCounter++,
                    Mask = request.Mask,
                    BaselineValue = request.BaselineValue,
                    ComparisonOperator = request.ComparisonOperator,
                    FilterField = request.FilterField
                });
            }

            return result;
        }
        
            */
        public GameListFilter SetGameListFilter(MediusSetGameListFilterRequest request)
        {
            GameListFilter result;

            GameListFilters.Add(result = new GameListFilter()
            {
                FieldID = _gameListFilterIdCounter++,
                Mask = request.Mask,
                BaselineValue = (ulong)request.BaselineValue,
                ComparisonOperator = request.ComparisonOperator,
                FilterField = request.FilterField
            });

            /*
            if (request.FilterField == MediusGameListFilterField.MEDIUS_FILTER_LOBBY_WORLDID)
            {
                GameListFilters.Add(result = new GameListFilter()
                {
                    FieldID = _gameListFilterIdCounter++,
                    Mask = request.Mask,
                    BaselineValue = (int)WorldId,
                    ComparisonOperator = MediusComparisonOperator.EQUAL_TO,
                    FilterField = request.FilterField
                });
            }
            else
            {
                GameListFilters.Add(result = new GameListFilter()
                {
                    FieldID = _gameListFilterIdCounter++,
                    Mask = request.Mask,
                    BaselineValue = request.BaselineValue,
                    ComparisonOperator = request.ComparisonOperator,
                    FilterField = request.FilterField
                });
            }
            */

            return result;
        }

        public GameListFilter SetGameListFilter(MediusSetGameListFilterRequest0 request)
        {
            GameListFilter result;
            GameListFilters.Add(result = new GameListFilter()
            {
                FieldID = _gameListFilterIdCounter++,
                Mask = 0xFFFFFFF,
                BaselineValue = (ulong)request.BaselineValue,
                ComparisonOperator = request.ComparisonOperator,
                FilterField = request.FilterField
            });

            return result;
        }

        public void ClearGameListFilter(uint filterID)
        {
            GameListFilters.RemoveAll(x => x.FieldID == filterID);
        }

        public bool IsGameMatch(Game game)
        {
            return !GameListFilters.Any(x => !x.IsMatch(game));
        }

        #endregion

        #region Send Queue

        public void Queue(BaseScertMessage message)
        {
            SendMessageQueue.Enqueue(message);
        }

        public void Queue(IEnumerable<BaseScertMessage> messages)
        {
            foreach (var message in messages)
                Queue(message);
        }

        public void Queue(BaseMediusMessage message)
        {
            if (NetConnectionType == NetConnectionType.NetConnectionTypeClientListenerTCP ||
               NetConnectionType == NetConnectionType.NetConnectionTypeClientListenerTCPAuxUDP ||
               NetConnectionType == NetConnectionType.NetConnectionTypeClientListenerUDP)
                LoggerAccessor.LogWarn("\"Can't send on a client listener connection type\"");
            else
                Queue(new RT_MSG_SERVER_APP() { Message = message });
        }

        public void Queue(BaseMediusPluginMessage message)
        {
            Queue(new RT_MSG_SERVER_PLUGIN_TO_APP() { Message = message });
        }

        public void Queue(IEnumerable<BaseMediusMessage> messages)
        {
            Queue(messages.Select(x => new RT_MSG_SERVER_APP() { Message = x }));
        }

        #endregion

        #region SetIP
        public void SetIp(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return;

            switch (Uri.CheckHostName(ip))
            {
                case UriHostNameType.IPv4:
                    {
                        IP = IPAddress.Parse(ip);
                        break;
                    }
                case UriHostNameType.IPv6:
                    {
                        IP = IPAddress.Parse(ip).MapToIPv4();
                        break;
                    }
                case UriHostNameType.Dns:
                    {
                        IP = Dns.GetHostAddresses(ip).FirstOrDefault()?.MapToIPv4() ?? IPAddress.Any;
                        break;
                    }
                default:
                    {
                        LoggerAccessor.LogError($"Unhandled UriHostNameType {Uri.CheckHostName(ip)} from {ip} in ClientObject.SetIp()");
                        break;
                    }
            }
        }

        public void SetIpPort(NetAddress ListenServerAddress)
        {
            if (IPAddress.TryParse(ListenServerAddress.Address, out IPAddress? addr) && addr != null)
            {
                IP = addr;
                Port = ListenServerAddress.Port;
            }
            else
                LoggerAccessor.LogError($"Unhandled NetAddress {ListenServerAddress} in ClientObject.SetIpPort()");
        }
        #endregion

        #region SetHomePointer
        public void SetPointer(uint Pointer)
        {
            HomePointer = Pointer;
        }
        #endregion

        public Task DisposeTasks()
        {
            foreach (Task task in Tasks.Values)
            {
                try
                {
                    task.Dispose();
                }
                catch
                {

                }
            }

            return Task.CompletedTask;
        }

        /*
        public Task<RT_RESULT> rt_msg_server_check_protocol_compatibility(ushort clientVersion, int p_compatible)
        {
            if(clientVersion <= 113)
            {

                LoggerAccessor.LogInfo($"rt_msg_server_check_protocol_compatibility: client_version {clientVersion}, p_compatible {p_compatible}");
            }
            return 
        }
        */

        public override string ToString()
        {
            return $"[ ({AccountId}:{AccountName}:{ApplicationId}:{DmeId}) ({IP}:{Port}) ({SessionKey}:{AccessToken}) ]";
        }
    }

    public class UploadState
    {
        public FileStream? Stream { get; set; }
        public int FileId { get; set; }
        public int PacketNumber { get; set; }
        public int TotalSize { get; set; }
        public int BytesReceived { get; set; }
        public DateTime TimeBegan { get; set; } = DateTime.UtcNow;
    }
}
