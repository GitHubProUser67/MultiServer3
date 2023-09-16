using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Hqc
{
    public class HqcKeyGenerationParameters : KeyGenerationParameters
    {
        private HqcParameters param;

        public HqcKeyGenerationParameters(
            SecureRandom random,
            HqcParameters param) : base(random, 256)
            {
                this.param = param;
            }

            public HqcParameters Parameters => param;
        }
}
