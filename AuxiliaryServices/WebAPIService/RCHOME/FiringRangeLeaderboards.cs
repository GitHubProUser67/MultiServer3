using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WebAPIService.RCHOME
{
    internal class FiringRangeLeaderboards
    {
        private object _Lock = new object();

        public class FiringRangeScoreboardEntry
        {
            public string psnid { get; set; }
            public int score { get; set; }
        }

        private List<FiringRangeScoreboardEntry> scoreboard = new List<FiringRangeScoreboardEntry>();

        public void UpdateScoreBoard(string psnid, int newScore)
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
                    scoreboard.Add(new FiringRangeScoreboardEntry { psnid = psnid, score = newScore });
            }

            // Sort the scoreboard by score in descending order
            scoreboard.Sort((a, b) => b.score.CompareTo(a.score));

            // Trim the scoreboard to the top 10 entries
            if (scoreboard.Count > 10)
                scoreboard.RemoveRange(10, scoreboard.Count - 10);
        }

        public string ConvertScoreboardToXml()
        {
            XElement xmlScoreboard = new XElement("data");

            foreach (var entry in scoreboard)
            {
                XElement xmlEntry = new XElement("row",
                    new XElement("c", entry.psnid ?? "Voodooperson05"),
                    new XElement("c", entry.score.ToString()));

                xmlScoreboard.Add(xmlEntry);
            }

            return xmlScoreboard.ToString();
        }

        public void UpdateScoreboardXml(string apiPath, string gameName)
        {
            string directoryPath = $"{apiPath}/RCHOME/{gameName}";
            string filePath = $"{apiPath}/RCHOME/{gameName}/leaderboard.xml";

            lock (_Lock)
            {
                Directory.CreateDirectory(directoryPath);
                File.WriteAllText(filePath, ConvertScoreboardToXml());
                CustomLogger.LoggerAccessor.LogDebug($"[RCHOME] - {gameName} - scoreboard XML updated.");
            }
        }
    }
}
