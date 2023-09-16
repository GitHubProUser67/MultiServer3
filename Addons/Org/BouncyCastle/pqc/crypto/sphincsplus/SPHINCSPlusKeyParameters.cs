using MultiServer.Addons.Org.BouncyCastle.Crypto;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.SphincsPlus
{
    public abstract class SphincsPlusKeyParameters
        : AsymmetricKeyParameter
    {
        private readonly SphincsPlusParameters m_parameters;

        internal SphincsPlusKeyParameters(bool isPrivate, SphincsPlusParameters parameters)
            : base(isPrivate)
        {
            m_parameters = parameters;
        }

        public SphincsPlusParameters Parameters => m_parameters;
    }
}
