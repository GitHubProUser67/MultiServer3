using System.Xml.Linq;

namespace HTTPServer.API.JUGGERNAUT.clearasil
{
    public class ScoreBoardData
    {
        public class ScoreboardEntry
        {
            public string? Name { get; set; }
            public int Score { get; set; }
            public string? Time { get; set; }
        }

        private static List<ScoreboardEntry> scoreboard = new();

        public static void UpdateScore(string name, int newScore)
        {
            // Check if the player already exists in the scoreboard
            var existingEntry = scoreboard.Find(e => e.Name!= null && e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (existingEntry != null)
            {
                // If the new score is higher, update the existing entry
                if (newScore > existingEntry.Score)
                    existingEntry.Score = newScore;
            }
            else
            {
                // If the player is not in the scoreboard, add a new entry
                if (scoreboard.Count < 20)
                    scoreboard.Add(new ScoreboardEntry { Name = name, Score = newScore });
            }

            // Sort the scoreboard by score in descending order
            scoreboard.Sort((a, b) => b.Score.CompareTo(a.Score));

            // Trim the scoreboard to the top 20 entries
            if (scoreboard.Count > 20)
                scoreboard.RemoveRange(20, scoreboard.Count - 20);
        }

        public static void UpdateTime(string name, string newTime)
        {
            var existingEntry = scoreboard.Find(e => e.Name != null && e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (existingEntry != null)
                existingEntry.Time = newTime;
        }

        public static string ConvertScoreboardToXml()
        {
            XElement xmlScoreboard = new("xml");

            foreach (var entry in scoreboard)
            {
                XElement xmlEntry = new("entry",
                    new XElement("user", entry.Name ?? "Default"),
                    new XElement("score", entry.Score),
                    new XElement("time", entry.Time ?? "000"));

                xmlScoreboard.Add(xmlEntry);
            }

            return xmlScoreboard.ToString();
        }

        public static void UpdateScoreboardXml()
        {
            Directory.CreateDirectory($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/clearasil");
            File.WriteAllText($"{HTTPServerConfiguration.HTTPStaticFolder}/juggernaut/clearasil/scoreboard.xml", ConvertScoreboardToXml());
            CustomLogger.LoggerAccessor.LogDebug("[JUGGERNAUT] - Clearasil scoreboard XML updated.");
        }

        public static void ScheduledUpdate(object state)
        {
            UpdateScoreboardXml();
        }
    }
}
