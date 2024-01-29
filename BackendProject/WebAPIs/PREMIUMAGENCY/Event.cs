using CustomLogger;
using HttpMultipartParser;

namespace BackendProject.WebAPIs.PREMIUMAGENCY
{
    public class Event
    {
        public static string? checkEventRequestPOST(byte[]? PostData, string? ContentType, string eventId)
        {
            switch (eventId)
            {
                case "76":
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - CheckEvent sent for LOCAL MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                case "63":
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - CheckEvent sent for QA MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                case "95":
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                default:
                    {
                        LoggerAccessor.LogError($"CheckEvent unhandled for eventId {eventId} | POSTDATA: \n{PostData}");
                        return null;
                    }
            }
        }

        public static string? entryEventRequestPOST(byte[]? PostData, string? ContentType, string eventId)
        {

            switch (eventId)
            {
                case "63":
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n" +
                         "<status type=\"int\">1</status>\r\n" +
                         "</xml>";
                case "76":
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for LOCAL MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n" +
                         "<status type=\"int\">1</status>\r\n" +
                         "</xml>";
                case "95":
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n" +
                         "<status type=\"int\">1</status>\r\n" +
                         "</xml>";
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - EntryEvent unhandled for eventId {eventId} | POSTDATA: \n{PostData}");
                        return null;
                    }
            }
        }

        public static string? getUserEventCustomRequestPOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            switch (eventId)
            {
                case "63":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/qagetUserEventCustom.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/qagetUserEventCustom.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetUserEventCustom sent for QA MikuLiveEvent {eventId}!");
                    break;
                case "76":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/localgetUserEventCustom.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/localgetUserEventCustom.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetUserEventCustom sent for LOCAL MikuLiveEvent {eventId}!");
                    break;
                case "95":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/getUserEventCustom.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/getUserEventCustom.xml");
                    LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetUserEventCustom sent for PUBLIC MikuLiveEvent {eventId}!");
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetUserEventCustom unhandled for eventId {eventId} | POSTDATA: \n{PostData}");
                        return null;
                    }
            }

            return null;
        }

        public static string? clearEventRequestPOST(byte[]? PostData, string? ContentType, string eventId)
        {
            switch (eventId)
            {
                case "76":
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - ClearEvent sent for LOCAL MikuLiveEvent {eventId}!");
                        return "<xml>" +
                            "\r\n<result type=\"int\">1</result>" +
                            "\r\n<description type=\"text\">Success</description>" +
                            "\r\n<error_no type=\"int\">0</error_no>" +
                            "\r\n<error_message type=\"text\">None</error_message>" +
                            "\r\n<status type=\"int\">2</status>" +
                            "\r\n</xml>";
                    }
                case "63":
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - ClearEvent sent for QA MikuLiveEvent {eventId}!");

                        return "<xml>" +
                            "\r\n<result type=\"int\">1</result>" +
                            "\r\n<description type=\"text\">Success</description>" +
                            "\r\n<error_no type=\"int\">0</error_no>" +
                            "\r\n<error_message type=\"text\">None</error_message>" +
                            "\r\n<status type=\"int\">2</status>" +
                            "\r\n</xml>";
                    }
                case "95":
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - ClearEvent sent for PUBLIC MikuLiveEvent {eventId}!");
                        return "<xml>" +
                            "\r\n<result type=\"int\">1</result>" +
                            "\r\n<description type=\"text\">Success</description>" +
                            "\r\n<error_no type=\"int\">0</error_no>" +
                            "\r\n<error_message type=\"text\">None</error_message>" +
                            "\r\n<status type=\"int\">2</status>" +
                            "\r\n</xml>";
                    }
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - ClearEvent unhandled for eventId {eventId} | POSTDATA: \n{PostData}");
                        return null;
                    }
            }
        }
    }
}
