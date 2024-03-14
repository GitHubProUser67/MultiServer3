using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;

namespace WebUtils.PREMIUMAGENCY
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

                #region EventController Paths
                string basaraCollabEventPath = $"{workpath}/eventController/collabo_iln";


                string homecafeEnquetePath = $"{workpath}/eventController/hc_Enquete";
                string homecafeGalleryPath = $"{workpath}/eventController/hc_gallery";
                string homecafeShopPath = $"{workpath}/eventController/hc_Shop";
                string homecafePlanPath = $"{workpath}/eventController/hc_plan";

                string idolMasterPath = $"{workpath}/eventController/iDOLM@ASTERs";
                string j_liargame2Path = $"{workpath}/eventController/j_liargame2";
                string j_liargame2demoPath = $"{workpath}/eventController/j_liargame2demo";

                string july2009infoboard = $"{workpath}/eventController/infoboard/09";
                string MikuLiveEvent = $"{workpath}/eventController/MikuLiveEvent";
                string MikuLiveJukeboxPath = $"{workpath}/eventController/MikuLiveJukebox";
                string SonyAquariumPath = $"{workpath}/eventController/SonyAquarium";
                string shoeikingdomPath = $"{workpath}/eventController/shoeikingdom";
                string Spring2009 = $"{workpath}/eventController/Spring/2009";
                string WhiteDay2010 = $"{workpath}/eventController/WhiteDay/2010";

                #endregion

                switch (resKey)
                {
                    #region SonyAquarium
                    case "SonyAquarium_Blog":
                        {
                            Directory.CreateDirectory(SonyAquariumPath);
                            string filePath = $"{SonyAquariumPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            } else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }

                        }
                        break;
                    case "SonyAquarium_Config":
                        {
                            Directory.CreateDirectory(SonyAquariumPath);
                            string filePath = $"{SonyAquariumPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region j_LiarGame2
                    case "j_liargame2_screen":
                        {
                            Directory.CreateDirectory(j_liargame2Path);
                            string filePath = $"{j_liargame2Path}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }

                        }
                        break;
                    case "j_liargame2_event":
                        {
                            Directory.CreateDirectory(j_liargame2Path);
                            string filePath = $"{j_liargame2Path}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "j_liargame2_schedule":
                        {
                            Directory.CreateDirectory(j_liargame2Path);
                            string filePath = $"{j_liargame2Path}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "j_liargame2_reward":
                        {
                            Directory.CreateDirectory(j_liargame2Path);
                            string filePath = $"{j_liargame2Path}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;

                    case "j_liargame2_data":
                        {
                            Directory.CreateDirectory(j_liargame2Path);
                            string filePath = $"{j_liargame2Path}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region Liargame2 Demo ver
                    case "j_liargame2demo_screen":
                        {
                            Directory.CreateDirectory(j_liargame2demoPath);
                            string filePath = $"{j_liargame2demoPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region theater_ev
                    case "theater_ev_setting":
                        {
                            Directory.CreateDirectory(shoeikingdomPath);
                            string filePath = $"{shoeikingdomPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region InfoBoards

                    case "jul2009":
                        {
                            Directory.CreateDirectory(july2009infoboard);
                            string filePath = $"{july2009infoboard}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;

                    #endregion


                    #region Spring Events

                    case "spring2009":
                        {
                            Directory.CreateDirectory(Spring2009);
                            string filePath = $"{Spring2009}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }

                        }
                        break;
                    #endregion

                    #region WhiteDay2010
                    case "whiteday_ev_2010":
                        {
                            Directory.CreateDirectory(WhiteDay2010);
                            string filePath = $"{WhiteDay2010}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }

                        }
                        break;
                    #endregion

                    #region MikuLiveEvent
                    case "concert_complete_judge":
                        {
                            Directory.CreateDirectory(MikuLiveEvent);
                            string filePath = $"{MikuLiveEvent}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "concert_play_image":
                        {
                            Directory.CreateDirectory(MikuLiveEvent);
                            string filePath = $"{MikuLiveEvent}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "concert_play_info":
                        {
                            Directory.CreateDirectory(MikuLiveEvent);
                            string filePath = $"{MikuLiveEvent}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "concert_play_sound":
                        {
                            Directory.CreateDirectory(MikuLiveEvent);
                            string filePath = $"{MikuLiveEvent}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "concert_wait_image":
                        {
                            Directory.CreateDirectory(MikuLiveEvent);
                            string filePath = $"{MikuLiveEvent}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "concert_wait_info":
                        {
                            Directory.CreateDirectory(MikuLiveEvent);
                            string filePath = $"{MikuLiveEvent}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "concert_wait_sound":
                        {
                            Directory.CreateDirectory(MikuLiveEvent);
                            string filePath = $"{MikuLiveEvent}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "concert_diva_image":
                        {
                            Directory.CreateDirectory(MikuLiveEvent);
                            string filePath = $"{MikuLiveEvent}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region HomeCafe Questionaire
                    case "hc_Enquete_enable":
                        {
                            Directory.CreateDirectory(homecafeEnquetePath);
                            string filePath = $"{homecafeEnquetePath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "hc_Enquete_id":
                        {
                            Directory.CreateDirectory(homecafeEnquetePath);
                            string filePath = $"{homecafeEnquetePath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "hc_Enquete_reward":
                        {
                            Directory.CreateDirectory(homecafeEnquetePath);
                            string filePath = $"{homecafeEnquetePath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "hc_Enquete":
                        {
                            Directory.CreateDirectory(homecafeEnquetePath);
                            string filePath = $"{homecafeEnquetePath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "hc_Enquete_screen":
                        {
                            Directory.CreateDirectory(homecafeEnquetePath);
                            string filePath = $"{homecafeEnquetePath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region hc_gallery
                    case "hc_gallery_main":
                        {
                            Directory.CreateDirectory(homecafeGalleryPath);
                            string filePath = $"{homecafeGalleryPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "hc_gallery_skysetting":
                        {
                            Directory.CreateDirectory(homecafeGalleryPath);
                            string filePath = $"{homecafeGalleryPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region Homecafe Event Item Shop
                    case "hc_shop":
                        {
                            Directory.CreateDirectory(homecafeShopPath);
                            string filePath = $"{homecafeShopPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region RollyCafe1F Plan
                    case "hc_plan":
                        {
                            Directory.CreateDirectory(homecafeEnquetePath);
                            string filePath = $"{homecafeEnquetePath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "hc_plan_error":
                        {
                            Directory.CreateDirectory(homecafePlanPath);
                            string filePath = $"{homecafePlanPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region MikuMusicSurvey
                    case "miku_jukebox_mp4":
                        {
                            Directory.CreateDirectory(MikuLiveJukeboxPath);
                            string filePath = $"{shoeikingdomPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "miku_jukebox1_mp4":
                        {
                            Directory.CreateDirectory(MikuLiveJukeboxPath);
                            string filePath = $"{shoeikingdomPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "miku_jukebox1_board":
                        {
                            Directory.CreateDirectory(MikuLiveJukeboxPath);
                            string filePath = $"{shoeikingdomPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "miku_jukebox1_info":
                        {
                            Directory.CreateDirectory(MikuLiveJukeboxPath);
                            string filePath = $"{shoeikingdomPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "miku_jukebox1_time":
                        {
                            Directory.CreateDirectory(MikuLiveJukeboxPath);
                            string filePath = $"{shoeikingdomPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region IdolMasters
                    case "liveevent_idolmaster":
                        {
                            Directory.CreateDirectory(idolMasterPath);
                            string filePath = $"{idolMasterPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                return File.ReadAllText(filePath);
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    default:
                        {
                            LoggerAccessor.LogError($"[PREMIUMAGENCY] - Resource key {resKey} is unrecognized!\nPlease contact the developers!");

                            return "<xml>" +
                                   "\r\n\t<result type=\"int\">0</result>" +
                                   "\r\n\t<description type=\"text\">Failed</description>" +
                                   "\r\n\t<error_no type=\"int\">303</error_no>" +
                                   "\r\n\t<error_message type=\"text\">No Resource Found</error_message>" +
                                   $"\r\n\t<key type=\"text\">{resKey}</key>" +
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
