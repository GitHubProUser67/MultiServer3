using System.Net;
using System.Text;
using NetCoreServer;
using MultiServer.HTTPService;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEEData
    {
        public static void parkChallenges(HttpResponse response)
        {
            string parkchallengesdata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/challenges.json", HTTPPrivateKey.HTTPPrivatekey));

            if (parkchallengesdata != null)
            {
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody(VEEMEEProcessor.sign(parkchallengesdata));
            }
            else
            {
                response.SetBegin((int)HttpStatusCode.InternalServerError);
                response.SetBody();
            }
        }

        public static void parkTasks(HttpResponse response)
        {
            string parktasksdata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/tasks.json", HTTPPrivateKey.HTTPPrivatekey));

            if (parktasksdata != null)
            {
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody(VEEMEEProcessor.sign(parktasksdata));
            }
            else
            {
                response.SetBegin((int)HttpStatusCode.InternalServerError);
                response.SetBody();
            }
        }
    }
}
