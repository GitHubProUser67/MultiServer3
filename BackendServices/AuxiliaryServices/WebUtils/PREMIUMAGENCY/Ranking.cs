using CustomLogger;

namespace WebUtils.PREMIUMAGENCY
{
    public class Ranking
    {

        public static string? getItemRankingTable(byte[]? PostData, string? ContentType, string eventId)
        {
            switch (eventId)
            {
                case "95":
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - getItemRankingTable sent for PUBLIC MikuLiveEvent {eventId}!");
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
