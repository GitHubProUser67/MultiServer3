using CustomLogger;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace HTTPSecureServerLite.API.VEEMEE
{
    public class Processor
    {
        private static string Sha1Hash(string data)
        {
            byte[]? hashBytes = null;

            using (var sha1 = SHA1.Create())
            {
                hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes($"veemeeHTTPRequ9R3UMWDAT8F3*#@&$^{data}"));

                sha1.Clear();
            }

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        public static string? sign(string jsonData)
        {
            try
            {
                string formattedJson = JToken.Parse(jsonData.Replace("\n", "")).ToString(Newtonsoft.Json.Formatting.None);

                string hash = Sha1Hash(formattedJson).ToUpper();

                JToken token = JToken.Parse(formattedJson);

                if (token.Type == JTokenType.Object)
                {
                    JObject obj = (JObject)token;
                    obj["hash"] = hash;
                    formattedJson = obj.ToString(Newtonsoft.Json.Formatting.None);
                }
                else if (token.Type == JTokenType.Array)
                {
                    JArray array = (JArray)token;
                    JObject obj = new();
                    obj["hash"] = hash;
                    array.Add(obj);
                    formattedJson = array.ToString(Newtonsoft.Json.Formatting.None);
                }

                return formattedJson;
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[VEEMEE] : Exception in sign, file might be incorrect : {ex}");
            }

            return null;
        }
    }
}
