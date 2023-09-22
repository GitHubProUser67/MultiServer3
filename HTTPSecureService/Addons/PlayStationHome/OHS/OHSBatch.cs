using HttpMultipartParser;
using NetCoreServer;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.OHS
{
    public class OHSBatch
    {
        public static void Batch_Process(string directorypath, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
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

                if (dataforohs == null)
                {
                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                    response.SetBody();
                    return;
                }

                ms.Dispose();
            }

            // Deserialize the JSON data into a list of commands.
            var commands = JsonConvert.DeserializeObject<BatchCommand[]>(dataforohs);

            if (commands != null)
            {
                int i = 0;

                StringBuilder resultBuilder = new StringBuilder();

                foreach (var command in commands)
                {
                    i = i + 1;

                    string resultfromcommand = "";

                    string method = command.Method;
                    string project = command.Project;
                    string data = command.Data.ToString(Formatting.None);

                    if (project == "<dummy>")
                        project = "dummy";

                    ServerConfiguration.LogInfo($"[OHS] : {HTTPSClass.GetHeaderValue(Headers, "User-Agent")} Requested a /batch/ method, here are the details : method | {method} - project | {project} - data | {data}");

                    switch (method)
                    {
                        case "community/getscore/":

                            resultfromcommand = OHSCommunity.Community_Getscore(directorypath + $"/{project}/", data, request, response, Headers);

                            break;

                        case "community/updatescore/":

                            resultfromcommand = OHSCommunity.Community_UpdateScore(directorypath + $"/{project}/", data, request, response, Headers);

                            break;
                        case "global/getall/":

                            resultfromcommand = OHSUser.Get_All(directorypath + $"/{project}/", data, true, request, response, Headers);

                            break;

                        case "global/get/":

                            resultfromcommand = OHSUser.Get(directorypath + $"/{project}/", data, true, request, response, Headers);

                            break;

                        case "global/set/":

                            resultfromcommand = OHSUser.Set(directorypath + $"/{project}/", data, true, request, response, Headers);

                            break;

                        case "userid/":

                            resultfromcommand = OHSUser.User_Id(data, request, response, Headers);

                            break;

                        case "user/getall/":

                            resultfromcommand = OHSUser.Get_All(directorypath + $"/{project}/", data, false, request, response, Headers);

                            break;

                        case "user/get/":

                            resultfromcommand = OHSUser.Get(directorypath + $"/{project}/", data, false, request, response, Headers);

                            break;

                        case "user/set/":

                            resultfromcommand = OHSUser.Set(directorypath + $"/{project}/", data, false, request, response, Headers);

                            break;

                        case "user/getwritekey/":

                            resultfromcommand = OHSUser.User_GetWritekey(data, request, response, Headers);

                            break;

                        case "leaderboard/requestbyusers/":

                            resultfromcommand = OHSLeaderboard.Leaderboard_RequestByUsers(directorypath + $"/{project}/", data, request, response, Headers);

                            break;

                        case "leaderboard/requestbyrank/":

                            resultfromcommand = OHSLeaderboard.Leaderboard_RequestByRank(directorypath + $"/{project}/", data, request, response, Headers);

                            break;

                        case "leaderboard/update/":

                            resultfromcommand = OHSLeaderboard.Leaderboard_Update(directorypath + $"/{project}/", data, request, response, Headers);

                            break;

                        case "leaderboard/updatessameentry/":

                            resultfromcommand = OHSLeaderboard.Leaderboard_UpdatesSameEntry(directorypath + $"/{project}/", data, request, response, Headers);

                            break;

                        case "usercounter/set/":

                            resultfromcommand = OHSUserCounter.Set(directorypath + $"/{project}/", data, request, response, Headers);

                            break;

                        case "usercounter/getall/":

                            resultfromcommand = OHSUserCounter.Get_All(directorypath + $"/{project}/", data, request, response, Headers);

                            break;

                        case "usercounter/get/":

                            resultfromcommand = OHSUserCounter.Get(directorypath + $"/{project}/", data, request, response, Headers);

                            break;

                        case "userinventory/getglobalitems/":

                            resultfromcommand = OHSUserInventory.GetGlobalItems(directorypath + $"/{project}/", data, request, response, Headers);

                            break;

                        case "userinventory/getuserinventory/":

                            resultfromcommand = OHSUserInventory.GetUserInventory(directorypath + $"/{project}/", data, request, response, Headers);

                            break;

                        default:

                            ServerConfiguration.LogWarn($"OHS Server : Batch requested a method I don't know about, please report it to GITHUB {method} in {project} with data {data}");

                            break;
                    }

                    if (resultfromcommand == "")
                        resultfromcommand = "{ [\"status\"] = \"fail\" }";

                    if (resultBuilder.Length == 0)
                        resultBuilder.Append($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [{i}] = {resultfromcommand}");
                    else
                        resultBuilder.Append($", [{i}] = {resultfromcommand}");
                }

                resultBuilder.Append(" } }");

                dataforohs = resultBuilder.ToString();

                Console.WriteLine(dataforohs);
            }
            else
                dataforohs = "{ [\"status\"] = \"fail\" }";

            dataforohs = OHSProcessor.JaminFormat(dataforohs);

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
    }
}
