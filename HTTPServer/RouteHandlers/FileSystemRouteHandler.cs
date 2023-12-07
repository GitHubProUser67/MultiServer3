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
            else if (File.Exists(filepath) && request.Headers.Keys.Count(x => x == "Range") == 1) // Range can only be sent once.
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
            response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read);

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
            response.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read);

            return response;
        }

        private static HttpResponse Handle_LocalFile_Stream(HttpRequest request, string local_path)
        {
            using (FileStream fs = new(local_path, FileMode.Open, FileAccess.Read))
            {
                long filesize = fs.Length;
                long startByte = -1;
                long endByte = -1;
                if (request.Headers.ContainsKey("Range"))
                {
                    string HeaderString = request.GetHeaderValue("Range").Replace("bytes=", string.Empty);
                    if (HeaderString.Contains(','))
                    {
                        using (MemoryStream ms = new())
                        {
                            int i = 0;
                            byte Separator = (byte)'\n';
                            byte[] MultiPart = Encoding.UTF8.GetBytes("--multiserver_separator");
                            byte[] EndingMultiPart = Encoding.UTF8.GetBytes("--multiserver_separator--");
                            byte[] MimeType = Encoding.UTF8.GetBytes($"Content-Type: {CryptoSporidium.HTTPUtils.GetMimeType(Path.GetExtension(local_path))}");
                            // Split the ranges based on the comma (',') separator
                            string[] rangeValues = HeaderString.Split(',');
                            foreach (string RangeSelect in rangeValues)
                            {
                                ms.Write(MultiPart, 0, MultiPart.Length);
                                ms.WriteByte(Separator);
                                ms.Write(MimeType, 0, MimeType.Length);
                                ms.WriteByte(Separator);
                                fs.Position = 0;
                                startByte = -1;
                                endByte = -1;
                                string[] range = RangeSelect.Split('-');
                                if (range[0].Trim().Length > 0) _ = long.TryParse(range[0], out startByte);
                                if (range[1].Trim().Length > 0) _ = long.TryParse(range[1], out endByte);
                                if (endByte == -1) endByte = filesize;
                                else if (endByte != filesize) endByte++;
                                if (startByte == -1)
                                {
                                    startByte = filesize - endByte;
                                    endByte = filesize;
                                }
                                if (endByte > filesize) // Curl test showed this behaviour.
                                    endByte = filesize;
                                if (startByte >= filesize && endByte == filesize) // Curl test showed this behaviour.
                                {
                                    fs.Flush();
                                    fs.Close();
                                    ms.Flush();
                                    return new HttpResponse(false)
                                    {
                                        HttpStatusCode = HttpStatusCode.RangeNotSatisfiable
                                    };
                                }
                                else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                                {
                                    fs.Flush();
                                    fs.Close();
                                    ms.Flush();
                                    var okresponse = new HttpResponse(false)
                                    {
                                        HttpStatusCode = HttpStatusCode.Ok
                                    };
                                    okresponse.Headers.Add("Accept-Ranges", "bytes");
                                    okresponse.Headers.Add("Content-Type", CryptoSporidium.HTTPUtils.GetMimeType(Path.GetExtension(local_path)));
                                    okresponse.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read);

                                    return okresponse;
                                }
                                else
                                {
                                    byte[] ContentRange = Encoding.UTF8.GetBytes("Content-Range: " + string.Format("bytes {0}-{1}/{2}\n", startByte, endByte - 1, filesize));
                                    ms.Write(ContentRange, 0, ContentRange.Length);
                                    ms.WriteByte(Separator);
                                    long TotalBytes = endByte - startByte;
                                    // Check if endByte - startByte exceeds ConfigurationDefaults.BufferSize * 12.5
                                    // This is an AWESOME WORKAROUND for larger than 2gb files.
                                    if (TotalBytes > HTTPServerConfiguration.BufferSize * 12.5)
                                        TotalBytes = (long)(HTTPServerConfiguration.BufferSize * 12.5);
                                    Span<byte> buffer = new byte[TotalBytes];
                                    fs.Position = startByte;
                                    TotalBytes = fs.Read(buffer);
                                    ms.Write(buffer);
                                    ms.WriteByte(Separator);
                                }
                                i++;
                            }
                            ms.Write(EndingMultiPart, 0, EndingMultiPart.Length);
                            fs.Flush();
                            fs.Close();
                            var response = new HttpResponse(true)
                            {
                                HttpStatusCode = HttpStatusCode.PartialContent
                            };
                            response.Headers.Add("Content-Type", "multipart/byteranges; boundary=multiserver_separator");
                            response.Headers.Add("Accept-Ranges", "bytes");
                            ms.Position = 0;
                            response.ContentStream = new MemoryStream(ms.ToArray());
                            ms.Flush();
                            return response;
                        }
                    }
                    else
                    {
                        string[] range = HeaderString.Split('-');
                        if (range[0].Trim().Length > 0) _ = long.TryParse(range[0], out startByte);
                        if (range[1].Trim().Length > 0) _ = long.TryParse(range[1], out endByte);
                        if (endByte == -1) endByte = filesize;
                        else if (endByte != filesize) endByte++;
                        if (startByte == -1)
                        {
                            startByte = filesize - endByte;
                            endByte = filesize;
                        }
                    }
                }
                else
                {
                    startByte = 0;
                    endByte = filesize;
                }
                if (endByte > filesize) // Curl test showed this behaviour.
                    endByte = filesize;
                if (startByte >= filesize && endByte == filesize) // Curl test showed this behaviour.
                {
                    fs.Flush();
                    fs.Close();
                    return new HttpResponse(false)
                    {
                        HttpStatusCode = HttpStatusCode.RangeNotSatisfiable
                    };
                }
                else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                {
                    fs.Flush();
                    fs.Close();
                    var okresponse = new HttpResponse(false)
                    {
                        HttpStatusCode = HttpStatusCode.Ok
                    };
                    okresponse.Headers.Add("Accept-Ranges", "bytes");
                    okresponse.Headers.Add("Content-Type", CryptoSporidium.HTTPUtils.GetMimeType(Path.GetExtension(local_path)));
                    okresponse.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read);

                    return okresponse;
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
