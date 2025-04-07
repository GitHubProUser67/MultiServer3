﻿using CustomLogger;
using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using System.Linq;

namespace BackendProject.WebAPIs.OHS
{
    public class UserCounter
    {
        public static string? Set(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
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

            object? key = null;

            if (!string.IsNullOrEmpty(dataforohs))
            {
                JToken Token = JToken.Parse(dataforohs);

                key = MiscUtils.GetValueFromJToken(Token, "key");

                object? value = MiscUtils.GetValueFromJToken(Token, "value");

                object? user = MiscUtils.GetValueFromJToken(Token, "user");

                try
                {
                    string profiledatastring = directorypath + $"/User_Profiles/{user}_Currency.json";

                    if (File.Exists(profiledatastring))
                    {
                        JObject? jObject = JObject.Parse(File.ReadAllText(profiledatastring));

                        if (jObject != null)
                        {
                            // Check if the key name already exists in the JSON
                            JToken? existingKey = jObject.DescendantsAndSelf().FirstOrDefault(t => t.Path == key);

                            if (existingKey != null && value != null)
                                // Update the value of the existing key
                                existingKey.Replace(JToken.FromObject(value));
                            else
                            {
                                JToken? KeyEntry = jObject["key"];

                                if (KeyEntry != null && value != null && key != null)
                                    // Step 2: Add a new entry to the "Key" object
                                    KeyEntry[key] = JToken.FromObject(value);
                            }

                            File.WriteAllText(profiledatastring, jObject.ToString(Formatting.Indented));
                        }
                    }
                    else if (key != null)
                    {
                        string? keystring = key.ToString();

                        if (!string.IsNullOrEmpty(keystring) && user != null && value != null)
                        {
                            // Create a new profile with the key field
                            var newProfile = new OHSUserProfile
                            {
                                user = user.ToString(),
                                key = new JObject { { keystring, JToken.FromObject(value) } }
                            };
                            Directory.CreateDirectory(directorypath + "/User_Profiles");
                            File.WriteAllText(profiledatastring, JsonConvert.SerializeObject(newProfile));
                        }
                    }

                    if (value != null && JToken.FromObject(value).Type == JTokenType.Integer)
                        // Handle integer type
                        output = JToken.FromObject(value).ToString();

                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[UserCounter] - Json Format Error - {ex}");
                }
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(output))
                    return null;
                else
                    return $"{{ [\"{key}\"] = {output} }}";
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [\"{key}\"] = {output} }} }}", game);
            }

            return dataforohs;
        }

        public static string? Increment(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
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

            object? key = null;

            if (!string.IsNullOrEmpty(dataforohs))
            {
                // Deserialize the JSON data into a JObject
                JObject? jObject = JsonConvert.DeserializeObject<JObject>(dataforohs);

                key = jObject.Value<string>("key");

                string? user = jObject.Value<string>("user");

                int value = jObject.Value<int>("value");
                try
                {
                    string profileCurDataString = directorypath + $"User_Profiles/{user}_Currency.json";

                    if (File.Exists(profileCurDataString))
                    {
                        JObject? jObjectFromFile = JObject.Parse(File.ReadAllText(profileCurDataString));

                        if (jObjectFromFile != null)
                        {
                            JToken? existingKey = jObjectFromFile.SelectToken($"$..{key}");

                            if (existingKey != null && existingKey.Type == JTokenType.Integer)
                            {
                                // Increment the value of the existing key (assuming it's an integer)
                                int currentValue = existingKey.Value<int>();
                                int newValue = 0;

                                //CurrentValue is stored file value already, value is the requested change.
                                if(currentValue > value)
                                {
                                    newValue = currentValue - value;
                                } else
                                {
                                    newValue = currentValue + value;
                                }

                                existingKey.Replace(newValue);

                                // Set the output to the incremented value
                                output = (existingKey).ToString();
                            }
                            else
                            {
                                // If the key doesn't exist or its value is not an integer, handle accordingly
                                // For example, you might want to set the value to 1 or handle an error.
                                // Modify this part based on your specific requirements.
                                existingKey = value;
                                 // Set the output to the incremented value
                                output = (value).ToString();
                            }

                            File.WriteAllText(output, jObject.ToString(Formatting.Indented));
                        }
                    }
                    else if (key != null)
                    {
                        string? keystring = key.ToString();

                        if (!string.IsNullOrEmpty(keystring) && user != null)
                        {
                            // Create a new profile with the key field and set it to 1
                            var newProfile = new OHSUserProfile
                            {
                                user = user.ToString(),
                                key = new JObject { { keystring, 1 } }
                            };
                            Directory.CreateDirectory(directorypath + "User_Profiles");
                            File.WriteAllText(profileCurDataString, JsonConvert.SerializeObject(newProfile));

                            // Set the output to incremented value
                            output = (value).ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[UserCounter] - Json Format Error - {ex}");
                }
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
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [\"{key}\"] = {output} }} }}", game);
            }

            return dataforohs;
        }

        public static string? Get_All(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
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

                    if (dataforohs != null && File.Exists(directorypath + $"/User_Profiles/{dataforohs}_Currency.json"))
                    {
                        string tempreader = File.ReadAllText(directorypath + $"User_Profiles/{dataforohs}_Currency.json");

                        if (!string.IsNullOrEmpty(tempreader))
                        {
                            // Parse the JSON string to a JObject
                            jsonObject = JObject.Parse(tempreader);

                            // Check if the "key" property exists and if it is an object
                            if (jsonObject.TryGetValue("key", out JToken? keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                // Convert the JToken to a Lua table-like string
                                output = JaminProcessor.ConvertToLuaTable(keyValueToken, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[UserCounter] - Json Format Error - {ex}");
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

        public static string? Get(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
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
                    JObject jsonObject = JObject.Parse(dataforohs);

                    // Getting the value of the "user" field
                    dataforohs = (string?)jsonObject["user"];

                    if (dataforohs != null && File.Exists(directorypath + $"/User_Profiles/{dataforohs}_Currency.json"))
                    {
                        string currencydata = File.ReadAllText(directorypath + $"/User_Profiles/{dataforohs}_Currency.json");

                        if (!string.IsNullOrEmpty(currencydata))
                        {
                            // Parse the JSON string to a JObject
                            jsonObject = JObject.Parse(currencydata);

                            // Check if the "Key" property exists and if it is an object
                            if (jsonObject.TryGetValue("key", out JToken? keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                // Convert the JToken to a Lua table-like string
                                output = JaminProcessor.ConvertToLuaTable(keyValueToken, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[UserCounter] - Json Format Error - {ex}");
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

        public class OHSUserProfile
        {
            public string? user { get; set; }
            public object? key { get; set; }
        }
    }
}
