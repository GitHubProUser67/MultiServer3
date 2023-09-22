using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;
using MultiServer.HTTPService;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.OHS
{
    public class OHSProcessor
    {
        public static string JaminDeFormat(string dataforohs, bool hashed)
        {
            try
            {
                // Execute the Lua script and get the result
                object[] returnValues = null;

                if (hashed)
                    returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_FORMATEDJAMINVALUE_HERE", dataforohs.Substring(8))); // We remove the hash.
                else
                    returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_FORMATEDJAMINVALUE_HERE", dataforohs));

                if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                    return returnValues[0].ToString();
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogWarn($"[OHSProcessor] - JaminFormat failed - {ex}");
            }

            return null;
        }

        public static string JaminFormat(string dataforohs)
        {
            try
            {
                // Execute the Lua script and get the result
                object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", dataforohs));

                if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                    return returnValues[0].ToString();
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogWarn($"[OHSProcessor] - JaminDeFormat failed - {ex}");
            }

            return null;
        }

        public static string UpdateScoreboard(string json, string nameToUpdate, int newScore, string scoreboardfile)
        {
            try
            {
                bool noedits = false;

                string scoreboarddata = string.Empty;

                // Step 1: Deserialize JSON string into a C# object
                Scoreboard scoreboard = JsonConvert.DeserializeObject<Scoreboard>(json);

                int newIndex = -1;

                if (scoreboard != null)
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

                        FileHelper.CryptoWriteAsync(scoreboardfile, HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(updatedscoreboard), false).Wait();
                    }
                }

                if (noedits)
                    scoreboarddata = json;
                else
                    scoreboarddata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(scoreboardfile, HTTPPrivateKey.HTTPPrivatekey));

                // Step 1: Parse JSON to C# objects
                var jsonData = JsonConvert.DeserializeObject<JObject>(scoreboarddata);

                if (jsonData != null)
                {
                    JToken Entries = jsonData["Entries"];

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
                ServerConfiguration.LogWarn($"[OHSProcessor] - UpdateScoreboard failed - {ex}");
            }
            return "{ }";
        }

        public static string RequestByUsers(string jsontable, string scoreboardpath)
        {
            try
            {
                string scoreboardfile = "";

                string returnvalue = "{ [\"entries\"] = { }, [\"user\"] = { [\"score\"] = 0 } }";

                if (Directory.Exists(scoreboardpath))
                {
                    ScoreBoardUsersRequest data = JsonConvert.DeserializeObject<ScoreBoardUsersRequest>(jsontable);

                    if (data != null && data.Users != null)
                    {
                        scoreboardfile = scoreboardpath + $"/scoreboard_{data.Key}.json";

                        if (File.Exists(scoreboardfile))
                        {
                            StringBuilder resultBuilder = new StringBuilder();

                            foreach (string user in data.Users)
                            {
                                string scoreboarddata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(scoreboardfile, HTTPPrivateKey.HTTPPrivatekey));

                                if (scoreboarddata != null)
                                {
                                    var jsonData = JsonConvert.DeserializeObject<JObject>(scoreboarddata);

                                    if (jsonData != null)
                                    {
                                        JToken Entries = jsonData["Entries"];

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
                                                                resultBuilder.Append($"[\"user\"] = {{ [\"score\"] = {0.ToString()} }}");
                                                            else
                                                                resultBuilder.Append($", [\"user\"] = {{ [\"score\"] = {0.ToString()} }}");
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

                            // Step 1: Parse JSON to C# objects
                            var jsonDatascore = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(scoreboardfile, HTTPPrivateKey.HTTPPrivatekey)));

                            if (jsonDatascore != null)
                            {
                                JToken Entries = jsonDatascore["Entries"];

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

                                        returnvalue = "{ [\"entries\"] = " + luaString + ", " + resultBuilder.ToString() + " }";
                                    }
                                }
                            }
                        }
                    }
                }

                return returnvalue;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogWarn($"[OHSProcessor] - RequestByUsers failed - {ex}");
            }

            return "{ [\"entries\"] = { }, [\"user\"] = { [\"score\"] = 0 } }";
        }

        public static string RequestByRank(string jsontable, string scoreboardpath)
        {
            // Sometimes requestbyrank was used to create the scoreboard.

            try
            {
                int numEntries = 0;

                int start = 1;

                string user = string.Empty;

                string key = string.Empty;

                JObject jsonDatainit = GetJsonData(jsontable);

                if (jsonDatainit != null)
                {
                    JToken numEntriesToken = jsonDatainit["numEntries"];
                    if (numEntriesToken != null)
                        numEntries = (int)numEntriesToken;

                    JToken startToken = jsonDatainit["start"];
                    if (startToken != null)
                        start = (int)startToken;

                    user = (string)jsonDatainit["user"];
                    key = (string)jsonDatainit["key"];
                }
                else
                    return string.Empty;

                if (user == null || user == string.Empty)
                    return string.Empty;

                string scoreboardfile = scoreboardpath + $"/scoreboard_{key}.json";

                if (!File.Exists(scoreboardfile))
                {
                    Scoreboard scoreboard = GenerateSampleScoreboard(numEntries);

                    FileHelper.CryptoWriteAsync(scoreboardfile, HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(scoreboard, Formatting.Indented)), false).Wait();
                }

                scoreboardfile = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(scoreboardfile, HTTPPrivateKey.HTTPPrivatekey));

                if (scoreboardfile != null)
                {
                    // Step 1: Parse JSON to C# objects
                    JObject jsonData = JsonConvert.DeserializeObject<JObject>(scoreboardfile);

                    if (jsonData != null)
                    {
                        JToken Entries = jsonData["Entries"];

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

                                return $"{{ [\"user\"] = {{ [\"score\"] = {scoreforuser} }}, [\"entries\"] = " + luaString + " }";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogWarn($"[OHSProcessor] - RequestByRank failed - {ex}");
            }

            return $"{{ [\"user\"] = {{ [\"score\"] = 0 }}, [\"entries\"] = {{ }} }}";
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

        // Function to convert a JToken to a Lua table-like string
        public static string ConvertToLuaTable(JToken token, bool nested, string propertyName = null)
        {
            int arrayIndex = 1;

            if (nested)
            {
                if (token.Type == JTokenType.Object)
                {
                    StringBuilder resultBuilder = new StringBuilder("{ ");

                    foreach (JProperty property in token.Children<JProperty>())
                    {
                        if (property.Value.Type == JTokenType.String)
                            // Handle string type
                            resultBuilder.Append($"[\"{property.Name}\"] = \"{property.Value}\", ");
                        else if (property.Value.Type == JTokenType.Integer)
                            // Handle integer type
                            resultBuilder.Append($"[\"{property.Name}\"] = {property.Value}, ");
                        else if (property.Value.Type == JTokenType.Array)
                        {
                            // Handle array type
                            resultBuilder.Append($"[\"{property.Name}\"] = {{ ");
                            foreach (JToken arrayItem in property.Value)
                            {
                                resultBuilder.Append($"{ConvertToLuaTable(arrayItem, true)}");
                                if (arrayIndex < property.Value.Count())
                                    resultBuilder.Append(", ");
                                arrayIndex++;
                            }
                            resultBuilder.Append(" }, ");
                            arrayIndex = 1;
                        }
                        else if (property.Value.Type == JTokenType.Object)
                            // Handle nested object type
                            resultBuilder.Append($"[\"{property.Name}\"] = {ConvertToLuaTable(property.Value, true, property.Name)}, ");
                        else
                            resultBuilder.Append($"[\"{property.Name}\"] = {ConvertToLuaTable(property.Value, true)}, ");
                    }

                    if (resultBuilder.Length > 2)
                        resultBuilder.Length -= 2;

                    resultBuilder.Append(" }");

                    return resultBuilder.ToString();
                }
                else if (token.Type == JTokenType.String)
                    return $"\"{token.Value<string>()}\"";
                else
                    return token.ToString(); // For other value types, use their raw string representation
            }
            else
            {
                if (token.Type == JTokenType.Object)
                {
                    StringBuilder resultBuilder = new StringBuilder();

                    foreach (JProperty property in token.Children<JProperty>())
                    {
                        if (property.Value.Type == JTokenType.String)
                            // Handle string type
                            resultBuilder.Append($"\"{property.Value}\", ");
                        else if (property.Value.Type == JTokenType.Integer)
                            // Handle integer type
                            resultBuilder.Append($"{property.Value}, ");
                        else if (property.Value.Type == JTokenType.Array)
                        {
                            // Handle array type
                            resultBuilder.Append($"{{ ");
                            foreach (JToken arrayItem in property.Value)
                            {
                                resultBuilder.Append($"{ConvertToLuaTable(arrayItem, true)}");
                                if (arrayIndex < property.Value.Count())
                                    resultBuilder.Append(", ");
                                arrayIndex++;
                            }
                            resultBuilder.Append(" }, ");
                            arrayIndex = 1;
                        }
                        else if (property.Value.Type == JTokenType.Object)
                            // Handle nested object type
                            resultBuilder.Append($"{ConvertToLuaTable(property.Value, true, property.Name)}, ");
                        else
                            resultBuilder.Append($"{ConvertToLuaTable(property.Value, true)}, ");
                    }

                    if (resultBuilder.Length > 2)
                        resultBuilder.Length -= 2;

                    return resultBuilder.ToString();
                }
                else if (token.Type == JTokenType.String)
                    return $"\"{token.Value<string>()}\"";
                else
                    return token.ToString(); // For other value types, use their raw string representation
            }
        }

        public static object GetValueFromJToken(JToken jToken, string propertyName)
        {
            JToken valueToken = jToken[propertyName];

            if (valueToken != null)
            {
                if (valueToken.Type == JTokenType.Object || valueToken.Type == JTokenType.Array)
                    return valueToken.ToObject<object>();
                else if (valueToken.Type == JTokenType.Integer)
                    return valueToken.ToObject<int>();
                else if (valueToken.Type == JTokenType.String)
                    return valueToken.ToObject<string>();
                else if (valueToken.Type == JTokenType.Boolean)
                    return valueToken.ToObject<bool>();
                else if (valueToken.Type == JTokenType.Float)
                    return valueToken.ToObject<float>();
            }

            return null;
        }

        // Helper method to remove the trailing comma from the Lua table string
        public static string RemoveTrailingComma(string input)
        {
            return Regex.Replace(input, @",(\s*})|(\s*]\s*})", "$1$2");
        }

        public static JObject GetJsonData(string json)
        {
            try
            {
                return JObject.Parse(json);
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"Error while parsing JSON: {ex}");
            }

            return null;
        }

        public static Scoreboard GenerateSampleScoreboard(int numEntries)
        {
            Scoreboard scoreboard = new Scoreboard();
            scoreboard.Entries = new List<ScoreboardEntry>();

            Random random = new Random();
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

            return scoreboard;
        }

        public static string CalculateMD5HashToExadecimal(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public static string GetFirstEightCharacters(string input)
        {
            if (input.Length >= 8)
                return input.Substring(0, 8);

            else
                // If the input is less than 8 characters, you can handle it accordingly
                // For simplicity, let's just pad with zeros in this case
                return input.PadRight(8, '0');
        }
    }

    public static class ScoreboardNameGenerator
    {
        private static Random random = new Random();

        // List of silly French-sounding words to be used in the names
        private static string[] sillyFrenchWords = { "Croissant", "Baguette", "Fougasse", "TarteAuFromage", "Tabernack", "UnePetiteContine", "ChuckNorris", "Pamplemousse", "JimCarrey", "Fromage" };

        public static string GenerateRandomName()
        {
            return sillyFrenchWords[random.Next(0, sillyFrenchWords.Length)];
        }
    }

    public static class UniqueNumberGenerator
    {
        // Function to generate a unique number based on a string using MD5
        public static int GenerateUniqueNumber(string inputString)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes("0HS0000000000000A" + inputString));

                // To get a small integer within Lua int bounds, take the least significant 16 bits of the hash and convert to int16
                int uniqueNumber = Math.Abs(BitConverter.ToUInt16(data, 0));

                return uniqueNumber;
            }
        }
    }

    public class ScoreboardEntry
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public int Rank { get; set; } // Add this property to hold the rank
    }

    public class Scoreboard
    {
        public List<ScoreboardEntry> Entries { get; set; }
    }

    public class ScoreBoardUpdateSameEntry
    {
        public string user { get; set; }
        public string[] keys { get; set; }
        public int score { get; set; }
        public object[] value { get; set; }
    }

    public class ScoreBoardUpdate
    {
        public string user { get; set; }
        public string key { get; set; }
        public int score { get; set; }
        public object[] value { get; set; }
    }

    public class ScoreBoardUsersRequest
    {
        public string[] Users { get; set; }
        public string Key { get; set; }
    }

    public class BatchCommand
    {
        public string Method { get; set; }
        public JToken Data { get; set; }
        public string Project { get; set; }
    }
    public class OHSUserProfile
    {
        public string user { get; set; }
        public object key { get; set; }
    }

    public class OHSGlobalProfile
    {
        public object Key { get; set; }
    }
}
