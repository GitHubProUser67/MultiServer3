using MultiServer.Addons.Org.BouncyCastle.Crypto;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Falcon
{
    public abstract class FalconKeyParameters 
        : AsymmetricKeyParameter
    {
        private readonly FalconParameters m_parameters;

        internal FalconKeyParameters(bool isprivate, FalconParameters parameters)
            : base(isprivate)
        {
            m_parameters = parameters;
        }

        public FalconParameters Parameters => m_parameters;
    }
}
