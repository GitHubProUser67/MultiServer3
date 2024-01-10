using CustomLogger;
using DotNetty.Extensions;
using QuazalServer.QNetZ;
using System.Collections.Concurrent;
using System.Net;

namespace QuazalServer.ServerProcessors
{
	public class RDVServer
	{
        private readonly ConcurrentBag<UdpSocket> _listeners = new();
        private readonly ConcurrentBag<Timer> _timers = new();
        private CancellationTokenSource _cts = null!;

		public void Start(List<Tuple<int, int, string>> PrudpInstance, uint BackendPID, CancellationToken cancellationToken)
		{
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Parallel.ForEach(PrudpInstance, tuple => { new Thread(() => HandleNettyClient(tuple.Item1, BackendPID, tuple.Item2, tuple.Item3, null)).Start(); });
        }

        public void StopAsync()
        {
            _cts.Cancel();
            _listeners.ToList().ForEach(async x => await x.StopAsync());
            _timers.ToList().ForEach(async x => await x.DisposeAsync());
        }

        public void HandleNettyClient(int listenPort, uint BackendPID, int BackendPort, string AccessKey, object? obj)
        {
            Task serverPRUDP = Task.Run(async () =>
            {
                UdpSocket listener = new(listenPort);
                _listeners.Add(listener);
                while (!_cts.Token.IsCancellationRequested)
                {
                    QPacketHandlerPRUDP? packetHandler = new(listener, BackendPID, listenPort, BackendPort, AccessKey, "RendezVous");

                    Timer timer = new(packetHandler.Update, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(5)); // Needs to be very tight.
                    _timers.Add(timer);

                    listener.OnStart(() =>
                    {
                        LoggerAccessor.LogInfo($"[Rendez-vous Service] - Server started on port {listenPort}");
                    });

                    listener.OnRecieve((endPoint, bytes) =>
                    {
                        if (endPoint is IPEndPoint EndPointIp)
                        {
                            LoggerAccessor.LogInfo($"[Rendez-vous Service] - Received request from {endPoint}");
                            Task.Run(() => packetHandler.ProcessPacket(bytes, EndPointIp));
                        }
                    });

                    listener.OnException(ex =>
                    {
                        LoggerAccessor.LogError($"[Rendez-vous Service] - DotNetty Thrown an exception : {ex}");
                    });

                    listener.OnStop(ex =>
                    {
                        LoggerAccessor.LogWarn("[Rendez-vous Service] - DotNetty was stopped!");
                        packetHandler = null;
                    });

                    _ = listener.StartAsync();

                    await Task.Delay(Timeout.Infinite);
                }
            }, _cts.Token);
        }
	}
}
