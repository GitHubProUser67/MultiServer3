using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;

namespace WebUtils.OHS
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
                    try
                    {
                        LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                    try
                    {
                        dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, 0);
                    }
                    catch (Exception ex)
                    {
                        LoggerAccessor.LogWarn($"[OHS] : Client issued Statistics with an unknown body format, report this to GITHUB: {ex}");
                    }
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
