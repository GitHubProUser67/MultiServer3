using HttpMultipartParser;
using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEEAudiHS
    {
        public static async Task audiHSGetTopUser(HttpListenerRequest request, HttpListenerResponse response)
        {
            string key = "";
            string psnid = "";

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

                key = data.GetParameterValue("key");

                psnid = data.GetParameterValue("psnid");

                copyStream.Dispose();
            }

            if (key == "3Ebadrebr6qezag8")
            {
                byte[] clientresponse = await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Audi_Vrun/GeneralHSML.hsml", HTTPPrivateKey.HTTPPrivatekey);

                response.ContentType = "text/xml; charset=utf-8";
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
            else
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            return;
        }

        public static async Task audiHSSetUserData(HttpListenerRequest request, HttpListenerResponse response)
        {
            string key = "";
            string psnid = "";
            string time = "";
            string dist = "";

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

                key = data.GetParameterValue("key");

                psnid = data.GetParameterValue("psnid");

                time = data.GetParameterValue("time");

                dist = data.GetParameterValue("dist");

                copyStream.Dispose();
            }

            if (key == "3Ebadrebr6qezag8")
            {
                byte[] resultData = Encoding.UTF8.GetBytes($"<XML>\r\n<PAGE>\r\n<RECT X=\"0\" Y=\"1\" W=\"0\" H=\"0\" col=\"#C0C0C0\" /><RECT X=\"0\" Y=\"0\" W=\"1280\" H=\"720\" col=\"#000000\" /><TEXT X=\"57\" Y=\"82\" col=\"#FFFFFF\" size=\"4\">{psnid}</TEXT><TEXT X=\"77\" Y=\"82\" col=\"#FFFFFF\" size=\"4\">{time}</TEXT><TEXT X=\"97\" Y=\"82\" col=\"#FFFFFF\" size=\"4\">{dist}</TEXT>\r\n</PAGE>\r\n</XML>");

                await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Audi_Vrun/User_Score/{psnid}.hsml", HTTPPrivateKey.HTTPPrivatekey, resultData);

                response.ContentType = "text/xml; charset=utf-8";
                response.StatusCode = (int)HttpStatusCode.OK;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = resultData.Length;
                        response.OutputStream.Write(resultData, 0, resultData.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            return;
        }

        public static async Task audiHSGetUserData(HttpListenerRequest request, HttpListenerResponse response)
        {
            string key = "";
            string psnid = "";

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

                key = data.GetParameterValue("key");

                psnid = data.GetParameterValue("psnid");

                copyStream.Dispose();
            }

            byte[] clientresponse = new byte[0];

            if (key == "3Ebadrebr6qezag8")
            {
                if (psnid != "nil")
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Audi_Vrun/User_Score/{psnid}.hsml"))
                    {
                        clientresponse = await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Audi_Vrun/User_Score/{psnid}.hsml", HTTPPrivateKey.HTTPPrivatekey);
                    }
                    else
                    {
                        clientresponse = Encoding.UTF8.GetBytes($"<XML>\r\n<PAGE>\r\n<RECT X=\"0\" Y=\"1\" W=\"0\" H=\"0\" col=\"#C0C0C0\" /><RECT X=\"0\" Y=\"0\" W=\"1280\" H=\"720\" col=\"#000000\" /><TEXT X=\"57\" Y=\"82\" col=\"#FFFFFF\" size=\"4\">{psnid}</TEXT><TEXT X=\"77\" Y=\"82\" col=\"#FFFFFF\" size=\"4\">0.000000</TEXT><TEXT X=\"97\" Y=\"82\" col=\"#FFFFFF\" size=\"4\">0000.000000</TEXT>\r\n</PAGE>\r\n</XML>");
                    }
                }
                else
                {
                    clientresponse = Encoding.UTF8.GetBytes($"<XML>\r\n<PAGE>\r\n</PAGE>\r\n</XML>");
                }

                response.ContentType = "text/xml; charset=utf-8";
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
            else
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            return;
        }

        public static async Task audiHSGlobalTable(HttpListenerRequest request, HttpListenerResponse response)
        {
            string key = "";
            string psnid = "";
            string title = "";

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

                key = data.GetParameterValue("key");

                psnid = data.GetParameterValue("psnid");

                title = data.GetParameterValue("title");

                copyStream.Dispose();
            }

            if (key == "3Ebadrebr6qezag8")
            {
                byte[] clientresponse = await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Audi_Vrun/GeneralHSML.hsml", HTTPPrivateKey.HTTPPrivatekey);

                response.ContentType = "text/xml; charset=utf-8";
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
                        // Not Important
                    }
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            return;
        }
    }
}
