using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using BackendProject.FileHelper;

namespace SSFWServer
{
    public class SSFWAvatarLayoutService : IDisposable
    {
        private string? key;
        private bool disposedValue;

        public SSFWAvatarLayoutService(string sessionid, string? key)
        {
            this.key = key;
        }

        public bool HandleAvatarLayout(byte[] buffer, string directorypath, string filepath, string absolutepath, bool delete)
        {
            // Define the regular expression pattern to match a number at the end of the string
            Regex regex = new(@"\d+(?![\d/])$");

            // Check if the string ends with a number
            if (regex.IsMatch(absolutepath))
            {
                // Get the matched number as a string
                string numberString = regex.Match(absolutepath).Value;

                // Check if the number is valid
                if (int.TryParse(numberString, out int number))
                {
                    SSFWUpdateAvatar(directorypath + "/list.json", number, delete);

                    if (delete && File.Exists(filepath + ".json"))
                    {
                        File.Delete(filepath + ".json");
                        return true;
                    }
                    else if (buffer != null)
                    {
                        Directory.CreateDirectory(directorypath);
                        File.WriteAllBytes($"{SSFWServerConfiguration.SSFWStaticFolder}/{absolutepath}.json", buffer);
                        return true;
                    }
                }
            }

            return false;
        }

        public void SSFWUpdateAvatar(string filePath, int contentToUpdate, bool delete)
        {
            try
            {
                string? json = null;

                if (File.Exists(filePath))
                    json = FileHelper.ReadAllText(filePath, key);

                if (json != null)
                {
                    // Parse the JSON content
                    JArray jsonArray = JArray.Parse(json);

                    if (delete)
                        // Remove the element(s) with the specified number
                        jsonArray = new JArray(jsonArray.Where(item => item.Value<int>(string.Empty) != contentToUpdate));
                    else
                    {
                        // Create a new JSON object with the specified number
                        JObject jsonObject = new()
                        {
                            [string.Empty] = contentToUpdate
                        };

                        // Check if the element already exists
                        JToken? existingItem = jsonArray.FirstOrDefault(item => item.Value<int>(string.Empty) == contentToUpdate);

                        if (existingItem != null)
                            // Update the existing element
                            existingItem.Replace(jsonObject);
                        else
                            // Add the new element to the array
                            jsonArray.Add(jsonObject);
                    }

                    File.WriteAllText(filePath, jsonArray.ToString());
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[SSFW] - SSFWUpdateAvatar errored out with this exception - {ex}");
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
        // ~SSFWAvatarLayoutService()
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
