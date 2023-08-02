using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.OHS
{
    public class OHSCommunity
    {
        public static async Task<string> community_getscore(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
        {
            string dataforohs = "";

            int value = 0;

            if (batchparams == "")
            {
                var data = MultipartFormDataParser.Parse(request.InputStream, Misc.ExtractBoundary(request.ContentType));

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
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
            {
                dataforohs = batchparams;
            }

            if (dataforohs == null)
            {
                if (batchparams == "")
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "";
            }

            // Parsing the JSON string
            JObject inputjsonObject = JObject.Parse(dataforohs);

            // Getting the value of the "user" field
            dataforohs = (string)inputjsonObject["key"];

            if (File.Exists(directorypath + $"/Community_Profiles/{dataforohs}.json"))
            {
                string tempreader = Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(directorypath + $"/Community_Profiles/{dataforohs}.json", HTTPPrivateKey.HTTPPrivatekey));

                // Deserialize the JSON string into a dynamic object
                dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(tempreader);

                if (jsonObject.Key != null)
                {
                    if (jsonObject.Value != null)
                    {
                        value = (int)jsonObject.Value;
                    }
                }
            }

            if (batchparams != "")
            {
                return "{ [\"score\"] = " + value.ToString() + " }";
            }
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\", [\"value\"] = { [\"score\"] = " + value.ToString() + " } }"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                    {
                        dataforohs = returnValues2nd[0]?.ToString();
                    }
                }
                catch (Exception ex)
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
                    catch (Exception ex)
                    {
                        // Not Important.
                    }
                }
            }

            return "";
        }

        public static async Task<string> community_updatescore(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
        {
            string dataforohs = "";

            int value = 0;

            int increment = 0;

            if (batchparams == "")
            {
                var data = MultipartFormDataParser.Parse(request.InputStream, Misc.ExtractBoundary(request.ContentType));

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
                catch (Exception ex)
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
            JObject inputjsonObject = JObject.Parse(dataforohs);

            // Getting the value of the "user" field
            dataforohs = (string)inputjsonObject["key"];

            increment = (int)inputjsonObject["inc"];

            if (File.Exists(directorypath + $"/Community_Profiles/{dataforohs}.json"))
            {
                string tempreader = Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(directorypath + $"/Community_Profiles/{dataforohs}.json", HTTPPrivateKey.HTTPPrivatekey));

                // Deserialize the JSON string into a dynamic object
                dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(tempreader);

                if (jsonObject.Key != null)
                {
                    // Check if the Value is null before attempting to increment
                    if (jsonObject.Value != null)
                    {
                        jsonObject.Value = (int)jsonObject.Value + increment;
                    }
                    else
                    {
                        // If Value is null, initialize it with the increment value
                        jsonObject.Value = increment;
                    }

                    value = (int)jsonObject.Value;
                }

                await FileHelper.CryptoWriteAsync(directorypath + $"/Community_Profiles/{dataforohs}.json", HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonObject, Formatting.Indented)));
            }
            else
            {
                await FileHelper.CryptoWriteAsync(directorypath + $"/Community_Profiles/{dataforohs}.json", HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes($"{{ \"Key\":{increment} }}"));

                value = increment;
            }

            if (batchparams != "")
            {
                return "{ [\"score\"] = " + value.ToString() + " }";
            }
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\", [\"value\"] = { [\"score\"] = " + value.ToString() + " } }"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                    {
                        dataforohs = returnValues2nd[0]?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
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
                catch (Exception ex)
                {
                    // Not Important.
                }
            }

            return "";
        }
    }
}
