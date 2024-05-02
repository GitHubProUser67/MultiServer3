using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace CastleLibrary.Utils.Blowfish
{
    public class BlowfishCTREncryptDecrypt
    {
        public static byte[]? ProcessCTRBuffer(byte[] FileBytes, byte[] KeyBytes, byte[] m_iv)
        {
            if (KeyBytes.Length == 32 && m_iv.Length == 8)
            {
                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher("Blowfish/CTR/NOPADDING");

                cipher.Init(false, new ParametersWithIV(new KeyParameter(KeyBytes), m_iv));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(FileBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(FileBytes, 0, FileBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return ciphertextBytes;
            }
            else
                CustomLogger.LoggerAccessor.LogError("[BlowfishCTREncryptDecrypt] - ProcessCTRBuffer - KeyBytes or IV!");

            return null;
        }
    }
}
