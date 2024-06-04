using CustomLogger;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace CastleLibrary.Utils.Blowfish
{
    public class BlowfishECBDecrypt
    {
        public static byte[] ProcessECBBuffer(byte[] FileBytes, byte[] KeyBytes)
        {
            if (KeyBytes.Length == 32)
            {
                // Create the cipher
                IBufferedCipher cipher = CipherUtilities.GetCipher("Blowfish/ECB/NOPADDING");

                cipher.Init(false, new KeyParameter(KeyBytes));

                // Decrypt the ciphertext
                byte[] plaintextBytes = new byte[cipher.GetOutputSize(FileBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(FileBytes, 0, FileBytes.Length, plaintextBytes, 0);
                cipher.DoFinal(plaintextBytes, ciphertextLength);

                cipher = null;

                return plaintextBytes;
            }
            else
                LoggerAccessor.LogError("[BlowfishECBDecrypt] - ProcessECBBuffer, Invalid KeyBytes or IV!");

            return null;
        }
    }
}
