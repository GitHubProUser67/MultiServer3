using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MultiServer.HTTPService.Addons.PlayStationHome.SSFW
{
    public class SSFWProcessor
    {
        public static async Task SSFWtrunkserviceprocess(string filePath, string request)
        {
            string json = null;

            if (File.Exists(filePath))
                json = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(filePath, SSFWPrivateKey.SSFWPrivatekey));

            if (json != null)
            {
                JObject mainFile = JObject.Parse(json);

                // Parse the request
                JObject requestObject = JObject.Parse(request);

                if (requestObject == null)
                {
                    return;
                }

                // Check if 'add' operation is requested
                if (requestObject.ContainsKey("add"))
                {
                    JArray addArray = (JArray)requestObject["add"]["objects"];
                    JArray mainArray = (JArray)mainFile["objects"];

                    if (addArray == null || mainArray == null)
                    {
                        return;
                    }

                    // Add each new object to the main file
                    foreach (JObject addObject in addArray)
                    {
                        mainArray.Add(addObject);
                    }
                }

                // Check if 'update' operation is requested
                if (requestObject.ContainsKey("update"))
                {
                    JArray updateArray = (JArray)requestObject["update"]["objects"];
                    JArray mainArray = (JArray)mainFile["objects"];

                    if (mainArray == null)
                    {
                        return;
                    }

                    // Update the existing objects in the main file
                    foreach (JObject updateObj in updateArray)
                    {
                        string objectId = updateObj["objectId"].ToString();

                        // Find the object in the main file and update its properties
                        JObject existingObj = (JObject)mainArray.FirstOrDefault(obj => obj["objectId"].ToString() == objectId);
                        if (existingObj != null)
                        {
                            existingObj.Merge(updateObj, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
                        }
                    }
                }

                // Check if 'delete' operation is requested
                if (requestObject.ContainsKey("delete"))
                {
                    JArray deleteArray = (JArray)requestObject["delete"]["objects"];
                    JArray mainArray = (JArray)mainFile["objects"];

                    // Remove the objects from the main file
                    foreach (JObject deleteObj in deleteArray)
                    {
                        string objectId = deleteObj["objectId"].ToString();

                        // Find and remove the object from the main file
                        JObject existingObj = (JObject)mainArray.FirstOrDefault(obj => obj["objectId"].ToString() == objectId);
                        if (existingObj != null)
                        {
                            existingObj.Remove();
                        }
                    }
                }

                await FileHelper.CryptoWriteAsync(filePath, SSFWPrivateKey.SSFWPrivatekey, Encoding.UTF8.GetBytes(mainFile.ToString()), true);
            }
        }

        public static async Task SSFWupdatemini(string filePath, string postData)
        {
            string json = null;

            if (File.Exists(filePath))
                json = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(filePath, SSFWPrivateKey.SSFWPrivatekey));

            // Parse the JSON string as a JArray
            JArray jsonArray;
            if (string.IsNullOrEmpty(json))
            {
                jsonArray = new JArray();
            }
            else
            {
                jsonArray = JArray.Parse(json);
            }

            // Extract the rewards object from the POST data
            JObject postDataObject = JObject.Parse(postData);
            JObject rewardsObject = (JObject)postDataObject["rewards"];

            if (rewardsObject == null)
            {
                return;
            }

            // Iterate over each reward in the POST data
            foreach (var reward in rewardsObject)
            {
                string rewardKey = reward.Key;

                // Check if the reward exists in the JSON array
                JToken existingReward = jsonArray.FirstOrDefault(r => r[rewardKey] != null);
                if (existingReward != null)
                {
                    // Update the value of the reward to 1
                    existingReward[rewardKey] = 1;
                }
                else
                {
                    // Create a new reward object with the value 1
                    JObject newReward = new JObject();
                    newReward.Add(rewardKey, 1);

                    // Add the new reward to the JSON array
                    jsonArray.Add(newReward);
                }
            }

            await FileHelper.CryptoWriteAsync(filePath, SSFWPrivateKey.SSFWPrivatekey, Encoding.UTF8.GetBytes(jsonArray.ToString()), true);
        }

        public static async Task SSFWfurniturelayout(string filePath, string postData, string objectId)
        {
            string json = null;

            if (File.Exists(filePath))
                json = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(filePath, SSFWPrivateKey.SSFWPrivatekey));

            // Parse the JSON string as a JArray
            JArray jsonArray;
            if (string.IsNullOrEmpty(json))
            {
                jsonArray = new JArray();
            }
            else
            {
                jsonArray = JArray.Parse(json);
            }

            // Find the index of the existing object with the given objectId
            int existingIndex = -1;
            for (int i = 0; i < jsonArray.Count; i++)
            {
                JObject obj = jsonArray[i] as JObject;
                if (obj != null && obj.Properties().Any(p => p.Name == objectId))
                {
                    existingIndex = i;
                    break;
                }
            }

            if (existingIndex >= 0)
            {
                // Update the existing object with the new POST data
                JObject existingObject = jsonArray[existingIndex] as JObject;

                if (existingObject != null)
                {
                    existingObject[objectId] = JObject.Parse(postData);
                }
            }
            else
            {
                // Create a new object with the objectId and POST data
                JObject newObject = new JObject();
                newObject.Add(objectId, JObject.Parse(postData));

                // Add the new object to the JSON array
                jsonArray.Add(newObject);
            }

            await FileHelper.CryptoWriteAsync(filePath, SSFWPrivateKey.SSFWPrivatekey, Encoding.UTF8.GetBytes(jsonArray.ToString(Newtonsoft.Json.Formatting.None)), true);
        }

        public static string SSFWgetfurniturelayout(string filePath, string objectId)
        {
            string json = null;

            if (File.Exists(filePath))
                json = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(filePath, SSFWPrivateKey.SSFWPrivatekey));

            if (json != null)
            {
                // Parse the JSON string as a JArray
                JArray jsonArray = JArray.Parse(json);

                // Find the object with the given objectId
                JObject existingObject = jsonArray
                    .FirstOrDefault(obj => obj is JObject && obj[objectId] != null) as JObject;

                if (existingObject != null)
                {
                    // Retrieve the value for the given objectId
                    JToken value = existingObject[objectId];

                    if (value != null)
                    {
                        return value.ToString();
                    }
                }
            }

            // If the objectId is not found, return null
            return "";
        }

        public static async Task SSFWUpdateavatar(string filePath, int contentToUpdate, bool delete)
        {
            string json = null;

            if (File.Exists(filePath))
                json = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(filePath, SSFWPrivateKey.SSFWPrivatekey));

            if (json != null)
            {
                // Parse the JSON content
                JArray jsonArray = JArray.Parse(json);

                if (delete)
                {
                    // Remove the element(s) with the specified number
                    jsonArray = new JArray(jsonArray.Where(item => item.Value<int>("") != contentToUpdate));
                }
                else
                {
                    // Create a new JSON object with the specified number
                    JObject jsonObject = new JObject();
                    jsonObject[""] = contentToUpdate;

                    if (!delete)
                    {
                        // Check if the element already exists
                        JToken existingItem = jsonArray.FirstOrDefault(item => item.Value<int>("") == contentToUpdate);

                        if (existingItem != null)
                        {
                            // Update the existing element
                            existingItem.Replace(jsonObject);
                        }
                        else
                        {
                            // Add the new element to the array
                            jsonArray.Add(jsonObject);
                        }
                    }
                    else
                    {
                        ServerConfiguration.LogError("[SSFW] : SSFWUpdateavatar - Invalid mode specified. Please use 'UPDATE' or 'DELETE'.");

                        return;
                    }
                }

                await FileHelper.CryptoWriteAsync(filePath, SSFWPrivateKey.SSFWPrivatekey, Encoding.UTF8.GetBytes(jsonArray.ToString()), true);
            }
        }

        public static string ssfwgenerateguid(string input1, string input2)
        {
            string md5hash = "";
            string sha512hash = "";

            using (MD5 md5 = MD5.Create())
            {
                string salt = "**H0mEIsG3reAT!!!!!!!!!!!!!!";

                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input1 + salt));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Dispose();
            }

            using (SHA512 sha512 = SHA512.Create())
            {
                string salt = "C0MeBaCKHOm3*!*!*!*!*!*!*!*!";

                byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(salt + input2));
                sha512hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                sha512.Dispose();
            }

            return (md5hash.Substring(1, 8) + "-" + sha512hash.Substring(2, 4) + "-" + md5hash.Substring(10, 4) + "-" + sha512hash.Substring(16, 4) + "-" + sha512hash.Substring(19, 12)).ToLower();
        }
    }
}
