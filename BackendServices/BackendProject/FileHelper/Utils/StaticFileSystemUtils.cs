using CustomLogger;
using System.Text;

namespace BackendProject.FileHelper.Utils
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
                foreach (FileSystemInfo o in d.AllFilesAndFolders())
                    yield return o;
            }
        }

        public static IEnumerable<string>? GetMediaFilesList(string directoryPath)
        {
            return Directory.EnumerateFiles(directoryPath, "*.*")
            .Where(s => (s.ToLower().EndsWith(".mp3") || s.ToLower().EndsWith(".aac") || s.ToLower().EndsWith(".ts")) && !File.GetAttributes(s).HasFlag(FileAttributes.Hidden));
        }

        public static string? GetM3UStreamFromDirectory(string directoryPath, string httpdirectoryrequest)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    IEnumerable<string>? MediaPaths = GetMediaFilesList(directoryPath);

                    if (MediaPaths?.Any() == true)
                    {
                        StringBuilder builder = new();

                        // Add M3U header
                        builder.AppendLine("#EXTM3U");

                        foreach (string mediaPath in MediaPaths)
                        {
                            string fileName = Path.GetFileName(mediaPath);

                            // Add the song path to the playlist
                            builder.AppendLine($"#EXTINF:,-1,{fileName}");
                            builder.AppendLine(httpdirectoryrequest + $"/{fileName}");
                        }

                        return builder.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[GetM3UStreamFromDirectory] - Errored out with exception - {ex}");
            }

            return null;
        }
    }
}
