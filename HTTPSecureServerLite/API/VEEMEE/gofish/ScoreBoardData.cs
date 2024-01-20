using System.Globalization;
using System.Xml.Linq;

namespace HTTPSecureServerLite.API.VEEMEE.gofish
{
    public class ScoreBoardData
    {
        public class ScoreboardEntry
        {
            public string? psnid { get; set; }
            public int score { get; set; }
            public string? fishcount { get; set; }
            public string? biggestfishweight { get; set; }
            public string? totalfishweight { get; set; }
        }

        private static List<ScoreboardEntry> scoreboard = new();

        public static void UpdateScoreBoard(string psnid, string newFishcount, string newBiggestfishweight, string newTotalfishweight, int newScore)
        {
            // Check if the player already exists in the scoreboard
            var existingEntry = scoreboard.Find(e => e.psnid != null && e.psnid.Equals(psnid, StringComparison.OrdinalIgnoreCase));

            if (existingEntry != null)
            {
                existingEntry.fishcount = newFishcount;
                existingEntry.biggestfishweight = newBiggestfishweight;
                existingEntry.totalfishweight = newTotalfishweight;

                // If the new score is higher, update the existing entry
                if (newScore > existingEntry.score)
                    existingEntry.score = newScore;
            }
            else
            {
                // If the player is not in the scoreboard, add a new entry
                if (scoreboard.Count < 20)
                    scoreboard.Add(new ScoreboardEntry { psnid = psnid, score = newScore, fishcount = newFishcount, biggestfishweight = newBiggestfishweight, totalfishweight = newTotalfishweight });
            }

            // Sort the scoreboard by score in descending order
            scoreboard.Sort((a, b) => b.score.CompareTo(a.score));

            // Trim the scoreboard to the top 20 entries
            if (scoreboard.Count > 20)
                scoreboard.RemoveRange(20, scoreboard.Count - 20);
        }

        public static string ConvertScoreboardToXml()
        {
            XElement xmlScoreboard = new("leaderboard");

            foreach (var entry in scoreboard)
            {
                XElement xmlEntry = new("player",
                    new XElement("psnid", entry.psnid ?? "Voodooperson05"),
                    new XElement("score", entry.score),
                    new XElement("fishcount", entry.fishcount ?? "0"),
                    new XElement("biggestfishweight", entry.biggestfishweight ?? "0"),
                    new XElement("totalfishweight", entry.totalfishweight ?? "0"));

                xmlScoreboard.Add(xmlEntry);
            }

            XElement xmlGameboard = new("games");

            foreach (var entry in scoreboard)
            {
                XElement xmlEntry = new("game",
                    new XElement("psnid", entry.psnid ?? "Voodooperson05"),
                    new XElement("score", entry.score),
                    new XElement("fishcount", entry.fishcount ?? "0"),
                    new XElement("biggestfishweight", entry.biggestfishweight ?? "0"),
                    new XElement("totalfishweight", entry.totalfishweight ?? "0"));

                xmlGameboard.Add(xmlEntry);
            }

            xmlScoreboard.Add(xmlGameboard.Elements());

            return xmlScoreboard.ToString();
        }

        public static void UpdateAllTimeScoreboardXml()
        {
            Directory.CreateDirectory($"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/gofish");
            File.WriteAllText($"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/gofish/leaderboard_alltime.xml", ConvertScoreboardToXml());
            CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - gofish - scoreboard alltime XML updated.");
        }

        public static void UpdateTodayScoreboardXml(string date)
        {
            Directory.CreateDirectory($"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/gofish");
            File.WriteAllText($"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/gofish/leaderboard_{date}.xml", ConvertScoreboardToXml());
            CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - gofish - scoreboard {date} XML updated.");
        }

        public static void SanityCheckLeaderboards(string directoryPath, DateTime thresholdDate)
        {
            if (Directory.Exists(directoryPath))
            {
                DirectoryInfo directoryInfo = new(directoryPath);

                foreach (FileInfo file in directoryInfo.GetFiles("leaderboard_*.xml"))
                {
                    // Extract date from the file name
                    if (DateTime.TryParseExact(
                            file.Name.Replace("leaderboard_", "").Replace(".xml", ""),
                            "yyyy_MM_dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTime leaderboardDate)
                        && leaderboardDate < thresholdDate)
                    {
                        // If the leaderboard date is older than the threshold, delete the file
                        file.Delete();
                        CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - gofish - Removed outdated leaderboard: {file.Name}.");
                    }
                }
            }
        }

        public static void ScheduledUpdate(object? state)
        {
            SanityCheckLeaderboards($"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/gofish", DateTime.Now.AddDays(-1));
        }
    }
}
