using CustomLogger;
using QuazalServer.QNetZ;
using System.Net;
using System.Net.Sockets;

namespace QuazalServer.ServerProcessors
{
	public class BackendServicesServer
	{
		public uint serverPID = 2;
		public readonly object _sync = new();
		public bool _exit = false;
		public UdpClient? listener;
		public ushort _skipNextNAT = 0xFFFF;
		public QPacketHandlerPRUDP? packetHandler;
		Task<UdpReceiveResult>? CurrentRecvTask = null;

		public void Start(int listenport, uint PID, string AccessKey)
		{
			serverPID = PID;
            _exit = false;
            new Thread(() => HandleClient(listenport, AccessKey, null)).Start();
		}

		public void Stop()
		{
			lock (_sync)
			{
				_exit = true;
			}
		}

		public void HandleClient(int listenPort, string AccessKey, object? obj)
		{
			listener = new UdpClient(listenPort);
			packetHandler = new QPacketHandlerPRUDP(listener, serverPID, listenPort, listenPort, AccessKey, "BackendServices");
			packetHandler.Updates.Add(() => NetworkPlayers.DropPlayers());

            LoggerAccessor.LogInfo($"Backend Service Server initiated on port: {listenPort}...");

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
                        LoggerAccessor.LogError($"[Backend Service] - HandleClient thrown an exception : {ex}");
                    CurrentRecvTask = null;
                }

                Thread.Sleep(1);
            }

            LoggerAccessor.LogWarn($"Backend Service Server stopped.");

            CurrentRecvTask = null;
			listener.Close();
		}

		public void ProcessPacket(byte[] data, IPEndPoint ep)
		{
			packetHandler?.ProcessPacket(data, ep);
		}
	}
}
