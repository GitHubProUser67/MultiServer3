using CyberBackendLibrary.AIModels;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace CyberBackendLibrary.HTTP
{
    public class DefaultHTMLPages
    {
        public static ConcurrentDictionary<string, Tuple<DateTime, List<string>>> AINotFoundGuessingResultCache = new ConcurrentDictionary<string, Tuple<DateTime, List<string>>>();
        private static SemaphoreSlim searchSemaphore = new SemaphoreSlim(Environment.ProcessorCount); // Limit to number-of-CPU-cores concurrent searches

        public static Task<string> GenerateNotFound(string absolutepathUrl, string urlBase, string HttpRootFolder, string serverSignature, string serverPort, bool AIAssistant)
        {
            string HTMLContent = $@"<!DOCTYPE html PUBLIC ""-//IETF//DTD HTML 2.0//EN"">
                                <html><head><meta http-equiv=""Content-Type"" content=""text/html; charset=windows-1252"">
                                <title>404 Not Found</title>
                                <style>.hiclass {{background-color: rgb(51, 144, 255); color: white}}</style>
                                <script>
                                    function toggleList() {{
                                        var list = document.getElementById('urlList');
                                        list.style.display = list.style.display === 'none' ? 'block' : 'none';
                                    }}
                                </script>
                                </head><body>
                                <h1>Not Found</h1>";

            if (AIAssistant && WebMachineLearning.IsAvailable())
            {
                List<string> Urls = null;

                // The idea behind this logic, is to prevent too much processing for the same url.
                if (AINotFoundGuessingResultCache.TryGetValue(absolutepathUrl, out Tuple<DateTime, List<string>> value) && (DateTime.Now - value.Item1).TotalMinutes <= 30) // We renew cache entry after 30 minutes of validity.
                    Urls = value.Item2;
                else if (searchSemaphore.Wait(0)) // We not await for the semaphore, rather we check if yes or not we can enter.
                {
                    Urls = WebMachineLearning.GenerateUrlsSuggestions(absolutepathUrl, HttpRootFolder, 0.1)?.Where(url => url != null).ToList();

                    if (Urls != null)
                    {
                        if (AINotFoundGuessingResultCache.ContainsKey(absolutepathUrl))
                            AINotFoundGuessingResultCache[absolutepathUrl] = Tuple.Create(DateTime.Now, Urls);
                        else
                            AINotFoundGuessingResultCache.TryAdd(absolutepathUrl, Tuple.Create(DateTime.Now, Urls));
                    }

                    searchSemaphore.Release();
                }

                if (Urls != null && Urls.Count != 0)
                {
                    HTMLContent += $@"<p>The requested URL was not found on this server, but we found some ressources you might want.</p>
                          <button onclick=""toggleList()"">Toggle Suggested URL List</button>
                          <ul id=""urlList"" style=""display: none;"">";

                    foreach (string Url in Urls)
                    {
                        HTMLContent += $"<li><a href=\"{urlBase + Url}\">{urlBase + Url}</a></li>";
                    }

                    HTMLContent += $@"</ul><hr><address>{serverSignature} Server at Port {serverPort}</address></body></html>";
                }
                else
                    HTMLContent += $@"<p>The requested URL was not found on this server, but there might be some suggestions in the future, please check again later.</p>
                          <hr>
                          <address>{serverSignature} Server at Port {serverPort}</address>";
            }
            else
                HTMLContent += $@"<p>The requested URL was not found on this server.</p>
                                  <hr>
                                  <address>{serverSignature} Server at Port {serverPort}</address>";

            return Task.FromResult(HTMLContent);
        }
    }
}
