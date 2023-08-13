using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEEData
    {
        public static async Task parkChallenges(HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] clientresponse = VEEMEEProcessor.sign(Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/challenges.json", HTTPPrivateKey.HTTPPrivatekey)));

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
        }

        public static async Task parkTasks(HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] clientresponse = VEEMEEProcessor.sign(Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/tasks.json", HTTPPrivateKey.HTTPPrivatekey)));

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
        }
    }
}
