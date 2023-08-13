using HttpMultipartParser;
using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.NDREAMS
{
    public class blueprinthome
    {
        public async static Task blueprint_furniture(HttpListenerRequest request, HttpListenerResponse response)
        {
            string territory = "";

            string name = "";

            string style = "";

            string owner = "";

            string func = "";

            string key = "";

            string boundary = Extensions.ExtractBoundary(request.ContentType);

            // Get the input stream from the context
            Stream inputStream = request.InputStream;

            // Create a memory stream to copy the content
            using (MemoryStream copyStream = new MemoryStream())
            {
                // Copy the input stream to the memory stream
                inputStream.CopyTo(copyStream);

                // Reset the position of the copy stream to the beginning
                copyStream.Position = 0;

                var data = MultipartFormDataParser.Parse(copyStream, boundary);

                // Reset the position of the copy stream to the beginning
                copyStream.Position = 0;

                territory = data.GetParameterValue("territory");

                name = data.GetParameterValue("name");

                style = data.GetParameterValue("style");

                owner = data.GetParameterValue("owner");

                func = data.GetParameterValue("func");

                key = data.GetParameterValue("key");

                byte[] responsetooutput = Encoding.UTF8.GetBytes("<XML></XML>");

                response.StatusCode = (int)HttpStatusCode.OK;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = responsetooutput.Length;
                        response.OutputStream.Write(responsetooutput, 0, responsetooutput.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                }

                copyStream.Dispose();
            }
        }
    }
}
