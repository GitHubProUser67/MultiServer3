using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace HorizonService.LIBRARY.Database.Simulated
{
    public class JsonMacBanController
    {
        public static void WriteToJsonFile(string? directoryPath, string CID)
        {
            if (string.IsNullOrEmpty(directoryPath))
                return;

            try
            {
                Directory.CreateDirectory(directoryPath + "/MediusSimulatedDatabase/MacDatabase");

                File.WriteAllText(directoryPath + $"/MediusSimulatedDatabase/MacDatabase/{CID}.json", JsonConvert.SerializeObject(new Dictionary<string, bool> { { "IsBanned", false } }));
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"[JsonMacBanController] - WriteToJsonFile errored out with exception: {ex}");
            }
        }

        public static (string, bool) ReadFromJsonFile(string? directoryPath, string CID)
        {
            if (string.IsNullOrEmpty(directoryPath))
                return ("INVALIDFOLDER", false);

            try
            {
                if (File.Exists(directoryPath + $"/MediusSimulatedDatabase/MacDatabase/{CID}.json"))
                {
                    string json = File.ReadAllText(directoryPath + $"/MediusSimulatedDatabase/MacDatabase/{CID}.json");

                    Dictionary<string, bool>? JsonOutput = JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);

                    if (JsonOutput != null && JsonOutput.ContainsKey("IsBanned"))
                        return ("OK", JsonOutput["IsBanned"]);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.LoggerAccessor.LogError($"[JsonMacBanController] - ReadFromJsonFile errored out with exception: {ex}");
            }

            return ("NOTFOUND", false);
        }
    }
}
