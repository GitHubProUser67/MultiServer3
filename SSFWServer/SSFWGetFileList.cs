using CustomLogger;
using Newtonsoft.Json;

namespace SSFWServer
{
    public class SSFWGetFileList : IDisposable
    {
        private bool disposedValue;

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
                    {
                        FilesContainer container = new FilesContainer { files = files };

                        return JsonConvert.SerializeObject(container, Formatting.Indented);
                    }
                }
            }
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[SSFW] - FILELIST ERROR: {e}");
            }

            return null;
        }

        private List<FileItem>? GetFilesInfo(string directoryPath)
        {
            List<FileItem> files = new List<FileItem>();
            try
            {

                foreach (string filePath in Directory.GetFiles(directoryPath).Where(name => name.EndsWith(".json")))
                {
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
            catch (Exception e)
            {
                LoggerAccessor.LogError($"[SSFW] - GETFILELIST ERROR: {e}");
            }

            return null;
        }

        private class FileItem
        {
            public string? objectId { get; set; }
            public int size { get; set; }
            public string? lastUpdate { get; set; }
        }

        private class FilesContainer
        {
            public List<FileItem>? files { get; set; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: supprimer l'état managé (objets managés)
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~SSFWGetFileList()
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
