using CustomLogger;
using System.IO;

namespace WebAPIService.LOOT
{
    public class LOOTTeleporter
    {

        public static string FetchTeleporterInfo(string workPath)
        {
            string LOOTTeleporterPath = $"{workPath}/LOOT/Teleporter";
            Directory.CreateDirectory(LOOTTeleporterPath);
            string teleporterJSONFilePath = $"{LOOTTeleporterPath}/Teleporter.json";

            if(File.Exists(teleporterJSONFilePath))
            {
                LoggerAccessor.LogInfo($"[LOOT] Teleporter - Found Teleporter JSON!");
                return $"<parameter>{File.ReadAllText(teleporterJSONFilePath)}</parameter>";
            } else
            {
                LoggerAccessor.LogWarn($"[LOOT] Teleporter - No override Teleporter JSON found, using default!\nExpected path {teleporterJSONFilePath}");
                //NOT 100% yet working
                return $"<parameter>{{\"g_destinations\":[{{\"sceneName\":\"tardis_open_house_b48d_2762\",\"name\":\"Destination 1\"}},{{\"sceneName\":\"pub_hollywood_hills_2d44_46fa\",\"name\":\"Destination 2\"}},{{\"sceneName\":\"stageset2_promo_c149_bd6e\",\"name\":\"Destination 3\"}}]}}</parameter>";
            }
        }
    }
}