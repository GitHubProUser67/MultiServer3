using CustomLogger;
using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;

namespace MitmDNS
{
    public class MitmDNSUDPProcessor
    {
        private bool _exit = false;
        private UdpClient listener = null;
        private CancellationTokenSource _cts = null!;

        public void Start(CancellationToken cancellationToken)
        {
            _exit = false;

            if (CyberBackendLibrary.TCP_IP.TCP_UDPUtils.IsUDPPortAvailable(53))
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                new Thread(() => HandleClient(53)).Start();
            }
            else
                LoggerAccessor.LogError("[DNS_UDP] - UDP Port 53 is occupied, UDP server failed to start!");
        }

        public void Stop()
        {
            _cts?.Cancel();
            _exit = true;
            listener?.Dispose();
        }

        public void HandleClient(int listenPort)
        {
            Task serverDNS = Task.Run(() =>
            {
                object _sync = new();
                Task<UdpReceiveResult> CurrentRecvTask = null;
                listener = new UdpClient(listenPort);

                LoggerAccessor.LogInfo($"[DNS_UDP] Server initiated on port: {listenPort}...");

                while (!_cts.Token.IsCancellationRequested)
                {
                    lock (_sync)
                    {
                        if (_exit)
                            break;
                    }

                    try
                    {
                        // use non-blocking recieve
                        if (CurrentRecvTask != null)
                        {
                            if (CurrentRecvTask.IsCompleted)
                            {
                                UdpReceiveResult result = CurrentRecvTask.Result;
                                CurrentRecvTask = null;
                                byte[] ResultBuffer = DNSResolver.ProcRequest(result.Buffer);
                                if (ResultBuffer != null)
                                    _ = listener.SendAsync(ResultBuffer, ResultBuffer.Length, result.RemoteEndPoint);
                            }
                            else if (CurrentRecvTask.IsCanceled || CurrentRecvTask.IsFaulted)
                                CurrentRecvTask = null;
                        }

                        if (CurrentRecvTask == null)
                        {
                            CurrentRecvTask = listener.ReceiveAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException is SocketException socketException &&
                            socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted)
                            LoggerAccessor.LogError($"[DNS_UDP] - HandleClient thrown an exception : {ex}");
                        CurrentRecvTask = null;
                    }

                    Thread.Sleep(1);
                }

                LoggerAccessor.LogWarn($"[DNS_UDP] Server on port: {listenPort} was stopped...");

                CurrentRecvTask = null;
            }, _cts.Token);
        }
    }
}
