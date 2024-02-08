using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackendProject.WebAPIs.PREMIUMAGENCY
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