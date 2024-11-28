// Copyright (C) 2016 by Barend Erasmus, David Jeske and donated to the public domain
using NetworkLibrary.Extension;
using NetworkLibrary.FileSystem;
using NetworkLibrary.HTTP;
using HTTPServer.Extensions;
using HTTPServer.Models;
using System.IO;
using System.Net;
using System.Text;
using WebAPIService.WebArchive;

namespace HTTPServer.RouteHandlers
{
    public class FileSystemRouteHandler
    {
        public static (bool, HttpResponse) Handle(HttpRequest request, string absolutepath, string fullurl, string filePath, string Host, string Accept, 
            string directoryUrl , bool GET, bool noCompressCacheControl)
        {
            if (Directory.Exists(filePath) && filePath.EndsWith("/"))
                return (false, Handle_LocalDir(request, filePath, directoryUrl, noCompressCacheControl));
            else if (File.Exists(filePath))
                return (true, Handle_LocalFile(request, filePath, noCompressCacheControl));
            else if (!string.IsNullOrEmpty(Accept) && Accept.Contains("text/html") && Directory.Exists(filePath + "/"))
                return (false, Handle_ApachePermanentRedirect(request, absolutepath, filePath, Host, noCompressCacheControl));

            if (GET && HTTPServerConfiguration.NotFoundWebArchive && !string.IsNullOrEmpty(Host) && !Host.Equals("web.archive.org") && !Host.Equals("archive.org"))
            {
                WebArchiveRequest archiveReq = new($"http://{Host}" + fullurl);
                if (archiveReq.Archived)
                    return (false, HttpBuilder.PermanantRedirect(archiveReq.ArchivedURL));
            }

            return (false, HttpBuilder.NotFound(request, absolutepath, Host, !string.IsNullOrEmpty(Accept) && Accept.Contains("html")));
        }

        public static HttpResponse HandleHEAD(HttpRequest request, string absolutepath, string filePath, string Host, string Accept)
        {
            if (File.Exists(filePath))
            {
                HttpResponse response = new()
                {
                    HttpStatusCode = HttpStatusCode.OK,
                    ContentAsUTF8 = string.Empty
                };
                string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(filePath), HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                if (ContentType == "application/octet-stream")
                {
                    bool matched = false;
                    byte[] VerificationChunck = OtherExtensions.ReadSmallFileChunck(filePath, 10);
                    foreach (var entry in HTTPProcessor._PathernDictionary)
                    {
                        if (OtherExtensions.FindBytePattern(VerificationChunck, entry.Value) != -1)
                        {
                            matched = true;
                            response.Headers["Content-Type"] = entry.Key;
                            break;
                        }
                    }
                    if (!matched)
                        response.Headers["Content-Type"] = ContentType;
                }
                else
                    response.Headers["Content-Type"] = ContentType;

                response.Headers.Add("Content-Length", new FileInfo(filePath).Length.ToString());

                return response;
            }
            else
                return HttpBuilder.NotFound(request, absolutepath, Host, !string.IsNullOrEmpty(Accept) && Accept.Contains("html"));
        }

        private static HttpResponse Handle_LocalFile(HttpRequest request, string filePath, bool noCompressCacheControl)
        {
            HttpResponse? response = new()
                {
                    HttpStatusCode = HttpStatusCode.OK
                };
				
            string? encoding = request.RetrieveHeaderValue("Accept-Encoding");

            string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(filePath), HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
            if (ContentType == "application/octet-stream")
            {
                byte[] VerificationChunck = OtherExtensions.ReadSmallFileChunck(filePath, 10);
                foreach (var entry in HTTPProcessor._PathernDictionary)
                {
                    if (OtherExtensions.FindBytePattern(VerificationChunck, entry.Value) != -1)
                    {
                        ContentType = entry.Key;
                        break;
                    }
                }
            }

            response.Headers.Add("Accept-Ranges", "bytes");
            response.Headers.Add("Content-Type", ContentType);

            if (ContentType.StartsWith("image/") && HTTPServerConfiguration.EnableImageUpscale)
            {
                Ionic.Crc.CRC32 crc = new();
                byte[] PathIdent = Encoding.UTF8.GetBytes(filePath);

                crc.SlurpBlock(PathIdent, 0, PathIdent.Length);

                response.ContentStream = new MemoryStream(ImageOptimizer.OptimizeImage(filePath, crc.Crc32Result));
            }
            else if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(encoding) && ContentType.StartsWith("text/"))
            {
                if (encoding.Contains("zstd"))
                {
                    response.Headers.Add("Content-Encoding", "zstd");
                    response.ContentStream = HTTPProcessor.ZstdCompressStream(File.OpenRead(filePath));
                }
                else if (encoding.Contains("br"))
                {
                    response.Headers.Add("Content-Encoding", "br");
                    response.ContentStream = HTTPProcessor.BrotliCompressStream(File.OpenRead(filePath));
                }
                else if (encoding.Contains("gzip"))
                {
                    response.Headers.Add("Content-Encoding", "gzip");
                    response.ContentStream = HTTPProcessor.GzipCompressStream(File.OpenRead(filePath));
                }
                else if (encoding.Contains("deflate"))
                {
                    response.Headers.Add("Content-Encoding", "deflate");
                    response.ContentStream = HTTPProcessor.InflateStream(File.OpenRead(filePath));
                }
                else
                    response.ContentStream = File.OpenRead(filePath);
            }
            else
                response.ContentStream = File.OpenRead(filePath);

            return response;
        }

        private static HttpResponse Handle_ApachePermanentRedirect(HttpRequest request, string absolutepath, string filePath, string Host, bool noCompressCacheControl)
        {
            HttpResponse? response = new()
            {
                HttpStatusCode = HttpStatusCode.MovedPermanently
            };

            MemoryStream MovedPayloadStream = new MemoryStream(Encoding.UTF8.GetBytes($@"
            <!DOCTYPE HTML PUBLIC ""-//IETF//DTD HTML 2.0//EN"">
            <html><head>
            <title>301 Moved Permanently</title>
            </head><body>
            <h1>Moved Permanently</h1>
            <p>The document has moved <a href=""http://{(!string.IsNullOrEmpty(Host) ? Host : request.ServerIP)}{absolutepath}/"">here</a>.</p>
            <hr>
            <address>{request.ServerIP} Port {request.ServerPort}</address>
            </body></html>"));

            string? encoding = request.RetrieveHeaderValue("Accept-Encoding");

            response.Headers.Add("Location", $"http://{(!string.IsNullOrEmpty(Host) ? Host : request.ServerIP)}{absolutepath}/");
            response.Headers.Add("Content-Type", "text/html; charset=iso-8859-1");

            if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(encoding))
            {
                if (encoding.Contains("zstd"))
                {
                    response.Headers.Add("Content-Encoding", "zstd");
                    response.ContentStream = HTTPProcessor.ZstdCompressStream(MovedPayloadStream);
                }
                else if (encoding.Contains("br"))
                {
                    response.Headers.Add("Content-Encoding", "br");
                    response.ContentStream = HTTPProcessor.BrotliCompressStream(MovedPayloadStream);
                }
                else if (encoding.Contains("gzip"))
                {
                    response.Headers.Add("Content-Encoding", "gzip");
                    response.ContentStream = HTTPProcessor.GzipCompressStream(MovedPayloadStream);
                }
                else if (encoding.Contains("deflate"))
                {
                    response.Headers.Add("Content-Encoding", "deflate");
                    response.ContentStream = HTTPProcessor.InflateStream(MovedPayloadStream);
                }
                else
                    response.ContentStream = MovedPayloadStream;
            }
            else
                response.ContentStream = MovedPayloadStream;

            return response;
        }

        public static HttpResponse Handle_LocalFile_Download(HttpRequest request, string local_path)
        {
            HttpResponse response = new()
            {
                HttpStatusCode = HttpStatusCode.OK,
            };
            response.Headers["Content-disposition"] = $"attachment; filename={Path.GetFileName(local_path)}";
            response.ContentStream = File.OpenRead(local_path);

            return response;
        }

        public static HttpResponse Handle_ByteSubmit_Download(HttpRequest request, byte[]? Data, string fileName)
        {
            HttpResponse response = new();

            if (Data != null)
            {
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Headers["Content-disposition"] = $"attachment; filename={fileName}";
                response.ContentStream = new MemoryStream(Data);
            }
            else
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

            return response;
        }

        private static HttpResponse Handle_LocalDir(HttpRequest request, string filePath, string directoryUrl, bool noCompressCacheControl)
        {
            string? encoding = request.RetrieveHeaderValue("Accept-Encoding");

            if (request.QueryParameters != null && request.QueryParameters.TryGetValue("directory", out string? queryparam) && queryparam == "on")
            {
                if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(encoding))
                {
                    if (encoding.Contains("zstd"))
                        return HttpResponse.Send(HTTPProcessor.CompressZstd(
                                Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(filePath[..^1], directoryUrl, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes))), "application/json", new string[][] { new string[] { "Content-Encoding", "zstd" } });
                    else if (encoding.Contains("br"))
                        return HttpResponse.Send(HTTPProcessor.CompressBrotli(
                                Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(filePath[..^1], directoryUrl, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes))), "application/json", new string[][] { new string[] { "Content-Encoding", "br" } });
                    else if (encoding.Contains("gzip"))
                        return HttpResponse.Send(HTTPProcessor.CompressGzip(
                                Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(filePath[..^1], directoryUrl, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes))), "application/json", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                    else if (encoding.Contains("deflate"))
                        return HttpResponse.Send(HTTPProcessor.Inflate(
                                Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(filePath[..^1], directoryUrl, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes))), "application/json", new string[][] { new string[] { "Content-Encoding", "deflate" } });
                    else
                        return HttpResponse.Send(FileStructureToJson.GetFileStructureAsJson(filePath[..^1], directoryUrl, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes), "application/json");
                }
                else
                    return HttpResponse.Send(FileStructureToJson.GetFileStructureAsJson(filePath[..^1], directoryUrl, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes), "application/json");
            }
            else if (request.QueryParameters != null && request.QueryParameters.TryGetValue("m3u", out queryparam) && queryparam == "on")
            {
                string? m3ufile = StaticFileSystem.GetM3UStreamFromDirectory(filePath[..^1], directoryUrl);
                if (!string.IsNullOrEmpty(m3ufile))
                {
                    if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(encoding))
                    {
                        if (encoding.Contains("zstd"))
                            return HttpResponse.Send(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(m3ufile)), "audio/x-mpegurl", new string[][] { new string[] { "Content-Encoding", "zstd" } });
                        else if (encoding.Contains("br"))
                            return HttpResponse.Send(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(m3ufile)), "audio/x-mpegurl", new string[][] { new string[] { "Content-Encoding", "br" } });
                        else if (encoding.Contains("gzip"))
                            return HttpResponse.Send(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(m3ufile)), "audio/x-mpegurl", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                        else if (encoding.Contains("deflate"))
                            return HttpResponse.Send(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(m3ufile)), "audio/x-mpegurl", new string[][] { new string[] { "Content-Encoding", "deflate" } });
                        else
                            return HttpResponse.Send(m3ufile, "audio/x-mpegurl");
                    }
                    else
                        return HttpResponse.Send(m3ufile, "audio/x-mpegurl");
                }
                else
                    return HttpBuilder.NoContent();
            }
            else
            {
                foreach (string indexFile in HTTPProcessor._DefaultFiles)
                {
                    if (File.Exists(filePath + $"/{indexFile}"))
                    {
                        if (indexFile.Contains(".php") && Directory.Exists(HTTPServerConfiguration.PHPStaticFolder))
                        {
                            (byte[]?, string[][]) CollectPHP = PHP.ProcessPHPPage(filePath + $"/{indexFile}", HTTPServerConfiguration.PHPStaticFolder, HTTPServerConfiguration.PHPVersion, request);
                            if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(encoding) && CollectPHP.Item1 != null)
                            {
                                if (encoding.Contains("zstd"))
                                    return HttpResponse.Send(HTTPProcessor.CompressZstd(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "zstd" }));
                                else if (encoding.Contains("br"))
                                    return HttpResponse.Send(HTTPProcessor.CompressBrotli(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "br" }));
                                else if (encoding.Contains("gzip"))
                                    return HttpResponse.Send(HTTPProcessor.CompressGzip(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "gzip" }));
                                else if (encoding.Contains("deflate"))
                                    return HttpResponse.Send(HTTPProcessor.Inflate(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "deflate" }));
                                else
                                    return HttpResponse.Send(CollectPHP.Item1, "text/html", CollectPHP.Item2);
                            }
                            else
                                return HttpResponse.Send(CollectPHP.Item1, "text/html", CollectPHP.Item2);
                        }
                        else
                        {
                            if (HTTPServerConfiguration.EnableHTTPCompression && !noCompressCacheControl && !string.IsNullOrEmpty(encoding))
                            {
                                if (encoding.Contains("zstd"))
                                    return HttpResponse.Send(HTTPProcessor.CompressZstd(File.ReadAllBytes(filePath + $"/{indexFile}")), "text/html", new string[][] { new string[] { "Content-Encoding", "zstd" } });
                                else if (encoding.Contains("br"))
                                    return HttpResponse.Send(HTTPProcessor.CompressBrotli(File.ReadAllBytes(filePath + $"/{indexFile}")), "text/html", new string[][] { new string[] { "Content-Encoding", "br" } });
                                else if (encoding.Contains("gzip"))
                                    return HttpResponse.Send(HTTPProcessor.CompressGzip(File.ReadAllBytes(filePath + $"/{indexFile}")), "text/html", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                                else if (encoding.Contains("deflate"))
                                    return HttpResponse.Send(HTTPProcessor.Inflate(File.ReadAllBytes(filePath + $"/{indexFile}")), "text/html", new string[][] { new string[] { "Content-Encoding", "deflate" } });
                                else
                                    return HttpResponse.Send(File.OpenRead(filePath + $"/{indexFile}"), "text/html");
                            }
                            else
                                return HttpResponse.Send(File.OpenRead(filePath + $"/{indexFile}"), "text/html");
                        }
                    }
                }

                return new HttpResponse()
                {
                    HttpStatusCode = HttpStatusCode.NotFound,
                    ContentAsUTF8 = string.Empty
                };
            }
        }
    }
}
