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
            byte[] HashedSVOMac = MD5.HashData(Encoding.ASCII.GetBytes(clientSVOMac + "sp9ck0348sld00000000000000000000"));

            if (HashedSVOMac.Length != 16)
                return null;

            return $"{BitConverter.ToString(HashedSVOMac).Replace("-", string.Empty).ToLower()}";
        }
    }
}