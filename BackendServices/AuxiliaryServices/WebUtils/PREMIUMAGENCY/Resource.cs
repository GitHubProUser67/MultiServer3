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
                string avatarfx = $"{workpath}/eventController/hs/avatarfx";
                string basaraCollabEventPath = $"{workpath}/eventController/collabo_iln";

                string DemoAsset_Settings130222 = $"{workpath}/eventController/hs/DemoAssets/130222";
                string GundamPath = $"{workpath}/eventController/Gundam";
                string listenerfx = $"{workpath}/eventController/hs/listnerfx";

                
                string homecafeEnquetePath = $"{workpath}/eventController/hc_Enquete";
                string homecafeGalleryPath = $"{workpath}/eventController/hc_gallery";
                string homecafeShopPath = $"{workpath}/eventController/hc_Shop";
                string homecafePlanPath = $"{workpath}/eventController/hc_plan";

                string hs_commu_ev_cfg_130222 = $"{workpath}/eventController/hs/CommEvent/130222";
                string hs_userinfo_cfg = $"{workpath}/eventController/hs/UserInfo";

                string idolMasterPath = $"{workpath}/eventController/iDOLM@ASTERs";

                string j_liargame2Path = $"{workpath}/eventController/j_liargame2";
                string j_liargame2demoPath = $"{workpath}/eventController/j_liargame2demo";

                string july2009infoboard = $"{workpath}/eventController/infoboard/09";
                string MikuLiveEvent = $"{workpath}/eventController/MikuLiveEvent";
                string MikuLiveJukeboxPath = $"{workpath}/eventController/MikuLiveJukebox/Resources";
                string RollyJukeboxPath = $"{workpath}/eventController/RollyJukebox/Resources";
                string SonyAquariumPath = $"{workpath}/eventController/SonyAquarium";
                string shoeikingdomPath = $"{workpath}/eventController/ShoeiKingdom";

                string Spring2009 = $"{workpath}/eventController/Spring/2009";
                string Spring2013 = $"{workpath}/eventController/Spring/2013";

                string WhiteDay2010 = $"{workpath}/eventController/WhiteDay/2010";
                string WhiteDay2013 = $"{workpath}/eventController/WhiteDay/2013";
                string HSAquariumStatuePath = $"{workpath}/eventController/AquariumStatue";
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "SonyAquarium_VideoConfig":
                        {
                            Directory.CreateDirectory(SonyAquariumPath);
                            string filePath = $"{SonyAquariumPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion


                    #region avatarfx
                    case "avatarfx":
                        {
                            Directory.CreateDirectory(avatarfx);
                            string filePath = $"{avatarfx}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }

                        }
                        break;
                    #endregion

                    #region CommunicationEvents
                    case "hs_commu_ev_cfg_130222":
                        {
                            Directory.CreateDirectory(hs_commu_ev_cfg_130222);
                            string filePath = $"{hs_commu_ev_cfg_130222}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }

                        }
                        break;
                    #endregion

                    #region hs_userinfo_cfg
                    case "hs_userinfo_cfg":
                        {
                            Directory.CreateDirectory(hs_userinfo_cfg);
                            string filePath = $"{hs_userinfo_cfg}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }

                        }
                        break;
                    #endregion

                    #region listenerfx
                    case "listenerfx":
                        {
                            Directory.CreateDirectory(listenerfx);
                            string filePath = $"{listenerfx}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }

                        }
                        break;
                    #endregion

                    #region DemoAssets
                    case "DemoAsset_Settings130222":
                        {
                            Directory.CreateDirectory(DemoAsset_Settings130222);
                            string filePath = $"{DemoAsset_Settings130222}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region Basara
                    case "collabo_iln_def_scr":
                        {
                            Directory.CreateDirectory(basaraCollabEventPath);
                            string filePath = $"{basaraCollabEventPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;

                    case "collabo_iln":
                        {
                            Directory.CreateDirectory(basaraCollabEventPath);
                            string filePath = $"{basaraCollabEventPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region Gundam
                    case "jhome_square_gundam_time":
                        {
                            Directory.CreateDirectory(GundamPath);
                            string filePath = $"{GundamPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;

                    #endregion

                    #region Sony Aquarium Home Square
                    case "SonyAquarium_Relocate":
                        {
                            Directory.CreateDirectory(HSAquariumStatuePath);
                            string filePath = $"{HSAquariumStatuePath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }

                        }
                        break;


                    case "spring2013":
                        {
                            Directory.CreateDirectory(Spring2013);
                            string filePath = $"{Spring2013}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                            string filePath = $"{MikuLiveJukeboxPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                            string filePath = $"{MikuLiveJukeboxPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                            string filePath = $"{MikuLiveJukeboxPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                            string filePath = $"{MikuLiveJukeboxPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                            string filePath = $"{MikuLiveJukeboxPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    #endregion

                    #region Rolly Jukebox
                    case "rolly_jukebox1_mp4":
                        {
                            Directory.CreateDirectory(RollyJukeboxPath);
                            string filePath = $"{RollyJukeboxPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "rolly_jukebox1_info":
                        {
                            Directory.CreateDirectory(RollyJukeboxPath);
                            string filePath = $"{RollyJukeboxPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "rolly_jukebox1_board":
                        {
                            Directory.CreateDirectory(RollyJukeboxPath);
                            string filePath = $"{RollyJukeboxPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
                            }
                            else
                            {
                                LoggerAccessor.LogError($"[PREMIUMAGENCY] - Failed to find resource {resKey} with expected path {filePath}!");
                            }
                        }
                        break;
                    case "rolly_jukebox1_time":
                        {
                            Directory.CreateDirectory(RollyJukeboxPath);
                            string filePath = $"{RollyJukeboxPath}/{resKey}.xml";
                            if (File.Exists(filePath))
                            {
                                LoggerAccessor.LogInfo($"[PREMIUMAGENCY] - Resource with resource key {resKey} found and sent!");
                                string res = File.ReadAllText(filePath);

                                string resourceXML = "<xml>\r\n" +
                                    "<result type=\"int\">1</result>\r\n" +
                                    "<description type=\"text\">Success</description>\r\n" +
                                    "<error_no type=\"int\">0</error_no>\r\n" +
                                    "<error_message type=\"text\">None</error_message>\r\n" +
                                    $"<key type=\"text\">{resKey}</key>\r\n" +
                                    $"{res}\r\n" +
                                    "</xml>";

                                return resourceXML;
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

                            return "<xml>\r\n" +
                                "<result type=\"int\">1</result>\r\n" +
                                "<description type=\"text\">Success</description>\r\n" +
                                "<error_no type=\"int\">0</error_no>\r\n" +
                                "<error_message type=\"text\">None</error_message>\r\n" +
                                "<key type=\"text\"></key>\r\n" +
                                "<resource>\r\n" +
                                "<seq type=\"int\">1</seq>\r\n" +
                                "<data type=\"text\"></data>\r\n" +
                                "</resource>\r\n" +
                                "</xml>";
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

                switch (eventId)
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