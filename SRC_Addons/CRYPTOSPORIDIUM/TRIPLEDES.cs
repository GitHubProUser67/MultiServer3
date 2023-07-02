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
            TripleDES des = TripleDES.Create();
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;
            des.Key = Encoding.UTF8.GetBytes(encryptionKey);

            ICryptoTransform cryptoTransform = des.CreateEncryptor();
            byte[] encryptedDataBytes = cryptoTransform.TransformFinalBlock(data, 0, data.Length);

            des.Dispose();

            return encryptedDataBytes;
        }

        public static byte[] DecryptData(byte[] encryptedData, string encryptionKey)
        {
            TripleDES des = TripleDES.Create();
            des.Key = Encoding.UTF8.GetBytes(encryptionKey);
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            ICryptoTransform cryptoTransform = des.CreateDecryptor();
            byte[] plainDataBytes = cryptoTransform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

            des.Dispose();

            return plainDataBytes;
        }
    }
}
