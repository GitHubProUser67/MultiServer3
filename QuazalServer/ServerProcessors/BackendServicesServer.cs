using CustomLogger;
using QuazalServer.QNetZ;
using System.Net;
using System.Net.Sockets;

namespace QuazalServer.ServerProcessors
{
	public static class BackendServicesServer
	{
		public static readonly uint serverPID = 2;
		public static readonly object _sync = new();
		public static bool _exit = false;
		public static UdpClient? listener;
		public static ushort _skipNextNAT = 0xFFFF;
		public static QPacketHandlerPRUDP? packetHandler;

		static Task<UdpReceiveResult>? CurrentRecvTask = null;

		public static void Start()
		{
			_exit = false;
            new Thread(tMainThread).Start();
		}

		public static void Stop()
		{
			lock (_sync)
			{
				_exit = true;
			}
		}

		public static void tMainThread(object obj)
		{
			int listenPort = QuazalServerConfiguration.BackendServiceServerPort;

			listener = new UdpClient(listenPort);
			packetHandler = new QPacketHandlerPRUDP(listener, serverPID, listenPort, "BackendServices");
			packetHandler.Updates.Add(() => NetworkPlayers.DropPlayers());

            LoggerAccessor.LogInfo($"Backend Server initiated on port: {listenPort}...");

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
							var result = CurrentRecvTask.Result;
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
                        LoggerAccessor.LogError($"[Backend] - HandleClient thrown an exception : {ex}");
                    CurrentRecvTask = null;
                }

                Thread.Sleep(1);
            }

            LoggerAccessor.LogWarn($"Backend Server stopped.");

            CurrentRecvTask = null;
			listener.Close();
		}

		public static void ProcessPacket(byte[] data, IPEndPoint ep)
		{
			packetHandler?.ProcessPacket(data, ep);
		}
	}
}
