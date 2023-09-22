using System.Net;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEECommerce
    {
        public static void get_count(HttpResponse response)
        {
            VEEMEELoginCounter counter = new VEEMEELoginCounter();
            response.SetBegin((int)HttpStatusCode.OK);
            response.SetBody(VEEMEEProcessor.sign("{ \"count\": PUT_NUMBER_HERE }".Replace("PUT_NUMBER_HERE", counter.GetLoginCount("Voodooperson05").ToString())));
        }

        public static void get_ownership(HttpResponse response)
        {
            response.SetBegin((int)HttpStatusCode.OK);
            response.SetBody(VEEMEEProcessor.sign("{ \"owner\": \"Voodooperson05\" }"));
        }
    }
}
