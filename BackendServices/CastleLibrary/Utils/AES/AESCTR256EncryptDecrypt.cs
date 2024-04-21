using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Text;
using System;

namespace CastleLibrary.Utils.AES
{
    public class AESCTR256EncryptDecrypt
    {
        public static byte[] InitiateCTRBuffer(byte[]? FileBytes, byte[] KeyBytes, byte[]? m_iv)
        {
            if (KeyBytes != null && KeyBytes.Length >= 16 && m_iv != null && m_iv.Length == 16 && FileBytes != null)
            {
                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher("AES/CTR/NOPADDING");

                cipher.Init(false, new ParametersWithIV(new KeyParameter(KeyBytes), m_iv));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(FileBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(FileBytes, 0, FileBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return ciphertextBytes;
            }
            else
                CustomLogger.LoggerAccessor.LogError("[AESCTR256EncryptDecrypt] - InitiateCTRBuffer - Invalid FileBytes, KeyByes or IV!");

            return Array.Empty<byte>();
        }

        public static string? InitiateCTRBufferTobase64String(string FileString, byte[] KeyBytes, byte[]? m_iv)
        {
            if (KeyBytes != null && KeyBytes.Length >= 16 && m_iv != null && m_iv.Length == 16)
            {
                byte[] FileBytes = Encoding.UTF8.GetBytes(FileString);

                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher("AES/CTR/NOPADDING");

                cipher.Init(false, new ParametersWithIV(new KeyParameter(KeyBytes), m_iv));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(FileBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(FileBytes, 0, FileBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return Convert.ToBase64String(ciphertextBytes);
            }
            else
                CustomLogger.LoggerAccessor.LogError("[AESCTR256EncryptDecrypt] - InitiateCTRBuffer - Invalid FileString, KeyBytes or IV!");

            return null;
        }
    }
}
