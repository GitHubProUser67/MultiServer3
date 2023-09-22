using HttpMultipartParser;
using MultiServer.HTTPService;
using NetCoreServer;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.OHS
{
    public class OHSUserCounter
    {
        public static string Set(string directorypath, string batchparams, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string dataforohs = string.Empty;

            string output = string.Empty;

            if (batchparams == string.Empty)
            {
                string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                    ms.Position = 0;

                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                    dataforohs = OHSProcessor.JaminDeFormat(data.GetParameterValue("data"), true);

                    if (dataforohs == null)
                    {
                        response.SetBegin((int)HttpStatusCode.InternalServerError);
                        response.SetBody();
                        return string.Empty;
                    }

                    ms.Dispose();
                }
            }
            else
                dataforohs = batchparams;

            JToken Token = JToken.Parse(dataforohs);

            object value = OHSProcessor.GetValueFromJToken(Token, "value");

            object key = OHSProcessor.GetValueFromJToken(Token, "key");

            object user = OHSProcessor.GetValueFromJToken(Token, "user");

            string profiledatastring = directorypath + $"/User_Profiles/{user}_Currency.json";

            if (File.Exists(profiledatastring))
            {
                JObject jObject = JObject.Parse(Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(profiledatastring, HTTPPrivateKey.HTTPPrivatekey)));

                if (jObject != null)
                {
                    // Check if the key name already exists in the JSON
                    JToken existingKey = jObject.SelectToken($"$..{key}");

                    if (existingKey != null)
                        // Update the value of the existing key
                        existingKey.Replace(JToken.FromObject(value));
                    else
                    {
                        JToken KeyEntry = jObject["key"];

                        if (KeyEntry != null)
                            // Step 2: Add a new entry to the "Key" object
                            KeyEntry[key] = JToken.FromObject(value);
                    }

                    FileHelper.CryptoWriteAsync(profiledatastring, HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(jObject.ToString(Formatting.None)), false).Wait();
                }
            }
            else
            {
                string keystring = key.ToString();

                if (keystring != null)
                {
                    // Create a new profile with the key field
                    OHSUserProfile newProfile = new OHSUserProfile
                    {
                        user = user.ToString(),
                        key = new JObject { { keystring, JToken.FromObject(value) } }
                    };

                    FileHelper.CryptoWriteAsync(profiledatastring, HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(newProfile)), false).Wait();
                }
            }

            if (JToken.FromObject(value).Type == JTokenType.Integer)
                // Handle integer type
                output = JToken.FromObject(value).ToString();

            if (batchparams != "")
                return $"{{ [\"{key}\"] = {output} }}";
            else
            {
                dataforohs = OHSProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [\"{key}\"] = {output} }} }}");

                if (dataforohs == null)
                {
                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                    response.SetBody();
                    return string.Empty;
                }

                response.SetContentType("application/xml;charset=UTF-8");
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody($"<ohs>{dataforohs}</ohs>");
            }

            return string.Empty;
        }

        public static string Get_All(string directorypath, string batchparams, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string dataforohs = string.Empty;

            string value = string.Empty;

            if (batchparams == string.Empty)
            {
                string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                    ms.Position = 0;

                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                    dataforohs = OHSProcessor.JaminDeFormat(data.GetParameterValue("data"), true);

                    if (dataforohs == null)
                    {
                        response.SetBegin((int)HttpStatusCode.InternalServerError);
                        response.SetBody();
                        return string.Empty;
                    }

                    ms.Dispose();
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

                    // Check if the "key" property exists and if it is an object
                    if (jsonObject.TryGetValue("key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                        // Convert the JToken to a Lua table-like string
                        value = OHSProcessor.ConvertToLuaTable(keyValueToken, false);
                }
            }

            if (value == string.Empty)
                value = "{ }";

            if (batchparams != string.Empty)
                return value;
            else
            {
                dataforohs = OHSProcessor.JaminFormat("{ [\"status\"] = \"success\", [\"value\"] = " + value + " }");

                if (dataforohs == null)
                {
                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                    response.SetBody();
                    return string.Empty;
                }

                response.SetContentType("application/xml;charset=UTF-8");
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody($"<ohs>{dataforohs}</ohs>");
            }

            return string.Empty;
        }

        public static string Get(string directorypath, string batchparams, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string dataforohs = string.Empty;

            string value = string.Empty;

            if (batchparams == string.Empty)
            {
                string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                    ms.Position = 0;

                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                    dataforohs = OHSProcessor.JaminDeFormat(data.GetParameterValue("data"), true);

                    if (dataforohs == null)
                    {
                        response.SetBegin((int)HttpStatusCode.InternalServerError);
                        response.SetBody();
                        return string.Empty;
                    }

                    ms.Dispose();
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
                    if (jsonObject.TryGetValue("key", out JToken keyValueToken) && keyValueToken.Type == JTokenType.Object)
                        // Convert the JToken to a Lua table-like string
                        value = OHSProcessor.ConvertToLuaTable(keyValueToken, false);
                }
            }

            if (value == string.Empty)
                value = "{ }";

            if (batchparams != string.Empty)
                return value;
            else
            {
                dataforohs = OHSProcessor.JaminFormat("{ [\"status\"] = \"success\", [\"value\"] = " + value + " }");

                if (dataforohs == null)
                {
                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                    response.SetBody();
                    return string.Empty;
                }

                response.SetContentType("application/xml;charset=UTF-8");
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody($"<ohs>{dataforohs}</ohs>");
            }

            return string.Empty;
        }
    }
}
