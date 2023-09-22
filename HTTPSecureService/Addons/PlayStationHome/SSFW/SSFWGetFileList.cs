using System.Text;
using System.Net;
using Newtonsoft.Json;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.SSFW
{
    public class SSFWGetFileList
    {
        public static void SSFWSaveDataDebugGetFileList(string directoryPath, string absolutepath, HttpResponse response, string[] segments)
        {
            string jsonString = null;
            try
            {
                List<FileItem> files = GetFilesInfo(directoryPath + "/" + segments);

                ServerConfiguration.LogWarn(files.ToString());

                FilesContainer container = new FilesContainer { files = files };

                jsonString = JsonConvert.SerializeObject(container, Formatting.Indented);

                byte[] buffer = Encoding.UTF8.GetBytes("PUT_FILES_HERE".Replace("PUT_FILES_HERE", jsonString));

                response.SetBegin((int)HttpStatusCode.OK);
                response.SetContentType("application/json");
                response.SetBody(buffer);
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(absolutepath, ex);
                response.SetBegin((int)HttpStatusCode.InternalServerError);
                response.SetBody();
            }
        }

        public static List<FileItem> GetFilesInfo(string directoryPath)
        {
            List<FileItem> files = new List<FileItem>();
            try
            {

                foreach (string filePath in Directory.GetFiles(directoryPath).Where(name => name.EndsWith(".json")))
                {
                    ServerConfiguration.LogWarn($"{filePath}");
                    FileInfo fileInfo = new FileInfo(filePath);
                    FileItem fileItem = new FileItem
                    {
                        objectId = Path.GetFileNameWithoutExtension(fileInfo.Name),
                        size = (int)fileInfo.Length,
                        lastUpdate = "0"
                    };
                    files.Add(fileItem);
                }

                return files;
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"ERROR: {ex.Message}", ex);
                return null;
            }

        }

        public class FileItem
        {
            public string objectId { get; set; }
            public int size { get; set; }
            public string lastUpdate { get; set; }
        }

        public class FilesContainer
        {
            public List<FileItem> files { get; set; }
        }
    }
}
