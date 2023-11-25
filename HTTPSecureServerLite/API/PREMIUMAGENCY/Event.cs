using HttpMultipartParser;

namespace HTTPSecureServerLite.API.PREMIUMAGENCY
{
    public class Event
    {
        public static string? checkEventRequestPOST(byte[]? PostData, string? ContentType)
        {
            string eventId = string.Empty;
            string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    eventId = data.GetParameterValue("evid");

                    ms.Flush();
                }

                switch (eventId)
                {
                    case "76":
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             "<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    case "95":
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             "<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                }
            }

            return null;
        }

        public static string? entryEventRequestPOST(byte[]? PostData, string? ContentType)
        {
            string eventId = string.Empty;
            string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    eventId = data.GetParameterValue("evid");

                    ms.Flush();
                }

                switch (eventId)
                {
                    case "76":
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "<status type=\"int\">1</status>\r\n" +
                             "</xml>";
                    case "63":
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "<status type=\"int\">1</status>\r\n" +
                             "</xml>";
                    case "95":
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "<status type=\"int\">1</status>\r\n" +
                             "</xml>";
                }
            }

            return null;
        }

        public static string? getUserEventCustomRequestPOST(byte[]? PostData, string? ContentType)
        {
            string eventId = string.Empty;
            string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    eventId = data.GetParameterValue("evid");

                    ms.Flush();
                }

                switch (eventId)
                {
                    case "76":
                        if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/eventController/MikuLiveEvent/localgetUserEventCustom.xml"))
                            return File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/eventController/MikuLiveEvent/localgetUserEventCustom.xml");
                        break;
                    case "63":
                        if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/eventController/MikuLiveEvent/qagetUserEventCustom.xml"))
                            return File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/eventController/MikuLiveEvent/qagetUserEventCustom.xml");
                        break;
                    case "95":
                        if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/eventController/MikuLiveEvent/getUserEventCustom.xml"))
                            return File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/eventController/MikuLiveEvent/getUserEventCustom.xml");
                        break;
                }
            }

            return null;
        }

        public static string? clearEventRequestPOST(byte[]? PostData, string? ContentType)
        {
            string eventId = string.Empty;
            string? boundary = CryptoSporidium.HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    eventId = data.GetParameterValue("evid");

                    ms.Flush();
                }

                switch (eventId)
                {
                    case "76":
                        return "<xml>" +
                            "\r\n<result type=\"int\">1</result>" +
                            "\r\n<description type=\"text\">Success</description>" +
                            "\r\n<error_no type=\"int\">0</error_no>" +
                            "\r\n<error_message type=\"text\">None</error_message>" +
                            "\r\n<status type=\"int\">2</status>" +
                            "\r\n</xml>";
                    case "63":
                        return "<xml>" +
                            "\r\n<result type=\"int\">1</result>" +
                            "\r\n<description type=\"text\">Success</description>" +
                            "\r\n<error_no type=\"int\">0</error_no>" +
                            "\r\n<error_message type=\"text\">None</error_message>" +
                            "\r\n<status type=\"int\">2</status>" +
                            "\r\n</xml>";
                    case "95":
                        return "<xml>" +
                            "\r\n<result type=\"int\">1</result>" +
                            "\r\n<description type=\"text\">Success</description>" +
                            "\r\n<error_no type=\"int\">0</error_no>" +
                            "\r\n<error_message type=\"text\">None</error_message>" +
                            "\r\n<status type=\"int\">2</status>" +
                            "\r\n</xml>";
                }
            }

            return null;
        }
    }
}
