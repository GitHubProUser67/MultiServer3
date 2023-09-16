using MultiServer.Addons.Org.BouncyCastle.Crypto;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Lms
{
    public sealed class HssKeyPairGenerator
        : IAsymmetricCipherKeyPairGenerator
    {
        private HssKeyGenerationParameters m_parameters;

        public void Init(KeyGenerationParameters parameters)
        {
            m_parameters = (HssKeyGenerationParameters)parameters;
        }

        public AsymmetricCipherKeyPair GenerateKeyPair()
        {
            HssPrivateKeyParameters privKey = Hss.GenerateHssKeyPair(m_parameters);

            return new AsymmetricCipherKeyPair(privKey.GetPublicKey(), privKey);
        }
    }
}
