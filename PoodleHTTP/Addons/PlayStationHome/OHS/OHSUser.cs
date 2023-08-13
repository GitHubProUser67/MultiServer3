using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.OHS
{
    public class OHSUser
    {
        public static async Task<string> set(string directorypath, string batchparams, bool global, HttpListenerRequest request, HttpListenerResponse response)
        {
            string dataforohs = "";

            string output = "";

            if (batchparams == "")
            {
                var data = MultipartFormDataParser.Parse(request.InputStream, Extensions.ExtractBoundary(request.ContentType));

                ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                dataforohs = data.GetParameterValue("data");

                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_ENCRYPTEDJAMINVALUE_HERE", dataforohs.Substring(8)));

                    if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                    {
                        dataforohs = returnValues[0]?.ToString();
                    }
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
            {
                dataforohs = batchparams;
            }

            JToken keyToken = JToken.Parse(dataforohs);

            // Deserialize the JSON data into a JObject
            JObject jObject = JsonConvert.DeserializeObject<JObject>(dataforohs);

            object value = OHSProcessor.GetValueFromJToken(keyToken, "value");

            keyToken = jObject.GetValue("key");

            string keyName = keyToken.Value<string>();

            if (!global)
            {
                string user = jObject.Value<string>("user");

                string profiledatastring = directorypath + $"/User_Profiles/{user}.json";

                if (File.Exists(profiledatastring))
                {
                    jObject = JObject.Parse(Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(profiledatastring, HTTPPrivateKey.HTTPPrivatekey)));

                    // Check if the key name already exists in the JSON
                    JToken existingKey = jObject.SelectToken($"$..{keyName}");

                    if (existingKey != null)
                    {
                        // Update the value of the existing key
                        existingKey.Replace(JToken.FromObject(value));
                    }
                    else
                    {
                        // Step 2: Add a new entry to the "Key" object
                        jObject["Key"][keyName] = JToken.FromObject(value);
                    }

                    await FileHelper.CryptoWriteAsync(profiledatastring, HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(jObject.ToString(Formatting.None)));
                }
                else
                {
                    // Create a new profile with the key field
                    OHSUserProfile newProfile = new OHSUserProfile
                    {
                        User = user,
                        Key = new JObject { { keyName, JToken.FromObject(value) } }
                    };

                    await FileHelper.CryptoWriteAsync(profiledatastring, HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newProfile)));
                }
            }
            else
            {
                string globaldatastring = directorypath + "/Global.json";

                if (File.Exists(globaldatastring))
                {
                    jObject = JObject.Parse(Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(globaldatastring, HTTPPrivateKey.HTTPPrivatekey)));

                    // Check if the key name already exists in the JSON
                    JToken existingKey = jObject.SelectToken($"$..{keyName}");

                    if (existingKey != null)
                    {
                        // Update the value of the existing key
                        existingKey.Replace(JToken.FromObject(value));
                    }
                    else
                    {
                        // Step 2: Add a new entry to the "Key" object
                        jObject["Key"][keyName] = JToken.FromObject(value);
                    }

                    await FileHelper.CryptoWriteAsync(globaldatastring, HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(jObject.ToString(Formatting.None)));
                }
                else
                {
                    // Create a new profile with the key field
                    OHSGlobalProfile newProfile = new OHSGlobalProfile
                    {
                        Key = new JObject { { keyName, JToken.FromObject(value) } }
                    };

                    await FileHelper.CryptoWriteAsync(globaldatastring, HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newProfile)));
                }
            }

            if (JToken.FromObject(value).Type == JTokenType.String)
            {
                // Handle string type
                output = "\"" + JToken.FromObject(value).ToString() + "\"";
            }
            else if (JToken.FromObject(value).Type == JTokenType.Integer)
            {
                // Handle integer type
                output = JToken.FromObject(value).ToString();
            }
            else if (JToken.FromObject(value).Type == JTokenType.Array)
            {
                // Handle array type
                output = OHSProcessor.ConvertToLuaTable(JToken.FromObject(value), false);
            }
            else if (JToken.FromObject(value).Type == JTokenType.Boolean)
            {
                // Handle boolean type
                output = JToken.FromObject(value).ToObject<bool>() ? "true" : "false";
            }

            if (batchparams != "")
            {
                return output;
            }
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", $"{{ [\"status\"] = \"success\", [\"value\"] = {output} }}"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                    {
                        dataforohs = returnValues2nd[0]?.ToString();
                    }
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }

                byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{dataforohs}</ohs>");

                response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                response.StatusCode = (int)HttpStatusCode.OK;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = postresponsetooutput.Length;
                        response.OutputStream.Write(postresponsetooutput, 0, postresponsetooutput.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                }
            }

            return "";
        }

        public static async Task<string> get_all(string directorypath, string batchparams, bool global, HttpListenerRequest request, HttpListenerResponse response)
        {
            string dataforohs = "";

            string value = "";

            if (batchparams == "")
            {
                var data = MultipartFormDataParser.Parse(request.InputStream, Extensions.ExtractBoundary(request.ContentType));

                ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                dataforohs = data.GetParameterValue("data");

                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_ENCRYPTEDJAMINVALUE_HERE", dataforohs.Substring(8)));

                    if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                    {
                        dataforohs = returnValues[0]?.ToString();
                    }
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
            {
                dataforohs = batchparams;
            }

            // Parsing the JSON string
            JObject jsonObject = JObject.Parse(dataforohs);

            if (!global)
            {
                // Getting the value of the "user" field
                dataforohs = (string)jsonObject["user"];

                if (File.Exists(directorypath + $"/User_Profiles/{dataforohs}.json"))
                {
                    string tempreader = Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(directorypath + $"/User_Profiles/{dataforohs}.json", HTTPPrivateKey.HTTPPrivatekey));

                    // Parse the JSON string to a JObject
                    jsonObject = JObject.Parse(tempreader);

                    // Check if the "Key" property exists and if it is an object
                    if (jsonObject.TryGetValue("Key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                    {
                        // Convert the JToken to a Lua table-like string
                        value = OHSProcessor.ConvertToLuaTable(keyValueToken, false);
                    }
                }
            }
            else
            {
                if (File.Exists(directorypath + $"/Global.json"))
                {
                    string tempreader = Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(directorypath + $"/Global.json", HTTPPrivateKey.HTTPPrivatekey));

                    // Parse the JSON string to a JObject
                    jsonObject = JObject.Parse(tempreader);

                    // Check if the "Key" property exists and if it is an object
                    if (jsonObject.TryGetValue("Key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                    {
                        // Convert the JToken to a Lua table-like string
                        value = OHSProcessor.ConvertToLuaTable(keyValueToken, false);
                    }
                }
            }

            if (value == "")
            {
                value = "{ }";
            }

            if (batchparams != "")
            {
                return value;
            }
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\", [\"value\"] = " + value + " }"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                    {
                        dataforohs = returnValues2nd[0]?.ToString();
                    }
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }

                byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{dataforohs}</ohs>");

                response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                response.StatusCode = (int)HttpStatusCode.OK;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = postresponsetooutput.Length;
                        response.OutputStream.Write(postresponsetooutput, 0, postresponsetooutput.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not important.
                    }
                }
            }

            return "";
        }

        public static async Task<string> get(string directorypath, string batchparams, bool global, HttpListenerRequest request, HttpListenerResponse response)
        {
            string dataforohs = "";

            string value = "";

            if (batchparams == "")
            {
                var data = MultipartFormDataParser.Parse(request.InputStream, Extensions.ExtractBoundary(request.ContentType));

                ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                dataforohs = data.GetParameterValue("data");

                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_ENCRYPTEDJAMINVALUE_HERE", dataforohs.Substring(8)));

                    if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                    {
                        dataforohs = returnValues[0]?.ToString();
                    }
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
            {
                dataforohs = batchparams;
            }

            // Parsing the JSON string
            JObject jsonObject = JObject.Parse(dataforohs);

            if (!global)
            {
                // Getting the value of the "user" field
                dataforohs = (string)jsonObject["user"];

                if (File.Exists(directorypath + $"/User_Profiles/{dataforohs}.json"))
                {
                    // Parse the JSON string to a JObject
                    jsonObject = JObject.Parse(Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(directorypath + $"/User_Profiles/{dataforohs}.json", HTTPPrivateKey.HTTPPrivatekey)));

                    // Check if the "Key" property exists and if it is an object
                    if (jsonObject.TryGetValue("Key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                    {
                        // Convert the JToken to a Lua table-like string
                        value = OHSProcessor.ConvertToLuaTable(keyValueToken, false);
                    }
                }
            }
            else
            {
                if (File.Exists(directorypath + $"/Global.json"))
                {
                    // Parse the JSON string to a JObject
                    jsonObject = JObject.Parse(Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(directorypath + $"/Global.json", HTTPPrivateKey.HTTPPrivatekey)));

                    // Check if the "Key" property exists and if it is an object
                    if (jsonObject.TryGetValue("Key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                    {
                        // Convert the JToken to a Lua table-like string
                        value = OHSProcessor.ConvertToLuaTable(keyValueToken, false);
                    }
                }
                else if ((string)jsonObject["key"] == "vickie_version")
                {
                    value = $"{{ [\"vickie_version\"] = 7 }}"; // Random value for vickie if file not exist.
                }
            }

            if (value == "")
            {
                value = "{ }";
            }

            if (batchparams != "")
            {
                return value;
            }
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\", [\"value\"] = " + value + " }"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                    {
                        dataforohs = returnValues2nd[0]?.ToString();
                    }
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }

                byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{dataforohs}</ohs>");

                response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                response.StatusCode = (int)HttpStatusCode.OK;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = postresponsetooutput.Length;
                        response.OutputStream.Write(postresponsetooutput, 0, postresponsetooutput.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not important.
                    }
                }
            }

            return "";
        }

        public static async Task<string> user_id(string batchparams, HttpListenerRequest request, HttpListenerResponse response)
        {
            string dataforohs = "";

            if (batchparams == "")
            {
                var data = MultipartFormDataParser.Parse(request.InputStream, Extensions.ExtractBoundary(request.ContentType));

                ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                dataforohs = data.GetParameterValue("data");

                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_ENCRYPTEDJAMINVALUE_HERE", dataforohs.Substring(8)));

                    if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                    {
                        dataforohs = returnValues[0]?.ToString();
                    }
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
            {
                dataforohs = batchparams;
            }

            // Parsing the JSON string
            JObject jsonObject = JObject.Parse(dataforohs);

            // Getting the value of the "user" field
            dataforohs = (string)jsonObject["user"];

            if (batchparams != "")
            {
                return UniqueNumberGenerator.GenerateUniqueNumber(dataforohs).ToString();
            }
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\", [\"value\"] = " + UniqueNumberGenerator.GenerateUniqueNumber(dataforohs).ToString() + " }"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                    {
                        dataforohs = returnValues2nd[0]?.ToString();
                    }
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }

                byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{dataforohs}</ohs>");

                response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                response.StatusCode = (int)HttpStatusCode.OK;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = postresponsetooutput.Length;
                        response.OutputStream.Write(postresponsetooutput, 0, postresponsetooutput.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            return "";
        }

        public static async Task<string> user_getwritekey(string batchparams, HttpListenerRequest request, HttpListenerResponse response)
        {
            string dataforohs = "";

            if (batchparams == "")
            {
                var data = MultipartFormDataParser.Parse(request.InputStream, Extensions.ExtractBoundary(request.ContentType));

                ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                dataforohs = data.GetParameterValue("data");

                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_ENCRYPTEDJAMINVALUE_HERE", dataforohs.Substring(8)));

                    if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                    {
                        dataforohs = returnValues[0]?.ToString();
                    }
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
            {
                dataforohs = batchparams;
            }

            // Parsing the JSON string
            JObject jsonObject = JObject.Parse(dataforohs);

            dataforohs = OHSProcessor.GetFirstEightCharacters(OHSProcessor.CalculateMD5HashToExadecimal((string)jsonObject["user"]));

            if (batchparams != "")
            {
                return "{ [\"writeKey\"] = \"" + dataforohs + "\" }";
            }
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\",[\"value\"] = { [\"writeKey\"] = \"" + dataforohs + "\" } }"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                    {
                        dataforohs = returnValues2nd[0]?.ToString();
                    }
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }

                byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{dataforohs}</ohs>");

                response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                response.StatusCode = (int)HttpStatusCode.OK;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = postresponsetooutput.Length;
                        response.OutputStream.Write(postresponsetooutput, 0, postresponsetooutput.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not Important!
                    }
                }
            }

            return "";
        }
    }
}
