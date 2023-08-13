using HttpMultipartParser;
using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.NDREAMS
{
    public class xi2
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

        public async static Task xi2_cont(HttpListenerRequest request, HttpListenerResponse response)
        {
            string territory = "";

            string func = "";

            string name = "";

            string region = "";

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

                func = data.GetParameterValue("func");

                name = data.GetParameterValue("name");

                region = data.GetParameterValue("region");

                key = data.GetParameterValue("key");

                byte[] responsetooutput = Encoding.UTF8.GetBytes("<GameDay>0</GameDay><GameDayProgress>0</GameDayProgress><NextDay>1</NextDay><Idx>1</Idx><UTId>1</UTId>" +
                    "<Hash>356a192b7913b04c54574d18c28d46e6395428ab</Hash><Stats></Stats>");

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

        public async static Task articles_cont(HttpListenerRequest request, HttpListenerResponse response)
        {
            string func = "";

            string name = "";

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

                func = data.GetParameterValue("func");

                name = data.GetParameterValue("name");

                key = data.GetParameterValue("key");

                byte[] responsetooutput = Encoding.UTF8.GetBytes("<email>test@yopmail.com</email><docs>article</docs><videos>http://127.0.0.1/vid.mp4</videos><avatars>Me</avatars><quinta>something</quinta>");

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

        public async static Task battle_cont(HttpListenerRequest request, HttpListenerResponse response)
        {
            string func = "";

            string name = "";

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

                func = data.GetParameterValue("func");

                name = data.GetParameterValue("name");

                key = data.GetParameterValue("key");

                byte[] responsetooutput = Encoding.UTF8.GetBytes("<Data>1</Data><Hash>356a192b7913b04c54574d18c28d46e6395428ab</Hash><Missions>0</Missions><Wins>0</Wins><Lost>0</Lost>" +
                    "<Best>0</Best><Avg>0</Avg><Conn>0</Conn><Quits>0</Quits><Packs>0</Packs>");

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
