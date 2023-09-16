using System.Net;
using System.Text;

namespace MultiServer.HTTPService
{
    public class RootProcess
    {
        public static async Task ProcessRootRequest(string filePath, string directoryPath, string crc32, HttpListenerRequest request, HttpListenerResponse response)
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

                        ServerConfiguration.LogInfo($"[HTTP] - {request.UserAgent} Redirected to {infcontent}");

                        response.StatusCode = (int)HttpStatusCode.Redirect; // You can use other redirect codes like 301, 302, etc.
                        response.Headers.Add("Location", infcontent);
                        return;
                    }
                }

                bool compress = false;

                byte[] buffer = null;

#pragma warning disable // Sometimes Visual Studio is weird.
                if (request.Headers["Accept-Encoding"] != null && request.Headers["Accept-Encoding"].Contains("gzip"))
                    compress = true;
#pragma warning restore

                response.ContentType = MimeTypes.GetMimeType(filePath);

                if (response.ContentType.StartsWith("image/") && ServerConfiguration.EnableUpscale) // Fix-Me Linux probably has a binary for ImageMagick.
                    buffer = await Extensions.UpscaleImage(filePath, crc32);
                else
                    buffer = FileHelper.CryptoReadAsync(filePath, HTTPPrivateKey.HTTPPrivatekey);

                if (buffer == null)
                {
                    response.StatusCode = 500;
                    return;
                }
                else
                    response.StatusCode = 200;

                if (response.ContentType.StartsWith("audio/") || response.ContentType.StartsWith("video/"))
                {
#pragma warning disable // Sometimes Visual Studio is weird.
                    if (request.Headers["Accept"] != null && request.Headers["Accept"].Contains("text/html"))
                    {
#pragma warning restore
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
                                  <source src=""" + request.Url.AbsolutePath + $@""" type=""{response.ContentType}"">
                                </video>
                              </div>
                            </body>
                            </html>";

                        // Set the response headers for the HTML content
                        response.ContentType = "text/html";
                        response.ContentEncoding = Encoding.UTF8;

                        // Write the HTML content to the response
                        buffer = Encoding.UTF8.GetBytes(html);

                        response.ContentLength64 = buffer.Length;

                        if (response.OutputStream.CanWrite)
                        {
                            try
                            {
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                response.OutputStream.Close();
                            }
                            catch (Exception)
                            {
                                // Not Important.
                            }
                        }
                    }
                    else
                    {
                        response.ContentLength64 = buffer.Length;

                        if (response.OutputStream.CanWrite)
                        {
                            try
                            {
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                response.OutputStream.Close();
                            }
                            catch (Exception)
                            {
                                // Not Important.
                            }
                        }
                    }
                }
                else if (compress)
                {
                    buffer = Extensions.Compress(buffer);

                    response.Headers.Set("Content-Encoding", "gzip");

                    response.ContentLength64 = buffer.Length;

                    if (response.OutputStream.CanWrite)
                    {
                        try
                        {
                            response.OutputStream.Write(buffer, 0, buffer.Length);
                            response.OutputStream.Close();
                        }
                        catch (Exception)
                        {
                            // Not Important.
                        }
                    }
                }
                else
                {
                    response.ContentLength64 = buffer.Length;

                    if (response.OutputStream.CanWrite)
                    {
                        try
                        {
                            response.OutputStream.Write(buffer, 0, buffer.Length);
                            response.OutputStream.Close();
                        }
                        catch (Exception)
                        {
                            // Not Important.
                        }
                    }
                }
            }
            else
            {
                ServerConfiguration.LogWarn($"[HTTP] : {request.UserAgent} Requested a non-existing file: '{filePath}'.");
                if (request.UserAgent.Contains("PSHome"))
                    response.StatusCode = 404;
                else
                {
                    response.StatusCode = 404;
                    await Extensions.ReturnNotFoundError(response, $"{request.Url.Scheme}://{request.Url.Authority}{request.RawUrl}");
                }
            }
        }
    }
}
