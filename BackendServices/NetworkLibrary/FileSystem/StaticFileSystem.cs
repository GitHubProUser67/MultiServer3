using CustomLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetworkLibrary.FileSystem
{
    public static class StaticFileSystem
    {
        public static IEnumerable<FileSystemInfo> AllFilesAndFolders(this DirectoryInfo directory)
        {
            if ((directory.Attributes & FileAttributes.Hidden) == 0)
            {
                foreach (FileInfo f in directory.GetFiles().Where(file => (file.Attributes & FileAttributes.Hidden) == 0))
                    yield return f;
                foreach (DirectoryInfo d in directory.GetDirectories().Where(dir => (dir.Attributes & FileAttributes.Hidden) == 0))
                {
                    yield return d;
                    foreach (FileSystemInfo o in d.AllFilesAndFolders())
                        yield return o;
                }
            }
        }

        public static IEnumerable<FileSystemInfo> AllFilesAndFoldersLinq(this DirectoryInfo dir)
        {
            return dir.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount)
                .AsUnordered().Where(info => (info.Attributes & FileAttributes.Hidden) == 0);
        }

        public static IEnumerable<string> GetMediaFilesList(string directoryPath)
        {
            // Define a set of valid extensions for media quick lookup
            HashSet<string> validExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".mp3", ".aac", ".ts" };

            return Directory.EnumerateFiles(directoryPath, "*.*")
                            .Where(s => validExtensions.Contains(Path.GetExtension(s)) && !File.GetAttributes(s)
                            .HasFlag(FileAttributes.Hidden));
        }

        public static string GetM3UStreamFromDirectory(string directoryPath, string httpdirectoryrequest)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    IEnumerable<string> MediaPaths = GetMediaFilesList(directoryPath);

                    if (MediaPaths?.Any() == true)
                    {
                        StringBuilder builder = new StringBuilder();

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
