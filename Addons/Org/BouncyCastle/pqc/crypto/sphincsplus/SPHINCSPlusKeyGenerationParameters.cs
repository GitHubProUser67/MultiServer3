using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.SphincsPlus
{
    public sealed class SphincsPlusKeyGenerationParameters
        : KeyGenerationParameters
    {
        private readonly SphincsPlusParameters m_parameters;

        public SphincsPlusKeyGenerationParameters(SecureRandom random, SphincsPlusParameters parameters)
            : base(random, 256)
        {
            m_parameters = parameters;
        }

        public SphincsPlusParameters Parameters => m_parameters;
    }
}
