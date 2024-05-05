using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using CustomLogger;
using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using CyberBackendLibrary.HTTP;
using System.Threading.Tasks;
using System.Text;

namespace WebAPIService.OHS
{
    public class UserCounter
    {
        public static string? Set(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
        {
            string? dataforohs = null;
            string? output = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
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

                object? user = Utils.JtokenUtils.GetValueFromJToken(Token, "user");

                object? value = Utils.JtokenUtils.GetValueFromJToken(Token, "value");

                key = Utils.JtokenUtils.GetValueFromJToken(Token, "key");

                if (value == null && key == null) // Special object (seen in sodium 2)
                {
                    KeyValuePair<object, object> firstKeyValuePair = ExtractKeyValues(Token.ToString(), "data").FirstOrDefault(); // Maybe there can be more?

                    key = firstKeyValuePair.Key;

                    value = firstKeyValuePair.Value;
                }

                Directory.CreateDirectory(directorypath + $"/User_Profiles");

                try
                {
                    string profiledatastring = directorypath + $"/User_Profiles/{user}_Stats.json";

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

                            File.WriteAllText(profiledatastring, JsonConvert.SerializeObject(newProfile));
                        }
                    }

                    if (value != null)
                        output = JaminProcessor.JsonValueToLuaValue(JToken.FromObject(value));
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

        public static string? Increment(byte[] PostData, string ContentType, string directorypath, string batchparams, int game, bool v2)
        {
            string? dataforohs = null;
            string? output = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
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

                if (jObject != null)
                {
                    key = jObject.Value<string>("key");

                    string? user = jObject.Value<string>("user");

                    int value = jObject.Value<int>("value");

                    try
                    {
                        string profileCurDataString = directorypath + $"User_Profiles/{user}_Stats.json";

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

                                    // CurrentValue is stored file value already, value is the requested change.
                                    if (currentValue > value)
                                        newValue = currentValue - value;
                                    else
                                        newValue = currentValue + value;

                                    existingKey.Replace(newValue);

                                    // Set the output to the incremented value
                                    output = existingKey.ToString();
                                }
                                else
                                {
                                    existingKey = value;
                                    output = existingKey.ToString();
                                }

                                File.WriteAllText(profileCurDataString, jObjectFromFile.ToString(Formatting.Indented));
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
                                    user = user,
                                    key = new JObject { { keystring, value < 0 ? 0 : value } }
                                };

                                Directory.CreateDirectory(directorypath + $"/User_Profiles");

                                File.WriteAllText(profileCurDataString, JsonConvert.SerializeObject(newProfile));

                                // Set the output to incremented value
                                output = value.ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[UserCounter] - Json Format Error - {ex}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(output))
                    return "{ }";
                else
                    return v2 ? $"{{ [\"sessionCount\"] = {output} }}" : output;
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {(v2 ? $"{{ [\"sessionCount\"] = {output} }}" : output)} }}", game);
            }

            return dataforohs;
        }

        public static string? Get_All(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
        {
            string? dataforohs = null;
            string? output = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
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

                    if (dataforohs != null && File.Exists(directorypath + $"/User_Profiles/{dataforohs}_Stats.json"))
                    {
                        string tempreader = File.ReadAllText(directorypath + $"User_Profiles/{dataforohs}_Stats.json");

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
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}", game);
            }

            return dataforohs;
        }

        public static string? Get_Many(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
        {
            string? dataforohs = null;
            string? output = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
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
                    string[]? keys = jsonObject["keys"]?.ToObject<string[]>();

                    if (dataforohs != null && File.Exists(directorypath + $"/User_Profiles/{dataforohs}_Stats.json"))
                    {
                        string tempreader = File.ReadAllText(directorypath + $"User_Profiles/{dataforohs}_Stats.json");

                        if (!string.IsNullOrEmpty(tempreader))
                        {
                            // Parse the JSON string to a JObject
                            jsonObject = JObject.Parse(tempreader);

                            if (keys != null)
                            {
                                foreach (string key in keys)
                                {
                                    // Check if the "key" property exists
                                    if (jsonObject.TryGetValue(key, out JToken? keyValueToken))
                                        // Convert the JToken to a Lua table-like string
                                        output = JaminProcessor.ConvertJTokenToLuaTable(keyValueToken, false);
                                }
                            }
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
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
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
                string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
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

                    if (dataforohs != null && File.Exists(directorypath + $"/User_Profiles/{dataforohs}_Stats.json"))
                    {
                        string currencydata = File.ReadAllText(directorypath + $"/User_Profiles/{dataforohs}_Stats.json");

                        if (!string.IsNullOrEmpty(currencydata))
                        {
                            // Parse the JSON string to a JObject
                            jsonObject = JObject.Parse(currencydata);

                            // Check if the "Key" property exists and if it is an object
                            if (jsonObject.TryGetValue("key", out JToken? keyValueToken) && keyValueToken.Type == JTokenType.Object)
                                // Convert the JToken to a Lua table-like string
                                output = JaminProcessor.ConvertJTokenToLuaTable(keyValueToken, false);
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
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}", game);
            }

            return dataforohs;
        }

        public static string? Increment_Many(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
        {
            string? dataforohs = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

                if (!string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
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

            Dictionary<string, string> IncrementResults = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(dataforohs))
            {
                // Deserialize the JSON data into a JObject
                JObject? jObject = JsonConvert.DeserializeObject<JObject>(dataforohs);

                if (jObject != null)
                {
                    try
                    {
                        // Getting the value of the "user" field
                        dataforohs = (string?)jObject["user"];
                        string[]? keys = jObject["keys"]?.ToObject<string[]>();
                        string[]? projects = jObject["projects"]?.ToObject<string[]>();
                        int[]? values = jObject["values"]?.ToObject<int[]>();

                        if (!string.IsNullOrEmpty(dataforohs) && keys != null && projects != null && values != null && keys.Length == projects.Length && projects.Length == values.Length)
                        {
                            string profileCurDataString = string.Empty;

                            int i = 0;

                            foreach (string project in projects)
                            {
                                profileCurDataString = directorypath + $"/{project}/User_Profiles/{dataforohs}_Stats.json";

                                if (File.Exists(profileCurDataString))
                                {
                                    JObject? jObjectFromFile = JObject.Parse(File.ReadAllText(profileCurDataString));

                                    if (jObjectFromFile != null)
                                    {
                                        JToken? existingKey = jObjectFromFile.SelectToken($"$..{keys[i]}");

                                        if (existingKey != null && existingKey.Type == JTokenType.Integer)
                                        {
                                            // Increment the value of the existing key (assuming it's an integer)
                                            int currentValue = existingKey.Value<int>();
                                            int newValue = 0;

                                            // CurrentValue is stored file value already, value is the requested change.
                                            if (currentValue > values[i])
                                                newValue = currentValue - values[i];
                                            else
                                                newValue = currentValue + values[i];

                                            existingKey.Replace(newValue);

                                            // Set the output to the incremented value
                                            IncrementResults.Add(keys[i], existingKey.ToString());
                                        }
                                        else
                                        {
                                            existingKey = values[i];
                                            IncrementResults.Add(keys[i], values[i].ToString());
                                        }

                                        File.WriteAllText(profileCurDataString, jObjectFromFile.ToString(Formatting.Indented));
                                    }
                                }
                                else if (keys[i] != null)
                                {
                                    string? keystring = keys[i];

                                    if (!string.IsNullOrEmpty(keystring) && !string.IsNullOrEmpty(dataforohs))
                                    {
                                        // Create a new profile with the key field and set it to 1
                                        var newProfile = new OHSUserProfile
                                        {
                                            user = dataforohs,
                                            key = new JObject { { keystring, values[i] < 0 ? 0 : values[i] } }
                                        };

                                        Directory.CreateDirectory(directorypath + $"/{project}/User_Profiles/");

                                        File.WriteAllText(profileCurDataString, JsonConvert.SerializeObject(newProfile));

                                        // Set the output to incremented value
                                        IncrementResults.Add(keys[i], values[i].ToString());
                                    }
                                }

                                i++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogError($"[UserCounter] - Json Format Error - {ex}");
                    }
                }
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (IncrementResults.Count <= 0)
                    return "{ }";
                else
                {
                    StringBuilder sb = new StringBuilder();

                    int i = 1;

                    foreach (var item in IncrementResults)
                    {
                        if (sb.Length != 0)
                        {
                            sb.Append($", [{i}] = {{ [\"value\"] = {item.Value} }}");
                        }
                        else
                            sb.Append($"{{ [{i}] = {{ [\"value\"] = {item.Value} }}");

                        i++;
                    }

                    if (sb.Length != 0)
                        sb.Append(" }");
                    else
                        sb.Append("{ }");

                    return sb.ToString();
                }
            }
            else
            {
                if (IncrementResults.Count <= 0)
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                {
                    StringBuilder sb = new StringBuilder();

                    int i = 1;

                    foreach (var item in IncrementResults)
                    {
                        if (sb.Length != 0)
                        {
                            sb.Append($", [{i}] = {{ [\"value\"] = {item.Value} }}");
                        }
                        else
                            sb.Append($"{{ [{i}] = {{ [\"value\"] = {item.Value} }}");

                        i++;
                    }

                    if (sb.Length != 0)
                        sb.Append(" }");
                    else
                        sb.Append("{ }");

                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {sb} }}", game);
                }
            }

            return dataforohs;
        }

        private static Dictionary<object, object> ExtractKeyValues(string jsonString, string nameProperty)
        {
            var jsonObject = JObject.Parse(jsonString);
            var result = new Dictionary<object, object>();

            if (jsonObject.TryGetValue(nameProperty, out var dataToken) && dataToken is JObject dataObject)
            {
                foreach (var property in dataObject.Properties())
                {
                    result.Add(property.Name, property.Value);
                }
            }

            return result;
        }

        public class OHSUserProfile
        {
            public string? user { get; set; }
            public object? key { get; set; }
        }
    }
}
