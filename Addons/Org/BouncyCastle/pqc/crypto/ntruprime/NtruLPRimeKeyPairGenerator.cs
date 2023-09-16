using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.NtruPrime
{
    public class NtruLPRimeKeyPairGenerator
    {
        private NtruLPRimeKeyGenerationParameters _ntruPrimeParams;

        private int p;
        private int q;

        private SecureRandom random;

        private void Initialize(KeyGenerationParameters param)
        {
            _ntruPrimeParams = (NtruLPRimeKeyGenerationParameters) param;
            random = param.Random;

            // n = ntruParams.Parameters.N;

            p = _ntruPrimeParams.Parameters.P;
            q = _ntruPrimeParams.Parameters.Q;

        }

        private AsymmetricCipherKeyPair GenKeyPair()
        {
            NtruPrimeEngine primeEngine = _ntruPrimeParams.Parameters.PrimeEngine;
            byte[] sk = new byte[primeEngine.PrivateKeySize];
            byte[] pk = new byte[primeEngine.PublicKeySize];
            primeEngine.kem_keypair( pk,sk,random);

            NtruLPRimePublicKeyParameters pubKey = new NtruLPRimePublicKeyParameters(_ntruPrimeParams.Parameters, pk);
            NtruLPRimePrivateKeyParameters privKey = new NtruLPRimePrivateKeyParameters(_ntruPrimeParams.Parameters, sk);
            return new AsymmetricCipherKeyPair(pubKey, privKey);
        }
        
        public void Init(KeyGenerationParameters param)
        {
            this.Initialize(param);
        }
        
        public AsymmetricCipherKeyPair GenerateKeyPair()
        {
            return GenKeyPair();
        }
    }
}
