using CryptoSporidium;
using HTTPServer.API;
using HTTPServer.Models;

namespace HTTPServer.RouteHandlers.staticRoutes
{
    public class Main
    {
        public static List<Route> index = new List<Route>() {
                new() {
                    Name = "Hello Handler",
                    UrlRegex = @"^/$",
                    Method = "GET",
                    Host = string.Empty,
                    Callable = (HttpRequest request) => {
                        bool handled = false;
                        foreach (string indexFile in HTTPUtils.DefaultDocuments)
                        {
                            if (File.Exists(Path.Combine(HTTPServerConfiguration.HTTPStaticFolder, indexFile)))
                            {
                                handled = true;

                                string? encoding = request.GetHeaderValue("Accept-Encoding");

                                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                                {
                                    using (FileStream stream = new(indexFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                    {
                                        byte[]? buffer = null;

                                        using (MemoryStream ms = new())
                                        {
                                            stream.CopyTo(ms);
                                            buffer = ms.ToArray();
                                            ms.Flush();
                                        }

                                        stream.Flush();

                                        return HttpResponse.Send(HTTPUtils.Compress(buffer), "text/html", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                                    }
                                }
                                else
                                    return HttpResponse.Send(new FileStream(indexFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), "text/html");
                            }
                        }

                        if (!handled)
                        {
                            return new HttpResponse(false)
                                {
                                    HttpStatusCode = HttpStatusCode.NotFound,
                                    ContentAsUTF8 = string.Empty
                                };
                        }

                        return new HttpResponse(false)
                            {
                                HttpStatusCode = HttpStatusCode.InternalServerError,
                                ContentAsUTF8 = string.Empty
                            };
                     }
                },
                new() {
                    Name = "UFC Home Connector",
                    UrlRegex = "/index.php",
                    Method = "POST",
                    Host = "sonyhome.thqsandbox.com",
                    Callable = (HttpRequest request) => {
                        if (request.getDataStream != null)
                        {
                            string? UFCResult = THQ.ProcessUFCUserData(request.getDataStream, HTTPUtils.ExtractBoundary(request.GetContentType()));
                            if (!string.IsNullOrEmpty(UFCResult))
                                return HttpResponse.Send(UFCResult, "text/xml");
                        }

                        return new HttpResponse(false)
                            {
                                HttpStatusCode = HttpStatusCode.InternalServerError,
                                ContentAsUTF8 = string.Empty
                            };
                     }
                }
            };
    }
}
