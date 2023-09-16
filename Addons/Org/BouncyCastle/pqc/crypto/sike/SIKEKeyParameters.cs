using System;

using MultiServer.Addons.Org.BouncyCastle.Crypto;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Sike
{
    [Obsolete("Will be removed")]
    public abstract class SikeKeyParameters
        : AsymmetricKeyParameter
    {
        private readonly SikeParameters m_parameters;

        internal SikeKeyParameters(bool isPrivate, SikeParameters parameters)
            : base(isPrivate)
        {
            m_parameters = parameters;
        }

        public SikeParameters Parameters => m_parameters;
    }
}
