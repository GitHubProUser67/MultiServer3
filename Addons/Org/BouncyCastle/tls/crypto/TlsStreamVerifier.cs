using System;
using System.IO;

namespace MultiServer.Addons.Org.BouncyCastle.Tls.Crypto
{
    public interface TlsStreamVerifier
    {
        /// <exception cref="IOException"/>
        Stream Stream { get; }

        /// <exception cref="IOException"/>
        bool IsVerified();
    }
}
