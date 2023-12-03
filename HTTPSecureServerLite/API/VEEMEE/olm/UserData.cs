using System.Xml;

namespace HTTPSecureServerLite.API.VEEMEE.olm
{
    public  class UserData
    {
        public static string? SetUserDataPOST(byte[]? PostData, string? ContentType)
        {
            string key = string.Empty;
            string psnid = string.Empty;
            string score = string.Empty;
            string throws = string.Empty;

            if (ContentType == "application/x-www-form-urlencoded" && PostData != null)
            {
                var data = CryptoSporidium.HTTPUtils.ExtractAndSortUrlEncodedPOSTData(PostData);
                key = data["key"];
                if (key != "KEqZKh3At4Ev")
                {
                    CustomLogger.LoggerAccessor.LogError("[VEEMEE] - olm - Client tried to push invalid key! Invalidating request.");
                    return null;
                }
                psnid = data["psnid"];
                score = data["score"];
                throws = data["throws"];

                Directory.CreateDirectory($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/olm/User_Data");

                if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/olm/User_Data/{psnid}.xml"))
                {
                    /*try
                    {
                        ScoreBoardData.UpdateScoreBoard(psnid, throws, int.Parse(score));
                        ScoreBoardData.UpdateAllTimeScoreboardXml(); // We finalized edit, so we issue a write.
                        ScoreBoardData.UpdateTodayScoreboardXml(DateTime.Now.ToString("yyyy_MM_dd")); // We finalized edit, so we issue a write.
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }*/

                    // Load the XML string into an XmlDocument
                    XmlDocument xmlDoc = new();
                    xmlDoc.LoadXml($"<xml>{File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/olm/User_Data/{psnid}.xml")}</xml>");

                    // Find the <score> element
                    XmlElement? scoreElement = xmlDoc.SelectSingleNode("/xml/score") as XmlElement;

                    if (scoreElement != null)
                    {
                        // Replace the value of <score> with a new value
                        scoreElement.InnerText = score;

                        // Find the <throws> element
                        XmlElement? throwsElement = xmlDoc.SelectSingleNode("/xml/throws") as XmlElement;

                        if (throwsElement != null)
                        {
                            // Replace the value of <throws> with a new value
                            throwsElement.InnerText = throws;

                            string XmlResult = xmlDoc.OuterXml.Replace("<xml>", string.Empty).Replace("</xml>", string.Empty);
                            File.WriteAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/olm/User_Data/{psnid}.xml", XmlResult);
                            return XmlResult;
                        }
                    }
                }
                else
                {
                    string XmlData = $"<psnid>{psnid}</psnid><score>{score}</score><throws>{throws}</throws>";
                    File.WriteAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/olm/User_Data/{psnid}.xml", XmlData);
                    return XmlData;
                }
            }

            return null;
        }

        public static string? GetUserDataPOST(byte[]? PostData, string? ContentType)
        {
            string key = string.Empty;
            string psnid = string.Empty;

            if (ContentType == "application/x-www-form-urlencoded" && PostData != null)
            {
                var data = CryptoSporidium.HTTPUtils.ExtractAndSortUrlEncodedPOSTData(PostData);
                key = data["key"];
                if (key != "KEqZKh3At4Ev")
                {
                    CustomLogger.LoggerAccessor.LogError("[VEEMEE] - olm - Client tried to push invalid key! Invalidating request.");
                    return null;
                }
                psnid = data["psnid"];

                if (File.Exists($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/olm/User_Data/{psnid}.xml"))
                    return File.ReadAllText($"{HTTPSServerConfiguration.HTTPSStaticFolder}/VEEMEE/olm/User_Data/{psnid}.xml");
            }

            return null;
        }
    }
}
