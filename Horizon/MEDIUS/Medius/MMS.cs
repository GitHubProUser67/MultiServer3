using CustomLogger;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.RT.Cryptography;
using BackendProject.Horizon.RT.Models;
using BackendProject.Horizon.LIBRARY.Pipeline.Tcp;
using System.Net;

namespace Horizon.MEDIUS.Medius
{
    /// <summary>
    /// Introduced in Medius 3.03
    /// </summary>
    public class MMS : BaseMediusComponent
    {
        public static Random RNG = new();
        public override int TCPPort => MediusClass.Settings.MMSTCPPort;
        public override int UDPPort => 0;
        public IPAddress IPAddress => MediusClass.SERVER_IP;

        protected IEventLoopGroup? _bossGroup = null;
        protected IEventLoopGroup? _workerGroup = null;
        protected IChannel? _boundChannel = null;
        protected ScertServerHandler? _scertHandler = null;
        private uint _clientCounter = 0;

        public MMS()
        {

        }

        /// <summary>
        /// Start the MMS TCP Server.
        /// </summary>
        public virtual async void Start()
        {
            _bossGroup = new MultithreadEventLoopGroup(1);
            _workerGroup = new MultithreadEventLoopGroup();
            _scertHandler = new ScertServerHandler();

            // Add client on connect
            _scertHandler.OnChannelActive += (channel) =>
            {
                string key = channel.Id.AsLongText();
                _channelDatas.TryAdd(key, new ChannelData());
            };

            // Remove client on disconnect
            _scertHandler.OnChannelInactive += (channel) =>
            {
                string key = channel.Id.AsLongText();
                _channelDatas.TryRemove(key, out var data);
            };

            // Queue all incoming messages
            _scertHandler.OnChannelMessage += (channel, message) =>
            {
                string key = channel.Id.AsLongText();
                if (_channelDatas.TryGetValue(key, out var data))
                    data.RecvQueue.Enqueue(message);

                // Log if id is set
                if (message.CanLog())
                    LoggerAccessor.LogInfo($"TCP RECV {channel}: {message}");
            };

            var bootstrap = new ServerBootstrap();
            bootstrap
                .Group(_bossGroup, _workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .Handler(new LoggingHandler(LogLevel.INFO))
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

                    pipeline.AddLast(new WriteTimeoutHandler(15));
                    pipeline.AddLast(new ScertEncoder());
                    pipeline.AddLast(new ScertIEnumerableEncoder());
                    pipeline.AddLast(new ScertTcpFrameDecoder(DotNetty.Buffers.ByteOrder.LittleEndian, 1024, 1, 2, 0, 0, false));
                    pipeline.AddLast(new ScertDecoder());
                    pipeline.AddLast(new ScertMultiAppDecoder());
                    pipeline.AddLast(_scertHandler);
                }));

            _boundChannel = await bootstrap.BindAsync(TCPPort);
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        public virtual async Task Stop()
        {
            try
            {
                if (_boundChannel != null)
                    await _boundChannel.CloseAsync();
            }
            finally
            {
                if (_bossGroup != null && _workerGroup != null)
                    await Task.WhenAll(
                        _bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                        _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }

        /// <summary>
        /// Process messages.
        /// </summary>
        public async Task Tick()
        {
            if (_scertHandler == null || _scertHandler.Group == null)
                return;

            await Task.WhenAll(_scertHandler.Group.Select(c => Tick(c)));
        }

        private async Task Tick(IChannel clientChannel)
        {
            if (clientChannel == null)
                return;

            List<BaseScertMessage> responses = new List<BaseScertMessage>();
            string key = clientChannel.Id.AsLongText();

            try
            {
                if (_channelDatas.TryGetValue(key, out var data))
                {
                    // Process all messages in queue
                    while (data.RecvQueue.TryDequeue(out var message))
                    {
                        try
                        {
                            await ProcessMessage(message, clientChannel, data);
                        }
                        catch (Exception e)
                        {
                            LoggerAccessor.LogError(e);
                        }
                    }

                    // Send if writeable
                    if (clientChannel.IsWritable)
                    {
                        // Add send queue to responses
                        while (data.SendQueue.TryDequeue(out var message))
                            responses.Add(message);

                        if (responses.Count > 0)
                            await clientChannel.WriteAndFlushAsync(responses);
                    }
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);
            }
        }

        #region Message Processing

        protected override async Task ProcessMessage(BaseScertMessage message, IChannel clientChannel, ChannelData data)
        {
            // Get ScertClient data
            var scertClient = clientChannel.GetAttribute(BackendProject.Horizon.LIBRARY.Pipeline.Constants.SCERT_CLIENT).Get();
            var enableEncryption = MediusClass.GetAppSettingsOrDefault(data.ApplicationId).EnableEncryption;
            scertClient.CipherService.EnableEncryption = enableEncryption;

            switch (message)
            {
                case RT_MSG_CLIENT_HELLO clientHello:
                    {
                        // send hello
                        Queue(new RT_MSG_SERVER_HELLO() { RsaPublicKey = false ? MediusClass.Settings.DefaultKey.N : Org.BouncyCastle.Math.BigInteger.Zero }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CRYPTKEY_PUBLIC clientCryptKeyPublic:
                    {
                        // generate new client session key
                        scertClient.CipherService.GenerateCipher(CipherContext.RSA_AUTH, clientCryptKeyPublic.PublicKey.Reverse().ToArray());
                        scertClient.CipherService.GenerateCipher(CipherContext.RC_CLIENT_SESSION);

                        Queue(new RT_MSG_SERVER_CRYPTKEY_PEER() { SessionKey = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_TCP clientConnectTcp:
                    {
                        #region Compatible AppId
                        if (!MediusClass.Manager.IsAppIdSupported(clientConnectTcp.AppId))
                        {
                            LoggerAccessor.LogError($"Client {clientChannel.RemoteAddress} attempting to authenticate with incompatible app id {clientConnectTcp.AppId}");
                            await clientChannel.CloseAsync();
                            return;
                        }
                        #endregion

                        data.ApplicationId = clientConnectTcp.AppId;
                        scertClient.ApplicationID = clientConnectTcp.AppId;

                        data.ClientObject = MediusClass.Manager.GetClientByAccessToken(clientConnectTcp.AccessToken, clientConnectTcp.AppId);

                        #region Client Object Null?
                        //If Client Object is null, then ignore
                        if (data.ClientObject == null)
                        {
                            LoggerAccessor.LogError($"IGNORING CLIENT 1 {data} || {data.ClientObject}");
                            data.Ignore = true;
                        }
                        #endregion
                        else
                        {
                            data.ClientObject.OnConnected();

                            // Update our client object to use existing one
                            data.ClientObject.ApplicationId = clientConnectTcp.AppId;

                            #region if PS3
                            if (scertClient.IsPS3Client)
                            {
                                //CAC & Warhawk
                                if (data.ClientObject.ApplicationId == 20623 || data.ClientObject.ApplicationId == 20624 || data.ClientObject.ApplicationId == 20043 || data.ClientObject.ApplicationId == 20044)
                                {
                                    Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                                    {
                                        PlayerId = 0,
                                        ScertId = GenerateNewScertClientId(),
                                        PlayerCount = 0x0001,
                                        IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address
                                    }, clientChannel);
                                }
                                else
                                {
                                    Queue(new RT_MSG_SERVER_CONNECT_REQUIRE(), clientChannel);
                                }
                            }
                            #endregion
                            else if (scertClient.MediusVersion > 108 && scertClient.MediusVersion != 111 && scertClient.ApplicationID != 20624 && scertClient.ApplicationID != 11484)
                                Queue(new RT_MSG_SERVER_CONNECT_REQUIRE(), clientChannel);
                            else
                            {
                                //If Frequency, TMBO, Socom 1, ATV Offroad Fury 2,  My Street, or Field Commander Beta then
                                if (data.ApplicationId == 10010 || data.ApplicationId == 10031 || data.ApplicationId == 10274 || data.ApplicationId == 10284 || data.ApplicationId == 20190)
                                {
                                    //Do NOT send hereCryptKey Game
                                    Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                                    {
                                        PlayerId = 0,
                                        ScertId = GenerateNewScertClientId(),
                                        PlayerCount = 0x0001,
                                        IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address
                                    }, clientChannel);

                                    //If ATV Offroad Fury 2, complete connection
                                    if (data.ApplicationId == 10284)
                                        Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);
                                }
                                else
                                {
                                    // If RFOM, Starhawk
                                    if (data.ApplicationId == 20174 || data.ApplicationId == 20043 || data.ApplicationId == 22920)
                                    {
                                        //Do Nothing
                                    }
                                    else
                                    {
                                        //Older Medius titles do NOT use CRYPTKEY_GAME, newer ones have this.
                                        if (scertClient.CipherService.EnableEncryption != false)
                                            Queue(new RT_MSG_SERVER_CRYPTKEY_GAME() { GameKey = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                                        Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                                        {
                                            PlayerId = 0,
                                            ScertId = GenerateNewScertClientId(),
                                            PlayerCount = 0x0001,
                                            IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address
                                        }, clientChannel);
                                    }

                                    if (scertClient.ApplicationID != 11484)
                                        Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);
                                }


                                if (scertClient.MediusVersion > 109)
                                {
                                    //Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);
                                }

                            }

                        }

                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_READY_REQUIRE clientConnectReadyRequire:
                    {
                        if (!scertClient.IsPS3Client)
                            Queue(new RT_MSG_SERVER_CRYPTKEY_GAME() { GameKey = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                        Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                        {
                            PlayerId = 0,
                            ScertId = GenerateNewScertClientId(),
                            PlayerCount = 0x0001,
                            IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address
                        }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_READY_TCP clientConnectReadyTcp:
                    {
                        Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);

                        if (scertClient.MediusVersion > 108)
                            Queue(new RT_MSG_SERVER_ECHO(), clientChannel);
                        break;
                    }
                case RT_MSG_SERVER_ECHO serverEchoReply:
                    {

                        break;
                    }
                case RT_MSG_CLIENT_ECHO clientEcho:
                    {
                        if (data.ClientObject == null || !data.ClientObject.IsLoggedIn)
                            break;

                        Queue(new RT_MSG_CLIENT_ECHO() { Value = clientEcho.Value }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_APP_TOSERVER clientAppToServer:
                    {
                        await ProcessMediusMessage(clientAppToServer.Message, clientChannel, data);
                        break;
                    }

                case RT_MSG_CLIENT_DISCONNECT _:
                    {
                        //Medius 1.08 (Used on WRC 4) haven't a state
                        if (scertClient.MediusVersion <= 108)
                            await clientChannel.CloseAsync();
                        else
                            data.State = ClientState.DISCONNECTED;
                        await clientChannel.CloseAsync();

                        LoggerAccessor.LogInfo($"Client id = {data.ClientObject.AccountId} disconnected by request with no specific reason\n");
                        break;
                    }
                case RT_MSG_CLIENT_DISCONNECT_WITH_REASON clientDisconnectWithReason:
                    {
                        if (clientDisconnectWithReason.disconnectReason <= RT_MSG_CLIENT_DISCONNECT_REASON.RT_MSG_CLIENT_DISCONNECT_LENGTH_MISMATCH)
                            LoggerAccessor.LogInfo($"disconnected by request with reason of {clientDisconnectWithReason.disconnectReason}\n");
                        else
                            LoggerAccessor.LogInfo($"disconnected by request with (application specified) reason of {clientDisconnectWithReason.disconnectReason}\n");

                        data.State = ClientState.DISCONNECTED;
                        await clientChannel.CloseAsync();
                        break;
                    }

                default:
                    {
                        LoggerAccessor.LogWarn($"UNHANDLED RT MESSAGE: {message}");

                        break;
                    }
            }

            return;
        }

        protected virtual Task ProcessMediusMessage(BaseMediusMessage message, IChannel clientChannel, ChannelData data)
        {
            if (message == null)
                return Task.CompletedTask;

            switch (message)
            {
                default:
                    {
                        LoggerAccessor.LogWarn($"UNHANDLED MEDIUS MESSAGE: {message}");
                        break;
                    }
            }

            return Task.CompletedTask;
        }

        #endregion

        #region Queue

        public void Queue(BaseScertMessage message, params IChannel[] clientChannels)
        {
            Queue(message, (IEnumerable<IChannel>)clientChannels);
        }

        public void Queue(BaseScertMessage message, IEnumerable<IChannel> clientChannels)
        {
            foreach (var clientChannel in clientChannels)
                if (clientChannel != null)
                    if (_channelDatas.TryGetValue(clientChannel.Id.AsLongText(), out var data))
                        data.SendQueue.Enqueue(message);
        }

        public void Queue(IEnumerable<BaseScertMessage> messages, params IChannel[] clientChannels)
        {
            Queue(messages, (IEnumerable<IChannel>)clientChannels);
        }

        public void Queue(IEnumerable<BaseScertMessage> messages, IEnumerable<IChannel> clientChannels)
        {
            foreach (var clientChannel in clientChannels)
                if (clientChannel != null)
                    if (_channelDatas.TryGetValue(clientChannel.Id.AsLongText(), out var data))
                        foreach (var message in messages)
                            data.SendQueue.Enqueue(message);
        }

        #endregion

        protected uint GenerateNewScertClientId()
        {
            return _clientCounter++;
        }
    }
}
