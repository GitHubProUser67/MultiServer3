using System;

using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters
{
    public class X25519KeyGenerationParameters
        : KeyGenerationParameters
    {
        public X25519KeyGenerationParameters(SecureRandom random)
            : base(random, 255)
        {
        }
    }
}
