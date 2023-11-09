using CustomLogger;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace CryptoSporidium.UnBAR
{
    public class BlowfishCTREncryptDecrypt
    {
        public static void InitiateMetadataCryptoContext()
        {
            if (ToolsImpl.INFIVA == null)
            {
                byte[] nulledBytes = new byte[524]; // Encrypt 524 in bytes as shown in eboot.

                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher("Blowfish/CTR/NOPADDING");

                cipher.Init(false, new ParametersWithIV(new KeyParameter(ToolsImpl.MetaDataV1Key), ToolsImpl.MetaDataV1IV));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(nulledBytes.Length)];
                int ciphertextLength = cipher.ProcessBytes(nulledBytes, 0, nulledBytes.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                ToolsImpl.INFIVA = ciphertextBytes;
            }
        }

        public byte[]? EncryptionProxyInit(byte[]? Headerdata, byte[]? SignatureIV)
        {
            if (SignatureIV != null && SignatureIV.Length == 8 && Headerdata != null && Headerdata.Length == 24)
            {
                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher("Blowfish/CTR/NOPADDING");

                cipher.Init(false, new ParametersWithIV(new KeyParameter(ToolsImpl.SignatureKey), SignatureIV));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(Headerdata.Length)];
                int ciphertextLength = cipher.ProcessBytes(Headerdata, 0, Headerdata.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return ciphertextBytes;
            }
            else
                LoggerAccessor.LogError("[BlowfishCTREncryptDecrypt] - EncryptionProxyInit, Invalid Headerdata and/or SignatureIV!");

            return null;
        }

        public byte[]? InitiateCTRBuffer(byte[]? Data, byte[]? IV)
        {
            if (IV != null && IV.Length == 8 && Data != null)
            {
                // Create the cipher
                IBufferedCipher? cipher = CipherUtilities.GetCipher("Blowfish/CTR/NOPADDING");

                cipher.Init(false, new ParametersWithIV(new KeyParameter(ToolsImpl.DefaultKey), IV));

                // Encrypt the plaintext
                byte[] ciphertextBytes = new byte[cipher.GetOutputSize(Data.Length)];
                int ciphertextLength = cipher.ProcessBytes(Data, 0, Data.Length, ciphertextBytes, 0);
                cipher.DoFinal(ciphertextBytes, ciphertextLength);

                cipher = null;

                return ciphertextBytes;
            }
            else
                LoggerAccessor.LogError("[BlowfishCTREncryptDecrypt] - InitiateCTRBuffer, Invalid Data and/or IV!");

            return null;
        }
    }
}
