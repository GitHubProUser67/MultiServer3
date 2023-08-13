using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace PSMultiServer.PoodleHTTP
{
    public static class Extensions
    {
        public static readonly JsonSerializerSettings JsonSettings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static async Task ReturnNotFoundError(HttpListenerResponse response)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(PreMadeWebPages.filenotfound);

            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "text/html";
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        }

        public static async Task<byte[]> ProcessPhpPage(string pageFileName, string phpver, HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] returndata;
            var index = request.RawUrl.IndexOf("?");
            var queryString = index == -1 ? "" : request.RawUrl.Substring(index + 1);
            // Get paths for PHP
            var documentRootPath = Path.GetDirectoryName(pageFileName);
            var scriptFilePath = Path.GetFullPath(pageFileName);
            var scriptFileName = Path.GetFileName(pageFileName);
            var tempPath = Path.GetTempPath();

            // Extract POST data (if available)
            string postData = null;
            if (request.HttpMethod == "POST")
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await request.InputStream.CopyToAsync(ms);
                    postData = Convert.ToBase64String(ms.ToArray());
                }
            }

            Process proc = new Process();

            proc.StartInfo.FileName = Directory.GetCurrentDirectory() + $"{ServerConfiguration.PHPStaticFolder}{phpver}/php-cgi";

            if (postData == null)
            {
                proc.StartInfo.Arguments = $"-q -d \"error_reporting=E_ALL\" -d \"display_errors={ServerConfiguration.PHPDebugErrors}\" -d \"expose_php=Off\" -d \"include_path='{documentRootPath}'\" " +
                $"-d \"extension_dir='{Directory.GetCurrentDirectory() + $@"{ServerConfiguration.PHPStaticFolder}{phpver}/ext/"}'\" -d \"extension=php_bz2.dll\" " +
                $"-d \"extension=php_com_dotnet.dll\" -d \"extension=php_curl.dll\" -d \"extension=php_dba.dll\" -d \"extension=php_dl_test.dll\" -d \"extension=php_enchant.dll\" " +
                $"-d \"extension=php_exif.dll\" -d \"extension=php_ffi.dll\" -d \"extension=php_fileinfo.dll\" -d \"extension=php_ftp.dll\" -d \"extension=php_gd.dll\" -d \"extension=php_gettext.dll\" " +
                $"-d \"extension=php_gmp.dll\" -d \"extension=php_imap.dll\" -d \"extension=php_intl.dll\" -d \"extension=php_ldap.dll\" -d \"extension=php_mbstring.dll\" " +
                $"-d \"extension=php_mysqli.dll\" -d \"extension=php_odbc.dll\" -d \"extension=php_openssl.dll\" " +
                $"-d \"extension=php_pdo_mysql.dll\" -d \"extension=php_pdo_odbc.dll\" -d \"extension=php_pdo_pgsql.dll\" " +
                $"-d \"extension=php_pdo_sqlite.dll\" -d \"extension=php_pgsql.dll\" -d \"extension=php_shmop.dll\" -d \"extension=php_snmp.dll\" -d \"extension=php_soap.dll\" " +
                $"-d \"extension=php_sockets.dll\" -d \"extension=php_sodium.dll\" -d \"extension=php_sqlite3.dll\" -d \"extension=php_sysvshm.dll\" -d \"extension=php_tidy.dll\" " +
                $"-d \"extension=php_xsl.dll\" -d \"extension=php_zend_test.dll\" -d \"extension=php_zip.dll\" \"{pageFileName}\"";
            }
            else
            {
                proc.StartInfo.Arguments = $"-q -d \"error_reporting=E_ALL\" -d \"display_errors={ServerConfiguration.PHPDebugErrors}\" -d \"expose_php=Off\" -d \"include_path='{documentRootPath}'\" " +
                $"-d \"extension_dir='{Directory.GetCurrentDirectory() + $@"{ServerConfiguration.PHPStaticFolder}{phpver}/ext/"}'\" -d \"extension=php_bz2.dll\" " +
                $"-d \"extension=php_com_dotnet.dll\" -d \"extension=php_curl.dll\" -d \"extension=php_dba.dll\" -d \"extension=php_dl_test.dll\" -d \"extension=php_enchant.dll\" " +
                $"-d \"extension=php_exif.dll\" -d \"extension=php_ffi.dll\" -d \"extension=php_fileinfo.dll\" -d \"extension=php_ftp.dll\" -d \"extension=php_gd.dll\" -d \"extension=php_gettext.dll\" " +
                $"-d \"extension=php_gmp.dll\" -d \"extension=php_imap.dll\" -d \"extension=php_intl.dll\" -d \"extension=php_ldap.dll\" -d \"extension=php_mbstring.dll\" " +
                $"-d \"extension=php_mysqli.dll\" -d \"extension=php_odbc.dll\" -d \"extension=php_openssl.dll\" " +
                $"-d \"extension=php_pdo_mysql.dll\" -d \"extension=php_pdo_odbc.dll\" -d \"extension=php_pdo_pgsql.dll\" " +
                $"-d \"extension=php_pdo_sqlite.dll\" -d \"extension=php_pgsql.dll\" -d \"extension=php_shmop.dll\" -d \"extension=php_snmp.dll\" -d \"extension=php_soap.dll\" " +
                $"-d \"extension=php_sockets.dll\" -d \"extension=php_sodium.dll\" -d \"extension=php_sqlite3.dll\" -d \"extension=php_sysvshm.dll\" -d \"extension=php_tidy.dll\" " +
                $"-d \"extension=php_xsl.dll\" -d \"extension=php_zend_test.dll\" -d \"extension=php_zip.dll\" -d \"post_data={postData}\" \"{pageFileName}\"";
            }

            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            proc.StartInfo.EnvironmentVariables.Clear();

            // Set environment variables for PHP

            // Set content length for POST data
            if (postData != null)
                proc.StartInfo.EnvironmentVariables.Add("CONTENT_LENGTH", postData.Length.ToString());

            proc.StartInfo.EnvironmentVariables.Add("GATEWAY_INTERFACE", "CGI/1.1");
            proc.StartInfo.EnvironmentVariables.Add("SERVER_PROTOCOL", "HTTP/1.1");
            proc.StartInfo.EnvironmentVariables.Add("REDIRECT_STATUS", "200");
            proc.StartInfo.EnvironmentVariables.Add("DOCUMENT_ROOT", documentRootPath);
            proc.StartInfo.EnvironmentVariables.Add("SCRIPT_NAME", scriptFileName);
            proc.StartInfo.EnvironmentVariables.Add("SCRIPT_FILENAME", scriptFilePath);
            proc.StartInfo.EnvironmentVariables.Add("QUERY_STRING", queryString);
            proc.StartInfo.EnvironmentVariables.Add("CONTENT_TYPE", request.ContentType);
            proc.StartInfo.EnvironmentVariables.Add("REQUEST_METHOD", request.HttpMethod);
            proc.StartInfo.EnvironmentVariables.Add("USER_AGENT", request.UserAgent);
            proc.StartInfo.EnvironmentVariables.Add("SERVER_ADDR", request.LocalEndPoint.Address.ToString());
            proc.StartInfo.EnvironmentVariables.Add("REMOTE_ADDR", request.RemoteEndPoint.Address.ToString());
            proc.StartInfo.EnvironmentVariables.Add("REMOTE_PORT", request.RemoteEndPoint.Port.ToString());
            proc.StartInfo.EnvironmentVariables.Add("REFERER", request.UrlReferrer?.ToString() ?? "");
            proc.StartInfo.EnvironmentVariables.Add("REQUEST_URI", request.Url.AbsoluteUri);
            proc.StartInfo.EnvironmentVariables.Add("HTTP_COOKIE", request.Headers["Cookie"]);
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT", request.Headers["Accept"]);
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_CHARSET", request.Headers["Accept-Charset"]);
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_ENCODING", request.Headers["Accept-Encoding"]);
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_LANGUAGE", request.Headers["Accept-Language"]);
            proc.StartInfo.EnvironmentVariables.Add("TMPDIR", tempPath);
            proc.StartInfo.EnvironmentVariables.Add("TEMP", tempPath);

            proc.Start();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamReader reader = proc.StandardOutput)
                {
                    // Read PHP output to memory stream
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        memoryStream.Write(buffer, 0, bytesRead);
                    }

                    // If no output, read error stream
                    if (memoryStream.Length == 0)
                    {
                        using (StreamReader errorReader = proc.StandardError)
                        {
                            string errorOutput = errorReader.ReadToEnd();
                            errorOutput = PreMadeWebPages.phperror.Replace("PUT_ERROR_HERE", errorOutput);
                            byte[] errorOutputBytes = RemoveUnwantedHeaders(Encoding.UTF8.GetBytes(errorOutput));
                            memoryStream.Write(errorOutputBytes, 0, errorOutputBytes.Length);
                        }
                    }

                    // Process Set-Cookie headers
                    List<string> setCookieHeaders = new List<string>();
                    foreach (var header in proc.StandardOutput.ReadToEnd().Split('\n'))
                    {
                        if (header.Trim().StartsWith("Set-Cookie:", StringComparison.OrdinalIgnoreCase))
                        {
                            setCookieHeaders.Add(header.Trim());
                        }
                    }

                    // Add cookies to the HttpListenerResponse
                    foreach (var setCookieHeader in setCookieHeaders)
                    {
                        int colonIndex = setCookieHeader.IndexOf(':');
                        if (colonIndex != -1)
                        {
                            string cookieHeaderValue = setCookieHeader.Substring(colonIndex + 1).Trim();
                            string[] cookieParts = cookieHeaderValue.Split(';');

                            foreach (var cookiePart in cookieParts)
                            {
                                int equalIndex = cookiePart.IndexOf('=');
                                if (equalIndex != -1)
                                {
                                    // Create and set the cookie in the response
                                    Cookie cookie = new Cookie(cookiePart.Substring(0, equalIndex).Trim(), cookiePart.Substring(equalIndex + 1).Trim());
                                    cookie.Path = "/"; // Set the cookie path as needed
                                    response.Cookies.Add(cookie);
                                }
                            }
                        }
                    }

                    // Get the final byte array
                    returndata = RemoveUnwantedHeaders(memoryStream.ToArray());

                    reader.Dispose();
                }

                memoryStream.Dispose();
            }

            proc.WaitForExit(); // Wait for the PHP process to complete
            proc.Close();

            return returndata;
        }

        private static byte[] RemoveUnwantedHeaders(byte[] phpOutputBytes)
        {
            // Find the index where headers end and content starts (indicated by an empty line)
            int emptyLineIndex = -1;
            for (int i = 0; i < phpOutputBytes.Length - 3; i++)
            {
                if (phpOutputBytes[i] == '\r' && phpOutputBytes[i + 1] == '\n' && phpOutputBytes[i + 2] == '\r' && phpOutputBytes[i + 3] == '\n')
                {
                    emptyLineIndex = i + 4; // Skip the empty line
                    break;
                }
            }

            if (emptyLineIndex == -1)
            {
                // If no empty line found, return the original bytes
                return phpOutputBytes;
            }

            List<byte> filteredOutput = new List<byte>();

            bool skipHeaders = false;

            for (int i = emptyLineIndex; i < phpOutputBytes.Length; i++)
            {
                byte currentByte = phpOutputBytes[i];

                if (currentByte == '\r' && i < phpOutputBytes.Length - 1 && phpOutputBytes[i + 1] == '\n')
                {
                    // Empty line indicates end of headers, switch to normal content
                    skipHeaders = true;
                    filteredOutput.Add((byte)'\r');
                    filteredOutput.Add((byte)'\n');
                    i++; // Skip the '\n' character
                }
                else if (!skipHeaders)
                {
                    // Check if the line contains unwanted headers and skip them
                    bool skipLine = false;

                    if (currentByte == 'C' && i + 12 < phpOutputBytes.Length && CheckHeaderMatch(phpOutputBytes, i, "Content-Type:"))
                    {
                        skipLine = true;
                        i += 13; // Skip "Content-Type:" and the following space
                    }
                    else if (currentByte == 'S' && i + 10 < phpOutputBytes.Length && CheckHeaderMatch(phpOutputBytes, i, "Set-Cookie:"))
                    {
                        skipLine = true;
                        i += 11; // Skip "Set-Cookie:" and the following space
                    }

                    if (!skipLine)
                    {
                        filteredOutput.Add(currentByte);
                    }
                }
                else
                {
                    filteredOutput.Add(currentByte);
                }
            }

            return filteredOutput.ToArray();
        }

        public static string ExtractBoundary(string contentType)
        {
            int boundaryIndex = contentType.IndexOf("boundary=", StringComparison.InvariantCultureIgnoreCase);
            if (boundaryIndex != -1)
            {
                return contentType.Substring(boundaryIndex + 9);
            }
            return null;
        }

        private static bool CheckHeaderMatch(byte[] byteArray, int startIndex, string header)
        {
            for (int i = 0; i < header.Length; i++)
            {
                if (startIndex + i >= byteArray.Length || byteArray[startIndex + i] != (byte)header[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static async Task File(this HttpListenerResponse response, string filePath, bool compress)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return;
            }

            await FileHelper.ReadAsync(filePath, async stream =>
            {
                response.ContentType = MimeTypes.GetMimeType(filePath);
                response.ContentLength64 = stream.Length;
                if (compress)
                    using (var compressedStream = new GZipStream(stream, CompressionMode.Compress))
                        await compressedStream.CopyToAsync(response.OutputStream);
                else
                    await stream.CopyToAsync(response.OutputStream);
            });
        }

        public static async Task Error(this HttpListenerResponse response, bool dataresponse, int statusCode = 500)
        {
            response.StatusCode = statusCode;

            if (dataresponse)
                await ReturnNotFoundError(response);
        }

        public static Task WriteFile(HttpListenerResponse response, string path)
        {
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                string filename = Path.GetFileName(path);

                response.AddHeader("Content-disposition", "attachment; filename=" + filename);

                byte[] buffer = new byte[64 * 1024];
                int read;
                using (BinaryWriter bw = new BinaryWriter(response.OutputStream))
                {
                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bw.Write(buffer, 0, read);
                        bw.Flush();
                    }

                    bw.Close();
                    bw.Dispose();
                }

                response.StatusCode = (int)HttpStatusCode.OK;
                response.StatusDescription = "OK";
                response.SendChunked = false;
                response.ContentLength64 = fs.Length;

                fs.Dispose();
            }

            return Task.CompletedTask;
        }

        public static byte[] ReadLine(BinaryReader reader)
        {
            using (MemoryStream lineStream = new MemoryStream())
            {
                byte prevByte = 0;
                byte currentByte;

                while ((currentByte = reader.ReadByte()) != -1)
                {
                    if (prevByte == 13 && currentByte == 10)
                    {
                        lineStream.SetLength(lineStream.Length - 1); // Remove the trailing \r\n
                        return lineStream.ToArray();
                    }

                    lineStream.WriteByte(currentByte);
                    prevByte = currentByte;
                }

                if (lineStream.Length > 0)
                    return lineStream.ToArray();

                return null;
            }
        }

        public static bool ByteArrayStartsWith(byte[] byteArray, byte[] prefix)
        {
            if (byteArray.Length < prefix.Length)
                return false;

            for (int i = 0; i < prefix.Length; i++)
            {
                if (byteArray[i] != prefix[i])
                    return false;
            }

            return true;
        }

        private static bool RemoveAllPrefixes(HttpListener listener)
        {
            // Get the prefixes that the Web server is listening to.
            HttpListenerPrefixCollection prefixes = listener.Prefixes;
            try
            {
                prefixes.Clear();
            }
            // If the operation failed, return false.
            catch
            {
                return false;
            }

            return true;
        }

        public static async Task<T?> JsonFromBody<T>(this HttpListenerRequest request)
        {
            string jsonText = await request.TextFromBody();
            return jsonText.ToObject<T>();
        }

        public static async Task<string> TextFromBody(this HttpListenerRequest request)
        {
            using StreamReader reader = new(request.InputStream);
            return await reader.ReadToEndAsync();
        }

        public static string ToJson(this object value, Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(value, formatting, JsonSettings);
        }

        public static T? ToObject<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JsonSettings);
        }
    }
}
