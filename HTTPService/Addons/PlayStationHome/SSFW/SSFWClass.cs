using System.Net;

namespace MultiServer.HTTPService.Addons.PlayStationHome.SSFW
{
    public class SSFWClass
    {
        private static volatile bool _keepGoing = true;

        private static bool stopserver = false;

        public static bool ssfwstarted = false;

        // Create and start the HttpListener
        private static HttpListener listener = new();

        private static Task? _mainLoop;

        public static Task SSFWstart()
        {
            ssfwstarted = true;

            SSFWPrivateKey.setup();

            stopserver = false;
            _keepGoing = true;
            if (_mainLoop != null && !_mainLoop.IsCompleted) return Task.CompletedTask; //Already started
            _mainLoop = loopserver();

            return Task.CompletedTask;
        }

        private async static Task loopserver()
        {
            listener.Prefixes.Add("http://*:10443/");

            ServerConfiguration.LogInfo($"SSFW Server started - Listening for requests...");

            listener.Start();

            while (_keepGoing)
            {
                try
                {
                    //GetContextAsync() returns when a new request come in
                    var context = await listener.GetContextAsync();

                    lock (listener)
                    {
                        if (_keepGoing)
                            Task.Run(() => ProcessRequest(context));
                    }
                }
                catch (Exception ex)
                {
                    if (ex is HttpListenerException)
                    {
                        _keepGoing = false;

                        if (stopserver)
                        {
                            RemoveAllPrefixes(listener);
                            stopserver = false;
                        }
                        else
                        {
                            _keepGoing = false;

                            ServerConfiguration.LogError($"[SSFW] - FATAL ERROR OCCURED - {ex} - Trying to listen for requests again...");

                            if (!listener.IsListening)
                            {
                                _keepGoing = true;
                                listener.Start();
                            }
                            else
                                _keepGoing = true;
                        }
                    }
                }
            }
        }

        private async static Task ProcessRequest(HttpListenerContext ctx)
        {
            try
            {
                string clientip = ctx.Request.RemoteEndPoint.Address.ToString();

                if (ctx.Request.Url == null || ctx.Request.RawUrl == null || !ctx.Request.UserAgent.Contains("PSHome") || ServerConfiguration.IsIPBanned(clientip))
                {
                    ServerConfiguration.LogError($"[SECURITY] - Client - {clientip} Requested the SSFW server while being banned!");

                    ctx.Response.StatusCode = 403;
                    ctx.Response.Close();
                    return;
                }
                else
                    ServerConfiguration.LogInfo($"[SECURITY] - Client - {clientip} Requested the SSFW server.");
            }
            catch (Exception)
            {
                ctx.Response.StatusCode = 403;
                ctx.Response.Close();
                return;
            }

            ServerConfiguration.LogInfo($"[SSFW] - {ctx.Request.UserAgent} Requested {ctx.Request.Url.AbsolutePath}");

            string url = ctx.Request.Url.LocalPath;

            string httpMethod = ctx.Request.HttpMethod;

            // Split the URL into segments
            string[] segments = url.Trim('/').Split('/');

            // Combine the folder segments into a directory path
            string directorypath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

            // Process the request based on the HTTP method
            string filepath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.SSFWStaticFolder, url.Substring(1));

            switch (httpMethod)
            {
                case "PUT":
                    if (ctx.Request.Headers["X-Home-Session-Id"] != null)
                        PUT.handlePUT(directorypath, ctx.Request, ctx.Response);
                    else
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                case "POST":
                    await POST.handlePOST(directorypath, filepath, ctx.Request, ctx.Response);
                    break;
                case "GET":
                    if (ctx.Request.Url.AbsolutePath.Contains("/LayoutService/cprod/person/") && ctx.Request.Headers["X-Home-Session-Id"] != null)
                        await SSFWLayoutService.HandleLayoutServiceGET(directorypath, ctx.Request.Url.AbsolutePath, ctx.Request, ctx.Response);
                    else if (ctx.Request.Url.AbsolutePath.Contains("/AdminObjectService/start") && ctx.Request.Headers["X-Home-Session-Id"] != null)
                        await SSFWAdminObjectService.HandleAdminObjectService(ctx.Request, ctx.Response);
                    else if (ctx.Request.Url.AbsolutePath.Contains($"/SaveDataService/cprod/{ctx.Request.Url.Segments.LastOrDefault()}") && ctx.Request.Headers["X-Home-Session-Id"] != null)
                        await SSFWGetFileList.SSFWSaveDataDebugGetFileList(directorypath, ctx.Request.Url.AbsolutePath, ctx.Request, ctx.Response);
                    else if (ctx.Request.Headers["X-Home-Session-Id"] != null)
                    {
                        if (File.Exists(filepath + ".json"))
                        {
                            byte[] byteresponse = FileHelper.CryptoReadAsync(filepath + ".json", SSFWPrivateKey.SSFWPrivatekey);

                            if (byteresponse != null)
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                if (ctx.Request.Headers["Accept"] == "application/json")
                                    ctx.Response.ContentType = "application/json";
                                ctx.Response.ContentLength64 = byteresponse.Length;
                                if (ctx.Response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        ctx.Response.OutputStream.Write(byteresponse, 0, byteresponse.Length);
                                        ctx.Response.OutputStream.Close();
                                    }
                                    catch (Exception)
                                    {
                                        // Not Important
                                    }
                                }
                            }
                            else
                                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }
                        else if (File.Exists(filepath + ".bin"))
                        {
                            byte[] byteresponse = FileHelper.CryptoReadAsync(filepath + ".bin", SSFWPrivateKey.SSFWPrivatekey);

                            if (byteresponse != null)
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                ctx.Response.ContentLength64 = byteresponse.Length;
                                if (ctx.Response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        ctx.Response.OutputStream.Write(byteresponse, 0, byteresponse.Length);
                                        ctx.Response.OutputStream.Close();
                                    }
                                    catch (Exception)
                                    {
                                        // Not Important
                                    }
                                }
                            }
                            else
                                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }
                        else if (File.Exists(filepath + ".jpeg"))
                        {
                            byte[] byteresponse = FileHelper.CryptoReadAsync(filepath + ".jpeg", SSFWPrivateKey.SSFWPrivatekey);

                            if (byteresponse != null)
                            {
                                ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                ctx.Response.ContentLength64 = byteresponse.Length;
                                if (ctx.Response.OutputStream.CanWrite)
                                {
                                    try
                                    {
                                        ctx.Response.OutputStream.Write(byteresponse, 0, byteresponse.Length);
                                        ctx.Response.OutputStream.Close();
                                    }
                                    catch (Exception)
                                    {
                                        // Not Important
                                    }
                                }
                            }
                            else
                                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }
                        else
                        {
                            ServerConfiguration.LogWarn($"[SSFW] : {ctx.Request.UserAgent} Requested a file that doesn't exist - {filepath}");
                            ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                    }
                    else
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                case "DELETE":
                    if (ctx.Request.Url.AbsolutePath.Contains("/AvatarLayoutService/cprod/") && ctx.Request.Headers["X-Home-Session-Id"] != null)
                        await SSFWAvatarLayoutService.HandleAvatarLayout(directorypath, filepath, ctx.Request.Url.AbsolutePath, ctx.Request, ctx.Response, true);
                    else if (ctx.Request.Headers["X-Home-Session-Id"] != null)
                    {
                        if (File.Exists(filepath + ".json"))
                        {
                            File.Delete(filepath + ".json");
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        }
                        else if (File.Exists(filepath + ".bin"))
                        {
                            File.Delete(filepath + ".bin");
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        }
                        else if (File.Exists(filepath + ".jpeg"))
                        {
                            File.Delete(filepath + ".jpeg");
                            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        }
                        else
                        {
                            ServerConfiguration.LogError($"[SSFW] : {ctx.Request.UserAgent} Requested a file to delete that doesn't exist - {filepath}");
                            ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                    }
                    else
                        ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
                default:
                    ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    break;
            }

            ctx.Response.Close();
        }

        private static bool RemoveAllPrefixes(HttpListener listener)
        {
            // Get the prefixes that the Web server is listening to.
            HttpListenerPrefixCollection prefixes = listener.Prefixes;
            try
            {
                prefixes.Clear();
            }
            // If the operation failed, return false.
            catch
            {
                return false;
            }

            return true;
        }

        public static void SSFWstop()
        {
            stopserver = true;
            listener.Stop();
            _keepGoing = false;

            ssfwstarted = false;
        }
    }
}
