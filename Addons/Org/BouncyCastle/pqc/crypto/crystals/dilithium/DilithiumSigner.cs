using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters;
using MultiServer.Addons.Org.BouncyCastle.Security;

namespace MultiServer.Addons.Org.BouncyCastle.Pqc.Crypto.Crystals.Dilithium
{
    public class DilithiumSigner 
        : IMessageSigner
    {
        private DilithiumPrivateKeyParameters privKey;
        private DilithiumPublicKeyParameters pubKey;

        private SecureRandom random;

        public DilithiumSigner()
        {
        }

        public void Init(bool forSigning, ICipherParameters param)
        {
            if (forSigning)
            {
                if (param is ParametersWithRandom withRandom)
                {
                    privKey = (DilithiumPrivateKeyParameters)withRandom.Parameters;
                    random = withRandom.Random;
                }
                else
                {
                    privKey = (DilithiumPrivateKeyParameters)param;
                    random = null;
                }
            }
            else
            {
                pubKey = (DilithiumPublicKeyParameters)param;
                random = null;
            }
        }

        public byte[] GenerateSignature(byte[] message)
        {
            DilithiumEngine engine = privKey.Parameters.GetEngine(random);
            byte[] sig = new byte[engine.CryptoBytes];
            engine.Sign(sig, sig.Length, message, message.Length, privKey.m_rho, privKey.m_k, privKey.m_tr,
                privKey.m_t0, privKey.m_s1, privKey.m_s2);
            return sig;
        }

        public bool VerifySignature(byte[] message, byte[] signature)
        {
            DilithiumEngine engine = pubKey.Parameters.GetEngine(random);
            return engine.SignOpen(message,signature, signature.Length, pubKey.m_rho, pubKey.m_t1);
        }
    }
}
