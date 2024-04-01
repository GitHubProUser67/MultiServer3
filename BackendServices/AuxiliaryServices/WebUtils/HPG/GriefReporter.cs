using CustomLogger;
using System.Text;
using BackendProject.MiscUtils;
using HttpMultipartParser;
using static Community.CsharpSqlite.Sqlite3;
using Newtonsoft.Json.Linq;

namespace WebUtils.HPG
{
    public class GriefReporter
    {
        public static string? caponeContentStoreUpload(byte[]? PostData, string? ContentType, string workPath)
        {
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            using (MemoryStream ms = new(PostData))
            {
                try
                {
                    // Read the multipart content.
                    var provider = MultipartFormDataParser.Parse(ms, boundary);

                    Directory.CreateDirectory(workPath);

                    // Process each part.
                    foreach (var part in provider.Files)
                    {
                        // Only process file parts.
                        if (part.FileName != null)
                        {
                            // Get the filename.
                            string fileName = part.FileName;

                            // Save the file with name.
                            string filePath = Path.Combine(workPath, fileName);

                            LoggerAccessor.LogInfo($"[HPG] - Writing evidence file {fileName} to {filePath}!");

                            // Save the file content.
                            using (FileStream fs = new FileStream(filePath, FileMode.Create))
                            {
                                fs.Write(Encoding.UTF8.GetBytes(Convert.ToString(part.Data)), 0, Convert.ToInt32(part.Data.Length));
                                LoggerAccessor.LogInfo($"[HPG] GriefReporter - Written evidence file {fileName} to {filePath}!");
                            }
                        }
                    }
                    LoggerAccessor.LogInfo($"[HPG] GriefReporter - GriefReport evidence receieved and written to contentStore!");

                    ms.Flush();
                    return "";
                }
                catch (Exception e)
                {
                    return $"InternalServerError with exception {e}";
                }


            }

        }

        public static string? caponeReportCollectorSubmit(byte[]? PostData, string? ContentType, string workPath)
        {
            using (MemoryStream ms = new(PostData))
            {
                string fileName = string.Empty;

                JToken Token = JToken.Parse(Encoding.UTF8.GetString(PostData));

                string? dataURL = (string?)VariousUtils.GetValueFromJToken(Token, "dataLocation");
                string pathTrim = dataURL.Substring(42).Replace("?countryCode=us", "");
                string finalPath = Path.Combine(workPath, pathTrim);

                LoggerAccessor.LogWarn($"[HPG] GriefReporter - Path check {finalPath}");

                try
                {
                    Directory.CreateDirectory(finalPath);

                    // Save the file with name.
                    string filePath = Path.Combine(finalPath, fileName);

                    LoggerAccessor.LogInfo($"[HPG] GriefReporter - Writing JSON Summary {fileName} to {filePath}!");

                    // Save the file content.
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        fs.Write(Encoding.UTF8.GetBytes(Convert.ToString(PostData)), 0, Convert.ToInt32(PostData.Length));
                        LoggerAccessor.LogInfo($"[HPG] GriefReporter - Written JSON {fileName} to {filePath}!");
                    }
                    LoggerAccessor.LogInfo($"[HPG] GriefReporter - GriefReport JSON receieved and written to contentStore!");

                    ms.Flush();
                    return "";
                }
                catch (Exception e)
                {
                    return $"InternalServerError with exception {e}";
                }


            }

        }

    }
}