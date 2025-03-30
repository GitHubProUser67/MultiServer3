using CustomLogger;
using Newtonsoft.Json;

namespace SSFWServer.Helpers
{
    public class SSFWGetFileList
    {
        public SSFWGetFileList()
        {

        }

        public string? SSFWSaveDataDebugGetFileList(string directoryPath, string? segment)
        {
            try
            {
                if (segment != null)
                {
                    List<FileItem>? files = GetFilesInfo(directoryPath + "/" + segment);

                    if (files != null)
                        return JsonConvert.SerializeObject(new FilesContainer() { Files = files }, Formatting.Indented);

#if DEBUG
                    LoggerAccessor.LogInfo($"[SSFW] - SaveDataDebug GetFileList Returned: \n{JsonConvert.SerializeObject(new FilesContainer() { Files = files }, Formatting.Indented)}");
#endif
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[SSFW] - SaveDataDebug GetFileList ERROR: \n{e}");
            }

            return null;
        }

        private List<FileItem>? GetFilesInfo(string directoryPath)
        {
            List<FileItem> files = new();
            try
            {

                foreach (string filePath in Directory.GetFiles(directoryPath).Where(name => name.EndsWith(".json")))
                {
                    FileInfo fileInfo = new(filePath);
                    files.Add(new FileItem()
                    {
                        ObjectId = Path.GetFileNameWithoutExtension(fileInfo.Name),
                        Size = (int)fileInfo.Length,
                        LastUpdate = "0"
                    });
                }

                return files;
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[SSFW] - SaveDataDebug GetFileList ERROR: \n{e}");
            }

            return null;
        }

        private class FileItem
        {
            public string? ObjectId { get; set; }
            public int Size { get; set; }
            public string? LastUpdate { get; set; }
        }

        private class FilesContainer
        {
            public List<FileItem>? Files { get; set; }
        }

    }
}
