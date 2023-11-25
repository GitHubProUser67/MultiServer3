using CustomLogger;
using HttpMultipartParser;

namespace HTTPSecureServerLite.API.VEEMEE
{
    public class Stats
    {
        public static string? GetConfig(bool get, byte[]? PostData, string? ContentType)
        {
            if (!get)
            {
                string id = string.Empty;
                string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);

                if (boundary != null && PostData != null)
                {
                    using (MemoryStream ms = new(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);

                        id = data.GetParameterValue("id");

                        ms.Flush();
                    }
                }

                LoggerAccessor.LogInfo($"[VEEMEE] - Getconfig values : id|{id}");
            }

            if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/Acorn_Medow/stats_config.json"))
                return Processor.sign(File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/Acorn_Medow/stats_config.json"));
            else
                return null;
        }

        public static string? Crash(byte[]? PostData, string? ContentType)
        {
            string corehook = string.Empty;
            string territory = string.Empty;
            string region = string.Empty;
            string psnid = string.Empty;
            string scene = string.Empty;
            string sceneid = string.Empty;
            string scenetime = string.Empty;
            string sceneowner = string.Empty;
            string owner = string.Empty;
            string owned = string.Empty;
            string crash = string.Empty;
            string numplayers = string.Empty;
            string numpeople = string.Empty;
            string objectid = string.Empty;
            string objectname = string.Empty;
            string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    corehook = data.GetParameterValue("corehook");

                    territory = data.GetParameterValue("territory");

                    region = data.GetParameterValue("region");

                    psnid = data.GetParameterValue("psnid");

                    scene = data.GetParameterValue("scene");

                    sceneid = data.GetParameterValue("sceneid");

                    scenetime = data.GetParameterValue("scenetime");

                    sceneowner = data.GetParameterValue("sceneowner");

                    owner = data.GetParameterValue("owner");

                    owned = data.GetParameterValue("owned");

                    crash = data.GetParameterValue("crash");

                    numplayers = data.GetParameterValue("numplayers");

                    numpeople = data.GetParameterValue("numpeople");

                    objectid = data.GetParameterValue("objectid");

                    objectname = data.GetParameterValue("objectname");

                    ms.Flush();
                }

                LoggerAccessor.LogWarn($"[VEEMEE] : A Client Crash Happened - Details : corehook|{corehook} - territory|{territory} - region|{region} - psnid|{psnid}" +
                    $" - scene|{scene} - sceneid|{sceneid} - scenetime|{scenetime} - sceneowner|{sceneowner} - owner|{owner} - owned|{owned} - crash|{crash} -" +
                    $" numplayers|{numplayers} - numpeople|{numpeople} - objectid|{objectid} - objectname|{objectname}");
            }

            if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/Acorn_Medow/stats_config.json"))
                return Processor.sign(File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/Acorn_Medow/stats_config.json"));
            else
                return null;
        }

        public static string? Usage(byte[]? PostData, string? ContentType)
        {
            string usage = string.Empty;
            string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    usage = data.GetParameterValue("usage");

                    ms.Flush();
                }

                return Processor.sign(usage);
            }

            return null;
        }
    }
}
