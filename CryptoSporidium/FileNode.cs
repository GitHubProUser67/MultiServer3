using CustomLogger;
using Newtonsoft.Json;

namespace CryptoSporidium
{
    public class FileNode
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public long? Size { get; set; }
        public DateTime CreationDate { get; set; }
        public List<FileNode>? Childrens { get; set; }
    }

    public class FileStructure
    {
        public FileNode? root { get; set; }
    }

    public class FileStructureToJson
    {
        public static string GetFileStructureAsJson(string rootDirectory)
        {
            try
            {
                if (!Directory.Exists(rootDirectory))
                    return "[]";

                var rootFileNode = CreateFileNode(rootDirectory);
                var fileStructure = new FileStructure { root = rootFileNode };

                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                return JsonConvert.SerializeObject(fileStructure, Formatting.Indented, jsonSettings);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[GetFileStructureAsJson] - Errored out with exception - {ex}");
            }

            return "[]";
        }

        private static bool IsHidden(FileSystemInfo fileSystemInfo)
        {
            return (fileSystemInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        }

        private static FileNode? CreateFileNode(string directoryPath)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                var fileNode = new FileNode
                {
                    Name = directoryInfo.Name,
                    Type = "Directory",
                    CreationDate = directoryInfo.CreationTimeUtc,
                    Childrens = new List<FileNode>()
                };

                foreach (var file in directoryInfo.GetFiles())
                {
                    if (!IsHidden(file))
                        fileNode.Childrens.Add(new FileNode
                        {
                            Name = file.Name,
                            Type = HTTPUtils.mimeTypes[Path.GetExtension(file.FullName)],
                            Size = file.Length,
                            CreationDate = file.CreationTimeUtc
                        });
                }

                foreach (var subdirectory in directoryInfo.GetDirectories())
                {
                    if (!IsHidden(subdirectory))
                        fileNode.Childrens.Add(new FileNode
                        {
                            Name = subdirectory.Name,
                            Type = "Directory",
                            CreationDate = subdirectory.CreationTimeUtc
                        });
                }

                return fileNode;
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[CreateFileNode] - Errored out with exception - {ex}");
            }

            return null;
        }
    }
}
