using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Text;
using CryptoSporidium.FileHelper;

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

            string inputurlfortrim = absolutepath;
            string[] words = inputurlfortrim.Split('/');

            if (words.Length > 0)
                inputurlfortrim = words[words.Length - 1];

            if (inputurlfortrim != absolutepath) // If ends with UUID Ok.
            {
                SSFWFurnitureLayoutProcess(directorypath + "/mylayout.json", buffer, inputurlfortrim);
                return true;
            }
            
            return false;
        }

        public string? HandleLayoutServiceGET(string directorypath, string absolutepath)
        {
            string inputurlfortrim = absolutepath;
            string[] words = inputurlfortrim.Split('/');

            if (words.Length > 0)
                inputurlfortrim = words[words.Length - 1];

            if (inputurlfortrim != absolutepath) // If ends with UUID Ok.
            {

                string? stringlayout = SSFWGetFurnitureLayoutProcess(directorypath + "/mylayout.json", inputurlfortrim);

                if (stringlayout != null)
                    return $"[{{\"{inputurlfortrim}\":{stringlayout}}}]";
                else
                    return string.Empty;
            }

            return null;
        }

        public void SSFWFurnitureLayoutProcess(string filePath, byte[] postData, string objectId)
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
                    if (obj != null && obj.Properties().Any(p => p.Name == objectId))
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
                        existingObject[objectId] = JObject.Parse(Encoding.UTF8.GetString(postData));
                }
                else
                {
                    // Create a new object with the objectId and POST data
                    JObject newObject = new JObject
                    {
                        { objectId, JObject.Parse(Encoding.UTF8.GetString(postData)) }
                    };

                    // Add the new object to the JSON array
                    jsonArray.Add(newObject);
                }

                File.WriteAllText(filePath, jsonArray.ToString());
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SSFW] - SSFWFurnitureLayout errored out with this exception - {ex}");
            }
        }

        public string? SSFWGetFurnitureLayoutProcess(string filePath, string objectId)
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
                            .FirstOrDefault(obj => obj[objectId] != null);

                        if (existingObject != null && existingObject.TryGetValue(objectId, out JToken? value))
                            return value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SSFW] - SSFWGetFurnitureLayout errored out with this exception - {ex}");
            }

            return null;
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
