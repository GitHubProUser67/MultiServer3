using CustomLogger;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Horizon.RT.Common;
using Horizon.RT.Cryptography;
using Horizon.RT.Models;
using Horizon.LIBRARY.Pipeline.Tcp;
using Horizon.LIBRARY.Common;
using Horizon.DME.Models;
using System.Collections.Concurrent;
using System.Net;
using Horizon.LIBRARY.Pipeline.Attribute;
using Horizon.MEDIUS;

namespace Horizon.DME
{
    public class MPSClient
    {
        public bool IsConnected => _mpsChannel != null && _mpsChannel.Active && _mpsState > 0;
        public DateTime? TimeLostConnection { get; set; } = null;
        public string? SessionKey = null;
        public string? AccessKey = null;
        public int ApplicationId { get; } = 0;

        private enum MPSConnectionState
        {
            FAILED = -1,
            NO_CONNECTION,
            CONNECTED,
            HELLO,
            HANDSHAKE,
            CONNECT_TCP,
            SET_ATTRIBUTES,
            PENDING_TCP_ACK,
            AUTHENTICATED
        }

        private ConcurrentDictionary<string, ClientObject> _appIdToClient = new();
        private ConcurrentDictionary<string, ClientObject> _accessTokenToClient = new();
        private ConcurrentDictionary<string, ClientObject> _sessionKeyToClient = new();

        private DateTime _utcConnectionState;
        private MPSConnectionState _mpsState = MPSConnectionState.NO_CONNECTION;

        private IEventLoopGroup? _group = null;
        private IChannel? _mpsChannel = null;
        private Bootstrap? _bootstrap = null;
        private ScertServerHandler? _scertHandler = null;

        private List<World> _worlds = new();
        private ConcurrentQueue<World> _removeWorldQueue = new();

        private ConcurrentQueue<BaseScertMessage> _mpsRecvQueue { get; } = new();
        private ConcurrentQueue<BaseScertMessage> _mpsSendQueue { get; } = new();

        public MPSClient(int appId, string? SessionKey, string? AccessKey)
        {
            this.SessionKey = SessionKey;
            this.AccessKey = AccessKey;
            ApplicationId = appId;
        }

        #region Clients
        public ClientObject? GetClientByAppId(string appId)
        {
            if (_accessTokenToClient.TryGetValue(appId, out var result))
                return result;

            return null;
        }
        public ClientObject? GetClientByAccessToken(string accessToken)
        {
            if (_accessTokenToClient.TryGetValue(accessToken, out var result))
                return result;

            return null;
        }
        public ClientObject? GetClientBySessionKey(string sessionKey)
        {
            if (_sessionKeyToClient.TryGetValue(sessionKey, out var result))
                return result;

            return null;
        }


        public void AddClient(ClientObject client)
        {
            if (client.Destroy)
                throw new InvalidOperationException($"Attempting to add {client} to MediusManager but client is ready to be destroyed.");

            if (_accessTokenToClient.TryAdd(client.Token ?? string.Empty, client))
            {
                if (!_sessionKeyToClient.TryAdd(client.SessionKey ?? string.Empty, client))
                    _accessTokenToClient.TryRemove(client.Token ?? string.Empty, out _);
            }
        }

        public void RemoveClient(ClientObject client)
        {
            if (client == null)
                return;

            _sessionKeyToClient.TryRemove(client.SessionKey ?? string.Empty, out _);
            _accessTokenToClient.TryRemove(client.Token ?? string.Empty, out _);
        }

        #endregion

        #region MPS Client

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
                LoggerAccessor.LogError("Lost connection to MPS");
                TimeLostConnection = Utils.GetHighPrecisionUtcTime();
                await Stop();
            };

            // Queue all incoming messages
            _scertHandler.OnChannelMessage += (channel, message) =>
            {
                // Add to queue
                _mpsRecvQueue.Enqueue(message);

                // Log if id is set
                if (message.CanLog())
                    LoggerAccessor.LogDebug($"MPS {channel}: {message}");
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

            await ConnectMPS();
        }

        public async Task Stop()
        {
            await Task.WhenAll(_worlds.Select(x => x.Stop()));
            if (_mpsChannel != null)
            {
                await _mpsChannel.CloseAsync();
                _mpsChannel = null;
            }
            if (_group != null)
                await _group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));

            _worlds.Clear();
            _removeWorldQueue.Clear();
            _mpsRecvQueue.Clear();
            _mpsSendQueue.Clear();
            _mpsState = MPSConnectionState.NO_CONNECTION;
        }

        public bool CheckMPSConnectivity()
        {
            if (_mpsState == MPSConnectionState.FAILED ||
                (_mpsState != MPSConnectionState.AUTHENTICATED && (Utils.GetHighPrecisionUtcTime() - _utcConnectionState).TotalSeconds > 30))
            {
                LoggerAccessor.LogError("[DMEMediusManager] - HandleIncomingMessages() - MPS server is not authenticated!");
                return false;
            }
            else
                return true;
        }

        public async Task HandleIncomingMessages()
        {
            if (_mpsChannel == null)
                return;

            try
            {
                // Process all messages in queue
                while (_mpsRecvQueue.TryDequeue(out var message))
                {
                    try
                    {
                        await ProcessMessage(message, _mpsChannel);
                    }
                    catch (Exception e)
                    {
                        LoggerAccessor.LogError(e);
                    }
                }

                // Handle incoming for each world
                await Task.WhenAll(_worlds.Select(x => x.HandleIncomingMessages()));
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);
            }
        }

        public async Task HandleOutgoingMessages()
        {
            if (_mpsChannel == null)
                return;

            List<BaseScertMessage> responses = new();

            try
            {
                // Handle outgoing for each world
                await Task.WhenAll(_worlds.Select(x => x.HandleOutgoingMessages()));

                // Handle world removals
                while (_removeWorldQueue.TryDequeue(out var world))
                    _worlds.Remove(world);

                // Send if writeable
                if (_mpsChannel.IsWritable)
                {
                    // Add send queue to responses
                    while (_mpsSendQueue.TryDequeue(out var message))
                        responses.Add(message);

                    if (responses.Count > 0)
                        await _mpsChannel.WriteAndFlushAsync(responses);
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);
            }
        }

        private async Task ConnectMPS()
        {
            _utcConnectionState = Utils.GetHighPrecisionUtcTime();
            _mpsState = MPSConnectionState.NO_CONNECTION;

            try
            {
                if (_bootstrap != null)
                    _mpsChannel = await _bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(DmeClass.Settings.MPS.Ip) ?? MediusClass.SERVER_IP, DmeClass.Settings.MPS.Port));
            }
            catch (Exception)
            {
                LoggerAccessor.LogError($"Failed to connect to MPS");
                TimeLostConnection = Utils.GetHighPrecisionUtcTime();
                return;
            }

            if (_mpsChannel != null && !_mpsChannel.Active)
                return;

            _mpsState = MPSConnectionState.CONNECTED;

            if (_mpsChannel != null && !_mpsChannel.HasAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT))
                _mpsChannel.GetAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT).Set(new ScertClientAttribute());
            var scertClient = _mpsChannel?.GetAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();
            if (scertClient != null)
            {
                scertClient.RsaAuthKey = DmeClass.Settings.MPS.Key;
                scertClient.CipherService?.GenerateCipher(scertClient.RsaAuthKey);
            }

            RT_MSG_CLIENT_HELLO clientHello = new()
            {
                Parameters = new ushort[]
                {
                    2,
                    0x6e,
                    0x6d,
                    1,
                    1
                }
            };

            // Send hello
            if (_mpsChannel != null)
                await _mpsChannel.WriteAndFlushAsync(clientHello);

            _mpsState = MPSConnectionState.HELLO;
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
                        if (_mpsState != MPSConnectionState.HELLO)
                            throw new Exception($"Unexpected RT_MSG_SERVER_HELLO from server. {serverHello}");

                        // Send public key
                        Enqueue(new RT_MSG_CLIENT_CRYPTKEY_PUBLIC()
                        {
                            PublicKey = DmeClass.Settings.MPS.Key.N.ToByteArrayUnsigned().Reverse().ToArray()
                        });

                        _mpsState = MPSConnectionState.HANDSHAKE;
                        break;
                    }
                case RT_MSG_SERVER_CRYPTKEY_PEER serverCryptKeyPeer:
                    {
                        if (_mpsState != MPSConnectionState.HANDSHAKE)
                            throw new Exception($"Unexpected RT_MSG_SERVER_CRYPTKEY_PEER from server. {serverCryptKeyPeer}");

                        // generate new client session key
                        scertClient.CipherService?.GenerateCipher(CipherContext.RC_CLIENT_SESSION, serverCryptKeyPeer.SessionKey ?? Array.Empty<byte>());

                        if (_mpsChannel != null)
                            await _mpsChannel.WriteAndFlushAsync(new RT_MSG_CLIENT_CONNECT_TCP()
                            {
                                SessionKey = SessionKey,
                                AccessToken = AccessKey,
                                AppId = ApplicationId,
                                Key = DmeClass.GlobalAuthPublic
                            });

                        _mpsState = MPSConnectionState.CONNECT_TCP;
                        break;
                    }
                case RT_MSG_SERVER_CONNECT_ACCEPT_TCP serverConnectAcceptTcp:
                    {
                        if (_mpsState != MPSConnectionState.CONNECT_TCP)
                            throw new Exception($"Unexpected RT_MSG_SERVER_CONNECT_ACCEPT_TCP from server. {serverConnectAcceptTcp}");

                        if (_mpsChannel != null)
                            await _mpsChannel.WriteAndFlushAsync(new RT_MSG_CLIENT_CONNECT_READY_TCP()
                            {

                            });

                        _mpsState = MPSConnectionState.PENDING_TCP_ACK;
                        break;
                    }
                case RT_MSG_SERVER_CONNECT_COMPLETE serverComplete:
                    {
                        if (_mpsState != MPSConnectionState.PENDING_TCP_ACK)
                            throw new Exception($"Unexpected RT_MSG_SERVER_CONNECT_COMPLETE from server. {serverComplete}");

                        _mpsState = MPSConnectionState.AUTHENTICATED;
                        break;
                    }
                case RT_MSG_SERVER_CONNECT_REQUIRE serverRequire:
                    {
                        if (_mpsChannel != null)
                            await _mpsChannel.WriteAndFlushAsync(new RT_MSG_CLIENT_CONNECT_READY_REQUIRE()
                            {
                                ServReq = 0
                            });
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
                        if (_mpsChannel != null && cheatQuery.QueryType == CheatQueryType.DME_SERVER_CHEAT_QUERY_RAW_MEMORY)
                        {
                            byte[]? CheatQueryData = MemoryQuery.QueryValueFromOffset(cheatQuery.StartAddress, cheatQuery.Length);
                            await _mpsChannel.WriteAndFlushAsync(new RT_MSG_SERVER_CHEAT_QUERY()
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
                        _mpsState = MPSConnectionState.NO_CONNECTION;
                        break;
                    }
                default:
                    {
                        LoggerAccessor.LogWarn($"UNHANDLED MPS MESSAGE: {message}");

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
                case MediusServerCreateGameWithAttributesRequest createGameWithAttributesRequest:
                    {
                        try
                        {
                            if (createGameWithAttributesRequest.MessageID != null && createGameWithAttributesRequest.MessageID.Value.Contains('-'))
                            {
                                bool offseted = false;
                                int partyType = 0;
                                uint gameOrPartyId = 0;
                                int accountId = 0;
                                string msgId = string.Empty;

                                string[] messageParts = createGameWithAttributesRequest.MessageID.Value.Split('-');

                                if (messageParts.Length == 5) // This is an ugly hack, anonymous accounts can have a negative ID which messes up the traditional parser.
                                {
                                    offseted = true;
                                    gameOrPartyId = uint.Parse(messageParts[0]);
                                    accountId = -int.Parse(messageParts[2]);
                                    msgId = messageParts[3];
                                }
                                else if (uint.TryParse(messageParts[0], out gameOrPartyId) &&
                                    int.TryParse(messageParts[1], out accountId))
                                    msgId = messageParts[2];
                                else
                                {
                                    LoggerAccessor.LogWarn("[MPSClient] - MediusServerCreateGameWithAttributesRequest received an invalid MessageID, ignoring request...");
                                    break;
                                }

                                try
                                {
                                    if (offseted)
                                        partyType = int.Parse(messageParts[4]);
                                    else
                                        partyType = int.Parse(messageParts[3]);
                                }
                                catch
                                {
                                    // Not Important.
                                }

                                World world = new(this, createGameWithAttributesRequest.ApplicationID, createGameWithAttributesRequest.MaxClients, gameOrPartyId);
                                _worlds.Add(world);

                                Enqueue(new MediusServerCreateGameWithAttributesResponse()
                                {
                                    MessageID = new MessageId($"{world.WorldId}-{accountId}-{msgId}-{partyType}"),
                                    Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                                    WorldID = (int)createGameWithAttributesRequest.WorldID,
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            LoggerAccessor.LogWarn($"error at {e}");
                        };

                        break;
                    }
                case MediusServerJoinGameRequest joinGameRequest:
                    {
                        if (uint.TryParse(joinGameRequest.MessageID.Value.Split('-')[0], out uint gameOrPartyId))
                        {
                            World? world = _worlds.FirstOrDefault(x => x.WorldId == gameOrPartyId);
                            if (world == null)
                                Enqueue(new MediusServerJoinGameResponse()
                                {
                                    MessageID = joinGameRequest.MessageID,
                                    Confirmation = MGCL_ERROR_CODE.MGCL_INVALID_ARG,
                                });
                            else
                                Enqueue(await world.OnJoinGameRequest(joinGameRequest));
                        }
                        else
                        {
                            LoggerAccessor.LogWarn("[MPSClient] - joinGameRequest received an invalid MessageID, ignoring request...");

                            Enqueue(new MediusServerJoinGameResponse()
                            {
                                MessageID = joinGameRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_DME_ERROR,
                            });

                            break;
                        }
                        break;
                    }
                case MediusServerEndGameRequest endGameRequest:
                    {
                        _worlds.FirstOrDefault(x => x.WorldId == endGameRequest.MediusWorldID)?.OnEndGameRequest(endGameRequest);

                        break;
                    }
                default:
                    {
                        LoggerAccessor.LogWarn($"UNHANDLED MPS MESSAGE: {message}");

                        break;
                    }
            }
        }

        #endregion

        #region Queue

        public void Enqueue(BaseScertMessage message)
        {
            _mpsSendQueue.Enqueue(message);
        }

        public void Enqueue(IEnumerable<BaseScertMessage> messages)
        {
            foreach (var message in messages)
                _mpsSendQueue.Enqueue(message);
        }

        public void Enqueue(BaseMediusMessage message)
        {
            _mpsSendQueue.Enqueue(new RT_MSG_CLIENT_APP_TOSERVER() { Message = message });
        }

        #endregion

        #region World Manager

        public void RemoveWorld(World world)
        {
            if (world != null)
                _removeWorldQueue.Enqueue(world);
        }

        #endregion
    }
}
