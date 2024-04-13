// Copyright (C) 2016 by Barend Erasmus, David Jeske and donated to the public domain
using CyberBackendLibrary.DataTypes;
using CyberBackendLibrary.HTTP;

using HTTPServer.Extensions;
using HTTPServer.Models;
using System.Text;

namespace HTTPServer.RouteHandlers
{
    public class FileSystemRouteHandler
    {
        public static HttpResponse Handle(HttpRequest request, string filepath, string httpdirectoryrequest, string clientip, string? clientport)
        {
            if (Directory.Exists(filepath) && filepath.EndsWith("/"))
                return Handle_LocalDir(request, filepath, httpdirectoryrequest, clientip, clientport);
            else if (File.Exists(filepath))
                return Handle_LocalFile(request, filepath);
            else
                return HttpBuilder.NotFound();
        }

        public static HttpResponse HandleHEAD(HttpRequest request, string local_path)
        {
            if (File.Exists(local_path))
            {
                HttpResponse response = new(request.RetrieveHeaderValue("Connection") == "keep-alive")
                {
                    HttpStatusCode = HttpStatusCode.OK
                };
                string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(local_path));
                if (ContentType == "application/octet-stream")
                {
                    bool matched = false;
                    byte[] VerificationChunck = DataTypesUtils.ReadSmallFileChunck(local_path, 10);
                    foreach (var entry in HTTPProcessor.PathernDictionary)
                    {
                        if (DataTypesUtils.FindbyteSequence(VerificationChunck, entry.Value))
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
                return HttpBuilder.NotFound();
        }

        private static HttpResponse Handle_LocalFile(HttpRequest request, string local_path)
        {
            HttpResponse? response = null;
            string? encoding = request.RetrieveHeaderValue("Accept-Encoding");

            string ContentType = HTTPProcessor.GetMimeType(Path.GetExtension(local_path));
            if (ContentType == "application/octet-stream")
            {
                byte[] VerificationChunck = DataTypesUtils.ReadSmallFileChunck(local_path, 10);
                foreach (var entry in HTTPProcessor.PathernDictionary)
                {
                    if (DataTypesUtils.FindbyteSequence(VerificationChunck, entry.Value))
                    {
                        ContentType = entry.Key;
                        break;
                    }
                }
            }

            if (request.RetrieveHeaderValue("User-Agent").Contains("PSHome") && (ContentType == "video/mp4" || ContentType == "video/mpeg" || ContentType == "audio/mpeg"))
                response = new(request.RetrieveHeaderValue("Connection") == "keep-alive", "1.0") // Home has a game bug where media files do not play well in screens/jukboxes with http 1.1.
                {
                    HttpStatusCode = HttpStatusCode.OK
                };
            else
                response = new(request.RetrieveHeaderValue("Connection") == "keep-alive")
                {
                    HttpStatusCode = HttpStatusCode.OK
                };
            response.Headers.Add("Accept-Ranges", "bytes");
            response.Headers.Add("Content-Type", ContentType);

            long fileSize = new FileInfo(local_path).Length;

            if (ContentType.StartsWith("image/") && HTTPServerConfiguration.EnableImageUpscale && fileSize <= 2147483648) // 2gb limit.
            {
                byte[]? UpscalledOrOriginalData = ImageUpscaler.UpscaleImage(local_path, $"{new CastleLibrary.Custom.Crc32().Get(Encoding.UTF8.GetBytes(local_path + "As1L8ttt?????")):X}")?.Result;

                if (UpscalledOrOriginalData != null)
                    response.ContentStream = new MemoryStream(UpscalledOrOriginalData);
                else
                {
                    response.Dispose();

                    return new HttpResponse(request.RetrieveHeaderValue("Connection") == "keep-alive")
                    {
                        HttpStatusCode = HttpStatusCode.InternalServerError
                    };
                }
            }
            else
            {
                long FileLength = new FileInfo(local_path).Length;

                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && FileLength <= 8000000)
                {
                    response.Headers.Add("Content-Encoding", "gzip");
                    response.ContentStream = HTTPProcessor.CompressStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), FileLength > 8000000);
                }
                else if (!string.IsNullOrEmpty(encoding) && encoding.Contains("deflate") && FileLength <= 8000000)
                {
                    response.Headers.Add("Content-Encoding", "deflate");
                    response.ContentStream = HTTPProcessor.InflateStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), FileLength > 8000000);
                }
                else
                    response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            }

            return response;
        }

        public static HttpResponse Handle_LocalFile_Download(HttpRequest request, string local_path)
        {
            string? encoding = request.RetrieveHeaderValue("Accept-Encoding");

            HttpResponse response = new(request.RetrieveHeaderValue("Connection") == "keep-alive")
            {
                HttpStatusCode = HttpStatusCode.OK,
            };
            response.Headers["Content-disposition"] = $"attachment; filename={Path.GetFileName(local_path)}";

            long FileLength = new FileInfo(local_path).Length;

            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && FileLength <= 8000000)
            {
                response.Headers.Add("Content-Encoding", "gzip");
                response.ContentStream = HTTPProcessor.CompressStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), FileLength > 8000000);
            }
            else if (!string.IsNullOrEmpty(encoding) && encoding.Contains("deflate") && FileLength <= 8000000)
            {
                response.Headers.Add("Content-Encoding", "deflate");
                response.ContentStream = HTTPProcessor.InflateStream(File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), FileLength > 8000000);
            }
            else
                response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return response;
        }

        public static HttpResponse Handle_ByteSubmit_Download(HttpRequest request, byte[]? Data, string FileName)
        {
            string? encoding = request.RetrieveHeaderValue("Accept-Encoding");

            HttpResponse response = new(request.RetrieveHeaderValue("Connection") == "keep-alive");

            if (Data != null)
            {
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Headers["Content-disposition"] = $"attachment; filename={FileName}";

                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && Data.Length <= 8000000)
                {
                    response.Headers.Add("Content-Encoding", "gzip");
                    response.ContentStream = new MemoryStream(HTTPProcessor.Compress(Data));
                }
                else
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
                if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                    return HttpResponse.Send(HTTPProcessor.Compress(
                            Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest))), "application/json", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                else
                    return HttpResponse.Send(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest), "application/json");
            }
            else if (request.QueryParameters != null && request.QueryParameters.TryGetValue("m3u", out queryparam) && queryparam == "on")
            {
                string? m3ufile = StaticFileSystem.GetM3UStreamFromDirectory(local_path[..^1], httpdirectoryrequest);
                if (!string.IsNullOrEmpty(m3ufile))
                {
                    if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                        return HttpResponse.Send(HTTPProcessor.Compress(Encoding.UTF8.GetBytes(m3ufile)), "audio/x-mpegurl", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                    else
                        return HttpResponse.Send(m3ufile, "audio/x-mpegurl");
                }
                else
                    return HttpBuilder.NoContent();
            }
            else
            {
                foreach (string indexFile in HTTPProcessor.DefaultDocuments)
                {
                    if (File.Exists(local_path + indexFile))
                    {
                        if (indexFile.Contains(".php") && Directory.Exists(HTTPServerConfiguration.PHPStaticFolder))
                        {
                            (byte[]?, string[][]) CollectPHP = PHP.ProcessPHPPage(local_path + indexFile, HTTPServerConfiguration.PHPStaticFolder, HTTPServerConfiguration.PHPVersion, clientip, clientport, request);
                            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip") && CollectPHP.Item1 != null)
                                return HttpResponse.Send(HTTPProcessor.Compress(CollectPHP.Item1), "text/html", HttpMisc.AddElementToLastPosition(CollectPHP.Item2, new string[] { "Content-Encoding", "gzip" }));
                            else
                                return HttpResponse.Send(CollectPHP.Item1, "text/html", CollectPHP.Item2);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                            {
                                using FileStream stream = new(local_path + indexFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                byte[]? buffer = null;

                                using (MemoryStream ms = new())
                                {
                                    stream.CopyTo(ms);
                                    buffer = ms.ToArray();
                                    ms.Flush();
                                }

                                stream.Flush();

                                return HttpResponse.Send(HTTPProcessor.Compress(buffer), "text/html", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                            }
                            else
                                return HttpResponse.Send(File.Open(local_path + indexFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), "text/html");
                        }
                    }
                }

                return new HttpResponse(request.RetrieveHeaderValue("Connection") == "keep-alive")
                {
                    HttpStatusCode = HttpStatusCode.Not_Found,
                    ContentAsUTF8 = string.Empty
                };

            }
        }
    }
}
