using HttpMultipartParser;
using MultiServer.HTTPService;
using NetCoreServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.OHS
{
    public class OHSCommunity
    {
        public static string Community_Getscore(string directorypath, string batchparams, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string dataforohs = string.Empty;

            int value = 0;

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
                JToken Token = JToken.Parse(dataforohs);

                object key = OHSProcessor.GetValueFromJToken(Token, "key");

                if (File.Exists(directorypath + $"/Community_Profiles/{dataforohs}.json"))
                {
                    string tempreader = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(directorypath + $"/Community_Profiles/{dataforohs}.json", HTTPPrivateKey.HTTPPrivatekey));

                    JObject jObject = JObject.Parse(tempreader);

                    if (jObject != null)
                    {
                        // Check if the key name already exists in the JSON
                        JToken existingKey = jObject.SelectToken($"$..{key}");

                        if (existingKey != null)
                            // Get the value of the existing key
                            value = existingKey.Value<int>();
                    }
                }

            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[OHSCommunity] - Json Format Error - {ex}");
            }

            if (batchparams != string.Empty)
                return "{ [\"score\"] = " + value.ToString() + " }";
            else
            {
                dataforohs = OHSProcessor.JaminFormat("{ [\"status\"] = \"success\", [\"value\"] = { [\"score\"] = " + value.ToString() + " } }");

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

        public static string Community_UpdateScore(string directorypath, string batchparams, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string dataforohs = string.Empty;

            int value = 0;

            int increment = 0;

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
                JToken Token = JToken.Parse(dataforohs);

                object key = OHSProcessor.GetValueFromJToken(Token, "key");

                object inc = OHSProcessor.GetValueFromJToken(Token, "inc");

                if (File.Exists(directorypath + $"/Community_Profiles/{dataforohs}.json"))
                {
                    string tempreader = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(directorypath + $"/Community_Profiles/{dataforohs}.json", HTTPPrivateKey.HTTPPrivatekey));

                    JObject jObject = JObject.Parse(tempreader);

                    if (jObject != null)
                    {
                        // Check if the key name already exists in the JSON
                        JToken existingKey = jObject.SelectToken($"$..{key}");

                        if (existingKey != null)
                            // Get the value of the existing key
                            existingKey[key] = existingKey.Value<int>() + increment;
                        else
                        {
                            string keyname = key.ToString();

                            if (keyname != null)
                                // If the key doesn't exist, add it with the increment value
                                jObject.Add(new JProperty(keyname, increment));
                        }
                    }

                    FileHelper.CryptoWriteAsync(directorypath + $"/Community_Profiles/{dataforohs}.json", HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jObject, Formatting.Indented)), false).Wait();
                }
                else
                {
                    FileHelper.CryptoWriteAsync(directorypath + $"/Community_Profiles/{dataforohs}.json", HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes($"{{ \"Key\":{increment} }}"), false).Wait();
                    value = increment;
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[OHSCommunity] - Json Format Error - {ex}");
            }

            if (batchparams != string.Empty)
                return "{ [\"score\"] = " + value.ToString() + " }";
            else
            {
                dataforohs = OHSProcessor.JaminFormat("{ [\"status\"] = \"success\", [\"value\"] = { [\"score\"] = " + value.ToString() + " } }");

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
