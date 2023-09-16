using System.Net;

namespace MultiServer.HTTPService.Addons.PlayStationHome.POTTERMORE
{
    public class POTTERMOREClass
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
                case "GET":
                    switch (absolutepath)
                    {
                        case "/api/user":
                            await POTTERMOREApiAuth.ApiUserRequest(request, response);
                            break;
                        default:
                            response.StatusCode = (int)HttpStatusCode.Forbidden;
                            break;
                    }
                    break;
                case "POST":
                    switch (absolutepath)
                    {
                        case "/api/auth":
                            await POTTERMOREApiAuth.ApiAuthRequest(request, response);
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
