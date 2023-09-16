using System.Net;
using System.Text;

namespace MultiServer.HTTPService.Addons.PlayStationHome.SSFW
{
    public class SSFWRewardsService
    {
        public static async Task HandleRewardServicePOST(string directorypath, string filepath, string absolutepath, HttpListenerRequest request, HttpListenerResponse response)
        {
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

                Directory.CreateDirectory(directorypath);

                await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".json", SSFWPrivateKey.SSFWPrivatekey, buffer, true);

                await SSFWProcessor.SSFWupdatemini(filepath + "/mini.json", Encoding.UTF8.GetString(buffer));

                response.StatusCode = (int)HttpStatusCode.OK;
                response.ContentType = "application/json";
                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = contentLength;
                        response.OutputStream.Write(buffer, 0, contentLength);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }
                }

                ms.Dispose();
            }
        }

        public static async Task HandleRewardServiceTrunksPOST(string directorypath, string filepath, string absolutepath, HttpListenerRequest request, HttpListenerResponse response)
        {
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

                Directory.CreateDirectory(directorypath);

                await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".json", SSFWPrivateKey.SSFWPrivatekey, buffer, true);

                await SSFWProcessor.SSFWtrunkserviceprocess(filepath.Replace("/setpartial", "") + ".json", Encoding.UTF8.GetString(buffer));

                ms.Dispose();
            }

            response.StatusCode = (int)HttpStatusCode.OK;
        }

        public static async Task HandleRewardServiceTrunksEmergencyPOST(string directorypath, string filepath, string absolutepath, HttpListenerRequest request, HttpListenerResponse response)
        {
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

                Directory.CreateDirectory(directorypath);

                await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".json", SSFWPrivateKey.SSFWPrivatekey, buffer, true);

                ms.Dispose();
            }

            response.StatusCode = (int)HttpStatusCode.OK;
        }
    }
}
