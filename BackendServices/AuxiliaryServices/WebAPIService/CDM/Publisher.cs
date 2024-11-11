using CustomLogger;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIService.CDM
{
    public class Publisher
    {
        public static string handlePublisherList(byte[] PostData, string ContentType, string workpath, string absolutePath)
        {
            string pubListPath = $"{workpath}/CDM/Publishers";

            Directory.CreateDirectory(pubListPath);
            string filePath = $"{pubListPath}/list.xml";
            if (File.Exists(filePath))
            {
                LoggerAccessor.LogInfo($"[CDM] - Publisher List found and sent!");
                string res = File.ReadAllText(filePath);

                string resourceXML = "<xml>\r\n\t" +
                    "<status>success</status>\r\n" +
                    $"{res}\r\n" +
                    "</xml>";

                return resourceXML;
            }
            else
            {
                LoggerAccessor.LogError($"[CDM] - Failed to find publisher list with expected path {filePath}!");
            }

            return "<xml><status>fail</status></xml>";
        }

    }
}