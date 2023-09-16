using System;
using System.IO;

namespace MultiServer.Addons.Org.BouncyCastle.Tls.Crypto
{
    public interface TlsStreamSigner
    {
        /// <exception cref="IOException"/>
        Stream Stream { get; }

        /// <exception cref="IOException"/>
        byte[] GetSignature();
    }
}
