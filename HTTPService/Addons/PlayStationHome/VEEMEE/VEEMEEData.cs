using System.Net;
using System.Text;

namespace MultiServer.HTTPService.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEEData
    {
        public static async Task parkChallenges(HttpListenerRequest request, HttpListenerResponse response)
        {
            string parkchallengesdata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/challenges.json", HTTPPrivateKey.HTTPPrivatekey));

            if (parkchallengesdata != null)
            {
                byte[] clientresponse = VEEMEEProcessor.sign(parkchallengesdata);

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
            }
            else
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        public static async Task parkTasks(HttpListenerRequest request, HttpListenerResponse response)
        {
            string parktasksdata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/tasks.json", HTTPPrivateKey.HTTPPrivatekey));

            if (parktasksdata != null)
            {
                byte[] clientresponse = VEEMEEProcessor.sign(parktasksdata);

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
            }
            else
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
}
