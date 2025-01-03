using NetHasher;
using NetworkLibrary.Crypto;
using NetworkLibrary.Extension;
using System.Text;

namespace WebAPIService.SSFW
{
    public static class GuidGenerator
    {
        public static string SSFWGenerateGuid(string input1, string input2, string key = null)
        {
            string md5hash;
            string sha512hash;
            byte[] input1Bytes = Encoding.UTF8.GetBytes(input1 + "**H0mEIsG3reAT!!!!!!!!!!!!!!");
            byte[] input2Bytes = Encoding.UTF8.GetBytes("C0MeBaCKHOm3*!*!*!*!*!*!*!*!" + input2);

            if (!string.IsNullOrEmpty(key) && key.IsBase64().Item1)
            {
                md5hash = DotNetHasher.ComputeMD5String(WebCrypto.EncryptToByteArrayCBC(input1Bytes, key, WebCrypto.AuthIV));
                sha512hash = DotNetHasher.ComputeSHA512String(WebCrypto.EncryptToByteArrayCBC(input2Bytes, key, WebCrypto.AuthIV));
            }
            else
            {
                // Fallback to the older method.
                md5hash = DotNetHasher.ComputeMD5String(input1Bytes);
                sha512hash = DotNetHasher.ComputeSHA512String(input2Bytes);
            }
            
            return (md5hash.Substring(1, 8) + "-" + sha512hash.Substring(2, 4) + "-" + md5hash.Substring(10, 4) + "-" + sha512hash.Substring(16, 4) + "-" + sha512hash.Substring(19, 12)).ToLower();
        }
    }
}
