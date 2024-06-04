using System;
using CustomLogger;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace WebAPIService.VEEMEE
{
    public class Processor
    {
        private static string Sha1Hash(string data)
        {
            byte[]? SHA1Data = null;
            using (var sha1 = SHA1.Create())
            {
                SHA1Data = sha1.ComputeHash(Encoding.UTF8.GetBytes($"veemeeHTTPRequ9R3UMWDAT8F3*#@&$^{data}"));
            }
            return BitConverter.ToString(SHA1Data).Replace("-", string.Empty).ToLower();
        }

        public static string? Sign(string jsonData)
        {
            try
            {
                string formattedJson = JToken.Parse(jsonData.Replace("\n", string.Empty)).ToString(Newtonsoft.Json.Formatting.None);

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
                    JObject obj = new JObject
                    {
                        ["hash"] = hash
                    };
                    array.Add(obj);
                    formattedJson = array.ToString(Newtonsoft.Json.Formatting.None);
                }

                return formattedJson;
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[VEEMEE] : Exception in Sign, file might be incorrect : {ex}");
            }

            return null;
        }
    }
}
