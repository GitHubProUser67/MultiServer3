using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Utilities;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Cmce
{
    public sealed class CmceKemGenerator
        : IEncapsulatedSecretGenerator
    {
        // the source of randomness
        private readonly SecureRandom sr;

        public CmceKemGenerator(SecureRandom random)
        {
            this.sr = random;
        }

        public ISecretWithEncapsulation GenerateEncapsulated(AsymmetricKeyParameter recipientKey)
        {
            CmcePublicKeyParameters key = (CmcePublicKeyParameters)recipientKey;
            ICmceEngine engine = key.Parameters.Engine;

            return GenerateEncapsulated(recipientKey, engine.DefaultSessionKeySize);
        }

        private ISecretWithEncapsulation GenerateEncapsulated(AsymmetricKeyParameter recipientKey, int sessionKeySizeInBits)
        {
            CmcePublicKeyParameters key = (CmcePublicKeyParameters)recipientKey;
            ICmceEngine engine = key.Parameters.Engine;
            byte[] cipher_text = new byte[engine.CipherTextSize];
            byte[] sessionKey = new byte[sessionKeySizeInBits / 8];     // document as 32 - l/8  - Section 2.5.2
            engine.KemEnc(cipher_text, sessionKey, key.publicKey, sr);
            return new SecretWithEncapsulationImpl(sessionKey, cipher_text);
        }
    }
}
