using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium
{
    public class DilithiumKeyGenerationParameters
        : KeyGenerationParameters
    {
        private DilithiumParameters parameters;

        public DilithiumKeyGenerationParameters(SecureRandom random, DilithiumParameters parameters) : base(random, 255)
        {
            this.parameters = parameters;
        }

        public DilithiumParameters Parameters => parameters;
    }
}