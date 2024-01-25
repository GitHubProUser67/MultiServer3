// Copyright (C) 2016 by Barend Erasmus, David Jeske and donated to the public domain
using BackendProject.MiscUtils;
using HTTPServer.Models;
using System.Text;

namespace HTTPServer.RouteHandlers
{
    public class FileSystemRouteHandler
    {
        public static HttpResponse Handle(HttpRequest request, string filepath, string httpdirectoryrequest)
        {
            if (Directory.Exists(filepath) && filepath.EndsWith("/"))
                return Handle_LocalDir(request, filepath, httpdirectoryrequest);
            else if (File.Exists(filepath))
                return Handle_LocalFile(filepath);
            else
                return HttpBuilder.NotFound();
        }

        public static HttpResponse HandleHEAD(string local_path)
        {
            if (File.Exists(local_path))
            {
                HttpResponse response = new(false)
                {
                    HttpStatusCode = HttpStatusCode.Ok
                };
                string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
                if (ContentType == "application/octet-stream")
                {
                    bool matched = false;
                    byte[] VerificationChunck = VariousUtils.ReadSmallFileChunck(local_path, 10);
                    foreach (var entry in HTTPUtils.PathernDictionary)
                    {
                        if (VariousUtils.FindbyteSequence(VerificationChunck, entry.Value))
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

        private static HttpResponse Handle_LocalFile(string local_path)
        {
            HttpResponse response = new(false)
            {
                HttpStatusCode = HttpStatusCode.Ok
            };
            string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
            if (ContentType == "application/octet-stream")
            {
                bool matched = false;
                byte[] VerificationChunck = VariousUtils.ReadSmallFileChunck(local_path, 10);
                foreach (var entry in HTTPUtils.PathernDictionary)
                {
                    if (VariousUtils.FindbyteSequence(VerificationChunck, entry.Value))
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

            response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return response;
        }

        public static HttpResponse Handle_LocalFile_Download(string local_path)
        {
            HttpResponse response = new(false)
            {
                HttpStatusCode = HttpStatusCode.Ok,
            };
            response.Headers["Content-disposition"] = $"attachment; filename={Path.GetFileName(local_path)}";
            response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return response;
        }

        public static HttpResponse Handle_ByteSubmit_Download(byte[]? Data, string FileName)
        {
            HttpResponse response = new(false);
            if (Data != null)
            {
                response.HttpStatusCode = HttpStatusCode.Ok;
                response.Headers["Content-disposition"] = $"attachment; filename={FileName}";
                response.ContentStream = new MemoryStream(Data);
            }
            else
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

            return response;
        }

        private static HttpResponse Handle_LocalDir(HttpRequest request, string local_path, string httpdirectoryrequest)
        {
            string? encoding = request.GetHeaderValue("Accept-Encoding");

            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                return HttpResponse.Send(HTTPUtils.Compress(
                        Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest))), "application/json", new string[][] { new string[] { "Content-Encoding", "gzip" } });
            else
                return HttpResponse.Send(FileStructureToJson.GetFileStructureAsJson(local_path[..^1], httpdirectoryrequest), "application/json");
        }
    }
}
