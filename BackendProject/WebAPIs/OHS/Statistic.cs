using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;

namespace BackendProject.WebAPIs.OHS
{
    public class Statistic
    {
        public static string? Set(byte[] PostData, string ContentType)
        {
            string? dataforohs = null;

            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                    dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, 0);
                    ms.Flush();
                }
            }

            if (!string.IsNullOrEmpty(dataforohs))
            {
                LoggerAccessor.LogInfo($"[OHS] : Client issued Statistics - {dataforohs}");
                return JaminProcessor.JaminFormat("{ [\"status\"] = \"success\" }", 0);
            }
            else
                return JaminProcessor.JaminFormat("{ [\"status\"] = \"fail\" }", 0);
        }
    }
}
