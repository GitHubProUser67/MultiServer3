using System.Net;

namespace MultiServer.HTTPService.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEECommerce
    {
        public static Task get_count(HttpListenerRequest request, HttpListenerResponse response)
        {
            VEEMEELoginCounter counter = new VEEMEELoginCounter();

            byte[] clientresponse = VEEMEEProcessor.sign("{ \"count\": PUT_NUMBER_HERE }".Replace("PUT_NUMBER_HERE", counter.GetLoginCount("Voodooperson05").ToString()));

            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentLength64 = clientresponse.Length;

            if (response.OutputStream.CanWrite)
            {
                try
                {
                    response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                    response.OutputStream.Close();
                }
                catch (Exception)
                {
                    // Not Important.
                }
            }

            return Task.CompletedTask;
        }

        public static Task get_ownership(HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] clientresponse = VEEMEEProcessor.sign("{ \"owner\": \"Voodooperson05\" }");

            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentLength64 = clientresponse.Length;

            if (response.OutputStream.CanWrite)
            {
                try
                {
                    response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                    response.OutputStream.Close();
                }
                catch (Exception)
                {
                    // Not Important.
                }
            }

            return Task.CompletedTask;
        }
    }
}
