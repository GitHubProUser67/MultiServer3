using HttpMultipartParser;
using MultiServer.HTTPSecureService.Addons.PlayStationHome.OHS;
using System.Net;
using System.Text;

namespace MultiServer.HTTPService.Addons.PlayStationHome.OHS
{
    public class OHSClass
    {
        public static async Task ProcessRequest(string crc32, HttpListenerRequest request, HttpListenerResponse response)
        {
            if (request.Url == null)
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return;
            }

            string url = request.Url.LocalPath;

            string absolutepath = request.Url.AbsolutePath;

            string httpMethod = request.HttpMethod;

            // Split the URL into segments
            string[] segments = url.Trim('/').Split('/');

            // Combine the folder segments into a directory path
            string directorypath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

            switch (httpMethod)
            {
                case "POST":
                    if (request.Url.AbsolutePath.Contains("/statistic/set/") && request.ContentType!= null && request.ContentType.StartsWith("multipart/form-data"))
                    {
                        string dataforohs = string.Empty;

                        var data = MultipartFormDataParser.Parse(request.InputStream, Extensions.ExtractBoundary(request.ContentType));

                        ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                        dataforohs = OHSProcessor.JaminDeFormat(data.GetParameterValue("data"), true);

                        if (dataforohs == null)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            return;
                        }
                        else
                            ServerConfiguration.LogInfo($"[OHS] : {request.UserAgent} sent statistics - {dataforohs}");

                        dataforohs = OHSProcessor.JaminFormat("{ [\"status\"] = \"success\" }");

                        if (dataforohs == null)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            return;
                        }
                        else
                        {
                            byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{dataforohs}</ohs>");

                            response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentLength64 = postresponsetooutput.Length;

                            if (response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    response.OutputStream.Write(postresponsetooutput, 0, postresponsetooutput.Length);
                                    response.OutputStream.Close();
                                }
                                catch (Exception)
                                {
                                    // Not Important!
                                }
                            }
                        }
                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                case "GET":
                    // Process the request based on the HTTP method
                    await RootProcess.ProcessRootRequest(Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, url.Substring(1))
                        , Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, string.Join("/"
                        , segments.Take(segments.Length - 1).ToArray())), crc32, request, response);
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
            }
        }
    }
}
