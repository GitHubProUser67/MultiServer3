using System;

using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters
{
    public class X448KeyGenerationParameters
        : KeyGenerationParameters
    {
        public X448KeyGenerationParameters(SecureRandom random)
            : base(random, 448)
        {
        }
    }
}
