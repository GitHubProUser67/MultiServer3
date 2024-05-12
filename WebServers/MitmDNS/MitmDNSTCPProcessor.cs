using CustomLogger;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace MitmDNS
{
    public class MitmDNSTCPProcessor
    {
        private TcpListener? listener = null;
        private CancellationTokenSource _cts = null!;

        public void Start(CancellationToken cancellationToken)
        {
            if (CyberBackendLibrary.TCP_IP.TCP_UDPUtils.IsTCPPortAvailable(53))
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                new Thread(() => HandleClient(53)).Start();
            }
            else
                LoggerAccessor.LogError("[DNS_TCP] - TCP Port 53 is occupied, TCP server failed to start!");
        }

        public void Stop()
        {
            _cts?.Cancel();
            listener?.Stop();
            listener = null;
        }

        public void HandleClient(int listenPort)
        {
            Task serverDNS = Task.Run(async () =>
            {
                TcpListener Listener = new(IPAddress.Any, listenPort); // Set your listener
                Listener.Start(); // Start your listener

                LoggerAccessor.LogInfo($"[DNS_TCP] Server initiated on port: {listenPort}...");

                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        TcpClient Client = await Listener.AcceptTcpClientAsync();
                        _ = Task.Run(() => {

                            NetworkStream ClientStream = Client.GetStream();
                            if (ClientStream.CanRead)
                            {
                                byte[]? Buffer = new byte[Client.ReceiveBufferSize];
                                StringBuilder? SB = new();
                                do
                                {
                                    int BytesReaded = ClientStream.Read(Buffer, 0, Buffer.Length);
                                    SB.AppendFormat("{0}", Encoding.ASCII.GetString(Buffer, 0, BytesReaded));
                                } while (ClientStream.DataAvailable);

                                if (SB != null && SB.Length > 0)
                                {
                                    Buffer = DNSResolver.ProcRequest(Encoding.ASCII.GetBytes(SB.ToString()));
                                    if (Buffer != null)
                                        _ = ClientStream.WriteAsync(Buffer, 0, Buffer.Length);
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException is SocketException socketException &&
                            socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted)
                            LoggerAccessor.LogError($"[DNS_TCP] - HandleClient thrown an exception : {ex}");
                    }

                    Thread.Sleep(1);
                }

                LoggerAccessor.LogWarn($"[DNS_TCP] Server on port: {listenPort} was stopped...");

            }, _cts.Token);
        }
    }
}
