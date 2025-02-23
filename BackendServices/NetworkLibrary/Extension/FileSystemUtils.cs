using CustomLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLibrary.Extension
{
    public static class FileSystemUtils
    {
        private const FileAttributes hiddenAttribute = FileAttributes.Hidden;

        public static IEnumerable<FileSystemInfo> AllFilesAndFolders(this DirectoryInfo directory)
        {
            if (!directory.IsHidden())
            {
                foreach (FileInfo f in directory.GetFiles().Where(file => !file.IsHidden()))
                    yield return f;
                foreach (DirectoryInfo d in directory.GetDirectories().Where(dir => !dir.IsHidden()))
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
                .AsUnordered().Where(info => !info.IsHidden());
        }

        public static IEnumerable<string> GetMediaFilesList(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return null;

            // Define a set of valid extensions for media quick lookup
            HashSet<string> validExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".mp3", ".aac", ".ts" };

            return Directory.EnumerateFiles(directoryPath, "*.*")
                            .Where(s => validExtensions.Contains(Path.GetExtension(s)) && !File.GetAttributes(s)
                            .HasFlag(hiddenAttribute));
        }

        public static bool IsHidden(this FileSystemInfo fileSystemInfo)
        {
            return (fileSystemInfo.Attributes & hiddenAttribute) == hiddenAttribute;
        }

        public static bool IsHidden(this DirectoryInfo directorySystemInfo)
        {
            return (directorySystemInfo.Attributes & hiddenAttribute) == hiddenAttribute;
        }

        public static bool IsHidden(this FileInfo fileInfo)
        {
            return (fileInfo.Attributes & hiddenAttribute) == hiddenAttribute;
        }

        // https://stackoverflow.com/questions/24279882/file-open-hangs-and-freezes-thread-when-accessing-a-local-file
        public static async Task<bool> IsLocked(this FileInfo file)
        {
            Task<bool> checkTask = Task.Run(() =>
            {
                try
                {
                    using (file.Open(FileMode.Open, FileAccess.Read, FileShare.None)) { };
                    return false;
                }
                catch
                {
                }
                return true;
            });
            Task delayTask = Task.Delay(1000);
            try
            {
                if ((await Task.WhenAny(checkTask, delayTask).ConfigureAwait(false)) == delayTask)
                    return true;
                else
                    return await checkTask.ConfigureAwait(false);
            }
            catch
            {
            }
            return true;
        }

        public static async Task<FileStream> TryOpen(string filePath, int AwaiterTimeoutInMS = -1)
        {
            if (AwaiterTimeoutInMS != -1)
            {
                FileInfo info;
                try
                {
                    info = new FileInfo(filePath);
                }
                catch
                {
                    return null;
                }
                const int lockCheckInterval = 100;
                int elapsedTime = 0;
                bool TimedOut = false;
                while (await IsLocked(info).ConfigureAwait(false))
                {
                    TimedOut = elapsedTime >= AwaiterTimeoutInMS;
                    if (TimedOut)
                        break;
                    await Task.Delay(lockCheckInterval).ConfigureAwait(false);
                    elapsedTime += lockCheckInterval;
                }
                if (TimedOut)
                    return null;
            }
            try
            {
                return File.OpenRead(filePath);
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// Reads a fragment of a file with a given indicator.
        /// <para>Lire un fragment de fichier avec un indicateur explicite.</para>
        /// </summary>
        /// <param name="filePath">The path of the desired file.</param>
        /// <param name="bytesToRead">The amount of desired fragment data.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ReadFileChunck(string filePath, int bytesToRead)
        {
            if (bytesToRead <= 0)
                throw new ArgumentOutOfRangeException(nameof(bytesToRead), "[FileSystemUtils] - ReadFileChunck() - Number of bytes to read must be greater than zero.");

            int bytesRead;
#if NET5_0_OR_GREATER
            Span<byte> result = new byte[bytesToRead];
#else
            byte[] result = new byte[bytesToRead];
#endif

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
#if NET5_0_OR_GREATER
                    bytesRead = reader.Read(result);
#else
                    bytesRead = reader.Read(result, 0, bytesToRead);
#endif
                }

                // If the file is less than 'bytesToRead', pad with null bytes
                if (bytesRead < bytesToRead)
                {
#if NET5_0_OR_GREATER
                    result[bytesRead..].Fill(0);
#else
                    Array.Clear(result, bytesRead, bytesToRead - bytesRead);
#endif
                }
            }

            return result.ToArray();
        }

        public static string GetM3UStreamFromDirectory(string directoryPath, string httpdirectoryrequest)
        {
            try
            {
                IEnumerable<string> MediaPaths = GetMediaFilesList(directoryPath);

                if (MediaPaths != null && MediaPaths.Any())
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
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[GetM3UStreamFromDirectory] - Errored out with exception - {ex}");
            }

            return null;
        }
    }
}
