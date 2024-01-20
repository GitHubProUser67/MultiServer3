// Copyright (C) 2016 by Barend Erasmus, David Jeske and donated to the public domain
using BackendProject;
using HTTPServer.Models;
using System.Text;

namespace HTTPServer.RouteHandlers
{
    public class FileSystemRouteHandler
    {
        public static HttpResponse Handle(HttpRequest request, string filepath, string UserAgentLowercase, string httpdirectoryrequest)
        {
            if (Directory.Exists(filepath) && filepath.EndsWith("/"))
                return Handle_LocalDir(request, filepath, httpdirectoryrequest);
            else if (File.Exists(filepath) && request.Headers.Keys.Count(x => x == "Range") == 1
                && !UserAgentLowercase.Contains("vlc") && !UserAgentLowercase.Contains("lavf")) // Range can only be sent once, put here UserAgent of clients not liking our Range system.
                return Handle_LocalFile_Stream(request, filepath);                              // Most apps are fine with the idea of a Range bellow what requested (which should work), VLC and MPC-HC don't...
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
                    byte[] VerificationChunck = MiscUtils.ReadSmallFileChunck(local_path, 10);
                    foreach (var entry in HTTPUtils.PathernDictionary)
                    {
                        if (MiscUtils.FindbyteSequence(VerificationChunck, entry.Value))
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
                byte[] VerificationChunck = MiscUtils.ReadSmallFileChunck(local_path, 10);
                foreach (var entry in HTTPUtils.PathernDictionary)
                {
                    if (MiscUtils.FindbyteSequence(VerificationChunck, entry.Value))
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

        private static HttpResponse Handle_LocalFile_Stream(HttpRequest request, string local_path)
        {
            // We handle Range slightly differently, for now we limit a range of 8000000 bytes at a maximum (magic c# memory compliant value).
            // This decision was made to not stress the server too much.

            using (FileStream fs = new(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                long startByte = -1;
                long endByte = -1;
                try
                {
                    long filesize = fs.Length;
                    string HeaderString = request.GetHeaderValue("Range").Replace("bytes=", string.Empty);
                    if (HeaderString.Contains(','))
                    {
                        using (MemoryStream ms = new())
                        {
                            byte[] Separator = new byte[] { 0x0D, 0x0A };
                            byte[] MultiPart = Encoding.UTF8.GetBytes("--multiserver_separator");
                            byte[] EndingMultiPart = Encoding.UTF8.GetBytes("--multiserver_separator--");
                            string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
                            if (ContentType == "application/octet-stream")
                            {
                                byte[] VerificationChunck = MiscUtils.ReadSmallFileChunck(local_path, 10);
                                foreach (var entry in HTTPUtils.PathernDictionary)
                                {
                                    if (MiscUtils.FindbyteSequence(VerificationChunck, entry.Value))
                                    {
                                        ContentType = entry.Key;
                                        break;
                                    }
                                }
                            }
                            byte[] MimeType = Encoding.UTF8.GetBytes($"Content-Type: {ContentType}");
                            // Split the ranges based on the comma (',') separator
                            foreach (string RangeSelect in HeaderString.Split(','))
                            {
                                ms.Write(Separator, 0, Separator.Length);
                                ms.Write(MultiPart, 0, MultiPart.Length);
                                ms.Write(Separator, 0, Separator.Length);
                                ms.Write(MimeType, 0, MimeType.Length);
                                ms.Write(Separator, 0, Separator.Length);
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
                                    ms.Close();
                                    HttpResponse invalidresponse = new(false)
                                    {
                                        HttpStatusCode = HttpStatusCode.RangeNotSatisfiable
                                    };
                                    invalidresponse.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                                    invalidresponse.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                                    invalidresponse.ContentAsUTF8 = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n" +
                                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\r\n" +
                                        "         \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n" +
                                        "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\r\n" +
                                        "        <head>\r\n" +
                                        "                <title>416 - Requested Range Not Satisfiable</title>\r\n" +
                                        "        </head>\r\n" +
                                        "        <body>\r\n" +
                                        "                <h1>416 - Requested Range Not Satisfiable</h1>\r\n" +
                                        "        </body>\r\n" +
                                        "</html>";

                                    return invalidresponse;
                                }
                                else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                                {
                                    fs.Flush();
                                    fs.Close();
                                    ms.Flush();
                                    ms.Close();
                                    HttpResponse okresponse = new(false)
                                    {
                                        HttpStatusCode = HttpStatusCode.Ok
                                    };
                                    okresponse.Headers.Add("Accept-Ranges", "bytes");
                                    okresponse.Headers.Add("Content-Type", ContentType);
                                    okresponse.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                                    return okresponse;
                                }
                                else
                                {
                                    long TotalBytes = endByte - startByte;
                                    // Check if endByte - startByte exceeds 8000000 (value from DLNA Server : https://www.codeproject.com/Articles/1079847/DLNA-Media-Server-to-feed-Smart-TVs)
                                    // This is an AWESOME WORKAROUND for larger than 2gb files.
                                    if (TotalBytes > 8000000)
                                        TotalBytes = 8000000;
                                    Span<byte> buffer = new byte[TotalBytes];
                                    fs.Position = startByte;
                                    TotalBytes = fs.Read(buffer);
                                    TotalBytes = startByte + buffer.Length; // We optimize the spare integer to store total bytes.
                                    byte[] ContentRange = Encoding.UTF8.GetBytes("Content-Range: " + string.Format("bytes {0}-{1}/{2}", startByte, TotalBytes - 1, filesize));
                                    ms.Write(ContentRange, 0, ContentRange.Length);
                                    ms.Write(Separator, 0, Separator.Length);
                                    ms.Write(Separator, 0, Separator.Length);
                                    ms.Write(buffer);
                                }
                            }
                            ms.Write(Separator, 0, Separator.Length);
                            ms.Write(EndingMultiPart, 0, EndingMultiPart.Length);
                            ms.Write(Separator, 0, Separator.Length);
                            fs.Flush();
                            fs.Close();
                            HttpResponse response = new(true)
                            {
                                HttpStatusCode = HttpStatusCode.PartialContent
                            };
                            response.Headers.Add("Content-Type", "multipart/byteranges; boundary=multiserver_separator");
                            response.Headers.Add("Accept-Ranges", "bytes");
                            ms.Position = 0;
                            response.ContentStream = new MemoryStream(ms.ToArray());
                            ms.Flush();
                            ms.Close();
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
                    if (endByte > filesize) // Curl test showed this behaviour.
                        endByte = filesize;
                    if (startByte >= filesize && endByte == filesize) // Curl test showed this behaviour.
                    {
                        fs.Flush();
                        fs.Close();
                        HttpResponse invalidresponse = new(false)
                        {
                            HttpStatusCode = HttpStatusCode.RangeNotSatisfiable
                        };
                        invalidresponse.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                        invalidresponse.Headers.Add("Content-Type", "text/html; charset=UTF-8");
                        invalidresponse.ContentAsUTF8 = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n" +
                            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\r\n" +
                            "         \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n" +
                            "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\r\n" +
                            "        <head>\r\n" +
                            "                <title>416 - Requested Range Not Satisfiable</title>\r\n" +
                            "        </head>\r\n" +
                            "        <body>\r\n" +
                            "                <h1>416 - Requested Range Not Satisfiable</h1>\r\n" +
                            "        </body>\r\n" +
                            "</html>";

                        return invalidresponse;
                    }
                    else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                    {
                        fs.Flush();
                        fs.Close();
                        HttpResponse okresponse = new(false)
                        {
                            HttpStatusCode = HttpStatusCode.Ok
                        };
                        okresponse.Headers.Add("Accept-Ranges", "bytes");
                        string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
                        if (ContentType == "application/octet-stream")
                        {
                            bool matched = false;
                            byte[] VerificationChunck = MiscUtils.ReadSmallFileChunck(local_path, 10);
                            foreach (var entry in HTTPUtils.PathernDictionary)
                            {
                                if (MiscUtils.FindbyteSequence(VerificationChunck, entry.Value))
                                {
                                    matched = true;
                                    okresponse.Headers["Content-Type"] = entry.Key;
                                    break;
                                }
                            }
                            if (!matched)
                                okresponse.Headers["Content-Type"] = ContentType;
                        }
                        else
                            okresponse.Headers["Content-Type"] = ContentType;
                        okresponse.ContentStream = File.Open(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                        return okresponse;
                    }
                    else
                    {
                        HttpResponse response = new(true)
                        {
                            HttpStatusCode = HttpStatusCode.PartialContent
                        };
                        long TotalBytes = endByte - startByte;
                        // Check if endByte - startByte exceeds 8000000 (value from DLNA Server : https://www.codeproject.com/Articles/1079847/DLNA-Media-Server-to-feed-Smart-TVs)
                        // This is an AWESOME WORKAROUND for larger than 2gb files.
                        if (TotalBytes > 8000000)
                            TotalBytes = 8000000;
                        Span<byte> buffer = new byte[TotalBytes];
                        fs.Position = startByte;
                        TotalBytes = fs.Read(buffer);
                        fs.Flush();
                        fs.Close();
                        TotalBytes = startByte + buffer.Length; // We optimize the spare integer to store total bytes.
                        string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
                        if (ContentType == "application/octet-stream")
                        {
                            bool matched = false;
                            byte[] VerificationChunck = MiscUtils.ReadSmallFileChunck(local_path, 10);
                            foreach (var entry in HTTPUtils.PathernDictionary)
                            {
                                if (MiscUtils.FindbyteSequence(VerificationChunck, entry.Value))
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
                        response.Headers.Add("Accept-Ranges", "bytes");
                        response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", startByte, TotalBytes - 1, filesize));
                        response.ContentStream = new MemoryStream(buffer.ToArray());
                        return response;
                    }
                }
                catch (Exception)
                {
                    // Not Important
                }

                try
                {
                    fs.Flush();
                    fs.Close();
                }
                catch (ObjectDisposedException)
                {
                    // fs has been disposed already.
                }
            }
            return new HttpResponse(false)
            {
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
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
