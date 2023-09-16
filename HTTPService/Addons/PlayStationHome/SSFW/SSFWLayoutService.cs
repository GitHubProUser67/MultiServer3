using System.Net;
using System.Text;

namespace MultiServer.HTTPService.Addons.PlayStationHome.SSFW
{
    public class SSFWLayoutService
    {
        public static async Task HandleLayoutServicePOST(string directorypath, string absolutepath, HttpListenerRequest request, HttpListenerResponse response)
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

                string inputurlfortrim = absolutepath;
                string[] words = inputurlfortrim.Split('/');

                if (words.Length > 0)
                {
                    inputurlfortrim = words[words.Length - 1];
                }

                if (inputurlfortrim != absolutepath) // If ends with UUID Ok.
                {
                    await SSFWProcessor.SSFWfurniturelayout(directorypath + "/mylayout.json", Encoding.UTF8.GetString(buffer), inputurlfortrim);

                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                }

                ms.Dispose();
            }
        }
        public static Task HandleLayoutServiceGET(string directorypath, string absolutepath, HttpListenerRequest request, HttpListenerResponse response)
        {
            string inputurlfortrim = absolutepath;
            string[] words = inputurlfortrim.Split('/');

            if (words.Length > 0)
            {
                inputurlfortrim = words[words.Length - 1];
            }

            if (inputurlfortrim != absolutepath) // If ends with UUID Ok.
            {

                string stringlayout = SSFWProcessor.SSFWgetfurniturelayout(directorypath + "/mylayout.json", inputurlfortrim);

                if (stringlayout != "")
                {
                    byte[] layout = Encoding.UTF8.GetBytes("[{\"PUT_SCENEID_HERE\":PUT_LAYOUT_HERE}]".Replace("PUT_SCENEID_HERE", inputurlfortrim).Replace("PUT_LAYOUT_HERE", stringlayout));

                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.ContentType = "application/json";

                    if (response.OutputStream.CanWrite)
                    {
                        try
                        {
                            response.ContentLength64 = layout.Length;
                            response.OutputStream.Write(layout, 0, layout.Length);
                            response.OutputStream.Close();
                        }
                        catch (Exception)
                        {
                            // Not Important
                        }
                    }
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            return Task.CompletedTask;
        }
    }
}
