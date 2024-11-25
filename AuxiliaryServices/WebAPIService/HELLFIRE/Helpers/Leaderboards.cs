using CustomLogger;
using HttpMultipartParser;
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

        public static string GetLeaderboardsClearasil(byte[] PostData, string boundary, string UserID, string WorkPath)
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
        public static string GetLeaderboardsSlimJim(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string path = $"{WorkPath}/SlimJim/User_Data";

            var playerDataFiles = Directory.GetFiles(path);

            // Create an XmlDocument
            var doc = new XmlDocument();
            doc.LoadXml("<Response><table type=\"table\" classname=\"SlimJimLeaderboards\"></table></Response>");

            foreach (var playerData in playerDataFiles)
            {
                if (!File.Exists(playerData))
                {
                    //If file doesn't exist continue foreach
                    continue;
                }

                // Load the XML file
                XmlDocument doc2 = new XmlDocument();
                string xmlProfile = File.ReadAllText(playerData);
                doc2.LoadXml("<root>" + xmlProfile + "</root>");

                // Get all LeaderboardScore elements
                XmlNodeList leaderboardScoreNodeList = doc2.GetElementsByTagName("LeaderboardScore");

                foreach (XmlNode lbScoreNode in leaderboardScoreNodeList)
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

        public static string GetLeaderboardsNovusPrime(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string LBPath = $"{WorkPath}/NovusPrime/User_Data/Leaderboards.xml";
            //var playerDataFiles = Directory.GetFiles(path);

            #region Template LB to start with
            string templateLB = @"<GalaxyRider42>
    <DisplayName>GalaxyRider42</DisplayName>
    <Score>0</Score>
</GalaxyRider42>
<ShadowHunterX>
    <DisplayName>ShadowHunterX</DisplayName>
    <Score>0</Score>
</ShadowHunterX>
<NeonNova77>
    <DisplayName>NeonNova77</DisplayName>
    <Score>0</Score>
</NeonNova77>
<IronWraith89>
    <DisplayName>IronWraith89</DisplayName>
    <Score>0</Score>
</IronWraith89>
<PixelPhantom5>
    <DisplayName>PixelPhantom5</DisplayName>
    <Score>0</Score>
</PixelPhantom5>
<BlazeFalconer>
    <DisplayName>BlazeFalconer</DisplayName>
    <Score>0</Score>
</BlazeFalconer>
<QuantumLynx92>
    <DisplayName>QuantumLynx92</DisplayName>
    <Score>0</Score>
</QuantumLynx92>
<HyperBolt13>
    <DisplayName>HyperBolt13</DisplayName>
    <Score>0</Score>
</HyperBolt13>
<FrostEcho99>
    <DisplayName>FrostEcho99</DisplayName>
    <Score>0</Score>
</FrostEcho99>
<VoidTrekker11>
    <DisplayName>VoidTrekker11</DisplayName>
    <Score>0</Score>
</VoidTrekker11>";
            #endregion

            using (MemoryStream ms = new MemoryStream(PostData))
            {
                var data = MultipartFormDataParser.Parse(ms, boundary);
                string UserNovusPrimeID = data.GetParameterValue("UserID");
                ms.Flush();
            }

            // Create an XmlDocument
            var doc = new XmlDocument();

            if (File.Exists(LBPath))
            {
                doc.LoadXml($"<Response>{File.ReadAllText(LBPath)}</Response>");
            } else
            {
                doc.LoadXml($"<Response></Response>");
            }
            /*
            // Load the XML file
            XmlDocument doc2 = new XmlDocument();
            string xmlProfile = File.ReadAllText(path);
            doc2.LoadXml("<root>" + xmlProfile + "</root>");

            // Get all LeaderboardScore elements
            XmlNodeList leaderboardScoreNodeList = doc2.GetElementsByTagName("Score");

            foreach (XmlNode lbScoreNode in leaderboardScoreNodeList)
            {
                if (lbScoreNode != null)
                {
                    // Parse the integer value
                    if (float.TryParse(lbScoreNode.InnerText, out float score))
                    {
                        // Use the score value here to display
                        LoggerAccessor.LogInfo($"Leaderboard {Path.GetFileNameWithoutExtension(playerData)} Score:  {score}");
                        doc.SelectSingleNode("//table").InnerXml += $"<DisplayName>{Path.GetFileNameWithoutExtension(playerData)}</DisplayName><Score>{score}</Score>";
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
            */

            return doc.InnerXml;
        }

        public static string GetGlobalPopulationLeaderboard(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            /*
            string path = $"{WorkPath}/TYCOON/User_Data";

            var playerDataFiles = Directory.GetFiles(path);

            // Create an XmlDocument
            var doc = new XmlDocument();
            doc.LoadXml("<Response><table type=\"table\" classname=\"GlobalPopulationLeaderboard\"></table></Response>");

            foreach (var playerData in playerDataFiles)
            {
                if (!File.Exists(playerData))
                {
                    //If file doesn't exist continue foreach
                    continue;
                }

                // Load the XML file
                XmlDocument doc2 = new XmlDocument();
                string xmlProfile = File.ReadAllText(playerData);
                doc2.LoadXml("<root>" + xmlProfile + "</root>");

                // Get all LeaderboardScore elements
                XmlNodeList leaderboardScoreNodeList = doc2.GetElementsByTagName("LeaderboardScore");

                foreach (XmlNode lbScoreNode in leaderboardScoreNodeList)
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
            */



            return @"<Response>
        <Entry>
            <DisplayName>AgentDark447</DisplayName>
            <GlobalPop>10000</GlobalPop>
        </Entry>
        <Entry>
            <DisplayName>JumpSuitDev</DisplayName>
            <GlobalPop>9500</GlobalPop>
        </Entry>
</Response>";
        }

        public static string GetGlobalRevenueCollectedLeaderboard(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            /*
            string path = $"{WorkPath}/TYCOON/User_Data";

            var playerDataFiles = Directory.GetFiles(path);

            // Create an XmlDocument
            var doc = new XmlDocument();
            doc.LoadXml("<Response><table type=\"table\" classname=\"GlobalPopulationLeaderboard\"></table></Response>");

            foreach (var playerData in playerDataFiles)
            {
                if (!File.Exists(playerData))
                {
                    //If file doesn't exist continue foreach
                    continue;
                }

                // Load the XML file
                XmlDocument doc2 = new XmlDocument();
                string xmlProfile = File.ReadAllText(playerData);
                doc2.LoadXml("<root>" + xmlProfile + "</root>");

                // Get all LeaderboardScore elements
                XmlNodeList leaderboardScoreNodeList = doc2.GetElementsByTagName("LeaderboardScore");

                foreach (XmlNode lbScoreNode in leaderboardScoreNodeList)
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
            */



            return @"<Response>
        <Entry>
            <DisplayName>AgentDark447</DisplayName>
            <TotalCollected>10000</TotalCollected>
        </Entry>
        <Entry>
            <DisplayName>JumpSuitDev</DisplayName>
            <TotalCollected>9500</TotalCollected>
        </Entry>
</Response>";
        }
    }
}
