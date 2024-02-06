using System.IO.Compression;

namespace BackendProject.MiscUtils
{
    public static class StaticFileSystemUtils
    {
        public static IEnumerable<FileSystemInfo> AllFilesAndFolders(this DirectoryInfo dir)
        {
            foreach (FileInfo f in dir.GetFiles())
                yield return f;
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                yield return d;
                foreach (FileSystemInfo o in AllFilesAndFolders(d))
                    yield return o;
            }
        }
    }

    public class FileSystemUtils : IDisposable
    {
        private bool disposedValue;

        public Stream? ProcessAndZipFolder(string directoryPath)
        {
            if (Directory.Exists(directoryPath) && (File.GetAttributes(directoryPath) & FileAttributes.Archive) == FileAttributes.Archive) // We only process folders with the archive attribute.
            {
                HugeMemoryStream ZipStream = new();

                DirectoryInfo from = new(directoryPath);
                using (ZipArchive archive = new(ZipStream, ZipArchiveMode.Create))
                {
                    foreach (FileInfo file in from.AllFilesAndFolders().OfType<FileInfo>())
                    {
                        if ((File.GetAttributes(file.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden)
                            _ = archive.CreateEntryFromFile(file.FullName, Path.GetFileName(file.FullName), CompressionLevel.Fastest);
                    }
                }

                ZipStream.Seek(0, SeekOrigin.Begin);

                return ZipStream;
            }

            return null;
        }

        public Stream? ProcessFolderToMultiPartFormData(string directoryPath)
        {
            if (Directory.Exists(directoryPath) && (File.GetAttributes(directoryPath) & FileAttributes.Archive) == FileAttributes.Archive) // We only process folders with the archive attribute.
            {
                using (MultipartFormDataContent formData = new())
                {
                    // Add each file to the multipart form data
                    foreach (string filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
                    {
                        if ((File.GetAttributes(filePath) & FileAttributes.Hidden) != FileAttributes.Hidden)
                            formData.Add(new StreamContent(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)), Path.GetFileName(filePath), filePath);
                    }

                    HugeMemoryStream DataStream = new(formData.ReadAsStreamAsync().Result);

                    DataStream.Seek(0, SeekOrigin.Begin);

                    return DataStream;
                }
            }

            return null;
        }

        public Dictionary<string, string>? GetMediaFilesList(string directoryPath)
        {
            Dictionary<string, string> files = new();

            if (Directory.Exists(directoryPath))
            {
                DirectoryInfo from = new(directoryPath);
                Parallel.ForEach(from.AllFilesAndFolders().OfType<FileInfo>().Where(f =>
                                f.Extension.ToLower() == ".mp4" ||
                                f.Extension.ToLower() == ".mkv" ||
                                f.Extension.ToLower() == ".m4a" ||
                                f.Extension.ToLower() == ".mp3" ||
                                f.Extension.ToLower() == ".ogg" ||
                                f.Extension.ToLower() == ".wav"), file => {
                                    if ((File.GetAttributes(file.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden)
                                    {
                                        lock (files)
                                        {
                                            files.Add(Path.GetFileNameWithoutExtension(file.FullName), $"http://{VariousUtils.GetPublicIPAddress(true, true)}/" + Path.GetRelativePath(directoryPath, file.FullName).Replace("\\", "/"));
                                        }
                                    }
                                });
            }

            if (files.Count > 0)
                return files;

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~FileSystemUtils()
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
