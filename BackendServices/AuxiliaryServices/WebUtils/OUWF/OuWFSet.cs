using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebUtils.OUWF
{
    public class OuWFSet
    {
        public static string? Set(byte[]? PostData, string? ContentType)
        {
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            using (MemoryStream ms = new(PostData))
            {
                var multipartData = MultipartFormDataParser.Parse(ms, boundary);

                int instanceId = Convert.ToInt32(multipartData.GetParameterValue("instanceId"));
                string vers = multipartData.GetParameterValue("version");
                string path = multipartData.GetParameterValue("path");
                string data = multipartData.GetParameterValue("data");

                LoggerAccessor.LogInfo($"[OuWF] - Requested Set with instanceId {instanceId} | version {vers} | path {path} | data \n{data}");


                /*
                // Check if the directory exists, if not, create it
                if (!Directory.Exists(Path.GetPathRoot(path)))
                {
                    Directory.CreateDirectory(Path.GetPathRoot(path));
                }
                */
                // Create the file (this will also overwrite if the file already exists)
                using (FileStream fs = File.Create(path))
                {
                    LoggerAccessor.LogInfo("File created successfully!");
                    fs.Write(Encoding.UTF8.GetBytes(data));
                    fs.Close();


                    // Perform additional operations with the FileStream if needed
                }


                ms.Flush();

                return data;
            }
        }

        
    }
}
