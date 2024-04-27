using CyberBackendLibrary.FileSystem;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CyberBackendLibrary.AIModels
{
    // Self contained AI engine for HTTP driven use.
    public class WebMachineLearning
    {
        private static object _Lock = new();
        public static IEnumerable<FileSystemInfo>? fileSystemCache = null;

        /// <summary>
        /// Generates a assending list of potential matching Urls from a given text.
        /// <para>Génère une liste d'Urls potentiel depuis un text donné.</para>
        /// <param name="inputUrl">The text to compare against.</param>
        /// <param name="urlBase">The httpBase url.</param>
        /// <param name="HttpRootFolder">The root folder of the http server.</param>
        /// <param name="directoryPath">The directoryPath to check in.</param>
        /// <param name="minSimilarity">The minimum cosine similarity threshold for including a URL in the suggestions.</param>
        /// <param name="MaxResults">The maximum amount of Urls in the result.</param>
        /// </summary>
        /// <returns>A list of nullable strings.</returns>
        public static List<string?>? GenerateUrlsSuggestions(string inputUrl, string HttpRootFolder,
            double minSimilarity, int MaxResults = 20)
        {
            if (fileSystemCache == null || string.IsNullOrEmpty(inputUrl) || string.IsNullOrEmpty(HttpRootFolder) || !fileSystemCache.Any())
                return null;

            HttpRootFolder = HttpRootFolder.Replace("\\", "/");

            // Calculate similarity between inputText and given list of Urls using cosine similarity
            return fileSystemCache
                .AsParallel()
                .AsUnordered()
                .WithDegreeOfParallelism(2)
                .Where(entry => inputUrl.Replace("\\", "/").Split('/').Any(segment => entry.FullName.Replace("\\", "/").Contains(segment, StringComparison.InvariantCultureIgnoreCase)))
                .Select(entry =>
                {
                    if (File.Exists(entry.FullName))
                        return entry.FullName.Replace("\\", "/").Replace(HttpRootFolder, string.Empty);
                    else if (Directory.Exists(entry.FullName))
                        return entry.FullName.Replace("\\", "/").Replace(HttpRootFolder, string.Empty) + "/";
                    else
                        return null;
                })
                .Where(url => url != null)
                .Select(url => new { Url = url, Similarity = CosineSimilarity(inputUrl, url) })
                .Where(x => x.Similarity >= minSimilarity) // Filter URLs based on similarity threshold
                .OrderByDescending(x => x.Similarity)
                .Select(x => x.Url)
                .Take(MaxResults)
                .ToList();
        }

        /// <summary>
        /// Calculates the cosine between 2 string.
        /// <para>Calcule le Cosinus entre 2 string.</para>
        /// <param name="text1">The first text.</param>
        /// <param name="text2">The second text.</param>
        /// </summary>
        /// <returns>A double.</returns>
        public static double CosineSimilarity(string text1, string? text2)
        {
            if (string.IsNullOrEmpty(text2)) return 0;

            // Tokenize text
            Regex regex = new(@"\W+");

            // Calculate word frequency
            Dictionary<string, int>? freq1 = regex.Split(text1.ToLower()).Where(token => token.Length > 0)
                .GroupBy(word => word).ToDictionary(g => g.Key, g => g.Count());
            Dictionary<string, int>? freq2 = regex.Split(text2.ToLower()).Where(token => token.Length > 0)
                .GroupBy(word => word).ToDictionary(g => g.Key, g => g.Count());

            // Calculate magnitude
            double magnitude1 = Math.Sqrt(freq1.Values.Sum(count => count * count));
            double magnitude2 = Math.Sqrt(freq2.Values.Sum(count => count * count));

            // Calculate cosine similarity
            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            // Calculate dot product and return.
            return freq1.Keys.Intersect(freq2.Keys).Sum(word => freq1[word] * freq2[word]) / (magnitude1 * magnitude2);
        }

        public static bool IsAvailable()
        {
            return !Monitor.IsEntered(_Lock);
        }

        public static void ScheduledfileSystemUpdate(object? state)
        {
            if (state != null)
            {
                string HttpRootFolder = (string)state;

                if (Directory.Exists(HttpRootFolder))
                {
                    CustomLogger.LoggerAccessor.LogWarn("[WebMachineLearning] - Caching fileSystem structure, this might longer on very slow hard drives...");

                    lock (_Lock)
                        fileSystemCache = StaticFileSystem.AllFilesAndFoldersLinq(new DirectoryInfo(HttpRootFolder));

                    CustomLogger.LoggerAccessor.LogInfo("[WebMachineLearning] - fileSystem Cache has been actualized and is ready!");
                }
            }
        }
    }
}
