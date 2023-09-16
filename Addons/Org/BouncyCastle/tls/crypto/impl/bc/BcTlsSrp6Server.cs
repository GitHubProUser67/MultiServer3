using System;

using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Agreement.Srp;
using MultiServer.Addons.Org.BouncyCastle.Math;

namespace MultiServer.Addons.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    internal sealed class BcTlsSrp6Server
        : TlsSrp6Server
    {
        private readonly Srp6Server m_srp6Server;

        internal BcTlsSrp6Server(Srp6Server srp6Server)
        {
            this.m_srp6Server = srp6Server;
        }

        public BigInteger GenerateServerCredentials()
        {
            return m_srp6Server.GenerateServerCredentials();
        }

        public BigInteger CalculateSecret(BigInteger clientA)
        {
            try
            {
                return m_srp6Server.CalculateSecret(clientA);
            }
            catch (CryptoException e)
            {
                throw new TlsFatalAlert(AlertDescription.illegal_parameter, e);
            }
        }
    }
}
