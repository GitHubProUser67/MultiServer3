﻿using CustomLogger;
using HttpMultipartParser;
using NetworkLibrary.Extension;
using NetworkLibrary.HTTP;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace WebAPIService.HEAVYWATER
{
    public class HeavyWaterClass
    {
        private string absolutepath;
        private string method;
        private string apipath;

        public HeavyWaterClass(string method, string absolutepath, string apipath)
        {
            this.absolutepath = absolutepath;
            this.method = method;
            this.apipath = apipath;
        }

        public string ProcessRequest(IDictionary<string, string> QueryParameters, byte[] PostData, string ContentType)
        {
            const string playerPathern = @"/player/([^/]+)";

            try
            {
                Match match = Regex.Match(absolutepath, playerPathern);

                switch (method)
                {
                    case "GET":
                        if (match.Success)
                        {
                            string id = match.Groups[1].Value;

                            if (absolutepath.Contains("/D2O/Avalon/"))
                            {
                                string avalonProfilePath = apipath + $"/HEAVYWATER/Avalon_keep/{id}";

                                if (absolutepath.EndsWith("/data/HouseData"))
                                {
                                    string house = "{\"House\":{\"AVALON\":true}}";

                                    if (File.Exists(avalonProfilePath + "/House.json"))
                                        house = File.ReadAllText(avalonProfilePath + "/House.json");

                                    return $@"{{
                                    ""STATUS"": ""SUCCESS"",
                                    ""result"":
                                      {house}
                                  }}";
                                }
                                else if (absolutepath.EndsWith("/data/MyAvalonKeepData"))
                                {
                                    string AvalonKeepData = "{}";

                                    if (File.Exists(avalonProfilePath + "/AvalonKeepData.json"))
                                        AvalonKeepData = File.ReadAllText(avalonProfilePath + "/AvalonKeepData.json");

                                    return $@"{{
                                    ""STATUS"": ""SUCCESS"",
                                    ""result"":
                                      {AvalonKeepData}
                                  }}";
                                }
                            }
                            else if (absolutepath.Contains("/D2O/AvalonHexx/"))
                            {
                                string hexxProfilePath = apipath + $"/HEAVYWATER/Avalon_hexx/{id}";

                                if (absolutepath.EndsWith("/data/MyAvalonHexxData"))
                                {
                                    string AvalonHexxData = "{}";

                                    if (File.Exists(hexxProfilePath + "/HexxData.json"))
                                        AvalonHexxData = File.ReadAllText(hexxProfilePath + "/HexxData.json");

                                    return $@"{{
                                        ""STATUS"": ""SUCCESS"",
                                        ""result"":
                                          {AvalonHexxData}
                                      }}";
                                }
                            }
                            else if (absolutepath.Contains("/D2O/EmoRay/"))
                            {
                                string emorayProfilePath = apipath + $"/HEAVYWATER/EmoRay/{id}";

                                if (absolutepath.EndsWith("/data/ProgressionData"))
                                {
                                    string emorayProgData = "{}";

                                    if (File.Exists(emorayProfilePath + "/ProgressionData.json"))
                                        emorayProgData = File.ReadAllText(emorayProfilePath + "/ProgressionData.json");

                                    return $@"{{
                                        ""STATUS"": ""SUCCESS"",
                                        ""result"":
                                          {emorayProgData}
                                      }}";
                                }
                                else if (absolutepath.EndsWith("/data/EquippedData"))
                                {
                                    string emorayEquippedData = "{}";

                                    if (File.Exists(emorayProfilePath + "/EquippedData.json"))
                                        emorayEquippedData = File.ReadAllText(emorayProfilePath + "/EquippedData.json");

                                    return $@"{{
                                        ""STATUS"": ""SUCCESS"",
                                        ""result"":
                                          {emorayEquippedData}
                                      }}";
                                }
                                else if (absolutepath.EndsWith("/data/ScoresData"))
                                {
                                    string emorayScoresData = "{}";

                                    if (File.Exists(emorayProfilePath + "/ScoresData.json"))
                                        emorayScoresData = File.ReadAllText(emorayProfilePath + "/ScoresData.json");

                                    return $@"{{
                                        ""STATUS"": ""SUCCESS"",
                                        ""result"":
                                          {emorayScoresData}
                                      }}";
                                }
                                else if (absolutepath.EndsWith("/data/ControllerData"))
                                {
                                    string emorayControllerData = "{}";

                                    if (File.Exists(emorayProfilePath + "/ControllerData.json"))
                                        emorayControllerData = File.ReadAllText(emorayProfilePath + "/ControllerData.json");

                                    return $@"{{
                                        ""STATUS"": ""SUCCESS"",
                                        ""result"":
                                          {emorayControllerData}
                                      }}";
                                }
                                else if (absolutepath.EndsWith("/data/StoreProgressData"))
                                {
                                    string emorayStoreProgressData = "{}";

                                    if (File.Exists(emorayProfilePath + "/StoreProgressData.json"))
                                        emorayStoreProgressData = File.ReadAllText(emorayProfilePath + "/StoreProgressData.json");

                                    return $@"{{
                                        ""STATUS"": ""SUCCESS"",
                                        ""result"":
                                          {emorayStoreProgressData}
                                      }}";
                                }
                            }
                            else if (absolutepath.Contains("/D2O/D2OUniverse/"))
                            {
                                string ProfilePath = apipath + $"/HEAVYWATER/D2OUniverse/{id}";

                                if (absolutepath.EndsWith("/data/D2OData"))
                                {
                                    string D2OData = "{}";

                                    if (File.Exists(ProfilePath + "/D2OData.json"))
                                        D2OData = File.ReadAllText(ProfilePath + "/D2OData.json");

                                    return $@"{{
                                    ""STATUS"": ""SUCCESS"",
                                    ""result"":
                                      {D2OData}
                                  }}";
                                }
                            }
                        }
                        else if (absolutepath.Contains("/D2O/Avalon/"))
                        {
                            if (absolutepath.EndsWith("/contributions"))
                            {
                                string ContribData = "{\"Contribution\":{}}";
                                string ContribPath = apipath + $"/HEAVYWATER/Avalon_keep/contributions.json";

                                if (File.Exists(ContribPath))
                                    ContribData = File.ReadAllText(ContribPath);

                                return $@"{{
                                    ""STATUS"": ""SUCCESS"",
                                    ""result"":
                                      {ContribData}
                                  }}";
                            }
                        }
                        else if (absolutepath.Contains("/D2O/AvalonHexx/"))
                        {
                            const string d2oidPathern = @"/d2oid/([^/]+)";

                            match = Regex.Match(absolutepath, d2oidPathern);

                            if (match.Success)
                                return $@"{{
                                    ""STATUS"": ""SUCCESS"",
                                    ""result"":
                                      ""{GenerateD2OGuid(match.Groups[1].Value)}""
                                  }}";
                            else if (absolutepath.EndsWith("Scores/") && QueryParameters.ContainsKey("limit") && int.TryParse(QueryParameters["limit"], out int limit))
                            {
                                int i = 1;
                                StringBuilder scoreboardData = new StringBuilder("{\"Scores\":{");

                                var scoreData = GetTopScores(apipath + $"/HEAVYWATER/Avalon_hexx/Scoreboard.json", limit);
                                int scoreDataCount = scoreData.Count();

                                foreach (var scoreKeyPair in scoreData)
                                {
                                    if (i == scoreDataCount)
                                        scoreboardData.Append($"\"{scoreKeyPair.Key}\":{scoreKeyPair.Value}");
                                    else
                                        scoreboardData.Append($"\"{scoreKeyPair.Key}\":{scoreKeyPair.Value},");

                                    i++;
                                }

                                return $@"{{
                                    ""STATUS"": ""SUCCESS"",
                                    ""result"":
                                      {scoreboardData}}}}}
                                  }}";
                            }
                        }
                        else if (absolutepath.Contains("/D2O/EmoRay/"))
                        {
                            if (absolutepath.EndsWith("Scores/") && QueryParameters.ContainsKey("limit") && QueryParameters.ContainsKey("range")
                                && !string.IsNullOrEmpty(QueryParameters["range"]) && int.TryParse(QueryParameters["limit"], out int limit))
                            {
                                // TODO, figure out leaderboards.

                                return $@"{{
                                    ""STATUS"": ""SUCCESS"",
                                    ""result"": {{ }}
                                  }}";
                            }
                        }
                        break;
                    case "POST":
                        if (absolutepath.Contains("/D2O/Ticket/") && (absolutepath.Contains("validate/") || absolutepath.Contains("verify/")))
                        {
                            using (MemoryStream copyStream = new MemoryStream(PostData))
                            {
                                foreach (FilePart file in MultipartFormDataParser.Parse(copyStream, HTTPProcessor.ExtractBoundary(ContentType))
                                    .Files.Where(x => x.FileName == "ticket.bin" && x.Name == "base64-ticket"))
                                {
                                    using (Stream filedata = file.Data)
                                    {
                                        filedata.Position = 0;

                                        // Find the number of bytes in the stream
                                        int contentLength = (int)filedata.Length;

                                        // Create a byte array
                                        byte[] ticketData = new byte[contentLength];

                                        // Read the contents of the memory stream into the byte array
                                        filedata.Read(ticketData, 0, contentLength);

                                        (bool, byte[]) isValidBase64Data = Encoding.ASCII.GetString(ticketData).IsBase64();

                                        if (isValidBase64Data.Item1)
                                        {
                                            // Extract the desired portion of the binary data for a npticket 4.0
                                            byte[] extractedData = new byte[0x63 - 0x54 + 1];

                                            // Copy it
                                            Array.Copy(isValidBase64Data.Item2, 0x54, extractedData, 0, extractedData.Length);

                                            // Trim null bytes
                                            int nullByteIndex = Array.IndexOf(extractedData, (byte)0x00);
                                            if (nullByteIndex >= 0)
                                            {
                                                byte[] trimmedData = new byte[nullByteIndex];
                                                Array.Copy(extractedData, trimmedData, nullByteIndex);
                                                extractedData = trimmedData;
                                            }

                                            string UserId = Encoding.UTF8.GetString(extractedData);

                                            if (ByteUtils.FindBytePattern(isValidBase64Data.Item2, new byte[] { 0x52, 0x50, 0x43, 0x4E }, 184) != -1)
                                                LoggerAccessor.LogInfo($"[HeavyWaterClass] - ProcessRequest : User {UserId} logged in and is on RPCN");
                                            else
                                                LoggerAccessor.LogInfo($"[HeavyWaterClass] - ProcessRequest : User {UserId} logged in and is on PSN");

                                            string id = GenerateD2OGuid(UserId);

                                            return $@"{{
                                                    ""STATUS"": ""SUCCESS"",
                                                    ""result"": {{
                                                      ""d2oID"": ""{id}""
                                                    }}
                                                  }}";
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case "PUT":
                        if (PostData.Length > 0 && PostData.Length <= Array.MaxLength)
                        {
                            if (match.Success)
                            {
                                string id = match.Groups[1].Value;

                                if (absolutepath.Contains("/D2O/Avalon/"))
                                {
                                    string avalonProfilePath = apipath + $"/HEAVYWATER/Avalon_keep/{id}";

                                    Directory.CreateDirectory(avalonProfilePath);

                                    if (absolutepath.EndsWith("/data/HouseData"))
                                    {
                                        string house = Encoding.UTF8.GetString(PostData);
                                        File.WriteAllText(avalonProfilePath + "/House.json", house);

                                        return $@"{{
                                            ""STATUS"": ""SUCCESS"",
                                            ""result"":
                                              {house}
                                          }}";
                                    }
                                    else if (absolutepath.EndsWith("/data/MyAvalonKeepData"))
                                    {
                                        string AvalonKeepData = Encoding.UTF8.GetString(PostData);
                                        File.WriteAllText(avalonProfilePath + "/AvalonKeepData.json", AvalonKeepData);

                                        return $@"{{
                                            ""STATUS"": ""SUCCESS"",
                                            ""result"":
                                              {AvalonKeepData}
                                          }}";
                                    }
                                }
                                else if (absolutepath.Contains("/D2O/AvalonHexx/"))
                                {
                                    string hexxProfilePath = apipath + $"/HEAVYWATER/Avalon_hexx/{id}";

                                    Directory.CreateDirectory(hexxProfilePath);

                                    if (absolutepath.EndsWith("/data/MyAvalonHexxData"))
                                    {
                                        string AvalonHexxData = Encoding.UTF8.GetString(PostData);
                                        File.WriteAllText(hexxProfilePath + "/AvalonHexxData.json", AvalonHexxData);

                                        return $@"{{
                                            ""STATUS"": ""SUCCESS"",
                                            ""result"":
                                              {AvalonHexxData}
                                          }}";
                                    }
                                }
                                else if (absolutepath.Contains("/D2O/EmoRay/"))
                                {
                                    string emorayProfilePath = apipath + $"/HEAVYWATER/EmoRay/{id}";

                                    Directory.CreateDirectory(emorayProfilePath);

                                    if (absolutepath.EndsWith("/data/ProgressionData"))
                                    {
                                        string emorayProgData = Encoding.UTF8.GetString(PostData);
                                        File.WriteAllText(emorayProfilePath + "/ProgressionData.json", emorayProgData);

                                        return $@"{{
                                            ""STATUS"": ""SUCCESS"",
                                            ""result"":
                                              {emorayProgData}
                                          }}";
                                    }
                                    else if (absolutepath.EndsWith("/data/EquippedData"))
                                    {
                                        string emorayEquippedData = Encoding.UTF8.GetString(PostData);
                                        File.WriteAllText(emorayProfilePath + "/EquippedData.json", emorayEquippedData);

                                        return $@"{{
                                            ""STATUS"": ""SUCCESS"",
                                            ""result"":
                                              {emorayEquippedData}
                                          }}";
                                    }
                                    else if (absolutepath.EndsWith("/data/ScoresData"))
                                    {
                                        string emorayScoresData = Encoding.UTF8.GetString(PostData);
                                        File.WriteAllText(emorayProfilePath + "/ScoresData.json", emorayScoresData);

                                        return $@"{{
                                            ""STATUS"": ""SUCCESS"",
                                            ""result"":
                                              {emorayScoresData}
                                          }}";
                                    }
                                    else if (absolutepath.EndsWith("/data/ControllerData"))
                                    {
                                        string emorayControllerData = Encoding.UTF8.GetString(PostData);
                                        File.WriteAllText(emorayProfilePath + "/ControllerData.json", emorayControllerData);

                                        return $@"{{
                                            ""STATUS"": ""SUCCESS"",
                                            ""result"":
                                              {emorayControllerData}
                                          }}";
                                    }
                                    else if (absolutepath.EndsWith("/data/StoreProgressData"))
                                    {
                                        string emorayStoreProgressData = Encoding.UTF8.GetString(PostData);
                                        File.WriteAllText(emorayProfilePath + "/StoreProgressData.json", emorayStoreProgressData);

                                        return $@"{{
                                            ""STATUS"": ""SUCCESS"",
                                            ""result"":
                                              {emorayStoreProgressData}
                                          }}";
                                    }
                                }
                                else if (absolutepath.Contains("/D2O/D2OUniverse/"))
                                {
                                    string ProfilePath = apipath + $"/HEAVYWATER/D2OUniverse/{id}";

                                    Directory.CreateDirectory(ProfilePath);

                                    if (absolutepath.EndsWith("/data/D2OData"))
                                    {
                                        string D2OData = Encoding.UTF8.GetString(PostData);
                                        File.WriteAllText(ProfilePath + "/D2OData.json", D2OData);

                                        return $@"{{
                                            ""STATUS"": ""SUCCESS"",
                                            ""result"": 
                                              {D2OData}
                                          }}";
                                    }
                                }
                            }
                            else if (absolutepath.Contains("/D2O/Avalon/"))
                            {
                                if (absolutepath.EndsWith("/metrics"))
                                {
                                    // TODO: process metrics data?
                                    return @"{
                                        ""STATUS"": ""SUCCESS"",
                                      }";
                                }
                                else if (absolutepath.EndsWith("/contributions"))
                                {
                                    Directory.CreateDirectory(apipath + $"/HEAVYWATER/Avalon_keep");

                                    string ContribPath = apipath + $"/HEAVYWATER/Avalon_keep/contributions.json";

                                    using (JsonDocument requestDoc = JsonDocument.Parse(Encoding.UTF8.GetString(PostData)))
                                    {
                                        JsonElement contribution = requestDoc.RootElement.GetProperty("Contribution");
                                        string house = contribution.GetProperty("House").GetString();
                                        int amount = contribution.GetProperty("Amount").GetInt32();

                                        if (!File.Exists(ContribPath))
                                            File.WriteAllText(ContribPath, $"{{\"Contribution\":{{\"{house}\":{amount}}}");
                                        else
                                        {
                                            JObject existingData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(ContribPath));

                                            JObject publicContribution = existingData["Contribution"] as JObject;
                                            if (publicContribution != null)
                                            {
                                                if (publicContribution[house] != null)
                                                    amount += publicContribution[house].ToObject<int>();

                                                publicContribution[house] = amount;

                                                File.WriteAllText(ContribPath, existingData.ToString());
                                            }
                                        }

                                        return "{\"STATUS\": \"SUCCESS\"}";
                                    }
                                }
                            }
                            else if (absolutepath.Contains("/D2O/AvalonPublicHexx/"))
                            {
                                if (absolutepath.EndsWith("/metrics"))
                                {
                                    // TODO: process metrics data?
                                    return @"{
                                        ""STATUS"": ""SUCCESS"",
                                      }";
                                }
                            }
                            else if (absolutepath.Contains("/D2O/AvalonHexx/"))
                            {
                                if (absolutepath.Contains("Scores/"))
                                {
                                    string hexxDataPath = apipath + $"/HEAVYWATER/Avalon_hexx";
                                    string[] parts = absolutepath.Split('/');

                                    Directory.CreateDirectory(hexxDataPath);

                                    SaveScore(hexxDataPath + "/Scoreboard.json", parts[parts.Length - 2], int.Parse(parts[parts.Length - 1]));

                                    return @"{
                                        ""STATUS"": ""SUCCESS"",
                                      }";
                                }
                            }
                            else if (absolutepath.Contains("/D2O/HeavyWaterPublic/"))
                            {
                                if (absolutepath.EndsWith("/metrics"))
                                {
                                    // TODO: process metrics data?
                                    return @"{
                                        ""STATUS"": ""SUCCESS"",
                                      }";
                                }
                            }
                            else if (absolutepath.Contains("/D2O/EmoRay/"))
                            {
                                if (absolutepath.EndsWith("/metrics"))
                                {
                                    // TODO: process metrics data?
                                    return @"{
                                        ""STATUS"": ""SUCCESS"",
                                      }";
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[HeavyWaterClass] - ProcessRequest thrown an assertion. (Exception: {ex})");
            }

            return null;
        }

        private static Dictionary<string, List<int>> LoadScores(string scoreFilePath)
        {
            if (!File.Exists(scoreFilePath))
                return new Dictionary<string, List<int>>();

            return JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(File.ReadAllText(scoreFilePath)) ?? new Dictionary<string, List<int>>();
        }

        private static void SaveScore(string scoreFilePath, string username, int score)
        {
            Dictionary<string, List<int>> scores = LoadScores(scoreFilePath);

            if (!scores.ContainsKey(username))
                scores[username] = new List<int>();

            scores[username].Add(score);

            File.WriteAllText(scoreFilePath, JsonConvert.SerializeObject(scores, Formatting.Indented));
        }

        private static IEnumerable<KeyValuePair<string, int>> GetTopScores(string scoreFilePath, int amount)
        {
            return LoadScores(scoreFilePath)
                .SelectMany(user => user.Value.Select(score => new KeyValuePair<string, int>(user.Key, score)))
                .OrderByDescending(entry => entry.Value)
                .Take(amount);
        }

        private static string GenerateD2OGuid(string input)
        {
            return SSFW.GuidGenerator.SSFWGenerateGuid(input, "1amAH3vyFan?!0yY3ahhhhhhhh!!!!!");
        }
    }
}
