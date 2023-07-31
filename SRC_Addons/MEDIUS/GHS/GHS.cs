using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using PSMultiServer.Addons.Medius.RT.Common;
using PSMultiServer.Addons.Medius.RT.Cryptography;
using PSMultiServer.Addons.Medius.RT.Models;
using PSMultiServer.Addons.Medius.Server.Pipeline.Tcp;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace PSMultiServer.Addons.Medius.GHS
{
    /// <summary>
    /// Introduced in Medius 2.10
    /// </summary>
    public class GHS
    {
        public static Random RNG = new Random();

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<GHS>();

        private int _port = 0;
        public int Port => _port;

        protected IEventLoopGroup _bossGroup = null;
        protected IEventLoopGroup _workerGroup = null;
        protected IChannel _boundChannel = null;
        protected ScertServerHandler _scertHandler = null;
        private uint _clientCounter = 0;

        public TypeRtMsgState gConnectState = TypeRtMsgState.RTMSG_DISCONNECTED;
        public GhsClientState gPluginState = GhsClientState.GhsClient_OFFLINE;

        public Stopwatch WorldTimer { get; protected set; } = Stopwatch.StartNew();
        protected internal class ChannelData
        {
            public int ApplicationId { get; set; } = 0;
            public ConcurrentQueue<BaseScertMessage> RecvQueue { get; } = new ConcurrentQueue<BaseScertMessage>();
            public ConcurrentQueue<BaseScertMessage> SendQueue { get; } = new ConcurrentQueue<BaseScertMessage>();
        }

        protected ConcurrentDictionary<string, ChannelData> _channelDatas = new ConcurrentDictionary<string, ChannelData>();

        public GHS(int port)
        {
            _port = port;
        }

        /// <summary>
        /// Start the GHS TCP Server.
        /// </summary>
        public virtual async Task Start()
        {
            //
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
                {
                    data.RecvQueue.Enqueue(message);
                }

                // Log if id is set
                if (message.CanLog())
                    Logger.Info($"TCP RECV {channel}: {message}");
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

            _boundChannel = await bootstrap.BindAsync(Port);
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        public virtual async Task Stop()
        {
            try
            {
                await _boundChannel.CloseAsync();
            }
            finally
            {
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

            // 
            List<BaseScertMessage> responses = new List<BaseScertMessage>();
            string key = clientChannel.Id.AsLongText();

            try
            {
                // 
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
                            Logger.Error(e);
                        }
                    }

                    // Send if writeable
                    if (clientChannel.IsWritable)
                    {
                        // Add send queue to responses
                        while (data.SendQueue.TryDequeue(out var message))
                            responses.Add(message);

                        //
                        if (responses.Count > 0)
                            await clientChannel.WriteAndFlushAsync(responses);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #region Message Processing

        protected async Task ProcessMessage(BaseScertMessage message, IChannel clientChannel, ChannelData data)
        {
            // Get ScertClient data
            var scertClient = clientChannel.GetAttribute(Server.Pipeline.Constants.SCERT_CLIENT).Get();
            scertClient.CipherService.EnableEncryption = GhsClass.Settings.EncryptMessages;

            // 
            switch (message)
            {
                case RT_MSG_CLIENT_HELLO clientHello:
                    {
                        gPluginState = GhsClientState.GhsClient_CONNECTING;
                        gConnectState = TypeRtMsgState.RTMSG_TCPCONNECT;

                        // send hello
                        Queue(new RT_MSG_SERVER_HELLO()
                        {
                            RsaPublicKey = GhsClass.Settings.EncryptMessages ?
                            GhsClass.Settings.DefaultKey.N : Org.BouncyCastle.Math.BigInteger.Zero
                        }, clientChannel);

                        Logger.Info("plugin: RT_MSG_SERVER_HELLO\n");
                        break;
                    }
                case RT_MSG_CLIENT_CRYPTKEY_PUBLIC clientCryptKeyPublic:
                    {
                        // generate new client session key
                        scertClient.CipherService.GenerateCipher(CipherContext.RSA_AUTH, clientCryptKeyPublic.PublicKey.Reverse().ToArray());
                        scertClient.CipherService.GenerateCipher(CipherContext.RC_CLIENT_SESSION);

                        Queue(new RT_MSG_SERVER_CRYPTKEY_PEER()
                        {
                            SessionKey = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION)
                        }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_TCP clientConnectTcp:
                    {
                        data.ApplicationId = clientConnectTcp.AppId;

                        gConnectState = TypeRtMsgState.RTMSG_TCPCONNECT;
                        gPluginState = GhsClientState.GhsClient_CONNECTING;


                        Queue(new RT_MSG_SERVER_STARTUP_INFO_NOTIFY()
                        {
                            GameHostType = (byte)MGCL_GAME_HOST_TYPE.MGCLGameHostClientServer,
                            Timebase = (uint)WorldTimer.ElapsedMilliseconds
                        }, clientChannel);


                        Queue(new RT_MSG_SERVER_CONNECT_REQUIRE() { ReqServerPassword = 1, MaxPacketSize = 584, MaxUdpPacketSize = 584 }, clientChannel);
                        /*
                        Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP(){
                            IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address,
                        }, clientChannel);
                        */


                        /*
                        // HSG:F Pubeta, HW:O, LemmingsPS2, Arc the Lad, or EyeToy Chat Beta, Socom 2
                        if ()
                        {
                            // If this is NOT Arc the Lad, but Socom 2 continue
                            if (data.ApplicationId != 10984 || data.ApplicationId == 10202)
                            {
                                Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                                {
                                    IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address,
                                }, clientChannel);

                                //if this isn't Lemmings PS2, Arc the Lad, or Socom 2 continue the handshake
                                if (data.ApplicationId != 20474 || data.ApplicationId != 10984 || data.ApplicationId != 10550 || data.ApplicationId != 10202)
                                {
                                    Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);
                                }
                            }
                            else
                            {
                                if (scertClient.MediusVersion <= 109 || scertClient.MediusVersion == 113)
                                {
                                    Queue(new RT_MSG_SERVER_CONNECT_REQUIRE() { ReqServerPassword = 0x00, Contents = Utils.FromString("4802") }, clientChannel);
                                }
                                Queue(new RT_MSG_SERVER_CRYPTKEY_GAME() { Key = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                            }
                        }
                        else
                        //Default flow 
                        {
                            if (scertClient.MediusVersion <= 109 || data.ApplicationId == 22920)
                            {
                                Queue(new RT_MSG_SERVER_CONNECT_REQUIRE() { ReqServerPassword = 0x00, Contents = Utils.FromString("4802") }, clientChannel);
                            }
                            else
                            {
                                Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                                {
                                    IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address,
                                }, clientChannel);


                                if(scertClient.CipherService.EnableEncryption == true )
                                {
                                    Queue(new RT_MSG_SERVER_CRYPTKEY_GAME() { Key = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                                }

                                //If this isn't Motorstorm Pacific Rift then complete handshake
                                if (data.ApplicationId != 21624)
                                {
                                    //Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);
                                }
                            }
                        }
                        */
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_READY_REQUIRE clientConnectReadyRequire:
                    {
                        if (scertClient.MediusVersion >= 109 && scertClient.CipherService.EnableEncryption == true)
                        {
                            //Queue(new RT_MSG_SERVER_CRYPTKEY_GAME() { GameKey = scertClient.CipherService.GetPublicKey(CipherContext.RC_CLIENT_SESSION) }, clientChannel);
                        }
                        Queue(new RT_MSG_SERVER_CONNECT_ACCEPT_TCP()
                        {
                            PlayerId = 0,
                            ScertId = GenerateNewScertClientId(),
                            PlayerCount = 0x0001,
                            IP = (clientChannel.RemoteAddress as IPEndPoint)?.Address
                        }, clientChannel);
                        Logger.Info("plugin: RT_MSG_SERVER_CONNECT_ACCEPT_TCP\n");
                        break;
                    }
                case RT_MSG_CLIENT_CONNECT_READY_TCP clientConnectReadyTcp:
                    {
                        Logger.Info("plugin: RT_MSG_CLIENT_CONNECT_READY_TCP\n");
                        Queue(new RT_MSG_SERVER_CONNECT_COMPLETE() { ClientCountAtConnect = 0x0001 }, clientChannel);


                        Logger.Info("plugin: RT_MSG_SERVER_CONNECT_COMPLETE\n");
                        /*
                        Queue(new RT_MSG_SERVER_APP()
                        {
                            GHSMessage = new scertGhsTypeVersionRequest()
                            {
                                maxMajorVersion = ReverseBytes16(0x0001),
                                maxMinorVersion = 0,
                            }
                            
                        }, clientChannel);
                        */
                        break;
                    }
                case RT_MSG_SERVER_ECHO serverEchoReply:
                    {

                        break;
                    }
                case RT_MSG_CLIENT_ECHO clientEcho:
                    {
                        Logger.Info("plugin: RT_MSG_CLIENT_ECHO");
                        Queue(new RT_MSG_CLIENT_ECHO() { Value = clientEcho.Value }, clientChannel);
                        break;
                    }
                case RT_MSG_CLIENT_APP_TOSERVER clientAppToServer:
                    {
                        //await ProcessMediusGHSMessage(clientAppToServer.GhsMessage, clientChannel, data);
                        break;
                    }
                case RT_MSG_CLIENT_APP_LIST clientAppList:
                    {

                        break;
                    }
                case RT_MSG_CLIENT_DISCONNECT _:
                case RT_MSG_CLIENT_DISCONNECT_WITH_REASON clientDisconnectWithReason:
                    {
                        _ = clientChannel.CloseAsync();
                        break;
                    }
                default:
                    {
                        Logger.Warn($"UNHANDLED RT MESSAGE: {message}");
                        break;
                    }
            }

            return;
        }
        /*
        protected virtual async Task ProcessMediusGHSMessage(BaseMediusGHSMessage message, IChannel clientChannel, ChannelData data)
        {
            if (message == null)
                return;

            //var appSettings = Program.GetAppSettingsOrDefault(data.ApplicationId);

            switch (message)
            {
                
                case scertGhsTypeProtocolChoice ghsTypeProtocolChoice:
                    {

                        Logger.Info($"ghsTypeProtocolChoice: {ghsTypeProtocolChoice.MajorVersion}.{ghsTypeProtocolChoice.MinorVersion}\n");
                        break;
                    }
                default:
                    {
                        Logger.Warn($"UNHANDLED MEDIUS GHS MESSAGE: {message}");
                        break;
                    }
            }
        }
        */
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

        #region ReverseBytes16
        /// <summary>
        /// Reverses UInt16 
        /// </summary>
        /// <param name="nValue"></param>
        /// <returns></returns>
        public static ushort ReverseBytes16(ushort nValue)
        {
            return (ushort)((ushort)((nValue >> 8)) | (nValue << 8));
        }
        #endregion

        protected uint GenerateNewScertClientId()
        {
            return _clientCounter++;
        }
    }
}
