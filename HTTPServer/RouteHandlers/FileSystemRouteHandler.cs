// Copyright (C) 2016 by Barend Erasmus, David Jeske and donated to the public domain
using HTTPServer.Models;
using System.Text;

namespace HTTPServer.RouteHandlers
{
    public class FileSystemRouteHandler
    {
        public static HttpResponse Handle(HttpRequest request, string filepath)
        {
            if (Directory.Exists(filepath) && filepath.EndsWith("/"))
                return Handle_LocalDir(request, filepath);
            else if (File.Exists(filepath) && request.Headers.Keys.Count(x => x == "Range") == 1) // We not support multiple ranges.
                return Handle_LocalFile_Stream(request, filepath);
            else if (File.Exists(filepath))
                return Handle_LocalFile(filepath);
            else
                return HttpBuilder.NotFound();
        }

        public static HttpResponse HandleHEAD(string local_path)
        {
            if (File.Exists(local_path))
            {
                var response = new HttpResponse(false)
                {
                    HttpStatusCode = HttpStatusCode.Ok
                };
                response.Headers["Content-Type"] = CryptoSporidium.HTTPUtils.GetMimeType(Path.GetExtension(local_path));

                return response;
            }
            else
                return HttpBuilder.NotFound();
        }

        public static HttpResponse Handle_LocalFile_Download(string local_path)
        {
            var response = new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.Ok
            };
            response.Headers["Content-disposition"] = $"attachment; filename={Path.GetFileName(local_path)}";
            response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return response;
        }

        public static HttpResponse Handle_ByteSubmit_Download(byte[]? Data, string FileName)
        {
            var response = new HttpResponse(false);
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

        private static HttpResponse Handle_LocalFile(string local_path)
        {
            var response = new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.Ok
            };
            response.Headers["Content-Type"] = CryptoSporidium.HTTPUtils.GetMimeType(Path.GetExtension(local_path));
            response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return response;
        }

        private static HttpResponse Handle_LocalFile_Stream(HttpRequest request, string local_path)
        {
            using (FileStream fs = new(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) // Todo, test on safari browser.
            {
                long filesize = fs.Length;
                long startByte = -1;
                long endByte = -1;
                if (request.Headers.ContainsKey("Range"))
                {
                    string rangeHeader = request.GetHeaderValue("Range").Replace("bytes=", "");
                    string[] range = rangeHeader.Split('-');
                    startByte = long.Parse(range[0]);
                    if (range[1].Trim().Length > 0) _ = long.TryParse(range[1], out endByte);
                    if (endByte == -1) endByte = filesize;
                }
                else
                {
                    startByte = 0;
                    endByte = filesize;
                }
                if (startByte < 0 || endByte > filesize)
                {
                    return new HttpResponse(false)
                    {
                        HttpStatusCode = HttpStatusCode.RangeNotSatisfiable
                    };
                }
                else
                {
                    var response = new HttpResponse(true)
                    {
                        HttpStatusCode = HttpStatusCode.PartialContent
                    };
                    long TotalBytes = endByte - startByte;
                    // Check if endByte - startByte exceeds ConfigurationDefaults.BufferSize * 12.5
                    // This is an AWESOME WORKAROUND for larger than 2gb files.
                    if (TotalBytes > HTTPServerConfiguration.BufferSize * 12.5)
                        TotalBytes = (long)(HTTPServerConfiguration.BufferSize * 12.5);
                    Span<byte> buffer = new byte[TotalBytes];
                    fs.Position = startByte;
                    TotalBytes = fs.Read(buffer);
                    fs.Flush();
                    fs.Close();
                    response.Headers.Add("Content-Type", CryptoSporidium.HTTPUtils.GetMimeType(Path.GetExtension(local_path)));
                    response.Headers.Add("Accept-Ranges", "bytes");
                    TotalBytes = startByte + buffer.Length; // We optimize the spare integer to store total bytes.
                    response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", startByte, TotalBytes - 1, filesize));
                    response.Headers.Add("Content-Length", buffer.Length.ToString());
                    response.ContentStream = new MemoryStream(buffer.ToArray());
                    return response;
                }
            }
        }

        private static HttpResponse Handle_LocalDir(HttpRequest request, string local_path)
        {
            string? encoding = request.GetHeaderValue("Accept-Encoding");

            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
            {
                byte[]? CompresedFileBytes = CryptoSporidium.HTTPUtils.Compress(
                    Encoding.UTF8.GetBytes(CryptoSporidium.FileStructureToJson.GetFileStructureAsJson(local_path.Substring(0, local_path.Length - 1))));
                if (CompresedFileBytes != null)
                    return HttpResponse.Send(CompresedFileBytes, "application/json", new string[][] { new string[] { "Content-Encoding", "gzip" } });
                else
                    return HttpBuilder.InternalServerError();
            }
            else
                return HttpResponse.Send(CryptoSporidium.FileStructureToJson.GetFileStructureAsJson(local_path.Substring(0, local_path.Length - 1)), "application/json");
        }
    }
}
