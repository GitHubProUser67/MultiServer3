using System.Net;
using System.Text;
using WatsonWebserver.Core;
using NetworkLibrary.HTTP;
using System.IO;
using System;
using NetworkLibrary.Extension;

namespace HTTPSecureServerLite
{
    public class LocalFileStreamHelper
    {
        public static bool Handle_LocalFile_Stream(HttpContextBase ctx, string filePath, string ContentType)
        {
            ctx.Response.ChunkedTransfer = false;

            // This method directly communicate with the wire to handle, normally, imposible transfers.
            // If a part of the code sounds weird to you, it's normal... So does curl tests...

            if (HTTPProcessor.CheckLastWriteTime(filePath, ctx.Request.RetrieveHeaderValue("If-Unmodified-Since"), true))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
                return ctx.Response.Send().Result;
            }
            else
            {
                const int rangebuffersize = 32768;

                string? acceptencoding = ctx.Request.RetrieveHeaderValue("Accept-Encoding");

                using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long startByte = -1;
                long endByte = -1;
                long filesize = fs.Length;
                string HeaderString = ctx.Request.RetrieveHeaderValue("Range").Replace("bytes=", string.Empty);
                if (HeaderString.Contains(','))
                {
                    using HugeMemoryStream ms = new();
                    Span<byte> Separator = new byte[] { 0x0D, 0x0A };
                    // Split the ranges based on the comma (',') separator
                    foreach (string RangeSelect in HeaderString.Split(','))
                    {
                        ms.Write(Separator);
                        ms.Write(Encoding.UTF8.GetBytes("--multiserver_separator").AsSpan());
                        ms.Write(Separator);
                        ms.Write(Encoding.UTF8.GetBytes($"Content-Type: {ContentType}").AsSpan());
                        ms.Write(Separator);
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
                            string payload = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n" +
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

                            ms.Flush();
                            ms.Close();
                            fs.Flush();
                            fs.Close();
                            ctx.Response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                            ctx.Response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
                            ctx.Response.ContentType = "text/html; charset=UTF-8";
                            if (HTTPSServerConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(acceptencoding))
                            {
                                if (acceptencoding.Contains("zstd"))
                                {
                                    ctx.Response.Headers.Add("Content-Encoding", "zstd");
                                    return ctx.Response.Send(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(payload))).Result;
                                }
                                else if (acceptencoding.Contains("br"))
                                {
                                    ctx.Response.Headers.Add("Content-Encoding", "br");
                                    return ctx.Response.Send(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(payload))).Result;
                                }
                                else if (acceptencoding.Contains("gzip"))
                                {
                                    ctx.Response.Headers.Add("Content-Encoding", "gzip");
                                    return ctx.Response.Send(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(payload))).Result;
                                }
                                else if (acceptencoding.Contains("deflate"))
                                {
                                    ctx.Response.Headers.Add("Content-Encoding", "deflate");
                                    return ctx.Response.Send(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(payload))).Result;
                                }
                                else
                                    return ctx.Response.Send(payload).Result;
                            }
                            else
                                return ctx.Response.Send(payload).Result;
                        }
                        else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                        {
                            ms.Flush();
                            ms.Close();
                            fs.Position = 0;

                            ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                            ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                            ctx.Response.Headers.Add("Accept-Ranges", "bytes");
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                            ctx.Response.ContentType = ContentType;
                            return ctx.Response.Send(new FileInfo(filePath).Length, fs).Result;
                        }
                        else
                        {
                            int bytesRead = 0;
                            long TotalBytes = endByte - startByte;
                            long totalBytesCopied = 0;
                            byte[] buffer = new byte[rangebuffersize];
                            fs.Position = startByte;
                            ms.Write(Encoding.UTF8.GetBytes("Content-Range: " + string.Format("bytes {0}-{1}/{2}", startByte, endByte - 1, filesize)).AsSpan());
                            ms.Write(Separator);
                            ms.Write(Separator);
                            while (totalBytesCopied < TotalBytes && (bytesRead = fs.Read(buffer, 0, rangebuffersize)) > 0)
                            {
                                int bytesToWrite = (int)Math.Min(TotalBytes - totalBytesCopied, bytesRead);
                                ms.Write(buffer, 0, bytesToWrite);
                                totalBytesCopied += bytesToWrite;
                            }
                        }
                    }
                    ms.Write(Separator);
                    ms.Write(Encoding.UTF8.GetBytes("--multiserver_separator--").AsSpan());
                    ms.Write(Separator);
                    ms.Position = 0;
                    ctx.Response.ProtocolVersion = "1.0"; // Partial Content doesn't like chunked encoding on some broken browsers (netscape).
                    ctx.Response.ContentType = "multipart/byteranges; boundary=multiserver_separator";
                    ctx.Response.Headers.Add("Accept-Ranges", "bytes");
                    ctx.Response.Headers.Add("Content-Length", ms.Length.ToString());
                    ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                    ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                    ctx.Response.StatusCode = (int)HttpStatusCode.PartialContent;

                    fs.Flush();
                    fs.Close();

                    return ctx.Response.Send(ms.Length, ms).Result;
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
                    string payload = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n" +
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

                    fs.Flush();
                    fs.Close();
                    ctx.Response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                    ctx.Response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
                    ctx.Response.ContentType = "text/html; charset=UTF-8";
                    if (HTTPSServerConfiguration.EnableHTTPCompression && !string.IsNullOrEmpty(acceptencoding))
                    {
                        if (acceptencoding.Contains("zstd"))
                        {
                            ctx.Response.Headers.Add("Content-Encoding", "zstd");
                            return ctx.Response.Send(HTTPProcessor.CompressZstd(Encoding.UTF8.GetBytes(payload))).Result;
                        }
                        else if (acceptencoding.Contains("br"))
                        {
                            ctx.Response.Headers.Add("Content-Encoding", "br");
                            return ctx.Response.Send(HTTPProcessor.CompressBrotli(Encoding.UTF8.GetBytes(payload))).Result;
                        }
                        else if (acceptencoding.Contains("gzip"))
                        {
                            ctx.Response.Headers.Add("Content-Encoding", "gzip");
                            return ctx.Response.Send(HTTPProcessor.CompressGzip(Encoding.UTF8.GetBytes(payload))).Result;
                        }
                        else if (acceptencoding.Contains("deflate"))
                        {
                            ctx.Response.Headers.Add("Content-Encoding", "deflate");
                            return ctx.Response.Send(HTTPProcessor.Inflate(Encoding.UTF8.GetBytes(payload))).Result;
                        }
                        else
                            return ctx.Response.Send(payload).Result;
                    }
                    else
                        return ctx.Response.Send(payload).Result;
                }
                else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                {
                    fs.Position = 0;

                    ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                    ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                    ctx.Response.Headers.Add("Accept-Ranges", "bytes");
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                    ctx.Response.ContentType = ContentType;
                    return ctx.Response.Send(new FileInfo(filePath).Length, fs).Result;
                }
                else
                {
                    long TotalBytes = endByte - startByte;
                    fs.Position = startByte;
                    ctx.Response.ProtocolVersion = "1.0"; // Partial Content doesn't like chunked encoding on some broken browsers (netscape).
                    ctx.Response.ContentType = ContentType;
                    ctx.Response.Headers.Add("Accept-Ranges", "bytes");
                    ctx.Response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", startByte, endByte - 1, filesize));
                    ctx.Response.Headers.Add("Content-Length", TotalBytes.ToString());
                    ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                    ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(filePath).ToString("r"));
                    ctx.Response.StatusCode = (int)HttpStatusCode.PartialContent;

                    return ctx.Response.Send(TotalBytes, fs).Result;
                }
            }
        }
    }
}
