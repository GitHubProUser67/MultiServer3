using CustomLogger;
using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;

namespace HTTPSecureServer.API.OHS
{
    public class UserInventory
    {
        public static string? GetGlobalItems(byte[] PostData, string ContentType, string directorypath, string batchparams)
        {
            string? dataforohs = null;
            string? output = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);

                if (boundary != null)
                {
                    using (MemoryStream ms = new(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = CryptoSporidium.OHS.JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true);
                        ms.Flush();
                    }
                }
            }

            try
            {
                string globalinvdatastring = directorypath + "/Global_Variables.json";

                if (File.Exists(globalinvdatastring))
                {
                    string filedata = File.ReadAllText(globalinvdatastring);
                    if (string.IsNullOrEmpty(filedata))
                        output = "{ " + CryptoSporidium.OHS.JaminProcessor.ConvertToLuaTable(JToken.Parse(filedata), false) + " }";
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[UserInventory] - Json Format Error - {ex}");
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
                    dataforohs = CryptoSporidium.OHS.JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }");
                else
                    dataforohs = CryptoSporidium.OHS.JaminProcessor.JaminFormat("{ [\"status\"] = \"success\", [\"value\"] = { " + output + " } }");
            }

            return dataforohs;
        }

        public static string? GetUserInventory(byte[] PostData, string ContentType, string directorypath, string batchparams)
        {
            string? dataforohs = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);

                if (boundary != null)
                {
                    using (MemoryStream ms = new(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = CryptoSporidium.OHS.JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true);
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
                    // Deserialize the JSON data into a JObject
                    JObject? jObject = JsonConvert.DeserializeObject<JObject>(dataforohs);

                    if (jObject != null)
                    {
                        string? user = jObject.Value<string>("user");
                        string? region = jObject.Value<string>("region");

                        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(region))
                        {
                            string inventorypath = directorypath + $"/User_Inventory/{user}_{region}/";

                            StringBuilder? resultBuilder = new();

                            if (Directory.Exists(inventorypath))
                            {
                                JToken? keyToken = jObject.GetValue("inventory_names");

                                if (keyToken != null)
                                {
                                    foreach (string? key in keyToken)
                                    {
                                        if (File.Exists(inventorypath + key + ".json"))
                                        {
                                            string inventorydata = File.ReadAllText(inventorypath + key + ".json");

                                            if (inventorydata != null)
                                            {
                                                string datafrominventory = CryptoSporidium.OHS.JaminProcessor.ConvertToLuaTable(JToken.Parse(inventorydata), false);

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
                            }
                            else
                            {
                                JToken? keyToken = jObject.GetValue("inventory_names");

                                if (keyToken != null)
                                {
                                    foreach (string? key in keyToken)
                                    {
                                        if (resultBuilder.Length == 0)
                                            resultBuilder.Append($"{{ [\"{key}\"] = {{ }}");
                                        else
                                            resultBuilder.Append($", [\"{key}\"] = {{ }}");
                                    }
                                }
                            }

                            if (resultBuilder.Length != 0)
                            {
                                resultBuilder.Append(" }");
                                dataforohs = resultBuilder.ToString();
                            }
                            else
                                dataforohs = string.Empty;

                            resultBuilder = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[UserInventory] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(dataforohs))
                    return null;
                else
                    return dataforohs;
            }
            else
            {
                if (string.IsNullOrEmpty(dataforohs))
                    dataforohs = CryptoSporidium.OHS.JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }");
                else
                    dataforohs = CryptoSporidium.OHS.JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {dataforohs} }}");
            }

            return dataforohs;
        }
    }
}
