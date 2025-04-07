using System.IO;
using CustomLogger;
using CyberBackendLibrary.HTTP;
using HttpMultipartParser;
using System.Text;
using System.Web;

namespace WebAPIService.PREMIUMAGENCY
{
    public class Event
    {
        public static string? checkEventRequestPOST(byte[] PostData, string ContentType, string eventId, string workpath, string fulluripath, string method)
        {
            string nid = string.Empty;

            if (method == "GET")
            {
                nid = HttpUtility.ParseQueryString(fulluripath).Get("nid");
            }
            else
            {
                string boundary = HTTPProcessor.ExtractBoundary(ContentType);

                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    nid = data.GetParameterValue("nid");

                    ms.Flush();
                }
            }

            if (nid == null || eventId == null)
            {
                LoggerAccessor.LogError("[PREMIUMAGENCY] - name id or event id is null, this shouldn't happen!!!");
                return null;
            }

            switch (eventId)
            {
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
                case "90":
                    string jCinemaLobbyFilePathPublic = $"{workpath}/eventController/WatchDetection/JCinemaLobby/qacheckEvent.xml";

                    if (File.Exists(jCinemaLobbyFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for QA JCinema Lobby Watching Detection {eventId}!");
                        string res = File.ReadAllText(jCinemaLobbyFilePathPublic);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for QA JCinema Lobby Watching Detection {eventId}!\nExpected path {jCinemaLobbyFilePathPublic}");
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
                case "104":
                    string jCinemaLobbyPublicFilePathPublic = $"{workpath}/eventController/WatchDetection/JCinemaLobby/checkEvent.xml";

                    if (File.Exists(jCinemaLobbyPublicFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for QA JCinema Lobby Watching Detection {eventId}!");
                        string res = File.ReadAllText(jCinemaLobbyPublicFilePathPublic);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for QA JCinema Lobby Watching Detection {eventId}!\nExpected path {jCinemaLobbyPublicFilePathPublic}");
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }

                case "109":
                    string jCinemaLobbyLocalFilePathPublic = $"{workpath}/eventController/WatchDetection/JCinemaLobby/localcheckEvent.xml";

                    if (File.Exists(jCinemaLobbyLocalFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for LOCAL JCinema Lobby Watching Detection {eventId}!");
                        string res = File.ReadAllText(jCinemaLobbyLocalFilePathPublic);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for LOCAL JCinema Lobby Watching Detection {eventId}!\nExpected path {jCinemaLobbyLocalFilePathPublic}");
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "148":
                    string iDOLMAASTERsFilePathPublic = $"{workpath}/eventController/iDOLMASTERs/LiveEvent/checkEvent.xml";

                    if (File.Exists(iDOLMAASTERsFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC iDOLM@STER {eventId}!");
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC iDOLM@STER {eventId}!\nExpected path {iDOLMAASTERsFilePathPublic}");
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
                case "300":
                    string j_liargame2Path = $"{workpath}/eventController/j_liargame2";
                    if (File.Exists(j_liargame2Path))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC LiarGame2 {eventId}!");
                        string res = File.ReadAllText(j_liargame2Path);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC LiarGame2 {eventId}!\nExpected path {j_liargame2Path}");
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
                    string SonyAquariumConfigPath = $"{workpath}/eventController/SonyAquarium/Config/checkEvent.xml";
                    if (File.Exists(SonyAquariumConfigPath))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC SonyAquarium {eventId}!");
                        string res = File.ReadAllText(SonyAquariumConfigPath);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC SonyAquarium {eventId}!\nExpected path {SonyAquariumConfigPath}");
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "349":
                    string SonyAquariumVideoConfigPath = $"{workpath}/eventController/SonyAquarium/VideoConfig/checkEvent.xml";
                    if (File.Exists(SonyAquariumVideoConfigPath))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC SonyAquarium {eventId}!");
                        string res = File.ReadAllText(SonyAquariumVideoConfigPath);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC SonyAquarium {eventId}!\nExpected path {SonyAquariumVideoConfigPath}");
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "86":
                    string T037JPHS = $"{workpath}/eventController/ItemRankings/hs/T037/checkEvent.xml";
                    if (File.Exists(T037JPHS))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC T037 Home Square {eventId}!");
                        string res = File.ReadAllText(T037JPHS);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC T037 Home Square {eventId}!\nExpected path {T037JPHS}");
                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "80":
                    string RollyJukeboxPath = $"{workpath}/eventController/RollyJukebox/checkEvent.xml";
                    if (File.Exists(RollyJukeboxPath))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for PUBLIC RollyJukebox {eventId}!");
                        string res = File.ReadAllText(RollyJukeboxPath);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK sent for PUBLIC RollyJukebox {eventId}!\nExpected path {RollyJukeboxPath}");
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

        public static string? entryEventRequestPOST(byte[] PostData, string ContentType, string eventId, string workPath, string fulluripath, string method)
        {
            string nid = string.Empty;

            if (method == "GET")
            {
                nid = HttpUtility.ParseQueryString(fulluripath).Get("nid");
            }
            else
            {
                string boundary = HTTPProcessor.ExtractBoundary(ContentType);

                using MemoryStream ms = new MemoryStream(PostData);
                var data = MultipartFormDataParser.Parse(ms, boundary);

                nid = data.GetParameterValue("nid");

                ms.Flush();
            }

            if (nid == null || eventId == null)
            {
                LoggerAccessor.LogError("[PREMIUMAGENCY] - name id or event id is null, this shouldn't happen!!!");
                return null;
            }

            switch (eventId)
            {
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
                    string mikuLiveJackFilePathPublic = $"{workPath}/eventController/iDOLMASTERs/LiveEvent/entryEvent.xml";
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
                case "300":
                    string j_liargame2Path = $"{workPath}/eventController/j_liargame2";
                    if (File.Exists(j_liargame2Path))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for PUBLIC LiarGame2 {eventId}!");
                        string res = File.ReadAllText(j_liargame2Path);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for PUBLIC LiarGame2 {eventId}!\nExpected path {j_liargame2Path}");

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
                    string SonyAquariumConfigPath = $"{workPath}/eventController/SonyAquarium/Config/entryEvent.xml";
                    if (File.Exists(SonyAquariumConfigPath))
                    {

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for PUBLIC SonyAquarium {eventId}!");
                        string res = File.ReadAllText(SonyAquariumConfigPath);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for PUBLIC SonyAquarium {eventId}!\nExpected path {SonyAquariumConfigPath}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "349":
                    string SonyAquariumVideoConfigPath = $"{workPath}/eventController/SonyAquarium/VideoConfig/entryEvent.xml";
                    if (File.Exists(SonyAquariumVideoConfigPath))
                    {

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for PUBLIC SonyAquarium {eventId}!");
                        string res = File.ReadAllText(SonyAquariumVideoConfigPath);
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for PUBLIC SonyAquarium {eventId}!\nExpected path {SonyAquariumVideoConfigPath}");

                        return "<xml>\r\n\t" +
                             "<result type=\"int\">1</result>\r\n\t" +
                             "<description type=\"text\">Success</description>\r\n\t" +
                             "<error_no type=\"int\">0</error_no>\r\n\t" +
                             "<error_message type=\"text\">None</error_message>\r\n\r\n\t" +
                             $"<status type=\"int\">0</status>\r\n" +
                             "</xml>";
                    }
                case "86":
                    string HomeSquareT037 = $"{workPath}/eventController/ItemRankings/hs/T037/entryEvent.xml";

                    if (File.Exists(HomeSquareT037))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FOUND for PUBLIC HomeSquareT037 {eventId}!");
                        string res = File.ReadAllText(HomeSquareT037);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - EntryEvent FALLBACK sent for PUBLIC HomeSquareT037 {eventId}!\nExpected path {HomeSquareT037}");
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

        public static string? clearEventRequestPOST(byte[]? PostData, string? ContentType, string eventId, string workPath, string fulluripath, string method)
        {
            string nid = string.Empty;

            if (method == "GET")
            {
                nid = HttpUtility.ParseQueryString(fulluripath).Get("nid");
            }
            else
            {
                string boundary = HTTPProcessor.ExtractBoundary(ContentType);

                using MemoryStream ms = new MemoryStream(PostData);
                var data = MultipartFormDataParser.Parse(ms, boundary);

                nid = data.GetParameterValue("nid");

                ms.Flush();
            }

            if (nid == null || eventId == null)
            {
                LoggerAccessor.LogError("[PREMIUMAGENCY] - name id or event id is null, this shouldn't happen!!!");
                return null;
            }

            switch (eventId)
            {
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

                case "148":
                    string idolMasters = $"{workPath}/eventController/idolMasters/clearEvent.xml";

                    if (File.Exists(idolMasters))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ClearEvent FOUND for PUBLIC idolMasters {eventId}!");
                        string res = File.ReadAllText(idolMasters);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ClearEvent FALLBACK sent for PUBLIC idolMasters {eventId}!\nExpected path {idolMasters}");

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
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - ClearEvent unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }
        }
    }
}