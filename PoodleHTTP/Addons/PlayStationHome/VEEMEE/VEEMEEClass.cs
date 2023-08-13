using System.Net;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEEClass
    {
        public static async Task processrequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.Url.LocalPath == null)
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return;
            }

            string absolutepath = request.Url.AbsolutePath.Replace("//", "/");

            string httpMethod = request.HttpMethod;

            switch (httpMethod)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/audiHSGlobalTable.php":
                            await VEEMEEAudiHS.audiHSGlobalTable(request, response);
                            break;
                        case "/audiHSGetUserData.php":
                            await VEEMEEAudiHS.audiHSGetUserData(request, response);
                            break;
                        case "/audiHSSetUserData.php":
                            await VEEMEEAudiHS.audiHSSetUserData(request, response);
                            break;
                        case "/audiHSGetTopUser.php":
                            await VEEMEEAudiHS.audiHSGetTopUser(request, response);
                            break;
                        case "/commerce/get_count.php":
                            await VEEMEECommerce.get_count(request, response);
                            break;
                        case "/commerce/get_ownership.php":
                            await VEEMEECommerce.get_ownership(request, response);
                            break;
                        case "/data/parkChallenges.php":
                            await VEEMEEData.parkChallenges(request, response);
                            break;
                        case "/data/parkTasks.php":
                            await VEEMEEData.parkTasks(request, response);
                            break;
                        case "/slot-management/getobjectslot.php":
                            await VEEMEESlot_management.getobjectslot(request, response);
                            break;
                        case "/slot-management/remove.php":
                            await VEEMEESlot_management.remove(request, response);
                            break;
                        case "/slot-management/heartbeat.php":
                            await VEEMEESlot_management.heartbeat(request, response);
                            break;
                        case "/stats/getconfig.php":
                            await VEEMEEStats.getconfig(false, request, response);
                            break;
                        case "/stats/crash.php":
                            await VEEMEEStats.crash(request, response);
                            break;
                        case "/stats_tracking/usage.php":
                            await VEEMEEStats.usage(request, response);
                            break;
                        case "/storage/readconfig.php":
                            await VEEMEEStorage.readconfig(request, response);
                            break;
                        case "/storage/readtable.php":
                            await VEEMEEStorage.readtable(request, response);
                            break;
                        case "/storage/writetable.php":
                            await VEEMEEStorage.writetable(request, response);
                            break;
                        default:
                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                            break;
                    }
                    break;
                case "GET":
                    switch (absolutepath)
                    {
                        case "/stats/getconfig.php":
                            await VEEMEEStats.getconfig(true, request, response);
                            break;
                        case "/screens.php":
                            byte[] clientresponse = await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Audi_Vrun/SCREENLINKS.XML", HTTPPrivateKey.HTTPPrivatekey);
                            response.ContentType = "text/xml; charset=utf-8";
                            response.StatusCode = (int)HttpStatusCode.OK;
                            if (response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    response.ContentLength64 = clientresponse.Length;
                                    response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                                    response.OutputStream.Close();
                                }
                                catch (Exception)
                                {
                                    // Not Important.
                                }
                            }
                            break;
                        default:
                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                            break;
                    }
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
            }
        }
    }
}
