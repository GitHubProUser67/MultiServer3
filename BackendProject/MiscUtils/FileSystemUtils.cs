using HttpMultipartParser;
using System.IO.Compression;
using System.Net.Http.Headers;

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

        public HugeMemoryStream? ZipStream { get; set; }

        public void ProcessAndZipFolder(string directoryPath)
        {
            if ((File.GetAttributes(directoryPath) & FileAttributes.Archive) == FileAttributes.Archive)
            {
                ZipStream = new();

                DirectoryInfo from = new(directoryPath);
                using (ZipArchive archive = new(ZipStream, ZipArchiveMode.Create))
                {
                    foreach (FileInfo file in from.AllFilesAndFolders().OfType<FileInfo>())
                    {
                        _ = archive.CreateEntryFromFile(file.FullName, file.FullName[(from.FullName.Length + 1)..], CompressionLevel.Fastest);
                    }
                }

                ZipStream.Seek(0, SeekOrigin.Begin);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ZipStream?.Close();
                    ZipStream?.Dispose();
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
