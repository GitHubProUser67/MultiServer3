using BackendProject.MiscUtils;
using HttpMultipartParser;

namespace BackendProject.WebAPIs.PREMIUMAGENCY
{
    public class Resource
    {
        public static string? getResourcePOST(byte[]? PostData, string? ContentType, string workpath)
        {
            string resKey = string.Empty;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    resKey = data.GetParameterValue("key");

                    ms.Flush();
                }

                if (resKey == "jul2009")
                {
                    if (File.Exists($"{workpath}/eventController/infoboard/09/{resKey}.xml"))
                        return File.ReadAllText($"{workpath}/eventController/infoboard/09/{resKey}.xml");
                }
                else if (resKey.Contains("concert"))
                {
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml");
                }
                else if (resKey.Contains("miku_jukebox"))
                {
                    if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/resources/{resKey}.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/resources/{resKey}.xml");
                }

                return "<xml>" +
                       "\r\n\t<result type=\"int\">0</result>" +
                       "\r\n\t<description type=\"text\">Failed</description>" +
                       "\r\n\t<error_no type=\"int\">303</error_no>" +
                       "\r\n\t<error_message type=\"text\">No Resource Found</error_message>" +
                       $"\r\n\r\n\t<key type=\"text\">{resKey}</key>" +
                       "\r\n\t<seq type=\"int\">2</seq>" +
                       "\r\n\t<resource type=\"int\">1</resource>" +
                       "\r\n\t<data></data>" +
                       "\r\n</xml>";
            }

            return null;
        }
    }
}
