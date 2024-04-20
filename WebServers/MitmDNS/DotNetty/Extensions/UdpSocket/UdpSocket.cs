using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DotNetty.Extensions.UdpSocket
{
    public class UdpSocket
    {
        private int _port;

        private IPAddress? _ipAddress;

        public UdpSocket(int port = 0, IPAddress? ipAddress = null)
        {
            _port = port;
            _ipAddress = ipAddress;
        }

        private IEventLoopGroup? group;

        private Bootstrap? bootstrap;

        private IChannel? channel;

        private UdpEvent _event = new();

        public async Task StartAsync()
        {
            try
            {
                if (group == null)
                    group = new MultithreadEventLoopGroup();

                if (bootstrap == null)
                {
                    bootstrap = new Bootstrap();
                    bootstrap
                        .Group(group)
                        .Channel<SocketDatagramChannel>()
                        .Option(ChannelOption.SoBroadcast, true)
                        .Handler(new ActionChannelInitializer<IChannel>(ch =>
                        {
                            IChannelPipeline pipeline = ch.Pipeline;
                            _event.OnPipelineAction?.Invoke(pipeline);
                            pipeline.AddLast(new UdpHandler(_event));
                        }));
                }

                if (_ipAddress == null)
                    _ipAddress = IPAddress.Any;

                await Stop();

                channel = await bootstrap.BindAsync(_ipAddress, _port);
                _event.OnStartAction?.Invoke();
            }
            catch (Exception ex)
            {
                _event.OnStopAction?.Invoke(ex);
            }
        }

        private async Task Stop()
        {
            if (channel != null)
            {
                await channel.CloseAsync();
                channel = null;
            }
        }

        public async Task StopAsync()
        {
            await Stop();
            _event.OnStopAction?.Invoke(new Exception("StopAsync"));
        }

        public async Task ShutdownAsync()
        {
            await Stop();

            if (group != null)
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));

            group = null;
            bootstrap = null;
            _event.OnStopAction?.Invoke(new Exception("ShutdownAsync"));
        }

        public async Task SendAsync(EndPoint endPoint, byte[] bytes)
        {
            try
            {
                if (channel != null)
                {
                    var buffer = Unpooled.WrappedBuffer(bytes);
                    var dp = new DatagramPacket(buffer, endPoint);
                    await channel.WriteAndFlushAsync(dp);
                }
            }
            catch (Exception e)
            {
                CustomLogger.LoggerAccessor.LogError($"[UdpSocket] - Thrown an essertion : {e}");
            }
        }

        public void OnPipeline(Action<IChannelPipeline> action)
        {
            _event.OnPipelineAction = action;
        }

        public void OnStart(Action action)
        {
            _event.OnStartAction = action;
        }

        public void OnRecieve(Action<EndPoint, byte[]> action)
        {
            _event.OnRecieveAction = action;
        }

        public void OnException(Action<Exception> action)
        {
            _event.OnExceptionAction = action;
        }

        public void OnStop(Action<Exception> action)
        {
            _event.OnStopAction = action;
        }
    }
}
