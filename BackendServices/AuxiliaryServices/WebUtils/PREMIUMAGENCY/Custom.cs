using CustomLogger;
using System.Text;

namespace WebUtils.PREMIUMAGENCY
{
    public class Custom
    {
        public static string? setUserEventCustomPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            string? output = null;

            LoggerAccessor.LogInfo($"setUserEventCustom POSTDATA: \n{PostData}");

            switch (eventId)
            {
                case "95":
                    string mikuLiveEventFilePathPublic = $"{workpath}/eventController/MikuLiveEvent/setUserEventCustom.xml";
                    if (File.Exists(mikuLiveEventFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FOUND for PUBLIC MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveEventFilePathPublic);

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                             $"{res}" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FALLBACK sent for PUBLIC MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePathPublic}");

                        return ""; // For Rewards
                    }
                case "81":
                    //WhiteDay2010 doesn't care about setUserEventCustom, so we send a placebo response!
                    output = "<xml></xml>";
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom sent for WhiteDay2010 {eventId}!");

                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - SetUserEventCustom unhandled for eventId {eventId}");
                        return null;
                    }
            }

            return output;
        }

        public static string? getUserEventCustomRequestPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            switch (eventId)
            {
                case "63":
                    string mikuLiveEventFilePathQA = $"{workpath}/eventController/MikuLiveEvent/qagetUserEventCustom.xml";
                    if (File.Exists(mikuLiveEventFilePathQA))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FOUND for LOCAL MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveEventFilePathQA);

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                             $"{res}" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FALLBACK sent for LOCAL MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePathQA}");

                        return "<xml>\r\n" +
                            "<result type=\"int\">1</result>\r\n" +
                            "<description type=\"text\">Success</description>\r\n" +
                            "<error_no type=\"int\">0</error_no>\r\n" +
                            "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                            "<!-- entry 1 -->\r\n<field_list type=\"int\">1</field_list>\r\n" +
                            "<field_name type=\"text\">playrecord</field_name>\r\n" +
                            "<field_value type=\"int\">0</field_value>\r\n" +
                            "<update_year type=\"int\">2024</update_year>\r\n" +
                            "<update_month type=\"int\">02</update_month>\r\n" +
                            "<update_day type=\"int\">03</update_day>\r\n" +
                            "<update_hour type=\"int\">12</update_hour>\r\n" +
                            "<update_second type=\"int\">0</update_second>\r\n" +
                            "</xml>";
                    }
                    break;
                case "76":
                    string mikuLiveEventFilePathLocal = $"{workpath}/eventController/MikuLiveEvent/localgetUserEventCustom.xml";
                    if (File.Exists(mikuLiveEventFilePathLocal))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FOUND for LOCAL MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveEventFilePathLocal);

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                             $"{res}" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FALLBACK sent for LOCAL MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePathLocal}");

                        return "<xml>\r\n" +
                            "<result type=\"int\">1</result>\r\n" +
                            "<description type=\"text\">Success</description>\r\n" +
                            "<error_no type=\"int\">0</error_no>\r\n" +
                            "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                            "<!-- entry 1 -->\r\n<field_list type=\"int\">1</field_list>\r\n" +
                            "<field_name type=\"text\">playrecord</field_name>\r\n" +
                            "<field_value type=\"int\">0</field_value>\r\n" +
                            "<update_year type=\"int\">2024</update_year>\r\n" +
                            "<update_month type=\"int\">02</update_month>\r\n" +
                            "<update_day type=\"int\">03</update_day>\r\n" +
                            "<update_hour type=\"int\">12</update_hour>\r\n" +
                            "<update_second type=\"int\">0</update_second>\r\n" +
                            "</xml>";
                    }
                case "81":
                    string whiteDay2010FilePathPublic = $"{workpath}/eventController/MikuLiveEvent/getUserEventCustom.xml";
                    if (File.Exists(whiteDay2010FilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FOUND for PUBLIC WhiteDay2010 {eventId}!");
                        string res = File.ReadAllText(whiteDay2010FilePathPublic);

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                             $"{res}" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FALLBACK sent for PUBLIC WhiteDay2010 {eventId}!\nExpected path {whiteDay2010FilePathPublic}");

                        return "<xml>\r\n" +
                            "<result type=\"int\">1</result>\r\n" +
                            "<description type=\"text\">Success</description>\r\n" +
                            "<error_no type=\"int\">0</error_no>\r\n" +
                            "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                            "<!-- entry 1 -->\r\n<field_list type=\"int\">1</field_list>\r\n" +
                            "<field_name type=\"text\">playrecord</field_name>\r\n" +
                            "<field_value type=\"int\">0</field_value>\r\n" +
                            "<update_year type=\"int\">2024</update_year>\r\n" +
                            "<update_month type=\"int\">02</update_month>\r\n" +
                            "<update_day type=\"int\">03</update_day>\r\n" +
                            "<update_hour type=\"int\">12</update_hour>\r\n" +
                            "<update_second type=\"int\">0</update_second>\r\n" +
                            "</xml>";
                    }
                    break;
                case "95":
                    string mikuLiveEventFilePathPublic = $"{workpath}/eventController/MikuLiveEvent/getUserEventCustom.xml";
                    if (File.Exists(mikuLiveEventFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FOUND for PUBLIC MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveEventFilePathPublic);

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                             $"{res}" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FALLBACK sent for PUBLIC MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePathPublic}");

                        return "<xml>\r\n" +
                            "<result type=\"int\">1</result>\r\n" +
                            "<description type=\"text\">Success</description>\r\n" +
                            "<error_no type=\"int\">0</error_no>\r\n" +
                            "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                            "<!-- entry 1 -->\r\n<field_list type=\"int\">1</field_list>\r\n" +
                            "<field_name type=\"text\">playrecord</field_name>\r\n" +
                            "<field_value type=\"int\">0</field_value>\r\n" +
                            "<update_year type=\"int\">2024</update_year>\r\n" +
                            "<update_month type=\"int\">02</update_month>\r\n" +
                            "<update_day type=\"int\">03</update_day>\r\n" +
                            "<update_hour type=\"int\">12</update_hour>\r\n" +
                            "<update_second type=\"int\">0</update_second>\r\n" +
                            "</xml>";
                    }
                case "210":
                    string basaraEventFilePathPublic = $"{workpath}/eventController/collabo_iln/getUserEventCustom.xml";
                    if (File.Exists(basaraEventFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FOUND for PUBLIC Basara {eventId}!");
                        string res = File.ReadAllText(basaraEventFilePathPublic);

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                             $"{res}" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FALLBACK sent for PUBLIC Basara {eventId}!\nExpected path {basaraEventFilePathPublic}");

                        return "<xml>\r\n" +
                            "<result type=\"int\">1</result>\r\n" +
                            "<description type=\"text\">Success</description>\r\n" +
                            "<error_no type=\"int\">0</error_no>\r\n" +
                            "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                            "<!-- entry 1 -->\r\n<field_list type=\"int\">1</field_list>\r\n" +
                            "<field_name type=\"text\">playrecord</field_name>\r\n" +
                            "<field_value type=\"int\">0</field_value>\r\n" +
                            "<update_year type=\"int\">2024</update_year>\r\n" +
                            "<update_month type=\"int\">02</update_month>\r\n" +
                            "<update_day type=\"int\">03</update_day>\r\n" +
                            "<update_hour type=\"int\">12</update_hour>\r\n" +
                            "<update_second type=\"int\">0</update_second>\r\n" +
                            "</xml>";
                    }
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetUserEventCustom unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }
        }

        public static string? getUserEventCustomRequestListPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            switch (eventId)
            {
                case "92":
                    if (File.Exists($"{workpath}/eventController/MikeLiveJukebox/getUserEventCustomList.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikeLiveJukebox/getUserEventCustomList.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList sent for PUBLIC MikeLiveJukebox {eventId}!");
                    break;
                case "119":
                    if (File.Exists($"{workpath}/eventController/iDOLM@ASTERs/qagetUserEventCustomList.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLM@ASTERs/qagetUserEventCustomList.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList sent for QA iDOLM@ASTERs {eventId}!");
                    break;
                case "148":
                    if (File.Exists($"{workpath}/eventController/iDOLM@ASTERs/getUserEventCustomList.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLM@ASTERs/getUserEventCustomList.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList sent for PUBLIC iDOLM@ASTERs {eventId}!");
                    break;
                case "157":
                    if (File.Exists($"{workpath}/eventController/iDOLM@ASTERs/localgetUserEventCustomList.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLM@ASTERs/localgetUserEventCustomList.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList sent for LOCAL iDOLM@ASTERs {eventId}!");
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetUserEventCustomList unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }

            return null;
        }
    }
}