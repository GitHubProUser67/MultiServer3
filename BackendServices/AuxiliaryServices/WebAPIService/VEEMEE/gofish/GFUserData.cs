using System.Xml;
using CyberBackendLibrary.HTTP;
using WebAPIService.LeaderboardsService.VEEMEE;

namespace WebAPIService.VEEMEE.gofish
{
    public class GFUserData
    {
        public static string? SetUserDataPOST(byte[]? PostData, string? ContentType, string apiPath)
        {
            string key = string.Empty;
            string psnid = string.Empty;
            string score = string.Empty;
            string fishcount = string.Empty;
            string biggestfishweight = string.Empty;
            string totalfishweight = string.Empty;

            if (ContentType == "application/x-www-form-urlencoded" && PostData != null)
            {
                var data = HTTPProcessor.ExtractAndSortUrlEncodedPOSTData(PostData);
                key = data["key"];
                if (key != "tHeHuYUmuDa54qur")
                {
                    CustomLogger.LoggerAccessor.LogError("[VEEMEE] - goalie_sfrgbt - Client tried to push invalid key! Invalidating request.");
                    return null;
                }
                psnid = data["psnid"];
                score = data["score"];
                fishcount = data["fishcount"];
                biggestfishweight = data["biggestfishweight"];
                totalfishweight = data["totalfishweight"];

                Directory.CreateDirectory($"{apiPath}/VEEMEE/gofish/User_Data");

                if (File.Exists($"{apiPath}/VEEMEE/gofish/User_Data/{psnid}.xml"))
                {
                    try
                    {
                        GFScoreBoardData.UpdateScoreBoard(psnid, fishcount, biggestfishweight, totalfishweight, int.Parse(score));
                        GFScoreBoardData.UpdateAllTimeScoreboardXml(); // We finalized edit, so we issue a write.
                        GFScoreBoardData.UpdateTodayScoreboardXml(DateTime.Now.ToString("yyyy_MM_dd")); // We finalized edit, so we issue a write.
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }

                    // Load the XML string into an XmlDocument
                    XmlDocument xmlDoc = new();
                    xmlDoc.LoadXml($"<xml>{File.ReadAllText($"{apiPath}/VEEMEE/gofish/User_Data/{psnid}.xml")}</xml>");

                    // Find the <score> element
                    XmlElement? scoreElement = xmlDoc.SelectSingleNode("/xml/score") as XmlElement;

                    if (scoreElement != null)
                    {
                        // Replace the value of <score> with a new value
                        scoreElement.InnerText = score;

                        // Find the <fishcount> element
                        XmlElement? fishcountElement = xmlDoc.SelectSingleNode("/xml/fishcount") as XmlElement;

                        if (fishcountElement != null)
                        {
                            // Replace the value of <fishcount> with a new value
                            fishcountElement.InnerText = fishcount;

                            // Find the <biggestfishweight> element
                            XmlElement? biggestfishweightElement = xmlDoc.SelectSingleNode("/xml/biggestfishweight") as XmlElement;

                            if (biggestfishweightElement != null)
                            {
                                // Replace the value of <biggestfishweight> with a new value
                                biggestfishweightElement.InnerText = biggestfishweight;

                                // Find the <totalfishweight> element
                                XmlElement? totalfishweightElement = xmlDoc.SelectSingleNode("/xml/totalfishweight") as XmlElement;

                                if (totalfishweightElement != null)
                                {
                                    // Replace the value of <totalfishweight> with a new value
                                    totalfishweightElement.InnerText = totalfishweight;

                                    string XmlResult = xmlDoc.OuterXml.Replace("<xml>", string.Empty).Replace("</xml>", string.Empty);
                                    File.WriteAllText($"{apiPath}/VEEMEE/gofish/User_Data/{psnid}.xml", XmlResult);
                                    return XmlResult;
                                }
                            }
                        }
                    }
                }
                else
                {
                    string XmlData = $"<psnid>{psnid}</psnid><score>{score}</score><fishcount>{fishcount}</fishcount><psnid>{psnid}</psnid><biggestfishweight>{biggestfishweight}</biggestfishweight><totalfishweight>{totalfishweight}</totalfishweight>";
                    File.WriteAllText($"{apiPath}/VEEMEE/gofish/User_Data/{psnid}.xml", XmlData);
                    return XmlData;
                }
            }

            return null;
        }

        public static string? GetUserDataPOST(byte[]? PostData, string? ContentType, string apiPath)
        {
            string key = string.Empty;
            string psnid = string.Empty;

            if (ContentType == "application/x-www-form-urlencoded" && PostData != null)
            {
                var data = HTTPProcessor.ExtractAndSortUrlEncodedPOSTData(PostData);
                key = data["key"];
                if (key != "tHeHuYUmuDa54qur")
                {
                    CustomLogger.LoggerAccessor.LogError("[VEEMEE] - goalie_sfrgbt - Client tried to push invalid key! Invalidating request.");
                    return null;
                }
                psnid = data["psnid"];

                if (File.Exists($"{apiPath}/VEEMEE/gofish/User_Data/{psnid}.xml"))
                    return File.ReadAllText($"{apiPath}/VEEMEE/gofish/User_Data/{psnid}.xml");
            }

            return null;
        }
    }
}
