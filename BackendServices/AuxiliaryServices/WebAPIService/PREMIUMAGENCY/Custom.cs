using CyberBackendLibrary.HTTP;
using CustomLogger;
using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace WebAPIService.PREMIUMAGENCY
{
    public class Custom
    {
        public static string? setUserEventCustomPOST(byte[] PostData, string ContentType, string workpath, string eventId, string fulluripath, string method)
        {
            string nid = string.Empty;

            if (method == "GET")
            {
                nid = HttpUtility.ParseQueryString(fulluripath).Get("key");
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



            switch (eventId)
            {
                case "92":
                    string MikuLiveJukeboxPath = $"{workpath}/eventController/MikuLiveJukebox/Custom";
                    Directory.CreateDirectory(MikuLiveJukeboxPath);
                    string fullPath = MikuLiveJukeboxPath + $"/{nid}-{DateTime.Now.ToString("yyyyMMdd")}.cache";
                    PREMIUMAGENCYClass.WriteFormDataToFile(Encoding.UTF8.GetString(PostData), fullPath);
                    if (File.Exists(MikuLiveJukeboxPath))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FOUND for PUBLIC MikuLiveJukebox {eventId}!");

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FALLBACK sent for PUBLIC MikuLiveJukebox {eventId}!\nExpected path {MikuLiveJukeboxPath}");

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }
                case "95":
                    string mikuLiveEventFilePathPublic = $"{workpath}/eventController/MikuLiveEvent/Custom";
                    Directory.CreateDirectory(mikuLiveEventFilePathPublic);
                    PREMIUMAGENCYClass.WriteFormDataToFile(Encoding.UTF8.GetString(PostData),  $"{mikuLiveEventFilePathPublic}/{nid}-{DateTime.Now.ToString("yyyyMMdd")}.cache");
                    if (File.Exists(mikuLiveEventFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FOUND for PUBLIC MikuLiveEvent {eventId}!");

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FALLBACK sent for PUBLIC MikuLiveEvent {eventId}!\nExpected path {mikuLiveEventFilePathPublic}");

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }
                case "81":
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom sent for WhiteDay2010 {eventId}!");
                    return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                case "86":
                    string T037HomeSquare = $"{workpath}/eventController/ItemRankings/hs/T037";
                    Directory.CreateDirectory(T037HomeSquare);
                    PREMIUMAGENCYClass.WriteFormDataToFile(Encoding.UTF8.GetString(PostData), $"{T037HomeSquare}/{nid}-{DateTime.Now.ToString("yyyyMMdd")}.cache");
                    if (File.Exists(T037HomeSquare))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FOUND for PUBLIC T037HomeSquare {eventId}!");
                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FALLBACK sent for PUBLIC T037HomeSquare {eventId}!\nExpected path {T037HomeSquare}");

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }

                case "148":
                    string idolMasters = $"{workpath}/eventController/idolMasters/Custom";
                    Directory.CreateDirectory(idolMasters);
                    PREMIUMAGENCYClass.WriteFormDataToFile(Encoding.UTF8.GetString(PostData), $"{idolMasters}/{nid}.cache");
                    if (File.Exists(idolMasters))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FOUND for PUBLIC idolMasters {eventId}!");
                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FALLBACK sent for PUBLIC idolMasters {eventId}!\nExpected path {idolMasters}");

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }
                case "300":
                    string j_liargame2Path = $"{workpath}/eventController/j_liargame2/Custom";
                    Directory.CreateDirectory(j_liargame2Path);
                    PREMIUMAGENCYClass.WriteFormDataToFile(Encoding.UTF8.GetString(PostData), $"{j_liargame2Path}/{nid}-{DateTime.Now.ToString("yyyyMMdd")}.cache");
                    if (File.Exists(j_liargame2Path))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FOUND for PUBLIC LiarGame2 {eventId}!");
                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FALLBACK sent for PUBLIC LiarGame2 {eventId}!\nExpected path {j_liargame2Path}");

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }
                case "347":
                    string SonyAquarium = $"{workpath}/eventController/SonyAquarium/Config/cache/";
                    Directory.CreateDirectory(SonyAquarium);
                    PREMIUMAGENCYClass.WriteFormDataToFile(Encoding.UTF8.GetString(PostData), $"{SonyAquarium}/{nid}.cache");
                    if (File.Exists(SonyAquarium))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FOUND for PUBLIC SonyAquarium Config {eventId}!");
                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - SetUserEventCustom FALLBACK sent for PUBLIC SonyAquarium {eventId}!\nExpected path {SonyAquarium}");

                        return "<xml>\r\n" +
                             "<result type=\"int\">1</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">0</error_no>\r\n" +
                             "<error_message type=\"text\">None</error_message>\r\n" +
                             "</xml>";
                    }
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - SetUserEventCustom unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return "<xml>\r\n" +
                             "<result type=\"int\">303</result>\r\n" +
                             "<description type=\"text\">Success</description>\r\n" +
                             "<error_no type=\"int\">1</error_no>\r\n" +
                             "<error_message type=\"text\">SetUserEventCustom unhandled for eventId!</error_message>\r\n" +
                             "</xml>";
                    }
            }

        }

        public static string getUserEventCustomRequestPOST(byte[] PostData, string ContentType, string workpath, string eventId, string method)
        {
            string nid = string.Empty;

            string boundary = HTTPProcessor.ExtractBoundary(ContentType);

            using (MemoryStream ms = new MemoryStream(PostData))
            {
                var data = MultipartFormDataParser.Parse(ms, boundary);

                nid = data.GetParameterValue("nid");

                ms.Flush();
            }

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
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</xml>";
                    }
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
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
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
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</xml>";
                    }
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
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
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
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</xml>";
                    }
                case "86":
                    string rollyMusicSurveyFilePath = $"{workpath}/eventController/MusicSurvey/Rolly/getUserEventCustom.xml";
                    if (File.Exists(rollyMusicSurveyFilePath))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FOUND for PUBLIC Rolly Music Survey {eventId}!");
                        string res = File.ReadAllText(rollyMusicSurveyFilePath);

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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FALLBACK sent for PUBLIC Rolly Music Survey {eventId}!\nExpected path {rollyMusicSurveyFilePath}");

                        return "<xml>\r\n" +
                            "<result type=\"int\">1</result>\r\n" +
                            "<description type=\"text\">Success</description>\r\n" +
                            "<error_no type=\"int\">0</error_no>\r\n" +
                            "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                            "<!-- entry 1 -->\r\n<field_list type=\"int\">1</field_list>\r\n" +
                            "<field_name type=\"text\">playrecord</field_name>\r\n" +
                            "<field_value type=\"int\">0</field_value>\r\n" +
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</xml>";
                    }

                case "347":
                    string SonyAquarium = $"{workpath}/eventController/SonyAquarium/Config/cache/{nid}.cache";

                    if (File.Exists(SonyAquarium))
                    {
                        var sonyAquariumCacheSaveData = PREMIUMAGENCYClass.ReadFormDataFromFile($"{SonyAquarium}");

                        string sonyAquariumSaveDataCFName = sonyAquariumCacheSaveData.Find(x => x.Item1 == "cfnam").Item2;
                        string sonyAquariumSaveDataCFVal = sonyAquariumCacheSaveData.Find(x => x.Item1 == "cfval").Item2;
                        string sonyAquariumSaveDataCFNumberVal = sonyAquariumCacheSaveData.Find(x => x.Item1 == "cfnumber").Item2;
                        string sonyAquariumSaveDataCFFlagVal = sonyAquariumCacheSaveData.Find(x => x.Item1 == "cfflag").Item2;

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FOUND for PUBLIC SonyAquarium {eventId}!");
                        return "<xml>\r\n\t" +
                            "<result type=\"int\">1</result>\r\n" +
                            "    <description type=\"text\">Success</description>\r\n" +
                            "    <error_no type=\"int\">0</error_no>\r\n" +
                            "    <error_message type=\"text\">None</error_message>rn\r\n" +
                            "    <field_list>\r\n\t\t" +
                            $"      <field_name type=\"text\">{sonyAquariumSaveDataCFName}</field_name>\r\n\t\t" +
                            $"      <field_value type=\"text\">{sonyAquariumSaveDataCFVal}</field_value>\r\n\t\t" +
                            $"      <field_number type=\"int\">{sonyAquariumSaveDataCFNumberVal}</field_number>\r\n\t\t" +
                            $"      <field_flag type=\"text\">{sonyAquariumSaveDataCFFlagVal}</field_flag>\r\n\t\t" +
                            $"      <update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"      <update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"      <update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"      <update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"      <update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "   </field_list>\r\n" +
                            "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FALLBACK sent for PUBLIC SonyAquarium {eventId}!\nExpected path {SonyAquarium}");

                        return "<xml>\r\n\t" +
                            "<result type=\"int\">1</result>\r\n" +
                            "    <description type=\"text\">Success</description>\r\n" +
                            "    <error_no type=\"int\">0</error_no>\r\n" +
                            "    <error_message type=\"text\">None</error_message>rn\r\n" +
                            "    <field_list>\r\n\t\t" +
                            $"      <field_name type=\"text\">savedata</field_name>\r\n\t\t" +
                            $"      <field_value type=\"text\">0</field_value>\r\n\t\t" +
                            $"      <field_number type=\"int\">0</field_number>\r\n\t\t" +
                            $"      <field_flag type=\"text\">0</field_flag>\r\n\t\t" +
                            $"      <update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"      <update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"      <update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"      <update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"      <update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</field_list>\r\n" +
                            "</xml>";
                    }

                case "349":
                    string SonyAquariumVieoConfig = $"{workpath}/eventController/SonyAquarium/VideoConfig/cache/{nid}.cache";

                    if (File.Exists(SonyAquariumVieoConfig))
                    {
                        var sonyAquariumCacheSaveData = PREMIUMAGENCYClass.ReadFormDataFromFile($"{SonyAquariumVieoConfig}");

                        string sonyAquariumSaveDataCFName = sonyAquariumCacheSaveData.Find(x => x.Item1 == "cfnam").Item2;
                        string sonyAquariumSaveDataCFVal = sonyAquariumCacheSaveData.Find(x => x.Item1 == "cfval").Item2;
                        string sonyAquariumSaveDataCFNumberVal = sonyAquariumCacheSaveData.Find(x => x.Item1 == "cfnumber").Item2;
                        string sonyAquariumSaveDataCFFlagVal = sonyAquariumCacheSaveData.Find(x => x.Item1 == "cfflag").Item2;

                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FOUND for PUBLIC SonyAquarium {eventId}!");
                        return "<xml>\r\n\t" +
                            "<result type=\"int\">1</result>\r\n" +
                            "    <description type=\"text\">Success</description>\r\n" +
                            "    <error_no type=\"int\">0</error_no>\r\n" +
                            "    <error_message type=\"text\">None</error_message>rn\r\n" +
                            "    <field_list>\r\n\t\t" +
                            $"      <field_name type=\"text\">{sonyAquariumSaveDataCFName}</field_name>\r\n\t\t" +
                            $"      <field_value type=\"text\">{sonyAquariumSaveDataCFVal}</field_value>\r\n\t\t" +
                            $"      <field_number type=\"int\">{sonyAquariumSaveDataCFNumberVal}</field_number>\r\n\t\t" +
                            $"      <field_flag type=\"text\">{sonyAquariumSaveDataCFFlagVal}</field_flag>\r\n\t\t" +
                            $"      <update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"      <update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"      <update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"      <update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"      <update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "   </field_list>\r\n" +
                            "</xml>";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustom FALLBACK sent for PUBLIC SonyAquarium {eventId}!\nExpected path {SonyAquariumVieoConfig}");

                        return "<xml>\r\n\t" +
                            "<result type=\"int\">1</result>\r\n" +
                            "    <description type=\"text\">Success</description>\r\n" +
                            "    <error_no type=\"int\">0</error_no>\r\n" +
                            "    <error_message type=\"text\">None</error_message>rn\r\n" +
                            "    <field_list>\r\n\t\t" +
                            $"      <field_name type=\"text\">view</field_name>\r\n\t\t" +
                            $"      <field_value type=\"text\">0</field_value>\r\n\t\t" +
                            $"      <field_number type=\"int\">0</field_number>\r\n\t\t" +
                            $"      <field_flag type=\"text\">0</field_flag>\r\n\t\t" +
                            $"      <update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"      <update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"      <update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"      <update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"      <update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</field_list>\r\n" +
                            "</xml>";
                    }
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetUserEventCustom unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }
        }

        public static string? getUserEventCustomRequestListPOST(byte[] PostData, string ContentType, string workpath, string eventId, string fulluripath, string method)
        {

            string? nid = string.Empty;

            if(method == "GET")
            {
                nid = HttpUtility.ParseQueryString(fulluripath).Get("nid");
            } else {
                string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

                using MemoryStream ms = new MemoryStream(PostData);
                var data = MultipartFormDataParser.Parse(ms, boundary);

                nid = data.GetParameterValue("nid");

                ms.Flush();
            }

            switch (eventId)
            {
                case "92":
                    string mikuLiveJukeBoxFilePathPublic = $"{workpath}/eventController/MikuLiveJukebox/Custom/{nid}-{DateTime.Now.ToString("yyyyMMdd")}.cache";
                    // ANSWER FILE EXISTS, SO THEY CAN NOT USE QUESTIONAIRE/VOTE!
                    if (File.Exists(mikuLiveJukeBoxFilePathPublic))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList FOUND for PUBLIC MikeLiveJukebox {eventId}!");
                        List<(string, string)> formData = PREMIUMAGENCYClass.ReadFormDataFromFile(mikuLiveJukeBoxFilePathPublic);

                        string votedToday = formData.Find(x => x.Item1 == "cfval").Item2;

                        return "<result type=\"int\">1</result>\r\n" +
                            "<description type=\"text\">Success</description>\r\n" +
                            "<error_no type=\"int\">0</error_no>\r\n" +
                            "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                            "<field_list>\r\n" +
                            "<field_name type=\"text\">answer_record</field_name>\r\n" +
                            $"<field_value type=\"text\">{votedToday}</field_value>\r\n" +
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</field_list>\r\n\r\n" +
                            "<field_list>\r\n" +
                            "<field_name type=\"text\">datetime</field_name>\r\n" +
                            $"<field_value type=\"text\">{DateTime.Now.ToString("yyyy")}d{DateTime.Now.ToString("MM")}d{DateTime.Now.ToString("dd")}</field_value>\r\n" +
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</field_list>\r\n";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList FALLBACK sent for PUBLIC MikeLiveJukebox {eventId}!\nExpected path {mikuLiveJukeBoxFilePathPublic}");

                        return "<result type=\"int\">1</result>\r\n" +
                            "<description type=\"text\">Success</description>\r\n" +
                            "<error_no type=\"int\">0</error_no>\r\n" +
                            "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                            "<field_list>\r\n" +
                            "<field_name type=\"text\">answer_record</field_name>\r\n" +
                            "<field_value type=\"text\">0</field_value>\r\n" + 
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</field_list>\r\n\r\n" +
                            "<field_list>\r\n" +
                            "<field_name type=\"text\">datetime</field_name>\r\n" +
                            $"<field_value type=\"text\">{DateTime.Now.ToString("yyyy")}d{DateTime.Now.ToString("MM")}d{DateTime.Now.ToString("dd")}</field_value>\r\n" +
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</field_list>\r\n";
                    }
                case "86":
                    string T037HomeSquare = $"{workpath}/eventController/ItemRankings/hs/T037/{nid}-{DateTime.Now.ToString("yyyyMMdd")}.cache";
                    // ANSWER FILE EXISTS, SO THEY CAN NOT USE QUESTIONAIRE/VOTE!
                    if (File.Exists(T037HomeSquare))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList FOUND for PUBLIC MikeLiveJukebox {eventId}!");
                        List<(string, string)> formData = PREMIUMAGENCYClass.ReadFormDataFromFile(T037HomeSquare);

                        string votedToday = formData.Find(x => x.Item1 == "cfval").Item2;

                        return "<result type=\"int\">1</result>\r\n" +
                            "<description type=\"text\">Success</description>\r\n" +
                            "<error_no type=\"int\">0</error_no>\r\n" +
                            "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                            "<field_list>\r\n" +
                            "<field_name type=\"text\">answer_record</field_name>\r\n" +
                            $"<field_value type=\"text\">{votedToday}</field_value>\r\n" +
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</field_list>\r\n\r\n" +
                            "<field_list>\r\n" +
                            "<field_name type=\"text\">datetime</field_name>\r\n" +
                            $"<field_value type=\"text\">{DateTime.Now.ToString("yyyy")}d{DateTime.Now.ToString("MM")}d{DateTime.Now.ToString("dd")}</field_value>\r\n" +
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</field_list>\r\n";
                    }
                    else
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList FALLBACK sent for PUBLIC MikeLiveJukebox {eventId}!\nExpected path {T037HomeSquare}");

                        return "<result type=\"int\">1</result>\r\n" +
                            "<description type=\"text\">Success</description>\r\n" +
                            "<error_no type=\"int\">0</error_no>\r\n" +
                            "<error_message type=\"text\">None</error_message>\r\n\r\n" +
                            "<field_list>\r\n" +
                            "<field_name type=\"text\">answer_record</field_name>\r\n" +
                            "<field_value type=\"text\">0</field_value>\r\n" +
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</field_list>\r\n\r\n" +
                            "<field_list>\r\n" +
                            "<field_name type=\"text\">datetime</field_name>\r\n" +
                            $"<field_value type=\"text\">{DateTime.Now.ToString("yyyy")}d{DateTime.Now.ToString("MM")}d{DateTime.Now.ToString("dd")}</field_value>\r\n" +
                            $"<update_year type=\"int\">{DateTime.Now.ToString("yyyy")}</update_year>\r\n" +
                            $"<update_month type=\"int\">{DateTime.Now.ToString("MM")}</update_month>\r\n" +
                            $"<update_day type=\"int\">{DateTime.Now.ToString("dd")}</update_day>\r\n" +
                            $"<update_hour type=\"int\">{DateTime.Now.ToString("hh")}</update_hour>\r\n" +
                            $"<update_second type=\"int\">{DateTime.Now.ToString("ss")}</update_second>\r\n" +
                            "</field_list>\r\n";
                    }
                case "119":
                    if (File.Exists($"{workpath}/eventController/iDOLMASTERs/qagetUserEventCustomList.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLMASTERs/qagetUserEventCustomList.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList sent for QA iDOLMASTERs {eventId}!");
                    break;
                case "148":
                    if (File.Exists($"{workpath}/eventController/iDOLMASTERs/getUserEventCustomList.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLMASTERs/getUserEventCustomList.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList sent for PUBLIC iDOLMASTERs {eventId}!");
                    break;
                case "157":
                    if (File.Exists($"{workpath}/eventController/iDOLMASTERs/localgetUserEventCustomList.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLMASTERs/localgetUserEventCustomList.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetUserEventCustomList sent for LOCAL iDOLMASTERs {eventId}!");
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
