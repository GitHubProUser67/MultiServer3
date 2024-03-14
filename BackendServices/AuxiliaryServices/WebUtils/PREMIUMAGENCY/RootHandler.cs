using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebUtils.PREMIUMAGENCY
{
    public class RootHandler
    {

        public static string? eventControllerRootHandler(byte[]? PostData, string? ContentType, string workpath) {

            string resKey = string.Empty;
            string seq = string.Empty;

            string evid = string.Empty;

            string? boundary = HTTPUtils.ExtractBoundary(ContentType);
            if (boundary != null && PostData != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    resKey = data.GetParameterValue("key");
                    seq = data.GetParameterValue("seq");


                    evid = data.GetParameterValue("evid");


                    ms.Flush();
                }
            }

            if (resKey != null && seq != null)
            {
               return getRootResourcePOST(PostData, ContentType, workpath);
            } else if(evid != null)
            {
               return Event.checkEventRequestPOST(PostData, ContentType, evid);
            }


            return null;
        }


        public static string? getRootResourcePOST(byte[]? PostData, string? ContentType, string workpath)
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

                #region EventControllerPaths 


                string Spring2009 = $"{workpath}/eventController/Spring/2009";
                string Spring2013 = $"{workpath}/eventController/Spring/2013";

                #endregion


                #region Spring Events
                switch(resKey)
                {
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

                    case "spring2013":
                        {
                            Directory.CreateDirectory(Spring2013);
                            string filePath = $"{Spring2013}/{resKey}.xml";
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

    }






}
