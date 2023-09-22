using System.Net;
using System.Text;
using MultiServer.HTTPService;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.SSFW
{
    public class SSFWRewardsService
    {
        public static async void HandleRewardServicePOST(string directorypath, string filepath, string absolutepath, HttpRequest request, HttpResponse response)
        {
            // Create a byte array
            byte[] buffer = request.BodyBytes;

            Directory.CreateDirectory(directorypath);

            await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".json", SSFWPrivateKey.SSFWPrivatekey, buffer, true);

            await SSFWProcessor.SSFWupdatemini(filepath + "/mini.json", Encoding.UTF8.GetString(buffer));

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetContentType("application/json");
            response.SetBody(buffer);
        }

        public static async void HandleRewardServiceTrunksPOST(string directorypath, string filepath, string absolutepath, HttpRequest request, HttpResponse response)
        {
            // Create a byte array
            byte[] buffer = request.BodyBytes;

            Directory.CreateDirectory(directorypath);

            await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".json", SSFWPrivateKey.SSFWPrivatekey, buffer, true);

            await SSFWProcessor.SSFWtrunkserviceprocess(filepath.Replace("/setpartial", "") + ".json", Encoding.UTF8.GetString(buffer));

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetBody();
        }

        public static async void HandleRewardServiceTrunksEmergencyPOST(string directorypath, string absolutepath, HttpRequest request, HttpResponse response)
        {
            // Create a byte array
            byte[] buffer = request.BodyBytes;

            Directory.CreateDirectory(directorypath);

            await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".json", SSFWPrivateKey.SSFWPrivatekey, buffer, true);

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetBody();
        }
    }
}
