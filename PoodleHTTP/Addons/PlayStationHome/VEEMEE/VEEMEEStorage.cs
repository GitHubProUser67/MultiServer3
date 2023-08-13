using HttpMultipartParser;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEEStorage
    {
        public static async Task readconfig(HttpListenerRequest request, HttpListenerResponse response)
        {
            string config = "";
            string product = "";

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

                config = data.GetParameterValue("config");
                product = data.GetParameterValue("product");

                copyStream.Dispose();
            }

            string configValue = "{}"; // Default response when config field doesn't exist

            if (!string.IsNullOrEmpty(config) && !string.IsNullOrEmpty(product))
            {
                string jsonFilePath = Path.Combine(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/config.json");

                if (File.Exists(jsonFilePath))
                {
                    dynamic jsondata = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(jsonFilePath, HTTPPrivateKey.HTTPPrivatekey)));

                    if (jsondata.ContainsKey(product) && jsondata[product].ContainsKey(config))
                    {
                        configValue = jsondata[product][config].ToString();
                    }
                }
            }

            byte[] clientresponse = VEEMEEProcessor.sign(configValue);

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

        public static async Task readtable(HttpListenerRequest request, HttpListenerResponse response)
        {
            string psnid = "";
            string product = "";
            string hex = "";
            string __salt = "";

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

                psnid = data.GetParameterValue("psnid");

                product = data.GetParameterValue("product");

                hex = data.GetParameterValue("hex");

                __salt = data.GetParameterValue("__salt");

                copyStream.Dispose();
            }

            byte[] clientresponse = await VEEMEEProfileManager.ReadProfile(psnid, product, hex, __salt);

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

        public static async Task writetable(HttpListenerRequest request, HttpListenerResponse response)
        {
            string psnid = "";

            string profile = "";

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

                psnid = data.GetParameterValue("psnid");

                profile = data.GetParameterValue("profile");

                copyStream.Dispose();
            }

            byte[] clientresponse = await VEEMEEProfileManager.WriteProfile(psnid, profile);

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
