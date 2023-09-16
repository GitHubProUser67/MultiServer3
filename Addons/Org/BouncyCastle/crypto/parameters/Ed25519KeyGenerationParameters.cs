using System;

using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters
{
    public class Ed25519KeyGenerationParameters
        : KeyGenerationParameters
    {
        public Ed25519KeyGenerationParameters(SecureRandom random)
            : base(random, 256)
        {
        }
    }
}
