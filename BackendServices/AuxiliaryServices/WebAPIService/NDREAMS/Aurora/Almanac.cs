using CyberBackendLibrary.HTTP;
using HttpMultipartParser;
using Newtonsoft.Json;

namespace WebAPIService.NDREAMS.Aurora
{
    public static class Almanac
    {
        public static string? ProcessAlmanac(byte[]? PostData, string? ContentType, string fullurl, string apipath)
        {
            bool Weight = !string.IsNullOrEmpty(fullurl) && fullurl.Contains("Weights");
            string func = string.Empty;
            string name = string.Empty;
            string key = string.Empty;
            string element = string.Empty;
            string resdata = string.Empty;
            DateTime CurrentDate = DateTime.Today;
            string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

            if (!string.IsNullOrEmpty(boundary) && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    func = data.GetParameterValue("func");
                    name = data.GetParameterValue("name");
                    try
                    {
                        element = data.GetParameterValue("element");
                        resdata = data.GetParameterValue("data");
                    }
                    catch
                    {
                        // Not Important.
                    }
                    try
                    {
                        key = data.GetParameterValue("key");
                    }
                    catch
                    {
                        // Not Important.
                    }

                    ms.Flush();
                }

                Directory.CreateDirectory(apipath + "/NDREAMS/Aurora/SkyFishing");

                string SkyFishingProfilePath = apipath + $"/NDREAMS/Aurora/SkyFishing/{name}.json";

                switch (func)
                {
                    case "get":
                        // TODO, game seems to not work yet, but our response is correct.
                        int valuescount = 0;
                        return $"<xml><sig>{NDREAMSServerUtils.Server_GetSignature(fullurl, name, "SkyFishingGet", CurrentDate)}</sig><confirm>{NDREAMSServerUtils.Server_KeyToHash(key, CurrentDate, valuescount.ToString())}</confirm></xml>";
                    case "set":
                        if (File.Exists(SkyFishingProfilePath))
                        {
                            List<FishingProps>? props = JsonConvert.DeserializeObject<List<FishingProps>>(File.ReadAllText(SkyFishingProfilePath));

                            if (props == null)
                            {
                                CustomLogger.LoggerAccessor.LogWarn($"[nDreams] - Almanac: Profile:{SkyFishingProfilePath} has an invalid format! Erroring out client...");
                                return null;
                            }

                            foreach (FishingProps prop in props)
                            {
                                if (prop.a_id == element)
                                {
                                    if (Weight)
                                        prop.weight = resdata;
                                    else
                                        prop.str = resdata;
                                }
                            }

                            File.WriteAllText(SkyFishingProfilePath, JsonConvert.SerializeObject(props));
                        }
                        else
                        {
                            if (Weight)
                                File.WriteAllText(SkyFishingProfilePath, JsonConvert.SerializeObject(new List<FishingProps>() { new() { a_id = element, weight = resdata } }));
                            else
                                File.WriteAllText(SkyFishingProfilePath, JsonConvert.SerializeObject(new List<FishingProps>() { new() { a_id = element, str = resdata } }));
                        }
                        return $"<xml></xml>";
                }
            }

            return null;
        }

        public class FishingProps
        {
            public string? a_id { get; set; }
            public string? str { get; set; }
            public string? weight { get; set; }
        }
    }
}
