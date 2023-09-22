using System.Net;
using MultiServer.HTTPService;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.SSFW
{
    public class PUT
    {
        public static async void handlePUT(string directoryPath, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string absolutepath = request.Url;

            Directory.CreateDirectory(directoryPath);

            // Create a byte array
            byte[] buffer = request.BodyBytes;

            switch (HTTPSClass.GetHeaderValue(Headers, "Content-type"))
            {
                case "image/jpeg":
                    await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".jpeg", SSFWPrivateKey.SSFWPrivatekey, buffer, true);
                    break;
                case "application/json":
                    await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".json", SSFWPrivateKey.SSFWPrivatekey, buffer, true);
                    break;
                default:
                    await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".bin", SSFWPrivateKey.SSFWPrivatekey, buffer, true);
                    break;

            }

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetBody();
        }
    }
}
