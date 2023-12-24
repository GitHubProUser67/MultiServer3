namespace HTTPSecureServerLite.API.VEEMEE.gofish
{
    public class Leaderboard
    {
        public static string? GetLeaderboardPOST(byte[]? PostData, string? ContentType, int mode)
        {
            string key = string.Empty;
            string psnid = string.Empty;

            if (ContentType == "application/x-www-form-urlencoded" && PostData != null)
            {
                var data = BackendProject.HTTPUtils.ExtractAndSortUrlEncodedPOSTData(PostData);
                key = data["key"];
                if (key != "tHeHuYUmuDa54qur")
                {
                    CustomLogger.LoggerAccessor.LogError("[VEEMEE] - gofish - Client tried to push invalid key! Invalidating request.");
                    return null;
                }
                psnid = data["psnid"];

                DateTime refdate = DateTime.Now; // We avoid race conditions by calculating it one time.

                switch (mode)
                {
                    case 0:
                        if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/gofish/leaderboard_{refdate:yyyy_MM_dd}.xml"))
                            return File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/gofish/leaderboard_{refdate:yyyy_MM_dd}.xml");
                        break;
                    case 1:
                        if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/gofish/leaderboard_{refdate.AddDays(-1):yyyy_MM_dd}.xml"))
                            return File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/gofish/leaderboard_{refdate.AddDays(-1):yyyy_MM_dd}.xml");
                        break;
                    case 2:
                        if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/gofish/leaderboard_alltime.xml"))
                            return File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/gofish/leaderboard_alltime.xml");
                        break;
                }
            }

            return null;
        }
    }
}
