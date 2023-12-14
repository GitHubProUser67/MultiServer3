using System.Net.Sockets;
using CustomLogger;
using QuazalServer.QNetZ;
using QuazalServer.RDVServices;

namespace QuazalServer.ServerProcessors
{
	public static class RDVServer
	{
		public static readonly object _sync = new();
		public static bool _exit = false;
		private static UdpClient? listener;
		public static ushort _skipNextNAT = 0xFFFF;
		static QPacketHandlerPRUDP? packetHandler;

		static Task<UdpReceiveResult>? CurrentRecvTask = null;

		public static void Start()
		{
			_exit = false;
            new Thread(HandleClient).Start();
		}

		public static void Stop()
		{
			lock (_sync)
			{
				_exit = true;
			}
		}

		public static void HandleClient(object? obj)
		{
			int listenPort = QuazalServerConfiguration.RDVServerPort;

			listener = new UdpClient(listenPort);
			packetHandler = new QPacketHandlerPRUDP(listener, BackendServicesServer.serverPID, listenPort, "RendezVous");

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
