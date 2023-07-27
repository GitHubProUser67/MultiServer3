using System.Diagnostics;
using System.Net;
using System.Text;

namespace PSMultiServer
{
    public class ContextProcess
    {
        public async Task Processwwwroot(HttpListenerContext context, string page, string phpver, string userAgent)
        {
            try
            {
                bool keepalive = false;

                string contentType = GetContentType(page);

                if (contentType == "video/mp4" || contentType.StartsWith("audio/"))
                {
                    try
                    {
                        if (userAgent != null && userAgent.Contains("CellOS"))
                        {
                            byte[] fileBuffer;

                            byte[] firstNineBytes = new byte[9];

                            using (FileStream fileStream = new FileStream(page, FileMode.Open, FileAccess.Read))
                            {
                                fileStream.Read(firstNineBytes, 0, 9);
                                fileStream.Close();
                            }

                            if (HTTPserver.httpkey != "" && await Task.Run(() => Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 })))
                            {
                                byte[] src = File.ReadAllBytes(page);
                                byte[] dst = new byte[src.Length - 9];

                                Array.Copy(src, 9, dst, 0, dst.Length);

                                fileBuffer = SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                            SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(HTTPserver.httpkey));
                            }
                            else
                            {
                                fileBuffer = File.ReadAllBytes(page);
                            }

                            context.Response.Headers.Add("Accept-Ranges", "bytes");
                            context.Response.Headers.Add("ETag", $"{Guid.NewGuid().ToString().Substring(0, 4)}-{Guid.NewGuid().ToString().Substring(0, 12)}");

                            if (context.Response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    context.Response.StatusCode = 200;
                                    context.Response.ContentLength64 = fileBuffer.Length;
                                    context.Response.OutputStream.Write(fileBuffer, 0, fileBuffer.Length);
                                    context.Response.OutputStream.Close();
                                }
                                catch (Exception ex1)
                                {
                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Client Disconnected early");
                            }
                        }
                        else
                        {
                            context.Response.ContentType = contentType;

                            if (context.Request.Headers["Accept"] != null)
                            {
                                if (context.Request.Headers["Accept"].Contains("text/html")) // Chrome does that...
                                {
                                    // Generate an HTML page with the video element
                                    string html = @"
                                        <!DOCTYPE html>
                                        <html>
                                        <head>
                                            <title>Media Page</title>
                                        </head>
                                        <body>
                                            <video controls>
                                                <source src=" + "\"" + context.Request.Url.AbsolutePath + "\"" + $@" type=""{contentType}"">
                                            </video>
                                        </body>
                                        </html>";

                                    // Write the HTML content to the response
                                    byte[] buffer = Encoding.UTF8.GetBytes(html);

                                    if (context.Response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            // Set the response headers for the HTML content
                                            context.Response.ContentType = "text/html";
                                            context.Response.ContentEncoding = Encoding.UTF8;
                                            context.Response.StatusCode = 200;
                                            context.Response.ContentLength64 = buffer.Length;
                                            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                            context.Response.OutputStream.Close();
                                        }
                                        catch (Exception ex1)
                                        {
                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Client Disconnected early");
                                    }
                                }
                                else
                                {
                                    if (context.Response.OutputStream.CanWrite)
                                    {
                                        try
                                        {
                                            keepalive = true;

                                            context.Response.StatusCode = 200;

                                            // Set the Keep-Alive header in the response
                                            context.Response.KeepAlive = true;

                                            // Set the Chunked header in the response
                                            context.Response.SendChunked = true;

                                            byte[] firstNineBytes = new byte[9];

                                            using (FileStream fileStream = new FileStream(page, FileMode.Open, FileAccess.Read))
                                            {
                                                fileStream.Read(firstNineBytes, 0, 9);
                                                fileStream.Close();
                                            }

                                            if (HTTPserver.httpkey != "" && await Task.Run(() => Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 })))
                                            {
                                                byte[] src = File.ReadAllBytes(page);
                                                byte[] dst = new byte[src.Length - 9];

                                                Array.Copy(src, 9, dst, 0, dst.Length);

                                                byte[] fileBytes = SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                                            SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(HTTPserver.httpkey));

                                                context.Response.ContentLength64 = fileBytes.Length;

                                                using (MemoryStream memoryStream = new MemoryStream(fileBytes))
                                                {
                                                    byte[] buffer = new byte[context.Response.ContentLength64];
                                                    int bytesRead;

                                                    while ((bytesRead = memoryStream.Read(buffer, 0, buffer.Length)) > 0)
                                                    {
                                                        context.Response.OutputStream.Write(buffer, 0, bytesRead);
                                                        context.Response.OutputStream.Flush();
                                                    }

                                                    memoryStream.Close();
                                                }
                                            }
                                            else
                                            {
                                                context.Response.ContentLength64 = new FileInfo(page).Length;

                                                // Open the file and send it in chunks
                                                using (FileStream fileStream = new FileStream(page, FileMode.Open, FileAccess.Read))
                                                {
                                                    byte[] buffer = new byte[context.Response.ContentLength64];
                                                    int bytesRead;

                                                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                                                    {
                                                        context.Response.OutputStream.Write(buffer, 0, bytesRead);
                                                        context.Response.OutputStream.Flush();
                                                    }

                                                    fileStream.Close();
                                                }
                                            }

                                            context.Response.OutputStream.Close();
                                        }
                                        catch (Exception ex1)
                                        {
                                            Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Client Disconnected early");
                                    }
                                }
                            }
                            else
                            {
                                if (context.Response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        keepalive = true;

                                        context.Response.StatusCode = 200;

                                        // Set the Keep-Alive header in the response
                                        context.Response.KeepAlive = true;

                                        // Set the Chunked header in the response
                                        context.Response.SendChunked = true;

                                        byte[] firstNineBytes = new byte[9];

                                        using (FileStream fileStream = new FileStream(page, FileMode.Open, FileAccess.Read))
                                        {
                                            fileStream.Read(firstNineBytes, 0, 9);
                                            fileStream.Close();
                                        }

                                        if (HTTPserver.httpkey != "" && await Task.Run(() => Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 })))
                                        {
                                            byte[] src = File.ReadAllBytes(page);
                                            byte[] dst = new byte[src.Length - 9];

                                            Array.Copy(src, 9, dst, 0, dst.Length);

                                            byte[] fileBytes = SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                                        SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(HTTPserver.httpkey));

                                            context.Response.ContentLength64 = fileBytes.Length;

                                            using (MemoryStream memoryStream = new MemoryStream(fileBytes))
                                            {
                                                byte[] buffer = new byte[context.Response.ContentLength64];
                                                int bytesRead;

                                                while ((bytesRead = memoryStream.Read(buffer, 0, buffer.Length)) > 0)
                                                {
                                                    context.Response.OutputStream.Write(buffer, 0, bytesRead);
                                                    context.Response.OutputStream.Flush();
                                                }

                                                memoryStream.Close();
                                            }
                                        }
                                        else
                                        {
                                            context.Response.ContentLength64 = new FileInfo(page).Length;

                                            // Open the file and send it in chunks
                                            using (FileStream fileStream = new FileStream(page, FileMode.Open, FileAccess.Read))
                                            {
                                                byte[] buffer = new byte[context.Response.ContentLength64];
                                                int bytesRead;

                                                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                                                {
                                                    context.Response.OutputStream.Write(buffer, 0, bytesRead);
                                                    context.Response.OutputStream.Flush();
                                                }

                                                fileStream.Close();
                                            }
                                        }

                                        context.Response.OutputStream.Close();
                                    }
                                    catch (Exception ex1)
                                    {
                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Client Disconnected early");
                                }
                            }
                        }
                    }
                    catch (HttpListenerException ex) when (ex.HResult == -2147467259) // We EXPLODE httplistener limitations already, stream response is not-supported.
                    {
                        Console.WriteLine($"{userAgent} has sent a FIN response or called the request after stream has ended. Finish request.");

                        context.Response.OutputStream.Close();

                        keepalive = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Client Disconnected early or an error occured : {ex}");

                        // Return an internal server error response
                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                        if (context.Response.OutputStream.CanWrite)
                        {
                            try
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.ContentLength64 = InternnalError.Length;
                                context.Response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                context.Response.OutputStream.Close();
                            }
                            catch (Exception ex1)
                            {
                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Client Disconnected early");
                        }
                    }
                }
                else if (contentType.StartsWith("home/") || contentType.StartsWith("ps3/"))
                {
                    try
                    {
                        byte[] fileBuffer;

                        byte[] firstNineBytes = new byte[9];

                        using (FileStream fileStream = new FileStream(page, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.Read(firstNineBytes, 0, 9);
                            fileStream.Close();
                        }

                        if (HTTPserver.httpkey != "" && await Task.Run(() => Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 })))
                        {
                            byte[] src = File.ReadAllBytes(page);
                            byte[] dst = new byte[src.Length - 9];

                            Array.Copy(src, 9, dst, 0, dst.Length);

                            fileBuffer = SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                        SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(HTTPserver.httpkey));
                        }
                        else
                        {
                            fileBuffer = File.ReadAllBytes(page);
                        }

                        if (userAgent != null && userAgent.Contains("CellOS"))
                        {
                            context.Response.Headers.Add("Accept-Ranges", "bytes");
                            context.Response.Headers.Add("ETag", $"{Guid.NewGuid().ToString().Substring(0, 4)}-{Guid.NewGuid().ToString().Substring(0, 12)}");
                        }
                        else
                        {
                            context.Response.ContentType = contentType;
                        }

                        if (context.Response.OutputStream.CanWrite)
                        {
                            try
                            {
                                context.Response.StatusCode = 200;
                                context.Response.ContentLength64 = fileBuffer.Length;
                                context.Response.OutputStream.Write(fileBuffer, 0, fileBuffer.Length);
                                context.Response.OutputStream.Close();
                            }
                            catch (Exception ex1)
                            {
                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Client Disconnected early");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Client Disconnected early or an error occured : {ex}");

                        // Return an internal server error response
                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                        if (context.Response.OutputStream.CanWrite)
                        {
                            try
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.ContentLength64 = InternnalError.Length;
                                context.Response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                context.Response.OutputStream.Close();
                            }
                            catch (Exception ex1)
                            {
                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Client Disconnected early");
                        }
                    }
                }
                else if (contentType == "text/php")
                {
                    try
                    {
                        if (!Directory.Exists(Directory.GetCurrentDirectory() + "/PHP"))
                        {
                            Console.WriteLine($"HTTP - Client : {userAgent} requested a PHP file, but PHP is not present so we return nothing.");

                            // Return an internal server error response
                            byte[] PHPError = Encoding.UTF8.GetBytes(PreMadeWebPages.phpnotenabled);

                            if (context.Response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                    context.Response.ContentLength64 = PHPError.Length;
                                    context.Response.OutputStream.Write(PHPError, 0, PHPError.Length);
                                    context.Response.OutputStream.Close();
                                }
                                catch (Exception ex1)
                                {
                                    Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Client Disconnected early");
                            }
                        }
                        else
                        {
                            byte[] fileBuffer;

                            byte[] firstNineBytes = new byte[9];

                            using (FileStream fileStream = new FileStream(page, FileMode.Open, FileAccess.Read))
                            {
                                fileStream.Read(firstNineBytes, 0, 9);
                                fileStream.Close();
                            }

                            if (HTTPserver.httpkey != "" && await Task.Run(() => Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 })))
                            {
                                byte[] src = File.ReadAllBytes(page);
                                byte[] dst = new byte[src.Length - 9];

                                Array.Copy(src, 9, dst, 0, dst.Length);

                                fileBuffer = SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                            SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(HTTPserver.httpkey));
                            }
                            else
                            {
                                fileBuffer = File.ReadAllBytes(page);
                            }

                            if (await Task.Run(() => Misc.FindbyteSequence(fileBuffer, new byte[] { 0x3c, 0x3f, 0x70, 0x68, 0x70 })))
                            {
                                fileBuffer = Encoding.UTF8.GetBytes(await Task.Run(() => ProcessPhpPage(Directory.GetCurrentDirectory() + "/wwwroot" + context.Request.Url.AbsolutePath, phpver, context)));

                                if (userAgent != null && userAgent.Contains("CellOS"))
                                {
                                    context.Response.Headers.Add("Accept-Ranges", "bytes");
                                    context.Response.Headers.Add("ETag", $"{Guid.NewGuid().ToString().Substring(0, 4)}-{Guid.NewGuid().ToString().Substring(0, 12)}");
                                }
                                else
                                {
                                    context.Response.ContentType = "text/html";
                                }

                                if (context.Response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        context.Response.StatusCode = 200;
                                        context.Response.ContentLength64 = fileBuffer.Length;
                                        context.Response.OutputStream.Write(fileBuffer, 0, fileBuffer.Length);
                                        context.Response.OutputStream.Close();
                                    }
                                    catch (Exception ex1)
                                    {
                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Client Disconnected early");
                                }
                            }
                            else
                            {
                                Console.WriteLine("HTTP - An error occured when trying to find PHP pathern, or no PHP pathern at all. Closing...");

                                // Return an internal server error response
                                byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                                if (context.Response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        context.Response.ContentLength64 = InternnalError.Length;
                                        context.Response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                        context.Response.OutputStream.Close();
                                    }
                                    catch (Exception ex1)
                                    {
                                        Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Client Disconnected early");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Client Disconnected early or an error occured : {ex}");

                        // Return an internal server error response
                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                        if (context.Response.OutputStream.CanWrite)
                        {
                            try
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.ContentLength64 = InternnalError.Length;
                                context.Response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                context.Response.OutputStream.Close();
                            }
                            catch (Exception ex1)
                            {
                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Client Disconnected early");
                        }
                    }
                }
                else
                {
                    try
                    {
                        byte[] fileBuffer;

                        byte[] firstNineBytes = new byte[9];

                        using (FileStream fileStream = new FileStream(page, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.Read(firstNineBytes, 0, 9);
                            fileStream.Close();
                        }

                        if (HTTPserver.httpkey != "" && await Task.Run(() => Misc.FindbyteSequence(firstNineBytes, new byte[] { 0x74, 0x72, 0x69, 0x70, 0x6c, 0x65, 0x64, 0x65, 0x73 })))
                        {
                            byte[] src = File.ReadAllBytes(page);
                            byte[] dst = new byte[src.Length - 9];

                            Array.Copy(src, 9, dst, 0, dst.Length);

                            fileBuffer = SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.DecryptData(dst,
                                        SRC_Addons.CRYPTOSPORIDIUM.TRIPLEDES.GetEncryptionKey(HTTPserver.httpkey));
                        }
                        else
                        {
                            fileBuffer = File.ReadAllBytes(page);
                        }

                        if (userAgent != null && userAgent.Contains("CellOS"))
                        {
                            context.Response.Headers.Add("Accept-Ranges", "bytes");
                            context.Response.Headers.Add("ETag", $"{Guid.NewGuid().ToString().Substring(0, 4)}-{Guid.NewGuid().ToString().Substring(0, 12)}");
                        }
                        else
                        {
                            context.Response.ContentType = contentType;
                        }

                        if (context.Response.OutputStream.CanWrite)
                        {
                            try
                            {
                                context.Response.StatusCode = 200;
                                context.Response.ContentLength64 = fileBuffer.Length;
                                context.Response.OutputStream.Write(fileBuffer, 0, fileBuffer.Length);
                                context.Response.OutputStream.Close();
                            }
                            catch (Exception ex1)
                            {
                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Client Disconnected early");
                        }
                    }
                    catch (HttpListenerException ex) when (ex.HResult == -2147467259) // We EXPLODE httplistener limitations already, stream response is not-supported.
                    {
                        Console.WriteLine($"{userAgent} has sent a FIN response or called the request after stream has ended. Finish request.");

                        context.Response.OutputStream.Close();

                        keepalive = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Client Disconnected early or an error occured : {ex}");

                        // Return an internal server error response
                        byte[] InternnalError = Encoding.UTF8.GetBytes("An Error as occured, please retry.");

                        if (context.Response.OutputStream.CanWrite)
                        {
                            try
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                context.Response.ContentLength64 = InternnalError.Length;
                                context.Response.OutputStream.Write(InternnalError, 0, InternnalError.Length);
                                context.Response.OutputStream.Close();
                            }
                            catch (Exception ex1)
                            {
                                Console.WriteLine($"Client Disconnected early and thrown an exception {ex1}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Client Disconnected early");
                        }
                    }
                }

                if (!keepalive)
                {
                    context.Response.Close();
                }

                GC.Collect();

                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured while processing the file : {page} - {ex}");

                context.Response.Close();
                GC.Collect();

                return;
            }
        }
        public static async Task<string> ProcessPhpPage(string pageFileName, string phpver, HttpListenerContext context)
        {
            var index = context.Request.RawUrl.IndexOf("?");
            var queryString = index == -1 ? "" : context.Request.RawUrl.Substring(index + 1);

            // Get paths for PHP
            var documentRootPath = Directory.GetCurrentDirectory() + "/wwwroot/";

            var scriptFilePath = Path.GetFullPath(pageFileName);
            var scriptFileName = Path.GetFileName(pageFileName);
            var tempPath = Path.GetTempPath();

            Process proc = new Process();

            if (Misc.IsWindows())
            {
                proc.StartInfo.FileName = Directory.GetCurrentDirectory() + $"PHP/{phpver}threadsafe/php.exe";
            }
            else
            {
                proc.StartInfo.FileName = Directory.GetCurrentDirectory() + $"PHP/{phpver}threadsafe/php";
            }

            proc.StartInfo.Arguments = $"-d \"display_errors=1\" -d \"error_reporting=E_ALL\" -d \"display_errors=true\" -d \"include_path='{documentRootPath}'\" -d \"extension_dir='{Directory.GetCurrentDirectory() + $@"PHP/{phpver}threadsafe/ext/"}'\" -d \"extension=php_mysql.dll\" -d \"extension=php_mysqli.dll\" -d \"extension=php_pdo_mysql.dll\" \"" + pageFileName + "\"";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            proc.StartInfo.EnvironmentVariables.Clear();

            // Set environment variables for PHP
            proc.StartInfo.EnvironmentVariables.Add("GATEWAY_INTERFACE", "CGI/1.1");
            proc.StartInfo.EnvironmentVariables.Add("SERVER_PROTOCOL", "HTTP/1.1");
            proc.StartInfo.EnvironmentVariables.Add("REDIRECT_STATUS", "200");
            proc.StartInfo.EnvironmentVariables.Add("DOCUMENT_ROOT", documentRootPath);
            proc.StartInfo.EnvironmentVariables.Add("SCRIPT_NAME", scriptFileName);
            proc.StartInfo.EnvironmentVariables.Add("SCRIPT_FILENAME", scriptFilePath);
            proc.StartInfo.EnvironmentVariables.Add("QUERY_STRING", queryString);
            proc.StartInfo.EnvironmentVariables.Add("CONTENT_LENGTH", context.Request.ContentLength64.ToString());
            proc.StartInfo.EnvironmentVariables.Add("CONTENT_TYPE", context.Request.ContentType);
            proc.StartInfo.EnvironmentVariables.Add("REQUEST_METHOD", context.Request.HttpMethod);
            proc.StartInfo.EnvironmentVariables.Add("USER_AGENT", context.Request.UserAgent);
            proc.StartInfo.EnvironmentVariables.Add("SERVER_ADDR", context.Request.LocalEndPoint.Address.ToString());
            proc.StartInfo.EnvironmentVariables.Add("REMOTE_ADDR", context.Request.RemoteEndPoint.Address.ToString());
            proc.StartInfo.EnvironmentVariables.Add("REMOTE_PORT", context.Request.RemoteEndPoint.Port.ToString());
            proc.StartInfo.EnvironmentVariables.Add("REFERER", context.Request.UrlReferrer?.ToString() ?? "");
            proc.StartInfo.EnvironmentVariables.Add("REQUEST_URI", context.Request.Url.AbsoluteUri);
            proc.StartInfo.EnvironmentVariables.Add("HTTP_COOKIE", context.Request.Headers["Cookie"]);
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT", context.Request.Headers["Accept"]);
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_CHARSET", context.Request.Headers["Accept-Charset"]);
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_ENCODING", context.Request.Headers["Accept-Encoding"]);
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_LANGUAGE", context.Request.Headers["Accept-Language"]);
            proc.StartInfo.EnvironmentVariables.Add("TMPDIR", tempPath);
            proc.StartInfo.EnvironmentVariables.Add("TEMP", tempPath);

            proc.Start();

            string res = proc.StandardOutput.ReadToEnd();

            if (string.IsNullOrEmpty(res))
            {
                res = proc.StandardError.ReadToEnd();
                res = "<h2 style=\"color:red;\">Error!</h2><hr/> <h4>Error Details :</h4> <pre>" + res + "</pre>";
                proc.StandardError.Close();
            }
            if (res.StartsWith("\nParse error: syntax error"))
                res = "<h2 style=\"color:red;\">Error!</h2><hr/> <h4>Error Details :</h4> <pre>" + res + "</pre>";

            proc.StandardOutput.Close();
            proc.WaitForExit(); // Wait for the PHP process to complete

            proc.Close();

            return res;
        }
        public static string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName)?.ToLower();
            string contentType;

            switch (extension)
            {
                case ".png":
                    contentType = "image/png";
                    break;
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                case ".gif":
                    contentType = "image/gif";
                    break;
                case ".bmp":
                    contentType = "image/bmp";
                    break;
                case ".ico":
                    contentType = "image/x-icon";
                    break;
                case ".svg":
                    contentType = "image/svg+xml";
                    break;
                case ".xml":
                    contentType = "application/xml";
                    break;
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".doc":
                    contentType = "application/msword";
                    break;
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".xls":
                    contentType = "application/vnd.ms-excel";
                    break;
                case ".xlsx":
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                case ".ppt":
                    contentType = "application/vnd.ms-powerpoint";
                    break;
                case ".pptx":
                    contentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    break;
                case ".txt":
                    contentType = "text/plain";
                    break;
                case ".csv":
                    contentType = "text/csv";
                    break;
                case ".css":
                    contentType = "text/css";
                    break;
                case ".js":
                    contentType = "text/javascript";
                    break;
                case ".html":
                case ".htm":
                    contentType = "text/html";
                    break;
                case ".php":
                    contentType = "text/php";
                    break;
                case ".zip":
                    contentType = "application/zip";
                    break;
                case ".rar":
                    contentType = "application/x-rar-compressed";
                    break;
                case ".7z":
                    contentType = "application/x-7z-compressed";
                    break;
                case ".sdat":
                    contentType = "ps3/archive";
                    break;
                case ".bar":
                    contentType = "home/archive";
                    break;
                case ".sharc":
                    contentType = "home/archive";
                    break;
                case ".sdc":
                    contentType = "home/metadata";
                    break;
                case ".odc":
                    contentType = "home/metadata";
                    break;
                case ".mp4":
                    contentType = "video/mp4";
                    break;
                case ".avi":
                    contentType = "video/x-msvideo";
                    break;
                case ".mov":
                    contentType = "video/quicktime";
                    break;
                case ".wmv":
                    contentType = "video/x-ms-wmv";
                    break;
                case ".mkv":
                    contentType = "video/x-matroska";
                    break;
                case ".mp3":
                    contentType = "audio/mpeg";
                    break;
                case ".wav":
                    contentType = "audio/wav";
                    break;
                case ".ogg":
                    contentType = "audio/ogg";
                    break;
                case ".flac":
                    contentType = "audio/flac";
                    break;
                default:
                    contentType = "application/octet-stream";
                    break;
            }

            return contentType;
        }
    }
}
