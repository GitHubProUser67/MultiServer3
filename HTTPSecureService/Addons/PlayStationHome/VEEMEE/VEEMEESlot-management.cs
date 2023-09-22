using System.Net;
using NetCoreServer;
using HttpMultipartParser;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEESlot_management
    {
        public static void getobjectslot(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
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

            string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                ms.Position = 0;

                var data = MultipartFormDataParser.Parse(ms, boundary);

                slot_name = data.GetParameterValue("slot_name");

                session_key = data.GetParameterValue("session_key");

                scene_id = data.GetParameterValue("scene_id");

                region = data.GetParameterValue("region");

                try
                {
                    max_slot = int.Parse(data.GetParameterValue("max_slot"));
                }
                catch (Exception)
                {
                    // Not Important
                }

                object_id = data.GetParameterValue("object_id");

                psn_id = data.GetParameterValue("psn_id");

                instance_id = data.GetParameterValue("instance_id");

                hex = data.GetParameterValue("hex");

                __salt = data.GetParameterValue("__salt");

                ms.Dispose();
            }

            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/Object_Instances/{instance_id}/{slot_name}/");

            Directory.CreateDirectory(directoryPath);

            for (int i = 1; i <= max_slot; i++)
            {
                if (!File.Exists(directoryPath + $"{i}.xml"))
                    File.WriteAllText(directoryPath + $"{i}.xml", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<xml>\r\n  <slot>unnocupied</slot>\r\n  <expiration>01/01/1970 00:00:00</expiration>\r\n</xml>");
            }

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetBody(VEEMEEProcessor.sign("{\"slot\":PUT_NUMBER_HERE}".Replace("PUT_NUMBER_HERE", VEEMEEProcessor.UpdateSlot(directoryPath, psn_id, 0, false))));
        }

        public static void remove(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
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

            string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                ms.Position = 0;

                var data = MultipartFormDataParser.Parse(ms, boundary);

                slot_name = data.GetParameterValue("slot_name");

                session_key = data.GetParameterValue("session_key");

                scene_id = data.GetParameterValue("scene_id");

                region = data.GetParameterValue("region");

                object_id = data.GetParameterValue("object_id");

                psn_id = data.GetParameterValue("psn_id");

                instance_id = data.GetParameterValue("instance_id");

                hex = data.GetParameterValue("hex");

                __salt = data.GetParameterValue("__salt");

                ms.Dispose();
            }

            string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/Object_Instances/{instance_id}/{slot_name}/");

            if (!Directory.Exists(directoryPath))
            {
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody(VEEMEEProcessor.sign("{\"success\":false}"));
            }
            else
            {
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody(VEEMEEProcessor.sign("{\"success\":PUT_BOOL_HERE}".Replace("PUT_BOOL_HERE", VEEMEEProcessor.UpdateSlot(directoryPath, psn_id, slot_num, true))));
            }
        }

        public static void heartbeat(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
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

            string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                ms.Position = 0;

                var data = MultipartFormDataParser.Parse(ms, boundary);

                slot_name = data.GetParameterValue("slot_name");

                session_key = data.GetParameterValue("session_key");

                scene_id = data.GetParameterValue("scene_id");

                region = data.GetParameterValue("region");

                object_id = data.GetParameterValue("object_id");

                psn_id = data.GetParameterValue("psn_id");

                instance_id = data.GetParameterValue("instance_id");

                hex = data.GetParameterValue("hex");

                __salt = data.GetParameterValue("__salt");

                ms.Dispose();
            }

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetBody(VEEMEEProcessor.sign("{ \"heartbeat\": true }"));
        }
    }
}
