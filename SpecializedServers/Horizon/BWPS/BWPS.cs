using CustomLogger;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Handlers.Logging;
using Horizon.RT.Models;
using Horizon.LIBRARY.Pipeline.Udp;
using System.Collections.Concurrent;
using System.Net;

namespace Horizon.BWPS
{
    public class BWPS
    {
        public int Port => BWPSClass.Settings.BWPSPort;

        protected IEventLoopGroup? _workerGroup;
        protected IChannel? _boundChannel;
        protected SimpleDatagramHandler? _scertHandler;

        //protected ClientObject ClientObject { get; set; } = null;

        private ConcurrentQueue<ScertDatagramPacket> _recvQueue = new();
        private ConcurrentQueue<ScertDatagramPacket> _sendQueue = new();

        private BaseScertMessage? _lastMessage { get; set; } = null;

        /// <summary>
        /// Start the BWPS UDP Server.
        /// </summary>
        public async Task Start()
        {
            _workerGroup = new MultithreadEventLoopGroup();

            _scertHandler = new SimpleDatagramHandler();

            // Queue all incoming messages
            _scertHandler.OnChannelMessage += async (channel, message) =>
            {
                /*
                var pluginArgs = new OnUdpMsg()
                {
                    //Player = ClientObject,
                    Packet = message
                };
                */
                // Plugin
                //await Program.Plugins.OnEvent(PluginEvent.DME_GAME_ON_RECV_UDP, pluginArgs);

                //if (!pluginArgs.Ignore)
                //_recvQueue.Enqueue(message);

                LoggerAccessor.LogInfo($"[BWPS] - Received Message: {message} on {channel}");

                // Send ip and port back if the last byte isn't 0xD4

                if (message.Content.ReadableBytes == 18)
                {
                    var buffer = channel.Allocator.Buffer(18);

                    byte MessageId = message.Content.GetByte(0);
                    var data = Array.Empty<byte>();

                    if (message.Content.GetByte(9) == 0x05)
                        data = new byte[] { MessageId, 0xc8, 0x01, 0x00, 0xcf, 0x5e, 0x0c, 0x50, 0x01, 0x00, 0x00, 0x00, 0x03, 0x03, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x3 };
                    else
                        data = new byte[] { MessageId, 0xc8, 0x01, 0x00, 0xcf, 0x5e, 0x0c, 0x50, 0x01, 0x00, 0x00, 0x00, 0x05, 0x03, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x3 };

                    //var data = new byte[] { MessageId, 0x01, 0x00, 0x00, 0x00, 0x03, 0x03, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x3 };

                    LoggerAccessor.LogInfo($"[BWPS] - Sending 18 len response");

                    //buffer.WriteUnsignedShort((ushort)(message.Sender as IPEndPoint).Port);

                    await channel.WriteAndFlushAsync(new DatagramPacket(buffer, message.Sender));
                }
                else if (message.Content.ReadableBytes == 18 && message.Content.GetByte(message.Content.ReaderIndex + 2) != 0x01)
                {
                    if (message.Content.ReadableBytes == 18)
                    {
                        var buffer = channel.Allocator.Buffer(18);

                        LoggerAccessor.LogInfo($"[BWPS] - Sending 18 response");

                        buffer.WriteBytes(message.Content);
                        //buffer.WriteUnsignedShort((ushort)(message.Sender as IPEndPoint).Port);

                        await channel.WriteAndFlushAsync(new DatagramPacket(buffer, message.Sender));
                    }
                }

                if (message.Content.ReadableBytes == 6)
                {
                    var buffer = channel.Allocator.Buffer(6);

                    byte MessageId = message.Content.GetByte(0);
                    byte Unk1 = message.Content.GetByte(1);

                    byte[] padding = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    byte[] data = new byte[] { MessageId, Unk1, 0x01, 0x00, 0x01, 0x00 };

                    LoggerAccessor.LogInfo($"[BWPS] - Sending 6 len response ");

                    buffer.WriteBytes(data);
                    //buffer.WriteUnsignedShort((ushort)(message.Sender as IPEndPoint).Port);

                    await channel.WriteAndFlushAsync(new DatagramPacket(buffer, message.Sender));
                }
            };

            Bootstrap bootstrap = new();
            bootstrap
                .Group(_workerGroup)
                .Channel<SocketDatagramChannel>()
                .Handler(new LoggingHandler(LogLevel.INFO))
                .Handler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

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
                if (_boundChannel != null)
                {
                    await _boundChannel.CloseAsync();
                    _boundChannel = null;
                }
            }
            finally
            {
                if (_workerGroup != null)
                    await Task.WhenAll(
                        _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }

        public Task Tick()
        {
            return Task.CompletedTask;
        }

        #region Message Processing

        protected void ProcessMessage(ScertDatagramPacket packet)
        {
            var message = packet.Message;

            switch (message)
            {
                /*
                case RT_MSG_CLIENT_CONNECT_AUX_UDP connectAuxUdp:
                    {
                        var clientObject = Program.BWPS.GetClientByScertId(connectAuxUdp.ScertId);
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
                */
                default:
                    {
                        LoggerAccessor.LogInfo($"[BWPS] - UNHANDLED RT MESSAGE: {message}");

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


        #endregion

        #region Tick

        public Task HandleIncomingMessages()
        {
            if (_boundChannel == null || !BWPSClass.started)
                return Task.CompletedTask;

            try
            {
                // Process all messages in queue
                while (_recvQueue.TryDequeue(out var message))
                {
                    try
                    {
                        //if (!await PassMessageToPlugins(_boundChannel, ClientObject, message.Message, true))
                        ProcessMessage(message);
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

            return Task.CompletedTask;
        }

        public async Task HandleOutgoingMessages()
        {
            if (_boundChannel == null || !BWPSClass.started)
                return;

            List<ScertDatagramPacket> responses = new List<ScertDatagramPacket>();

            try
            {
                // Send if writeable
                if (_boundChannel.IsWritable)
                {
                    // Add send queue to responses
                    while (_sendQueue.TryDequeue(out var message))
                    {
                        //if (!await PassMessageToPlugins(_boundChannel, ClientObject, message.Message, false))
                        responses.Add(message);
                    }

                    if (responses.Count > 0)
                        await _boundChannel.WriteAndFlushAsync(responses);
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError(e);
            }
        }

        #endregion
    }
}
