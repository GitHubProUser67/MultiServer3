using HttpMultipartParser;
using MultiServer.HTTPService;
using NetCoreServer;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.OHS
{
    public class OHSUserInventory
    {
        public static string GetGlobalItems(string directorypath, string batchparams, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
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

            try
            {
                string globalinvdatastring = directorypath + $"Global_Variables.json";

                if (File.Exists(globalinvdatastring))
                {
                    string filedata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(globalinvdatastring, HTTPPrivateKey.HTTPPrivatekey));

                    if (filedata != null)
                        output = "{ " + OHSProcessor.ConvertToLuaTable(JToken.Parse(filedata), false) + " }";
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[OHSUserInventory] - Json Format Error - {ex}");
            }

            if (batchparams != string.Empty)
                return output;
            else
            {
                if (output == string.Empty)
                {
                    dataforohs = OHSProcessor.JaminFormat("{ [\"status\"] = \"fail\" }");

                    if (dataforohs == null)
                    {
                        response.SetBegin((int)HttpStatusCode.InternalServerError);
                        response.SetBody();
                        return string.Empty;
                    }
                }
                else
                {
                    dataforohs = OHSProcessor.JaminFormat("{ [\"status\"] = \"success\", [\"value\"] = { " + output + " } }");

                    if (dataforohs == null)
                    {
                        response.SetBegin((int)HttpStatusCode.InternalServerError);
                        response.SetBody();
                        return string.Empty;
                    }
                }

                response.SetContentType("application/xml;charset=UTF-8");
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody($"<ohs>{dataforohs}</ohs>");
            }

            return string.Empty;
        }

        public static string GetUserInventory(string directorypath, string batchparams, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
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

            try
            {
                // Deserialize the JSON data into a JObject
                JObject jObject = JsonConvert.DeserializeObject<JObject>(dataforohs);

                if (jObject != null)
                {
                    string user = jObject.Value<string>("user");
                    string region = jObject.Value<string>("region");
                    string inventorypath = directorypath + $"User_Inventory/{user}_{region}/";

                    StringBuilder resultBuilder = new StringBuilder();

                    if (Directory.Exists(inventorypath))
                    {
                        JToken keyToken = jObject.GetValue("inventory_names");

                        if (keyToken != null)
                        {
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
                    }
                    else
                    {
                        JToken keyToken = jObject.GetValue("inventory_names");

                        if (keyToken != null)
                        {
                            foreach (string key in keyToken)
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
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[OHSUserInventory] - Json Format Error - {ex}");
            }

            if (batchparams != string.Empty)
                return dataforohs;
            else
            {
                if (dataforohs == string.Empty)
                {
                    dataforohs = OHSProcessor.JaminFormat("{ [\"status\"] = \"fail\" }");

                    if (dataforohs == null)
                    {
                        response.SetBegin((int)HttpStatusCode.InternalServerError);
                        response.SetBody();
                        return string.Empty;
                    }
                }
                else
                {
                    dataforohs = OHSProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {dataforohs} }}");

                    if (dataforohs == null)
                    {
                        response.SetBegin((int)HttpStatusCode.InternalServerError);
                        response.SetBody();
                        return string.Empty;
                    }
                }

                response.SetContentType("application/xml;charset=UTF-8");
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody($"<ohs>{dataforohs}</ohs>");
            }

            return string.Empty;
        }
    }
}
