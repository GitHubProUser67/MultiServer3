using CustomLogger;
using HttpMultipartParser;
using System.Text;

namespace WebUtils.PREMIUMAGENCY
{
    public class Event
    {
        public static string? checkEventRequestPOST(byte[]? PostData, string? ContentType, string eventId, string workpath)
        {
            switch (eventId)
            {
                case "76":
                    string mikuLiveJackFilePathLocal = $"{workpath}/eventController/MikuLiveJack/localcheckEvent.xml";
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent found sent for LOCAL MikuLiveEvent {eventId}!");

                    if (File.Exists(mikuLiveJackFilePathLocal))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for LOCAL MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveJackFilePathLocal);

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"{res}\r\n" +
                             "</xml>";
                    } else {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for LOCAL MikuLiveEvent {eventId}!\nExpected path {mikuLiveJackFilePathLocal}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "63":
                    string mikuLiveJackFilePathQA = $"{workpath}/eventController/MikuLiveJack/qacheckEvent.xml";
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent found sent for QA MikuLiveEvent {eventId}!");

                    if (File.Exists(mikuLiveJackFilePathQA))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for QA MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveJackFilePathQA);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for QA MikuLiveEvent {eventId}!\nExpected path {mikuLiveJackFilePathQA}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "81":
                    string whiteDay2010FilePath = $"{workpath}/eventController/WhiteDay/2010/checkEvent.xml";
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent found sent for PUBLIC WhiteDay2010 {eventId}!");

                    if (File.Exists(whiteDay2010FilePath))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC WhiteDay2010 {eventId}!");
                        string res = File.ReadAllText(whiteDay2010FilePath);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC WhiteDay2010 {eventId}!\nExpected path {whiteDay2010FilePath}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "92":
                    string mikuLiveJukeboxFilePathPublic = $"{workpath}/eventController/MikuLiveJukebox/checkEvent.xml";

                    if (File.Exists(mikuLiveJukeboxFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC MikuLiveJukebox {eventId}!");
                        string res = File.ReadAllText(mikuLiveJukeboxFilePathPublic);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC MikuLiveJukebox {eventId}!\nExpected path {mikuLiveJukeboxFilePathPublic}");
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "95":
                    string mikuLiveEventFilePathPublic = $"{workpath}/eventController/MikuLiveEvent/checkEvent.xml";
                    if (File.Exists(mikuLiveEventFilePathPublic))
                    {

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveEventFilePathPublic);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePathPublic}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "148":
                    string iDOLMAASTERsFilePathPublic = $"{workpath}/eventController/iDOLM@ASTERs/checkEvent.xml";

                    if (File.Exists(iDOLMAASTERsFilePathPublic))
                    {

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC iDOL M@ASTER {eventId}!");
                        string res = File.ReadAllText(iDOLMAASTERsFilePathPublic);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC iDOL M@ASTER {eventId}!\nExpected path {iDOLMAASTERsFilePathPublic}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "210":
                    string basaraCollabEventPath = $"{workpath}/eventController/collabo_iln/checkEvent.xml";
                    if (File.Exists(basaraCollabEventPath))
                    {

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC Basara {eventId}!");
                        string res = File.ReadAllText(basaraCollabEventPath);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC Basara {eventId}!\nExpected path {basaraCollabEventPath}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "342":
                    string Spring2013 = $"{workpath}/eventController/Spring/2013/checkEvent.xml";
                    if (File.Exists(Spring2013))
                    {

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC Spring2013 {eventId}!");
                        string res = File.ReadAllText(Spring2013);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC Spring2013 {eventId}!\nExpected path {Spring2013}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "347":
                    string SonyAquariumPath = $"{workpath}/eventController/SonyAquarium/checkEvent.xml";
                    if (File.Exists(SonyAquariumPath))
                    {

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC SonyAquarium {eventId}!");
                        string res = File.ReadAllText(SonyAquariumPath);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC SonyAquarium {eventId}!\nExpected path {SonyAquariumPath}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "86":
                    string homeSquareRollyFilePath = $"{workpath}/eventController/MusicSurvey/Rolly/checkEvent.xml";
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent found sent for PUBLIC Rolly Music Survey {eventId}!");

                    if (File.Exists(homeSquareRollyFilePath))
{
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC Rolly Music Survey {eventId}!");
                        string res = File.ReadAllText(homeSquareRollyFilePath);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC Rolly Music Survey {eventId}!\nExpected path {homeSquareRollyFilePath}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                default:
                    {
                        LoggerAccessor.LogError($"CheckEvent unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }
        }

        public static string? entryEventRequestPOST(byte[]? PostData, string? ContentType, string eventId, string workPath)
        {

            switch (eventId)
            {
                case "76":
                    string mikuLiveEventFilePathLocal = $"{workPath}/eventController/MikuLiveEvent/localEntryEvent.xml";

                    if (File.Exists(mikuLiveEventFilePathLocal))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for LOCAL MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveEventFilePathLocal);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for LOCAL MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePathLocal}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "63":
                    string mikuLiveEventFilePathQA = $"{workPath}/eventController/MikuLiveEvent/qaentryEvent.xml";

                    if (File.Exists(mikuLiveEventFilePathQA))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for QA MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveEventFilePathQA);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for QA MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePathQA}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "81":
                    string whiteDay2010FilePath = $"{workPath}/eventController/WhiteDay/2010/entryEvent.xml";

                    if (File.Exists(whiteDay2010FilePath))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - WhiteDay2010 FOUND for PUBLIC WhiteDay2010 {eventId}!");
                        string res = File.ReadAllText(whiteDay2010FilePath);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - WhiteDay2010 FALLBACK sent for PUBLIC WhiteDay2010 {eventId}!\nExpected path {whiteDay2010FilePath}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "92":
                    string mikuLiveJukeboxFilePathPublic = $"{workPath}/eventController/MikuLiveJukebox/entryEvent.xml";

                    if (File.Exists(mikuLiveJukeboxFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for PUBLIC MikuLiveJukebox {eventId}!");
                        string res = File.ReadAllText(mikuLiveJukeboxFilePathPublic);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for PUBLIC MikuLiveJukebox {eventId}!\nExpected path {mikuLiveJukeboxFilePathPublic}");
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "95":
                    string mikuLiveEventFilePathPublic = $"{workPath}/eventController/MikuLiveEvent/entryEvent.xml";
                    if (File.Exists(mikuLiveEventFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for PUBLIC MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveEventFilePathPublic);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for PUBLIC MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePathPublic}");
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "148":
                    string mikuLiveJackFilePathPublic = $"{workPath}/eventController/MikuLiveJack/entryEvent.xml";
                    if (File.Exists(mikuLiveJackFilePathPublic))
                    {

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for PUBLIC MikuJackEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveJackFilePathPublic);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ] FALLBACK sent for PUBLIC MikuJackEvent {eventId}!\nExpected path {mikuLiveJackFilePathPublic}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "210":
                    string basaraCollabEventPath = $"{workPath}/eventController/collabo_iln/entryEvent.xml";
                    if (File.Exists(basaraCollabEventPath))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for PUBLIC Basara {eventId}!");
                        string res = File.ReadAllText(basaraCollabEventPath);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for PUBLIC Basara {eventId}!\nExpected path {basaraCollabEventPath}");
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "342":
                    string Spring2013 = $"{workPath}/eventController/Spring/2013/entryEvent.xml";
                    if (File.Exists(Spring2013))
                    {

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for PUBLIC Spring2013 {eventId}!");
                        string res = File.ReadAllText(Spring2013);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for PUBLIC Spring2013 {eventId}!\nExpected path {Spring2013}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "347":
                    string SonyAquariumPath = $"{workPath}/eventController/SonyAquarium/entryEvent.xml";
                    if (File.Exists(SonyAquariumPath))
                    {

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for PUBLIC SonyAquarium {eventId}!");
                        string res = File.ReadAllText(SonyAquariumPath);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for PUBLIC SonyAquarium {eventId}!\nExpected path {SonyAquariumPath}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "86":
                    string homeSquareRollyFilePathEntry = $"{workPath}/eventController/MusicSurvey/Rolly/EntryEvent.xml";
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent found sent for PUBLIC Rolly Music Survey {eventId}!");

                    if (File.Exists(homeSquareRollyFilePathEntry))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for PUBLIC Rolly Music Survey {eventId}!");
                        string res = File.ReadAllText(homeSquareRollyFilePathEntry);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for PUBLIC Rolly Music Survey {eventId}!\nExpected path {homeSquareRollyFilePathEntry}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - EntryEvent unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }
        }

        public static string? clearEventRequestPOST(byte[]? PostData, string? ContentType, string eventId, string workPath)
        {
            switch (eventId)
            {
                case "76":
                    string mikuLiveEventFilePathLocal = $"{workPath}/eventController/MikuLiveEvent/localclearEvent.xml";

                    if (File.Exists(mikuLiveEventFilePathLocal))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ClearEvent FOUND for LOCAL MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveEventFilePathLocal);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ClearEvent FALLBACK sent for LOCAL MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePathLocal}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "63":
                    string mikuLiveEventFilePathQA = $"{workPath}/eventController/MikuLiveEvent/qacheckEvent.xml";

                    if (File.Exists(mikuLiveEventFilePathQA))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ClearEvent FOUND for QA MikuLiveEvent {eventId}!");
                        string res = File.ReadAllText(mikuLiveEventFilePathQA);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ClearEvent FALLBACK sent for QA MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePathQA}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "95":
                    {
                        string mikuLiveEventFilePath = $"{workPath}/eventController/MikuLiveEvent/clearEvent.xml";

                        if (File.Exists(mikuLiveEventFilePath))
                        {
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ClearEvent FOUND for PUBLIC MikuLiveEvent {eventId}!");
                            string res = File.ReadAllText(mikuLiveEventFilePath);

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
                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ClearEvent FALLBACK sent for PUBLIC MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePath}");

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
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - ClearEvent unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }
        }
    }
}
