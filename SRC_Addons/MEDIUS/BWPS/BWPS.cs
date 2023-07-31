using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using PSMultiServer.Addons.Medius.Server.Pipeline.Udp;

namespace PSMultiServer.Addons.Medius.BWPS
{
    /// <summary>
    /// Unimplemented BWPS.
    /// </summary>
    public class BWPServer
    {
        public int Port => BwpsClass.Settings.BWPSPort;
        public bool IsRunning => _boundChannel != null && _boundChannel.Active;

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<BWPServer>();

        protected IEventLoopGroup _workerGroup;
        protected IChannel _boundChannel;
        protected SimpleDatagramHandler _scertHandler;

        public BWPServer()
        {

        }

        /// <summary>
        /// Start the BWPS UDP Server.
        /// </summary>
        public async Task Start()
        {
            //
            _workerGroup = new MultithreadEventLoopGroup();

            _scertHandler = new SimpleDatagramHandler();

            // Queue all incoming messages
            _scertHandler.OnChannelMessage += (channel, message) =>
            {

                ServerConfiguration.LogInfo($"Received Message: {message} on {channel}");

                // Send ip and port back if the last byte isn't 0xD4

                if (message.Content.ReadableBytes == 18)
                {
                    var buffer = channel.Allocator.Buffer(22);

                    byte MessageId = message.Content.GetByte(0);

                    //var data = new byte[] { MessageId, 0x01, 0x00, 0x00, 0x00, 0x03, 0x03, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x3 };
                    var data = new byte[] { MessageId, 0xc8, 0x01, 0x00, 0xcf, 0x5e, 0x0c, 0x50, 0x01, 0x00, 0x00, 0x00, 0x03, 0x03, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x3 };



                    ServerConfiguration.LogInfo("Sending 22 len response");

                    buffer.WriteBytes(data);
                    //buffer.WriteUnsignedShort((ushort)(message.Sender as IPEndPoint).Port);

                    channel.WriteAndFlushAsync(new DatagramPacket(buffer, message.Sender));
                }
                else if (message.Content.ReadableBytes == 18 && message.Content.GetByte(message.Content.ReaderIndex + 2) != 0x01)
                {
                    if (message.Content.ReadableBytes == 18)
                    {
                        var buffer = channel.Allocator.Buffer(18);

                        ServerConfiguration.LogInfo("Sending 18 response");

                        var data = message.Content;
                        buffer.WriteBytes(data);
                        //buffer.WriteUnsignedShort((ushort)(message.Sender as IPEndPoint).Port);

                        channel.WriteAndFlushAsync(new DatagramPacket(buffer, message.Sender));
                    }
                }

                if (message.Content.ReadableBytes == 6)
                {
                    var buffer = channel.Allocator.Buffer(6);

                    byte MessageId = message.Content.GetByte(0);
                    byte Unk1 = message.Content.GetByte(1);

                    var padding = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    var data = new byte[] { MessageId, Unk1, 0x01, 0x00, 0x01, 0x00 };

                    ServerConfiguration.LogInfo("Sending 6 len response ");

                    buffer.WriteBytes(data);
                    //buffer.WriteUnsignedShort((ushort)(message.Sender as IPEndPoint).Port);

                    channel.WriteAndFlushAsync(new DatagramPacket(buffer, message.Sender));
                }

            };

            var bootstrap = new Bootstrap();
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
                await _boundChannel.CloseAsync();
            }
            finally
            {
                await Task.WhenAll(
                        _workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }

        public Task Tick()
        {
            return Task.CompletedTask;
        }
    }
}
