using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace MultiServer.HTTPService.Addons.PlayStationHome.POTTERMORE
{
    public class POTTERMOREApiAuth
    {
        public static async Task ApiAuthRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            // Extract headers
            string userName = request.Headers["UserName"];
            string password = request.Headers["Password"];
            string apiKey = request.Headers["Api-Key"];

            using (MemoryStream ms = new MemoryStream())
            {
                request.InputStream.CopyTo(ms);

                // Reset the memory stream position to the beginning
                ms.Position = 0;

                // Find the number of bytes in the stream
                int contentLength = (int)ms.Length;

                // Create a byte array
                byte[] buffer = new byte[contentLength];

                // Read the contents of the memory stream into the byte array
                ms.Read(buffer, 0, contentLength);

                buffer = Encoding.UTF8.GetBytes($"{{\r\n\"Token\":\"{GeneratePottermoreAuth(userName + password + apiKey, Encoding.UTF8.GetString(buffer))}\"\r\n}}");

                response.ContentType = "application/json";
                response.StatusCode = 200;
                response.ContentLength64 = buffer.Length;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                }

                ms.Dispose();
            }
        }

        public static async Task ApiUserRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            // Extract headers
            string AccessToken = request.Headers["Access-Token"];

            using (MemoryStream ms = new MemoryStream())
            {
                request.InputStream.CopyTo(ms);

                // Reset the memory stream position to the beginning
                ms.Position = 0;

                // Find the number of bytes in the stream
                int contentLength = (int)ms.Length;

                // Create a byte array
                byte[] buffer = new byte[contentLength];

                // Read the contents of the memory stream into the byte array
                ms.Read(buffer, 0, contentLength);

                buffer = Encoding.UTF8.GetBytes($"{{\r\n\"House\":\"Gryffindor\",\"Wand\":{{\"Core\":1,\"Flexibility\":1,\"Wood\":1,\"State\":1,\"Length\":1.00}}\r\n}}");

                response.ContentType = "application/json";
                response.StatusCode = 200;
                response.ContentLength64 = buffer.Length;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                }

                ms.Dispose();
            }
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
