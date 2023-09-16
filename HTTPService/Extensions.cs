using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace MultiServer.HTTPService
{
    public class Extensions
    {
        public static Task ReturnNotFoundError(HttpListenerResponse response, string link)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(PreMadeWebPages.filenotfound.Replace("PUT_LINK_HERE", link));

            response.ContentEncoding = Encoding.UTF8;
            response.ContentType = "text/html";
            response.ContentLength64 = bytes.Length;
            response.OutputStream.Write(bytes, 0, bytes.Length);

            return Task.CompletedTask;
        }

        public static async Task<byte[]> UpscaleImage(string imagePath, string crc32)
        {
            string maindir = Directory.GetCurrentDirectory() + $"/static/ImageMagick/Cache/";

            string filepath = maindir + $"/{crc32}.tmp";

            int num = 0;
            foreach (char value in crc32 + "0utput".ToLower().Replace(Path.DirectorySeparatorChar, '/')) // 0utput is a anti-collision salt, cache can get big.
            {
                num *= 37;
                num += Convert.ToInt32(value);
            }

            string outfilepath = maindir + $"/{num.ToString("X8")}.tmp";

            if (File.Exists(filepath))
                File.Delete(filepath);

            if (File.Exists(outfilepath))
            {
                if (File.GetCreationTime(outfilepath) <= DateTime.Now.AddMonths(-3)) // 3 Month of cache period.
                    File.Delete(outfilepath);
                else
                    return FileHelper.CryptoReadAsync(outfilepath, HTTPPrivateKey.HTTPPrivatekey);
            }

            byte[] indata = FileHelper.CryptoReadAsync(imagePath, HTTPPrivateKey.HTTPPrivatekey);

            if (indata != null)
            {
                Directory.CreateDirectory(maindir);

                File.WriteAllBytes(filepath, indata);
            }
            else
                return null;

            try
            {
                string arguments = $"\"{filepath}\" -antialias \"{outfilepath}\"";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Directory.GetCurrentDirectory() + $"/static/audio-video/convert",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = Directory.GetCurrentDirectory() + $"/static/", // Can load various config files.
                    CreateNoWindow = false // This is a console app.
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        byte[] upscaledImageBytes = File.ReadAllBytes(outfilepath);
                        await FileHelper.CryptoWriteAsync(outfilepath, HTTPPrivateKey.HTTPPrivatekey, File.ReadAllBytes(outfilepath), true);
                        if (File.Exists(filepath))
                            File.Delete(filepath);
                        return upscaledImageBytes;
                    }
                    else
                    {
                        ServerConfiguration.LogWarn("[HTTP] - ImageMagick conversion process failed.");
                        if (File.Exists(filepath))
                            File.Delete(filepath);
                        return indata;
                    }
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[HTTP] - ImageMagick conversion process failed - {ex}");
            }

            return null;
        }

        public static async Task<byte[]> YoutubeDLP(string videourl, string inputstring)
        {
            string maindir = Directory.GetCurrentDirectory() + $"/static/Youtube/Cache/";

            Directory.CreateDirectory(maindir);

            int num = 0;
            foreach (char value in inputstring.ToLower().Replace(Path.DirectorySeparatorChar, '/'))
            {
                num *= 37;
                num += Convert.ToInt32(value);
            }

            string outfilepath = maindir + $"/{num.ToString("X8")}.mp4";

            if (File.Exists(outfilepath))
            {
                if (File.GetCreationTime(outfilepath) <= DateTime.Now.AddMonths(-3)) // 3 Month of cache period.
                    File.Delete(outfilepath);
                else
                    return FileHelper.CryptoReadAsync(outfilepath, HTTPPrivateKey.HTTPPrivatekey);
            }

            string tempfilepath = maindir + $"/{num.ToString("X8")}.tmp";

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Directory.GetCurrentDirectory() + $"/static/audio-video/yt-dlp",
                    Arguments = $"\"{videourl}\" --no-playlist -o \"{tempfilepath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = Directory.GetCurrentDirectory() + $"/static/video/", // Can load various config files.
                    CreateNoWindow = false // This is a console app.
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        byte[] videocontent = File.ReadAllBytes(tempfilepath);
                        await FileHelper.CryptoWriteAsync(outfilepath, HTTPPrivateKey.HTTPPrivatekey, videocontent, true);
                        if (File.Exists(tempfilepath))
                            File.Delete(tempfilepath);
                        return videocontent;
                    }
                    else
                    {
                        ServerConfiguration.LogWarn("[HTTP] - yt-dlp conversion process failed");
                        if (File.Exists(tempfilepath))
                            File.Delete(tempfilepath);
                        if (File.Exists(tempfilepath + ".part"))
                            File.Delete(tempfilepath + ".part");
                    }
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[HTTP] - yt-dlp conversion process failed - {ex}");
                if (File.Exists(tempfilepath))
                    File.Delete(tempfilepath);
                if (File.Exists(tempfilepath + ".part"))
                    File.Delete(tempfilepath + ".part");
            }

            return null;
        }

        public static byte[] ProcessPHPPage(string pageFileName, string phpver, string link, HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] returndata = null;
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
                    request.InputStream.CopyTo(ms);

                    ms.Position = 0;

                    // Find the number of bytes in the stream
                    int contentLength = (int)ms.Length;

                    // Create a byte array
                    byte[] buffer = new byte[contentLength];

                    // Read the contents of the memory stream into the byte array
                    ms.Read(buffer, 0, contentLength);

                    postData = Encoding.UTF8.GetString(buffer);

                    ms.Dispose();
                }
            }

            Process proc = new Process();

            proc.StartInfo.FileName = Directory.GetCurrentDirectory() + $"{ServerConfiguration.PHPStaticFolder}{phpver}/php-cgi";

            if (Misc.IsWindows())
            {
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
            }
            else
            {
                if (postData == null)
                {
                    proc.StartInfo.Arguments = $"-q -d \"error_reporting=E_ALL\" -d \"display_errors={ServerConfiguration.PHPDebugErrors}\" -d \"expose_php=Off\" -d \"include_path='{documentRootPath}'\" " +
                    $"-d \"extension_dir='{Directory.GetCurrentDirectory() + $@"{ServerConfiguration.PHPStaticFolder}{phpver}/ext/"}'\" -d \"extension=php_bz2.so\" " +
                    $"-d \"extension=php_com_dotnet.so\" -d \"extension=php_curl.so\" -d \"extension=php_dba.so\" -d \"extension=php_dl_test.so\" -d \"extension=php_enchant.so\" " +
                    $"-d \"extension=php_exif.so\" -d \"extension=php_ffi.so\" -d \"extension=php_fileinfo.so\" -d \"extension=php_ftp.so\" -d \"extension=php_gd.so\" -d \"extension=php_gettext.so\" " +
                    $"-d \"extension=php_gmp.so\" -d \"extension=php_imap.so\" -d \"extension=php_intl.so\" -d \"extension=php_ldap.so\" -d \"extension=php_mbstring.so\" " +
                    $"-d \"extension=php_mysqli.so\" -d \"extension=php_odbc.so\" -d \"extension=php_openssl.so\" " +
                    $"-d \"extension=php_pdo_mysql.so\" -d \"extension=php_pdo_odbc.so\" -d \"extension=php_pdo_pgsql.so\" " +
                    $"-d \"extension=php_pdo_sqlite.so\" -d \"extension=php_pgsql.so\" -d \"extension=php_shmop.so\" -d \"extension=php_snmp.so\" -d \"extension=php_soap.so\" " +
                    $"-d \"extension=php_sockets.so\" -d \"extension=php_sodium.so\" -d \"extension=php_sqlite3.so\" -d \"extension=php_sysvshm.so\" -d \"extension=php_tidy.so\" " +
                    $"-d \"extension=php_xsl.so\" -d \"extension=php_zend_test.so\" -d \"extension=php_zip.so\" \"{pageFileName}\"";
                }
                else
                {
                    proc.StartInfo.Arguments = $"-q -d \"error_reporting=E_ALL\" -d \"display_errors={ServerConfiguration.PHPDebugErrors}\" -d \"expose_php=Off\" -d \"include_path='{documentRootPath}'\" " +
                    $"-d \"extension_dir='{Directory.GetCurrentDirectory() + $@"{ServerConfiguration.PHPStaticFolder}{phpver}/ext/"}'\" -d \"extension=php_bz2.so\" " +
                    $"-d \"extension=php_com_dotnet.so\" -d \"extension=php_curl.so\" -d \"extension=php_dba.so\" -d \"extension=php_dl_test.so\" -d \"extension=php_enchant.so\" " +
                    $"-d \"extension=php_exif.so\" -d \"extension=php_ffi.so\" -d \"extension=php_fileinfo.so\" -d \"extension=php_ftp.so\" -d \"extension=php_gd.so\" -d \"extension=php_gettext.so\" " +
                    $"-d \"extension=php_gmp.so\" -d \"extension=php_imap.so\" -d \"extension=php_intl.so\" -d \"extension=php_ldap.so\" -d \"extension=php_mbstring.so\" " +
                    $"-d \"extension=php_mysqli.so\" -d \"extension=php_odbc.so\" -d \"extension=php_openssl.so\" " +
                    $"-d \"extension=php_pdo_mysql.so\" -d \"extension=php_pdo_odbc.so\" -d \"extension=php_pdo_pgsql.so\" " +
                    $"-d \"extension=php_pdo_sqlite.so\" -d \"extension=php_pgsql.so\" -d \"extension=php_shmop.so\" -d \"extension=php_snmp.so\" -d \"extension=php_soap.so\" " +
                    $"-d \"extension=php_sockets.so\" -d \"extension=php_sodium.so\" -d \"extension=php_sqlite3.so\" -d \"extension=php_sysvshm.so\" -d \"extension=php_tidy.so\" " +
                    $"-d \"extension=php_xsl.so\" -d \"extension=php_zend_test.so\" -d \"extension=php_zip.so\" -d \"post_data={postData}\" \"{pageFileName}\"";
                }
            }

            proc.StartInfo.CreateNoWindow = false;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;

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

            if (postData != null)
                proc.StandardInput.WriteLine(postData);

            response.Headers.Set("Content-Type", "text/html");
            response.AddHeader("Set-Cookie", $"ANID={Guid.NewGuid()}; Path=/"); // We initiate the cookie system that way.

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
                            errorOutput = PreMadeWebPages.phperror.Replace("PUT_ERROR_HERE", errorOutput).Replace("PUT_LINK_HERE", link);
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
                                    response.AppendHeader("Set-Cookie", $"{cookiePart.Substring(0, equalIndex).Trim()}={cookiePart.Substring(equalIndex + 1).Trim()}; Path=/");
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

        public static byte[] RemoveUnwantedHeaders(byte[] phpOutputBytes)
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

        public static byte[] Compress(byte[] input)
        {
            using (MemoryStream output = new())
            {
                using (GZipStream gzipStream = new(output, CompressionMode.Compress, leaveOpen: true))
                {
                    gzipStream.Write(input, 0, input.Length);
                }

                return output.ToArray();
            }
        }

        public static byte[] Decompress(byte[] input)
        {
            using (MemoryStream inputMemoryStream = new(input))
            using (GZipStream gzipStream = new(inputMemoryStream, CompressionMode.Decompress))
            using (MemoryStream outputMemoryStream = new())
            {
                gzipStream.CopyTo(outputMemoryStream);
                return outputMemoryStream.ToArray();
            }
        }
    }
}
