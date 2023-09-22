using System.Net;
using System.Text;
using NetCoreServer;
using HttpMultipartParser;
using MultiServer.HTTPService;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.VEEMEE
{
    public class VEEMEEStats
    {
        public static void getconfig(bool get, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            if (!get)
            {
                string id = "";

                string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                    ms.Position = 0;

                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    id = data.GetParameterValue("id");

                    ms.Dispose();
                }

                ServerConfiguration.LogInfo($"VEEMEE Server : Getconfig values : id|{id}");
            }

            string statsconfigdata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/stats_config.json", HTTPPrivateKey.HTTPPrivatekey));

            if (statsconfigdata != null)
            {
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody(VEEMEEProcessor.sign(statsconfigdata));
            }
            else
            {
                response.SetBegin((int)HttpStatusCode.InternalServerError);
                response.SetBody();
            }
        }

        public static void crash(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
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

            string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                ms.Position = 0;

                var data = MultipartFormDataParser.Parse(ms, boundary);

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

                ms.Dispose();
            }

            ServerConfiguration.LogInfo($"[VEEMEE] : A Client Crash Happened - issued by {HTTPSClass.GetHeaderValue(Headers, "User-Agent")} - Details : corehook|{corehook} - territory|{territory} - region|{region} - psnid|{psnid}" +
                $" - scene|{scene} - sceneid|{sceneid} - scenetime|{scenetime} - sceneowner|{sceneowner} - owner|{owner} - owned|{owned} - crash|{crash} - numplayers|{numplayers} - numpeople|{numpeople} - objectid|{objectid} - objectname|{objectname}");

            string statsconfigdata = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_VEEMEE/Acorn_Medow/stats_config.json", HTTPPrivateKey.HTTPPrivatekey));

            if (statsconfigdata != null)
            {
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody(VEEMEEProcessor.sign(statsconfigdata));
            }
            else
            {
                response.SetBegin((int)HttpStatusCode.InternalServerError);
                response.SetBody();
            }
        }

        public static void Usage(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string usage = "";

            string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                ms.Position = 0;

                var data = MultipartFormDataParser.Parse(ms, boundary);

                usage = data.GetParameterValue("usage");

                ms.Dispose();
            }

            response.SetBegin((int)HttpStatusCode.OK);
            response.SetBody(VEEMEEProcessor.sign(usage));
        }
    }
}
