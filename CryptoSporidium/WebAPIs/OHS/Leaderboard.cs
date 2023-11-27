using CustomLogger;
using HttpMultipartParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CryptoSporidium.WebAPIs.OHS
{
    public class Leaderboard
    {
        public static string? Leaderboard_RequestByUsers(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
        {
            string? dataforohs = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);

                if (boundary != null)
                {
                    using (MemoryStream ms = new(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = RequestByUsers(JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game), directorypath);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = RequestByUsers(batchparams, directorypath);

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(dataforohs))
                    return null;
                else
                    return dataforohs;
            }
            else
            {
                if (string.IsNullOrEmpty(dataforohs))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {dataforohs} }}", game);
            }

            return dataforohs;
        }

        public static string? Leaderboard_RequestByRank(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
        {
            string? dataforohs = null;

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);

                if (boundary != null)
                {
                    using (MemoryStream ms = new(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        dataforohs = RequestByRank(JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game), directorypath);
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = RequestByRank(batchparams, directorypath);

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(dataforohs))
                    return null;
                else
                    return dataforohs;
            }
            else
            {
                if (string.IsNullOrEmpty(dataforohs))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {dataforohs} }}", game);
            }

            return dataforohs;
        }

        public static string? Leaderboard_Update(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
        {
            string? dataforohs = null;
            string writekey = "11111111";

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);

                if (boundary != null)
                {
                    using (MemoryStream ms = new(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        var dualresult = JaminProcessor.JaminDeFormatWithWriteKey(data.GetParameterValue("data"), true, game);
                        writekey = dualresult.Item1;
                        dataforohs = dualresult.Item2;
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;
            // TODO! writekey must be somewhere.

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    // Deserialize the JSON string
                    ScoreBoardUpdate? rootObject = JsonConvert.DeserializeObject<ScoreBoardUpdate>(dataforohs);

                    if (rootObject != null)
                    {
                        // Extract the values
                        string? user = rootObject.user;
                        int score = rootObject.score;
                        string? key = rootObject.key;

                        string scoreboardfile = directorypath + $"/scoreboard_{key}.json";

                        if (File.Exists(scoreboardfile))
                        {
                            string tempreader = File.ReadAllText(scoreboardfile);
                            if (tempreader != null && user != null)
                                dataforohs = UpdateScoreboard(tempreader, user, score, scoreboardfile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Leaderboard] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (string.IsNullOrEmpty(dataforohs))
                    return null;
                else
                    return $"{{ [\"writeKey\"] = \"{writekey}\", [\"entries\"] = {dataforohs} }}";
            }
            else
            {
                if (string.IsNullOrEmpty(dataforohs))
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [\"writeKey\"] = \"{writekey}\", [\"entries\"] = {dataforohs} }} }}", game);
            }

            return dataforohs;
        }

        public static string? Leaderboard_UpdatesSameEntry(byte[] PostData, string ContentType, string directorypath, string batchparams, int game)
        {
            string? dataforohs = null;
            string writekey = "11111111";

            if (string.IsNullOrEmpty(batchparams))
            {
                string? boundary = HTTPUtils.ExtractBoundary(ContentType);

                if (boundary != null)
                {
                    using (MemoryStream ms = new(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                        var dualresult = JaminProcessor.JaminDeFormatWithWriteKey(data.GetParameterValue("data"), true, game);
                        writekey = dualresult.Item1;
                        dataforohs = dualresult.Item2;
                        ms.Flush();
                    }
                }
            }
            else
                dataforohs = batchparams;
            // TODO! writekey must be somewhere.

            StringBuilder? resultBuilder = new();

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    // Deserialize the JSON string
                    ScoreBoardUpdateSameEntry? rootObject = JsonConvert.DeserializeObject<ScoreBoardUpdateSameEntry>(dataforohs);

                    if (rootObject != null)
                    {
                        // Extract the values
                        string? user = rootObject.user;
                        int score = rootObject.score;
                        string[]? keys = rootObject.keys;

                        if (keys != null)
                        {
                            foreach (var key in keys)
                            {
                                string scoreboardfile = directorypath + $"/scoreboard_{key}.json";

                                if (File.Exists(scoreboardfile))
                                {
                                    string? tempreader = File.ReadAllText(scoreboardfile);

                                    if (tempreader != null && user != null)
                                    {
                                        if (resultBuilder.Length == 0)
                                            resultBuilder.Append(UpdateScoreboard(tempreader, user, score, scoreboardfile));
                                        else
                                            resultBuilder.Append(", " + UpdateScoreboard(tempreader, user, score, scoreboardfile));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Leaderboard] - Json Format Error - {ex}");
            }

            string res = resultBuilder.ToString();

            resultBuilder = null;

            if (!string.IsNullOrEmpty(batchparams))
            {
                if (res.Length == 0)
                    return null;
                else
                    return $"{{ [\"writeKey\"] = \"{writekey}\", [\"entries\"] = {res} }}";
            }
            else
            {
                if (res.Length == 0)
                    dataforohs = JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", game);
                else
                    dataforohs = JaminProcessor.JaminFormat($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [\"writeKey\"] = \"{writekey}\", [\"entries\"] = {res} }} }}", game);
            }

            return dataforohs;
        }

        public static string UpdateScoreboard(string json, string nameToUpdate, int newScore, string scoreboardfile)
        {
            try
            {
                bool noedits = false;

                int newIndex = -1;

                string scoreboarddata = string.Empty;

                // Step 1: Deserialize JSON string into a C# object
                Scoreboard? scoreboard = JsonConvert.DeserializeObject<Scoreboard>(json);

                if (scoreboard != null && scoreboard.Entries != null)
                {
                    for (int i = 0; i < scoreboard.Entries.Count; i++)
                    {
                        var entry = scoreboard.Entries[i];

                        if (newScore > entry.Score)
                        {
                            newIndex = i;
                            break;
                        }
                    }

                    // Step 2: Add the new entry at the appropriate position
                    if (newIndex >= 0)
                    {
                        scoreboard.Entries.Insert(newIndex, new ScoreboardEntry
                        {
                            Name = nameToUpdate,
                            Score = newScore
                        });

                        // Step 3: Calculate the number of entries to maintain based on existing entries
                        int maxEntries = scoreboard.Entries.Count;

                        // Step 4: Remove any excess entries if the scoreboard exceeds the calculated number of entries
                        while (scoreboard.Entries.Count >= maxEntries)
                        {
                            scoreboard.Entries.RemoveAt(scoreboard.Entries.Count - 1);
                        }
                    }
                    else
                        noedits = true;

                    if (!noedits)
                    {
                        // Step 5: Sort the entries based on the new scores
                        scoreboard.Entries.Sort((a, b) => b.Score.CompareTo(a.Score));

                        // Step 6: Adjust the ranks accordingly
                        for (int i = 0; i < scoreboard.Entries.Count; i++)
                        {
                            scoreboard.Entries[i].Rank = i + 1;
                        }

                        // Step 7: Serialize the updated object back to a JSON string
                        string updatedscoreboard = JsonConvert.SerializeObject(scoreboard, Formatting.Indented);

                        if (!string.IsNullOrEmpty(updatedscoreboard))
                            File.WriteAllText(scoreboardfile, updatedscoreboard);
                    }
                }

                if (noedits)
                    scoreboarddata = json;
                else
                    scoreboarddata = File.ReadAllText(scoreboardfile);

                // Step 1: Parse JSON to C# objects
                JObject? jsonData = JsonConvert.DeserializeObject<JObject>(scoreboarddata);

                if (jsonData != null)
                {
                    JToken? Entries = jsonData["Entries"];

                    if (Entries != null)
                    {
                        var entries = Entries.ToObject<List<ScoreboardEntry>>();

                        // Step 2: Convert to Lua table structure
                        var luaTable = new Dictionary<int, Dictionary<string, object>>();

                        if (entries != null)
                        {
                            foreach (var entry in entries)
                            {
                                var rankData = new Dictionary<string, object>
                            {
                                { "[\"user\"]", $"\"{entry.Name}\"" }, // Enclose string in double quotes and put it inside the brackets
                                { "[\"score\"]", $"\"{entry.Score}\"" } // For numbers, no need to enclose in quotes and put it inside the brackets
                            };

                                luaTable.Add(entry.Rank, rankData);
                            }

                            // Step 3: Format the Lua table as a string using regex
                            var luaString = FormatScoreBoardLuaTable(luaTable);

                            return luaString;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogWarn($"[Leaderboard] - UpdateScoreboard failed - {ex}");
            }

            return "{ }";
        }

        public static string RequestByUsers(string? jsontable, string scoreboardpath)
        {
            string returnvalue = "{ [\"entries\"] = { }, [\"user\"] = { [\"score\"] = 0 } }";

            try
            {
                string scoreboardfile = string.Empty;

                if (Directory.Exists(scoreboardpath))
                {
                    if (!string.IsNullOrEmpty(jsontable))
                    {
                        ScoreBoardUsersRequest? data = JsonConvert.DeserializeObject<ScoreBoardUsersRequest>(jsontable);

                        if (data != null && data.Users != null)
                        {
                            scoreboardfile = scoreboardpath + $"/scoreboard_{data.Key}.json";

                            if (File.Exists(scoreboardfile))
                            {
                                StringBuilder? resultBuilder = new();

                                foreach (string user in data.Users)
                                {
                                    string? scoreboarddata = File.ReadAllText(scoreboardfile);

                                    if (!string.IsNullOrEmpty(scoreboarddata))
                                    {
                                        JObject? jsonData = JsonConvert.DeserializeObject<JObject>(scoreboarddata);

                                        if (jsonData != null)
                                        {
                                            JToken? Entries = jsonData["Entries"];

                                            if (Entries != null)
                                            {
                                                var entries = Entries.ToObject<List<ScoreboardEntry>>();

                                                if (entries != null)
                                                {
                                                    foreach (var entry in entries)
                                                    {
                                                        if (entry.Name == user)
                                                        {
                                                            if (entry.Score != 0)
                                                            {
                                                                if (resultBuilder.Length == 0)
                                                                    resultBuilder.Append($"[\"user\"] = {{ [\"score\"] = {entry.Score.ToString()} }}");
                                                                else
                                                                    resultBuilder.Append($", [\"user\"] = {{ [\"score\"] = {entry.Score.ToString()} }}");
                                                            }
                                                            else
                                                            {
                                                                if (resultBuilder.Length == 0)
                                                                    resultBuilder.Append($"[\"user\"] = {{ [\"score\"] = 0 }}");
                                                                else
                                                                    resultBuilder.Append($", [\"user\"] = {{ [\"score\"] = 0 }}");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (resultBuilder.Length == 0)
                                    resultBuilder.Append($"[\"user\"] = {{ [\"score\"] = 0 }}");

                                string res = resultBuilder.ToString();

                                resultBuilder = null;

                                // Step 1: Parse JSON to C# objects
                                JObject? jsonDatascore = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(scoreboardfile));

                                if (jsonDatascore != null)
                                {
                                    JToken? Entries = jsonDatascore["Entries"];

                                    if (Entries != null)
                                    {
                                        var scoreentries = Entries.ToObject<List<ScoreboardEntry>>();

                                        if (scoreentries != null)
                                        {
                                            // Step 2: Convert to Lua table structure
                                            var luaTable = new Dictionary<int, Dictionary<string, object>>();

                                            int i = 1;

                                            foreach (var entry in scoreentries)
                                            {
                                                if (i >= 1)
                                                {
                                                    var rankData = new Dictionary<string, object>
                                                    {
                                                        { "[\"user\"]", $"\"{entry.Name}\"" },
                                                        { "[\"score\"]", $"\"{entry.Score}\"" }
                                                    };

                                                    luaTable.Add(entry.Rank, rankData);
                                                }
                                            }

                                            // Step 3: Format the Lua table as a string using regex
                                            string luaString = FormatScoreBoardLuaTable(luaTable);

                                            returnvalue = "{ [\"entries\"] = " + luaString + ", " + res + " }";
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogWarn($"[Leaderboard] - RequestByUsers failed - {ex}");
            }

            return returnvalue;
        }

        public static string? RequestByRank(string? jsontable, string scoreboardpath)
        {
            // Sometimes requestbyrank was used to create the scoreboard.

            try
            {
                int numEntries = 0;

                int start = 1;

                string? user = null;

                string? key = null;

                if (!string.IsNullOrEmpty(jsontable))
                {
                    JObject? jsonDatainit = GetJsonData(jsontable);

                    if (jsonDatainit != null)
                    {
                        JToken? numEntriesToken = jsonDatainit["numEntries"];
                        if (numEntriesToken != null)
                            numEntries = (int)numEntriesToken;

                        JToken? startToken = jsonDatainit["start"];
                        if (startToken != null)
                            start = (int)startToken;

                        user = (string?)jsonDatainit["user"];
                        key = (string?)jsonDatainit["key"];
                    }

                    if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(key))
                        return null;

                    string scoreboardfile = scoreboardpath + $"/scoreboard_{key}.json";

                    if (!File.Exists(scoreboardfile))
                    {
                        Scoreboard? scoreboard = GenerateSampleScoreboard(numEntries);
                        File.WriteAllText(scoreboardfile, JsonConvert.SerializeObject(scoreboard, Formatting.Indented));
                        scoreboard = null;
                    }

                    scoreboardfile = File.ReadAllText(scoreboardfile);

                    if (!string.IsNullOrEmpty(scoreboardfile))
                    {
                        // Step 1: Parse JSON to C# objects
                        JObject? jsonData = JsonConvert.DeserializeObject<JObject>(scoreboardfile);

                        if (jsonData != null)
                        {
                            JToken? Entries = jsonData["Entries"];

                            if (Entries != null)
                            {
                                var entries = Entries.ToObject<List<ScoreboardEntry>>();

                                if (entries != null)
                                {
                                    // Step 2: Convert to Lua table structure
                                    var luaTable = new Dictionary<int, Dictionary<string, object>>();

                                    int i = 1;

                                    int scoreforuser = 0;

                                    foreach (var entry in entries)
                                    {
                                        if (i >= start)
                                        {
                                            var rankData = new Dictionary<string, object>
                                            {
                                                { "[\"user\"]", $"\"{entry.Name}\"" }, // Enclose string in double quotes and put it inside the brackets
                                                { "[\"score\"]", $"\"{entry.Score}\"" } // For numbers, no need to enclose in quotes and put it inside the brackets
                                            };

                                            luaTable.Add(entry.Rank, rankData);

                                            if (entry.Name == user)
                                                scoreforuser = entry.Score;
                                        }
                                    }

                                    // Step 3: Format the Lua table as a string using regex
                                    var luaString = FormatScoreBoardLuaTable(luaTable);

                                    return $"{{ [\"user\"] = {{ [\"score\"] = {scoreforuser} }}, [\"entries\"] = {luaString} }}";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogWarn($"[Leaderboard] - RequestByRank failed - {ex}");
            }

            return $"{{ [\"user\"] = {{ [\"score\"] = 0 }}, [\"entries\"] = {{ }} }}";
        }

        public static Scoreboard GenerateSampleScoreboard(int numEntries)
        {
            Scoreboard scoreboard = new();
            Random? random = new();

            scoreboard.Entries = new List<ScoreboardEntry>();

            for (int i = 1; i <= numEntries; i++)
            {
                string playerName = ScoreboardNameGenerator.GenerateRandomName();
                int score = random.Next(100, 1000); // Generate a random score between 100 and 999
                scoreboard.Entries.Add(new ScoreboardEntry { Name = playerName, Score = score });
            }

            // Sort the entries by score in descending order
            scoreboard.Entries.Sort((entry1, entry2) => entry2.Score.CompareTo(entry1.Score));

            // Assign ranks based on the sorted order
            for (int i = 0; i < scoreboard.Entries.Count; i++)
            {
                scoreboard.Entries[i].Rank = i + 1;
            }

            random = null;

            return scoreboard;
        }

        // Helper method to format the Lua table as a string
        public static string FormatScoreBoardLuaTable(Dictionary<int, Dictionary<string, object>> luaTable)
        {
            var luaString = "{\n";
            foreach (var rankData in luaTable)
            {
                luaString += $"    [{rankData.Key}] = {{\n";
                foreach (var kvp in rankData.Value)
                {
                    luaString += $"        {kvp.Key} = {kvp.Value},\n"; // We already formatted the keys and values accordingly
                }
                luaString = RemoveTrailingComma(luaString); // Remove the trailing comma for the last element in each number category
                luaString += "    },\n";
            }
            luaString += "}";

            // Remove trailing commas
            luaString = RemoveTrailingComma(luaString);

            return luaString;
        }

        // Helper method to remove the trailing comma from the Lua table string
        public static string RemoveTrailingComma(string input)
        {
            return Regex.Replace(input, @",(\s*})|(\s*]\s*})", "$1$2");
        }

        public static JObject? GetJsonData(string json)
        {
            try
            {
                return JObject.Parse(json);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"Error while parsing JSON: {ex}");
            }

            return null;
        }

        public class ScoreboardNameGenerator
        {
            private static Random random = new();

            // List of silly French-sounding words to be used in the names
            private static string[] sillyFrenchWords = { "Croissant", "Baguette", "Fougasse", "TarteAuFromage", "Tabernack", "UnePetiteContine", "ChuckNorris", "Pamplemousse", "JimCarrey", "Fromage" };

            public static string GenerateRandomName()
            {
                return sillyFrenchWords[random.Next(0, sillyFrenchWords.Length)];
            }
        }

        public class ScoreboardEntry
        {
            public string? Name { get; set; }
            public int Score { get; set; }
            public int Rank { get; set; } // Add this property to hold the rank
        }

        public class Scoreboard
        {
            public List<ScoreboardEntry>? Entries { get; set; }
        }

        public class ScoreBoardUpdateSameEntry
        {
            public string? user { get; set; }
            public string[]? keys { get; set; }
            public int score { get; set; }
            public object[]? value { get; set; }
        }

        public class ScoreBoardUpdate
        {
            public string? user { get; set; }
            public string? key { get; set; }
            public int score { get; set; }
            public object[]? value { get; set; }
        }

        public class ScoreBoardUsersRequest
        {
            public string[]? Users { get; set; }
            public string? Key { get; set; }
        }
    }
}
