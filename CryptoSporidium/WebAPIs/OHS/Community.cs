using CustomLogger;
using Newtonsoft.Json.Linq;
using HttpMultipartParser;
using Newtonsoft.Json;

namespace CryptoSporidium.WebAPIs.OHS
{
    public class Community
    {
        public static string? Community_Getscore(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
        {
            string? dataforohs = null;
            int output = 0;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);

                if (boundary != null)
                {
                    using (MemoryStream ms = new(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    JToken? Token = JToken.Parse(dataforohs);

                    if (Token != null)
                    {
                        object? key = MiscUtils.GetValueFromJToken(Token, "key");

                        if (key != null)
                        {
                            if (File.Exists(directorypath + $"/Community_Profiles/{key}.json"))
                            {
                                string tempreader = File.ReadAllText(directorypath + $"/Community_Profiles/{key}.json");

                                JObject? jObject = JObject.Parse(tempreader);

                                if (jObject != null)
                                {
                                    // Check if the key name already exists in the JSON
                                    JToken? existingKey = jObject.SelectToken($"$..{key}");

                                    if (existingKey != null)
                                        // Get the value of the existing key
                                        output = existingKey.Value<int>();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Community] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
                return "{ [\"score\"] = " + output.ToString() + " }";
            else
                dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [\"score\"] = {output} }} }}", game);

            return dataforohs;
        }

        public static string? Community_UpdateScore(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
        {
            string? dataforohs = null;
            int output = 0;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);

                if (boundary != null)
                {
                    using (MemoryStream ms = new(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    JToken? Token = JToken.Parse(dataforohs);

                    if (Token != null)
                    {
                        object? key = MiscUtils.GetValueFromJToken(Token, "key");

                        int inc = Convert.ToInt32(MiscUtils.GetValueFromJToken(Token, "inc"));

                        if (File.Exists(directorypath + $"/Community_Profiles/{key}.json") && key != null)
                        {
                            string tempreader = File.ReadAllText(directorypath + $"/Community_Profiles/{key}.json");

                            if (!string.IsNullOrEmpty(tempreader))
                            {
                                JObject? jObject = JObject.Parse(tempreader);

                                if (jObject != null)
                                {
                                    // Check if the key name already exists in the JSON
                                    JToken? existingKey = jObject.SelectToken($"$..{key}");

                                    if (existingKey != null)
                                        // Get the value of the existing key
                                        existingKey[key] = existingKey.Value<int>() + inc;
                                    else if (key != null)
                                    {
                                        string? keyname = key.ToString();

                                        if (keyname != null)
                                            // If the key doesn't exist, add it with the increment value
                                            jObject.Add(new JProperty(keyname, inc));
                                    }
                                }

                                File.WriteAllText(directorypath + $"/Community_Profiles/{key}.json", JsonConvert.SerializeObject(jObject, Formatting.Indented));
                            }
                        }
                        else if (key != null)
                        {
                            Directory.CreateDirectory(directorypath + "/Community_Profiles");
                            File.WriteAllText(directorypath + $"/Community_Profiles/{key}.json", $"{{ \"{key}\":{inc} }}");
                            output = inc;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Community] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
                return "{ [\"score\"] = " + output.ToString() + " }";
            else
                dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [\"score\"] = {output} }} }}", game);

            return dataforohs;
        }
    }
}
