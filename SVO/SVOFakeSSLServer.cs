using CryptoSporidium;
using CustomLogger;
using System.Net;
using System.Net.Sockets;
using SVO.Extensions;

namespace SVO
{
    public class SVOFakeSSLServer
    {
        #region Fields

        private int Port;
        private TcpListener? Listener;
        private bool IsActive = true;

        #endregion

        #region Public Methods

        public SVOFakeSSLServer(int port)
        {
            Port = port;
        }

        public Task Listen()
        {
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();
            LoggerAccessor.LogInfo($"Fake SVO SSL Server initiated on port: {Port}...");
            while (IsActive)
            {
                try
                {
                    TcpClient tcpClient = Listener.AcceptTcpClient();

                    Thread thread = new(() =>
                    {
                        try
                        {
                            while (tcpClient.IsConnected())
                            {
                                NetworkStream ClientStream = tcpClient.GetStream();

                                if (tcpClient.Available > 0 && ClientStream.CanWrite)
                                {
                                    string? clientip = ((IPEndPoint?)tcpClient.Client.RemoteEndPoint).Address.ToString();

                                    string? clientport = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port.ToString();

                                    if (SVOServer.IsIPBanned(clientip))
                                    {
                                        LoggerAccessor.LogError($"[SECURITY] - Client - {clientip} Requested the Fake SVO server while being banned!");
                                        tcpClient.Close();
                                        return;
                                    }
                                    else
                                    {
                                        LoggerAccessor.LogInfo($"[SECURITY] - Client - {clientip}:{clientport} Requested the Fake SVO SSL server!");

                                        int mode = -1;

                                        byte[] bytes = new byte[65536];
                                        int len = 0;
                                        while ((len = ClientStream.Read(bytes, 0, bytes.Length)) != 0)
                                        {
                                            if (len >= 10 && bytes[0] == 0x80 && bytes[1] == 0x1C && bytes[2] == 0x01 && bytes[3] == 0x03 && bytes[4] == 0x00 && bytes[5] == 0x00 && bytes[6] == 0x03 && bytes[7] == 0x00 && bytes[8] == 0x00 && bytes[9] == 0x00 && bytes[10] == 0x10)
                                            {
                                                LoggerAccessor.LogInfo($"[Fake_SVO_SSL] - Client - {clientip}:{clientport} Requested a SSLv2 Client Hello!");
                                                mode = 0;
                                                break;
                                            }
                                        }

                                        switch (mode)
                                        {
                                            case 0:
                                                byte[] SCERT_Packet = MiscUtils.HexStringToByteArray(SSLPackets.SCERT_HELLO);
                                                ClientStream.Write(SCERT_Packet);
                                                SCERT_Packet = MiscUtils.HexStringToByteArray(SSLPackets.SCERT_HELLO_CONFIRM);
                                                ClientStream.Write(SCERT_Packet);
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
								
								ClientStream.Flush();
								ClientStream.Dispose();
                            }
                        }
                        catch (IOException ex)
                        {
                            if (ex.InnerException is SocketException socketException &&
                                socketException.SocketErrorCode != SocketError.ConnectionReset && socketException.SocketErrorCode != SocketError.ConnectionAborted)
                                LoggerAccessor.LogError($"[Fake_SVO_SSL] - HandleClient - IO-Socket thrown an exception : {ex}");
                        }
                        catch (SocketException ex)
                        {
                            if (ex.SocketErrorCode != SocketError.ConnectionReset && ex.SocketErrorCode != SocketError.ConnectionAborted)
                                LoggerAccessor.LogError($"[Fake_SVO_SSL] - HandleClient - Socket thrown an exception : {ex}");
                        }
                        catch (Exception ex)
                        {
                            LoggerAccessor.LogError($"[Fake_SVO_SSL] - HandleClient thrown an exception : {ex}");
                        }

                        tcpClient.Close();
                    });
                    thread.Start();
                    Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[Fake_SVO_SSL] - Listen thrown an exception : {ex}");
                }
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
