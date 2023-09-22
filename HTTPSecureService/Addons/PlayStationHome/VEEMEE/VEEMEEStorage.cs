using System.Net;
using System.Text;
using NetCoreServer;
using HttpMultipartParser;
using MultiServer.HTTPService;
using Newtonsoft.Json;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEEStorage
    {
        public static void readconfig(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string config = "";
            string product = "";

            string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                ms.Position = 0;

                var data = MultipartFormDataParser.Parse(ms, boundary);

                config = data.GetParameterValue("config");

                product = data.GetParameterValue("product");

                ms.Dispose();
            }

            string configValue = "{}"; // Default response when config field doesn't exist

            if (!string.IsNullOrEmpty(config) && !string.IsNullOrEmpty(product))
            {
                string jsonFilePath = Path.Combine(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/config.json");

                if (File.Exists(jsonFilePath))
                {
                    string injsondata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(jsonFilePath, HTTPPrivateKey.HTTPPrivatekey));

                    if (injsondata != null)
                    {
                        dynamic jsondata = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(jsonFilePath, HTTPPrivateKey.HTTPPrivatekey)));

                        if (jsondata != null)
                        {
                            if (jsondata.ContainsKey(product) && jsondata[product].ContainsKey(config))
                                configValue = jsondata[product][config].ToString();
                        }
                    }
                }
            }

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetBody(VEEMEEProcessor.sign(configValue));
        }

        public static void readtable(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string psnid = "";
            string product = "";
            string hex = "";
            string __salt = "";

            string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                ms.Position = 0;

                var data = MultipartFormDataParser.Parse(ms, boundary);

                psnid = data.GetParameterValue("psnid");

                product = data.GetParameterValue("product");

                hex = data.GetParameterValue("hex");

                __salt = data.GetParameterValue("__salt");

                ms.Dispose();
            }

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetBody(VEEMEEProfileManager.ReadProfile(psnid, product, hex, __salt));
        }

        public static async void writetable(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string psnid = "";
            string profile = "";

            string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                ms.Position = 0;

                var data = MultipartFormDataParser.Parse(ms, boundary);

                psnid = data.GetParameterValue("psnid");

                profile = data.GetParameterValue("profile");

                ms.Dispose();
            }

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetBody(await VEEMEEProfileManager.WriteProfile(psnid, profile));
        }
    }
}
