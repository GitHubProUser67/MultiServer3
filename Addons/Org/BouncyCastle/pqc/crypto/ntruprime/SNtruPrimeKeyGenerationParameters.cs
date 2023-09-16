using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.NtruPrime
{
    public class SNtruPrimeKeyGenerationParameters : KeyGenerationParameters
    {
        private SNtruPrimeParameters _primeParameters;
        
        public SNtruPrimeKeyGenerationParameters(SecureRandom random, SNtruPrimeParameters ntruPrimeParameters) : base(random,256)
        {
            this._primeParameters = ntruPrimeParameters;
        }

        public SNtruPrimeParameters Parameters => _primeParameters;

    }
}
