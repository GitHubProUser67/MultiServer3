using System.Net;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.SSFW
{
    public class SSFWClass
    {
        public static Middleware<Context> StaticSSFWRoot(string route, string userAgentdrm)
        {
            return async (ctx, next) =>
            {
                string userAgent = ctx.Request.Headers["User-Agent"];

                if (userAgent == null)
                {
                    userAgent = "Hidden Client"; // Medius Client can hide userAgent.
                }
                else
                {
                    await next();
                    return;
                }

                if (userAgentdrm != null && !userAgent.Contains(userAgentdrm))
                {
                    await next();
                    return;
                }

                if (ctx.Request.Url != null)
                {
                    // Don't use Request.RawUrl, because it contains url parameters. (e.g. '?a=1&b=2')
                    string relativePath = ctx.Request.Url.AbsolutePath;

                    bool handled = relativePath.StartsWith(route);
                    if (!handled)
                    {
                        await next();
                        return;
                    }

                    await processrequest(ctx.Request, ctx.Response);
                }
            };
        }
        public static async Task processrequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string url = "";

            if (request.Url.LocalPath == null)
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return;
            }

            url = request.Url.LocalPath;

            string httpMethod = request.HttpMethod;

            // Split the URL into segments
            string[] segments = url.Trim('/').Split('/');

            // Combine the folder segments into a directory path
            string directorypath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

            // Process the request based on the HTTP method
            string filepath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder, url.Substring(1));

            switch (httpMethod)
            {
                case "PUT":
                    if (request.Headers["X-Home-Session-Id"] != null)
                        PUT.handlePUT(directorypath, request, response);
                    else
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                case "POST":
                    await POST.handlePOST(directorypath, filepath, request, response);
                    break;
                case "GET":
                    if (request.Url.AbsolutePath.Contains("/LayoutService/cprod/person/") && request.Headers["X-Home-Session-Id"] != null)
                        await SSFWLayoutService.HandleLayoutServiceGET(directorypath, request.Url.AbsolutePath, request, response);
                    else if (request.Url.AbsolutePath.Contains("/AdminObjectService/start") && request.Headers["X-Home-Session-Id"] != null)
                        await SSFWAdminObjectService.HandleAdminObjectService(request, response);
                    else if (request.Headers["X-Home-Session-Id"] != null)
                    {
                        if (File.Exists(filepath + ".json"))
                        {
                            byte[] byteresponse = await FileHelper.CryptoReadAsync(filepath + ".json", SSFWPrivateKey.SSFWPrivatekey);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            if (request.Headers["Accept"] == "application/json")
                            {
                                response.ContentType = "application/json";
                            }
                            if (response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    response.ContentLength64 = byteresponse.Length;
                                    response.OutputStream.Write(byteresponse, 0, byteresponse.Length);
                                    response.OutputStream.Close();
                                }
                                catch (Exception ex)
                                {
                                    // Not Important
                                }
                            }
                        }
                        else if (File.Exists(filepath + ".bin"))
                        {
                            byte[] byteresponse = await FileHelper.CryptoReadAsync(filepath + ".bin", SSFWPrivateKey.SSFWPrivatekey);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            if (response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    response.ContentLength64 = byteresponse.Length;
                                    response.OutputStream.Write(byteresponse, 0, byteresponse.Length);
                                    response.OutputStream.Close();
                                }
                                catch (Exception ex)
                                {
                                    // Not Important
                                }
                            }
                        }
                        else if (File.Exists(filepath + ".jpeg"))
                        {
                            byte[] byteresponse = await FileHelper.CryptoReadAsync(filepath + ".jpeg", SSFWPrivateKey.SSFWPrivatekey);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            if (response.OutputStream.CanWrite)
                            {
                                try
                                {
                                    response.ContentLength64 = byteresponse.Length;
                                    response.OutputStream.Write(byteresponse, 0, byteresponse.Length);
                                    response.OutputStream.Close();
                                }
                                catch (Exception ex)
                                {
                                    // Not Important
                                }
                            }
                        }
                        else
                        {
                            ServerConfiguration.LogWarn($"[SSFW] : {request.UserAgent} Requested a file that doesn't exist - {filepath}");
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                case "DELETE":
                    if (request.Url.AbsolutePath.Contains("/AvatarLayoutService/cprod/") && request.Headers["X-Home-Session-Id"] != null)
                        await SSFWAvatarLayoutService.HandleAvatarLayout(directorypath, filepath,request.Url.AbsolutePath, request, response, true);
                    else if (request.Headers["X-Home-Session-Id"] != null)
                    {
                        if (File.Exists(filepath + ".json"))
                        {
                            File.Delete(filepath + ".json");
                            response.StatusCode = (int)HttpStatusCode.OK;
                        }
                        else if (File.Exists(filepath + ".bin"))
                        {
                            File.Delete(filepath + ".bin");
                            response.StatusCode = (int)HttpStatusCode.OK;
                        }
                        else if (File.Exists(filepath + ".jpeg"))
                        {
                            File.Delete(filepath + ".jpeg");
                            response.StatusCode = (int)HttpStatusCode.OK;
                        }
                        else
                        {
                            ServerConfiguration.LogError($"[SSFW] : {request.UserAgent} Requested a file to delete that doesn't exist - {filepath}");
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                    }
                    else
                        response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
            }
        }
    }
}
