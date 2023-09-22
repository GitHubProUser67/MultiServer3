using System.Net;
using System.Text;
using NetCoreServer;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.SSFW
{
    public class SSFWLayoutService
    {
        public static async void HandleLayoutServicePOST(string directorypath, string absolutepath, HttpRequest request, HttpResponse response)
        {
            // Create a byte array
            byte[] buffer = request.BodyBytes;

            Directory.CreateDirectory(directorypath);

            string inputurlfortrim = absolutepath;
            string[] words = inputurlfortrim.Split('/');

            if (words.Length > 0)
                inputurlfortrim = words[words.Length - 1];

            if (inputurlfortrim != absolutepath) // If ends with UUID Ok.
            {
                await SSFWProcessor.SSFWfurniturelayout(directorypath + "/mylayout.json", Encoding.UTF8.GetString(buffer), inputurlfortrim);

                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody();
            }
            else
            {
                response.SetBegin((int)HttpStatusCode.Forbidden);
                response.SetBody();
            }
        }

        public static void HandleLayoutServiceGET(string directorypath, string absolutepath, HttpRequest request, HttpResponse response)
        {
            string inputurlfortrim = absolutepath;
            string[] words = inputurlfortrim.Split('/');

            if (words.Length > 0)
                inputurlfortrim = words[words.Length - 1];

            if (inputurlfortrim != absolutepath) // If ends with UUID Ok.
            {

                string stringlayout = SSFWProcessor.SSFWgetfurniturelayout(directorypath + "/mylayout.json", inputurlfortrim);

                if (stringlayout != "")
                {
                    response.SetBegin((int)HttpStatusCode.OK);
                    response.SetContentType("application/json");
                    response.SetBody("[{\"PUT_SCENEID_HERE\":PUT_LAYOUT_HERE}]".Replace("PUT_SCENEID_HERE", inputurlfortrim).Replace("PUT_LAYOUT_HERE", stringlayout));
                }
                else
                {
                    response.SetBegin((int)HttpStatusCode.NotFound);
                    response.SetBody();
                }
            }
            else
            {
                response.SetBegin((int)HttpStatusCode.Forbidden);
                response.SetBody();
            }
        }
    }
}
