using DotNetty.Common.Internal.Logging;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.VisualBasic.FileIO;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Cryptography;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Models;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Pipeline.Tcp;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Pipeline.Udp;
using PSMultiServer.SRC_Addons.MEDIUS.DME.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PSMultiServer.SRC_Addons.MEDIUS.DME.PluginArgs;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Plugins.Interface;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Pipeline.Attribute;

namespace PSMultiServer.SRC_Addons.MEDIUS.DME
{
    public class UdpServer
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<UdpServer>();

        public int Port { get; protected set; } = -1;

        protected IEventLoopGroup _workerGroup = null;
        protected IChannel _boundChannel = null;
        protected ScertDatagramHandler _scertHandler = null;

        protected ClientObject ClientObject { get; set; } = null;
        protected EndPoint AuthenticatedEndPoint { get; set; } = null;

        private ConcurrentQueue<ScertDatagramPacket> _recvQueue = new ConcurrentQueue<ScertDatagramPacket>();
        private ConcurrentQueue<ScertDatagramPacket> _sendQueue = new ConcurrentQueue<ScertDatagramPacket>();

        private BaseScertMessage _lastMessage { get; set; } = null;

        #region Port Management

        private static ConcurrentDictionary<int, UdpServer> _portToServer = new ConcurrentDictionary<int, UdpServer>();
        private void RegisterPort()
        {
            int i = DmeClass.Settings.UDPPort;
            while (_portToServer.ContainsKey(i))
                ++i;

            if (_portToServer.TryAdd(i, this))
                Port = i;
        }

        private void FreePort()
        {
            if (Port < 0)
                return;

            _portToServer.TryRemove(Port, out _);
        }

        #endregion

        public UdpServer(ClientObject clientObject)
        {
            ClientObject = clientObject;
            RegisterPort();
        }

        /// <summary>
        /// Start the Dme Udp Client Server.
        /// </summary>
        public virtual async Task Start()
        {
            //
            _workerGroup = new MultithreadEventLoopGroup();
            _scertHandler = new ScertDatagramHandler();

            //
            _scertHandler.OnChannelActive = channel =>
            {
                // get scert client
                if (!channel.HasAttribute(Server.Pipeline.Constants.SCERT_CLIENT))
                    channel.GetAttribute(Server.Pipeline.Constants.SCERT_CLIENT).Set(new ScertClientAttribute());
                var scertClient = channel.GetAttribute(Server.Pipeline.Constants.SCERT_CLIENT).Get();

                // pass medius version
                scertClient.MediusVersion = ClientObject.MediusVersion;
            };

            // Queue all incoming messages
            _scertHandler.OnChannelMessage += async (channel, message) =>
            {
                var pluginArgs = new OnUdpMsg()
                {
                    Player = ClientObject,
                    Packet = message
                };

                // Plugin
                await DmeClass.Plugins.OnEvent(PluginEvent.DME_GAME_ON_RECV_UDP, pluginArgs);

                if (!pluginArgs.Ignore)
                    _recvQueue.Enqueue(message);
            };

            var bootstrap = new Bootstrap();
            bootstrap
                .Group(_workerGroup)
                .Channel<SocketDatagramChannel>()
                .Handler(new LoggingHandler(LogLevel.INFO))
                .Handler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

                    pipeline.AddLast(new ScertDatagramEncoder(Constants.MEDIUS_UDP_MESSAGE_MAXLEN));
                    pipeline.AddLast(new ScertDatagramIEnumerableEncoder(Constants.MEDIUS_UDP_MESSAGE_MAXLEN));
                    pipeline.AddLast(new ScertDatagramDecoder());
                    //pipeline.AddLast(new ScertDecoder());
                    pipeline.AddLast(new ScertDatagramMultiAppDecoder());
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
                        _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));

                FreePort();
            }
        }

        #region Message Processing

        protected void ProcessMessage(ScertDatagramPacket packet)
        {
            var message = packet.Message;

            //
            switch (message)
            {
                case RT_MSG_CLIENT_CONNECT_AUX_UDP connectAuxUdp:
                    {
                        var clientObject = DmeClass.TcpServer.GetClientByScertId(connectAuxUdp.ScertId);
                        if (clientObject != ClientObject && ClientObject.DmeId != connectAuxUdp.PlayerId)
                            break;

                        //
                        AuthenticatedEndPoint = packet.Source;

                        ClientObject.RemoteUdpEndpoint = AuthenticatedEndPoint as IPEndPoint;
                        ClientObject.OnUdpConnected();

                        //
                        var msg = new RT_MSG_SERVER_CONNECT_ACCEPT_AUX_UDP()
                        {
                            PlayerId = (ushort)ClientObject.DmeId,
                            ScertId = ClientObject.ScertId,
                            PlayerCount = (ushort)ClientObject.DmeWorld.Clients.Count,
                            EndPoint = ClientObject.RemoteUdpEndpoint
                        };

                        // Send it twice in case of packet loss
                        //_boundChannel.WriteAndFlushAsync(new ScertDatagramPacket(msg, packet.Source));
                        _boundChannel.WriteAndFlushAsync(new ScertDatagramPacket(msg, packet.Source));
                        break;
                    }
                case RT_MSG_SERVER_ECHO serverEchoReply:
                    {

                        break;
                    }
                case RT_MSG_CLIENT_ECHO clientEcho:
                    {
                        SendTo(new RT_MSG_CLIENT_ECHO() { Value = clientEcho.Value }, packet.Source);
                        break;
                    }
                case RT_MSG_CLIENT_APP_BROADCAST clientAppBroadcast:
                    {
                        if (AuthenticatedEndPoint == null || !AuthenticatedEndPoint.Equals(packet.Source))
                            break;

                        ClientObject.DmeWorld?.BroadcastUdp(ClientObject, clientAppBroadcast.Payload);
                        break;
                    }
                case RT_MSG_CLIENT_APP_LIST clientAppList:
                    {
                        if (AuthenticatedEndPoint == null || !AuthenticatedEndPoint.Equals(packet.Source))
                            break;

                        ClientObject.DmeWorld?.SendUdpAppList(ClientObject, clientAppList.Targets, clientAppList.Payload);
                        break;
                    }
                case RT_MSG_CLIENT_APP_SINGLE clientAppSingle:
                    {
                        if (AuthenticatedEndPoint == null || !AuthenticatedEndPoint.Equals(packet.Source))
                            break;

                        ClientObject.DmeWorld?.SendUdpAppSingle(ClientObject, clientAppSingle.TargetOrSource, clientAppSingle.Payload);
                        break;
                    }
                case RT_MSG_CLIENT_APP_TOSERVER clientAppToServer:
                    {
                        if (AuthenticatedEndPoint == null || !AuthenticatedEndPoint.Equals(packet.Source))
                            break;

                        ProcessMediusMessage(clientAppToServer.Message);
                        break;
                    }

                case RT_MSG_CLIENT_DISCONNECT _:
                case RT_MSG_CLIENT_DISCONNECT_WITH_REASON _:
                    {

                        break;
                    }
                default:
                    {
                        Logger.Warn($"UNHANDLED MESSAGE: {message}");

                        break;
                    }
            }

            return;
        }

        protected virtual void ProcessMediusMessage(BaseMediusMessage message)
        {
            if (message == null)
                return;
        }

        #endregion

        #region Send

        private void SendTo(BaseScertMessage message, EndPoint target)
        {
            if (target == null)
                return;

            _sendQueue.Enqueue(new ScertDatagramPacket(message, target));
        }

        public void Send(BaseScertMessage message)
        {
            if (AuthenticatedEndPoint == null)
                return;

            _sendQueue.Enqueue(new ScertDatagramPacket(message, AuthenticatedEndPoint));
        }

        public void Send(IEnumerable<BaseScertMessage> messages)
        {
            if (AuthenticatedEndPoint == null)
                return;

            foreach (var message in messages)
                _sendQueue.Enqueue(new ScertDatagramPacket(message, AuthenticatedEndPoint));
        }

        public Task SendImmediate(BaseScertMessage message)
        {
            if (AuthenticatedEndPoint == null || _boundChannel == null)
                return Task.CompletedTask;

            return _boundChannel.WriteAndFlushAsync(new ScertDatagramPacket(message, AuthenticatedEndPoint));
        }

        public Task SendImmediate(IEnumerable<BaseScertMessage> messages)
        {
            if (AuthenticatedEndPoint == null || _boundChannel == null)
                return Task.CompletedTask;

            return _boundChannel.WriteAndFlushAsync(messages.Select(x => new ScertDatagramPacket(x, AuthenticatedEndPoint)));
        }

        #endregion

        #region Tick

        public async Task HandleIncomingMessages()
        {
            if (_boundChannel == null || !_boundChannel.Active)
                return;

            try
            {
                // Process all messages in queue
                while (_recvQueue.TryDequeue(out var message))
                {
                    try
                    {
                        if (!await PassMessageToPlugins(_boundChannel, ClientObject, message.Message, true))
                            ProcessMessage(message);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public async Task HandleOutgoingMessages()
        {
            if (_boundChannel == null || !_boundChannel.Active)
                return;

            //
            List<ScertDatagramPacket> responses = new List<ScertDatagramPacket>();

            try
            {
                // Send if writeable
                if (_boundChannel.IsWritable)
                {
                    // Add send queue to responses
                    while (_sendQueue.TryDequeue(out var message))
                    {
                        if (!await PassMessageToPlugins(_boundChannel, ClientObject, message.Message, false))
                            responses.Add(message);
                    }

                    //
                    if (responses.Count > 0)
                        await _boundChannel.WriteAndFlushAsync(responses);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion

        protected async Task<bool> PassMessageToPlugins(IChannel clientChannel, ClientObject clientObject, BaseScertMessage message, bool isIncoming)
        {
            var onMsg = new OnMessageArgs(isIncoming)
            {
                Player = clientObject,
                Channel = clientChannel,
                Message = message
            };

            // Send to plugins
            await DmeClass.Plugins.OnMessageEvent(message.Id, onMsg);
            if (onMsg.Ignore)
                return true;



            // Send medius message to plugins
            if (message is RT_MSG_CLIENT_APP_TOSERVER clientApp)
            {
                var onMediusMsg = new OnMediusMessageArgs(isIncoming)
                {
                    Player = clientObject,
                    Channel = clientChannel,
                    Message = clientApp.Message
                };
                await DmeClass.Plugins.OnMediusMessageEvent(clientApp.Message.PacketClass, clientApp.Message.PacketType, onMediusMsg);
                if (onMediusMsg.Ignore)
                    return true;
            }
            else if (message is RT_MSG_SERVER_APP serverApp)
            {
                var onMediusMsg = new OnMediusMessageArgs(isIncoming)
                {
                    Player = clientObject,
                    Channel = clientChannel,
                    Message = serverApp.Message
                };
                await DmeClass.Plugins.OnMediusMessageEvent(serverApp.Message.PacketClass, serverApp.Message.PacketType, onMediusMsg);
                if (onMediusMsg.Ignore)
                    return true;
            }

            return false;
        }

    }
}
