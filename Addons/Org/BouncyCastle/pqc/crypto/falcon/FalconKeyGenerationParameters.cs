using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Falcon
{
    public class FalconKeyGenerationParameters
        : KeyGenerationParameters
    {
        private FalconParameters parameters;

        public FalconKeyGenerationParameters(SecureRandom random, FalconParameters parameters)
            : base(random, 320)
        {
            this.parameters = parameters;
        }

        public FalconParameters Parameters
        {
            get { return this.parameters; }
        }
    }
}
