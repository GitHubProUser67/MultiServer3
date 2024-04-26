using CyberBackendLibrary.AIModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyberBackendLibrary.HTTP
{
    public class DefaultHTMLPages
    {
        public static ConcurrentDictionary<string, Tuple<DateTime, List<string>>> AINotFoundGuessingResultCache = new();

        public static Task<string> GenerateNotFound(string absolutepathUrl, string urlBase, string directoryPath, string HttpRootFolder, string serverSignature, string serverPort, bool AIAssistant)
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

            if (AIAssistant)
            {
                List<string>? Urls = null;

                // The idea behind this logic, is to cache results to avoid as much as possible to trigger the file/directory seaching which is very costly on large data storage.

                if (AINotFoundGuessingResultCache.TryGetValue(absolutepathUrl, out Tuple<DateTime, List<string>>? value) && (DateTime.Now - value.Item1).TotalMinutes <= 30) // We renew cache entry after 30 minutes of validity.
                    Urls = value.Item2;
                else
                {
                    Urls = WebMachineLearning.GenerateUrlsSuggestionsFromDirectory(absolutepathUrl, urlBase, HttpRootFolder, directoryPath, 0.1);

                    if (AINotFoundGuessingResultCache.ContainsKey(absolutepathUrl))
                        AINotFoundGuessingResultCache[absolutepathUrl] = Tuple.Create(DateTime.Now, Urls);
                    else
                        AINotFoundGuessingResultCache.TryAdd(absolutepathUrl, Tuple.Create(DateTime.Now, Urls));
                }

                if (Urls != null && Urls.Count != 0)
                {
                    HTMLContent += $@"<p>The requested URL was not found on this server, but we found some ressources you might want.</p>
                                  <button onclick=""toggleList()"">Toggle Suggested URL List</button>
                                  <ul id=""urlList"" style=""display: none;"">";

                    foreach (string Url in Urls)
                    {
                        HTMLContent += $"<li><a href=\"{Url}\">{Url}</a></li>";
                    }

                    HTMLContent += $@"</ul><hr><address>{serverSignature} Server at Port {serverPort}</address></body></html>";
                }
                else
                    HTMLContent += $@"<p>The requested URL was not found on this server.</p>
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
