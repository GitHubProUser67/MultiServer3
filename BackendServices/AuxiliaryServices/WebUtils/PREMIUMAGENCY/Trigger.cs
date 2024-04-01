using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;
using System.Text;

namespace WebUtils.PREMIUMAGENCY
{
    public class Trigger
    {
        public static string? getEventTriggerRequestPOST(byte[] PostData, string ContentType, string workpath, string eventId)
        {

            switch (eventId)
            {
                #region MikuLiveJack
                //MikuLiveJack -- Appears on top Music Survey Jukebox in Theatre

                case "63":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/qagetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/qagetEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for QA MikuLiveJack {eventId}!");
                    break;
                case "76":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/localgetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/localgetEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for LOCAL MikuLiveJack {eventId}!");
                    break;
                #endregion

                case "80":
                    if (File.Exists($"{workpath}/eventController/RollyJukebox/global/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyJukebox/global/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC Rolly Music Survey {eventId}!");
                    break;

                case "90":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC MikuLiveJack {eventId}!");
                    break;
                //MikuLiveJukebox
                case "91":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/global/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/global/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC MikuLiveJukebox {eventId}!");
                    break;
                case "93":
                    if (File.Exists($"{workpath}/eventController/PrinnyJack/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/PrinnyJack/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC Prinny {eventId}!");
                    break;
                //MikuLiveEvent
                case "95":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC MikuLiveEvent LiveStage {eventId}!");
                    break;
                case "111":
                    if (File.Exists($"{workpath}/eventController/Georgia/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Georgia/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC Georgia {eventId}!");
                    break;

                #region iDOLMASTERs
                case "148":
                    if (File.Exists($"{workpath}/eventController/iDOLMASTERs/LiveEvent/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLMASTERs/LiveEvent/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for Public iDOLMASTERs LiveStage {eventId}!");
                    break;

                case "150":
                    if (File.Exists($"{workpath}/eventController/iDOLMASTERs/SPMoveToSpecialVenue/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLMASTERs/SPMoveToSpecialVenue/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for Public THE IDOLM@STER SP Move to special live venue {eventId}!");
                    break;

                case "155":
                    if (File.Exists($"{workpath}/eventController/iDOLMASTERs/SPMoveToSpecialVenue/qagetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLMASTERs/SPMoveToSpecialVenue/qagetEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for QA THE IDOLM@STER SP Move to special live venue {eventId}!");
                    break;
                case "172":
                    if (File.Exists($"{workpath}/eventController/iDOLMASTERs/SPMoveToSpecialVenue/localgetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLMASTERs/SPMoveToSpecialVenue/localgetEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for LOCAL THE IDOLM@STER SP Move to special live venue {eventId}!");
                    break;

                #endregion

                //RollyCafe1F
                case "174":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/localgetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/localgetEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for LOCAL RollyCafe1F {eventId}!");
                    break;
                case "179":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/qagetEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/qagetEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for QA RollyCafe1F {eventId}!");
                    break;
                //Basara
                case "180":
                    if (File.Exists($"{workpath}/eventController/collabo_iln/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/collabo_iln/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for LOCAL Basara {eventId}!");
                    break;
                case "192":
                    if (File.Exists($"{workpath}/eventController/collabo_iln/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/collabo_iln/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for QA Basara {eventId}!");
                    break;

                //DJMusic
                case "197":
                    if (File.Exists($"{workpath}/eventController/DJMusic/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/DJMusic/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC DJMusic {eventId}!");
                    break;
                case "201":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC RollyCafe1F {eventId}!");
                    break;
                    //Basara
                case "210":
                    if (File.Exists($"{workpath}/eventController/collabo_iln/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/collabo_iln/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC Basara {eventId}!");
                    break;
                //hc_gallery
                case "264":
                    if (File.Exists($"{workpath}/eventController/hc_gallery/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/hc_gallery/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC RollyCafe1F {eventId}!");
                    break;
                case "297":
                    if (File.Exists($"{workpath}/eventController/j_liargame2/getEventTriggerPOST.xml"))
                        return File.ReadAllText($"{workpath}/eventController/j_liargame2/getEventTriggerPOST.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for POST evid PUBLIC LiarGame2 {eventId}!");
                    break;
                case "298":
                    if (File.Exists($"{workpath}/eventController/j_liargame2/getEventTriggerCALL.xml"))
                        return File.ReadAllText($"{workpath}/eventController/j_liargame2/getEventTriggerCALL.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for CALL evid PUBLIC LiarGame2 {eventId}!");
                    break;
                case "346":
                    if (File.Exists($"{workpath}/eventController/AquariumStatue/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/AquariumStatue/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC Sony Aquarium Statue {eventId}!");
                    break;
                case "349":
                    if (File.Exists($"{workpath}/eventController/SonyAquarium/VideoConfig/getEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/SonyAquarium/VideoConfig/getEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - GetEventTrigger sent for PUBLIC Sony Aquarium VideoScreens {eventId}!");
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetEventTrigger unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }

            return null;
        }

        #region getEventTriggerEx
        public static string? getEventTriggerExRequestPOST(byte[] PostData, string ContentType, string workpath, string eventId)
        {
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);
            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    string today = data.GetParameterValue("tday");


                    ms.Flush();
                }
            }

            switch (eventId)
            {
                case "342":
                    string Spring2013 = $"{workpath}/eventController/Spring/2013/getEventTriggerEx.xml";
                    if (File.Exists(Spring2013))
                    {
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FOUND for GetEventTriggerEx POST evid PUBLIC Spring2013 {eventId}!");
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
                        LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - CheckEvent FALLBACK GetEventTriggerEx POST evid PUBLIC Spring2013 {eventId}!\nExpected path {Spring2013}");

                        return "<xml>\r\n" +
                            "<result type=\"int\">1</result>\r\n" +
                            "<description type=\"text\">Success</description>\r\n" +
                            "<error_no type=\"int\">0</error_no>\r\n" +
                            "<error_message type=\"text\">None</error_message>\r\n" +
                            "<trigger_count type=\"int\">200</trigger_count>\r\n" +
                            "<is_active type=\"bool\">true</is_active>\r\n" +
                            "<trigger_id type=\"int\">1</trigger_id>\r\n\r\n" +
                            "<start_year type=\"int\">2023</start_year>\r\n" +
                            "<start_month type=\"int\">09</start_month>\r\n" +
                            "<start_day type=\"int\">12</start_day>\r\n" +
                            "<start_hour type=\"int\">07</start_hour>\r\n" +
                            "<start_minutes type=\"int\">30</start_minutes>\r\n" +
                            "<start_second type=\"int\">0</start_second>\r\n\r\n" +
                            "<end_year type=\"int\">2024</end_year>\r\n" +
                            "<end_month type=\"int\">09</end_month>\r\n" +
                            "<end_day type=\"int\">17</end_day>\r\n" +
                            "<end_hour type=\"int\">00</end_hour>\r\n" +
                            "<end_minutes type=\"int\">30</end_minutes>\r\n" +
                            "<end_second type=\"int\">0</end_second>\r\n\r\n" +
                            "<trigger_flag type=\"int\">1</trigger_flag>\r\n\r\n" +
                            "</xml>";
                    }
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - GetEventTriggerEx unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }
        }
        #endregion

        public static string? confirmEventTriggerRequestPOST(byte[] PostData, string ContentType, string workpath, string eventId)
        {
            switch (eventId)
            {
                case "55":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA MikuLiveJack {eventId}!");
                    break;
                case "63":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA MikuLiveEvent {eventId}!");
                    break;
                case "72":
                    if (File.Exists($"{workpath}/eventController/Rainbow/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Rainbow/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA Rainbow {eventId}!");
                    break;
                case "76":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for Local MikuLiveJack {eventId}!");
                    break;
                case "80":
                    if (File.Exists($"{workpath}/eventController/RollyJukebox/global/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyJukebox/global/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC Rolly Music Survey {eventId}!");
                    break;
                case "90":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveJack {eventId}!");
                    break;
                case "91":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/global/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/global/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveJukebox {eventId}!");
                    break;
                case "93":
                    if (File.Exists($"{workpath}/eventController/PrinnyJack/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/PrinnyJack/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC Prinny {eventId}!");
                    break;
                //MikuLiveEvent
                case "95":
                    if (File.Exists($"{workpath}/eventController/MikuLiveEvent/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveEvent {eventId}!");
                    break;
                case "98":
                    if (File.Exists($"{workpath}/eventController/Rainbow/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Rainbow/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for Rainbow {eventId}!");
                    break;
                //MikuLiveJack
                case "110":
                    if (File.Exists($"{workpath}/eventController/MikuLiveJack/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/MikuLiveJack/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for MikuLiveJack {eventId}!");
                    break;
                case "111":
                    if (File.Exists($"{workpath}/eventController/Georgia/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Georgia/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC Georgia {eventId}!");
                    break;
                case "124":
                    if (File.Exists($"{workpath}/eventController/Rainbow/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Rainbow/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for LOCAL Rainbow {eventId}!");
                    break;
                #region SCEAsia Distribution
                case "138":
                    if (File.Exists($"{workpath}/eventController/Distribution/SCEAsia/lounge/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Distribution/SCEAsia/lounge/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA Free distribution SCEAsia area lounge MiddleFloor {eventId}!");
                    break;
                case "140":
                    if (File.Exists($"{workpath}/eventController/Distribution/SCEAsia/lounge/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Distribution/SCEAsia/lounge/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC Free distribution SCEAsia area lounge MiddleFloor {eventId}!");
                    break;
                #endregion
                //iDOLM@ASTER
                case "148":
                    if (File.Exists($"{workpath}/eventController/iDOLMASTERs/LiveEvent/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLMASTERs/LiveEvent/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for iDOLMASTERs {eventId}!");
                    break;
                case "149":
                    if (File.Exists($"{workpath}/eventController/iDOLMASTERs/EventShop/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLMASTERs/EventShop/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA iDOLM@STERs Event Shop {eventId}!");
                    break;
                case "150":
                    if (File.Exists($"{workpath}/eventController/iDOLMASTERs/SPMoveToSpecialVenue/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLMASTERs/SPMoveToSpecialVenue/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC THE IDOLM@STER SP Move to special live venue {eventId}!");
                    break;
                case "159":
                    if (File.Exists($"{workpath}/eventController/iDOLMASTERs/EventShop/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/iDOLMASTERs/EventShop/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for LOCAL iDOLM@STERs Event Shop {eventId}!");
                    break;
                //DJMusic
                case "197":
                    if (File.Exists($"{workpath}/eventController/DJMusic/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/DJMusic/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC DJMusic {eventId}!");
                    break;
                //hc_gallery
                case "264":
                    if (File.Exists($"{workpath}/eventController/hc_gallery/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/hc_gallery/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC RollyCafe1F {eventId}!");
                    break;
                //RollyCafe1F
                case "174":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for LOCAL RollyCafe1F {eventId}!");
                    break;
                case "179":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA RollyCafe1F {eventId}!");
                    break;
                //Basara
                case "180":
                    if (File.Exists($"{workpath}/eventController/collabo_iln/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/collabo_iln/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA Basara {eventId}!");
                    break;
                case "183":
                    if (File.Exists($"{workpath}/eventController/Macross/SSF/localconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Macross/SSF/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for LOCAL Macross SS F/Fifa Relocator {eventId}!");
                    break;
                //Basara
                case "192":
                    if (File.Exists($"{workpath}/eventController/collabo_iln/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/collabo_iln/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA Basara {eventId}!");
                    break;
                //RollyCafe1F
                case "201":
                    if (File.Exists($"{workpath}/eventController/RollyCafe1F/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC RollyCafe1F {eventId}!");
                    break;
                case "210":
                    if (File.Exists($"{workpath}/eventController/collabo_iln/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/collabo_iln/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC Basara {eventId}!");
                    break;
                case "275":
                    if (File.Exists($"{workpath}/eventController/Macross/SSF    /qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Macross/SSF/localconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA Macross SS F/Fifa Relocator {eventId}!");
                    break;
                case "282":
                    if (File.Exists($"{workpath}/eventController/Macross/VF25/qaconfirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Macross/VF25/qaconfirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for QA Macross VF25 HS {eventId}!");
                    break;
                case "297":
                    if (File.Exists($"{workpath}/eventController/j_liargame2/confirmEventTriggerPOST.xml"))
                        return File.ReadAllText($"{workpath}/eventController/j_liargame2/confirmEventTriggerPOST.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for POST evid PUBLIC LiarGame2 {eventId}!");
                    break;
                case "298":
                    if (File.Exists($"{workpath}/eventController/j_liargame2/confirmEventTriggerCALL.xml"))
                        return File.ReadAllText($"{workpath}/eventController/j_liargame2/confirmEventTriggerCALL.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for CALL evid PUBLIC LiarGame2 {eventId}!");
                    break;
                case "332":
                    if (File.Exists($"{workpath}/eventController/Macross/SSF/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Macross/SSF/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC Macross SS F/Fifa Relocator {eventId}!");
                    break;
                case "335":
                    if (File.Exists($"{workpath}/eventController/Macross/VF25/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/Macross/VF25/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC Macross VF25 HS {eventId}!");
                    break;
                case "346":
                    if (File.Exists($"{workpath}/eventController/AquariumStatue/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/AquariumStatue/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC Sony Aquarium Statue {eventId}!");
                    break;
                case "349":
                    if (File.Exists($"{workpath}/eventController/SonyAquarium/VideoConfig/confirmEventTrigger.xml"))
                        return File.ReadAllText($"{workpath}/eventController/SonyAquarium/VideoConfig/confirmEventTrigger.xml");
                    LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - ConfirmEventTrigger sent for PUBLIC Sony Aquarium VideoScreens {eventId}!");
                    break;
                default:
                    {
                        LoggerAccessor.LogError($"[PREMIUMAGENCY] - ConfirmEventTrigger unhandled for eventId {eventId} | POSTDATA: \n{Encoding.UTF8.GetString(PostData)}");
                        return null;
                    }
            }

            return null;
        }
    }
}
