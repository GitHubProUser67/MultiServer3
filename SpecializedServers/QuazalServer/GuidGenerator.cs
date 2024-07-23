using CastleLibrary.Utils.Hash;
using System.Security.Cryptography;
using System.Text;

namespace QuazalServer
{
    public class GuidGenerator
    {
        public static string UBISOFTGenerateGuid(string input1, string input2)
        {
            string md5hash = NetHasher.ComputeMD5StringWithCleanup(input1 + "!*JAd3!!!!!!!!!!*!!!");
            string sha512hash = NetHasher.ComputeSHA512StringWithCleanup("*!*!*!*!*!*!*!*!Qn3TZZZ" + input2);
            return (md5hash.Substring(1, 8) + "-" + sha512hash.Substring(2, 4) + "-" + md5hash.Substring(10, 4) + "-" + sha512hash.Substring(16, 4) + "-" + sha512hash.Substring(19, 12)).ToLower();
        }
    }
}
