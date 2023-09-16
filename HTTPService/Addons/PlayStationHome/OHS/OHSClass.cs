using HttpMultipartParser;
using System.Net;
using System.Text;

namespace MultiServer.HTTPService.Addons.PlayStationHome.OHS
{
    public class OHSClass
    {
        public static async Task processrequest(string crc32, HttpListenerRequest request, HttpListenerResponse response)
        {
            string url = "";

            if (request.Url.LocalPath == null)
            {
                response.StatusCode = (int)HttpStatusCode.Unauthorized;

                return;
            }

            url = request.Url.LocalPath;

            string absolutepath = request.Url.AbsolutePath;

            string httpMethod = request.HttpMethod;

            // Split the URL into segments
            string[] segments = url.Trim('/').Split('/');

            // Combine the folder segments into a directory path
            string directorypath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

            switch (httpMethod)
            {
                case "POST":
                    if (request.Url.AbsolutePath.Contains("/batch/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSBatch.batch_process(directorypath, "", request, response);
                    else if (absolutepath.Contains("community/getscore/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSCommunity.community_getscore(directorypath, "", request, response);
                    else if (absolutepath.Contains("community/updatescore/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSCommunity.community_updatescore(directorypath, "", request, response);
                    else if (request.Url.AbsolutePath.Contains("global/set/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUser.set(directorypath, "", true, request, response);
                    else if (request.Url.AbsolutePath.Contains("global/getall/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUser.get_all(directorypath, "", true, request, response);
                    else if (request.Url.AbsolutePath.Contains("global/get/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUser.get(directorypath, "", true, request, response);
                    else if (request.Url.AbsolutePath.Contains("user/getwritekey/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUser.user_getwritekey("", request, response);
                    else if (request.Url.AbsolutePath.Contains("user/set/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUser.set(directorypath, "", false, request, response);
                    else if (request.Url.AbsolutePath.Contains("user/getall/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUser.get_all(directorypath, "", false, request, response);
                    else if (request.Url.AbsolutePath.Contains("user/get/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUser.get(directorypath, "", false, request, response);
                    else if (request.Url.AbsolutePath.Contains("usercounter/set/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUserCounter.set(directorypath, "", request, response);
                    else if (request.Url.AbsolutePath.Contains("usercounter/getall/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUserCounter.get_all(directorypath, "", request, response);
                    else if (request.Url.AbsolutePath.Contains("usercounter/get/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUserCounter.get(directorypath, "", request, response);
                    else if (request.Url.AbsolutePath.Contains("userinventory/getglobalitems/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUserInventory.getGlobalItems(directorypath, "", request, response);
                    else if (request.Url.AbsolutePath.Contains("userinventory/getuserinventory/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSUserInventory.getUserInventory(directorypath, "", request, response);
                    else if (request.Url.AbsolutePath.Contains("leaderboard/requestbyusers/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSLeaderboard.leaderboard_requestbyusers(directorypath, "", request, response);
                    else if (request.Url.AbsolutePath.Contains("leaderboard/requestbyrank/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSLeaderboard.leaderboard_requestbyrank(directorypath, "", request, response);
                    else if (request.Url.AbsolutePath.Contains("leaderboard/update/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSLeaderboard.leaderboard_update(directorypath, "", request, response);
                    else if (request.Url.AbsolutePath.Contains("leaderboard/updatessameentry/") && request.ContentType.StartsWith("multipart/form-data"))
                        await OHSLeaderboard.leaderboard_updatessameentry(directorypath, "", request, response);
                    else if (request.Url.AbsolutePath.Contains("/statistic/set/") && request.ContentType.StartsWith("multipart/form-data"))
                    {
                        string dataforohs = "";

                        var data = MultipartFormDataParser.Parse(request.InputStream, Extensions.ExtractBoundary(request.ContentType));

                        ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                        try
                        {
                            // Execute the Lua script and get the result
                            object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_ENCRYPTEDJAMINVALUE_HERE", data.GetParameterValue("data").Substring(8)));

                            if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                                Console.WriteLine($"[OHS] : {request.UserAgent} sent statistics - {returnValues[0]?.ToString()}");
                        }
                        catch (Exception)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            return;
                        }

                        try
                        {
                            // Execute the Lua script and get the result
                            object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\" }"));

                            if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                                dataforohs = returnValues2nd[0]?.ToString();
                        }
                        catch (Exception)
                        {
                            response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            return;
                        }

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
