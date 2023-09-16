using System;

using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Sike
{
    [Obsolete("Will be removed")]
    public sealed class SikeKeyGenerationParameters
        : KeyGenerationParameters
    {
        private readonly SikeParameters m_parameters;

        public SikeKeyGenerationParameters(SecureRandom random, SikeParameters sikeParameters)
            : base(random, 256)
        {
            m_parameters = sikeParameters;
        }

        public SikeParameters Parameters => m_parameters;
    }
}
