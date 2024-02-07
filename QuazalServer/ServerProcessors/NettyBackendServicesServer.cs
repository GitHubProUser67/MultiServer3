using CustomLogger;
using DotNetty.Extensions;
using QuazalServer.QNetZ;
using System.Collections.Concurrent;
using System.Net;

namespace QuazalServer.ServerProcessors
{
    public class NettyBackendServicesServer
    {
        private readonly ConcurrentBag<UdpSocket> _listeners = new();
        private readonly ConcurrentBag<bool> _shutflags = new();
        private CancellationTokenSource _cts = null!;

        public void Start(List<Tuple<int, string>> PrudpInstance, uint serverPID, CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Parallel.ForEach(PrudpInstance, tuple => { new Thread(() => HandleClient(tuple.Item1, serverPID, tuple.Item2)).Start(); });
        }

        public void StopAsync()
        {
            _cts.Cancel();
            _shutflags.ToList().ForEach(x => x = false);
            _listeners.ToList().ForEach(x => _ = x.StopAsync());
        }

        public void HandleClient(int listenPort, uint serverPID, string AccessKey)
        {
            Task serverPRUDP = Task.Run(() =>
            {
                bool _exit = false;
                object _sync = new();
                UdpSocket udp = new(listenPort);
                QPacketHandlerPRUDP? packetHandler = new(udp, serverPID, listenPort, listenPort, AccessKey, "BackendServices");
                packetHandler.Updates.Add(() => NetworkPlayers.DropPlayers());
                _listeners.Add(udp);
                _shutflags.Add(_exit);

                udp.OnStart(() =>
                {
                    LoggerAccessor.LogInfo($"Backend Service Server initiated on port: {listenPort}...");

                    while (!_cts.Token.IsCancellationRequested)
                    {
                        lock (_sync)
                        {
                            if (_exit)
                                break;
                        }

                        packetHandler.Update();

                        Thread.Sleep(1);
                    }
                });

                udp.OnRecieve((endPoint, bytes) =>
                {
                    if (endPoint is IPEndPoint EndPointIp)
                    {
                        LoggerAccessor.LogInfo($"[Backend Service] - Received request from {endPoint}");
                        packetHandler.ProcessPacket(bytes, EndPointIp);
                    }
                });

                udp.OnException(ex =>
                {
                    LoggerAccessor.LogError($"[Backend Service] - DotNetty Thrown an exception : {ex}");
                });

                udp.OnStop(ex =>
                {
                    LoggerAccessor.LogWarn($"[Backend Service] - DotNetty was stopped!");
                });

                _ = udp.StartAsync();

                packetHandler = null;
            }, _cts.Token);
        }
    }
}
