using System.Net.Sockets;

namespace SVO.Extensions
{
    public static class OtherExtensions
    {
        public static bool IsConnected(this TcpClient tcpClient)
        {
            if (tcpClient.Client.Connected)
            {
                if (tcpClient.Client.Poll(0, SelectMode.SelectWrite) && !tcpClient.Client.Poll(0, SelectMode.SelectError))
                {
                    byte[] buffer = new byte[1];
                    if (tcpClient.Client.Receive(buffer, SocketFlags.Peek) == 0)
                        return false;
                    else
                        return true;
                }
                else
                    return false;
            }

            return false;
        }
    }
}
