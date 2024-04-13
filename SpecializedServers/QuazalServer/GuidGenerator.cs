using System.Security.Cryptography;
using System.Text;

namespace QuazalServer
{
    public class GuidGenerator
    {
        public static string UBISOFTGenerateGuid(string input1, string input2)
        {
            string md5hash = string.Empty;
            string sha512hash = string.Empty;

            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input1 + "!*JAd3!!!!!!!!!!*!!!"));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Clear();
            }

            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes("*!*!*!*!*!*!*!*!Qn3TZZZ" + input2));
                sha512hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                sha512.Clear();
            }

            return (md5hash.Substring(1, 8) + "-" + sha512hash.Substring(2, 4) + "-" + md5hash.Substring(10, 4) + "-" + sha512hash.Substring(16, 4) + "-" + sha512hash.Substring(19, 12)).ToLower();
        }
    }
}
