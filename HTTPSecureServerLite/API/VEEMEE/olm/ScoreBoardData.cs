using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace HTTPSecureServerLite.API.VEEMEE.olm
{
    public class ScoreBoardData
    {
        public class ScoreboardEntry
        {
            public string? psnid { get; set; }
            public int score { get; set; }
            public string? throws { get; set; }
        }

        private static List<ScoreboardEntry> scoreboard = new();

        public static void UpdateScoreBoard(string psnid, string newthrows, int newScore)
        {
            // Check if the player already exists in the scoreboard
            var existingEntry = scoreboard.Find(e => e.psnid != null && e.psnid.Equals(psnid, StringComparison.OrdinalIgnoreCase));

            if (existingEntry != null)
            {
                existingEntry.throws = newthrows;

                // If the new score is higher, update the existing entry
                if (newScore > existingEntry.score)
                    existingEntry.score = newScore;
            }
            else
            {
                // If the player is not in the scoreboard, add a new entry
                if (scoreboard.Count < 20)
                    scoreboard.Add(new ScoreboardEntry { psnid = psnid, score = newScore, throws = newthrows });
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
                    new XElement("throws", entry.throws ?? "0"));

                xmlScoreboard.Add(xmlEntry);
            }

            XElement xmlGameboard = new("games");

            foreach (var entry in scoreboard)
            {
                XElement xmlEntry = new("game",
                    new XElement("psnid", entry.psnid ?? "Voodooperson05"),
                    new XElement("score", entry.score),
                    new XElement("throws", entry.throws ?? "0"));

                xmlGameboard.Add(xmlEntry);
            }

            xmlScoreboard.Add(xmlGameboard.Elements());

            return xmlScoreboard.ToString();
        }

        public static void UpdateAllTimeScoreboardXml()
        {
            Directory.CreateDirectory($"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/olm");
            File.WriteAllText($"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/olm/leaderboard_alltime.xml", ConvertScoreboardToXml());
            CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - olm - scoreboard alltime XML updated.");
        }

        public static void UpdateWeeklyScoreboardXml(string date)
        {
            Directory.CreateDirectory($"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/olm");
            // Get all XML files in the scoreboard folder
            foreach (string file in Directory.GetFiles($"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/olm", "leaderboard_*.xml"))
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
                        if ((DateTime.Parse(date) - fileDateTime).TotalDays <= 7)
                        {
                            // Update the older scoreboard.
                            File.WriteAllText(file, ConvertScoreboardToXml());
                            CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - olm - Replaced old scoreboard file entry: {file}");
                            return;
                        }
                    }
                }
            }
            File.WriteAllText($"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/olm/leaderboard_{date}.xml", ConvertScoreboardToXml());
            CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - olm - scoreboard {date} XML updated.");
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
                        CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - olm - Removed outdated leaderboard: {file.Name}.");
                    }
                }
            }
        }

        public static void ScheduledUpdate(object? state)
        {
            SanityCheckLeaderboards($"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/olm", DateTime.Now.AddDays(-7));
        }
    }
}
