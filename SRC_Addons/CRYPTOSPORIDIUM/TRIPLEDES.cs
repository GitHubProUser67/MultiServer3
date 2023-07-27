using System.Security.Cryptography;
using System.Text;

namespace PSMultiServer.SRC_Addons.CRYPTOSPORIDIUM
{
    internal class TRIPLEDES
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

        public static byte[] EncryptData(string encryptionKey, byte[] data)
        {
            try
            {
                byte[] xteakey = new byte[16];

                byte[] cipheredkey = new byte[xteakey.Length];

                TripleDES des = TripleDES.Create();
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;
                des.Key = Encoding.UTF8.GetBytes(encryptionKey);

                Array.Copy(des.Key, 0, xteakey, 0, 16);

                ICryptoTransform cryptoTransform = des.CreateEncryptor();
                byte[] encryptedDataBytes = cryptoTransform.TransformFinalBlock(data, 0, data.Length);

                // With PSMultiServer 1.3 and up, TripleDes is improved with a custom crypto on top.
                AES128 aes = new AES128();

                aes.Key = Misc.ReverseByteArray(xteakey);

                bool success = aes.Encrypt(xteakey, ref cipheredkey);

                if (success)
                {
                    byte[] outfile = new byte[] { 0x58, 0x54, 0x4e, 0x44, 0x56, 0x32 };

                    encryptedDataBytes = Misc.Combinebytearay(outfile, CUSTOMXTEA.Encrypt(encryptedDataBytes, cipheredkey));
                }
                else
                {
                    // Encryption failed, fallback to classic crypto.
                }

                des.Dispose();

                return encryptedDataBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRYPTOSPORIDIUM : has throw an exception in TripleDes EncryptData - {ex}");

                return Encoding.UTF8.GetBytes("ERROR IN EncryptData");
            }
        }

        public static byte[] DecryptData(byte[] encryptedData, string encryptionKey)
        {
            try
            {
                TripleDES des = TripleDES.Create();
                des.Key = Encoding.UTF8.GetBytes(encryptionKey);
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;

                byte[] firstSixBytes = new byte[6];

                Array.Copy(encryptedData, 0, firstSixBytes, 0, 6);

                if (Misc.FindbyteSequence(firstSixBytes, new byte[] { 0x58, 0x54, 0x4e, 0x44, 0x56, 0x32 }))
                {
                    byte[] xteakey = new byte[16];

                    byte[] cipheredkey = new byte[xteakey.Length];

                    Array.Copy(des.Key, 0, xteakey, 0, 16);

                    // With PSMultiServer 1.3 and up, TripleDes is improved with a custom crypto on top.
                    AES128 aes = new AES128();

                    aes.Key = Misc.ReverseByteArray(xteakey);

                    bool success = aes.Encrypt(xteakey, ref cipheredkey);

                    if (success)
                    {
                        byte[] dst = new byte[encryptedData.Length - 6];

                        Array.Copy(encryptedData, 6, dst, 0, dst.Length);

                        encryptedData = CUSTOMXTEA.Decrypt(dst, cipheredkey);
                    }
                    else
                    {
                        // Encryption failed, fallback to classic crypto.
                    }
                }

                ICryptoTransform cryptoTransform = des.CreateDecryptor();
                byte[] plainDataBytes = cryptoTransform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                des.Dispose();

                return plainDataBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRYPTOSPORIDIUM : has throw an exception in TripleDes DecryptData - {ex}");

                return Encoding.UTF8.GetBytes("ERROR IN DecryptData");
            }
        }
    }
}
