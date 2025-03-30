using CustomLogger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SSFWServer.Helpers.FileHelper;
using SSFWServer.Helpers.RegexHelper;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace SSFWServer.Services
{
    public class SSFWRewardsService : IDisposable
    {
        private string? key;
        private bool disposedValue;

        public SSFWRewardsService(string? key)
        {
            this.key = key;
        }

        public byte[] HandleRewardServicePOST(byte[] buffer, string directorypath, string filepath, string absolutepath)
        {
            Directory.CreateDirectory(directorypath);

            File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.json", buffer);

            SSFWUpdateMini(filepath + "/mini.json", Encoding.UTF8.GetString(buffer));

            return buffer;
        }

        public byte[] HandleRewardServiceInvPOST(byte[] buffer, string directorypath, string filepath, string absoultepath)
        {
            Directory.CreateDirectory(directorypath);

            return SSFWRewardServiceInventoryPOST(buffer, directorypath, filepath, absoultepath);
        }

        public void HandleRewardServiceTrunksPOST(byte[] buffer, string directorypath, string filepath, string absolutepath)
        {
            Directory.CreateDirectory(directorypath);

            File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.json", buffer);

            SSFWTrunkServiceProcess(filepath.Replace("/setpartial", string.Empty) + ".json", Encoding.UTF8.GetString(buffer), false);
        }

        public void HandleRewardServiceTrunksEmergencyPOST(byte[] buffer, string directorypath, string absolutepath)
        {
            Directory.CreateDirectory(directorypath);

            File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.json", buffer);
        }

        public void SSFWUpdateMini(string filePath, string postData)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string? json = FileHelper.ReadAllText(filePath, key);

                    // Parse the JSON string as a JArray
                    JArray? jsonArray = null;
                    if (string.IsNullOrEmpty(json))
                        jsonArray = new JArray();
                    else
                        jsonArray = JArray.Parse(json);

                    // Extract the rewards object from the POST data
                    JObject postDataObject = JObject.Parse(postData);
                    JObject? rewardsObject = (JObject?)postDataObject["rewards"];

                    if (rewardsObject != null)
                    {
                        // Iterate over each reward in the POST data
                        foreach (var reward in rewardsObject)
                        {
                            string rewardKey = reward.Key;

                            // Check if the reward exists in the JSON array
                            JToken? existingReward = jsonArray.FirstOrDefault(r => r[rewardKey] != null);
                            if (existingReward != null)
                                // Update the value of the reward to 1
                                existingReward[rewardKey] = 1;
                            else
                            {
                                // Create a new reward object with the value 1
                                JObject newReward = new JObject
                                {
                                    { rewardKey, 1 }
                                };

                                // Add the new reward to the JSON array
                                jsonArray.Add(newReward);
                            }
                        }

                        File.WriteAllText(filePath, jsonArray.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SSFW] - SSFWUpdateMini errored out with this exception - {ex}");
            }
        }

        public void SSFWTrunkServiceProcess(string filePath, string request, bool addOnMissingUpdateElement)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string? json = FileHelper.ReadAllText(filePath, key);

                    if (!string.IsNullOrEmpty(json))
                    {
                        // Parse the request
                        JObject? requestObject = JsonConvert.DeserializeObject<JObject>(request);

                        if (requestObject != null)
                        {
                            JObject mainFile = JObject.Parse(json);

                            // Check if 'add' operation is requested
                            if (requestObject.ContainsKey("add") && requestObject["add"]?["objects"] is JArray addArray)
                            {
                                JArray? mainArray = (JArray?)mainFile["objects"];
                                if (mainArray != null)
                                {
                                    foreach (JObject addObject in addArray)
                                    {
                                        mainArray.Add(addObject);
                                    }
                                }
                            }

                            // Check if 'update' operation is requested
                            if (requestObject.ContainsKey("update") && requestObject["update"]?["objects"] is JArray updateArray)
                            {
                                JArray? mainArray = (JArray?)mainFile["objects"];
                                if (mainArray != null)
                                {
                                    foreach (JObject updateObj in updateArray)
                                    {
                                        if (updateObj.TryGetValue("objectId", out var objectIdToken) && objectIdToken is JValue objectIdValue)
                                        {
                                            string objectId = objectIdValue.ToString();
                                            JObject? existingObj = mainArray.FirstOrDefault(obj => obj["objectId"]?.ToString() == objectId) as JObject;
                                            if (existingObj != null)
                                                existingObj.Merge(updateObj, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
                                            else if (addOnMissingUpdateElement)
                                                mainArray.Add(updateObj);
                                        }
                                    }
                                }
                            }

                            // Check if 'delete' operation is requested
                            if (requestObject.ContainsKey("delete") && requestObject["delete"]?["objects"] is JArray deleteArray)
                            {
                                JArray? mainArray = (JArray?)mainFile["objects"];
                                if (mainArray != null)
                                {
                                    foreach (JObject deleteObj in deleteArray)
                                    {
                                        if (deleteObj.TryGetValue("objectId", out var objectIdToken) && objectIdToken is JValue objectIdValue)
                                        {
                                            string objectId = objectIdValue.ToString();
                                            JObject? existingObj = mainArray.FirstOrDefault(obj => obj["objectId"]?.ToString() == objectId) as JObject;
                                            if (existingObj != null)
                                                existingObj.Remove();
                                        }
                                    }
                                }
                            }

                            File.WriteAllText(filePath, mainFile.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SSFW] - SSFWTrunkServiceProcess errored out with this exception - {ex}");
            }
        }

        public byte[] SSFWRewardServiceInventoryPOST(byte[] buffer, string directorypath, string filepath, string absolutePath)
        {
#if DEBUG
            //LoggerAccessor.LogWarn($"POST CHECK {Encoding.UTF8.GetString(buffer)}");
#endif
            //Tracking Inventory
            string trackingGuid = "00000000-00000000-00000000-00000001"; // fallback/hardcoded tracking GUID

            // File paths based on the provided format
            string countsStoreDir = $"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutePath}/";
            string countsStore = $"{countsStoreDir}/counts.json";
            string trackingFileDir = $"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutePath}/object/";
            string trackingFile = $"{trackingFileDir}/{trackingGuid}.json";

            Directory.CreateDirectory(Path.GetDirectoryName(countsStoreDir));
            Directory.CreateDirectory(Path.GetDirectoryName(trackingFileDir));

            // Preprocess JSON to ensure GUIDs are quoted
            string fixedJsonPayload = GUIDValidator.FixJsonValues(Encoding.UTF8.GetString(buffer));
            try
            {
                //Console.WriteLine($"Fixed JSON: {fixedJsonPayload}");

                using JsonDocument document = JsonDocument.Parse(fixedJsonPayload);
                JsonElement root = document.RootElement;
                if (!root.TryGetProperty("rewards", out JsonElement rewardsElement) || rewardsElement.ValueKind != JsonValueKind.Array)
                {
                    LoggerAccessor.LogError("[SSFW] : Invalid payload: 'rewards' must be an array.");
                    return Encoding.UTF8.GetBytes("{\"idList\": [\"00000000-00000000-00000000-00000001\"] }");
                }

                var rewards = rewardsElement.EnumerateArray();
                if (!rewards.MoveNext())
                {
                    LoggerAccessor.LogError("[SSFW] : Invalid payload: 'rewards' array is empty.");
                    return Encoding.UTF8.GetBytes("{\"idList\": [\"00000000-00000000-00000000-00000001\"] }");
                }

                Dictionary<string, int> counts;
                if (File.Exists(countsStore))
                {
                    string countsJson = File.ReadAllText(countsStore);
                    counts = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(countsJson) ?? new Dictionary<string, int>();
                }
                else
                {
                    counts = new Dictionary<string, int>();
                }

                Dictionary<string, Dictionary<string, object>> existingTrackingData = null;
                if (File.Exists(trackingFile))
                {
                    string existingTrackingJson = File.ReadAllText(trackingFile);
                    using JsonDocument trackingDoc = JsonDocument.Parse(existingTrackingJson);
                    JsonElement trackingRoot = trackingDoc.RootElement;
                    if (trackingRoot.TryGetProperty("rewards", out JsonElement trackingRewardsElement) &&
                        trackingRewardsElement.ValueKind == JsonValueKind.Object)
                    {
                        existingTrackingData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(
                            trackingRewardsElement.GetRawText());
                    }
                }

                foreach (JsonElement reward in rewards)
                {
                    if (!reward.TryGetProperty("objectId", out JsonElement objectIdElement) ||
                        objectIdElement.ValueKind != JsonValueKind.String)
                    {
                        LoggerAccessor.LogError("[SSFW] : Invalid reward: 'objectId' missing or not a string.");
                        continue;
                    }

                    if (objectIdElement.ValueKind != JsonValueKind.String)
                    {
                        Console.WriteLine($"Invalid reward: 'objectId' must be a string, got {objectIdElement.ValueKind}.");
                        continue;
                    }

                    string objectId = objectIdElement.GetString();

                    // Update counts
                    if (counts.ContainsKey(objectId))
                    {
                        counts[objectId]++;
                    }
                    else
                    {
                        counts[objectId] = 1;
                    }

                    // Check if this is a tracking object (has metadata or matches tracking GUID)
                    bool hasMetadata = reward.TryGetProperty("_id", out _) ||
                                      reward.TryGetProperty("scene", out _) ||
                                      reward.TryGetProperty("boost", out _) ||
                                      reward.TryGetProperty("game", out _) ||
                                      reward.TryGetProperty("migrated", out _);
                    if (hasMetadata || objectId == trackingGuid || objectId != string.Empty)
                    {
                        var trackingRewards = existingTrackingData ?? new Dictionary<string, Dictionary<string, object>>();
                        var metadata = new Dictionary<string, object>();

                        foreach (JsonProperty prop in reward.EnumerateObject())
                        {
                            if (prop.Name != "objectId")
                            {
                                switch (prop.Value.ValueKind)
                                {
                                    case JsonValueKind.String:
                                        metadata[prop.Name] = prop.Value.GetString();
                                        break;
                                    case JsonValueKind.Number:
                                        metadata[prop.Name] = prop.Value.GetInt32();
                                        break;
                                    case JsonValueKind.True:
                                    case JsonValueKind.False:
                                        metadata[prop.Name] = prop.Value.GetBoolean();
                                        break;
                                    default:
                                        metadata[prop.Name] = prop.Value.ToString();
                                        break;
                                }
                            }
                        }

                        trackingRewards[objectId] = metadata;

                        // Write tracking data
                        var trackingData = new Dictionary<string, object>
                        {
                            { "result", 0 },
                            { "rewards", trackingRewards }
                        };
                        string trackingJson = System.Text.Json.JsonSerializer.Serialize(trackingData, new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(trackingFile, trackingJson);
#if DEBUG
                        LoggerAccessor.LogInfo($"[SSFW] : Updated tracking file: {trackingFile}");
#endif
                    }
                }

                string updatedCountsJson = System.Text.Json.JsonSerializer.Serialize(counts, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(countsStore, updatedCountsJson);
#if DEBUG
                LoggerAccessor.LogInfo($"[SSFW] : Updated counts file: {countsStore}");
#endif
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine($"Error parsing JSON payload: {ex.Message}");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SSFW] : Error processing POST request: {ex.Message}");
            }
            return Encoding.UTF8.GetBytes(@"{ ""idList"": [""00000000-00000000-00000000-00000001""]}");
        }

        public void AddMiniEntry(List<Dictionary<string, byte>> rewardsList, string uuid, byte invtype, string trunkFilePath)
        {
            if (!rewardsList.Any(x => x.ContainsKey(uuid)))
                rewardsList.Add(new Dictionary<string, byte>
                {
                    { uuid, invtype }
                });

            ProcessTrunkObjectUpdate(trunkFilePath, new Dictionary<string, byte> { { uuid, invtype } }, true);
        }

        public void RemoveMiniEntry(List<Dictionary<string, byte>> rewardsList, string uuid, byte invtype, string trunkFilePath)
        {
            foreach (Dictionary<string, byte> dict in rewardsList)
            {
                if (dict.ContainsKey(uuid) && dict[uuid] == invtype)
                {
                    rewardsList.Remove(dict);
                    break;
                }
            }

            ProcessTrunkObjectUpdate(trunkFilePath, new Dictionary<string, byte> { { uuid, invtype } }, false);
        }

        public void AddMiniEntries(List<Dictionary<string, byte>> rewardsList, Dictionary<string, byte> entriesToAdd, string trunkFilePath)
        {
            foreach (var entry in entriesToAdd)
            {
                string uuid = entry.Key;

                if (!rewardsList.Any(x => x.ContainsKey(uuid)))
                {
                    rewardsList.Add(new Dictionary<string, byte>
                    {
                        { uuid, entry.Value }
                    });
                }
            }

            ProcessTrunkObjectUpdate(trunkFilePath, entriesToAdd, true);
        }

        public void RemoveMiniEntries(List<Dictionary<string, byte>> rewardsList, Dictionary<string, byte> entriesToRemove, string trunkFilePath)
        {
            foreach (var entry in entriesToRemove)
            {
                string uuid = entry.Key;
                byte invtype = entry.Value;

                foreach (Dictionary<string, byte> dict in rewardsList)
                {
                    if (dict.ContainsKey(uuid) && dict[uuid] == invtype)
                    {
                        rewardsList.Remove(dict);
                        break;
                    }
                }
            }

            ProcessTrunkObjectUpdate(trunkFilePath, entriesToRemove, false);
        }

        private void ProcessTrunkObjectUpdate(string trunkFilePath, Dictionary<string, byte> entries, bool addOrUpdate)
        {
            string? trunkJson = FileHelper.ReadAllText(trunkFilePath, key);

            if (!string.IsNullOrEmpty(trunkJson))
            {
                try
                {
                    string setPartialDirectory = trunkFilePath.Substring(0, trunkFilePath.Length - 5);
                    List<int> indexList = new();
                    MatchCollection matches = new Regex(@"\""index\"":\s*\""(\d+)\""").Matches(trunkJson);
                    int i = matches.Count;
                    int lastIndex = 0;

                    while (i > 0)
                    {
                        if (int.TryParse(matches[i - 1].Groups[1].Value, out lastIndex))
                        {
                            indexList.Add(lastIndex);
                        }

                        i--;
                    }

                    if (indexList.Count > 0)
                        lastIndex = indexList.Max() + 1;

                    string setpartialRequest = BuildSetPartialJson(entries, lastIndex, addOrUpdate);

                    Directory.CreateDirectory(setPartialDirectory);

                    File.WriteAllText(setPartialDirectory + "/setpartial.json", setpartialRequest);

                    SSFWTrunkServiceProcess(trunkFilePath, setpartialRequest, true);
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[SSFW] - ProcessTrunkObjectUpdate: setpartial update errored out with this exception - {ex}");
                }
            }
        }

        private string BuildSetPartialJson(Dictionary<string, byte> entries, int index, bool addOrUpdate)
        {
            // Create the object to build the JSON structure
            if (addOrUpdate)
            {
                var jsonObject = new
                {
                    update = new
                    {
                        objects = new List<object>()
                    }
                };

                // Loop through the dictionary and add each item to the objects list
                foreach (var item in entries)
                {
                    jsonObject.update.objects.Add(new
                    {
                        objectId = item.Key,
                        type = item.Value.ToString(),
                        trunk = "0",
                        index = index.ToString()
                    });

                    index++;
                }

                // Serialize the object to JSON string
                return JsonConvert.SerializeObject(jsonObject);
            }
            else
            {
                var jsonObject = new
                {
                    delete = new
                    {
                        objects = new List<object>()
                    }
                };

                // Loop through the dictionary and add each item to the objects list
                foreach (var item in entries)
                {
                    jsonObject.delete.objects.Add(new
                    {
                        objectId = item.Key,
                        type = item.Value.ToString(),
                        trunk = "0",
                        index = index.ToString()
                    });

                    index++;
                }

                // Serialize the object to JSON string
                return JsonConvert.SerializeObject(jsonObject);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    key = null;

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~SSFWRewardsService()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
