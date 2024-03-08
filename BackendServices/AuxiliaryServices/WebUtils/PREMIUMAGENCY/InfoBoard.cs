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
            LoggerAccessor.LogInfo($"InfoBoardSchedule: POSTDATA: {Encoding.UTF8.GetString(PostData)}");
            
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);
            string? lounge = string.Empty;
            using (MemoryStream ms = new(PostData))
            {
                var data = MultipartFormDataParser.Parse(ms, boundary);

                lounge = data.GetParameterValue("lounge");

                ms.Flush();
            }

            switch (lounge)
            {
                case "HomeSquare":
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - HomeSquare lounge found for InfoBoardSchedule: {lounge}");

                        if (File.Exists($"{workpath}/eventController/InfoBoards/Schedule/{lounge}.xml"))
                            return File.ReadAllText($"{workpath}/eventController/InfoBoards/Schedule/{lounge}.xml");

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - InfoBoard Schedule found for lounge {lounge}!");
                        return null;
                    }

                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - Unsupported scene lounge found for InfoBoardSchedule: {lounge}");
                        return null;
                    }

                    
            }
            

        }


    }
}