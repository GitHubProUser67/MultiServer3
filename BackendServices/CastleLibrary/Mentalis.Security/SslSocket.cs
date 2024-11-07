using Org.Mentalis.Security.Certificates;
using Org.Mentalis.Security.Ssl;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace FixedSsl
{
    public static class SslSocket
    {
        static SslSocket()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        private const int SSLv3 = 0x0300;
        private const int TLSv1 = 0x0301;
        private static SecureProtocol legacyProtocols = SecureProtocol.Ssl3 | SecureProtocol.Tls1;
        public static async Task<Stream> AuthenticateAsServerAsync(Socket socket, X509Certificate2 certificate, bool forceSsl)
        {
            //no certificate, no ssl
            if (certificate == null)
                return new NetworkStream(socket, true);

            //content type - 1 byte
            //version - 2 bytes
            //length - 2 bytes
            //handshake type - 1 byte
            //length - 3 bytes
            //max version - 2 bytes (this is the actual ssl version we want to check)

            //total 11 bytes

            //read first 11 bytes, but do not consume them.
            byte[] buffer = new byte[11];
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            int received = await socket.ReceiveAsync(buffer, SocketFlags.Peek).ConfigureAwait(false);
#else
            int received = socket.Receive(buffer, SocketFlags.Peek);
#endif
            if (received != buffer.Length)
                return null;

            //content type needs to be handshake (0x16) and handshake type needs to be client hello (0x01)
            bool ssl = buffer[0] == 0x16 && buffer[5] == 0x01;

            if (!ssl)
            {
                if (forceSsl)
                    return null;
                return new NetworkStream(socket, true);
            }

            int maxSslVersion = buffer[9] << 8 | buffer[10];

            // Microsoft doesn't like our FESL exploit, so we fallback to a older crypto supported by Mentalis if that's the case.
            if (!certificate.Verify() || maxSslVersion == SSLv3 || maxSslVersion == TLSv1)
            {
                SecurityOptions options = new SecurityOptions(legacyProtocols, new Certificate(certificate), ConnectionEnd.Server);
                SecureSocket ss = new SecureSocket(socket, options);
                return new SecureNetworkStream(ss, true);
            }
            SslStream sslStream = new SslStream(new NetworkStream(socket, true), false);
            await sslStream.AuthenticateAsServerAsync(certificate).ConfigureAwait(false);
            return sslStream;
        }

        public static Stream AuthenticateAsServer(Socket socket, X509Certificate2 certificate, bool forceSsl)
        {
            //no certificate, no ssl
            if (certificate == null)
                return new NetworkStream(socket, true);

            //content type - 1 byte
            //version - 2 bytes
            //length - 2 bytes
            //handshake type - 1 byte
            //length - 3 bytes
            //max version - 2 bytes (this is the actual ssl version we want to check)

            //total 11 bytes

            //read first 11 bytes, but do not consume them.
            byte[] buffer = new byte[11];
            int received = socket.Receive(buffer, SocketFlags.Peek);
            if (received != buffer.Length)
                return null;

            //content type needs to be handshake (0x16) and handshake type needs to be client hello (0x01)
            bool ssl = buffer[0] == 0x16 && buffer[5] == 0x01;

            if (!ssl)
            {
                if (forceSsl)
                    return null;
                return new NetworkStream(socket, true);
            }

            int maxSslVersion = buffer[9] << 8 | buffer[10];

            // Microsoft doesn't like our FESL exploit, so we fallback to a older crypto supported by Mentalis if that's the case.
            if (!certificate.Verify() || maxSslVersion == SSLv3 || maxSslVersion == TLSv1)
            {
                SecurityOptions options = new SecurityOptions(legacyProtocols, new Certificate(certificate), ConnectionEnd.Server);
                SecureSocket ss = new SecureSocket(socket, options);
                return new SecureNetworkStream(ss, true);
            }

            SslStream sslStream = new SslStream(new NetworkStream(socket, true), false);
            sslStream.AuthenticateAsServer(certificate);
            return sslStream;
        }

        public static IAsyncResult BeginAuthenticateAsServer(Socket socket, X509Certificate2 certificate, bool forceSsl, AsyncCallback callback, object state)
        {
            return AuthenticateAsServerAsync(socket, certificate, forceSsl).AsApm(callback, state);
        }

        public static Stream EndAuthenticateAsServer(IAsyncResult result)
        {
            return ((Task<Stream>)result).Result;
        }

        #region Helpers
        private static IAsyncResult AsApm<T>(this Task<T> task,
                                    AsyncCallback callback,
                                    object state)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            var tcs = new TaskCompletionSource<T>(state);
            task.ContinueWith(t =>
            {
                if (t.IsFaulted && t.Exception != null && t.Exception.InnerExceptions != null)
                    tcs.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(t.Result);

                if (callback != null)
                    callback(tcs.Task);
            }, TaskScheduler.Default);
            return tcs.Task;
        }
        #endregion
    }
}