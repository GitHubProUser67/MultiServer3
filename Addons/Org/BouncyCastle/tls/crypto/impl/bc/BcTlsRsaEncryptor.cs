using System;

using MultiServer.Addons.Org.BouncyCastle.Crypto;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Encodings;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Engines;
using MultiServer.Addons.Org.BouncyCastle.Crypto.Parameters;

namespace MultiServer.Addons.Org.BouncyCastle.Tls.Crypto.Impl.BC
{
    internal sealed class BcTlsRsaEncryptor
        : TlsEncryptor
    {
        private static RsaKeyParameters CheckPublicKey(RsaKeyParameters pubKeyRsa)
        {
            if (null == pubKeyRsa || pubKeyRsa.IsPrivate)
                throw new ArgumentException("No public RSA key provided", "pubKeyRsa");

            return pubKeyRsa;
        }

        private readonly BcTlsCrypto m_crypto;
        private readonly RsaKeyParameters m_pubKeyRsa;

        internal BcTlsRsaEncryptor(BcTlsCrypto crypto, RsaKeyParameters pubKeyRsa)
        {
            this.m_crypto = crypto;
            this.m_pubKeyRsa = CheckPublicKey(pubKeyRsa);
        }

        public byte[] Encrypt(byte[] input, int inOff, int length)
        {
            try
            {
                Pkcs1Encoding encoding = new Pkcs1Encoding(new RsaBlindedEngine());
                encoding.Init(true, new ParametersWithRandom(m_pubKeyRsa, m_crypto.SecureRandom));
                return encoding.ProcessBlock(input, inOff, length);
            }
            catch (InvalidCipherTextException e)
            {
                /*
                 * This should never happen, only during decryption.
                 */
                throw new TlsFatalAlert(AlertDescription.internal_error, e);
            }
        }
    }
}
