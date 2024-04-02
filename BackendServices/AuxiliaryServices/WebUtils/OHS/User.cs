using CustomLogger;
using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static WebUtils.OHS.UserCounter;
using System.Text;
using System.Security.Cryptography;
using BackendProject.MiscUtils;

namespace WebUtils.OHS
{
    public class User
    {
        public static string? Set(byte[] PostData, string ContentType, string directorypath, string batchparams, bool global, int game)
        {
            string? dataforohs = null;
            string? output = null;

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
                    JToken Token = JToken.Parse(dataforohs);

                    object? value = VariousUtils.GetValueFromJToken(Token, "value");

                    object? key = VariousUtils.GetValueFromJToken(Token, "key");

                    object? user = VariousUtils.GetValueFromJToken(Token, "user");

                    if (!global)
                    {
                        string profiledatastring = directorypath + $"/User_Profiles/{user}.json";

                        if (File.Exists(profiledatastring))
                        {
                            string profiledata = File.ReadAllText(profiledatastring);

                            if (!string.IsNullOrEmpty(profiledata))
                            {
                                JObject? jObject = JObject.Parse(profiledata);

                                if (jObject != null)
                                {
                                    // Check if the key name already exists in the JSON
                                    JToken? existingKey = jObject.DescendantsAndSelf().FirstOrDefault(t => t.Path == key);

                                    if (existingKey != null && value != null)
                                        // Update the value of the existing key
                                        existingKey.Replace(JToken.FromObject(value));
                                    else if (key != null && value != null)
                                    {
                                        JToken? KeyEntry = jObject["key"];

                                        if (KeyEntry != null)
                                            // Step 2: Add a new entry to the "Key" object
                                            KeyEntry[key] = JToken.FromObject(value);
                                    }

                                    File.WriteAllText(profiledatastring, jObject.ToString(Formatting.Indented));
                                }
                            }
                        }
                        else if (key != null)
                        {
                            string? keystring = key.ToString();

                            if (keystring != null && user != null && value != null)
                            {
                                // Create a new profile with the key field
                                OHSUserProfile newProfile = new OHSUserProfile
                                {
                                    user = user.ToString(),
                                    key = new JObject { { keystring, JToken.FromObject(value) } }
                                };
                                Directory.CreateDirectory(directorypath + "/User_Profiles");
                                File.WriteAllText(profiledatastring, JsonConvert.SerializeObject(newProfile));
                            }
                        }
                    }
                    else
                    {
                        string globaldatastring = directorypath + "/Global.json";

                        if (File.Exists(globaldatastring))
                        {
                            string globaldata = File.ReadAllText(globaldatastring);

                            if (!string.IsNullOrEmpty(globaldata))
                            {
                                JObject? jObject = JObject.Parse(globaldata);

                                if (jObject != null && value != null)
                                {
                                    // Check if the key name already exists in the JSON
                                    JToken? existingKey = jObject.DescendantsAndSelf().FirstOrDefault(t => t.Path == key);

                                    if (existingKey != null)
                                        // Update the value of the existing key
                                        existingKey.Replace(JToken.FromObject(value));
                                    else if (key != null)
                                    {
                                        JToken? KeyEntry = jObject["key"];

                                        if (KeyEntry != null)
                                            // Step 2: Add a new entry to the "Key" object
                                            KeyEntry[key] = JToken.FromObject(value);
                                    }

                                    File.WriteAllText(globaldatastring, jObject.ToString(Formatting.Indented));
                                }
                            }
                        }
                        else if (key != null)
                        {
                            string? keystring = key.ToString();

                            if (keystring != null && value != null)
                            {
                                // Create a new profile with the key field
                                OHSGlobalProfile newProfile = new OHSGlobalProfile
                                {
                                    Key = new JObject { { keystring, JToken.FromObject(value) } }
                                };

                                File.WriteAllText(globaldatastring, JsonConvert.SerializeObject(newProfile));
                            }
                        }
                    }

                    if (value != null)
                        output = JaminProcessor.JsonValueToLuaValue(JToken.FromObject(value));
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(output))
                    return null;
                else
                    return output;
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}", game);
            }

            return dataforohs;
        }

        public static string? Get_All(byte[] PostData, string ContentType, string directorypath, string batchparams, bool global, int game)
        {
            string? dataforohs = string.Empty;
            string? output = string.Empty;
            string? projectName = string.Empty;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);

                if (boundary != null)
                {
                    using (MemoryStream ms = new(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        projectName = data.GetParameterValue("project");
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
                    // Parsing the JSON string
                    JObject? jsonObject = JObject.Parse(dataforohs);
					
                    if (!global)
                    {
                        // Getting the value of the "user" field
                        dataforohs = (string?)jsonObject["user"];

                        if (dataforohs != null && File.Exists(directorypath + $"/User_Profiles/{dataforohs}.json"))
                        {
                            string tempreader = File.ReadAllText(directorypath + $"/User_Profiles/{dataforohs}.json");

                            if (!string.IsNullOrEmpty(tempreader))
                            {
                                // Parse the JSON string to a JObject
                                jsonObject = JObject.Parse(tempreader);

                                // Check if the "key" property exists and if it is an object
                                if (jsonObject.TryGetValue("key", out JToken? keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                    // Convert the JToken to a Lua table-like string
                                    output = JaminProcessor.ConvertJTokenToLuaTable(keyValueToken, false);
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(directorypath + $"/Global.json"))
                        {
                            string tempreader = File.ReadAllText(directorypath + $"/Global.json");

                            if (!string.IsNullOrEmpty(tempreader))
                            {
                                // Parse the JSON string to a JObject
                                jsonObject = JObject.Parse(tempreader);	

                                // Check if the "key" property exists and if it is an object
                                if (jsonObject.TryGetValue("key", out JToken? keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                    // Convert the JToken to a Lua table-like string
                                    output = JaminProcessor.ConvertJTokenToLuaTable(keyValueToken, false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(output))
                    return "{ }";
                else
                    return output;
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}", game);
            }

            return dataforohs;
        }

        public static string? Get(byte[] PostData, string ContentType, string directorypath, string batchparams, bool global, int game)
        {
            string? dataforohs = null;
            string? output = null;

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
                    // Parsing the JSON string
                    JObject? jsonObject = JObject.Parse(dataforohs);
                        
                    if (!global)
                    {
                        // Getting the value of the "user" field
                        string? ohsUserName = (string?)jsonObject["user"];
                        string? ohsKey = (string?)jsonObject["key"];

                        if (dataforohs != null && File.Exists(directorypath + $"/User_Profiles/{ohsUserName}.json"))
                        {
                            string userprofile = File.ReadAllText(directorypath + $"/User_Profiles/{ohsUserName}.json");

                            if (!string.IsNullOrEmpty(userprofile))
                            {
                                // Parse the JSON string to a JObject
                                jsonObject = JObject.Parse(userprofile);

                                // Check if the "key" property exists and if it is an object
                                if (jsonObject.TryGetValue("key", out JToken? keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                {
                                    if (keyValueToken.ToObject<JObject>().TryGetValue(ohsKey, out JToken? ohsKeyValue))
                                        // Convert the JToken to a Lua table-like string
                                        output = JaminProcessor.ConvertJTokenToLuaTable(ohsKeyValue, false);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(directorypath + $"/Global.json"))
                        {
                            string globaldata = File.ReadAllText(directorypath + $"/Global.json");

                            if (!string.IsNullOrEmpty(globaldata))
                            {
                                // Parse the JSON string to a JObject
                                jsonObject = JObject.Parse(globaldata);

                                // Check if the "key" property exists and if it is an object
                                if (jsonObject.TryGetValue("key", out JToken? keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                    // Convert the JToken to a Lua table-like string
                                    output = JaminProcessor.ConvertJTokenToLuaTable(keyValueToken, false);
                            }
                        }
                        else if ((string?)jsonObject["key"] == "global_data" && directorypath.Contains("Uncharted3"))
                            output = "{[\"unlocks\"] = \"WAVE3\",[\"community_score\"] = 1,[\"challenges\"] = {[\"accuracy\"] = 1}}";
                        else if ((string?)jsonObject["key"] == "unlock_data" && directorypath.Contains("killzone_3"))
                            output = "{ [\"wave_1\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = True }, [\"wave_2\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = True }, [\"wave_3\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = True } }, { [\"wave_1\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = True }, [\"wave_2\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = True }, [\"wave_3\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = True } }, { [\"wave_1\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = True }, [\"wave_2\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = True }, [\"wave_3\"] = { [\"unlocked\"] = \"1999:10:10\", [\"override\"] = True } } } }";
                        else if ((string?)jsonObject["key"] == "global_data" && directorypath.Contains("Halloween2012"))
                            output = "{ [\"unlocks\"] = { [\"dance\"] = { [\"open\"] = \"20230926113000\", [\"closed\"] = \"20990926163000\" }, [\"limbo\"] = { [\"open\"] = \"20230926113000\", [\"closed\"] = \"20990926163000\" }, [\"hemlock\"] = { [\"open\"] = \"20230926113000\", [\"closed\"] = \"20990926163000\" }, [\"wolfsbane\"] = { [\"open\"] = \"20230926113000\", [\"closed\"] = \"20990926163000\" } } }";
                        else if ((string?)jsonObject["key"] == "vickie_version")
                            output = "{[\"vickie_version\"] = 7}";
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(output))
                    return "{ }";
                else
                    return output;
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}", game);
            }

            return dataforohs;
        }

        public static string? Gets(byte[] PostData, string ContentType, string directorypath, string batchparams, bool global, int game)
        {
            string? dataforohs = null;
            string? output = null;

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
                    // Parsing the JSON string
                    JObject? jsonObject = JObject.Parse(dataforohs);

                    // Getting the value of the "user" field
                    dataforohs = (string?)jsonObject["user"];
                    string[] keys = jsonObject["keys"].ToObject<string[]>();

                    if (!global)
                    {

                        if (dataforohs != null && File.Exists(directorypath + $"/User_Profiles/{dataforohs}.json"))
                        {
                            string userprofile = File.ReadAllText(directorypath + $"/User_Profiles/{dataforohs}.json");

                            if (!string.IsNullOrEmpty(userprofile))
                            {
                                // Parse the JSON string to a JObject
                                jsonObject = JObject.Parse(userprofile);

                                foreach (var key in keys)
                                {
                                    // Check if the "key" property exists and if it is an object
                                    if (jsonObject.TryGetValue(key, out JToken? keyValueToken))
                                        // Convert the JToken to a Lua table-like string
                                        output = JaminProcessor.ConvertJTokenToLuaTable(keyValueToken, false);
                                }

                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(directorypath + $"/Global.json"))
                        {
                            string globaldata = File.ReadAllText(directorypath + $"/Global.json");

                            if (!string.IsNullOrEmpty(globaldata))
                            {
                                // Parse the JSON string to a JObject
                                jsonObject = JObject.Parse(globaldata);

                                string response = ConcatenateValues(jsonObject, keys);
                                output = response;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(output))
                    return "{ }";
                else
                    return output;
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ }} }}", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}", game);
            }

            return dataforohs;
        }

        public static string? User_Id(byte[] PostData, string ContentType, string batchparams, int game)
        {
            string? dataforohs = null;

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
                    // Parsing the JSON string
                    JObject jsonObject = JObject.Parse(dataforohs);

                    // Getting the value of the "user" field
                    dataforohs = (string?)jsonObject["user"];
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(dataforohs))
                    return null;
                else
                    return UniqueNumberGenerator.GenerateUniqueNumber(dataforohs).ToString();
            }
            else
            {
                if (string.IsNullOrEmpty(dataforohs))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {UniqueNumberGenerator.GenerateUniqueNumber(dataforohs)} }}", game);
            }

            return dataforohs;
        }

        public static string? User_GetWritekey(byte[] PostData, string ContentType, string batchparams, int game)
        {
            string? dataforohs = null;

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
                    // Parsing the JSON string
                    JObject jsonObject = JObject.Parse(dataforohs);

                    dataforohs = GetFirstEightCharacters(CalculateMD5HashToExadecimal((string?)jsonObject["user"]));
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[User] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(dataforohs))
                    return null;
                else
                    return "{ [\"writeKey\"] = \"" + dataforohs + "\" }";
            }
            else
            {
                if (string.IsNullOrEmpty(dataforohs))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [\"writeKey\"] = \"{dataforohs}\" }} }}", game);
            }

            return dataforohs;
        }

        public static string? CalculateMD5HashToExadecimal(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public static string? GetFirstEightCharacters(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            if (input.Length >= 8)
                return input.Substring(0, 8);

            else
                // If the input is less than 8 characters, you can handle it accordingly
                // For simplicity, let's just pad with zeros in this case
                return input.PadRight(8, '0');
        }

        public class OHSGlobalProfile
        {
            public object? Key { get; set; }
        }

        public static class UniqueNumberGenerator
        {
            // Function to generate a unique number based on a string using MD5
            public static int GenerateUniqueNumber(string inputString)
            {
                using (MD5 md5Hash = MD5.Create())
                {
                    byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes("0HS0000000000000A" + inputString));

                    // To get a small integer within Lua int bounds, take the least significant 16 bits of the hash and convert to int16
                    int uniqueNumber = Math.Abs(BitConverter.ToUInt16(data, 0));

                    return uniqueNumber;
                }
            }
        }

        static string ConcatenateValues(JObject jsonObject, string[] keysRequested)
        {
            string response = "";
            foreach (string key in keysRequested)
            {
                if (jsonObject.ContainsKey(key))
                {
                    // Check if the "key" property exists and if it is an object
                    if (jsonObject.TryGetValue("keys", out JToken? keyValueToken))
                        response += JaminProcessor.ConvertJTokenToLuaTable(jsonObject[key].ToString(), false);
                }
                else
                {
                    // Don't write response '//response += "Key '" + key + "' not found ";
                }
            }
            return response.Trim();
        }
    }

}
