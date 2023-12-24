using System.Security.Cryptography;
using System.Text;

namespace BackendProject.WebAPIs.SSFW
{
    public class GuidGenerator
    {
        public static string SSFWGenerateGuid(string input1, string input2)
        {
            string md5hash = "";
            string sha512hash = "";

            using (MD5 md5 = MD5.Create())
            {
                string salt = "**H0mEIsG3reAT!!!!!!!!!!!!!!";

                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input1 + salt));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Clear();
            }

            using (SHA512 sha512 = SHA512.Create())
            {
                string salt = "C0MeBaCKHOm3*!*!*!*!*!*!*!*!";

                byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(salt + input2));
                sha512hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                sha512.Clear();
            }

            return (md5hash.Substring(1, 8) + "-" + sha512hash.Substring(2, 4) + "-" + md5hash.Substring(10, 4) + "-" + sha512hash.Substring(16, 4) + "-" + sha512hash.Substring(19, 12)).ToLower();
        }
    }
}
