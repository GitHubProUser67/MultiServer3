using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Collections.Concurrent;
using System.Net;

namespace DotNetty.Extensions
{
    public class TcpSocketServer
    {
        private int _port;

        private IPAddress? _ipAddress;

        private int _soBacklog;

        public TcpSocketServer(int port, IPAddress? ipAddress = null, int soBacklog = 1024)
        {
            _port = port;
            _ipAddress = ipAddress;
            _soBacklog = soBacklog;
        }

        private IEventLoopGroup? bossGroup;

        private IEventLoopGroup? workerGroup;

        private ServerBootstrap? bootstrap;

        private IChannel? channel;

        private TcpServerEvent _event = new();

        private readonly ConcurrentDictionary<string, TcpSocketConnection> connectionDict = new ConcurrentDictionary<string, TcpSocketConnection>();

        public async Task StartAsync()
        {
            try
            {
                if (bossGroup == null && workerGroup == null)
                {
                    bossGroup = new MultithreadEventLoopGroup();
                    workerGroup = new MultithreadEventLoopGroup();
                }

                if (bootstrap == null)
                {
                    bootstrap = new ServerBootstrap();
                    bootstrap.Group(bossGroup, workerGroup);
                    bootstrap.Channel<TcpServerSocketChannel>();

                    bootstrap
                        .Option(ChannelOption.SoBacklog, _soBacklog)
                        .ChildHandler(new ActionChannelInitializer<IChannel>(ch =>
                        {
                            IChannelPipeline pipeline = ch.Pipeline;
                            _event.OnPipelineAction?.Invoke(pipeline);
                            pipeline.AddLast(new TcpServerHandler(_event, connectionDict));

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
                await Task.WhenAll(connectionDict.Values.Select(s => s.CloseAsync()).ToArray());
                connectionDict.Clear();
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
            if (bossGroup  != null && workerGroup != null)
            {
                var task1 = bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
                var task2 = workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
                await Task.WhenAll(task1, task2);
                bossGroup = null;
                workerGroup = null;
            }
            bootstrap = null;
            _event.OnStopAction?.Invoke(new Exception("ShutdownAsync"));
        }

        public ICollection<TcpSocketConnection> GetAllConnection()
        {
            return connectionDict.Values;
        }

        public IEnumerable<string> GetAllConnectionName()
        {
            return connectionDict.Values.Select(s => s.Name);
        }

        public TcpSocketConnection? GetConnectionById(string id)
        {
            connectionDict.TryGetValue(id, out TcpSocketConnection? connection);
            return connection;
        }

        public IEnumerable<TcpSocketConnection> GetConnectionByName(string name)
        {
            return connectionDict.Values.Where(w => w.Name == name);
        }

        public int GetConnectionCount()
        {
            return connectionDict.Count;
        }

        public void OnPipeline(Action<IChannelPipeline> action)
        {
            _event.OnPipelineAction = action;
        }

        public void OnStart(Action action)
        {
            _event.OnStartAction = action;
        }

        public void OnConnectionConnect(Action<TcpSocketConnection> action)
        {
            _event.OnConnectionConnectAction = action;
        }

        public void OnConnectionReceive(Action<TcpSocketConnection, byte[]> action)
        {
            _event.OnConnectionReceiveAction = action;
        }

        public void OnConnectionException(Action<TcpSocketConnection, Exception> action)
        {
            _event.OnConnectionExceptionAction = action;
        }

        public void OnConnectionClose(Action<TcpSocketConnection> action)
        {
            _event.OnConnectionCloseAction = action;
        }

        public void OnStop(Action<Exception> action)
        {
            _event.OnStopAction = action;
        }
    }
}
