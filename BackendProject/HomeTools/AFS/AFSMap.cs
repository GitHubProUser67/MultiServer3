using System.Text.RegularExpressions;

namespace BackendProject.HomeTools.AFS
{
    public class AFSMap
    {
        public static async Task SubHashMapBatch(string CurrentFolder, string prefix, string? FileContent)
        {
            if (!string.IsNullOrEmpty(FileContent))
            {
                foreach (MappedList match in new List<MappedList>(AFSRegexProcessor.ScanForString(FileContent)))
                {
                    if (!string.IsNullOrEmpty(match.file))
                    {
                        string text = AFSHash.EscapeString(match.file);

                        if (text.ToLower().EndsWith(".atmos"))
                        {
                            string cdatapath = text.Remove(text.Length - 6) + ".cdata"; // Eboot does this by default.

                            // Search for files with names matching the CRC hash, regardless of the extension
                            foreach (string filePath in Directory.GetFiles(CurrentFolder)
                              .Where(path => new Regex($"(?:0X)?{AFSHash.ComputeAFSHash(cdatapath)}(?:\\..*)?").IsMatch(Path.GetFileName(path)))
                              .ToArray())
                            {
                                string NewfilePath = CurrentFolder + $"/{cdatapath}";
                                string? destinationDirectory = Path.GetDirectoryName(NewfilePath);

                                if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                                    Directory.CreateDirectory(destinationDirectory);

                                if (!File.Exists(NewfilePath))
                                    File.Move(filePath, NewfilePath);
                            }
                        }

                        // Search for files with names matching the CRC hash, regardless of the extension
                        foreach (string filePath in Directory.GetFiles(CurrentFolder)
                          .Where(path => new Regex($"(?:0X)?{AFSHash.ComputeAFSHash(prefix + text)}(?:\\..*)?").IsMatch(Path.GetFileName(path)))
                          .ToArray())
                        {
                            string NewfilePath = CurrentFolder + $"/{text}";
                            string? destinationDirectory = Path.GetDirectoryName(NewfilePath);

                            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                                Directory.CreateDirectory(destinationDirectory);

                            if (!File.Exists(NewfilePath))
                                File.Move(filePath, NewfilePath);

                            if (File.Exists(NewfilePath) && (NewfilePath.ToLower().EndsWith(".mdl") || NewfilePath.ToLower().EndsWith(".atmos")
                            || NewfilePath.ToLower().EndsWith(".efx") || NewfilePath.ToLower().EndsWith(".xml") || NewfilePath.ToLower().EndsWith(".scene")
                            || NewfilePath.ToLower().EndsWith(".map") || NewfilePath.ToLower().EndsWith(".lua") || NewfilePath.ToLower().EndsWith(".luac")
                            || NewfilePath.ToLower().EndsWith(".unknown")))
                                await SubHashMapBatch(CurrentFolder, prefix, File.ReadAllText(NewfilePath));
                        }
                    }
                }
            }
        }
    }
}
