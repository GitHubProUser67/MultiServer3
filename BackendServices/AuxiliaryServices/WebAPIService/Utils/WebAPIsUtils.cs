using CustomLogger;
using System.IO.Compression;
using System.IO;
using System;
using HashLib;

namespace WebAPIService.Utils
{
    public class WebAPIsUtils
    {
        public static string GenerateDynamicCacheGuid(string input)
        {
            return NetHasher.ComputeMD5String(GetCurrentDateTime() + input + NetworkLibrary.SSL.SSLUtils.GetRandomInt64(long.MinValue, long.MaxValue));
        }

        public static void AddFileToZip(ZipArchive archive, string entryName, Stream contentStream)
        {
            contentStream.Position = 0;

            // Create a new entry in the zip archive
            ZipArchiveEntry entry = archive.CreateEntry(entryName);

            // Write content to the entry
            using (Stream entryStream = entry.Open())
                contentStream.CopyTo(entryStream);
        }

        public static void UncompressFile(string compressedFilePath, string extractionFolderPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(compressedFilePath, extractionFolderPath);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[File Uncompress] - An error occurred: {ex}");
            }
        }


        /// <summary>
        /// Get the current date-time.
        /// <para>Obtenir la date actuelle.</para>
        /// </summary>
        /// <returns>A string.</returns>
        public static string GetCurrentDateTime()
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}{GetNanoseconds()}";
        }

        /// <summary>
        /// Get Nanoseconds of the current date-time.
        /// <para>Obtenir la date actuelle avec une �valuation en nano-secondes.</para>
        /// </summary>
        /// <returns>A string.</returns>
        public static string GetNanoseconds()
        {
            // C# DateTime only provides up to ticks (100 nanoseconds) resolution
            return (DateTime.Now.Ticks % TimeSpan.TicksPerMillisecond * 100).ToString("00000000"); // Pad with zeros to 8 digits
        }
    }
}
