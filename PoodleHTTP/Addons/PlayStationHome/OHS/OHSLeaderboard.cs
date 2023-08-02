using HttpMultipartParser;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.OHS
{
    public class OHSLeaderboard
    {
        public static async Task<string> leaderboard_requestbyusers(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
        {
            string dataforohs = "";

            if (batchparams == "")
            {
                var data = MultipartFormDataParser.Parse(request.InputStream, Misc.ExtractBoundary(request.ContentType));

                ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                dataforohs = data.GetParameterValue("data");

                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_ENCRYPTEDJAMINVALUE_HERE", dataforohs.Substring(8)));

                    if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                    {
                        dataforohs = await OHSProcessor.requestbyusers(returnValues[0]?.ToString(), directorypath);
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
            {
                dataforohs = await OHSProcessor.requestbyusers(batchparams, directorypath);
            }

            if (batchparams != "")
            {
                return dataforohs;
            }
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\",[\"value\"] = " + dataforohs + " }"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                    {
                        dataforohs = returnValues2nd[0]?.ToString();
                    }
                }
                catch (Exception ex)
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
                    catch (Exception ex)
                    {
                        // Not Important.
                    }
                }
            }

            return "";
        }

        public static async Task<string> leaderboard_requestbyrank(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
        {
            string dataforohs = "";

            if (batchparams == "")
            {
                var data = MultipartFormDataParser.Parse(request.InputStream, Misc.ExtractBoundary(request.ContentType));

                ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                dataforohs = data.GetParameterValue("data");

                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_ENCRYPTEDJAMINVALUE_HERE", dataforohs.Substring(8)));

                    if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                    {
                        dataforohs = await OHSProcessor.requestbyrank(returnValues[0]?.ToString(), directorypath);
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
            {
                dataforohs = await OHSProcessor.requestbyrank(batchparams, directorypath);
            }

            if (dataforohs == "")
            {
                dataforohs = "{ [\"user\"] = { [\"score\"] = 0 }, [\"entries\"] = { } }";
            }

            if (batchparams != "")
            {
                return dataforohs;
            }
            else
            {
                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", "{ [\"status\"] = \"success\", [\"value\"] = " + dataforohs + " }"));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                    {
                        dataforohs = returnValues2nd[0]?.ToString();
                    }
                }
                catch (Exception ex)
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
                    catch (Exception ex)
                    {
                        // Not Important.
                    }
                }
            }

            return "";
        }

        public static async Task<string> leaderboard_update(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
        {
            string resultfromjamin = "";

            string writekey = "11111111";

            if (batchparams == "")
            {
                var data = MultipartFormDataParser.Parse(request.InputStream, Misc.ExtractBoundary(request.ContentType));

                ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                string hasheddataforohs = data.GetParameterValue("data");

                string dataforohs = hasheddataforohs.Substring(8);

                writekey = dataforohs.Substring(0, 8);

                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_ENCRYPTEDJAMINVALUE_HERE", dataforohs.Substring(8)));

                    if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                    {
                        resultfromjamin = returnValues[0]?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
            {
                resultfromjamin = batchparams;

                // TODO! writekey must be somewhere.
            }

            // Deserialize the JSON string
            ScoreBoardUpdate rootObject = JsonConvert.DeserializeObject<ScoreBoardUpdate>(resultfromjamin);

            // Extract the values
            string user = rootObject.user;
            int score = rootObject.score;
            string key = rootObject.key;

            string scoreboardfile = directorypath + $"/scoreboard_{key}.json";

            if (File.Exists(scoreboardfile))
            {
                string tempreader = Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(scoreboardfile, HTTPPrivateKey.HTTPPrivatekey));

                resultfromjamin = await OHSProcessor.UpdateScoreboard(tempreader, user, score, scoreboardfile);
            }

            if (batchparams != "")
            {
                if (resultfromjamin == "")
                {
                    return "";
                }

                return "{ [\"writeKey\"] = \"" + writekey + "\", [\"entries\"] = " + resultfromjamin + " }";
            }
            else
            {
                string returnvalue = "";

                if (resultfromjamin == "")
                {
                    returnvalue = "{ [\"status\"] = \"fail\" }";
                }
                else
                {
                    returnvalue = "{ [\"status\"] = \"success\",[\"value\"] = { [\"writeKey\"] = \"" + writekey + "\", [\"entries\"] = " + resultfromjamin + " } }";
                }

                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", returnvalue));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                    {
                        resultfromjamin = returnValues2nd[0]?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }

                byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{resultfromjamin}</ohs>");

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
                    catch (Exception ex)
                    {
                        // Not Important.
                    }
                }
            }

            return "";
        }

        public static async Task<string> leaderboard_updatessameentry(string directorypath, string batchparams, HttpListenerRequest request, HttpListenerResponse response)
        {
            string resultfromjamin = "";

            string writekey = "11111111";

            if (batchparams == "")
            {
                var data = MultipartFormDataParser.Parse(request.InputStream, Misc.ExtractBoundary(request.ContentType));

                ServerConfiguration.LogInfo($"[OHS] : Client Version - {data.GetParameterValue("version")}");

                string hasheddataforohs = data.GetParameterValue("data");

                string dataforohs = hasheddataforohs.Substring(8);

                writekey = dataforohs.Substring(0, 8);

                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jamindecrypt.Replace("PUT_ENCRYPTEDJAMINVALUE_HERE", dataforohs.Substring(8)));

                    if (!string.IsNullOrEmpty(returnValues[0]?.ToString()))
                    {
                        resultfromjamin = returnValues[0]?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }
            }
            else
            {
                resultfromjamin = batchparams;

                // TODO! writekey must be somewhere.
            }

            // Deserialize the JSON string
            ScoreBoardUpdateSameEntry rootObject = JsonConvert.DeserializeObject<ScoreBoardUpdateSameEntry>(resultfromjamin);

            // Extract the values
            string user = rootObject.user;
            int score = rootObject.score;
            string[] keys = rootObject.keys;

            StringBuilder resultBuilder = new StringBuilder();

            foreach (var key in keys)
            {
                string scoreboardfile = directorypath + $"/scoreboard_{key}.json";

                if (File.Exists(scoreboardfile))
                {
                    string tempreader = Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(scoreboardfile, HTTPPrivateKey.HTTPPrivatekey));

                    if (resultBuilder.Length == 0)
                    {
                        resultBuilder.Append(resultfromjamin = await OHSProcessor.UpdateScoreboard(tempreader, user, score, scoreboardfile));
                    }
                    else
                    {
                        resultBuilder.Append(", " + (resultfromjamin = await OHSProcessor.UpdateScoreboard(tempreader, user, score, scoreboardfile)));
                    }
                }
            }

            if (batchparams != "")
            {
                if (resultBuilder.ToString() == "")
                {
                    return "";
                }

                return "{ [\"writeKey\"] = \"" + writekey + "\", [\"entries\"] = " + resultBuilder.ToString() + " }";
            }
            else
            {
                string returnvalue = "";

                if (resultBuilder.ToString() == "")
                {
                    returnvalue = "{ [\"status\"] = \"fail\" }";
                }
                else
                {
                    returnvalue = "{ [\"status\"] = \"success\",[\"value\"] = { [\"writeKey\"] = \"" + writekey + "\", [\"entries\"] = " + resultBuilder.ToString() + " } }";
                }

                try
                {
                    // Execute the Lua script and get the result
                    object[] returnValues2nd = Misc.ExecuteLuaScript(OHSJaminLuaStaticFiles.jaminencrypt.Replace("PUT_TABLEINPUT_HERE", returnvalue));

                    if (!string.IsNullOrEmpty(returnValues2nd[0]?.ToString()))
                    {
                        resultfromjamin = returnValues2nd[0]?.ToString();
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return "";
                }

                byte[] postresponsetooutput = Encoding.UTF8.GetBytes($"<ohs>{resultfromjamin}</ohs>");

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
                    catch (Exception ex)
                    {
                        // Not Important.
                    }
                }
            }

            return "";
        }
    }
}
