using System.Net;

namespace MultiServer.HTTPService.Addons.PlayStationHome.HELLFIREGAMES
{
    public class HELLFIREGAMESClass
    {
        public static async Task ProcessRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string url = "";

            if (request.Url == null)
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return;
            }

            url = request.Url.LocalPath;

            string absolutepath = request.Url.AbsolutePath.Replace("//", "/");

            string httpMethod = request.HttpMethod;

            // Split the URL into segments
            string[] segments = url.Trim('/').Split('/');

            // Combine the folder segments into a directory path
            string directorypath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

            switch (httpMethod)
            {
                case "POST":
                    switch (absolutepath)
                    {
                        case "/HomeTycoon/Main_SCEE.php":
                            await HOMETYCOONMain.Main(request, response);
                            break;
                        case "/HomeTycoon/Main_SCEJ.php":
                            await HOMETYCOONMain.Main(request, response);
                            break;
                        case "/HomeTycoon/Main_SCEAsia.php":
                            await HOMETYCOONMain.Main(request, response);
                            break;
                        case "/HomeTycoon/Main.php":
                            await HOMETYCOONMain.Main(request, response);
                            break;
                        case "/Postcards/":
                            await HOMETYCOONMain.PostCards(request, response);
                            break;
                        default:
                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                            break;
                    }
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
            }
        }
    }
}
