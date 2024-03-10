using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;
using System.Text;

namespace WebUtils.PREMIUMAGENCY
{
    public class InfoBoard
    {
        public static string? getInformationBoardSchedulePOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);
            string? lounge = string.Empty;
            using (MemoryStream ms = new(PostData))
            {
                var data = MultipartFormDataParser.Parse(ms, boundary);

                lounge = data.GetParameterValue("lounge");
                string lang = data.GetParameterValue("lang");
                string regcd = data.GetParameterValue("regcd");

                ms.Flush();
            }

            switch (lounge)
            {
                case "HomeSquare":
                    {
                        if (File.Exists($"{workpath}/eventController/InfoBoards/Schedule/{lounge}.xml"))
                            return File.ReadAllText($"{workpath}/eventController/InfoBoards/Schedule/{lounge}.xml");
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - {lounge} lounge found for InfoBoardSchedule");
                        break;
                    }

                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - Unsupported scene lounge {lounge} found for InfoBoardSchedule");
                        return null;
                    }

                    
            }
            
            return null;
        }


    }
}