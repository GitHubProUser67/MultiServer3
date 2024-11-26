using CustomLogger;
using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace WebAPIService.HELLFIRE.Helpers
{
    public class User
    {
        public static string DefaultHomeTycoonProfile = @"<NewPlayer>1</NewPlayer>
            <TotalCollected>0.000000</TotalCollected>
            <Wallet>0.000000</Wallet>
            <Workers>99.000000</Workers>
            <GoldCoins>999999.000000</GoldCoins>
            <SilverCoins>999999.000000</SilverCoins>
            <Options><MusicVolume>1.0</MusicVolume><PrivacySetting>3</PrivacySetting></Options>
            <Missions></Missions>
            <Journal></Journal>
            <Dialogs><Entry>Dialog_S01M00</Entry><Entry>MissionS01M02</Entry><Entry>MissionS01M03</Entry></Dialogs>
            <Unlocked></Unlocked>
            <Activities></Activities>
            <Expansions></Expansions>
            <Vehicles></Vehicles>
            <Flags></Flags>
            <Inventory></Inventory>";

        public static string DefaultClearasilSkaterProfile = "<BestScoreStage1>0</BestScoreStage1><BestScoreStage2>0</BestScoreStage2><LeaderboardScore>0</LeaderboardScore>";

        public static string GetUserHomeTycoon(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            // Retrieve the user's JSON profile
            string profilePath = $"{WorkPath}/TYCOON/User_Data/{UserID}.xml";

            string xmlProfile;
            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);
            else
                xmlProfile = DefaultHomeTycoonProfile;

            return $"<Response>{xmlProfile}</Response>";
        }

        public static string AddUnlocked(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string BuildingName = string.Empty;

            if (PostData != null && !string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    BuildingName = data.GetParameterValue("BuildingName");
                    ms.Flush();
                }
                LoggerAccessor.LogInfo($"BuildingName: {BuildingName}");

                DateTime CurrentTime = DateTime.Now;

                if (File.Exists($"{WorkPath}/TYCOON/User_Data/{UserID}.xml"))
                    File.WriteAllText($"{WorkPath}/TYCOON/User_Data/{UserID}.xml",
                    Regex.Replace(File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}.xml"),
                    $"(<Unlocked><Entry>).*?(</Entry></Unlocked>)",
                    $"<Unlocked><Entry>{BuildingName}</Entry></Unlocked>"));

                return "<Response>></Response>";
            }

            return "<Response></Response>";
        }

        public static string SetPrivacy(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string TownID = string.Empty;
            string NewSetting = string.Empty;

            if (PostData != null && !string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    TownID = data.GetParameterValue("TownID");
                    NewSetting = data.GetParameterValue("NewSetting");
                    ms.Flush();
                }


                LoggerAccessor.LogInfo($"BuildingData: {TownID} \n NewSetting: {NewSetting}");

                DateTime CurrentTime = DateTime.Now;

                if (File.Exists($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"))
                    File.WriteAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml",
                    Regex.Replace(File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{TownID}.xml"),
                    $"(<PrivacySetting>).*?(</PrivacySetting>)",
                    $"<PrivacySetting>{NewSetting}</PrivacySetting>"));

                //return $"<Response><TimeBuilt>{CurrentTime}</TimeBuilt><Orientation>{Orientation}</Orientation><Index>{Index}</Index><Type>{Type}</Type></Response>";

                //idk why, but we can ban people from changing their privacy!
                return "<Response><Banned>0</Banned></Response>";
            }

            return "<Response><Banned>0</Banned></Response>";
        }

        public static string UpdateUserHomeTycoon(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string xmlProfile = string.Empty;
            string updatedXMLProfile = string.Empty;

            // Retrieve the user's JSON profile
            string profilePath = $"{WorkPath}/TYCOON/User_Data/{UserID}.xml";

            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);
            else
                xmlProfile = DefaultHomeTycoonProfile;

            try
            {
                // Create an XmlDocument
                var doc = new XmlDocument();
                doc.LoadXml("<root>" + xmlProfile + "</root>"); // Wrap the XML string in a root element
                if (doc != null && PostData != null && !string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        // Update the profile values from the provided data
                        // Update the values based on IDs
#pragma warning disable 8602
                        doc.SelectSingleNode("//TotalCollected").InnerText = data.GetParameterValue("TotalCollected");
                        doc.SelectSingleNode("//Wallet").InnerText = data.GetParameterValue("Wallet");
                        doc.SelectSingleNode("//Workers").InnerText = data.GetParameterValue("Workers");
                        doc.SelectSingleNode("//GoldCoins").InnerText = data.GetParameterValue("GoldCoins");
                        doc.SelectSingleNode("//SilverCoins").InnerText = data.GetParameterValue("SilverCoins");
                        doc.SelectSingleNode("//NewPlayer").InnerText = data.GetParameterValue("NewPlayer");
                        var jsonObject = JToken.Parse(data.GetParameterValue("Options"));
                        var fieldValues = jsonObject.ToObject<Dictionary<string, object>>();

                        if (fieldValues != null)
                        {
                            foreach (var fieldValue in fieldValues)
                            {
                                if (fieldValue.Key == "MusicVolume" && fieldValue.Value != null)
                                    doc.SelectSingleNode("//Options/MusicVolume").InnerText = fieldValue.Value.ToString() ?? "1.0";
                            }
                        }

                        // Get the updated XML string
                        updatedXMLProfile = doc.DocumentElement.InnerXml.Replace("<root>", string.Empty).Replace("</root>", string.Empty);
#pragma warning restore 8602
                        // Save the updated profile back to the file
                        File.WriteAllText(profilePath, updatedXMLProfile);

                        ms.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[HFGAMES] User - An assertion was thrown in UpdateUser : {ex}");
            }

            return $"<Response>{updatedXMLProfile}</Response>";
        }

        public static string UpdateUserClearasilSkater(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string xmlProfile = string.Empty;
            string updatedXMLProfile = string.Empty;

            // Retrieve the user's JSON profile
            string profilePath = $"{WorkPath}/ClearasilSkater/User_Data/{UserID}.xml";

            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);
            else
                xmlProfile = DefaultClearasilSkaterProfile;

            try
            {
                // Create an XmlDocument
                var doc = new XmlDocument();
                doc.LoadXml("<root>" + xmlProfile + "</root>"); // Wrap the XML string in a root element
                if (doc != null && PostData != null && !string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        // Update the profile values from the provided data
                        // Update the values based on IDs
#pragma warning disable 8602
                        doc.SelectSingleNode("//BestScoreStage1").InnerText = data.GetParameterValue("BestScoreStage1");
                        doc.SelectSingleNode("//BestScoreStage2").InnerText = data.GetParameterValue("BestScoreStage2");
                        doc.SelectSingleNode("//LeaderboardScore").InnerText = data.GetParameterValue("LeaderboardScore");
                        
                        // Get the updated XML string
                        updatedXMLProfile = doc.DocumentElement.InnerXml.Replace("<root>", string.Empty).Replace("</root>", string.Empty);
#pragma warning restore 8602
                        // Save the updated profile back to the file
                        File.WriteAllText(profilePath, updatedXMLProfile);

                        ms.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[HFGAMES] User - An assertion was thrown in UpdateUser : {ex}");
            }

            return $"<Response>{updatedXMLProfile}</Response>";
        }

        public static string RequestUserClearasilSkater(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            // Retrieve the user's JSON profile
            string profilePath = $"{WorkPath}/ClearasilSkater/User_Data/{UserID}.xml";

            string xmlProfile;
            if (File.Exists(profilePath))
            {
                LoggerAccessor.LogInfo($"[HFGAMES] - Detected existing player data, sending!");
                xmlProfile = File.ReadAllText(profilePath);
            }
            else
            {
                LoggerAccessor.LogInfo($"[HFGAMES] - New player with no player data! Using default!");
                xmlProfile = DefaultClearasilSkaterProfile;
            }

            return $"<Response>{xmlProfile}</Response>";
        }
    }
}
