// Copyright (C) 2016 by Barend Erasmus, David Jeske and donated to the public domain
using CryptoSporidium;
using HTTPServer.Models;
using System.Text;

namespace HTTPServer.RouteHandlers
{
    public class FileSystemRouteHandler
    {
        public static HttpResponse Handle(HttpRequest request, string filepath, string UserAgentLowercase)
        {
            if (Directory.Exists(filepath) && filepath.EndsWith("/"))
                return Handle_LocalDir(request, filepath);
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
                var response = new HttpResponse(false)
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

                return response;
            }
            else
                return HttpBuilder.NotFound();
        }

        private static HttpResponse Handle_LocalFile(string local_path)
        {
            var response = new HttpResponse(false)
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

        private static HttpResponse Handle_LocalFile_Stream(HttpRequest request, string local_path)
        {
            // We handle Range slightly differently, for now we limit a range of 500000 bytes at a maximum (magic c# memory compliant value).
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
                        using (HugeMemoryStream ms = new())
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
                            string[] rangeValues = HeaderString.Split(',');
                            foreach (string RangeSelect in rangeValues)
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
                                    var invalidresponse = new HttpResponse(false)
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
                                    var okresponse = new HttpResponse(false)
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
                                    // Check if endByte - startByte exceeds 500000 (value from DLNA Server : https://www.codeproject.com/Articles/1079847/DLNA-Media-Server-to-feed-Smart-TVs)
                                    // This is an AWESOME WORKAROUND for larger than 2gb files.
                                    if (TotalBytes > 500000)
                                        TotalBytes = 500000;
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
                            var response = new HttpResponse(true)
                            {
                                HttpStatusCode = HttpStatusCode.PartialContent
                            };
                            response.Headers.Add("Content-Type", "multipart/byteranges; boundary=multiserver_separator");
                            response.Headers.Add("Accept-Ranges", "bytes");
                            ms.Position = 0;
                            response.ContentStream = new HugeMemoryStream(ms);
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
                        var invalidresponse = new HttpResponse(false)
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
                        var okresponse = new HttpResponse(false)
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
                        var response = new HttpResponse(true)
                        {
                            HttpStatusCode = HttpStatusCode.PartialContent
                        };
                        long TotalBytes = endByte - startByte;
                        // Check if endByte - startByte exceeds 500000 (value from DLNA Server : https://www.codeproject.com/Articles/1079847/DLNA-Media-Server-to-feed-Smart-TVs)
                        // This is an AWESOME WORKAROUND for larger than 2gb files.
                        if (TotalBytes > 500000)
                            TotalBytes = 500000;
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
                        response.ContentStream = new HugeMemoryStream(buffer);
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

        private static HttpResponse Handle_LocalDir(HttpRequest request, string local_path)
        {
            string? encoding = request.GetHeaderValue("Accept-Encoding");

            if (!string.IsNullOrEmpty(encoding) && encoding.Contains("gzip"))
                return HttpResponse.Send(HTTPUtils.Compress(
                        Encoding.UTF8.GetBytes(FileStructureToJson.GetFileStructureAsJson(local_path[..^1]))), "application/json", new string[][] { new string[] { "Content-Encoding", "gzip" } });
            else
                return HttpResponse.Send(FileStructureToJson.GetFileStructureAsJson(local_path[..^1]), "application/json");
        }
    }

    public class HugeMemoryStream : Stream
    {
        #region Fields

        private const int PAGE_SIZE = 1024000;
        private const int ALLOC_STEP = 1024;

        private byte[][]? _streamBuffers;

        private int _pageCount = 0;
        private long _allocatedBytes = 0;

        private long _position = 0;
        private long _length = 0;

        #endregion Fields

        #region Internals

        public HugeMemoryStream(Stream st)
        {
            st.Position = 0;
            st.CopyTo(this);
            Position = 0;
        }

        public HugeMemoryStream(Span<byte> SpanToMem)
        {
            Write(SpanToMem);
            Position = 0;
        }

        public HugeMemoryStream()
        {

        }

        private int GetPageCount(long length)
        {
            int pageCount = (int)(length / PAGE_SIZE) + 1;

            if ((length % PAGE_SIZE) == 0)
                pageCount--;

            return pageCount;
        }

        private void ExtendPages()
        {
            if (_streamBuffers == null)
            {
                _streamBuffers = new byte[ALLOC_STEP][];
            }
            else
            {
                byte[][] streamBuffers = new byte[_streamBuffers.Length + ALLOC_STEP][];

                Array.Copy(_streamBuffers, streamBuffers, _streamBuffers.Length);

                _streamBuffers = streamBuffers;
            }

            _pageCount = _streamBuffers.Length;
        }

        private void AllocSpaceIfNeeded(long value)
        {
            if (value < 0)
                throw new InvalidOperationException("AllocSpaceIfNeeded < 0");

            if (value == 0)
                return;

            int currentPageCount = GetPageCount(_allocatedBytes);
            int neededPageCount = GetPageCount(value);

            while (currentPageCount < neededPageCount)
            {
                if (currentPageCount == _pageCount)
                    ExtendPages();

                _streamBuffers[currentPageCount++] = new byte[PAGE_SIZE];
            }

            _allocatedBytes = (long)currentPageCount * PAGE_SIZE;

            value = Math.Max(value, _length);

            if (_position > (_length = value))
                _position = _length;
        }

        #endregion Internals

        #region Stream

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _length;

        public override long Position
        {
            get { return _position; }
            set
            {
                if (value > _length)
                    throw new InvalidOperationException("Position > Length");
                else if (value < 0)
                    throw new InvalidOperationException("Position < 0");
                else
                    _position = value;
            }
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int currentPage = (int)(_position / PAGE_SIZE);
            int currentOffset = (int)(_position % PAGE_SIZE);
            int currentLength = PAGE_SIZE - currentOffset;

            long startPosition = _position;

            if (startPosition + count > _length)
                count = (int)(_length - startPosition);

            while (count != 0 && _position < _length)
            {
                if (currentLength > count)
                    currentLength = count;

                Array.Copy(_streamBuffers[currentPage++], currentOffset, buffer, offset, currentLength);

                offset += currentLength;
                _position += currentLength;
                count -= currentLength;

                currentOffset = 0;
                currentLength = PAGE_SIZE;
            }

            return (int)(_position - startPosition);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    break;

                case SeekOrigin.Current:
                    offset += _position;
                    break;

                case SeekOrigin.End:
                    offset = _length - offset;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("origin");
            }

            return Position = offset;
        }

        public override void SetLength(long value)
        {
            if (value < 0)
                throw new InvalidOperationException("SetLength < 0");

            if (value == 0)
            {
                _streamBuffers = null;
                _allocatedBytes = _position = _length = 0;
                _pageCount = 0;
                return;
            }

            int currentPageCount = GetPageCount(_allocatedBytes);
            int neededPageCount = GetPageCount(value);

            // Removes unused buffers if decreasing stream length
            while (currentPageCount > neededPageCount)
                _streamBuffers[--currentPageCount] = null;

            AllocSpaceIfNeeded(value);

            if (_position > (_length = value))
                _position = _length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int currentPage = (int)(_position / PAGE_SIZE);
            int currentOffset = (int)(_position % PAGE_SIZE);
            int currentLength = PAGE_SIZE - currentOffset;

            long startPosition = _position;

            AllocSpaceIfNeeded(_position + count);

            while (count != 0)
            {
                if (currentLength > count)
                    currentLength = count;

                Array.Copy(buffer, offset, _streamBuffers[currentPage++], currentOffset, currentLength);

                offset += currentLength;
                _position += currentLength;
                count -= currentLength;

                currentOffset = 0;
                currentLength = PAGE_SIZE;
            }
        }

        #endregion Stream
    }
}
