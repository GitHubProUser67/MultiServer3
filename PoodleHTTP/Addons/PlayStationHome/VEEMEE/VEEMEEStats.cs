using HttpMultipartParser;
using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEEStats
    {
        public static async Task getconfig(bool get, HttpListenerRequest request, HttpListenerResponse response)
        {
            if (!get)
            {
                string id = "";

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

                    id = data.GetParameterValue("id");

                    copyStream.Dispose();
                }

                Console.WriteLine($"VEEMEE Server : Getconfig values : id|{id}");
            }

            byte[] clientresponse = VEEMEEProcessor.sign(Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/stats_config.json", HTTPPrivateKey.HTTPPrivatekey)));

            response.StatusCode = (int)HttpStatusCode.OK;

            if (response.OutputStream.CanWrite)
            {
                try
                {
                    response.ContentLength64 = clientresponse.Length;
                    response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                    response.OutputStream.Close();
                }
                catch (Exception)
                {
                    // Not Needed.
                }
            }
        }

        public static async Task crash(HttpListenerRequest request, HttpListenerResponse response)
        {
            string corehook = "";
            string territory = "";
            string region = "";
            string psnid = "";
            string scene = "";
            string sceneid = "";
            string scenetime = "";
            string sceneowner = "";
            string owner = "";
            string owned = "";
            string crash = "";
            string numplayers = "";
            string numpeople = "";
            string objectid = "";
            string objectname = "";

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

                corehook = data.GetParameterValue("corehook");

                territory = data.GetParameterValue("territory");

                region = data.GetParameterValue("region");

                psnid = data.GetParameterValue("psnid");

                scene = data.GetParameterValue("scene");

                sceneid = data.GetParameterValue("sceneid");

                scenetime = data.GetParameterValue("scenetime");

                sceneowner = data.GetParameterValue("sceneowner");

                owner = data.GetParameterValue("owner");

                owned = data.GetParameterValue("owned");

                crash = data.GetParameterValue("crash");

                numplayers = data.GetParameterValue("numplayers");

                numpeople = data.GetParameterValue("numpeople");

                objectid = data.GetParameterValue("objectid");

                objectname = data.GetParameterValue("objectname");

                copyStream.Dispose();
            }

            Console.WriteLine($"[VEEMEE] : A Client Crash Happened - issued by {request.UserAgent} - Details : corehook|{corehook} - territory|{territory} - region|{region} - psnid|{psnid}" +
                $" - scene|{scene} - sceneid|{sceneid} - scenetime|{scenetime} - sceneowner|{sceneowner} - owner|{owner} - owned|{owned} - crash|{crash} - numplayers|{numplayers} - numpeople|{numpeople} - objectid|{objectid} - objectname|{objectname}");

            byte[] clientresponse = VEEMEEProcessor.sign(Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/stats_config.json", HTTPPrivateKey.HTTPPrivatekey)));

            response.StatusCode = (int)HttpStatusCode.OK;

            if (response.OutputStream.CanWrite)
            {
                try
                {
                    response.ContentLength64 = clientresponse.Length;
                    response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                    response.OutputStream.Close();
                }
                catch (Exception)
                {
                    // Not Important.
                }
            }
        }

        public static async Task usage(HttpListenerRequest request, HttpListenerResponse response)
        {
            string usage = "";

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

                usage = data.GetParameterValue("usage");

                copyStream.Dispose();
            }

            byte[] clientresponse = VEEMEEProcessor.sign(usage);

            response.StatusCode = (int)HttpStatusCode.OK;

            if (response.OutputStream.CanWrite)
            {
                try
                {
                    response.ContentLength64 = clientresponse.Length;
                    response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                    response.OutputStream.Close();
                }
                catch (Exception)
                {
                    // Not Important.
                }
            }
        }
    }
}
