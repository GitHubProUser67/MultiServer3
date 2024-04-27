using System.Security.Cryptography;
using System.Text;

namespace SVO
{
    public class SVOProcessor
    {
        public static string? CalcuateSVOMac(string? clientSVOMac)
        {
            if (string.IsNullOrEmpty(clientSVOMac) || clientSVOMac.Length != 32)
                return null;

            //Get SVOMac from client and combine with speaksId together for new MD5, converting to a byte array for MD5 rehashing
            byte[] HashedSVOMac = MD5.HashData(Encoding.ASCII.GetBytes($"{clientSVOMac}sp9ck0348sld00000000000000000000"));

            if (HashedSVOMac.Length != 16)
                return null;

            return $"{BitConverter.ToString(HashedSVOMac).Replace("-", string.Empty).ToLower()}";
        }

        // HMAC-MD5 as result is 16 in length as shown in eboot.
        // Known keys - "m4nT15" (profile) - "GHOST_PWD" (ghost replays)
        public static string? CalcuateOTGSecuredHash(string keytohash)
        {
            // Create HMAC-MD5 Algorithm;
            byte[] HashedString = new HMACMD5(Encoding.ASCII.GetBytes("ca91f51f-a7f5-4b95-814a-5796cfff586c")).ComputeHash(Encoding.ASCII.GetBytes(keytohash));

            if (HashedString.Length != 16)
                return null;

            return $"{BitConverter.ToString(HashedString).Replace("-", string.Empty).ToLower()}";
        }
    }
}