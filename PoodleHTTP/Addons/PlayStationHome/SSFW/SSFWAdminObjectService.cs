using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.SSFW
{
    public class SSFWAdminObjectService
    {
        public static async Task HandleAdminObjectService(HttpListenerRequest request, HttpListenerResponse response)
        {
            string sessionid = request.Headers["X-Home-Session-Id"];

            if (File.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SSFWStaticFolder}SSFW_Accounts/{sessionid}.json"))
            {
                string tempcontent = Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.SSFWStaticFolder}SSFW_Accounts/{sessionid}.json", SSFWPrivateKey.SSFWPrivatekey));

                // Parsing JSON data to SSFWUserData object
                SSFWUserData userData = JsonConvert.DeserializeObject<SSFWUserData>(tempcontent);

                if (userData != null)
                {
                    ServerConfiguration.LogInfo($"[SSFW] : IGA Request from : {sessionid} - IGA status : {userData.IGA}");

                    if (userData.IGA == 1)
                    {
                        ServerConfiguration.LogInfo($"[SSFW] : Admin role confirmed for : {sessionid}");

                        response.StatusCode = (int)HttpStatusCode.OK;

                        return;
                    }
                }
            }

            ServerConfiguration.LogError($"[SSFW] Server : {request.UserAgent} requested a IGA access, but no access allowed so we forbid!");

            // Todo : Ban Medius MAC in that case, it's not normal for a user to request IGA without permission.

            response.StatusCode = (int)HttpStatusCode.Forbidden;
        }
    }
}
