using System.Net;
using System.Text;
using MultiServer.HTTPService;
using NetCoreServer;
using Newtonsoft.Json;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.SSFW
{
    public class SSFWAdminObjectService
    {
        public static void HandleAdminObjectService(HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string sessionid = HTTPSClass.GetHeaderValue(Headers, "X-Home-Session-Id");

            if (File.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SSFWStaticFolder}SSFW_Accounts/{sessionid}.json"))
            {
                string tempcontent = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SSFWStaticFolder}SSFW_Accounts/{sessionid}.json", SSFWPrivateKey.SSFWPrivatekey));

                if (tempcontent != null)
                {
                    // Parsing JSON data to SSFWUserData object
                    SSFWUserData userData = JsonConvert.DeserializeObject<SSFWUserData>(tempcontent);

                    if (userData != null)
                    {
                        ServerConfiguration.LogInfo($"[SSFW] : IGA Request from : {sessionid} - IGA status : {userData.IGA}");

                        if (userData.IGA == 1)
                        {
                            ServerConfiguration.LogInfo($"[SSFW] : Admin role confirmed for : {sessionid}");

                            response.SetBegin((int)HttpStatusCode.OK);
                            response.SetBody();

                            return;
                        }
                    }
                }
            }

            ServerConfiguration.LogError($"[SSFW] Server : {HTTPSClass.GetHeaderValue(Headers, "User-Agent")} requested a IGA access, but no access allowed so we forbid!");

            // Todo : Ban Medius MAC in that case, it's not normal for a user to request IGA without permission.

            response.SetBegin((int)HttpStatusCode.Forbidden);
            response.SetBody();
        }
    }
}
