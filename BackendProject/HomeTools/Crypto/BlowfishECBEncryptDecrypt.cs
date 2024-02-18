using CustomLogger;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace BackendProject.HomeTools.Crypto
{
    public class BlowfishECBEncryptDecrypt
    {
        public byte[]? ProcessECBBuffer(byte[]? InData)
        {
            if (InData != null)
            {
                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher("Blowfish/ECB/NOPADDING");

                cipher.Init(false, new KeyParameter(ToolsImpl.DefaultKey));

                // Decrypt the ciphertext
                byte[] plaintextBytes = new byte[cipher.GetOutputSize(InData.Length)];
                int ciphertextLength = cipher.ProcessBytes(InData, 0, InData.Length, plaintextBytes, 0);
                cipher.DoFinal(plaintextBytes, ciphertextLength);

                cipher = null;

                return plaintextBytes;
            }
            else
                LoggerAccessor.LogError("[BlowfishECBEncryptDecrypt] - ProcessECBBuffer, Invalid Data and/or IV!");

            return null;
        }
    }
}
