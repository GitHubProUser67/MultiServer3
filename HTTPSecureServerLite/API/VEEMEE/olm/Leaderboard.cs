using System.Text.RegularExpressions;

namespace HTTPSecureServerLite.API.VEEMEE.olm
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
                if (key != "KEqZKh3At4Ev")
                {
                    CustomLogger.LoggerAccessor.LogError("[VEEMEE] - olm - Client tried to push invalid key! Invalidating request.");
                    return null;
                }
                psnid = data["psnid"];

                DateTime refdate = DateTime.Now; // We avoid race conditions by calculating it one time.

                switch (mode)
                {
                    case 0:
                        if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/olm/leaderboard_{refdate:yyyy_MM_dd}.xml"))
                            return File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/olm/leaderboard_{refdate:yyyy_MM_dd}.xml");
                        break;
                    case 1:
                        // Get all XML files in the scoreboard folder
                        foreach (string file in Directory.GetFiles($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/olm", "leaderboard_*.xml"))
                        {
                            // Extract date from the filename
                            Match match = Regex.Match(file, @"leaderboard_(\d{4}_\d{2}_\d{2}).xml");
                            if (match.Success)
                            {
                                string fileDate = match.Groups[1].Value;

                                // Parse the file date
                                if (DateTime.TryParse(fileDate, out DateTime fileDateTime))
                                {
                                    // Check if the file is newer than 7 days
                                    double diff = (refdate - fileDateTime).TotalDays;
                                    if (diff <= 7)
                                        return File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/olm/leaderboard_{refdate.AddDays(-diff):yyyy_MM_dd}.xml");
                                }
                            }
                        }
                        break;
                }
            }

            return null;
        }
    }
}
