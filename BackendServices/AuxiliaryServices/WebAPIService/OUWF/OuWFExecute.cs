using System.IO;
using System;
using CyberBackendLibrary.HTTP;
using CustomLogger;
using HttpMultipartParser;

namespace WebAPIService.OUWF
{
    public class OuWFExecute
    {
        public static string? Execute(byte[]? PostData, string? ContentType)
        {
            string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

            using (MemoryStream ms = new(PostData))
            {
                var multipartData = MultipartFormDataParser.Parse(ms, boundary);

                int instanceId = Convert.ToInt32(multipartData.GetParameterValue("instanceId"));
                string vers = multipartData.GetParameterValue("version");
                string path = multipartData.GetParameterValue("path");
                string data = multipartData.GetParameterValue("data");

                LoggerAccessor.LogInfo($"[OuWF] - Requested Execute with instanceId {instanceId} | version {vers} | path {path} | data {data}");

                try
                {
                    string toTrim = "lua|";
                    var filePath = data.Substring(4);
                    string fileContent = ReadFile(filePath);
                    Console.WriteLine($"File content: {fileContent}");
                    return fileContent;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading the file: {ex.Message}");
                }

                ms.Flush();

                return null;
            }
        }

        static string ReadFile(string filePath)
        {
            // Check if the file exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            // Read the content of the file
            string fileContent;
            using (StreamReader reader = new StreamReader(filePath))
            {
                fileContent = reader.ReadToEnd();
            }

            return fileContent;
        }
    }
}
