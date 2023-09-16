using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Bike
{
    public sealed class BikeKeyGenerationParameters
        : KeyGenerationParameters
    {
        private readonly BikeParameters m_parameters;

        public BikeKeyGenerationParameters(SecureRandom random, BikeParameters parameters)
            : base(random, 256)
        {
            m_parameters = parameters;
        }

        public BikeParameters Parameters => m_parameters;
    }
}
