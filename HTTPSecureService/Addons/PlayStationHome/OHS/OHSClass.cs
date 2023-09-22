using HttpMultipartParser;
using NetCoreServer;
using System.Net;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.OHS
{
    public class OHSClass
    {
        public static void ProcessRequest(string crc32, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            if (request.Url == null)
            {
                response.SetBegin((int)HttpStatusCode.Forbidden);
                response.SetBody();
                return;
            }

            string url = request.Url;

            // Split the URL into segments
            string[] segments = url.Trim('/').Split('/');

            // Combine the folder segments into a directory path
            string directorypath = Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, string.Join("/", segments.Take(segments.Length - 1).ToArray()));

            switch (request.Method)
            {
                case "POST":
                    if (url.Contains("/batch/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSBatch.Batch_Process(directorypath, request, response, Headers);
                    else if (url.Contains("community/getscore/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSCommunity.Community_Getscore(directorypath, "", request, response, Headers);
                    else if (url.Contains("community/updatescore/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSCommunity.Community_UpdateScore(directorypath, "", request, response, Headers);
                    else if (url.Contains("global/set/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUser.Set(directorypath, "", true, request, response, Headers);
                    else if (url.Contains("global/getall/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUser.Get_All(directorypath, "", true, request, response, Headers);
                    else if (url.Contains("global/get/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUser.Get(directorypath, "", true, request, response, Headers);
                    else if (url.Contains("user/getwritekey/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUser.User_GetWritekey("", request, response, Headers);
                    else if (url.Contains("user/set/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUser.Set(directorypath, "", false, request, response, Headers);
                    else if (url.Contains("user/getall/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUser.Get_All(directorypath, "", false, request, response, Headers);
                    else if (url.Contains("user/get/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUser.Get(directorypath, "", false, request, response, Headers);
                    else if (url.Contains("usercounter/set/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUserCounter.Set(directorypath, "", request, response, Headers);
                    else if (url.Contains("usercounter/getall/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUserCounter.Get_All(directorypath, "", request, response, Headers);
                    else if (url.Contains("usercounter/get/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUserCounter.Get(directorypath, "", request, response, Headers);
                    else if (url.Contains("userinventory/getglobalitems/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUserInventory.GetGlobalItems(directorypath, "", request, response, Headers);
                    else if (url.Contains("userinventory/getuserinventory/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSUserInventory.GetUserInventory(directorypath, "", request, response, Headers);
                    else if (url.Contains("leaderboard/requestbyusers/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSLeaderboard.Leaderboard_RequestByUsers(directorypath, "", request, response, Headers);
                    else if (url.Contains("leaderboard/requestbyrank/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSLeaderboard.Leaderboard_RequestByRank(directorypath, "", request, response, Headers);
                    else if (url.Contains("leaderboard/update/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSLeaderboard.Leaderboard_Update(directorypath, "", request, response, Headers);
                    else if (url.Contains("leaderboard/updatessameentry/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                        OHSLeaderboard.Leaderboard_UpdatesSameEntry(directorypath, "", request, response, Headers);
                    else if (url.Contains("/statistic/set/") && HTTPSClass.GetHeaderValue(Headers, "Content-type").Contains("multipart/form-data"))
                    {
                        string dataforohs = string.Empty;

                        string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

                        using (MemoryStream ms = new MemoryStream())
                        {
                            ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                            ms.Position = 0;

                            var data = MultipartFormDataParser.Parse(ms, boundary);

                            ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                            dataforohs = OHSProcessor.JaminDeFormat(data.GetParameterValue("data"), true);

                            if (dataforohs != null && dataforohs != string.Empty)
                                ServerConfiguration.LogInfo($"[OHS] : {HTTPSClass.GetHeaderValue(Headers, "User-Agent")} sent statistics - {dataforohs}");

                            if (dataforohs == null)
                            {
                                response.SetBegin((int)HttpStatusCode.InternalServerError);
                                response.SetBody();
                                return;
                            }

                            ms.Dispose();
                        }

                        dataforohs = OHSProcessor.JaminFormat("{ [\"status\"] = \"success\" }");

                        if (dataforohs == null)
                        {
                            response.SetBegin((int)HttpStatusCode.InternalServerError);
                            response.SetBody();
                            return;
                        }

                        response.SetContentType("application/xml;charset=UTF-8");
                        response.SetBegin((int)HttpStatusCode.OK);
                        response.SetBody($"<ohs>{dataforohs}</ohs>");
                    }
                    else
                    {
                        response.SetBegin((int)HttpStatusCode.Forbidden);
                        response.SetBody();
                    }
                    break;
                case "GET":
                    // Process the request based on the HTTP method
                    RootProcess.ProcessRootRequest(Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, url.Substring(1))
                        , Path.Combine(Directory.GetCurrentDirectory() + ServerConfiguration.HTTPStaticFolder, string.Join("/"
                        , segments.Take(segments.Length - 1).ToArray())), crc32, request, response, Headers);
                    break;
                default:
                    response.SetBegin((int)HttpStatusCode.Forbidden);
                    response.SetBody();
                    break;
            }
        }
    }
}
