using System;

using MultiServer.Addons.Org.BouncyCastle.Tls.Crypto;

namespace MultiServer.Addons.Org.BouncyCastle.Tls
{
    public interface TlsPsk
    {
        byte[] Identity { get; }

        TlsSecret Key { get; }

        int PrfAlgorithm { get; }
    }
}
