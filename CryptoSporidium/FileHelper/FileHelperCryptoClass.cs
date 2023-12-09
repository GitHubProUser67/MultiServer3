using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;
using CustomLogger;

namespace CryptoSporidium.FileHelper
{
    public class FileHelperCryptoClass
    {
        /// <summary>
        /// Gets an encryption key from rave secret key.
        /// </summary>
        /// <param name="secretKey">The secret key generated from your rave dashboard</param>
        /// <returns>a string value encrypted</returns>
        public static string GetEncryptionKey(string secretKey)
        {
            // MD5 is the hash algorithm expected by rave to generate encryption key
            var md5 = MD5.Create();

            // MD5 works with bytes so a conversion of plain secretKey to it bytes equivalent is required.
            byte[] secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            byte[] hashedSecret = md5.ComputeHash(secretKeyBytes, 0, secretKeyBytes.Length);
            byte[] hashedSecretLast12Bytes = new byte[12];

            Array.Copy(hashedSecret, hashedSecret.Length - 12, hashedSecretLast12Bytes, 0, 12);
            string hashedSecretLast12HexString = BitConverter.ToString(hashedSecretLast12Bytes);

            hashedSecretLast12HexString = hashedSecretLast12HexString.ToLower().Replace("-", "");

            string secretKeyFirst12 = secretKey.Replace("FLWSECK-", "").Substring(0, 12);

            byte[] hashedSecretLast12HexBytes = Encoding.UTF8.GetBytes(hashedSecretLast12HexString);
            byte[] secretFirst12Bytes = Encoding.UTF8.GetBytes(secretKeyFirst12);
            byte[] combineKey = new byte[24];

            Array.Copy(secretFirst12Bytes, 0, combineKey, 0, secretFirst12Bytes.Length);
            Array.Copy(hashedSecretLast12HexBytes, hashedSecretLast12HexBytes.Length - 12, combineKey, 12, 12);

            return Encoding.UTF8.GetString(combineKey);
        }

        public static byte[] DecryptData(byte[] encryptedData, string encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey))
                return Array.Empty<byte>();

            CustomXTEA? xtea = new();

            try
            {
                TripleDES des = TripleDES.Create();
                des.Key = Encoding.UTF8.GetBytes(encryptionKey);
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;

                byte[] firstSixBytes = new byte[6];

                Array.Copy(encryptedData, 0, firstSixBytes, 0, firstSixBytes.Length);

                if (MiscUtils.FindbyteSequence(firstSixBytes, new byte[] { 0x58, 0x54, 0x4e, 0x44, 0x56, 0x32 }))
                {
                    byte[] xteakey = new byte[16];

                    byte[] cipheredkey = new byte[xteakey.Length];

                    Array.Copy(des.Key, 0, xteakey, 0, xteakey.Length);

                    // With PSMultiServer 1.3 and up, TripleDes is improved with a custom crypto on top.

                    cipheredkey = InitiateCustomXTEACipheredKey(xteakey, ReverseByteArray(xteakey));

                    if (cipheredkey != null)
                    {
                        byte[] dst = new byte[encryptedData.Length - 6];

                        Array.Copy(encryptedData, 6, dst, 0, dst.Length);

                        encryptedData = xtea.Decrypt(dst, cipheredkey);

                        if (encryptedData == null)
                        {
                            des.Dispose();
                            xtea = null;
                            return Array.Empty<byte>();
                        }
                    }
                    else
                    {
                        des.Dispose();
                        xtea = null;
                        return Array.Empty<byte>();
                    }
                }

                ICryptoTransform cryptoTransform = des.CreateDecryptor();
                encryptedData = cryptoTransform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                cryptoTransform.Dispose();

                des.Dispose();
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogInfo($"[FileHelperCryptoClass] : has throw an exception in DecryptData - {ex}");
            }

            xtea = null;

            return encryptedData;
        }

        public static byte[] InitiateCustomXTEACipheredKey(byte[] KeyBytes, byte[] ReversedKeyBytes)
        {
            // Create the cipher
            IBufferedCipher? cipher = CipherUtilities.GetCipher("AES/ECB/NOPADDING");

            cipher.Init(true, new KeyParameter(ReversedKeyBytes));

            // Encrypt the plaintext
            byte[] ciphertextBytes = new byte[cipher.GetOutputSize(KeyBytes.Length)];
            int ciphertextLength = cipher.ProcessBytes(KeyBytes, 0, KeyBytes.Length, ciphertextBytes, 0);
            cipher.DoFinal(ciphertextBytes, ciphertextLength);

            cipher = null;

            return ciphertextBytes;
        }

        private static byte[] ReverseByteArray(byte[] input)
        {
            byte[] reversedArray = new byte[input.Length];
            int lastIndex = input.Length - 1;

            for (int i = 0; i < input.Length; i++)
            {
                reversedArray[i] = input[lastIndex - i];
            }

            return reversedArray;
        }
    }
}
