using System;

using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters
{
    public class Ed448KeyGenerationParameters
        : KeyGenerationParameters
    {
        public Ed448KeyGenerationParameters(SecureRandom random)
            : base(random, 448)
        {
        }
    }
}
