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

            #region InfoBoard Paths

            string infoBoardSchedulePath = $"{workpath}/eventController/InfoBoards/Schedule";

            #endregion

            switch (lounge)
            {
                case "HomeSquare":
                    {
                        Directory.CreateDirectory(infoBoardSchedulePath);
                        string filePath = $"{infoBoardSchedulePath}/{lounge}.xml";
                        if (File.Exists(filePath))
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - InfoBoardSchedule for {lounge} found and sent!");
                            return File.ReadAllText(filePath);
                        }
                        else
                        {
                            LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find InfoBoardSchedule {lounge} with expected path {filePath}!");
                        }
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