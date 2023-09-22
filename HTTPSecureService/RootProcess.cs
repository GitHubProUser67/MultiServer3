using MultiServer.HTTPService;
using NetCoreServer;
using System.Net;
using System.Text;

namespace MultiServer.HTTPSecureService
{
    public class RootProcess
    {
        public static void ProcessRootRequest(string filePath, string directoryPath, string crc32, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            if (File.Exists(filePath) && request.Url != null)
            {
                string inffile = directoryPath + $"{Path.GetFileNameWithoutExtension(filePath)}_INF";

                if (File.Exists(inffile))
                {
                    string infcontent = File.ReadAllText(inffile);

                    if (infcontent.Substring(0, 8) == crc32)
                    {
                        infcontent = infcontent.Substring(8);

                        ServerConfiguration.LogInfo($"[HTTPS] - {HTTPSClass.GetHeaderValue(Headers, "User-Agent")} Redirected to {infcontent}");

                        response.SetBegin((int)HttpStatusCode.Redirect); // You can use other redirect codes like 301, 302, etc.
                        response.SetHeader("Location", infcontent);
                        response.SetBody();
                        return;
                    }
                }

                bool compress = false;

                byte[] buffer = null;

                if (HTTPSClass.GetHeaderValue(Headers, "Accept-Encoding") != string.Empty && HTTPSClass.GetHeaderValue(Headers, "Accept-Encoding").Contains("gzip"))
                    compress = true;

                string ContentType = MimeTypes.GetMimeType(filePath);

                response.SetContentType(ContentType);

                buffer = FileHelper.CryptoReadAsync(filePath, HTTPPrivateKey.HTTPPrivatekey);

                if (buffer == null)
                {
                    response.SetBegin(500);
                    response.SetBody();
                    return;
                }
                else
                    response.SetBegin(200);

                if (ContentType.StartsWith("audio/") || ContentType.StartsWith("video/"))
                {
                    if (HTTPSClass.GetHeaderValue(Headers, "Accept") != string.Empty && HTTPSClass.GetHeaderValue(Headers, "Accept").Contains("text/html"))
                    {
                        // Generate an HTML page with the video element
                        string html = @"
                            <!DOCTYPE html>
                            <html>
                            <head>
                              <title>Media Page</title>
                              <style>
                                body {
                                  display: flex;
                                  justify-content: center;
                                  align-items: center;
                                  height: 100vh;
                                  margin: 0;
                                  background-color: black;
                                }
                                #video-container {
                                  max-width: 80%;
                                  max-height: 80%;
                                }
                                video {
                                  width: 100%;
                                  height: 100%;
                                  object-fit: contain;
                                }
                              </style>
                            </head>
                            <body>
                              <div id=""video-container"">
                                <video controls>
                                  <source src=""" + request.Url + $@""" type=""{ContentType}"">
                                </video>
                              </div>
                            </body>
                            </html>";

                        // Set the response headers for the HTML content
                        response.SetContentType("text/html");
                        response.SetHeader("Content-Encoding", "UTF8");

                        // Write the HTML content to the response
                        buffer = Encoding.UTF8.GetBytes(html);

                        response.SetBody(buffer);
                    }
                    else
                        response.SetBody(buffer);
                }
                else if (compress)
                {
                    buffer = HTTPService.Extensions.Compress(buffer);
                    response.SetHeader("Content-Encoding", "gzip");
                    response.SetBody(buffer);
                }
                else
                    response.SetBody(buffer);
            }
            else
            {
                ServerConfiguration.LogWarn($"[HTTPS] : {HTTPSClass.GetHeaderValue(Headers, "User-Agent")} Requested a non-existing file: '{filePath}'.");
                if (HTTPSClass.GetHeaderValue(Headers, "User-Agent").Contains("PSHome"))
                    response.SetBegin(404);
                else if (request.Url != null)
                {
                    response.SetBegin(404);
                    Extensions.ReturnNotFoundError(response, request.Url);
                }
                else
                {
                    response.SetBegin(404);
                    Extensions.ReturnNotFoundError(response, "Invalid Link");
                }
            }
        }
    }
}
