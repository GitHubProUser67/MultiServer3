using System.Xml;
using BackendProject.MiscUtils;

namespace HTTPSecureServerLite.API.VEEMEE.goalie_sfrgbt
{
    public class UserData
    {
        public static string? SetUserDataPOST(byte[]? PostData, string? ContentType, bool global)
        {
            string key = string.Empty;
            string psnid = string.Empty;
            string guest = string.Empty;
            string goals = string.Empty;
            string duration = string.Empty;

            if (ContentType == "application/x-www-form-urlencoded" && PostData != null)
            {
                var data = HTTPUtils.ExtractAndSortUrlEncodedPOSTData(PostData);
                key = data["key"];
                if (key != "d2us7A2EcU2PuBuz")
                {
                    CustomLogger.LoggerAccessor.LogError("[VEEMEE] - goalie_sfrgbt - Client tried to push invalid key! Invalidating request.");
                    return null;
                }
                psnid = data["psnid"];
                guest = data["guest"];
                goals = data["goals"];
                duration = data["duration"];

                string directoryPath = string.Empty;
                string filePath = string.Empty;

                if (global)
                {
                    directoryPath = $"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/goalie/User_Data";
                    filePath = $"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/goalie/User_Data/{psnid}.xml";
                }
                else
                {
                    directoryPath = $"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/sfrgbt/User_Data";
                    filePath = $"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/sfrgbt/User_Data/{psnid}.xml";
                }

                Directory.CreateDirectory(directoryPath);

                if (File.Exists(filePath))
                {
                    try
                    {
                        ScoreBoardData.UpdateScoreBoard(psnid, duration, int.Parse(goals));
                        ScoreBoardData.UpdateAllTimeScoreboardXml(global); // We finalized edit, so we issue a write.
                        ScoreBoardData.UpdateTodayScoreboardXml(global, DateTime.Now.ToString("yyyy_MM_dd")); // We finalized edit, so we issue a write.
                    }
                    catch (Exception)
                    {
                        // Not Important
                    }

                    // Load the XML string into an XmlDocument
                    XmlDocument xmlDoc = new();
                    xmlDoc.LoadXml($"<xml>{File.ReadAllText(filePath)}</xml>");

                    // Find the <goals> element
                    XmlElement? goalsElement = xmlDoc.SelectSingleNode("/xml/goals") as XmlElement;

                    if (goalsElement != null)
                    {
                        // Replace the value of <goals> with a new value
                        goalsElement.InnerText = goals;

                        // Find the <duration> element
                        XmlElement? durationElement = xmlDoc.SelectSingleNode("/xml/duration") as XmlElement;

                        if (durationElement != null)
                        {
                            // Replace the value of <duration> with a new value
                            durationElement.InnerText = duration;

                            string XmlResult = xmlDoc.OuterXml.Replace("<xml>", string.Empty).Replace("</xml>", string.Empty);
                            File.WriteAllText(filePath, XmlResult);
                            return XmlResult;
                        }
                    }
                }
                else
                {
                    string XmlData = $"<scores></scores><entry></entry><psnid>{psnid}</psnid><goals>{goals}</goals><duration>{duration}</duration><paid_goals></paid_goals>";
                    File.WriteAllText(filePath, XmlData);
                    return XmlData;
                }
            }

            return null;
        }

        public static string? GetUserDataPOST(byte[]? PostData, string? ContentType, bool global)
        {
            string key = string.Empty;
            string psnid = string.Empty;

            if (ContentType == "application/x-www-form-urlencoded" && PostData != null)
            {
                var data = HTTPUtils.ExtractAndSortUrlEncodedPOSTData(PostData);
                key = data["key"];
                if (key != "d2us7A2EcU2PuBuz")
                {
                    CustomLogger.LoggerAccessor.LogError("[VEEMEE] - goalie_sfrgbt - Client tried to push invalid key! Invalidating request.");
                    return null;
                }
                psnid = data["psnid"];

                string directoryPath = string.Empty;
                string filePath = string.Empty;

                if (global)
                {
                    directoryPath = $"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/goalie/User_Data";
                    filePath = $"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/goalie/User_Data/{psnid}.xml";
                }
                else
                {
                    directoryPath = $"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/sfrgbt/User_Data";
                    filePath = $"{HTTPSServerConfiguration.APIStaticFolder}/VEEMEE/sfrgbt/User_Data/{psnid}.xml";
                }

                if (File.Exists(filePath))
                    return File.ReadAllText(filePath);
            }

            return null;
        }
    }
}
