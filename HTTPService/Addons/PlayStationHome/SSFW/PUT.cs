using System.Net;

namespace MultiServer.HTTPService.Addons.PlayStationHome.SSFW
{
    public class PUT
    {
        public static async void handlePUT(string directoryPath, HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.Url.AbsolutePath == null)
            {
                return;
            }

            string absolutepath = request.Url.AbsolutePath;

            Directory.CreateDirectory(directoryPath);

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

                switch (request.ContentType)
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

                ms.Dispose();
            }

            response.StatusCode = (int)HttpStatusCode.OK;
        }
    }
}
