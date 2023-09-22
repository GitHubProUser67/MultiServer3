using System.Net;
using NetCoreServer;
using MultiServer.HTTPService;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEEClass
    {
        public static void ProcessRequest(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            if (request.Url == null)
            {
                response.SetBegin((int)HttpStatusCode.Forbidden);
                response.SetBody();
                return;
            }

            string absolutepath = request.Url.Replace("//", "/");

            string httpMethod = request.Method;

            switch (httpMethod)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/commerce/get_count.php":
                            VEEMEECommerce.get_count(response);
                            break;
                        case "/commerce/get_ownership.php":
                            VEEMEECommerce.get_ownership(response);
                            break;
                        case "/data/parkChallenges.php":
                            VEEMEEData.parkChallenges(response);
                            break;
                        case "/data/parkTasks.php":
                            VEEMEEData.parkTasks(response);
                            break;
                        case "/slot-management/getobjectslot.php":
                            VEEMEESlot_management.getobjectslot(request, response, Headers);
                            break;
                        case "/slot-management/remove.php":
                            VEEMEESlot_management.remove(request, response, Headers);
                            break;
                        case "/slot-management/heartbeat.php":
                            VEEMEESlot_management.heartbeat(request, response, Headers);
                            break;
                        case "/stats/getconfig.php":
                            VEEMEEStats.getconfig(false, request, response, Headers);
                            break;
                        case "/stats/crash.php":
                            VEEMEEStats.crash(request, response, Headers);
                            break;
                        case "/stats_tracking/usage.php":
                            VEEMEEStats.Usage(request, response, Headers);
                            break;
                        case "/storage/readconfig.php":
                            VEEMEEStorage.readconfig(request, response, Headers);
                            break;
                        case "/storage/readtable.php":
                            VEEMEEStorage.readtable(request, response, Headers);
                            break;
                        case "/storage/writetable.php":
                            VEEMEEStorage.writetable(request, response, Headers);
                            break;
                        default:
                            response.SetBegin((int)HttpStatusCode.Forbidden);
                            response.SetBody();
                            break;
                    }
                    break;
                case "GET":
                    switch (absolutepath)
                    {
                        case "/stats/getconfig.php":
                            VEEMEEStats.getconfig(true, request, response, Headers);
                            break;
                        case "/screens.php":
                            byte[] clientresponse = FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Audi_Vrun/SCREENLINKS.XML", HTTPPrivateKey.HTTPPrivatekey);
                            if (clientresponse != null)
                            {
                                response.SetBegin((int)HttpStatusCode.Forbidden);
                                response.SetContentType("text/xml; charset=utf-8");
                                response.SetBody(clientresponse);
                            }
                            else
                                response.SetBegin((int)HttpStatusCode.InternalServerError);
                            response.SetBody();
                            break;
                        default:
                            response.SetBegin((int)HttpStatusCode.Forbidden);
                            response.SetBody();
                            break;
                    }
                    break;
                default:
                    response.SetBegin((int)HttpStatusCode.Forbidden);
                    response.SetBody();
                    break;
            }
        }
    }
}
