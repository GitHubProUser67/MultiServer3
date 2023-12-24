using CustomLogger;
using Newtonsoft.Json.Linq;
using HttpMultipartParser;
using Newtonsoft.Json;
using System.Text;

namespace BackendProject.WebAPIs.OHS
{
    public class Batch
    {
        public static string? Batch_Process(byte[] PostData, string ContentType, string directorypath, int game)
        {
            string? dataforohs = null;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    LoggerAccessor.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");
                    dataforohs = JaminProcessor.JaminDeFormat(data.GetParameterValue("data"), true, game);
                    ms.Flush();
                }
            }

            try
            {
                if (!string.IsNullOrEmpty(dataforohs))
                {
                    // Deserialize the JSON data into a list of commands.
                    var commands = JsonConvert.DeserializeObject<BatchCommand[]>(dataforohs);

                    if (commands != null)
                    {
                        int i = 0;

                        StringBuilder? resultBuilder = new();

                        foreach (var command in commands)
                        {
                            i = i + 1;

                            if (command != null && command.Data != null)
                            {
                                string? resultfromcommand = null;
                                string? method = command.Method;
                                string? project = command.Project;
                                string? data = command.Data.ToString(Formatting.None);

                                if (project == "<dummy>")
                                    project = "dummy";

                                Directory.CreateDirectory(directorypath + $"/{project}/");

                                LoggerAccessor.LogInfo($"[OHS] : Client Requested a /batch/ method, here are the details : method | {method} - project | {project} - data | {data}");

                                switch (method)
                                {
                                    case "community/getscore/":
                                        resultfromcommand = Community.Community_Getscore(PostData, ContentType, directorypath + $"/{project}/", data, game);
                                        break;
                                    case "community/updatescore/":
                                        resultfromcommand = Community.Community_UpdateScore(PostData, ContentType, directorypath + $"/{project}/", data, game);
                                        break;
                                    case "global/getall/":
                                        resultfromcommand = User.Get_All(PostData, ContentType, directorypath + $"/{project}/", data, true, game);
                                        break;
                                    case "global/get/":
                                        resultfromcommand = User.Get(PostData, ContentType, directorypath + $"/{project}/", data, true, game);
                                        break;
                                    case "global/set/":
                                        resultfromcommand = User.Set(PostData, ContentType, directorypath + $"/{project}/", data, true, game);
                                        break;
                                    case "userid/":
                                        resultfromcommand = User.User_Id(PostData, ContentType, data, game);
                                        break;
                                    case "user/getall/":
                                        resultfromcommand = User.Get_All(PostData, ContentType, directorypath + $"/{project}/", data, false, game);
                                        break;
                                    case "user/get/":
                                        resultfromcommand = User.Get(PostData, ContentType, directorypath + $"/{project}/", data, false, game);
                                        break;
                                    case "user/set/":
                                        resultfromcommand = User.Set(PostData, ContentType, directorypath + $"/{project}/", data, false, game);
                                        break;
                                    case "user/getwritekey/":
                                        resultfromcommand = User.User_GetWritekey(PostData, ContentType, data, game);
                                        break;
                                    case "leaderboard/requestbyusers/":
                                        resultfromcommand = Leaderboard.Leaderboard_RequestByUsers(PostData, ContentType, directorypath + $"/{project}/", data, game);
                                        break;
                                    case "leaderboard/requestbyrank/":
                                        resultfromcommand = Leaderboard.Leaderboard_RequestByRank(PostData, ContentType, directorypath + $"/{project}/", data, game);
                                        break;
                                    case "leaderboard/update/":
                                        resultfromcommand = Leaderboard.Leaderboard_Update(PostData, ContentType, directorypath + $"/{project}/", data, game);
                                        break;
                                    case "leaderboard/updatessameentry/":
                                        resultfromcommand = Leaderboard.Leaderboard_UpdatesSameEntry(PostData, ContentType, directorypath + $"/{project}/", data, game);
                                        break;
                                    case "usercounter/set/":
                                        resultfromcommand = UserCounter.Set(PostData, ContentType, directorypath + $"/{project}/", data, game);
                                        break;
                                    case "usercounter/getall/":
                                        resultfromcommand = UserCounter.Get_All(PostData, ContentType, directorypath + $"/{project}/", data, game);
                                        break;
                                    case "usercounter/get/":
                                        resultfromcommand = UserCounter.Get(PostData, ContentType, directorypath + $"/{project}/", data, game);
                                        break;
                                    case "userinventory/getglobalitems/":
                                        resultfromcommand = UserInventory.GetGlobalItems(PostData, ContentType, directorypath + $"/{project}/", data, game);
                                        break;
                                    case "userinventory/getuserinventory/":
                                        resultfromcommand = UserInventory.GetUserInventory(PostData, ContentType, directorypath + $"/{project}/", data, game);
                                        break;
                                    default:
                                        LoggerAccessor.LogWarn($"[OHS] - Batch requested a method I don't know about, please report it to GITHUB {method} in {project} with data {data}");
                                        break;
                                }

                                if (string.IsNullOrEmpty(resultfromcommand))
                                    resultfromcommand = "{ [\"status\"] = \"fail\" }";

                                if (resultBuilder.Length == 0)
                                    resultBuilder.Append($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [{i}] = {resultfromcommand}");
                                else
                                    resultBuilder.Append($", [{i}] = {resultfromcommand}");
                            }
                        }

                        resultBuilder.Append(" } }");
                        dataforohs = resultBuilder.ToString();
                        resultBuilder = null;
                    }
                    else
                        dataforohs = "{ [\"status\"] = \"fail\" }";
                }
                else
                    dataforohs = "{ [\"status\"] = \"fail\" }";
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[Batch] - Json Format Error - {ex}");
            }

            if (!string.IsNullOrEmpty(dataforohs))
                dataforohs = JaminProcessor.JaminFormat(dataforohs, game);

            return dataforohs;
        }

        public class BatchCommand
        {
            public string? Method { get; set; }
            public JToken? Data { get; set; }
            public string? Project { get; set; }
        }
    }
}
