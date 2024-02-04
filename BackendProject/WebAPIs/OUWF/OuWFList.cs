using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Org.BouncyCastle.Crypto.Modes.Gcm.GcmUtilities;

namespace BackendProject.WebAPIs.OUWF
{
    public class OuWFList
    {
        public static string? List(byte[]? PostData, string? ContentType)
        {
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);
            
            using (MemoryStream ms = new(PostData))
            {
                var data = MultipartFormDataParser.Parse(ms, boundary);

                int instanceId = Convert.ToInt32(data.GetParameterValue("instanceId"));
                string vers = data.GetParameterValue("version");
                string path = data.GetParameterValue("path");

                LoggerAccessor.LogInfo($"[OuWF] - Requested List with instanceId {instanceId} | version {vers} | path {path}");

                string xml = GenerateXml(path);
                LoggerAccessor.LogInfo($"[OuWF] - List Response: {xml}");
                ms.Flush();

                return xml;
            }
        }

        static string GenerateXml(string directoryPath)
        {
            string xml = "";

            var dirs = Directory.GetDirectories(directoryPath);
            var files = Directory.GetFiles(directoryPath);

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("list");

            XmlElement dirsElement = xmlDoc.CreateElement("dirs");
            foreach (var dir in dirs)
            {
                XmlElement dirElement = xmlDoc.CreateElement("dir");

                string HDKBuildRoot = "C:/HDK186/Build/";

                dirElement.InnerText = GetRelativePath(HDKBuildRoot, dir);
                dirsElement.AppendChild(dirElement);
            }

            XmlElement filesElement = xmlDoc.CreateElement("files");
            if (files.Length == 0)
            {
                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);

                    XmlElement fileElement = xmlDoc.CreateElement("file");
                    fileElement.SetAttribute("value", fileInfo.Name);

                    fileElement.SetAttribute("size", fileInfo.Length.ToString());
                    fileElement.SetAttribute("read_only", fileInfo.IsReadOnly.ToString());
                    filesElement.AppendChild(fileElement);
                }

                root.AppendChild(filesElement);
            }


            root.AppendChild(dirsElement);
            xmlDoc.AppendChild(root);

            //xmlDoc.Save(xml);
            return xmlDoc.OuterXml;
        }

        static string GetRelativePath(string rootPath, string fullPath)
        {
            if (Path.GetPathRoot(rootPath) != Path.GetPathRoot(fullPath))
            {
                throw new InvalidOperationException("Paths must have the same root.");
            }

            Uri rootUri = new Uri(rootPath);
            Uri fullUri = new Uri(fullPath);

            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fullUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}