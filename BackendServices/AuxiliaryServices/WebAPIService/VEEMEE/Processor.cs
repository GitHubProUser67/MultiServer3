using System;
using CustomLogger;
using Newtonsoft.Json.Linq;
using CastleLibrary.Utils.Hash;

namespace WebAPIService.VEEMEE
{
    public class Processor
    {
        public static string? Sign(string jsonData)
        {
            try
            {
                string formattedJson = JToken.Parse(jsonData.Replace("\n", string.Empty)).ToString(Newtonsoft.Json.Formatting.None);

                string hash = NetHasher.ComputeSHA1StringWithCleanup($"veemeeHTTPRequ9R3UMWDAT8F3*#@&$^{formattedJson}");

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
