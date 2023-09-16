using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MultiServer.HTTPService.Addons.PlayStationHome.SSFW
{
    public class SSFWGetFileList
    {
        public static Task SSFWSaveDataDebugGetFileList(string directoryPath, string absolutepath, HttpListenerRequest request, HttpListenerResponse response)
        {
            string jsonString = null;
            try
            {
                List<FileItem> files = GetFilesInfo(directoryPath + "/" + request.Url.Segments.LastOrDefault());

                ServerConfiguration.LogWarn(files.ToString());

                FilesContainer container = new FilesContainer { files = files };

                jsonString = JsonConvert.SerializeObject(container, Formatting.Indented);

                byte[] buffer = Encoding.UTF8.GetBytes("PUT_FILES_HERE".Replace("PUT_FILES_HERE", jsonString));

                response.StatusCode = 200;
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(absolutepath, ex);
                response.StatusCode = 500;
            }

            return Task.CompletedTask;
        }

        private static List<FileItem> GetFilesInfo(string directoryPath)
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
