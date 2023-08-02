using System.Diagnostics;
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

        public static async Task<string> ProcessPhpPage(string pageFileName, string phpver, HttpListenerRequest request)
        {
            var index = request.RawUrl.IndexOf("?");
            var queryString = index == -1 ? "" : request.RawUrl.Substring(index + 1);

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
            proc.StartInfo.EnvironmentVariables.Add("CONTENT_LENGTH", request.ContentLength64.ToString());
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

        public static async Task File(this HttpListenerResponse response, string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return;
            }

            await FileHelper.ReadAsync(filePath, async stream =>
            {
                response.ContentType = MimeTypes.GetMimeType(filePath);
                response.ContentLength64 = stream.Length;
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
