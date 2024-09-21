using CustomLogger;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace HTTPServer.Extensions
{
    public static class OtherExtensions
    {
        public static string ToHttpHeaders(this Dictionary<string, string> headers)
        {
            return string.Join("\r\n", headers.Select(x => string.Format("{0}: {1}", x.Key, x.Value)));
        }

        public static string GetString(this Stream stream)
        {
            return Encoding.ASCII.GetString(((MemoryStream)stream).ToArray());
        }

        public static bool IsConnected(this TcpClient tcpClient)
        {
            if (tcpClient.Client.Connected && tcpClient.Client.Poll(0, SelectMode.SelectWrite) && !tcpClient.Client.Poll(0, SelectMode.SelectError))
            {
                if (tcpClient.Client.Receive(new byte[1], SocketFlags.Peek) == 0)
                    return false;
                else
                    return true;
            }

            return false;
        }

        public static bool RemoveAt<T>(this HashSet<T> hashSet, int index)
        {
            if (index < 0 || index >= hashSet.Count)
            {
                LoggerAccessor.LogError($"[OtherExtensions] - HashSet - RemoveAt: Index is out of range");
                return false;
            }

            return hashSet.Remove(hashSet.Skip(index).First());
        }
    }
}
