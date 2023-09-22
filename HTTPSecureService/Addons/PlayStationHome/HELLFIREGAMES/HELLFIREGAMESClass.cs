using NetCoreServer;
using System.Net;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.HELLFIREGAMES
{
    public class HELLFIREGAMESClass
    {
        public static void ProcessRequest(HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            if (request.Url == null)
            {
                response.SetBegin((int)HttpStatusCode.Forbidden);
                response.SetBody();
                return;
            }

            string url = HTTPSClass.RemoveQueryString(request.Url);

            string absolutepath = url.Replace("//", "/");

            // Split the URL into segments
            string[] segments = url.Trim('/').Split('/');

            // Combine the folder segments into a directory path
            string directorypath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

            switch (request.Method)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/HomeTycoon/Main_SCEE.php":
                            HOMETYCOONRedirector.ProcessMainRedirector(request, response, Headers);
                            break;
                        case "/HomeTycoon/Main_SCEJ.php":
                            HOMETYCOONRedirector.ProcessMainRedirector(request, response, Headers);
                            break;
                        case "/HomeTycoon/Main_SCEAsia.php":
                            HOMETYCOONRedirector.ProcessMainRedirector(request, response, Headers);
                            break;
                        case "/HomeTycoon/Main.php":
                            HOMETYCOONRedirector.ProcessMainRedirector(request, response, Headers);
                            break;
                        default:
                            response.SetBegin((int)HttpStatusCode.Forbidden);
                            response.SetBody();
                            break;
                    }
                    break;
                default:
                    response.SetBegin((int)HttpStatusCode.Forbidden);
                    response.SetBody();
                    break;
            }
        }
    }
}
