using MultiServer.Addons.Org.BouncyCastle.Crypto;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Saber
{
    public abstract class SaberKeyParameters
        : AsymmetricKeyParameter
    {
        private readonly SaberParameters m_parameters;

        internal SaberKeyParameters(bool isPrivate, SaberParameters parameters)
            : base(isPrivate)
        {
            m_parameters = parameters;
        }

        public SaberParameters Parameters => m_parameters;
    }
}
