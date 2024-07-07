using CyberBackendLibrary.HTTP;
using WatsonWebserver.Core;
using System.Diagnostics;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using CyberBackendLibrary.DataTypes;

namespace HTTPSecureServerLite.Extensions
{
    public class PHP
    {
        public static (byte[]?, string[][]) ProcessPHPPage(string FilePath, string phppath, string phpver, string ip, string port, HttpContextBase ctx)
        {
            if (!string.IsNullOrEmpty(ctx.Request.Url.RawWithQuery) && !string.IsNullOrEmpty(port))
            {
                int index = ctx.Request.Url.RawWithQuery.IndexOf("?");
                string? queryString = index == -1 ? string.Empty : ctx.Request.Url.RawWithQuery[(index + 1)..];

                // Get paths for PHP
                string? documentRootPath = Path.GetDirectoryName(FilePath);
                string? scriptFilePath = Path.GetFullPath(FilePath);
                string? scriptFileName = Path.GetFileName(FilePath);
                string? tempPath = Path.GetTempPath();

                string[][] HeadersLocal = Array.Empty<string[]>();
                byte[]? returndata = null;
                byte[]? postData = null;

                // Extract POST data (if available)
                if (ctx.Request.Method.ToString() == "POST")
                    postData = ctx.Request.DataAsBytes;

                Process proc = new();

                proc.StartInfo.FileName = $"{phppath}/{phpver}/php-cgi";

                proc.StartInfo.Arguments = $"-q -d \"error_reporting=E_ALL\" -d \"display_errors={HTTPSServerConfiguration.PHPDebugErrors}\" -d \"expose_php=Off\" -d \"include_path='{documentRootPath}'\" " +
                             $"-d \"extension_dir='{$@"{phppath}/{phpver}/ext/"}'\" \"{FilePath}\"";

                proc.StartInfo.CreateNoWindow = false;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardInput = true;

                proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;

                proc.StartInfo.EnvironmentVariables.Clear();

                // Set environment variables for PHP

                // Set content length for POST data
                if (postData != null)
                    proc.StartInfo.EnvironmentVariables.Add("CONTENT_LENGTH", postData.Length.ToString());

                proc.StartInfo.EnvironmentVariables.Add("HTTPS", "on");
                proc.StartInfo.EnvironmentVariables.Add("GATEWAY_INTERFACE", "CGI/1.1");
                proc.StartInfo.EnvironmentVariables.Add("SERVER_PROTOCOL", "HTTP/1.1");
                proc.StartInfo.EnvironmentVariables.Add("REDIRECT_STATUS", "200");
                proc.StartInfo.EnvironmentVariables.Add("DOCUMENT_ROOT", documentRootPath);
                proc.StartInfo.EnvironmentVariables.Add("SCRIPT_NAME", scriptFileName);
                proc.StartInfo.EnvironmentVariables.Add("SCRIPT_FILENAME", scriptFilePath);
                proc.StartInfo.EnvironmentVariables.Add("QUERY_STRING", queryString);
                proc.StartInfo.EnvironmentVariables.Add("CONTENT_TYPE", ctx.Request.ContentType);
                proc.StartInfo.EnvironmentVariables.Add("REQUEST_METHOD", ctx.Request.Method.ToString());
                proc.StartInfo.EnvironmentVariables.Add("USER_AGENT", ctx.Request.Useragent);
                proc.StartInfo.EnvironmentVariables.Add("SERVER_ADDR", ctx.Request.Destination.IpAddress);
                proc.StartInfo.EnvironmentVariables.Add("SERVER_PORT", ctx.Request.Destination.Port.ToString());
                proc.StartInfo.EnvironmentVariables.Add("REMOTE_ADDR", ip);
                proc.StartInfo.EnvironmentVariables.Add("REMOTE_HOST", ip);
                proc.StartInfo.EnvironmentVariables.Add("REMOTE_PORT", port);
                proc.StartInfo.EnvironmentVariables.Add("REFERER", ctx.Request.RetrieveHeaderValue("Referer"));
                proc.StartInfo.EnvironmentVariables.Add("REQUEST_URI", $"http://{ip}:{port}{ctx.Request.Url.RawWithQuery}");
                proc.StartInfo.EnvironmentVariables.Add("HTTP_COOKIE", ctx.Request.RetrieveHeaderValue("Cookie"));
                proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT", ctx.Request.RetrieveHeaderValue("Accept"));
                proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_CHARSET", ctx.Request.RetrieveHeaderValue("Accept-Charset"));
                proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_ENCODING", ctx.Request.RetrieveHeaderValue("Accept-Encoding"));
                proc.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_LANGUAGE", ctx.Request.RetrieveHeaderValue("Accept-Language"));
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

                            HeadersLocal = DataTypesUtils.AddElement(HeadersLocal, new string[] { line[..index], line[(index + 2)..] });
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
                proc.Close();
                proc.Dispose();

                return (returndata, HeadersLocal);
            }

            return (null, Array.Empty<string[]>());
        }
    }
}
