using System.Net.Sockets;
using CustomLogger;
using QuazalServer.QNetZ;

namespace QuazalServer.ServerProcessors
{
	public class RDVServer
	{
		public readonly object _sync = new();
		public uint BackendPID = 2;
		public bool _exit = false;
		private UdpClient? listener;
		public ushort _skipNextNAT = 0xFFFF;
		QPacketHandlerPRUDP? packetHandler;
		Task<UdpReceiveResult>? CurrentRecvTask = null;

		public void Start(int listenPort, int BackendPort, uint BackendPID, string AccessKey)
		{
			this.BackendPID = BackendPID;
            _exit = false;
            new Thread(() => HandleClient(listenPort, BackendPort, AccessKey, null)).Start();
		}

		public void Stop()
		{
			lock (_sync)
			{
				_exit = true;
			}
		}

		public void HandleClient(int listenPort, int BackendPort, string AccessKey, object? obj)
		{
			listener = new UdpClient(listenPort);
			packetHandler = new QPacketHandlerPRUDP(listener, BackendPID, listenPort, BackendPort, AccessKey, "RendezVous");

            LoggerAccessor.LogInfo($"Rendez-vous Server initiated on port: {listenPort}...");

            while (true)
			{
				lock (_sync)
				{
					if (_exit)
						break;
				}

				try
				{
					packetHandler.Update();

					// use non-blocking recieve
					if (CurrentRecvTask != null)
					{
						if (CurrentRecvTask.IsCompleted)
						{
                            UdpReceiveResult result = CurrentRecvTask.Result;
							CurrentRecvTask = null;
							packetHandler.ProcessPacket(result.Buffer, result.RemoteEndPoint);
						}
						else if (CurrentRecvTask.IsCanceled || CurrentRecvTask.IsFaulted)
                            CurrentRecvTask = null;
                    }

                    if (CurrentRecvTask == null)
						CurrentRecvTask = listener.ReceiveAsync();
				}
                catch (Exception ex)
                {
                    if (ex.InnerException is SocketException socketException &&
                        socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted)
                        LoggerAccessor.LogError($"[Rendez-vous] - HandleClient thrown an exception : {ex}");
                    CurrentRecvTask = null;
                }

                Thread.Sleep(1);
            }

            LoggerAccessor.LogWarn($"Rendez-vous Server stopped.");

            CurrentRecvTask = null;
			listener.Close();
		}
	}
}
