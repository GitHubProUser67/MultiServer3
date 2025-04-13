using CustomLogger;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace SSFWServer.Services
{
    public class SSFWRewardsService
    {
        private string? key;

        public SSFWRewardsService(string? key)
        {
            this.key = key;
        }

        public byte[] HandleRewardServicePOST(byte[] buffer, string directorypath, string filepath, string absolutepath)
        {
            Directory.CreateDirectory(directorypath);

            File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.json", buffer);

            SSFWUpdateMini(filepath + "/mini.json", Encoding.UTF8.GetString(buffer), false);

            return buffer;
        }

        public void HandleRewardServiceTrunksPOST(byte[] buffer, string directorypath, string filepath, string absolutepath, string env, string? userId)
        {
            Directory.CreateDirectory(directorypath);

            File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.json", buffer);

            SSFWTrunkServiceProcess(filepath.Replace("/setpartial", string.Empty) + ".json", Encoding.UTF8.GetString(buffer), env, userId);
        }

        public void HandleRewardServiceTrunksEmergencyPOST(byte[] buffer, string directorypath, string absolutepath)
        {
            Directory.CreateDirectory(directorypath);

            File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.json", buffer);
        }

        public void SSFWUpdateMini(string filePath, string postData, bool delete)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string? json = FileHelper.ReadAllText(filePath, key);

                    // Parse the JSON string as a JArray
                    JArray? jsonArray;
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
                            JToken? rewardValue = reward.Value;
                            if (string.IsNullOrEmpty(rewardKey) || rewardValue == null)
                                continue;
                            if (rewardValue.Type != JTokenType.Integer)
                            {
                                LoggerAccessor.LogInfo($"[SSFWRewardsService] - Reward:{rewardValue} earned, adding to mini file:{filePath}.");
                                rewardValue = 1;
                            }

                            // Check if the reward exists in the JSON array
                            JToken? existingReward = jsonArray.FirstOrDefault(r => r[rewardKey] != null);
                            if (delete)
                            {
                                // If delete is true, remove the existing reward
                                if (existingReward != null)
                                    jsonArray.Remove(existingReward);
                            }
                            else
                            {
                                if (existingReward != null)
                                    // Update the value of the reward
                                    existingReward[rewardKey] = rewardValue;
                                else
                                {
                                    // Add the new reward to the JSON array
                                    jsonArray.Add(new JObject
                                    {
                                        { rewardKey, rewardValue }
                                    });
                                }
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

        public void SSFWTrunkServiceProcess(string filePath, string request, string env, string? userId)
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
                                    Dictionary<string, string> entriesToAddInMini = new Dictionary<string, string>();

                                    foreach (JObject addObject in addArray)
                                    {
                                        mainArray.Add(addObject);
                                        if (addObject.TryGetValue("objectId", out JToken? objectIdToken) && objectIdToken != null 
                                            && addObject.TryGetValue("type", out JToken? typeToken) && typeToken != null && int.TryParse(typeToken.ToString(), out int typeTokenInt) && typeTokenInt != 0)
                                            entriesToAddInMini.TryAdd(objectIdToken.ToString(), typeToken.ToString());
                                    }

                                    // Update the mini file accordingly.
                                    if (!string.IsNullOrEmpty(env) && !string.IsNullOrEmpty(userId))
                                    {
                                        string miniPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{userId}/mini.json";

                                        if (!File.Exists(miniPath))
                                            File.WriteAllText(miniPath, "[]");

                                        foreach (var entry in entriesToAddInMini)
                                        {
                                            SSFWUpdateMini(miniPath, $"{{\"rewards\":{{\"{entry.Key}\": {entry.Value}}}}}", false);
                                        }
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
                                            existingObj?.Merge(updateObj, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
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
                                    List<string> entriesToRemoveInMini = new List<string>();

                                    foreach (JObject deleteObj in deleteArray)
                                    {
                                        if (deleteObj.TryGetValue("objectId", out var objectIdToken) && objectIdToken is JValue objectIdValue)
                                        {
                                            string objectId = objectIdValue.ToString();
                                            JObject? existingObj = mainArray.FirstOrDefault(obj => obj["objectId"]?.ToString() == objectId) as JObject;
                                            existingObj?.Remove();
                                            if (deleteObj.TryGetValue("type", out JToken? typeToken) && typeToken != null && int.TryParse(typeToken.ToString(), out int typeTokenInt) && typeTokenInt != 0)
                                                entriesToRemoveInMini.Add(objectId);
                                        }
                                    }

                                    // Update the mini file accordingly.
                                    if (!string.IsNullOrEmpty(env) && !string.IsNullOrEmpty(userId))
                                    {
                                        string miniPath = $"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{userId}/mini.json";

                                        if (File.Exists(miniPath))
                                        {
                                            foreach (string entry in entriesToRemoveInMini)
                                            {
                                                SSFWUpdateMini(miniPath, $"{{\"rewards\":{{\"{entry}\": -1}}}}", true);
                                            }
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

        public void AddMiniEntry(string uuid, byte invtype, string trunkFilePath, string env, string? userId)
        {
            ProcessTrunkObjectUpdate(trunkFilePath, new Dictionary<string, byte> { { uuid, invtype } }, env, userId, true);
        }

        public void RemoveMiniEntry(string uuid, byte invtype, string trunkFilePath, string env, string? userId)
        {
            ProcessTrunkObjectUpdate(trunkFilePath, new Dictionary<string, byte> { { uuid, invtype } }, env, userId, false);
        }

        public void AddMiniEntries(Dictionary<string, byte> entriesToAdd, string trunkFilePath, string env, string? userId)
        {
            ProcessTrunkObjectUpdate(trunkFilePath, entriesToAdd, env, userId, true);
        }

        public void RemoveMiniEntries(Dictionary<string, byte> entriesToRemove, string trunkFilePath, string env, string? userId)
        {
            ProcessTrunkObjectUpdate(trunkFilePath, entriesToRemove, env, userId, false);
        }

        private void ProcessTrunkObjectUpdate(string trunkFilePath, Dictionary<string, byte> entries, string env, string? userId, bool add)
        {
            string? trunkJsonData = FileHelper.ReadAllText(trunkFilePath, key);

            if (!string.IsNullOrEmpty(trunkJsonData))
            {
                string setpartialRequest;

                try
                {
                    string setPartialDirectory = trunkFilePath.Substring(0, trunkFilePath.Length - 5);
                    using JsonDocument doc = JsonDocument.Parse(trunkJsonData);

                    List<int> indexList = new();
                    Dictionary<int, (string, byte)> indexToItem = new();

                    foreach (var obj in doc.RootElement.GetProperty("objects").EnumerateArray())
                    {
                        if (obj.TryGetProperty("index", out var indexProp) &&
                            obj.TryGetProperty("objectId", out var idProp) &&
                            obj.TryGetProperty("type", out var idType) &&
                            int.TryParse(indexProp.GetString(), out int index))
                        {
                            indexList.Add(index);
                            string? idPropStr = idProp.GetString();
                            string? idTypeStr = idType.GetString();
                            if (!string.IsNullOrEmpty(idTypeStr) && !string.IsNullOrEmpty(idPropStr) && byte.TryParse(idTypeStr, out byte typeOfEntry))
                                indexToItem[index] = (idPropStr, typeOfEntry);
                        }
                    }

                    int lastIndex = indexList.Count > 0 ? indexList.Max() + 1 : 0;

                    if (add)
                    {
                        // Make sure we don't add a given uuid twice (causes inventory errors at boot)
                        foreach (string key in entries.Keys.Where(key => trunkJsonData.Contains(key)))
                        {
                            entries.Remove(key);
                        }
                        setpartialRequest = BuildAddSetPartialJson(entries, lastIndex);
                    }
                    else
                        setpartialRequest = BuildDeleteSetPartialJson(entries, indexToItem);

                    Directory.CreateDirectory(setPartialDirectory);

                    File.WriteAllText(setPartialDirectory + "/setpartial.json", setpartialRequest);

                    SSFWTrunkServiceProcess(trunkFilePath, setpartialRequest, env, userId);
                }
                catch (Exception ex)
                {
                    LoggerAccessor.LogError($"[SSFW] - ProcessTrunkObjectUpdate: setpartial update errored out with this exception - {ex}");
                }
            }
        }

        private string BuildAddSetPartialJson(Dictionary<string, byte> entries, int startIndex)
        {
            // Create the object to build the JSON structure
            var jsonObject = new
            {
                add = new
                {
                    objects = new List<object>()
                }
            };

            // Loop through the dictionary and add each item to the objects list
            foreach (var item in entries)
            {
                jsonObject.add.objects.Add(new
                {
                    objectId = item.Key,
                    type = item.Value.ToString(),
                    trunk = "0",
                    index = startIndex.ToString()
                });

                startIndex++;
            }

            // Serialize the object to JSON string
            return JsonConvert.SerializeObject(jsonObject);
        }

        private string BuildDeleteSetPartialJson(Dictionary<string, byte> entries, Dictionary<int, (string, byte)> indexToItem)
        {
            // Create the object to build the JSON structure
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
                    index = indexToItem.Where(x => x.Value.Item2 == item.Value && x.Value.Item1 == item.Key).FirstOrDefault().Key.ToString(),
                });
            }

            // Serialize the object to JSON string
            return JsonConvert.SerializeObject(jsonObject);
        }
    }
}
