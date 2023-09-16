using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Utilities;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Crystals.Kyber
{
    public sealed class KyberKemExtractor
        : IEncapsulatedSecretExtractor
    {
        private readonly KyberKeyParameters m_key;
        private readonly KyberEngine m_engine;


        public KyberKemExtractor(KyberKeyParameters privParams)
        {
            m_key = privParams;
            m_engine = m_key.Parameters.Engine;
        }

        public byte[] ExtractSecret(byte[] encapsulation)
        {
            byte[] sharedSecret = new byte[m_engine.CryptoBytes];
            m_engine.KemDecrypt(sharedSecret, encapsulation, ((KyberPrivateKeyParameters)m_key).GetEncoded());
            return sharedSecret;
        }

        public int EncapsulationLength => m_engine.CryptoCipherTextBytes;
    }
}