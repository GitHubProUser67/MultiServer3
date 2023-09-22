using System.Net;
using System.Text.RegularExpressions;
using MultiServer.HTTPService;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.SSFW
{
    public class SSFWAvatarLayoutService
    {
        public static async void HandleAvatarLayout(string directorypath, string filepath, string absolutepath, HttpRequest request, HttpResponse response, bool delete)
        {
            // Define the regular expression pattern to match a number at the end of the string
            Regex regex = new Regex(@"\d+(?![\d/])$");

            // Check if the string ends with a number
            if (regex.IsMatch(absolutepath))
            {
                // Get the matched number as a string
                string numberString = regex.Match(absolutepath).Value;

                // Convert the matched number to an integer
                int number;

                // Check if the number is valid
                if (!int.TryParse(numberString, out number))
                {
                    response.SetBegin((int)HttpStatusCode.Forbidden);
                    response.SetBody();
                    return;
                }

                // Create a byte array
                byte[] buffer = request.BodyBytes;

                await SSFWProcessor.SSFWUpdateavatar(directorypath + "/list.json", number, delete);

                if (delete)
                    File.Delete(filepath + ".json");
                else
                {
                    Directory.CreateDirectory(directorypath);

                    await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".json", SSFWPrivateKey.SSFWPrivatekey, buffer, true);
                }

                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody();
            }
            else
            {
                response.SetBegin((int)HttpStatusCode.Forbidden);
                response.SetBody();
            }
        }
    }
}
