using CustomLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace WebAPIService.HELLFIRE.Helpers
{
    public class Leaderboards
    {

        public static string GetLeaderboards(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string path = $"{WorkPath}/ClearasilSkater/User_Data";

            var playerDataFiles = Directory.GetFiles(path);
    
            // Create an XmlDocument
            var doc = new XmlDocument();
            doc.LoadXml("<Response><table type=\"table\" classname=\"ClearasilLeaderboards\"></table></Response>");

            foreach (var playerData in playerDataFiles)
            {
                if(!File.Exists(playerData)) {
                    //If file doesn't exist continue foreach
                    continue;
                }

                // Load the XML file
                XmlDocument doc2 = new XmlDocument();
                string xmlProfile = File.ReadAllText(playerData);
                doc2.LoadXml("<root>" + xmlProfile + "</root>");

                // Get all LeaderboardScore elements
                XmlNodeList leaderboardScoreNodeList = doc2.GetElementsByTagName("LeaderboardScore");

                foreach(XmlNode lbScoreNode in leaderboardScoreNodeList)
                {
                    if (lbScoreNode != null)
                    {
                        // Parse the integer value
                        if (float.TryParse(lbScoreNode.InnerText, out float score))
                        {
                            // Use the score value here to display
                            LoggerAccessor.LogInfo($"Leaderboard {Path.GetFileNameWithoutExtension(playerData)} Score:  {score}");
                            doc.SelectSingleNode("//table").InnerXml += $"<DisplayName>{Path.GetFileNameWithoutExtension(playerData)}</DisplayName><LeaderboardScore>{score}</LeaderboardScore>";
                        }
                        else
                        {
                            LoggerAccessor.LogError("[HFGAMEs] - Unable to parse LeaderboardScore value as an float.");
                        }
                    }
                    else
                    {
                        LoggerAccessor.LogError("[HFGAMEs] - LeaderboardScore element not found in the XML.");
                    }
                }

                
            }

            return doc.OuterXml;
        }
    }
}
