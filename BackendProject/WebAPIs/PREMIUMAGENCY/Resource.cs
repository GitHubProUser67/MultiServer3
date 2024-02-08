using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;

namespace BackendProject.WebAPIs.PREMIUMAGENCY
{
    public class Resource
    {
        public static string? getResourcePOST(byte[]? PostData, string? ContentType, string workpath)
        {
            string resKey = string.Empty;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    resKey = data.GetParameterValue("key");

                    ms.Flush();
                }

                LoggerAccessor.LogInfo($"Attempting to locate resource with key {resKey}");

                switch(resKey)
                {
                    case "theater_ev_setting":
                        {
                            if (File.Exists($"{workpath}/eventController/shoeikingdom/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/shoeikingdom/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "jul2009":
                        {
                            if (File.Exists($"{workpath}/eventController/infoboard/09/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/infoboard/09/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;

                    case "concert_complete_judge":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "concert_play_image":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "concert_play_info":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "concert_play_sound":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "concert_wait_image":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "concert_wait_info":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "concert_wait_sound":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "concert_diva_image":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveEvent/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    //HomeCafe Questionaire
                    case "hc_Enquete_enable":
                        {
                            if (File.Exists($"{workpath}/eventController/hc_Enquete/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/hc_Enquete/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "hc_Enquete_id":
                        {
                            if (File.Exists($"{workpath}/eventController/hc_Enquete/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/hc_Enquete/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "hc_Enquete_reward":
                        {
                            if (File.Exists($"{workpath}/eventController/hc_Enquete/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/hc_Enquete/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "hc_Enquete":
                        {
                            if (File.Exists($"{workpath}/eventController/hc_Enquete/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/hc_Enquete/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "hc_Enquete_screen":
                        {
                            if (File.Exists($"{workpath}/eventController/hc_Enquete/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/hc_Enquete/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    //hc_gallery
                    case "hc_gallery_main":
                        {
                            if (File.Exists($"{workpath}/eventController/hc_gallery/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/hc_gallery/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "hc_gallery_skysetting":
                        {
                            if (File.Exists($"{workpath}/eventController/hc_gallery/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/hc_gallery/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    //Event Item Shop
                    case "hc_shop":
                        {
                            if (File.Exists($"{workpath}/eventController/hc_shop/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/hc_shop/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    //RollyCafe1F
                    case "hc_plan":
                        {
                            if (File.Exists($"{workpath}/eventController/RollyCafe1F/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "hc_plan_error":
                        {
                            if (File.Exists($"{workpath}/eventController/RollyCafe1F/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/RollyCafe1F/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    //Miku Music Survey
                    case "miku_jukebox_mp4":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "miku_jukebox1_mp4":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "miku_jukebox1_board":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "miku_jukebox1_info":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "miku_jukebox1_time":
                        {
                            if (File.Exists($"{workpath}/eventController/MikuLiveJukebox/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/MikuLiveJukebox/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    case "liveevent_idolmaster":
                        {
                            if (File.Exists($"{workpath}/eventController/iDOLM@ASTERs/{resKey}.xml"))
                                return File.ReadAllText($"{workpath}/eventController/iDOLM@ASTERs/{resKey}.xml");

                            LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                        }
                        break;
                    default:
                        {
                            LoggerAccessor.LogError($"[PREMIUMAGENCY] - Resource is missing with resource key {resKey}!");

                            return "<xml>" +
                                   "\r\n\t<result type=\"int\">0</result>" +
                                   "\r\n\t<description type=\"text\">Failed</description>" +
                                   "\r\n\t<error_no type=\"int\">303</error_no>" +
                                   "\r\n\t<error_message type=\"text\">No Resource Found</error_message>" +
                                   $"\r\n\r\n\t<key type=\"text\">{resKey}</key>" +
                                   "\r\n\t<seq type=\"int\">2</seq>" +
                                   "\r\n\t<resource type=\"int\">1</resource>" +
                                   "\r\n\t<data></data>" +
                                   "\r\n</xml>";
                        }
                }
            }

            return null;
        }

        public static string? getInformationBoardSchedulePOST(byte[]? PostData, string? ContentType, string workpath, string eventId)
        {
            string resKey = string.Empty;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    resKey = data.GetParameterValue("key");

                    ms.Flush();
                }

                switch(eventId)
                {
                    default:
                        {
                            LoggerAccessor.LogError($"[PREMIUMAGENCY] - InfoBoardSchedule is unhandled with eventId {eventId} | ResKey {resKey}!");
                            return "<xml></xml>";
                        }
                }
            }

            return null;
        }
    }
}
