// Copyright (C) 2016 by Barend Erasmus, David Jeske and donated to the public domain
using CyberBackendLibrary.DataTypes;
using CyberBackendLibrary.FileSystem;
using CyberBackendLibrary.HTTP;
using HTTPServer.Extensions;
using HTTPServer.Models;
using System.IO;
using System.Net;
using System.Text;

namespace HTTPServer.RouteHandlers
{
    public class FileSystemRouteHandler
    {
        public static HttpResponse Handle(HttpRequest request, string absolutepath, string Host, string filepath, string Accept, string ServerIP, ushort ListenerPort, string httpdirectoryrequest, string clientip, string? clientport)
        {
            if (Directory.Exists(filepath) && filepath.EndsWith("/"))
                return Handle_LocalDir(request, filepath, httpdirectoryrequest, clientip, clientport);
            else if (File.Exists(filepath))
                return Handle_LocalFile(request, filepath);
            else
                return HttpBuilder.NotFound(request, absolutepath, Host, ServerIP, ListenerPort.ToString(), !string.IsNullOrEmpty(Accept) && Accept.Contains("html"));
        }

        public static HttpResponse HandleHEAD(HttpRequest request, string absolutepath, string Host, string local_path, string Accept, string ServerIP, ushort ListenerPort)
        {
            if (File.Exists(local_path))
            {
                HttpResponse response = new()
                {
                    HttpStatusCode = HttpStatusCode.OK
                };
                string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(local_path), HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
                if (ContentType == "application/octet-stream")
                {
                    bool matched = false;
                    byte[] VerificationChunck = DataTypesUtils.ReadSmallFileChunck(local_path, 10);
                    foreach (var entry in HTTPProcessor._PathernDictionary)
                    {
                        if (DataTypesUtils.FindBytePattern(VerificationChunck, entry.Value) != -1)
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

                response.Headers.Add("Content-Length", new FileInfo(local_path).Length.ToString());

                return response;
            }
            else
                return HttpBuilder.NotFound(request, absolutepath, Host, ServerIP, ListenerPort.ToString(), !string.IsNullOrEmpty(Accept) && Accept.Contains("html"));
        }

        private static HttpResponse Handle_LocalFile(HttpRequest request, string local_path)
        {
            HttpResponse? response = null;
            string? encoding = request.RetrieveHeaderValue("Accept-Encoding");

            string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(local_path), HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes);
            if (ContentType == "application/octet-stream")
            {
                byte[] VerificationChunck = DataTypesUtils.ReadSmallFileChunck(local_path, 10);
                foreach (var entry in HTTPProcessor._PathernDictionary)
                {
                    if (DataTypesUtils.FindBytePattern(VerificationChunck, entry.Value) != -1)
                    {
                        ContentType = entry.Key;
                        break;
                    }
                }
            }

            if (request.RetrieveHeaderValue("User-Agent").Contains("PSHome") && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                response = new("1.0") // Home has a game bug where media files do not play well in screens/jukboxes with http 1.1.
                {
                    HttpStatusCode = HttpStatusCode.OK
                };
            else
                response = new()
                {
                    HttpStatusCode = HttpStatusCode.OK
                };
            response.Headers.Add("Accept-Ranges", "bytes");
            response.Headers.Add("Content-Type", ContentType);

            long FileLength = new FileInfo(local_path).Length;

            if (ContentType.StartsWith("image/") && HTTPServerConfiguration.EnableImageUpscale && FileLength <= 2147483648) // 2gb limit.
            {
                Ionic.Crc.CRC32 crc = new();
                byte[] PathIdent = Encoding.UTF8.GetBytes(local_path + "As1L8ttt?????");

                crc.SlurpBlock(PathIdent, 0, PathIdent.Length);

                byte[]? UpscalledOrOriginalData = ImageUpscaler.UpscaleImage(local_path, $"{crc.Crc32Result:X4}")?.Result;

                if (UpscalledOrOriginalData != null)
                    response.ContentStream = new MemoryStream(UpscalledOrOriginalData);
                else
                {
                    response.Dispose();

                    return new HttpResponse()
                    {
                        HttpStatusCode = HttpStatusCode.InternalServerError
                    };
                }
            }
            else
            {
                if (HTTPServerConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(encoding) && ContentType.StartsWith("text/") && FileLength <= 10 * 1024 * 1024)
                {
                    if (encoding.Contains("zstd"))
                    {
                        response.Headers.Add("Content-Encoding", "zstd");
                        response.ContentStream = HTTPProcessor.ZstdCompressStream(File.OpenRead(local_path), FileLength > 8000000);
                    }
                    else if (encoding.Contains("br"))
                    {
                        response.Headers.Add("Content-Encoding", "br");
                        response.ContentStream = HTTPProcessor.BrotliCompressStream(File.OpenRead(local_path), FileLength > 8000000);
                    }
                    else if (encoding.Contains("gzip"))
                    {
                        response.Headers.Add("Content-Encoding", "gzip");
                        response.ContentStream = HTTPProcessor.GzipCompressStream(File.OpenRead(local_path), FileLength > 8000000);
                    }
                    else if (encoding.Contains("deflate"))
                    {
                        response.Headers.Add("Content-Encoding", "deflate");
                        response.ContentStream = HTTPProcessor.InflateStream(File.OpenRead(local_path), FileLength > 8000000);
                    }
                    else
                        response.ContentStream = File.OpenRead(local_path);
                }
                else
                    response.ContentStream = File.OpenRead(local_path);
            }

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

        public static HttpResponse Handle_ByteSubmit_Download(HttpRequest request, byte[]? Data, string FileName)
        {
            HttpResponse response = new();

            if (Data != null)
            {
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Headers["Content-disposition"] = $"attachment; filename={FileName}";
                response.ContentStream = new MemoryStream(Data);
            }
            else
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

            return response;
        }

        private static HttpResponse Handle_LocalDir(HttpRequest request, string local_path, string httpdirectoryrequest, string clientip, string? clientport)
        {
            string? queryparam = null;
            string? encoding = request.RetrieveHeaderValue("Accept-Encoding");

            if (request.QueryParameters != null && request.QueryParameters.TryGetValue("directory", out queryparam) && queryparam == "on")
            {
                if (HTTPServerConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(encoding))
                {
                    if (encoding.Contains("zstd"))
                        return HttpResponse.Send(HTTPProcessor.CompressZstd(
                                Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes))), "application/json", new string[][] { new string[] { "Content-Encoding", "zstd" } });
                    else if (encoding.Contains("br"))
                        return HttpResponse.Send(HTTPProcessor.CompressBrotli(
                                Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes))), "application/json", new string[][] { new string[] { "Content-Encoding", "br" } });
                    else if (encoding.Contains("gzip"))
                        return HttpResponse.Send(HTTPProcessor.CompressGzip(
                                Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes))), "application/json", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                    else if (encoding.Contains("deflate"))
                        return HttpResponse.Send(HTTPProcessor.Inflate(
                                Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes))), "application/json", new string[][] { new string[] { "Content-Encoding", "deflate" } });
                    else
                        return HttpResponse.Send(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes), "application/json");
                }
                else
                    return HttpResponse.Send(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest, HTTPServerConfiguration.MimeTypes ?? HTTPProcessor._mimeTypes), "application/json");
            }
            else if (request.QueryParameters != null && request.QueryParameters.TryGetValue("m3u", out queryparam) && queryparam == "on")
            {
                string? m3ufile = StaticFileSystem.GetM3UStreamFromDirectory(local_path[..^1], httpdirectoryrequest);
                if (!string.IsNullOrEmpty(m3ufile))
                {
                    if (HTTPServerConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(encoding))
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
                    if (File.Exists(local_path + $"/{indexFile}"))
                    {
                        if (indexFile.Contains(".php") && Directory.Exists(HTTPServerConfiguration.PHPStaticFolder))
                        {
                            (byte[]?, string[][]) CollectPHP = PHP.ProcessPHPPage(local_path + $"/{indexFile}", HTTPServerConfiguration.PHPStaticFolder, HTTPServerConfiguration.PHPVersion, clientip, clientport, request);
                            if (HTTPServerConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(encoding) && CollectPHP.Item1 != null)
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
                            if (HTTPServerConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(encoding))
                            {
                                if (encoding.Contains("zstd"))
                                    return HttpResponse.Send(HTTPProcessor.CompressZstd(File.ReadAllBytes(local_path + $"/{indexFile}")), "text/html", new string[][] { new string[] { "Content-Encoding", "zstd" } });
                                else if (encoding.Contains("br"))
                                    return HttpResponse.Send(HTTPProcessor.CompressBrotli(File.ReadAllBytes(local_path + $"/{indexFile}")), "text/html", new string[][] { new string[] { "Content-Encoding", "br" } });
                                else if (encoding.Contains("gzip"))
                                    return HttpResponse.Send(HTTPProcessor.CompressGzip(File.ReadAllBytes(local_path + $"/{indexFile}")), "text/html", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                                else if (encoding.Contains("deflate"))
                                    return HttpResponse.Send(HTTPProcessor.Inflate(File.ReadAllBytes(local_path + $"/{indexFile}")), "text/html", new string[][] { new string[] { "Content-Encoding", "deflate" } });
                                else
                                    return HttpResponse.Send(File.OpenRead(local_path + $"/{indexFile}"), "text/html");
                            }
                            else
                                return HttpResponse.Send(File.OpenRead(local_path + $"/{indexFile}"), "text/html");
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
