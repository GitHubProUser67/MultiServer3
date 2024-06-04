using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Horizon.LIBRARY.Pipeline.Udp;
using System.Net;
using CustomLogger;

namespace Horizon.NAT
{
    /// <summary>
    /// SCE-RT NAT.
    /// </summary>
    public class NAT
    {
        public int Port => NATClass.Settings.Port;

        protected IEventLoopGroup? _workerGroup = null;
        protected IChannel? _boundChannel = null;
        protected SimpleDatagramHandler? _scertHandler = null;

        public NAT()
        {

        }

        /// <summary>
        /// Start the SCE-RT Server.
        /// </summary>
        public async Task Start()
        {
            _workerGroup = new MultithreadEventLoopGroup();

            _scertHandler = new SimpleDatagramHandler();

            // Queue all incoming messages
            _scertHandler.OnChannelMessage += (channel, message) =>
            {
                // Send ip and port back if the last byte isn't 0xD4
                if (message.Content.ReadableBytes == 4 && message.Content.GetByte(message.Content.ReaderIndex + 3) != 0xD4 && (ushort?)(message.Sender as IPEndPoint)?.Port != null)
                {
                    ushort DestPort = (ushort?)(message.Sender as IPEndPoint)?.Port ?? 0; // Should never ever take 0, as we check earlier.
                    LoggerAccessor.LogInfo($"Recieved External IP {(message.Sender as IPEndPoint)?.Address.MapToIPv4()} & Port {DestPort} request, sending their IP & Port as response!");
                    var buffer = channel.Allocator.Buffer(6);
                    buffer.WriteBytes((message.Sender as IPEndPoint)?.Address.MapToIPv4().GetAddressBytes());
                    buffer.WriteUnsignedShort(DestPort);
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
    }
}