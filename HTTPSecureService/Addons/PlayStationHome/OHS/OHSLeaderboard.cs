using HttpMultipartParser;
using MultiServer.HTTPService;
using NetCoreServer;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace MultiServer.HTTPSecureService.Addons.PlayStationHome.OHS
{
    public class OHSLeaderboard
    {
        public static string Leaderboard_RequestByUsers(string directorypath, string batchparams, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string dataforohs = string.Empty;

            if (batchparams == string.Empty)
            {
                string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                    ms.Position = 0;

                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                    dataforohs = OHSProcessor.RequestByUsers(OHSProcessor.JaminDeFormat(data.GetParameterValue("data"), true), directorypath);

                    if (dataforohs == null)
                    {
                        response.SetBegin((int)HttpStatusCode.InternalServerError);
                        response.SetBody();
                        return string.Empty;
                    }

                    ms.Dispose();
                }
            }
            else
                dataforohs = OHSProcessor.RequestByUsers(batchparams, directorypath);

            if (batchparams != string.Empty)
                return dataforohs;
            else
            {
                dataforohs = OHSProcessor.JaminFormat("{ [\"status\"] = \"success\", [\"value\"] = " + dataforohs + " }");

                if (dataforohs == null)
                {
                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                    response.SetBody();
                    return string.Empty;
                }

                response.SetContentType("application/xml;charset=UTF-8");
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody($"<ohs>{dataforohs}</ohs>");
            }

            return string.Empty;
        }

        public static string Leaderboard_RequestByRank(string directorypath, string batchparams, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string dataforohs = string.Empty;

            if (batchparams == string.Empty)
            {
                string boundary = HTTPService.Extensions.ExtractBoundary(HTTPSClass.GetHeaderValue(Headers, "Content-type"));

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(request.BodyBytes, 0, request.BodyBytes.Length);

                    ms.Position = 0;

                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                    dataforohs = OHSProcessor.RequestByRank(OHSProcessor.JaminDeFormat(data.GetParameterValue("data"), true), directorypath);

                    if (dataforohs == null)
                    {
                        response.SetBegin((int)HttpStatusCode.InternalServerError);
                        response.SetBody();
                        return string.Empty;
                    }

                    ms.Dispose();
                }
            }
            else
                dataforohs = OHSProcessor.RequestByRank(batchparams, directorypath);

            if (dataforohs == string.Empty)
                dataforohs = "{ [\"user\"] = { [\"score\"] = 0 }, [\"entries\"] = { } }";

            if (batchparams != string.Empty)
                return dataforohs;
            else
            {
                dataforohs = OHSProcessor.JaminFormat("{ [\"status\"] = \"success\", [\"value\"] = " + dataforohs + " }");

                if (dataforohs == null)
                {
                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                    response.SetBody();
                    return string.Empty;
                }

                response.SetContentType("application/xml;charset=UTF-8");
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody($"<ohs>{dataforohs}</ohs>");
            }

            return string.Empty;
        }

        public static string Leaderboard_Update(string directorypath, string batchparams, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string dataforohs = string.Empty;

            string writekey = "11111111";

            if (batchparams == string.Empty)
            {
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
                        return string.Empty;
                    }

                    ms.Dispose();
                }
            }
            else
                dataforohs = batchparams;
            // TODO! writekey must be somewhere.

            // Deserialize the JSON string
            ScoreBoardUpdate rootObject = JsonConvert.DeserializeObject<ScoreBoardUpdate>(dataforohs);

            if (rootObject != null)
            {
                // Extract the values
                string user = rootObject.user;
                int score = rootObject.score;
                string key = rootObject.key;

                string scoreboardfile = directorypath + $"/scoreboard_{key}.json";

                if (File.Exists(scoreboardfile))
                {
                    string tempreader = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(scoreboardfile, HTTPPrivateKey.HTTPPrivatekey));

                    if (tempreader != null)
                        dataforohs = OHSProcessor.UpdateScoreboard(tempreader, user, score, scoreboardfile);
                }
            }

            if (batchparams != string.Empty)
            {
                if (dataforohs == string.Empty)
                    return string.Empty;
                else
                    return "{ [\"writeKey\"] = \"" + writekey + "\", [\"entries\"] = " + dataforohs + " }";
            }
            else
            {
                if (dataforohs == string.Empty)
                    dataforohs = "{ [\"status\"] = \"fail\" }";
                else
                    dataforohs = "{ [\"status\"] = \"success\", [\"value\"] = { [\"writeKey\"] = \"" + writekey + "\", [\"entries\"] = " + dataforohs + " } }";

                dataforohs = OHSProcessor.JaminFormat(dataforohs);

                if (dataforohs == null)
                {
                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                    response.SetBody();
                    return string.Empty;
                }

                response.SetContentType("application/xml;charset=UTF-8");
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody($"<ohs>{dataforohs}</ohs>");
            }

            return string.Empty;
        }

        public static string Leaderboard_UpdatesSameEntry(string directorypath, string batchparams, HttpRequest request, HttpResponse response, (string HeaderIndex, string HeaderItem)[] Headers)
        {
            string dataforohs = string.Empty;

            string writekey = "11111111";

            if (batchparams == string.Empty)
            {
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
                        return string.Empty;
                    }

                    ms.Dispose();
                }
            }
            else
                dataforohs = batchparams;
            // TODO! writekey must be somewhere.

            // Deserialize the JSON string
            ScoreBoardUpdateSameEntry rootObject = JsonConvert.DeserializeObject<ScoreBoardUpdateSameEntry>(dataforohs);

            StringBuilder resultBuilder = new StringBuilder();

            if (rootObject != null)
            {
                // Extract the values
                string user = rootObject.user;
                int score = rootObject.score;
                string[] keys = rootObject.keys;

                foreach (var key in keys)
                {
                    string scoreboardfile = directorypath + $"/scoreboard_{key}.json";

                    if (File.Exists(scoreboardfile))
                    {
                        string tempreader = Encoding.UTF8.GetString(FileHelper.CryptoReadAsync(scoreboardfile, HTTPPrivateKey.HTTPPrivatekey));

                        if (tempreader != null)
                        {
                            if (resultBuilder.Length == 0)
                                resultBuilder.Append(OHSProcessor.UpdateScoreboard(tempreader, user, score, scoreboardfile));
                            else
                                resultBuilder.Append(", " + OHSProcessor.UpdateScoreboard(tempreader, user, score, scoreboardfile));
                        }
                    }
                }
            }

            if (batchparams != string.Empty)
            {
                if (resultBuilder.Length == 0)
                    return string.Empty;
                else
                    return "{ [\"writeKey\"] = \"" + writekey + "\", [\"entries\"] = " + resultBuilder.ToString() + " }";
            }
            else
            {
                if (resultBuilder.Length == 0)
                    dataforohs = "{ [\"status\"] = \"fail\" }";
                else
                    dataforohs = "{ [\"status\"] = \"success\", [\"value\"] = { [\"writeKey\"] = \"" + writekey + "\", [\"entries\"] = " + resultBuilder.ToString() + " } }";

                dataforohs = OHSProcessor.JaminFormat(dataforohs);

                if (dataforohs == null)
                {
                    response.SetBegin((int)HttpStatusCode.InternalServerError);
                    response.SetBody();
                    return string.Empty;
                }

                response.SetContentType("application/xml;charset=UTF-8");
                response.SetBegin((int)HttpStatusCode.OK);
                response.SetBody($"<ohs>{dataforohs}</ohs>");
            }

            return string.Empty;
        }
    }
}
