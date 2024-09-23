using System.Net;

namespace MultiSocks.Utils
{
    public class NetworkUtils
    {
        public static uint GetIPAddressAsUInt(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                throw new ArgumentException(nameof(ipAddress));

            IPAddress address = IPAddress.Parse(ipAddress);
            byte[] bytes = address.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }
    }
}
