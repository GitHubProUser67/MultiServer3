using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Text;
using BackendProject.FileHelper;
using BackendProject.MiscUtils;

namespace SSFWServer
{
    public class SSFWLayoutService : IDisposable
    {
        private string? key;
        private bool disposedValue;

        public SSFWLayoutService(string? key)
        {
            this.key = key;
        }

        public bool HandleLayoutServicePOST(byte[] buffer, string directorypath, string absolutepath)
        {
            Directory.CreateDirectory(directorypath);

            string sceneid = absolutepath;
            string[] words = sceneid.Split('/');

            if (words.Length > 0)
                sceneid = words[^1];

            if (sceneid != absolutepath) // If ends with UUID Ok.
            {
                if (File.Exists(SSFWServerConfiguration.ScenelistFile))
                {
                    bool handled = false;

                    Dictionary<string, string> scenemap = ScenelistParser.sceneDictionary;

                    if (File.Exists(directorypath + "/mylayout.json")) // Migrate data.
                    {
                        // Parsing each value in the dictionary
                        foreach (var kvp in SSFWGetLegacyFurnitureLayouts(directorypath + "/mylayout.json"))
                        {
                            if (kvp.Key == "00000000-00000000-00000000-00000004")
                            {
                                File.WriteAllText(directorypath + "/HarborStudio.json", kvp.Value);
                                handled = true;
                            }
                            else
                            {
                                string scenename = scenemap.FirstOrDefault(x => x.Value == VariousUtils.ExtractPortion(kvp.Key, 13, 18)).Key;
                                if (!string.IsNullOrEmpty(scenename))
                                {
                                    File.WriteAllText(directorypath + $"/{scenename}.json", kvp.Value);
                                    handled = true;
                                }
                            }

                            if (!handled)
                                File.WriteAllText(directorypath + $"/{kvp.Key}.json", kvp.Value);

                            handled = false;
                        }

                        File.Delete(directorypath + "/mylayout.json");
                    }
                    else
                    {
                        if (sceneid == "00000000-00000000-00000000-00000004")
                        {
                            File.WriteAllText(directorypath + "/HarborStudio.json", Encoding.UTF8.GetString(buffer));
                            handled = true;
                        }
                        else
                        {
                            string scenename = scenemap.FirstOrDefault(x => x.Value == VariousUtils.ExtractPortion(sceneid, 13, 18)).Key;
                            if (!string.IsNullOrEmpty(scenename))
                            {
                                File.WriteAllText(directorypath + $"/{scenename}.json", Encoding.UTF8.GetString(buffer));
                                handled = true;
                            }
                        }

                        if (!handled)
                            File.WriteAllText(directorypath + $"/{sceneid}.json", Encoding.UTF8.GetString(buffer));
                    }
                }
                else
                    SSFWLegacyFurnitureLayoutProcess(directorypath + "/mylayout.json", buffer, sceneid);

                return true;
            }
            
            return false;
        }

        public string? HandleLayoutServiceGET(string directorypath, string absolutepath)
        {
            string sceneid = absolutepath;
            string[] words = sceneid.Split('/');

            if (words.Length > 0)
                sceneid = words[^1];

            if (sceneid != absolutepath) // If ends with UUID Ok.
            {
                if (File.Exists(SSFWServerConfiguration.ScenelistFile))
                {
                    if (sceneid == "00000000-00000000-00000000-00000004")
                    {
                        if (File.Exists(directorypath + "/HarborStudio.json"))
                            return $"[{{\"{sceneid}\":{FileHelper.ReadAllText(directorypath + "/HarborStudio.json", key)}}}]";
                    }
                    else
                    {
                        string filepath = directorypath + $"/{ScenelistParser.sceneDictionary.FirstOrDefault(x => x.Value == VariousUtils.ExtractPortion(sceneid, 13, 18)).Key}.json";
                        if (File.Exists(filepath))
                            return $"[{{\"{sceneid}\":{FileHelper.ReadAllText(filepath, key)}}}]";
                    }

                    if (File.Exists(directorypath + $"/{sceneid}.json"))
                        return $"[{{\"{sceneid}\":{FileHelper.ReadAllText(directorypath + $"/{sceneid}.json", key)}}}]";
                }

                string? stringlayout = SSFWGetLegacyFurnitureLayoutProcess(directorypath + "/mylayout.json", sceneid);

                if (stringlayout != null)
                    return $"[{{\"{sceneid}\":{stringlayout}}}]";
                else
                    return string.Empty;
            }

            return null;
        }

        public void SSFWLegacyFurnitureLayoutProcess(string filePath, byte[] postData, string sceneid)
        {
            try
            {
                string? json = null;

                if (File.Exists(filePath))
                    json = FileHelper.ReadAllText(filePath, key);

                // Parse the JSON string as a JArray
                JArray jsonArray;
                if (string.IsNullOrEmpty(json))
                    jsonArray = new JArray();
                else
                    jsonArray = JArray.Parse(json);

                // Find the index of the existing object with the given objectId
                int existingIndex = -1;
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    JObject? obj = jsonArray[i] as JObject;
                    if (obj != null && obj.Properties().Any(p => p.Name == sceneid))
                    {
                        existingIndex = i;
                        break;
                    }
                }

                if (existingIndex >= 0)
                {
                    // Update the existing object with the new POST data
                    JObject? existingObject = jsonArray[existingIndex] as JObject;

                    if (existingObject != null)
                        existingObject[sceneid] = JObject.Parse(Encoding.UTF8.GetString(postData));
                }
                else
                {
                    // Create a new object with the objectId and POST data
                    JObject newObject = new()
                    {
                        { sceneid, JObject.Parse(Encoding.UTF8.GetString(postData)) }
                    };

                    // Add the new object to the JSON array
                    jsonArray.Add(newObject);
                }

                File.WriteAllText(filePath, jsonArray.ToString());
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SSFW] - SSFWLegacyFurnitureLayoutProcess errored out with this exception - {ex}");
            }
        }

        public string? SSFWGetLegacyFurnitureLayoutProcess(string filePath, string sceneid)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string? json = FileHelper.ReadAllText(filePath, key);
                    if (!string.IsNullOrEmpty(json))
                    {
                        JArray jsonArray = JArray.Parse(json);
                        JObject? existingObject = jsonArray
                            .OfType<JObject>()
                            .FirstOrDefault(obj => obj[sceneid] != null);

                        if (existingObject != null && existingObject.TryGetValue(sceneid, out JToken? value))
                            return value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SSFW] - SSFWGetLegacyFurnitureLayoutProcess errored out with this exception - {ex}");
            }

            return null;
        }

        public Dictionary<string, string> SSFWGetLegacyFurnitureLayouts(string filePath)
        {
            Dictionary<string, string> outputDictionary = new();
            try
            {
                if (File.Exists(filePath))
                {
                    string? json = FileHelper.ReadAllText(filePath, key);
                    if (!string.IsNullOrEmpty(json))
                    {
                        foreach (JObject obj in JArray.Parse(json).OfType<JObject>())
                        {
                            foreach (JProperty? property in obj.Properties())
                            {
                                outputDictionary.Add(property.Name, property.Value.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SSFW] - SSFWGetLegacyFurnitureLayouts errored out with this exception - {ex}");
            }

            return outputDictionary;
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
        // ~SSFWLayoutService()
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
