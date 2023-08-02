using HttpMultipartParser;
using System.Net;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEESlot_management
    {
        public static async Task getobjectslot(HttpListenerRequest request, HttpListenerResponse response)
        {
            int max_slot = 0;
            string slot_name = "";
            string session_key = "";
            string scene_id = "";
            string region = "";
            string object_id = "";
            string psn_id = "";
            string instance_id = "";
            string hex = "";
            string __salt = "";

            string boundary = Misc.ExtractBoundary(request.ContentType);

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

                slot_name = data.GetParameterValue("slot_name");

                session_key = data.GetParameterValue("session_key");

                scene_id = data.GetParameterValue("scene_id");

                region = data.GetParameterValue("region");

                max_slot = int.Parse(data.GetParameterValue("max_slot"));

                object_id = data.GetParameterValue("object_id");

                psn_id = data.GetParameterValue("psn_id");

                instance_id = data.GetParameterValue("instance_id");

                hex = data.GetParameterValue("hex");

                __salt = data.GetParameterValue("__salt");

                copyStream.Dispose();
            }

            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/Object_Instances/{instance_id}/{slot_name}/");

            Directory.CreateDirectory(directoryPath);

            for (int i = 1; i <= max_slot; i++)
            {
                if (!File.Exists(directoryPath + $"{i}.xml"))
                {
                    File.WriteAllText(directoryPath + $"{i}.xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<xml>\r\n  <slot>unnocupied</slot>\r\n  <expiration>01/01/1970 00:00:00</expiration>\r\n</xml>");
                }
            }

            byte[] clientresponse = VEEMEEProcessor.sign("{\"slot\":PUT_NUMBER_HERE}".Replace("PUT_NUMBER_HERE", VEEMEEProcessor.UpdateSlot(directoryPath, psn_id, 0, false)));

            response.StatusCode = (int)HttpStatusCode.OK;

            if (response.OutputStream.CanWrite)
            {
                try
                {
                    response.ContentLength64 = clientresponse.Length;
                    response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                    response.OutputStream.Close();
                }
                catch (Exception ex)
                {
                    // Not Important.
                }
            }
        }

        public static async Task remove(HttpListenerRequest request, HttpListenerResponse response)
        {
            int slot_num = 0;
            string slot_name = "";
            string session_key = "";
            string scene_id = "";
            string region = "";
            string object_id = "";
            string psn_id = "";
            string instance_id = "";
            string hex = "";
            string __salt = "";

            string boundary = Misc.ExtractBoundary(request.ContentType);

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

                try
                {
                    slot_num = int.Parse(data.GetParameterValue("slot_num"));
                }
                catch (Exception ex)
                {
                    slot_num = 0;
                }

                slot_name = data.GetParameterValue("slot_name");

                session_key = data.GetParameterValue("session_key");

                scene_id = data.GetParameterValue("scene_id");

                region = data.GetParameterValue("region");

                object_id = data.GetParameterValue("object_id");

                psn_id = data.GetParameterValue("psn_id");

                instance_id = data.GetParameterValue("instance_id");

                hex = data.GetParameterValue("hex");

                __salt = data.GetParameterValue("__salt");

                copyStream.Dispose();
            }

            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/Object_Instances/{instance_id}/{slot_name}/");

            if (!Directory.Exists(directoryPath))
            {
                byte[] clientresponse = VEEMEEProcessor.sign("{\"success\":false}");

                response.StatusCode = (int)HttpStatusCode.OK;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = clientresponse.Length;
                        response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception ex)
                    {
                        // Not Important.
                    }
                }
            }
            else
            {
                byte[] clientresponse = VEEMEEProcessor.sign("{\"success\":PUT_BOOL_HERE}".Replace("PUT_BOOL_HERE", VEEMEEProcessor.UpdateSlot(directoryPath, psn_id, slot_num, true)));

                response.StatusCode = (int)HttpStatusCode.OK;

                if (response.OutputStream.CanWrite)
                {
                    try
                    {
                        response.ContentLength64 = clientresponse.Length;
                        response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                        response.OutputStream.Close();
                    }
                    catch (Exception ex)
                    {
                        // Not Important.
                    }
                }
            }
        }

        public static async Task heartbeat(HttpListenerRequest request, HttpListenerResponse response)
        {
            string session_key = "";
            string scene_id = "";
            string region = "";
            string slot_name = "";
            string object_id = "";
            string psn_id = "";
            string instance_id = "";
            string hex = "";
            string __salt = "";

            string boundary = Misc.ExtractBoundary(request.ContentType);

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

                session_key = data.GetParameterValue("session_key");

                scene_id = data.GetParameterValue("scene_id");

                region = data.GetParameterValue("territory");

                slot_name = data.GetParameterValue("slot_name");

                region = data.GetParameterValue("territory");

                object_id = data.GetParameterValue("object_id");

                psn_id = data.GetParameterValue("psn_id");

                instance_id = data.GetParameterValue("instance_id");

                hex = data.GetParameterValue("hex");

                __salt = data.GetParameterValue("__salt");

                copyStream.Dispose();
            }

            byte[] clientresponse = VEEMEEProcessor.sign("{ \"heartbeat\": true }");

            response.StatusCode = (int)HttpStatusCode.OK;

            if (response.OutputStream.CanWrite)
            {
                try
                {
                    response.ContentLength64 = clientresponse.Length;
                    response.OutputStream.Write(clientresponse, 0, clientresponse.Length);
                    response.OutputStream.Close();
                }
                catch (Exception ex)
                {
                    // Not Important.
                }
            }
        }
    }
}
