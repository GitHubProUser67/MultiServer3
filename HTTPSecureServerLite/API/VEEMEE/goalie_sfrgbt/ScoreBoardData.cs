using System.Globalization;
using System.Xml.Linq;

namespace HTTPSecureServerLite.API.VEEMEE.goalie_sfrgbt
{
    public class ScoreBoardData
    {
        public class ScoreboardEntry
        {
            public string? psnid { get; set; }
            public int score { get; set; }
            public string? duration { get; set; }
        }

        private static List<ScoreboardEntry> scoreboard = new();

        public static void UpdateScoreBoard(string psnid, string newDuration, int newScore)
        {
            // Check if the player already exists in the scoreboard
            var existingEntry = scoreboard.Find(e => e.psnid != null && e.psnid.Equals(psnid, StringComparison.OrdinalIgnoreCase));

            if (existingEntry != null)
            {
                existingEntry.duration = newDuration;

                // If the new score is higher, update the existing entry
                if (newScore > existingEntry.score)
                    existingEntry.score = newScore;
            }
            else
            {
                // If the player is not in the scoreboard, add a new entry
                if (scoreboard.Count < 20)
                    scoreboard.Add(new ScoreboardEntry { psnid = psnid, score = newScore, duration = newDuration });
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
                    new XElement("duration", entry.duration ?? "0"));

                xmlScoreboard.Add(xmlEntry);
            }

            return xmlScoreboard.ToString();
        }

        public static void UpdateAllTimeScoreboardXml(bool global)
        {
            string directoryPath = string.Empty;
            string filePath = string.Empty;

            if (global)
            {
                directoryPath = $"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/goalie";
                filePath = $"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/goalie/leaderboard_alltime.xml";
            }
            else
            {
                directoryPath = $"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/sfrgbt";
                filePath = $"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/sfrgbt/leaderboard_alltime.xml";
            }

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, ConvertScoreboardToXml());
            CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - goalie_sfrgbt - scoreboard alltime XML updated.");
        }

        public static void UpdateTodayScoreboardXml(bool global, string date)
        {
            string directoryPath = string.Empty;
            string filePath = string.Empty;

            if (global)
            {
                directoryPath = $"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/goalie";
                filePath = $"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/goalie/leaderboard_{date}.xml";
            }
            else
            {
                directoryPath = $"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/sfrgbt";
                filePath = $"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/sfrgbt/leaderboard_{date}.xml";
            }

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, ConvertScoreboardToXml());
            CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - goalie_sfrgbt - scoreboard {date} XML updated.");
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
                        CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - goalie_sfrgbt - Removed outdated leaderboard: {file.Name}.");
                    }
                }
            }
        }

        public static void ScheduledUpdate(object state)
        {
            SanityCheckLeaderboards($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/goalie", DateTime.Now.AddDays(-1));
            SanityCheckLeaderboards($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/sfrgbt", DateTime.Now.AddDays(-1));
        }
    }
}
