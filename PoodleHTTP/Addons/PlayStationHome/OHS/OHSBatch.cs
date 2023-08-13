using HttpMultipartParser;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.OHS
{
    public class OHSBatch
    {
        public static async Task<string> batch_process(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
        {
            var multipartdata = MultipartFormDataParser.Parse(request.InputStream, Extensions.ExtractBoundary(request.ContentType));

            ServerConfiguration.LogInfo($"[OHS] : Client Version - {multipartdata.GetParameterValue("version")}");

            string dataforohs = multipartdata.GetParameterValue("data");

            try
            {
                // Execute the Lua script and get the result
                object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_ENCRYPTEDJAMINVALUE_HERE", dataforohs.Substring(8)));

                if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                {
                    dataforohs = returnValues[0]?.ToString();
                }
            }
            catch (Exception)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "";
            }

            // Deserialize the JSON data into a list of commands.
            var commands = JsonConvert.DeserializeObject<BatchCommand[]>(dataforohs);

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
                {
                    project = "dummy";
                }

                ServerConfiguration.LogInfo($"[OHS] : {request.UserAgent} Requested a /batch/ method, here are the details : method | {method} - project | {project} - data | {data}");

                switch (method)
                {
                    case "community/getscore/":

                        resultfromcommand = await OHSCommunity.community_getscore(directorypath + $"/{project}/", data, request, response);

                        break;

                    case "community/updatescore/":

                        resultfromcommand = await OHSCommunity.community_updatescore(directorypath + $"/{project}/", data, request, response);

                        break;
                    case "global/getall/":

                        resultfromcommand = await OHSUser.get_all(directorypath + $"/{project}/", data, true, request, response);

                        break;

                    case "global/get/":

                        resultfromcommand = await OHSUser.get(directorypath + $"/{project}/", data, true, request, response);

                        break;

                    case "global/set/":

                        resultfromcommand = await OHSUser.set(directorypath + $"/{project}/", data, true, request, response);

                        break;

                    case "userid/":

                        resultfromcommand = await OHSUser.user_id(data, request, response);

                        break;

                    case "user/getall/":

                        resultfromcommand = await OHSUser.get_all(directorypath + $"/{project}/", data, false, request, response);

                        break;

                    case "user/get/":

                        resultfromcommand = await OHSUser.get(directorypath + $"/{project}/", data, false, request, response);

                        break;

                    case "user/set/":

                        resultfromcommand = await OHSUser.set(directorypath + $"/{project}/", data, false, request, response);

                        break;

                    case "user/getwritekey/":

                        resultfromcommand = await OHSUser.user_getwritekey(data, request, response);

                        break;

                    case "leaderboard/requestbyusers/":

                        resultfromcommand = await OHSLeaderboard.leaderboard_requestbyusers(directorypath + $"/{project}/", data, request, response);

                        break;

                    case "leaderboard/requestbyrank/":

                        resultfromcommand = await OHSLeaderboard.leaderboard_requestbyrank(directorypath + $"/{project}/", data, request, response);

                        break;

                    case "leaderboard/update/":

                        resultfromcommand = await OHSLeaderboard.leaderboard_update(directorypath + $"/{project}/", data, request, response);

                        break;

                    case "leaderboard/updatessameentry/":

                        resultfromcommand = await OHSLeaderboard.leaderboard_updatessameentry(directorypath + $"/{project}/", data, request, response);

                        break;

                    default:

                        Console.WriteLine($"OHS Server : Batch requested a method I don't know about, please report it to GITHUB {method} in {project} with data {data}");

                        break;
                }

                if (resultfromcommand == "")
                {
                    resultfromcommand = "{ [\"status\"] = \"fail\" }";
                }

                if (resultBuilder.Length == 0)
                {
                    resultBuilder.Append($"{{ [\"status\"] = \"success\", [\"value\"] = {{ [{i}] = {resultfromcommand}");
                }
                else
                {
                    resultBuilder.Append($", [{i}] = {resultfromcommand}");
                }
            }

            resultBuilder.Append(" } }");

            dataforohs = resultBuilder.ToString();

            try
            {
                // Execute the Lua script and get the result
                object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", dataforohs));

                if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                {
                    dataforohs = returnValues2nd[0]?.ToString();
                }
            }
            catch (Exception)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "";
            }

            byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{dataforohs}</ohs>");

            response.Headers.Set("Content-Type", "application/xml;charset=UTF-8");
            response.StatusCode = (int)HttpStatusCode.OK;

            if (response.OutputStream.CanWrite)
            {
                try
                {
                    response.ContentLength64 = postresponsetooutput.Length;
                    response.OutputStream.Write(postresponsetooutput, 0, postresponsetooutput.Length);
                    response.OutputStream.Close();
                }
                catch (Exception)
                {
                    // Not Important.
                }
            }

            return "";
        }
    }
}
