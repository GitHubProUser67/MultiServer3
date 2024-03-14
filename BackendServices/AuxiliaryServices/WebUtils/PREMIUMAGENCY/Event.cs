using CustomLogger;
using HttpMultipartParser;
using System.Text;

namespace WebUtils.PREMIUMAGENCY
{
    public class Event
    {
        public static string? checkEventRequestPOST(byte[]? PostData, string? ContentType, string eventId)
        {
            switch (eventId)
            {
                case "76":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent sent for LOCAL MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                case "63":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent sent for QA MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                case "81":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent sent for Public WhiteDay2010 {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                         "<status type=\"int\">1</status>\r\n" +
                         "</xml>";
                case "92":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent sent for PUBLIC MikuLiveJukebox {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                case "95":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent sent for PUBLIC MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                case "148":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent sent for PUBLIC iDOL M@ASTER Event! {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                case "342":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent sent for PUBLIC Spring2013 Event! {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                case "347":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent sent for PUBLIC SonyAquarium Event! {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                default:
                    {
                        LoggerAccessor.LogError($"CheckEvent unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }
        }

        public static string? entryEventRequestPOST(byte[]? PostData, string? ContentType, string eventId)
        {

            switch (eventId)
            {
                case "63":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent sent for QA MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n" +
                         "<status type=\"int\">1</status>\r\n" +
                         "</xml>";
                case "76":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent sent for LOCAL MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n" +
                         "<status type=\"int\">1</status>\r\n" +
                         "</xml>";
                case "81":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent sent for PUBLIC WhiteDay2010 {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n" +
                         "<status type=\"int\">1</status>\r\n" +
                         "</xml>";
                case "92":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent sent for PUBLIC MikuLiveJukebox {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n" +
                         "</xml>";
                case "95":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent sent for PUBLIC MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n" +
                         "<status type=\"int\">1</status>\r\n" +
                         "</xml>";
                case "148":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent sent for PUBLIC MikuLiveEvent {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n" +
                         "<status type=\"int\">1</status>\r\n" +
                         "</xml>";
                case "342":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent sent for PUBLIC Spring2013 {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                case "347":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent sent for PUBLIC SonyAquarium {eventId}!");
                    return "<xml>\r\n\t" +
                         "<result type=\"int\">1</result>\r\n\t" +
                         "<description type=\"text\">Success</description>\r\n\t" +
                         "<error_no type=\"int\">0</error_no>\r\n\t" +
                         "<error_message type=\"text\">None</error_message>\r\n" +
                         "<status type=\"int\">0</status>\r\n" +
                         "</xml>";
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - EntryEvent unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }
        }

        public static string? clearEventRequestPOST(byte[]? PostData, string? ContentType, string eventId)
        {
            switch (eventId)
            {
                case "76":
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ClearEvent sent for LOCAL MikuLiveEvent {eventId}!");
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ClearEvent sent for QA MikuLiveEvent {eventId}!");

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ClearEvent sent for PUBLIC MikuLiveEvent {eventId}!");
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
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - ClearEvent unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }
        }
    }
}
