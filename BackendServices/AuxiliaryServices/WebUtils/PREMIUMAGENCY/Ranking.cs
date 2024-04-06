using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;
using System.Text;

namespace WebUtils.PREMIUMAGENCY
{
    public class Ranking
    {
        public static string? getItemRankingTableHandler(byte[]? PostData, string? ContentType, string workPath, string eventId)
        {
            #region Paths

            string homeSquareT037Path = $"{workPath}/eventController/ItemRankings/hs/T037/";


            string MikuLiveJukeboxPath = $"{workPath}/eventController/MikuLiveJukebox";
            string j_liargame2Path = $"{workPath}/eventController/j_liargame2/ItemRankings/";
            #endregion

            switch (eventId)
            {
                case "86":
                    {
                        Directory.CreateDirectory(homeSquareT037Path);
                        string filePath = $"{homeSquareT037Path}/getItemRankingTable.xml";
                        if (File.Exists(filePath))
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetItemRankingTable FOUND for PUBLIC HomeSquare T037 {eventId}!");
                            string res = File.ReadAllText(filePath);
                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"{res}\r\n" +
                                 "</xml>";
                        }
                        else
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetItemRankingTable FALLBACK sent for PUBLIC HomeSquare T037 {eventId}!\nExpected path {filePath}");

                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"<status type=\"int\">0</status>\r\n" +
                                 "</xml>";
                        }
                    }
                case "92":
                    {
                        Directory.CreateDirectory(MikuLiveJukeboxPath);
                        string filePath = $"{MikuLiveJukeboxPath}/getItemRankingTable.xml";
                        if (File.Exists(filePath))
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetItemRankingTable FOUND for PUBLIC MikuLiveJukebox {eventId}!");
                            string res = File.ReadAllText(filePath);
                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"{res}\r\n" +
                                 "</xml>";
                        }
                        else
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetItemRankingTable FALLBACK sent for PUBLIC MikuLiveJukebox {eventId}!\nExpected path {MikuLiveJukeboxPath}");

                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\t" +
                                 "</xml>";
                        }
                    }
                case "299":
                    {
                        Directory.CreateDirectory(j_liargame2Path);
                        string filePath = $"{j_liargame2Path}/getItemRankingTable.xml";
                        if (File.Exists(filePath))
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetItemRankingTable FOUND for PUBLIC LiarGame2 {eventId}!");
                            string res = File.ReadAllText(filePath);
                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"{res}\r\n" +
                                 "</xml>";
                        }
                        else
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetItemRankingTable FALLBACK sent for PUBLIC LiarGame2 {eventId}!\nExpected path {filePath}");

                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"<status type=\"int\">0</status>\r\n" +
                                 "</xml>";
                        }
                    }
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetItemRankingTable unhandled for eventId {eventId} | POSTDATA: \n{PostData}");
                        return null;
                    }
            }
        }

        public static string? entryItemRankingPointsHandler(byte[]? PostData, string? ContentType, string workPath, string eventId)
        {
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            string nid = string.Empty;

            using (MemoryStream ms = new(PostData))
            {
                var data = MultipartFormDataParser.Parse(ms, boundary);

                nid = data.GetParameterValue("nid");

                ms.Flush();
            }

            #region Paths

            string homeSquareT037Path = $"{workPath}/eventController/ItemRankings/hs/T037/ItemRankings";


            string MikuLiveJukeboxPath = $"{workPath}/eventController/MikuLiveJukebox/ItemRankings";
            string j_liargame2Path = $"{workPath}/eventController/j_liargame2/ItemRankings";
            #endregion

            switch (eventId)
            {
                case "86":
                    {
                        Directory.CreateDirectory(homeSquareT037Path);
                        string filePath = $"{homeSquareT037Path}/{nid}.cache";
                        PREMIUMAGENCYClass.WriteFormDataToFile(Encoding.UTF8.GetString(PostData), filePath);
                        Console.WriteLine("FormData written to file: " + filePath);
                        if (File.Exists(filePath))
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryItemRankingPoints FOUND for PUBLIC HomeSquare T037 {eventId}!");
                            string res = File.ReadAllText(filePath);
                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"<status type=\"int\">0</status>\r\n" +
                                 "</xml>";
                        }
                        else
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryItemRankingPoints FALLBACK sent for PUBLIC HomeSquare T037 {eventId}!\nExpected path {filePath}");

                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"<status type=\"int\">0</status>\r\n" +
                                 "</xml>";
                        }
                    }
                case "92":
                    {
                        Directory.CreateDirectory(MikuLiveJukeboxPath);
                        string filePath = $"{MikuLiveJukeboxPath}/{nid}.cache";
                        PREMIUMAGENCYClass.WriteFormDataToFile(Encoding.UTF8.GetString(PostData), filePath);
                        Console.WriteLine("FormData written to file: " + filePath);
                        if (File.Exists(filePath))
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryItemRankingPoints FOUND for PUBLIC MikuliveJukebox {eventId}!");
                            string res = File.ReadAllText(filePath);
                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"<status type=\"int\">0</status>\r\n" +
                                 "</xml>";
                        }
                        else
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryItemRankingPoints FALLBACK sent for PUBLIC MikuliveJukebox {eventId}!\nExpected path {filePath}");

                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"<status type=\"int\">0</status>\r\n" +
                                 "</xml>";
                        }
                    }

                case "299":
                    {
                        Directory.CreateDirectory(j_liargame2Path);
                        string filePath = $"{j_liargame2Path}/{nid}.cache";
                        PREMIUMAGENCYClass.WriteFormDataToFile(Encoding.UTF8.GetString(PostData), filePath);
                        Console.WriteLine("FormData written to file: " + filePath);
                        if (File.Exists(filePath))
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryItemRankingPoints FOUND for PUBLIC LiarGame2 {eventId}!");
                            string res = File.ReadAllText(filePath);
                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"<status type=\"int\">0</status>\r\n" +
                                 "</xml>";
                        }
                        else
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryItemRankingPoints FALLBACK sent for PUBLIC LiarGame2 {eventId}!\nExpected path {filePath}");

                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"<status type=\"int\">0</status>\r\n" +
                                 "</xml>";
                        }
                    }
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - EntryItemRankingPoints unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }
        }


        public static string? getItemRankingTargetListHandler(byte[]? PostData, string? ContentType, string workPath, string eventId)
        {
            #region Paths


            string MikuLiveJukeboxPath = $"{workPath}/eventController/MikuLiveJukebox";
            string T037HomeSquare = $"{workPath}/eventController/ItemRankings/hs/T037";
            #endregion

            switch (eventId)
            {
                case "92":
                    {
                        Directory.CreateDirectory(MikuLiveJukeboxPath);
                        string filePath = $"{MikuLiveJukeboxPath}/getItemRankingTargetList.xml";
                        if (File.Exists(filePath))
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetItemRankingTable FOUND for PUBLIC MikuLiveJukebox {eventId}!");
                            string res = File.ReadAllText(filePath);
                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"{res}\r\n" +
                                 "</xml>";
                        }
                        else
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetItemRankingTable FALLBACK sent for PUBLIC MikuLiveJukebox {eventId}!\nExpected path {MikuLiveJukeboxPath}");

                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\t" +
                                 "</xml>";
                        }
                    }
                case "86":
                    {
                        Directory.CreateDirectory(T037HomeSquare);
                        string filePath = $"{T037HomeSquare}/getItemRankingTargetList.xml";
                        if (File.Exists(filePath))
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetItemRankingTable FOUND for PUBLIC T037HomeSquare {eventId}!");
                            string res = File.ReadAllText(filePath);
                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                                 $"{res}\r\n" +
                                 "</xml>";
                        }
                        else
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetItemRankingTable FALLBACK sent for PUBLIC T037HomeSquare {eventId}!\nExpected path {T037HomeSquare}");

                            return "<xml>\r\n\t" +
                                 "<result type=\"int\">1</result>\r\n\t" +
                                 "<description type=\"text\">Success</description>\r\n\t" +
                                 "<error_no type=\"int\">0</error_no>\r\n\t" +
                                 "<error_message type=\"text\">None</error_message>\r\n\t" +
                                 "</xml>";
                        }
                    }
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetItemRankingTable unhandled for eventId {eventId} | POSTDATA: \n{PostData}");
                        return null;
                    }
            }
        }




    }
}
