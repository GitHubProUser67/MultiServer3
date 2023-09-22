using HttpMultipartParser;
using NetCoreServer;
using System.Net;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.HELLFIREGAMES
{
    public class HOMETYCOONRedirector
    {
        public static void ProcessMainRedirector(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                ms.Position = 0;

                var data = MultipartFormDataParser.Parse(ms, boundary);

                string Command = data.GetParameterValue("Command");

                switch (Command)
                {
                    case "VersionCheck":
                        response.SetBegin((int)HttpStatusCode.OK);
                        response.SetBody("<Response><URL>http://game2.hellfiregames.com/HomeTycoon</URL></Response>");
                        break;
                    default:
                        response.SetBegin((int)HttpStatusCode.Forbidden);
                        response.SetBody();
                        break;

                }

                ms.Dispose();
            }
        }
    }
}
