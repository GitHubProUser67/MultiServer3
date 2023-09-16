using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MultiServer.HTTPService.Addons.PlayStationHome.OHS
{
    public class OHSUserCounter
    {
        public static async Task<string> set(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
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
                        dataforohs = returnValues[0]?.ToString();
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
                dataforohs = batchparams;

            JToken keyToken = JToken.Parse(dataforohs);

            // Deserialize the JSON data into a JObject
            JObject jObject = JsonConvert.DeserializeObject<JObject>(dataforohs);

            object value = OHSProcessor.GetValueFromJToken(keyToken, "value");

            keyToken = jObject.GetValue("key");

            string keyName = keyToken.Value<string>();

            string user = jObject.Value<string>("user");

            string profiledatastring = directorypath + $"/User_Profiles/{user}_Currency.json";

            if (File.Exists(profiledatastring))
            {
                jObject = JObject.Parse(Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(profiledatastring, HTTPPrivateKey.HTTPPrivatekey)));

                if (jObject != null)
                {
                    // Check if the key name already exists in the JSON
                    JToken existingKey = jObject.SelectToken($"$..{keyName}");

                    if (existingKey != null)
                        // Update the value of the existing key
                        existingKey.Replace(JToken.FromObject(value));
                    else
                        // Step 2: Add a new entry to the "Key" object
                        jObject["Key"][keyName] = JToken.FromObject(value);

                    await FileHelper.CryptoWriteAsync(profiledatastring, HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(jObject.ToString(Formatting.None)), false);
                }
            }
            else
            {
                // Create a new profile with the key field
                OHSUserProfile newProfile = new OHSUserProfile
                {
                    User = user,
                    Key = new JObject { { keyName, JToken.FromObject(value) } }
                };

                await FileHelper.CryptoWriteAsync(profiledatastring, HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newProfile)), false);
            }

            if (JToken.FromObject(value).Type == JTokenType.Integer)
                // Handle integer type
                output = JToken.FromObject(value).ToString();

            if (batchparams != "")
                return $"{{ [\"{keyName}\"] = {output} }}";
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", $"{{ [\"status\"] = \"success\", [\"value\"] = {{ [\"{keyName}\"] = {output} }} }}"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                        dataforohs = returnValues2nd[0]?.ToString();
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }

                byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{dataforohs}</ohs>");

                response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentLength64 = postresponsetooutput.Length;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
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

        public static async Task<string> get_all(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
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
                        dataforohs = returnValues[0]?.ToString();
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
                dataforohs = batchparams;

            // Parsing the JSON string
            JObject jsonObject = JObject.Parse(dataforohs);

            // Getting the value of the "user" field
            dataforohs = (string)jsonObject["user"];

            if (File.Exists(directorypath + $"/User_Profiles/{dataforohs}_Currency.json"))
            {
                string tempreader = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(directorypath + $"/User_Profiles/{dataforohs}_Currency.json", HTTPPrivateKey.HTTPPrivatekey));

                if (tempreader != null)
                {
                    // Parse the JSON string to a JObject
                    jsonObject = JObject.Parse(tempreader);

                    // Check if the "Key" property exists and if it is an object
                    if (jsonObject.TryGetValue("Key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                        // Convert the JToken to a Lua table-like string
                        value = OHSProcessor.ConvertToLuaTable(keyValueToken, false);
                }
            }

            if (value == "")
                value = "{ }";

            if (batchparams != "")
                return value;
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\", [\"value\"] = " + value + " }"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                        dataforohs = returnValues2nd[0]?.ToString();
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }

                byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{dataforohs}</ohs>");

                response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentLength64 = postresponsetooutput.Length;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
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

        public static async Task<string> get(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
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
                        dataforohs = returnValues[0]?.ToString();
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
                dataforohs = batchparams;

            // Parsing the JSON string
            JObject jsonObject = JObject.Parse(dataforohs);

            // Getting the value of the "user" field
            dataforohs = (string)jsonObject["user"];

            if (File.Exists(directorypath + $"/User_Profiles/{dataforohs}_Currency.json"))
            {
                string currencydata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(directorypath + $"/User_Profiles/{dataforohs}_Currency.json", HTTPPrivateKey.HTTPPrivatekey));

                if (currencydata != null)
                {
                    // Parse the JSON string to a JObject
                    jsonObject = JObject.Parse(currencydata);

                    // Check if the "Key" property exists and if it is an object
                    if (jsonObject.TryGetValue("Key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                        // Convert the JToken to a Lua table-like string
                        value = OHSProcessor.ConvertToLuaTable(keyValueToken, false);
                }
            }

            if (value == "")
                value = "{ }";

            if (batchparams != "")
                return value;
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\", [\"value\"] = " + value + " }"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                        dataforohs = returnValues2nd[0]?.ToString();
                }
                catch (Exception)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }

                byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{dataforohs}</ohs>");

                response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentLength64 = postresponsetooutput.Length;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
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
    }
}
