using System.Net;
using System.Text;
using MultiServer.HTTPService;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.SSFW
{
    public class POST
    {
        public static async void handlePOST(string directorypath, string filepath, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            if (request.Url == null || request.Url == string.Empty)
                return;

            string absolutepath = request.Url;

            switch (absolutepath)
            {
                case "/bb88aea9-6bf8-4201-a6ff-5d1f8da0dd37/login/token/psn":
                    if (HTTPSClass.GetHeaderValue(Headers, "X-HomeClientVersion") != string.Empty && HTTPSClass.GetHeaderValue(Headers, "general-secret") != string.Empty)
                        SSFWLogin.HandleLogin(request, response, Headers);
                    break;
                default:
                    if (request.Url.EndsWith("/morelife") && HTTPSClass.GetHeaderValue(Headers, "x-signature") != string.Empty)
                    {
                        response.SetBegin((int)HttpStatusCode.OK);
                        response.SetBody("{}");
                    }
                    else if (request.Url.Contains("/AvatarLayoutService/cprod/") && HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                        SSFWAvatarLayoutService.HandleAvatarLayout(directorypath, filepath, absolutepath, request, response, false);
                    else if (request.Url.Contains("/RewardsService/cprod/rewards/") && HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                        SSFWRewardsService.HandleRewardServicePOST(directorypath, filepath, absolutepath, request, response);
                    else if (request.Url.Contains("/RewardsService/trunks-cprod/trunks/") && request.Url.Contains("/setpartial") && HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                        SSFWRewardsService.HandleRewardServiceTrunksPOST(directorypath, filepath, absolutepath, request, response);
                    else if (request.Url.Contains("/RewardsService/trunks-cprod/trunks/") && request.Url.Contains("/set") && HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                        SSFWRewardsService.HandleRewardServiceTrunksEmergencyPOST(directorypath, absolutepath, request, response);
                    else if (request.Url.Contains("/LayoutService/cprod/person/") && HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                        SSFWLayoutService.HandleLayoutServicePOST(directorypath, absolutepath, request, response);
                    else if (HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id") != string.Empty)
                    {
                        // Create a byte array
                        byte[] buffer = request.BodyBytes;

                        ServerConfiguration.LogWarn("[HTTPS - SSFW] : Host requested a POST method I don't know about! - Report it to GITHUB with the request : " + Encoding.UTF8.GetString(buffer));

                        Directory.CreateDirectory(directorypath);

                        if (HTTPSClass.GetHeaderValue(Headers, "Content-Type") == "image/jpeg")
                            await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".jpeg", SSFWPrivateKey.SSFWPrivatekey, buffer, true);
                        else if (HTTPSClass.GetHeaderValue(Headers, "Content-Type") == "application/json")
                            await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".json", SSFWPrivateKey.SSFWPrivatekey, buffer, true);
                        else
                            await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder + absolutepath + ".bin", SSFWPrivateKey.SSFWPrivatekey, buffer, true);


                        response.SetBegin((int)HttpStatusCode.OK);
                        response.SetBody();
                    }
                    else
                    {
                        response.SetBegin((int)HttpStatusCode.Forbidden);
                        response.SetBody();
                    }
                    break;
            }
        }
    }
}
