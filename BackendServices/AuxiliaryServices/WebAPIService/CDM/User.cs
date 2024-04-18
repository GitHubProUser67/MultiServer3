using CustomLogger;
using CyberBackendLibrary.HTTP;
using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIService.CDM
{
    internal class User
    {
        public static string? handleGame(byte[] PostData, string ContentType, string workpath, string absolutePath)
        {
            string pubListPath = $"{workpath}/CDM/User";

            Directory.CreateDirectory(pubListPath);
            string filePath = $"{pubListPath}/game.xml";
            if (File.Exists(filePath))
            {
                LoggerAccessor.LogInfo($"[CDM] User Game - found and sent!");
                string res = File.ReadAllText(filePath);

                string resourceXML = "<xml>\r\n\t" +
                    "<status>success</status>\r\n" +
                    $"{res}\r\n" +
                    "</xml>";

                return resourceXML;
            }
            else
            {
                LoggerAccessor.LogError($"[CDM] - Failed to find publisher game with expected path {filePath}!");

                return "<xml>" +
                    "<status>fail</status>" +
                    "</xml>";
            }

        }

        public static string? handleUserSync(byte[] PostData, string ContentType, string workpath, string absolutePath)
        {
            string status;
            string userSync = string.Empty;

            string boundary = HTTPProcessor.ExtractBoundary(ContentType);
            using (MemoryStream ms = new(PostData))
            {
                var data = MultipartFormDataParser.Parse(ms, boundary);

                userSync = data.GetParameterValue("sync");

                ms.Flush();
            }

            LoggerAccessor.LogInfo($"[CDM] User Sync - Received: \n{userSync}");

            string pubListPath = $"{workpath}/CDM/{absolutePath}";
            Directory.CreateDirectory(pubListPath);
            string filePath = $"{pubListPath}/UserSyncData.json";

            try
            {
                File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(userSync));
                status = "<xml>\r\n\t" +
                    "<status>success</status>\r\n" +
                    "</xml>";
            } catch (Exception e) {
                LoggerAccessor.LogError($"[CDM] User Sync JSON write failed with exception {e}");

                status = "<xml>\r\n\t" +
                    "<status>fail</status>\r\n" +
                    "</xml>";
            }

            return status;
        }

    }
}