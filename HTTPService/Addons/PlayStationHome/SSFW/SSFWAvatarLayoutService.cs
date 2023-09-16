using System.Net;
using System.Text.RegularExpressions;

namespace MultiServer.HTTPService.Addons.PlayStationHome.SSFW
{
    public class SSFWAvatarLayoutService
    {
        public static async Task HandleAvatarLayout(string directorypath, string filepath, string absolutepath, HttpListenerRequest request, HttpListenerResponse response, bool delete)
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
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    request.InputStream.CopyTo(ms);

                    // Reset the memory stream position to the beginning
                    ms.Position = 0;

                    // Find the number of bytes in the stream
                    int contentLength = (int)ms.Length;

                    // Create a byte array
                    byte[] buffer = new byte[contentLength];

                    // Read the contents of the memory stream into the byte array
                    ms.Read(buffer, 0, contentLength);

                    await SSFWProcessor.SSFWUpdateavatar(directorypath + "/list.json", number, delete);

                    if (delete)
                    {
                        File.Delete(filepath + ".json");
                    }
                    else
                    {
                        Directory.CreateDirectory(directorypath);

                        await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".json", SSFWPrivateKey.SSFWPrivatekey, buffer, true);
                    }

                    ms.Dispose();
                }

                response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
