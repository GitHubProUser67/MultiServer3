using System.IO;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;

namespace WebAPIService.LeaderboardsService.NDREAMS
{
    public class OrbrunnerScoreBoardData
    {
        public class ScoreboardEntry
        {
            public string? psnid { get; set; }
            public int score { get; set; }
        }

        private static List<ScoreboardEntry> scoreboard = new();

        public static void UpdateScoreBoard(string psnid, int newScore)
        {
            // Check if the player already exists in the scoreboard
            var existingEntry = scoreboard.Find(e => e.psnid != null && e.psnid.Equals(psnid, StringComparison.OrdinalIgnoreCase));

            if (existingEntry != null)
            {
                // If the new score is higher, update the existing entry
                if (newScore > existingEntry.score)
                    existingEntry.score = newScore;
            }
            else
            {
                // If the player is not in the scoreboard, add a new entry
                if (scoreboard.Count < 10)
                    scoreboard.Add(new ScoreboardEntry { psnid = psnid, score = newScore });
            }

            // Sort the scoreboard by score in descending order
            scoreboard.Sort((a, b) => b.score.CompareTo(a.score));

            // Trim the scoreboard to the top 10 entries
            if (scoreboard.Count > 10)
                scoreboard.RemoveRange(10, scoreboard.Count - 10);
        }

        public static (string?, int)? GetHighestScore()
        {
            if (scoreboard.Count == 0)
                return null;

            ScoreboardEntry highestEntry = scoreboard.OrderByDescending(x => x.score > 0)
                                        .ThenBy(x => x.score)
                                        .ToList()[0];

            return (highestEntry.psnid, highestEntry.score);
        }

        public static string ConvertScoreboardToText()
        {
            StringBuilder sb = new();

            foreach (var entry in scoreboard)
            {
                if (sb.Length == 0)
                    sb.Append(entry.psnid + "," + entry.score);
                else
                    sb.Append("," + entry.psnid + "," + entry.score);
            }

            return sb.ToString();
        }

        public static void UpdateTodayScoreboardXml(string date)
        {
            Directory.CreateDirectory($"{LeaderboardClass.APIPath}/NDREAMS/Aurora/Orbrunner");
            File.WriteAllText($"{LeaderboardClass.APIPath}/NDREAMS/Aurora/Orbrunner/leaderboard_{date}.txt", ConvertScoreboardToText());
            CustomLogger.LoggerAccessor.LogDebug($"[ndreams] - Orbrunner - scoreboard {date} TEXT updated.");
        }

        public static void SanityCheckLeaderboards(string directoryPath, DateTime thresholdDate)
        {
            if (Directory.Exists(directoryPath))
            {
                DirectoryInfo directoryInfo = new(directoryPath);

                foreach (FileInfo file in directoryInfo.GetFiles("leaderboard_*.txt"))
                {
                    // Extract date from the file name
                    if (DateTime.TryParseExact(
                            file.Name.Replace("leaderboard_", string.Empty).Replace(".txt", string.Empty),
                            "yyyy_MM_dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTime leaderboardDate)
                        && leaderboardDate < thresholdDate)
                    {
                        // If the leaderboard date is older than the threshold, delete the file
                        file.Delete();
                        CustomLogger.LoggerAccessor.LogDebug($"[ndreams] - Orbrunner - Removed outdated leaderboard: {file.Name}.");
                    }
                }
            }
        }
    }
}
