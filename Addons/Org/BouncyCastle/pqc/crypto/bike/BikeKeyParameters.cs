using MultiServer.Addons.Org.BouncyCastle.Crypto;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Bike
{
    public abstract class BikeKeyParameters
        : AsymmetricKeyParameter
    {
        private readonly BikeParameters m_parameters;

        internal BikeKeyParameters(bool isPrivate, BikeParameters parameters)
            : base(isPrivate)
        {
            this.m_parameters = parameters;
        }

        public BikeParameters Parameters => m_parameters;
    }
}
