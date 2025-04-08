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
                    string path = directoryPath + "/" + segment;
                    List<FileItem>? files = GetFilesInfo(path);

                    if (files != null)
                        return JsonConvert.SerializeObject(new FilesContainer() { files = files }, Formatting.Indented);

#if DEBUG
                    LoggerAccessor.LogInfo($"[SSFW] - SaveDataDebug GetFileList Returned: \n{JsonConvert.SerializeObject(new FilesContainer() { files = files }, Formatting.Indented)}");
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

                foreach (string filePath in Directory.GetFiles(directoryPath))
                {
                    FileInfo fileInfo = new(filePath);
                    files.Add(new FileItem()
                    {
                        objectId = Path.GetFileNameWithoutExtension(fileInfo.Name),
                        size = (int)fileInfo.Length,
                        lastUpdate = (long)fileInfo.LastWriteTime.Subtract(DateTime.UnixEpoch).TotalSeconds
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
            public string? objectId { get; set; }
            public int size { get; set; }
            public long lastUpdate { get; set; }
        }

        private class FilesContainer
        {
            public List<FileItem>? files { get; set; }
        }

    }
}
