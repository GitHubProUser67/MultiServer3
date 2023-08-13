using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.SSFW
{
    public class POST
    {
        public static async Task handlePOST(string directorypath, string filepath, HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.Url.AbsolutePath == null)
            {
                return;
            }

            string absolutepath = request.Url.AbsolutePath;

            switch (absolutepath)
            {
                case "/bb88aea9-6bf8-4201-a6ff-5d1f8da0dd37/login/token/psn":
                    if (request.Headers["X-HomeClientVersion"] != null && request.Headers["general-secret"] != null)
                        await SSFWLogin.HandleLogin(request, response);
                        break;
                default:
                    if (request.Url.AbsolutePath.EndsWith("/morelife") && request.Headers["x-signature"] != null)
                    {
                        byte[] moreliferesponse = Encoding.UTF8.GetBytes("{}");
                        response.StatusCode = (int)HttpStatusCode.OK;
                        if (response.OutputStream.CanWrite)
                        {
                            try
                            {
                                response.ContentLength64 = moreliferesponse.Length;
                                response.OutputStream.Write(moreliferesponse, 0, moreliferesponse.Length);
                                response.OutputStream.Close();
                            }
                            catch (Exception)
                            {
                                // Not Important
                            }
                        }
                    }
                    else if (request.Url.AbsolutePath.Contains("/AvatarLayoutService/cprod/") && request.Headers["X-Home-Session-Id"] != null)
                        await SSFWAvatarLayoutService.HandleAvatarLayout(directorypath, filepath, absolutepath, request, response, false);
                    else if (request.Url.AbsolutePath.Contains("/RewardsService/cprod/rewards/") && request.Headers["X-Home-Session-Id"] != null)
                        await SSFWRewardsService.HandleRewardServicePOST(directorypath, filepath, absolutepath, request, response);
                    else if (request.Url.AbsolutePath.Contains("/RewardsService/trunks-cprod/trunks/") && request.Url.AbsolutePath.Contains("/setpartial") && request.Headers["X-Home-Session-Id"] != null)
                        await SSFWRewardsService.HandleRewardServiceTrunksPOST(directorypath, filepath, absolutepath, request, response);
                    else if (request.Url.AbsolutePath.Contains("/RewardsService/trunks-cprod/trunks/") && request.Url.AbsolutePath.Contains("/set") && request.Headers["X-Home-Session-Id"] != null)
                        await SSFWRewardsService.HandleRewardServiceTrunksEmergencyPOST(directorypath, filepath, absolutepath, request, response);
                    else if (request.Url.AbsolutePath.Contains("/LayoutService/cprod/person/") && request.Headers["X-Home-Session-Id"] != null)
                        await SSFWLayoutService.HandleLayoutServicePOST(directorypath, absolutepath, request, response);
                    else if (request.Headers["X-Home-Session-Id"] != null)
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

                            ServerConfiguration.LogWarn("[SSFW] : Host requested a POST method I don't know about! - Report it to GITHUB with the request : " + Encoding.UTF8.GetString(buffer));

                            Directory.CreateDirectory(directorypath);

                            if (request.ContentType == "image/jpeg")
                                await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".jpeg", SSFWPrivateKey.SSFWPrivatekey, buffer);
                            else if (request.ContentType == "application/json")
                                await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".json", SSFWPrivateKey.SSFWPrivatekey, buffer);
                            else
                                await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".bin", SSFWPrivateKey.SSFWPrivatekey, buffer);

                            ms.Dispose();
                        }

                        response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
            }

        }
    }
}
