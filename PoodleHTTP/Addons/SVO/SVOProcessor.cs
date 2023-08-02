using System.Security.Cryptography;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.SVO
{
    public class SVOProcessor
    {
        public static string CalcuateSVOMac(string clientSVOMac)
        {
            if (string.IsNullOrEmpty(clientSVOMac))
                return null;


            if (clientSVOMac.Length != 32)
                return null;

            //SVO SpeaksId internal SCE-RT
            string speaksId = Misc.Base64Decode("c3A5Y2swMzQ4c2xkMDAwMDAwMDAwMDAwMDAwMDAw");

            //Get SVOMac from client and combine with speaksId together for new MD5, converting to a byte array for MD5 rehashing
            byte[] HashedSVOMac = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(clientSVOMac + speaksId));

            if (HashedSVOMac.Length != 16)
                return null;

            // Create the Cipher RSA_RC4_40_MD5 value by concatenating the encoded key and the MD5 hash
            string cipher = $"{BitConverter.ToString(HashedSVOMac).Replace("-", string.Empty).ToLower()}";

            return cipher;
        }
    }
}
