using CastleLibrary.Utils.Hash;
using System.Text;

namespace WebAPIService.SSFW
{
    public class GuidGenerator
    {
        public static string SSFWGenerateGuid(string input1, string input2)
        {
            string md5hash = NetHasher.ComputeMD5StringWithCleanup(Encoding.UTF8.GetBytes(input1 + "**H0mEIsG3reAT!!!!!!!!!!!!!!"));
            string sha512hash = NetHasher.ComputeSHA512StringWithCleanup(Encoding.UTF8.GetBytes("C0MeBaCKHOm3*!*!*!*!*!*!*!*!" + input2));
            return (md5hash.Substring(1, 8) + "-" + sha512hash.Substring(2, 4) + "-" + md5hash.Substring(10, 4) + "-" + sha512hash.Substring(16, 4) + "-" + sha512hash.Substring(19, 12)).ToLower();
        }
    }
}
