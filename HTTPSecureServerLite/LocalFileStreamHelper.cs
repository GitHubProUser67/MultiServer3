using BackendProject.MiscUtils;
using System.Net;
using System.Text;
using WatsonWebserver.Core;

namespace HTTPSecureServerLite
{
    public class LocalFileStreamHelper
    {
        public bool Handle_LocalFile_Stream(HttpContextBase ctx, string local_path)
        {
            // This method directly communicate with the wire to handle, normally, imposible transfers.
            // If a part of the code sounds weird to you, it's normal... So does curl tests...
            const int rangebuffersize = 32768;

            using (FileStream fs = new(local_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                long startByte = -1;
                long endByte = -1;
                long filesize = fs.Length;
                string HeaderString = ctx.Request.RetrieveHeaderValue("Range").Replace("bytes=", string.Empty);
                if (HeaderString.Contains(','))
                {
                    using (MemoryStream ms = new())
                    {
                        Span<byte> Separator = new byte[] { 0x0D, 0x0A };
                        string ContentType = HTTPUtils.GetMimeType(Path.GetExtension(local_path));
                        if (ContentType == "application/octet-stream")
                        {
                            foreach (var entry in HTTPUtils.PathernDictionary)
                            {
                                if (VariousUtils.FindbyteSequence(VariousUtils.ReadSmallFileChunck(local_path, 10), entry.Value))
                                {
                                    ContentType = entry.Key;
                                    break;
                                }
                            }
                        }
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
                                ms.Flush();
                                ms.Close();
                                fs.Flush();
                                fs.Close();
                                ctx.Response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                                ctx.Response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
                                ctx.Response.ContentType = "text/html; charset=UTF-8";
                                return ctx.Response.Send("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n" +
                                    "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\r\n" +
                                    "         \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n" +
                                    "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\r\n" +
                                    "        <head>\r\n" +
                                    "                <title>416 - Requested Range Not Satisfiable</title>\r\n" +
                                    "        </head>\r\n" +
                                    "        <body>\r\n" +
                                    "                <h1>416 - Requested Range Not Satisfiable</h1>\r\n" +
                                    "        </body>\r\n" +
                                    "</html>").Result;
                            }
                            else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                            {
                                ms.Flush();
                                ms.Close();
                                ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                                ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                                ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));
                                ctx.Response.Headers.Add("Accept-Ranges", "bytes");
                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                ctx.Response.ContentType = ContentType;
                                fs.Position = 0;
                                return ctx.Response.Send(new FileInfo(local_path).Length, fs).Result;
                            }
                            else
                            {
                                int bytesRead = 0;
                                long TotalBytes = endByte - startByte - 1;
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
                        ctx.Response.Headers.Add("Content-Type", "multipart/byteranges; boundary=multiserver_separator");
                        ctx.Response.Headers.Add("Accept-Ranges", "bytes");
                        ctx.Response.Headers.Add("Server", VariousUtils.GenerateServerSignature());
                        ctx.Response.Headers.Add("Content-Length", ms.Length.ToString());
                        ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                        ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                        ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));
                        ctx.Response.StatusCode = (int)HttpStatusCode.PartialContent;

                        fs.Flush();
                        fs.Close();

                        return ctx.Response.Send(ms.Length, ms).Result;
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
                    ctx.Response.Headers.Add("Content-Range", string.Format("bytes */{0}", filesize));
                    ctx.Response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
                    ctx.Response.ContentType = "text/html; charset=UTF-8";
                    return ctx.Response.Send("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n" +
                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\r\n" +
                        "         \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n" +
                        "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">\r\n" +
                        "        <head>\r\n" +
                        "                <title>416 - Requested Range Not Satisfiable</title>\r\n" +
                        "        </head>\r\n" +
                        "        <body>\r\n" +
                        "                <h1>416 - Requested Range Not Satisfiable</h1>\r\n" +
                        "        </body>\r\n" +
                        "</html>").Result;
                }
                else if ((startByte >= endByte) || startByte < 0 || endByte <= 0) // Curl test showed this behaviour.
                {
                    ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                    ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                    ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));
                    ctx.Response.Headers.Add("Accept-Ranges", "bytes");
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
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
                                ctx.Response.ContentType = entry.Key;
                                break;
                            }
                        }
                        if (!matched)
                            ctx.Response.ContentType = ContentType;
                    }
                    else
                        ctx.Response.ContentType = ContentType;
                    fs.Position = 0;
                    return ctx.Response.Send(new FileInfo(local_path).Length, fs).Result;
                }
                else
                {
                    long TotalBytes = endByte - startByte; // Todo : Curl showed that we should load TotalBytes - 1, but VLC and Chrome complains about it...
                    fs.Position = startByte;
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
                                ctx.Response.ContentType = entry.Key;
                                break;
                            }
                        }
                        if (!matched)
                            ctx.Response.ContentType = ContentType;
                    }
                    else
                        ctx.Response.ContentType = ContentType;
                    ctx.Response.Headers.Add("Accept-Ranges", "bytes");
                    ctx.Response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", startByte, endByte - 1, filesize));
                    ctx.Response.Headers.Add("Server", VariousUtils.GenerateServerSignature());
                    ctx.Response.Headers.Add("Content-Length", TotalBytes.ToString());
                    ctx.Response.Headers.Add("Date", DateTime.Now.ToString("r"));
                    ctx.Response.Headers.Add("ETag", Guid.NewGuid().ToString()); // Well, kinda wanna avoid client caching.
                    ctx.Response.Headers.Add("Last-Modified", File.GetLastWriteTime(local_path).ToString("r"));
                    ctx.Response.StatusCode = (int)HttpStatusCode.PartialContent;

                    return ctx.Response.Send(TotalBytes, fs).Result;
                }
            }
        }
    }
}
