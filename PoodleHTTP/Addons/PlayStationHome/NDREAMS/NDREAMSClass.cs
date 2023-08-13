using System.Net;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.NDREAMS
{
    public class NDREAMSClass
    {
        public static async Task processrequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string url = "";

            if (request.Url.LocalPath == null)
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
                        case "/blueprint/blueprint_furniture.php":
                            await blueprinthome.blueprint_furniture(request, response);
                            break;
                        case "/aurora/PStats.php":
                            await aurora.PSStats(request, response);
                            break;
                        case "/xi2/cont/PStats.php":
                            await xi2.PSStats(request, response);
                            break;
                        case "/xi2/cont/xi2_cont.php":
                            await xi2.xi2_cont(request, response);
                            break;
                        case "/xi2/cont/articles_cont.php":
                            await xi2.articles_cont(request, response);
                            break;
                        case "/xi2/cont/battle_cont.php":
                            await xi2.battle_cont(request, response);
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
