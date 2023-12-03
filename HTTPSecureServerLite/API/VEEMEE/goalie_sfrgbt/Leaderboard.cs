namespace HTTPSecureServerLite.API.VEEMEE.goalie_sfrgbt
{
    public class Leaderboard
    {
        public static string? GetLeaderboardPOST(byte[]? PostData, string? ContentType, bool global)
        {
            string key = string.Empty;
            string psnid = string.Empty;
            string type = string.Empty;

            if (ContentType == "application/x-www-form-urlencoded" && PostData != null)
            {
                var data = CryptoSporidium.HTTPUtils.ExtractAndSortUrlEncodedPOSTData(PostData);
                key = data["key"];
                if (key != "d2us7A2EcU2PuBuz")
                {
                    CustomLogger.LoggerAccessor.LogError("[VEEMEE] - goalie_sfrgbt - Client tried to push invalid key! Invalidating request.");
                    return null;
                }
                psnid = data["psnid"];
                type = data["type"];

                string directoryPath = string.Empty;

                if (global)
                    directoryPath = $"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/goalie";
                else
                    directoryPath = $"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/sfrgbt";

                DateTime refdate = DateTime.Now; // We avoid race conditions by calculating it one time.

                switch (type)
                {
                    case "Today":
                        if (File.Exists($"{directoryPath}/leaderboard_{refdate:yyyy_MM_dd}.xml"))
                            return File.ReadAllText($"{directoryPath}/leaderboard_{refdate:yyyy_MM_dd}.xml");
                        break;
                    case "Yesterday":
                        if (File.Exists($"{directoryPath}/leaderboard_{refdate.AddDays(-1):yyyy_MM_dd}.xml"))
                            return File.ReadAllText($"{directoryPath}/leaderboard_{refdate.AddDays(-1):yyyy_MM_dd}.xml");
                        break;
                    case "All Time":
                        if (File.Exists($"{directoryPath}/leaderboard_alltime.xml"))
                            return File.ReadAllText($"{directoryPath}/leaderboard_alltime.xml");
                        break;
                }
            }

            return null;
        }
    }
}
