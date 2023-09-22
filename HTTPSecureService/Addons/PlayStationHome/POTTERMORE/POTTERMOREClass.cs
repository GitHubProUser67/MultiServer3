using System.Net;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.POTTERMORE
{
    public class POTTERMOREClass
    {
        public static void ProcessRequest(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            if (request.Url == null)
            {
                response.SetBegin((int)HttpStatusCode.Forbidden);
                response.SetBody();
                return;
            }

            string url = request.Url;

            switch (request.Method)
            {
                case "GET":
                    switch (url)
                    {
                        case "/api/user":
                            POTTERMOREApiAuth.ApiUserRequest(request, response, Headers);
                            break;
                        default:
                            response.SetBegin((int)HttpStatusCode.Forbidden);
                            response.SetBody();
                            break;
                    }
                    break;
                case "POST":
                    switch (url)
                    {
                        case "/api/auth":
                            POTTERMOREApiAuth.ApiAuthRequest(request, response, Headers);
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
