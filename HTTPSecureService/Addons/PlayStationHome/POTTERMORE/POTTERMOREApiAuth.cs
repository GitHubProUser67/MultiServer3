using System.Net;
using System.Security.Cryptography;
using System.Text;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.POTTERMORE
{
    public class POTTERMOREApiAuth
    {
        public static void ApiAuthRequest(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            // Extract headers
            string userName = HTTPSClass.GetHeaderValue(Headers, "UserName");
            string password = HTTPSClass.GetHeaderValue(Headers, "Password");
            string apiKey = HTTPSClass.GetHeaderValue(Headers, "Api-Key");

            // Create a byte array
            byte[] buffer = request.BodyBytes;

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetContentType("application/json");
            response.SetBody($"{{\r\n\"Token\":\"{GeneratePottermoreAuth(userName + password + apiKey, Encoding.UTF8.GetString(buffer))}\"\r\n}}");
        }

        public static void ApiUserRequest(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            // Extract headers
            string AccessToken = HTTPSClass.GetHeaderValue(Headers, "Access-Token");

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetContentType("application/json");
            response.SetBody($"{{\r\n\"House\":\"Gryffindor\",\"Wand\":{{\"Core\":1,\"Flexibility\":1,\"Wood\":1,\"State\":1,\"Length\":1.00}}\r\n}}");
        }

        public static string GeneratePottermoreAuth(string input1, string input2)
        {
            string md5hash = "";
            string sha512hash = "";

            using (MD5 md5 = MD5.Create())
            {
                string salt = "Il0v3HarryPotter......!!!!!!!!!";

                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input1 + salt));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Dispose();
            }

            using (SHA512 sha512 = SHA512.Create())
            {
                string salt = "H3rM1oneGrANGERL1Ke1nthePS1Game!";

                byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(salt + input2));
                sha512hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                sha512.Dispose();
            }

            return (sha512hash + md5hash).ToLower();
        }
    }
}
