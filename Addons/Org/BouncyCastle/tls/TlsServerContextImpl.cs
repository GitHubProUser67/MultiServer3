using System;

using MultiServer.Addons.Org.BouncyCastle.Tls.Crypto;

namespace MultiServer.Addons.Org.BouncyCastle.Tls
{
    internal class TlsServerContextImpl
        : AbstractTlsContext, TlsServerContext
    {
        internal TlsServerContextImpl(TlsCrypto crypto)
            : base(crypto, ConnectionEnd.server)
        {
        }

        public override bool IsServer
        {
            get { return true; }
        }
    }
}
