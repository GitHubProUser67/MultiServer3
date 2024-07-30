using System.IO;
using System.Globalization;
using System.Xml.Linq;
using System;
using System.Collections.Generic;

namespace WebAPIService.LeaderboardsService.VEEMEE
{
    public class GSScoreBoardData
    {
        public class ScoreboardEntry
        {
            public string psnid { get; set; }
            public int score { get; set; }
            public string duration { get; set; }
        }

        private static List<ScoreboardEntry> scoreboard = new List<ScoreboardEntry>();

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
            XElement xmlScoreboard = new XElement("leaderboard");

            foreach (var entry in scoreboard)
            {
                XElement xmlEntry = new XElement("player",
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
                directoryPath = $"{LeaderboardClass.APIPath}/VEEMEE/goalie";
                filePath = $"{LeaderboardClass.APIPath}/VEEMEE/goalie/leaderboard_alltime.xml";
            }
            else
            {
                directoryPath = $"{LeaderboardClass.APIPath}/VEEMEE/sfrgbt";
                filePath = $"{LeaderboardClass.APIPath}/VEEMEE/sfrgbt/leaderboard_alltime.xml";
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
                directoryPath = $"{LeaderboardClass.APIPath}/VEEMEE/goalie";
                filePath = $"{LeaderboardClass.APIPath}/VEEMEE/goalie/leaderboard_{date}.xml";
            }
            else
            {
                directoryPath = $"{LeaderboardClass.APIPath}/VEEMEE/sfrgbt";
                filePath = $"{LeaderboardClass.APIPath}/VEEMEE/sfrgbt/leaderboard_{date}.xml";
            }

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, ConvertScoreboardToXml());
            CustomLogger.LoggerAccessor.LogDebug($"[VEEMEE] - goalie_sfrgbt - scoreboard {date} XML updated.");
        }

        public static void SanityCheckLeaderboards(string directoryPath, DateTime thresholdDate)
        {
            if (Directory.Exists(directoryPath))
            {
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

                    foreach (FileInfo file in directoryInfo.GetFiles("leaderboard_*.xml"))
                    {
                        try
                        {
                            // Extract date from the file name
                            if (DateTime.TryParseExact(
                                    file.Name.Replace("leaderboard_", string.Empty).Replace(".xml", string.Empty),
                                    "yyyy_MM_dd",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out DateTime leaderboardDate)
                                && leaderboardDate < thresholdDate)
                            {
                                // If the leaderboard date is older than the threshold, delete the file
                                try
                                {
                                    file.Delete();

                                    CustomLogger.LoggerAccessor.LogInfo($"[VEEMEE] - goalie_sfrgbt - Removed outdated leaderboard: {file.Name}.");
                                }
                                catch (Exception e)
                                {
                                    CustomLogger.LoggerAccessor.LogInfo($"[VEEMEE] - goalie_sfrgbt - Error while removing leaderboard: {file.Name} (Exception: {e}).");
                                }
                            }
                        }
                        catch (ArgumentException e)
                        {
                            CustomLogger.LoggerAccessor.LogInfo($"[VEEMEE] - goalie_sfrgbt - Error while parsing leaderboard name: {file.Name} (ArgumentException: {e}).");
                        }
                    }
                }
                catch (Exception e)
                {
                    CustomLogger.LoggerAccessor.LogInfo($"[VEEMEE] - goalie_sfrgbt - Error while creating directoryInfo of path: {directoryPath} (Exception: {e}).");
                }
            }
        }
    }
}
