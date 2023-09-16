using System;

using MultiServer.Addons.Org.BouncyCastle.Crypto.Prng.Drbg;

namespace MultiServer.Addons.Org.BouncyCastle.Crypto.Prng
{
    internal interface IDrbgProvider
    {
        ISP80090Drbg Get(IEntropySource entropySource);
    }
}
