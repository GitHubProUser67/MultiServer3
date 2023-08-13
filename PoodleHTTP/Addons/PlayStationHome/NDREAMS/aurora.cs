using HttpMultipartParser;
using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.NDREAMS
{
    public class aurora
    {
        public async static Task PSStats(HttpListenerRequest request, HttpListenerResponse response)
        {
            string func = "";

            string name = "";

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

                func = data.GetParameterValue("func");

                name = data.GetParameterValue("name");

                byte[] responsetooutput = Encoding.UTF8.GetBytes("<xml></xml>");

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
