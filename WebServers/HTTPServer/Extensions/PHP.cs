using NetworkLibrary.Extension;
using HTTPServer.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;

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
            proc.StartInfo.EnvironmentVariables.Add("SERVER_PROTOCOL", $"HTTP/{HTTPServerConfiguration.HttpVersion}");
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
            foreach (var headerKeyPair in ConvertHeadersToPhpFriendly(request.Headers))
            {
                proc.StartInfo.EnvironmentVariables.Add(headerKeyPair.Key, headerKeyPair.Value);
            }
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

                        HeadersLocal = HeadersLocal.AddArray(new string[] { line[..index], line[(index + 2)..] });
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

        private static List<KeyValuePair<string, string>> ConvertHeadersToPhpFriendly(List<KeyValuePair<string, string>>? headers)
        {
            List<KeyValuePair<string, string>> phpFriendlyHeaders = new List<KeyValuePair<string, string>>();

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    // Convert header name to uppercase, replace dashes with underscores, and prefix with "HTTP_"
                    string phpHeaderName = "HTTP_" + header.Key.ToUpper().Replace("-", "_");

                    // Add the transformed header name and its value to the list
                    phpFriendlyHeaders.Add(new KeyValuePair<string, string>(phpHeaderName, header.Value));
                }
            }

            return phpFriendlyHeaders;
        }
    }
}
