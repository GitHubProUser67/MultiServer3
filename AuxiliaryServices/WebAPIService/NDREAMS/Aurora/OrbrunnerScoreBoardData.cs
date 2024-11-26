using System.IO;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;

namespace WebAPIService.NDREAMS.Aurora
{
    public class OrbrunnerScoreBoardData
    {
        private static object _Lock = new object();

        public class ScoreboardEntry
        {
            public string psnid { get; set; }
            public int score { get; set; }
        }

        private static List<ScoreboardEntry> scoreboard = new List<ScoreboardEntry>();

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

        public static (string, int)? GetHighestScore()
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
            StringBuilder sb = new StringBuilder();

            foreach (var entry in scoreboard)
            {
                if (sb.Length == 0)
                    sb.Append(entry.psnid + "," + entry.score);
                else
                    sb.Append("," + entry.psnid + "," + entry.score);
            }

            return sb.ToString();
        }

        public static void UpdateTodayScoreboardXml(string apiPath, string date)
        {
            lock (_Lock)
            {
                Directory.CreateDirectory($"{apiPath}/NDREAMS/Aurora/Orbrunner");
                File.WriteAllText($"{apiPath}/NDREAMS/Aurora/Orbrunner/leaderboard_{date}.txt", ConvertScoreboardToText());
                CustomLogger.LoggerAccessor.LogDebug($"[ndreams] - Orbrunner - scoreboard {date} TEXT updated.");
            }
        }
    }
}
