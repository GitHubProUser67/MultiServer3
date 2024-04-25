using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CyberBackendLibrary.FileSystem;

namespace CyberBackendLibrary.AIModels
{
    // Self contained AI engine for HTTP driven use.
    public class WebMachineLearning
    {
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
        /// <returns>A list of strings.</returns>
        public static List<string> GenerateUrlsSuggestionsFromDirectory(string inputUrl, string urlBase, string HttpRootFolder,
            string directoryPath, double minSimilarity, int MaxResults = 10)
        {
            List<string> urls = new();

            if (string.IsNullOrEmpty(inputUrl) || string.IsNullOrEmpty(directoryPath) || string.IsNullOrEmpty(HttpRootFolder))
                return urls;

            string[] segments = inputUrl.Replace("\\", "/").Split('/');

            string? lastSegment = null;

            if (segments.Length != 0)
            {
                for (int i = segments.Length - 1; i >= 0; i--)
                {
                    if (!string.IsNullOrEmpty(segments[i]))
                    {
                        lastSegment = segments[i];
                        break;
                    }
                }

                // If the last segment is empty, use the previous one (if any)
                if (string.IsNullOrEmpty(lastSegment) && segments.Length >= 2)
                    lastSegment = segments[^2];
            }

            HttpRootFolder = HttpRootFolder.Replace("\\", "/");

            if (Directory.Exists(directoryPath) && !string.IsNullOrEmpty(lastSegment))
            {
                // We restrict a little our searching by checking only folders and files that contains last URI element.
                Parallel.ForEach(StaticFileSystem.AllFilesAndFolders(new DirectoryInfo(directoryPath), lastSegment, MaxResults), entry =>
                {
                    if (File.Exists(entry.FullName))
                    {
                        // It's a file, add its path to the list
                        lock (urls)
                            urls.Add(urlBase + entry.FullName.Replace("\\", "/").Replace(HttpRootFolder, string.Empty));
                    }
                    else if (Directory.Exists(entry.FullName))
                    {
                        // It's a directory, add its path to the list
                        lock (urls)
                            urls.Add(urlBase + entry.FullName.Replace("\\", "/").Replace(HttpRootFolder, string.Empty) + "/");
                    }
                });

                // Calculate similarity between inputText and given list of Urls using cosine similarity
                return urls.Select(url => new { Url = url, Similarity = CosineSimilarity(inputUrl, url) })
                                  .Where(x => x.Similarity >= minSimilarity) // Filter URLs based on similarity threshold
                                  .OrderByDescending(x => x.Similarity)
                                  .Select(x => x.Url)
                                  .Take(MaxResults)
                                  .ToList();
            }
            else if (Directory.Exists(HttpRootFolder) && !string.IsNullOrEmpty(lastSegment))
            {
                // We restrict a little our searching by checking only folders and files that contains last URI element.
                Parallel.ForEach(StaticFileSystem.AllFilesAndFolders(new DirectoryInfo(HttpRootFolder), lastSegment, MaxResults), entry =>
                {
                    if (File.Exists(entry.FullName))
                    {
                        // It's a file, add its path to the list
                        lock (urls)
                            urls.Add(urlBase + entry.FullName.Replace("\\", "/").Replace(HttpRootFolder, string.Empty));
                    }
                    else if (Directory.Exists(entry.FullName))
                    {
                        // It's a directory, add its path to the list
                        lock (urls)
                            urls.Add(urlBase + entry.FullName.Replace("\\", "/").Replace(HttpRootFolder, string.Empty) + "/");
                    }
                });

                // Calculate similarity between inputText and given list of Urls using cosine similarity
                return urls.Select(url => new { Url = url, Similarity = CosineSimilarity(inputUrl, url) })
                                  .Where(x => x.Similarity >= minSimilarity) // Filter URLs based on similarity threshold
                                  .OrderByDescending(x => x.Similarity)
                                  .Select(x => x.Url)
                                  .Take(MaxResults)
                                  .ToList();
            }

            return urls;
        }

        /// <summary>
        /// Calculates the cosine between 2 string.
        /// <para>Calcule le Cosinus entre 2 string.</para>
        /// <param name="text1">The first text.</param>
        /// <param name="text2">The second text.</param>
        /// </summary>
        /// <returns>A double.</returns>
        public static double CosineSimilarity(string text1, string text2)
        {
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
    }
}
