namespace CavemanTcp
{
    using System;
    using System.IO;
    using System.Security.Authentication;

    internal static class Common
    {
        internal static SslProtocols GetSslProtocol
        {
            get
            {
#pragma warning disable
                SslProtocols protocols = SslProtocols.Default | SslProtocols.Tls11 | SslProtocols.Tls12;
#pragma warning restore
#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER

                protocols |= SslProtocols.Tls13;

#endif

                return protocols;
            }
        }

        internal static byte[] StreamToBytes(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!input.CanRead) throw new InvalidOperationException("Input stream is not readable");

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;

                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

        internal static void ParseIpPort(string ipPort, out string ip, out int port)
        {
            if (String.IsNullOrEmpty(ipPort)) throw new ArgumentNullException(nameof(ipPort));

            ip = null;
            port = -1;

            int colonIndex = ipPort.LastIndexOf(':');
            if (colonIndex != -1)
            {
                ip = ipPort.Substring(0, colonIndex);
                port = Convert.ToInt32(ipPort.Substring(colonIndex + 1));
            }
        }
    }
}
