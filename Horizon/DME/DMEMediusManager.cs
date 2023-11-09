using CustomLogger;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using CryptoSporidium.Horizon.RT.Common;
using CryptoSporidium.Horizon.RT.Cryptography;
using CryptoSporidium.Horizon.RT.Models;
using Horizon.LIBRARY.Pipeline.Tcp;
using CryptoSporidium.Horizon.LIBRARY.Common;
using Horizon.DME.Models;
using System.Collections.Concurrent;
using System.Net;
using Horizon.LIBRARY.Pipeline.Attribute;

namespace Horizon.DME
{
    public class DMEMediusManager
    {
        public bool IsConnected => _mpsChannel != null && _mpsChannel.IsActive && _mpsState > 0;
        public DateTime? TimeLostConnection { get; set; } = null;
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
            AUTHENTICATED
        }

        private ConcurrentDictionary<string, ClientObject> _accessTokenToClient = new ConcurrentDictionary<string, ClientObject>();
        private ConcurrentDictionary<string, ClientObject> _sessionKeyToClient = new ConcurrentDictionary<string, ClientObject>();

        private DateTime _utcConnectionState;
        private MPSConnectionState _mpsState = MPSConnectionState.NO_CONNECTION;

        private IEventLoopGroup? _group = null;
        private IChannel? _mpsChannel = null;
        private Bootstrap? _bootstrap = null;
        private ScertServerHandler? _scertHandler = null;

        private List<World> _worlds = new List<World>();
        private ConcurrentQueue<World> _removeWorldQueue = new ConcurrentQueue<World>();

        private ConcurrentQueue<BaseScertMessage> _mpsRecvQueue { get; } = new ConcurrentQueue<BaseScertMessage>();
        private ConcurrentQueue<BaseScertMessage> _mpsSendQueue { get; } = new ConcurrentQueue<BaseScertMessage>();

        public DMEMediusManager(int appId)
        {
            ApplicationId = appId;
        }

        #region Clients

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

            if (_accessTokenToClient.TryAdd(client.Token, client))
            {
                if (!_sessionKeyToClient.TryAdd(client.SessionKey, client))
                {
                    _accessTokenToClient.TryRemove(client.Token, out _);
                }
            }
        }

        public void RemoveClient(ClientObject client)
        {
            if (client == null)
                return;

            _sessionKeyToClient.TryRemove(client.SessionKey, out _);
            _accessTokenToClient.TryRemove(client.Token, out _);
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
                await _mpsChannel.DisconnectAsync();
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
                LoggerAccessor.LogError("[DMEMediusManager] - HandleIncomingMessages() - Failed to authenticate with the MPS server, aborting listener...");
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

            List<BaseScertMessage> responses = new List<BaseScertMessage>();

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
                    _mpsChannel = await _bootstrap.ConnectAsync(new IPEndPoint(Misc.GetIp(DmeClass.Settings.MPS.Ip), DmeClass.Settings.MPS.Port));
            }
            catch (Exception)
            {
                LoggerAccessor.LogError($"Failed to connect to MPS");
                TimeLostConnection = Utils.GetHighPrecisionUtcTime();
                return;
            }

            if (_mpsChannel != null && !_mpsChannel.IsActive)
                return;

            _mpsState = MPSConnectionState.CONNECTED;

            if (_mpsChannel != null && !_mpsChannel.HasAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT))
                _mpsChannel.GetAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT).Set(new ScertClientAttribute());
            var scertClient = _mpsChannel.GetAttribute(LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();
            scertClient.RsaAuthKey = DmeClass.Settings.MPS.Key;
            scertClient.CipherService.GenerateCipher(scertClient.RsaAuthKey);

            var clientHello = new RT_MSG_CLIENT_HELLO()
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
                        scertClient.CipherService.GenerateCipher(CipherContext.RC_CLIENT_SESSION, serverCryptKeyPeer.SessionKey);

                        if (_mpsChannel != null)
                            await _mpsChannel.WriteAndFlushAsync(new RT_MSG_CLIENT_CONNECT_TCP()
                            {
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
                            // Send attributes
                            await _mpsChannel.WriteAndFlushAsync(new RT_MSG_CLIENT_APP_TOSERVER()
                            {
                                Message = new MediusServerSetAttributesRequest()
                                {
                                    MessageID = new MessageId(),
                                    ListenServerAddress = new NetAddress()
                                    {
                                        Address = DmeClass.SERVER_IP.ToString(),
                                        Port = DmeClass.TcpServer.Port
                                    }
                                }
                            });

                        _mpsState = MPSConnectionState.SET_ATTRIBUTES;
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
                case RT_MSG_SERVER_APP serverApp:
                    {
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
                        LoggerAccessor.LogWarn($"UNHANDLED MESSAGE: {message}");

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
                case MediusServerSetAttributesResponse setAttributesResponse:
                    {
                        if (_mpsState != MPSConnectionState.SET_ATTRIBUTES)
                            throw new Exception($"Unexpected MediusServerSetAttributesResponse from server. {setAttributesResponse}");

                        if (setAttributesResponse.Confirmation == MGCL_ERROR_CODE.MGCL_SUCCESS)
                            _mpsState = MPSConnectionState.AUTHENTICATED;
                        else
                            _mpsState = MPSConnectionState.FAILED;
                        break;
                    }

                case MediusServerCreateGameWithAttributesRequest createGameWithAttributesRequest:
                    {
                        try
                        {
                            World world = new World(this, createGameWithAttributesRequest.ApplicationID, createGameWithAttributesRequest.MaxClients);
                            _worlds.Add(world);

                            Enqueue(new MediusServerCreateGameWithAttributesResponse()
                            {
                                MessageID = createGameWithAttributesRequest.MessageID,
                                Confirmation = MGCL_ERROR_CODE.MGCL_SUCCESS,
                                WorldID = world.WorldId
                            });
                        }
                        catch (Exception e)
                        {
                            LoggerAccessor.LogWarn($"error at {e}");
                        };

                        break;
                    }
                case MediusServerJoinGameRequest joinGameRequest:
                    {
                        var world = _worlds.FirstOrDefault(x => x.WorldId == joinGameRequest.ConnectInfo.WorldID);
                        if (world == null)
                        {
                            if (ApplicationId == 20371)
                            {
                                World worldHome = new(this, 20371, 256);
                                _worlds.Add(worldHome);

                                Enqueue(await worldHome.OnJoinGameRequest(joinGameRequest));
                            }
                            else if (ApplicationId == 20374)
                            {
                                World worldHome = new(this, 20374, 256);
                                _worlds.Add(worldHome);

                                Enqueue(await worldHome.OnJoinGameRequest(joinGameRequest));
                            }
                            else
                            {
                                Enqueue(new MediusServerJoinGameResponse()
                                {
                                    MessageID = joinGameRequest.MessageID,
                                    Confirmation = MGCL_ERROR_CODE.MGCL_INVALID_ARG,
                                });
                            }
                        }
                        else
                            Enqueue(await world.OnJoinGameRequest(joinGameRequest));
                        break;
                    }
                case MediusServerEndGameRequest endGameRequest:
                    {
                        _worlds.FirstOrDefault(x => x.WorldId == endGameRequest.WorldID)?.OnEndGameRequest(endGameRequest);

                        break;
                    }
                default:
                    {
                        LoggerAccessor.LogWarn($"UNHANDLED MESSAGE: {message}");

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
