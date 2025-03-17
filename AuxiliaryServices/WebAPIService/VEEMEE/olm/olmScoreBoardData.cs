using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Collections.Generic;
using System;

namespace WebAPIService.VEEMEE.olm
{
    public class olmScoreBoardData
    {
        private static object _Lock = new object();

        public class ScoreboardEntry
        {
            public string psnid { get; set; }
            public float score { get; set; }
            public string throws { get; set; }
        }

        private static List<ScoreboardEntry> scoreboard = new List<ScoreboardEntry>();

        public static void UpdateScoreBoard(string psnid, string newthrows, float newScore)
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
            XElement xmlScoreboard = new XElement("leaderboard");

            foreach (var entry in scoreboard)
            {
                XElement xmlEntry = new XElement("player",
                    new XElement("psnid", entry.psnid ?? "Voodooperson05"),
                    new XElement("score", entry.score.ToString().Replace(",", ".")),
                    new XElement("throws", entry.throws ?? "0"));

                xmlScoreboard.Add(xmlEntry);
            }

            XElement xmlGameboard = new XElement("games");

            foreach (var entry in scoreboard)
            {
                XElement xmlEntry = new XElement("game",
                    new XElement("psnid", entry.psnid ?? "Voodooperson05"),
                    new XElement("score", entry.score.ToString().Replace(",", ".")),
                    new XElement("throws", entry.throws ?? "0"));

                xmlGameboard.Add(xmlEntry);
            }

            xmlScoreboard.Add(xmlGameboard.Elements());

            return xmlScoreboard.ToString();
        }

        public static void UpdateAllTimeScoreboardXml(string apiPath)
        {
            Directory.CreateDirectory($"{apiPath}/VEEMEE/olm");
            File.WriteAllText($"{apiPath}/VEEMEE/olm/leaderboard_alltime.xml", ConvertScoreboardToXml());
            CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - olm - scoreboard alltime XML updated.");
        }

        public static void UpdateWeeklyScoreboardXml(string apiPath, string date)
        {
            lock (_Lock)
            {
                Directory.CreateDirectory($"{apiPath}/VEEMEE/olm");

                // Get all XML files in the scoreboard folder
                foreach (string file in Directory.GetFiles($"{apiPath}/VEEMEE/olm", "leaderboard_*.xml"))
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

                File.WriteAllText($"{apiPath}/VEEMEE/olm/leaderboard_{date}.xml", ConvertScoreboardToXml());
                CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - olm - scoreboard {date} XML updated.");
            }
        }
    }
}
