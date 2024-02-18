using BackendProject.MiscUtils;
using HttpMultipartParser;
using Newtonsoft.Json;

namespace BackendProject.WeBAPIs.VEEMEE
{
    public class Storage
    {
        public static string? ReadConfig(byte[]? PostData, string? ContentType, string apiPath)
        {
            string config = string.Empty;
            string product = string.Empty;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    config = data.GetParameterValue("config");

                    product = data.GetParameterValue("product");

                    ms.Flush();
                }

                string configValue = "{}"; // Default response when config field doesn't exist

                if (!string.IsNullOrEmpty(config) && !string.IsNullOrEmpty(product))
                {
                    string jsonFilePath = Path.Combine($"{apiPath}/VEEMEE/Acorn_Medow/config.json");

                    if (File.Exists(jsonFilePath))
                    {
                        string? injsondata = File.ReadAllText(jsonFilePath);

                        if (injsondata != null)
                        {
                            dynamic? jsondata = JsonConvert.DeserializeObject(injsondata);

                            if (jsondata != null)
                            {
                                if (jsondata.ContainsKey(product) && jsondata[product].ContainsKey(config))
                                    configValue = jsondata[product][config].ToString();
                            }
                        }
                    }
                }

                return Processor.sign(configValue);
            }

            return null;
        }

        public static string? ReadTable(byte[]? PostData, string? ContentType, string apiPath)
        {
            string psnid = string.Empty;
            string product = string.Empty;
            string hex = string.Empty;
            string __salt = string.Empty;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    psnid = data.GetParameterValue("psnid");

                    product = data.GetParameterValue("product");

                    hex = data.GetParameterValue("hex");

                    __salt = data.GetParameterValue("__salt");

                    ms.Flush();
                }

                string? ProfileResult = ProfileManager.ReadProfile(psnid, product, hex, __salt, apiPath);

                if (ProfileResult != null)
                    return ProfileResult;
            }

            return null;
        }

        public static string? WriteTable(byte[]? PostData, string? ContentType, string apiPath)
        {
            string psnid = string.Empty;
            string profile = string.Empty;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    psnid = data.GetParameterValue("psnid");

                    profile = data.GetParameterValue("profile");

                    ms.Flush();
                }

                ProfileManager.WriteProfile(psnid, profile, apiPath);

                return Processor.sign(profile);
            }

            return null;
        }
    }

    public class ProfileManager
    {
        public static string? ReadProfile(string psnid, string product, string hex, string salt, string apiPath)
        {
            if (string.IsNullOrEmpty(hex) || string.IsNullOrEmpty(salt))
                return null;

            if (File.Exists($"{apiPath}/VEEMEE/Acorn_Medow/User_Profiles/{psnid}.json"))
                return Processor.sign(File.ReadAllText($"{apiPath}/VEEMEE/Acorn_Medow/User_Profiles/{psnid}.json"));
            else
                return Processor.sign(File.ReadAllText($"{apiPath}/VEEMEE/Acorn_Medow/default_profile.json"));
        }

        public static void WriteProfile(string psnid, string profile, string apiPath)
        {
            Directory.CreateDirectory($"{apiPath}/VEEMEE/Acorn_Medow/User_Profiles");

            File.WriteAllText($"{apiPath}/VEEMEE/Acorn_Medow/User_Profiles/{psnid}.json", profile);
        }
    }
}
