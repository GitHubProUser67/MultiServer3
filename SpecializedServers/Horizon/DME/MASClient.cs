using CustomLogger;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Horizon.RT.Common;
using Horizon.RT.Cryptography;
using Horizon.RT.Models;
using Horizon.LIBRARY.Pipeline.Tcp;
using Horizon.LIBRARY.Common;
using System.Collections.Concurrent;
using System.Net;
using Horizon.LIBRARY.Pipeline.Attribute;
using Horizon.MEDIUS;

namespace Horizon.DME
{
    public class MASClient
    {
        public TaskCompletionSource<bool> _masConnectionTaskCompletionSource = new();

        public bool IsConnected => _masChannel != null && _masChannel.Active && _masState > 0;
        public DateTime? TimeLostConnection { get; set; } = null;
        public string? SessionKey = null;
        public string? AccessKey = null;
        public int ApplicationId { get; } = 0;

        private enum MASConnectionState
        {
            FAILED = -1,
            NO_CONNECTION,
            CONNECTED,
            HELLO,
            HANDSHAKE,
            CONNECT_TCP,
            ACK_TCP,
            AUTHENTICATED
        }

        private DateTime _utcConnectionState;
        private MASConnectionState _masState = MASConnectionState.NO_CONNECTION;

        private IEventLoopGroup? _group = null;
        private IChannel? _masChannel = null;
        private Bootstrap? _bootstrap = null;
        private ScertServerHandler? _scertHandler = null;

        private ConcurrentQueue<BaseScertMessage> _masRecvQueue { get; } = new();
        private ConcurrentQueue<BaseScertMessage> _masSendQueue { get; } = new();

        public MASClient(int appId)
        {
            ApplicationId = appId;
        }

        #region MAS Client

        public async Task Start()
        {
            _group = new MultithreadEventLoopGroup();
            _scertHandler = new ScertServerHandler();

            TimeLostConnection = null;

            // Add client on connect
            _scertHandler.OnChannelActive += (channel) =>
            {

            };

            // Remove client on disconnect
            _scertHandler.OnChannelInactive += async (channel) =>
            {
                LoggerAccessor.LogWarn("MAS was disconnected or lost connection.");
                TimeLostConnection = Utils.GetHighPrecisionUtcTime();
                await Stop();
            };

            // Queue all incoming messages
            _scertHandler.OnChannelMessage += (channel, message) =>
            {
                // Add to queue
                _masRecvQueue.Enqueue(message);

                // Log if id is set
                if (message.CanLog())
                    LoggerAccessor.LogDebug($"MAS {channel}: {message}");
            };

            _bootstrap = new Bootstrap();
            _bootstrap
                .Group(_group)
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

                    pipeline.AddLast(new ScertEncoder());
                    pipeline.AddLast(new ScertIEnumerableEncoder());
                    pipeline.AddLast(new ScertTcpFrameDecoder(DotNetty.Buffers.ByteOrder.LittleEndian, Constants.MEDIUS_MESSAGE_MAXLEN, 1, 2, 0, 0, false));
                    pipeline.AddLast(new ScertDecoder());
                    pipeline.AddLast(new ScertMultiAppDecoder());
                    pipeline.AddLast(_scertHandler);
                }));

            await ConnectMAS();

            _ = Task.Run(async () =>
            {
                Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(10)); // Set timeout to 10 seconds

                if (await Task.WhenAny(_masConnectionTaskCompletionSource.Task, timeoutTask) == timeoutTask)
                {
                    LoggerAccessor.LogError("[DMEMediusManager] - Start() - Failed to authenticate with the MAS server within 10 seconds, aborting client...");
                    await Stop(); // Status already as false.
                }
                else
                {
                    lock (DmeClass.MPSManagersQueue)
                    {
                        if (!DmeClass.MPSManagersQueue.ContainsKey(ApplicationId))
                            DmeClass.MPSManagersQueue.Add(ApplicationId, new MPSClient(ApplicationId, SessionKey, AccessKey));
                        else
                            DmeClass.MPSManagersQueue[ApplicationId] = new MPSClient(ApplicationId, SessionKey, AccessKey); // Mostly placebo, unless you start 1000+ MAS for same appid at the same time...
                    }
                }
            });
        }

        public async Task Stop()
        {
            if (_masChannel != null)
            {
                await _masChannel.CloseAsync();
                _masChannel = null;
            }
            if (_group != null)
                await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));

            _masRecvQueue.Clear();
            _masSendQueue.Clear();
            _masState = MASConnectionState.NO_CONNECTION;
        }

        public async Task StopAndClearStatus()
        {
            if (_masChannel != null)
            {
                await _masChannel.CloseAsync();
                _masChannel = null;
            }
            if (_group != null)
                await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));

            _masRecvQueue.Clear();
            _masSendQueue.Clear();
            _masState = MASConnectionState.NO_CONNECTION;

            _masConnectionTaskCompletionSource.SetResult(false);
        }

        public bool CheckMASConnectivity()
        {
            if (_masState == MASConnectionState.FAILED ||
                (_masState != MASConnectionState.AUTHENTICATED && (Utils.GetHighPrecisionUtcTime() - _utcConnectionState).TotalSeconds > 30))
            {
                LoggerAccessor.LogError("[DMEMediusManager] - HandleIncomingMessages() - MAS server is not authenticated!");
                return false;
            }
            else
                return true;
        }

        public async Task HandleIncomingMessages()
        {
            if (_masChannel == null)
                return;

            try
            {
                // Process all messages in queue
                while (_masRecvQueue.TryDequeue(out var message))
                {
                    try
                    {
                        await ProcessMessage(message, _masChannel);
                    }
                    catch (Exception e)
                    {
                        LoggerAccessor.LogError(e);
                    }
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);
            }
        }

        public async Task HandleOutgoingMessages()
        {
            if (_masChannel == null)
                return;

            List<BaseScertMessage> responses = new();

            try
            {
                // Send if writeable
                if (_masChannel.IsWritable)
                {
                    // Add send queue to responses
                    while (_masSendQueue.TryDequeue(out var message))
                        responses.Add(message);

                    if (responses.Count > 0)
                        await _masChannel.WriteAndFlushAsync(responses);
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);
            }
        }

        private async Task ConnectMAS()
        {
            _utcConnectionState = Utils.GetHighPrecisionUtcTime();
            _masState = MASConnectionState.NO_CONNECTION;

            try
            {
                if (_bootstrap != null)
                    _masChannel = await _bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(DmeClass.Settings.MAS.Ip) ?? MediusClass.SERVER_IP, DmeClass.Settings.MAS.Port));
            }
            catch (Exception)
            {
                LoggerAccessor.LogError($"Failed to connect to MAS");
                TimeLostConnection = Utils.GetHighPrecisionUtcTime();
                return;
            }

            if (_masChannel != null && !_masChannel.Active)
                return;

            _masState = MASConnectionState.CONNECTED;

            if (_masChannel != null && !_masChannel.HasAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT))
                _masChannel.GetAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT).Set(new ScertClientAttribute());
            var scertClient = _masChannel?.GetAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();
            if (scertClient != null)
            {
                scertClient.RsaAuthKey = DmeClass.Settings.MAS.Key;
                scertClient.CipherService?.GenerateCipher(scertClient.RsaAuthKey);
            }

            // Send hello
            if (_masChannel != null)
                await _masChannel.WriteAndFlushAsync(new RT_MSG_CLIENT_HELLO()
                {
                    Parameters = new ushort[]
                    {
                        2,
                        0x6e,
                        0x6d,
                        1,
                        1
                    }
                });

            _masState = MASConnectionState.HELLO;
        }

        private async Task ProcessMessage(BaseScertMessage message, IChannel serverChannel)
        {
            // Get ScertClient data
            var scertClient = serverChannel.GetAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();

            switch (message)
            {
                // Authentication
                case RT_MSG_SERVER_HELLO serverHello:
                    {
                        if (_masState != MASConnectionState.HELLO)
                            throw new Exception($"Unexpected RT_MSG_SERVER_HELLO from server. {serverHello}");

                        // Send public key
                        Enqueue(new RT_MSG_CLIENT_CRYPTKEY_PUBLIC()
                        {
                            PublicKey = DmeClass.Settings.MAS.Key.N.ToByteArrayUnsigned().Reverse().ToArray()
                        });

                        _masState = MASConnectionState.HANDSHAKE;
                        break;
                    }
                case RT_MSG_SERVER_CRYPTKEY_PEER serverCryptKeyPeer:
                    {
                        if (_masState != MASConnectionState.HANDSHAKE)
                            throw new Exception($"Unexpected RT_MSG_SERVER_CRYPTKEY_PEER from server. {serverCryptKeyPeer}");

                        // generate new client session key
                        scertClient.CipherService?.GenerateCipher(CipherContext.RC_CLIENT_SESSION, serverCryptKeyPeer.SessionKey ?? Array.Empty<byte>());

                        if (_masChannel != null)
                            await _masChannel.WriteAndFlushAsync(new RT_MSG_CLIENT_CONNECT_TCP()
                            {
                                AppId = ApplicationId,
                                Key = DmeClass.GlobalAuthPublic,
                                TargetWorldId = 1
                            });

                        _masState = MASConnectionState.CONNECT_TCP;
                        break;
                    }
                case RT_MSG_SERVER_CONNECT_ACCEPT_TCP serverConnectAcceptTcp:
                    {
                        if (_masState != MASConnectionState.CONNECT_TCP)
                            throw new Exception($"Unexpected RT_MSG_SERVER_CONNECT_ACCEPT_TCP from server. {serverConnectAcceptTcp}");

                        if (_masChannel != null)
                            await _masChannel.WriteAndFlushAsync(new RT_MSG_CLIENT_APP_TOSERVER()
                            {
                                Message = new MediusServerSessionBeginRequest()
                                {
                                    MessageID = new MessageId(),
                                    LocationID = 0,
                                    Port = DmeClass.TcpServer.Port,
                                    ApplicationID = ApplicationId,
                                    ServerVersion = string.Empty,
                                    ServerType = MGCL_GAME_HOST_TYPE.MGCLGameHostIntegratedServer
                                }
                            });

                        _masState = MASConnectionState.ACK_TCP;
                        break;
                    }
                case RT_MSG_SERVER_CONNECT_REQUIRE serverRequire:
                    {
                        if (_masChannel != null)
                            await _masChannel.WriteAndFlushAsync(new RT_MSG_CLIENT_CONNECT_READY_REQUIRE()
                            {
                                ServReq = 0
                            });
                        break;
                    }
                case RT_MSG_SERVER_CONNECT_COMPLETE serverComplete:
                    {
                        // Ignore.
                        break;
                    }
                case RT_MSG_SERVER_ECHO serverEcho:
                    {
                        Enqueue(serverEcho);
                        break;
                    }
                case RT_MSG_CLIENT_ECHO clientEcho:
                    {
                        Enqueue(new RT_MSG_CLIENT_ECHO() { Value = clientEcho.Value });
                        break;
                    }
                case RT_MSG_SERVER_CHEAT_QUERY cheatQuery:
                    {
                        // We can query Client RAM, isn't that neat ;).
                        if (_masChannel != null && cheatQuery.QueryType == CheatQueryType.DME_SERVER_CHEAT_QUERY_RAW_MEMORY)
                        {
                            byte[]? CheatQueryData = MemoryQuery.QueryValueFromOffset(cheatQuery.StartAddress, cheatQuery.Length);
                            await _masChannel.WriteAndFlushAsync(new RT_MSG_SERVER_CHEAT_QUERY()
                            {
                                QueryType = CheatQueryType.DME_SERVER_CHEAT_QUERY_RAW_MEMORY,
                                SequenceId = cheatQuery.SequenceId,
                                StartAddress = cheatQuery.StartAddress,
                                Data = CheatQueryData,
                                Length = (CheatQueryData != null) ? CheatQueryData.Length : 0
                            });
                        }
                        break;
                    }
                case RT_MSG_SERVER_APP serverApp:
                    {
                        if (serverApp.Message != null)
                            await ProcessMediusMessage(serverApp.Message, serverChannel);
                        break;
                    }

                case RT_MSG_SERVER_FORCED_DISCONNECT serverForcedDisconnect:
                case RT_MSG_CLIENT_DISCONNECT_WITH_REASON clientDisconnectWithReason:
                    {
                        await serverChannel.CloseAsync();
                        _masState = MASConnectionState.NO_CONNECTION;
                        break;
                    }
                default:
                    {
                        LoggerAccessor.LogWarn($"UNHANDLED MAS MESSAGE: {message}");

                        break;
                    }
            }

            return;
        }

        private async Task ProcessMediusMessage(BaseMediusMessage message, IChannel clientChannel)
        {
            if (message == null)
                return;

            switch (message)
            {
                case MediusServerSessionBeginResponse setServerSessionResponse:
                    {
                        if (_masState != MASConnectionState.ACK_TCP)
                            throw new Exception($"Unexpected MediusServerSessionBeginResponse from server. {setServerSessionResponse}");

                        if (_masChannel != null)
                            await _masChannel.WriteAndFlushAsync(new RT_MSG_CLIENT_DISCONNECT_WITH_REASON()
                            {

                            });

                        SessionKey = setServerSessionResponse.ConnectInfo.SessionKey;
                        AccessKey = setServerSessionResponse.ConnectInfo.AccessKey;

                        /* Ideally, we should contact the NAT server with given infos to get our IP:PORT.
                         * For now we simply use a MPS constant in config */

                        _masState = MASConnectionState.AUTHENTICATED;

                        _masConnectionTaskCompletionSource.SetResult(true);

                        break;
                    }
                default:
                    {
                        LoggerAccessor.LogWarn($"UNHANDLED MAS MESSAGE: {message}");

                        break;
                    }
            }
        }

        #endregion

        #region Queue

        public void Enqueue(BaseScertMessage message)
        {
            _masSendQueue.Enqueue(message);
        }

        public void Enqueue(IEnumerable<BaseScertMessage> messages)
        {
            foreach (var message in messages)
                _masSendQueue.Enqueue(message);
        }

        public void Enqueue(BaseMediusMessage message)
        {
            _masSendQueue.Enqueue(new RT_MSG_CLIENT_APP_TOSERVER() { Message = message });
        }

        #endregion
    }
}
