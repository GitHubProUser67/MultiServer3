using CyberBackendLibrary.Extension;
using HTTPServer.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HTTPServer.Extensions
{
    public class PHP
    {
        public static (byte[]?, string[][]) ProcessPHPPage(string FilePath, string phppath, string phpver, HttpRequest request)
        {
            int index = request.RawUrlWithQuery!.IndexOf("?");
            string? queryString = index == -1 ? string.Empty : request.RawUrlWithQuery[(index + 1)..];

            // Get paths for PHP
            string? documentRootPath = Path.GetDirectoryName(FilePath);
            string? scriptFilePath = Path.GetFullPath(FilePath);
            string? scriptFileName = Path.GetFileName(FilePath);
            string? tempPath = Path.GetTempPath();

            string[][] HeadersLocal = Array.Empty<string[]>();
            byte[]? returndata = null;
            byte[]? postData = null;

            // Extract POST data (if available)
            if (request.Method == "POST" && request.GetDataStream != null)
            {
                using MemoryStream ms = new();
                request.GetDataStream.CopyTo(ms);
                postData = ms.ToArray();
                ms.Flush();
            }

            using Process proc = new();

            proc.StartInfo.FileName = $"{phppath}/{phpver}/php-cgi";

            proc.StartInfo.Arguments = $"-q -d \"error_reporting=E_ALL\" -d \"display_errors={HTTPServerConfiguration.PHPDebugErrors}\" -d \"expose_php=Off\" -d \"include_path='{documentRootPath}'\" " +
                         $"-d \"extension_dir='{$@"{phppath}/{phpver}/ext/"}'\" \"{FilePath}\"";

            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardInput = true;

            proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            proc.StartInfo.EnvironmentVariables.Clear();

            // Set content length for POST data
            if (postData != null)
                proc.StartInfo.EnvironmentVariables.Add("CONTENT_LENGTH", postData.Length.ToString());

            // Set environment variables for PHP
            proc.StartInfo.EnvironmentVariables.Add("GATEWAY_INTERFACE", "CGI/1.1");
            proc.StartInfo.EnvironmentVariables.Add("SERVER_PROTOCOL", "HTTP/1.1");
            proc.StartInfo.EnvironmentVariables.Add("REDIRECT_STATUS", "200");
            proc.StartInfo.EnvironmentVariables.Add("DOCUMENT_ROOT", documentRootPath);
            proc.StartInfo.EnvironmentVariables.Add("SCRIPT_NAME", scriptFileName);
            proc.StartInfo.EnvironmentVariables.Add("SCRIPT_FILENAME", scriptFilePath);
            proc.StartInfo.EnvironmentVariables.Add("QUERY_STRING", queryString);
            proc.StartInfo.EnvironmentVariables.Add("CONTENT_TYPE", request.GetContentType());
            proc.StartInfo.EnvironmentVariables.Add("REQUEST_METHOD", request.Method);
            proc.StartInfo.EnvironmentVariables.Add("USER_AGENT", request.RetrieveHeaderValue("User-Agent"));
            proc.StartInfo.EnvironmentVariables.Add("SERVER_ADDR", request.ServerIP.ToString());
            proc.StartInfo.EnvironmentVariables.Add("REMOTE_ADDR", request.IP);
            proc.StartInfo.EnvironmentVariables.Add("REMOTE_PORT", request.Port);
            proc.StartInfo.EnvironmentVariables.Add("REFERER", request.RetrieveHeaderValue("Referer"));
            proc.StartInfo.EnvironmentVariables.Add("REQUEST_URI", $"http://{request.ServerIP}:{request.ServerPort}{request.RawUrlWithQuery}");
            proc.StartInfo.EnvironmentVariables.Add("HTTP_COOKIE", request.RetrieveHeaderValue("Cookie"));
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT", request.RetrieveHeaderValue("Accept"));
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_CHARSET", request.RetrieveHeaderValue("Accept-Charset"));
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_ENCODING", request.RetrieveHeaderValue("Accept-Encoding"));
            proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_LANGUAGE", request.RetrieveHeaderValue("Accept-Language"));
            proc.StartInfo.EnvironmentVariables.Add("TMPDIR", tempPath);
            proc.StartInfo.EnvironmentVariables.Add("TEMP", tempPath);

            proc.Start();

            if (postData != null)
            {
                // Write request body to standard input, for POST data
                using StreamWriter? sw = proc.StandardInput;
                sw.BaseStream.Write(postData, 0, postData.Length);
            }

            // Write headers and content to response stream
            bool headersEnd = false;
            using (MemoryStream ms = new())
            using (StreamReader sr = proc.StandardOutput)
            using (StreamWriter output = new(ms))
            {
                int i = 0;
                string? line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!headersEnd)
                    {
                        if (line == string.Empty)
                        {
                            headersEnd = true;
                            continue;
                        }

                        // The first few lines are the headers, with a
                        // key and a value. Catch those, to write them
                        // into our response headers.
                        index = line.IndexOf(':');

                        HeadersLocal = OtherExtensions.AddElement(HeadersLocal, new string[] { line[..index], line[(index + 2)..] });
                    }
                    else
                        // Write non-header lines into the output as is.
                        output.WriteLine(line);

                    i++;
                }

                output.Flush();
                returndata = ms.ToArray();
            }

            proc.WaitForExit(); // Wait for the PHP process to complete

            return (returndata, HeadersLocal);
        }
    }
}
