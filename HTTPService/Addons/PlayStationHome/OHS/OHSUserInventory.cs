using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MultiServer.HTTPService.Addons.PlayStationHome.OHS
{
    public class OHSUserInventory
    {
        public static async Task<string> getGlobalItems(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
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

            string globalinvdatastring = directorypath + $"Global_Variables.json";

            if (File.Exists(globalinvdatastring))
            {
                string filedata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(globalinvdatastring, HTTPPrivateKey.HTTPPrivatekey));

                if (filedata != null)
                    output = "{ " + OHSProcessor.ConvertToLuaTable(JToken.Parse(filedata), false) + " }";
            }

            if (batchparams != "")
                return output;
            else
            {
                if (output == "")
                {
                    try
                    {
                        // Execute the Lua script and get the result
                        object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"fail\" }"));

                        if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                            dataforohs = returnValues2nd[0]?.ToString();
                    }
                    catch (Exception)
                    {
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return "";
                    }
                }
                else
                {
                    try
                    {
                        // Execute the Lua script and get the result
                        object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\", [\"value\"] = { " + output + " } }"));

                        if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                            dataforohs = returnValues2nd[0]?.ToString();
                    }
                    catch (Exception)
                    {
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return "";
                    }
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

        public static async Task<string> getUserInventory(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
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

            // Deserialize the JSON data into a JObject
            JObject jObject = JsonConvert.DeserializeObject<JObject>(dataforohs);

            string user = jObject.Value<string>("user");
            string region = jObject.Value<string>("region");

            string inventorypath = directorypath + $"User_Inventory/{user}_{region}/";

            StringBuilder resultBuilder = new StringBuilder();

            if (Directory.Exists(inventorypath))
            {
                JToken keyToken = jObject.GetValue("inventory_names");

                foreach (string key in keyToken)
                {
                    if (File.Exists(inventorypath + key + ".json"))
                    {
                        string inventorydata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(inventorypath + key + ".json", HTTPPrivateKey.HTTPPrivatekey));

                        if (inventorydata != null)
                        {
                            string datafrominventory = OHSProcessor.ConvertToLuaTable(JToken.Parse(inventorydata), false);

                            if (resultBuilder.Length == 0)
                                resultBuilder.Append($"{{ [\"{key}\"] = {datafrominventory}");
                            else
                                resultBuilder.Append($", [\"{key}\"] = {datafrominventory}");
                        }
                    }
                    else
                    {
                        if (resultBuilder.Length == 0)
                            resultBuilder.Append($"{{ [\"{key}\"] = {{ }}");
                        else
                            resultBuilder.Append($", [\"{key}\"] = {{ }}");
                    }
                }
            }
            else
            {
                JToken keyToken = jObject.GetValue("inventory_names");

                foreach (string key in keyToken)
                {
                    if (resultBuilder.Length == 0)
                        resultBuilder.Append($"{{ [\"{key}\"] = {{ }}");
                    else
                        resultBuilder.Append($", [\"{key}\"] = {{ }}");
                }
            }

            if (resultBuilder.Length != 0)
            {
                resultBuilder.Append(" }");
                dataforohs = resultBuilder.ToString();
            }
            else
                dataforohs = "";

            if (batchparams != "")
                return dataforohs;
            else
            {
                if (dataforohs == "")
                {
                    try
                    {
                        // Execute the Lua script and get the result
                        object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", $"{{ [\"status\"] = \"fail\" }}"));

                        if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                            dataforohs = returnValues2nd[0]?.ToString();
                    }
                    catch (Exception)
                    {
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return "";
                    }
                }
                else
                {
                    try
                    {
                        // Execute the Lua script and get the result
                        object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", $"{{ [\"status\"] = \"success\", [\"value\"] = {dataforohs} }}"));

                        if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                            dataforohs = returnValues2nd[0]?.ToString();
                    }
                    catch (Exception)
                    {
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        return "";
                    }
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
    }
}
