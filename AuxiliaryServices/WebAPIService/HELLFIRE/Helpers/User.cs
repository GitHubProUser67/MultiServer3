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
            <Wallet>5000.000000</Wallet>
            <Workers>99.000000</Workers>
            <GoldCoins>9999.000000</GoldCoins>
            <SilverCoins>0.000000</SilverCoins>
            <Options><MusicVolume>1.0</MusicVolume><PrivacySetting>3</PrivacySetting></Options>
            <Missions></Missions>
            <Journal></Journal>
            <Dialogs><Dialog_S01M00>Dialog_S01M00</Dialog_S01M00></Dialogs>
            <Unlocked></Unlocked>
            <Activities></Activities>
            <Expansions></Expansions>
            <Vehicles></Vehicles>
            <Flags></Flags>
            <Inventory></Inventory>";

        public static string DefaultNovusPrimeProfile = @"
            <CharData>
                <Nebulon>0</Nebulon>
                <TotalNebulonEver>0</TotalNebulonEver>
                <Experience>0</Experience>
                <Level>1</Level>
            </CharData>
                <Front1>0</Front1>
                <Front2>0</Front2>
                <Turret1>0</Turret1>
                <Turret2>0</Turret2>
                <Special>0</Special>
                <Maneuver>0</Maneuver>
                <Upgrade1>0</Upgrade1>
                <Upgrade2>0</Upgrade2>
                <Upgrade3>0</Upgrade3>
                <Upgrade4>0</Upgrade4>
                <PaintJob>0</PaintJob>
            </ShipConfig>
            <Inventory></Inventory>
            <Missions>
            </Missions>
            <DailyAvailable>
            </DailyAvailable>";

        public static string DefaultClearasilSkaterAndSlimJimProfile = "<BestScoreStage1>0</BestScoreStage1><BestScoreStage2>0</BestScoreStage2><LeaderboardScore>0</LeaderboardScore>";
        public static string DefaultPokerProfile = "<Bankroll>1000</Bankroll><NewPlayer>1</NewPlayer>";


        public static string GetUserHomeTycoon(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string profilePath = $"{WorkPath}/TYCOON/User_Data/{UserID}/Profile.xml";

            string xmlProfile;
            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);
            else
                xmlProfile = DefaultHomeTycoonProfile;

            return $"<Response>{xmlProfile}</Response>";
        }


        public static string UpdateUserHomeTycoon(byte[] PostData, string boundary, string UserID, string WorkPath, string cmd)
        {
            string xmlProfile = string.Empty;
            string updatedXMLProfile = string.Empty;

            // Retrieve the user's JSON profile
            string profilePath = $"{WorkPath}/TYCOON/User_Data/{UserID}/Profile.xml";

            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);
            else
                xmlProfile = DefaultHomeTycoonProfile;

            try
            {
                // Create an XmlDocument
                var doc = new XmlDocument();
                doc.LoadXml("<xml>" + xmlProfile + "</xml>"); // Wrap the XML string in a root element
                if (doc != null && PostData != null && !string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        // Update the profile values from the provided data
                        // Update the values based on IDs
#pragma warning disable 8602
                        switch (cmd)
                        {
                            case "AddUnlocked":
                                {
                                    var userProfileUnlockNode = doc.SelectSingleNode("//Unlocked");

                                    //We check if HomeTycoon sends multiple AddUnlockeds for same building.. we don't need dupe entries.
                                    if (userProfileUnlockNode.SelectSingleNode(data.GetParameterValue("BuildingName")) != null)
                                    {
                                        return "<Response></Response>";
                                    }

                                    XmlElement BuildingToAddEntry = doc.CreateElement(data.GetParameterValue("BuildingName"));
                                    BuildingToAddEntry.InnerText = data.GetParameterValue("BuildingName");

                                    userProfileUnlockNode.AppendChild(BuildingToAddEntry);
#if DEBUG
                                    LoggerAccessor.LogInfo($"Building to add: {BuildingName}");
#endif
                                }
                                break;
                            case "RemoveUnlocked":
                                {
                                    var userProfileUnlockNode = doc.DocumentElement.SelectSingleNode("//Unlocked");

                                    if (userProfileUnlockNode != null)
                                    {
                                        var buildingNode = userProfileUnlockNode.SelectSingleNode(data.GetParameterValue("BuildingName"));
                                        if (buildingNode != null)
                                        {
                                            doc.DocumentElement.SelectSingleNode("//Unlocked").RemoveChild(buildingNode);

#if DEBUG
                                            LoggerAccessor.LogInfo($"Building removed: {data.GetParameterValue("BuildingName")}");
#endif
                                        }
                                        else
                                        {
                                            LoggerAccessor.LogWarn($"Building not found: {data.GetParameterValue("BuildingName")}");
                                        }
                                    }
                                    else
                                    {
                                        LoggerAccessor.LogInfo("Unlocked node not found in the XML.");
                                    }
                                }
                                break;
                            case "AddDialog":
                                {
                                    var userProfileDialogNode = doc.SelectSingleNode("//Dialogs");
                                    XmlElement DialogToAdd = doc.CreateElement(data.GetParameterValue("DialogName"));
                                    DialogToAdd.InnerText = data.GetParameterValue("DialogName");
                                    userProfileDialogNode.AppendChild(DialogToAdd);
                                }
                                break;
                            case "CompleteDialog":
                                {
                                    var userProfileDialogNode = doc.DocumentElement.SelectSingleNode("//Dialogs").SelectSingleNode(data.GetParameterValue("DialogName"));
                                    doc.DocumentElement.SelectSingleNode("//Dialogs").RemoveChild(userProfileDialogNode);
#if DEBUG
                                    LoggerAccessor.LogInfo($"Removed Dialog {data.GetParameterValue("DialogName")}");
#endif
                                }
                                break;
                            case "AddVehicle":
                                {
                                    // Update the profile values from the provided data
                                    var userProfileVehicleNode = doc.SelectSingleNode("//Vehicles");
                                    XmlElement VeicleToAdd = doc.CreateElement(data.GetParameterValue("VehicleName"));
                                    VeicleToAdd.InnerText = data.GetParameterValue("VehicleName");
                                    userProfileVehicleNode.AppendChild(VeicleToAdd);
                                }
                                break;
                            case "RemoveVehicle":
                                {
                                    var userProfileVehicleNode = doc.DocumentElement.SelectSingleNode("//Vehicles");
                                    var VeicleToRemove = userProfileVehicleNode.SelectSingleNode(data.GetParameterValue("VehicleName"));
                                    userProfileVehicleNode.RemoveChild(userProfileVehicleNode);
                                }
                                break;
                            case "AddInventory":
                                {
                                    var userProfileInvNode = doc.SelectSingleNode("//Inventory");
                                    XmlElement BuildingToAdd = doc.CreateElement(data.GetParameterValue("BuildingID"));
                                    BuildingToAdd.InnerText = data.GetParameterValue("BuildingID");
                                    userProfileInvNode.AppendChild(BuildingToAdd);
                                }
                                break;
                            case "AddActivity":
                                {
                                    var userProfileFlagNode = doc.SelectSingleNode("//Activities");

                                    XmlElement BuildingToAddEntry = doc.CreateElement(data.GetParameterValue("ActivityName"));
                                    BuildingToAddEntry.InnerText = data.GetParameterValue("ActivityName");

                                    userProfileFlagNode.AppendChild(BuildingToAddEntry);
#if DEBUG
                                    LoggerAccessor.LogInfo($"Activity to add: {ActivityName}");
#endif
                                }
                                break;
                            case "RemoveActivity":
                                {
                                    var userProfileActivitiesNode = doc.DocumentElement.SelectSingleNode("//Activities");
                                    if (userProfileActivitiesNode != null)
                                    {
                                        var buildingNode = userProfileActivitiesNode.SelectSingleNode(data.GetParameterValue("ActivityName"));
                                        if (buildingNode != null)
                                        {
                                            doc.DocumentElement.SelectSingleNode("//Activities").RemoveChild(buildingNode);

                                            // Save the updated XML back to the file
                                            File.WriteAllText(profilePath, doc.InnerXml);
#if DEBUG
                                            LoggerAccessor.LogInfo($"Activity removed: {data.GetParameterValue("ActivityName")}");
#endif
                                        }
                                        else
                                        {
                                            LoggerAccessor.LogInfo($"Activity not found: {data.GetParameterValue("ActivityName")}");
                                        }
                                    }
                                    else
                                    {
                                        LoggerAccessor.LogInfo("Activities node not found in the XML.");
                                    }
                                }
                                break;
                            case "AddMission":
                                {
                                    // Update the profile values from the provided data
                                    var userProfileMissionsNode = doc.SelectSingleNode("//Missions");
                                    XmlElement MissionToAddEntry = doc.CreateElement(data.GetParameterValue("MissionName"));
                                    MissionToAddEntry.InnerText = data.GetParameterValue("MissionName");
                                    userProfileMissionsNode.AppendChild(MissionToAddEntry);
                                }
                                break;
                            case "CompleteMission":
                                {
                                    try
                                    {
#if DEBUG
                    LoggerAccessor.LogInfo($"missionNode to Remove: {MissionName}");
#endif
                                        var MissionsNode = doc.DocumentElement.SelectSingleNode("//Missions");
                                        var userProfileMissionsNode = MissionsNode.SelectSingleNode(data.GetParameterValue("MissionName"));
                                        MissionsNode.RemoveChild(userProfileMissionsNode);

                                    }
                                    catch (Exception ex)
                                    {
                                        LoggerAccessor.LogError($"Exception caught: {ex}");
                                    }
                                }
                                break;
                            case "AddMissionToJournal":
                                {
                                    // Update the profile values from the provided data
                                    var userProfileMissionsNode = doc.SelectSingleNode("//Journal");
                                    XmlElement MissionToAddEntry = doc.CreateElement(data.GetParameterValue("MissionName"));
                                    MissionToAddEntry.InnerText = data.GetParameterValue("MissionName");
                                    userProfileMissionsNode.AppendChild(MissionToAddEntry);
                                }
                                break;
                            case "RemoveMissionFromJournal":
                                {
                                    var userProfileActivitiesNode = doc.DocumentElement.SelectSingleNode("//Journal");
                                    if (userProfileActivitiesNode != null)
                                    {
                                        var buildingNode = userProfileActivitiesNode.SelectSingleNode(data.GetParameterValue("MissionName"));
                                        if (buildingNode != null)
                                        {
                                            doc.DocumentElement.SelectSingleNode("//Journal").RemoveChild(buildingNode);

                                            // Save the updated XML back to the file
                                            File.WriteAllText(profilePath, doc.InnerXml);
#if DEBUG
                                            LoggerAccessor.LogInfo($"Mission from Journal removed: {data.GetParameterValue("ActivityName")}");
#endif
                                        }
                                        else
                                        {
                                            LoggerAccessor.LogInfo($"Mission not found: {data.GetParameterValue("MissionName")}");
                                        }
                                    }
                                    else
                                    {
                                        LoggerAccessor.LogInfo("Journal node not found in the XML.");
                                    }
                                }
                                break;
                            case "AddFlag":
                                {
                                    var userProfileFlagNode = doc.SelectSingleNode("//Flags");

                                    XmlElement FlagEntry = doc.CreateElement(data.GetParameterValue("Flag"));
                                    FlagEntry.InnerText = data.GetParameterValue("Flag");

                                    userProfileFlagNode.AppendChild(FlagEntry);
                                }
                                break;
                            case "SpendCoins":
                                {
                                    string coinType = data.GetParameterValue("CoinType");
                                    int NumCoins = Convert.ToInt32(data.GetParameterValue("NumCoins"));
                                    string TransType = data.GetParameterValue("TransType");
                                    string TransParam = data.GetParameterValue("TransParam");

                                    switch (TransType)
                                    {
                                        case "CollectAllRevenue":
                                            {
                                                int profileGoldCoins = Convert.ToInt32(doc.SelectSingleNode("//GoldCoins").InnerText);
                                                doc.SelectSingleNode("//GoldCoins").InnerText = Convert.ToString(profileGoldCoins - NumCoins);
                                                doc.SelectSingleNode("//SilverCoins").InnerText = data.GetParameterValue("SilverCoins") ?? "0";

                                                return $@"<Response>
<ResponseCode>Success</ResponseCode>
<TotalSilver>{doc.SelectSingleNode("//SilverCoins").InnerText}</TotalSilver>
<TotalGold>{doc.SelectSingleNode("//GoldCoins").InnerText}</TotalGold>
<SilverSpent>0</SilverSpent>
<GoldSpent>{NumCoins}</GoldSpent>
</Response>";
                                            }
                                        case "BuyBuilding":
                                            {
                                                return $@"<Response>
<ResponseCode>Success</ResponseCode>
<TotalSilver>{doc.SelectSingleNode("//SilverCoins").InnerText}</TotalSilver>
<TotalGold>{doc.SelectSingleNode("//GoldCoins").InnerText}</TotalGold>
<SilverSpent>0</SilverSpent>
<GoldSpent>{NumCoins}</GoldSpent>
</Response>";
                                            }
                                        case "BuyWorkers":
                                            {
                                                return $@"<Response>
<ResponseCode>Success</ResponseCode>
<TotalSilver>{doc.SelectSingleNode("//SilverCoins").InnerText}</TotalSilver>
<TotalGold>{doc.SelectSingleNode("//GoldCoins").InnerText}</TotalGold>
<SilverSpent>0</SilverSpent>
<GoldSpent>{NumCoins}</GoldSpent>
</Response>";
                                            }
                                        case "BuyVehicles":
                                            {
                                                return $@"<Response>
<ResponseCode>Success</ResponseCode>
<TotalSilver>{doc.SelectSingleNode("//SilverCoins").InnerText}</TotalSilver>
<TotalGold>{doc.SelectSingleNode("//GoldCoins").InnerText}</TotalGold>
<SilverSpent>0</SilverSpent>
<GoldSpent>{NumCoins}</GoldSpent>
</Response>";
                                            }
                                        case "BuyExpansion":
                                            {
                                                return $@"<Response>
<ResponseCode>Success</ResponseCode>
<TotalSilver>{doc.SelectSingleNode("//SilverCoins").InnerText}</TotalSilver>
<TotalGold>{doc.SelectSingleNode("//GoldCoins").InnerText}</TotalGold>
<SilverSpent>0</SilverSpent>
<GoldSpent>{NumCoins}</GoldSpent>
</Response>";
                                            }
                                        case "BuyDollars":
                                            {
                                                return $@"<Response>
<ResponseCode>Success</ResponseCode>
<TotalSilver>{doc.SelectSingleNode("//SilverCoins").InnerText}</TotalSilver>
<TotalGold>{doc.SelectSingleNode("//GoldCoins").InnerText}</TotalGold>
<SilverSpent>0</SilverSpent>
<GoldSpent>{NumCoins}</GoldSpent>
</Response>";
                                            }
                                        case "BuySuburb":
                                            {
                                                return $@"<Response>
<ResponseCode>Success</ResponseCode>
<TotalSilver>{doc.SelectSingleNode("//SilverCoins").InnerText}</TotalSilver>
<TotalGold>{doc.SelectSingleNode("//GoldCoins").InnerText}</TotalGold>
<SilverSpent>0</SilverSpent>
<GoldSpent>{NumCoins}</GoldSpent>
</Response>";
                                            }
                                        case "BuyTimeOfDay":
                                            {
                                                return $@"<Response>
<ResponseCode>Success</ResponseCode>
<TotalSilver>{doc.SelectSingleNode("//SilverCoins").InnerText}</TotalSilver>
<TotalGold>{doc.SelectSingleNode("//GoldCoins").InnerText}</TotalGold>
<SilverSpent>0</SilverSpent>
<GoldSpent>{NumCoins}</GoldSpent>
</Response>";
                                            }
                                    }


                                }
                                break;

                            case "SetPrivacy":
                                {
                                    string ownID = data.GetParameterValue("TownID");
                                    string NewSetting = data.GetParameterValue("NewSetting");

                                    var userProfileOptionsNode = doc.DocumentElement.SelectSingleNode("//Options");

                                    userProfileOptionsNode.SelectSingleNode("PrivacySetting").InnerText = NewSetting;

                                    //Write a server txt for player names we can ban?
                                    return "<Response><Banned>0</Banned></Response>";
                                }
                            
                            case "UpdateUser":
                                {
                                    doc.SelectSingleNode("//TotalCollected").InnerText = data.GetParameterValue("TotalCollected");
                                    doc.SelectSingleNode("//Wallet").InnerText = data.GetParameterValue("Wallet");
                                    doc.SelectSingleNode("//Workers").InnerText = data.GetParameterValue("Workers");
                                    doc.SelectSingleNode("//GoldCoins").InnerText = data.GetParameterValue("GoldCoins");
                                    doc.SelectSingleNode("//SilverCoins").InnerText = data.GetParameterValue("SilverCoins") ?? "0";
                                    doc.SelectSingleNode("//NewPlayer").InnerText = data.GetParameterValue("NewPlayer") ?? "0";
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
                                }
                                break;
                        }

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

            string profilePath = $"{WorkPath}/ClearasilSkater/User_Data/{UserID}.xml";

            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);
            else
                xmlProfile = DefaultClearasilSkaterAndSlimJimProfile;

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
                        
                        updatedXMLProfile = doc.DocumentElement.InnerXml.Replace("<root>", string.Empty).Replace("</root>", string.Empty);
#pragma warning restore 8602
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

        public static string UpdateUserSlimJim(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string xmlProfile = string.Empty;
            string updatedXMLProfile = string.Empty;

            // Retrieve the user's JSON profile
            string profilePath = $"{WorkPath}/SlimJim/User_Data/{UserID}.xml";

            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);
            else
                xmlProfile = DefaultClearasilSkaterAndSlimJimProfile;

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

        public static string RequestInitialDataNovusPrime(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string profilePath = $"{WorkPath}/NovusPrime/User_Data/{UserID}.xml";

            string xmlProfile;
            if (File.Exists(profilePath))
            {
                LoggerAccessor.LogInfo($"[HFGAMES] - Detected existing player data, sending!");
                xmlProfile = File.ReadAllText(profilePath);
            }
            else
            {
                LoggerAccessor.LogInfo($"[HFGAMES] - New player with no player data! Using default!");
                xmlProfile = DefaultNovusPrimeProfile;
            }

            return $"<Response>{xmlProfile}</Response>";
        }

        public static string NovusCompleteMission(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string profilePath = $"{WorkPath}/NovusPrime/User_Data/{UserID}.xml";
            string xmlProfile = string.Empty;
            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);



            var doc = new XmlDocument();

            doc.LoadXml($"<xml>{xmlProfile}</xml>");

            var userProfileMissionsNode = doc.DocumentElement.SelectSingleNode("//Missions");

            if (userProfileMissionsNode != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);

                    // Retrieve the new MissionId from the parsed data
                    string newMissionId = data.GetParameterValue("MissionId");

                    // Check if the MissionId already exists
                    var MissionNodesList = userProfileMissionsNode.SelectNodes("Mission");
                    bool missionExists = false;

                    foreach (XmlNode MissionNode in MissionNodesList)
                    {
                        if (MissionNode.SelectSingleNode("MissionId").InnerText == newMissionId)
                        {
                            missionExists = true;
                            LoggerAccessor.LogInfo($"Mission {newMissionId} already exists.");
                            break;
                        }
                    }

                    // If the mission doesn't exist, add a new entry
                    if (!missionExists)
                    {
                        XmlElement newMissionNode = doc.CreateElement("Mission");

                        XmlElement missionIdNode = doc.CreateElement("MissionId");
                        missionIdNode.InnerText = newMissionId;

                        newMissionNode.AppendChild(missionIdNode);

                        userProfileMissionsNode.AppendChild(newMissionNode);

                        LoggerAccessor.LogInfo($"Mission {newMissionId} added.");

                        // Save the updated XML to file
                        File.WriteAllText(profilePath, doc.DocumentElement.InnerXml);
                    }
                }
            }
            else
            {
                LoggerAccessor.LogInfo("Missions node not found in the XML.");
            }
            return "<Response></Response>";
        }

        public static string UpdateCharacter(byte[] PostData, string boundary, string UserID, string WorkPath, string cmd)
        {
            //userId
            //Experience
            //Level
            //Nebulon
            //TotalNebulonEver

            string xmlProfile = string.Empty;
            string updatedXMLProfile = string.Empty;

            string profilePath = $"{WorkPath}/NovusPrime/User_Data/{UserID}.xml";
            string lbPath = $"{WorkPath}/NovusPrime/User_Data/Leaderboards.xml";

            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);
            else
                xmlProfile = DefaultNovusPrimeProfile;

            try
            {
                var doc = new XmlDocument();
                var lbDoc = new XmlDocument();


                doc.LoadXml("<root>" + xmlProfile + "</root>"); // Wrap the XML string in a root element
                if (doc != null && PostData != null && !string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        // Update the profile values from the provided data
                        // Update the values based on IDs
#pragma warning disable 8602

                        switch (cmd)
                        {
                            case "UpdateCharacter":
                                {

                                    doc.SelectSingleNode("//Experience").InnerText = data.GetParameterValue("Experience");
                                    doc.SelectSingleNode("//Level").InnerText = data.GetParameterValue("Level");
                                    doc.SelectSingleNode("//Nebulon").InnerText = data.GetParameterValue("Nebulon");
                                    doc.SelectSingleNode("//TotalNebulonEver").InnerText = data.GetParameterValue("TotalNebulonEver");

                                    #region Leaderboard entry update

                                    if (File.Exists(lbPath))
                                    {
                                        lbDoc.LoadXml("<root>" + File.ReadAllText(lbPath) + "</root>");

                                        if (lbDoc.SelectSingleNode($"//{UserID}") != null)
                                        {
                                            XmlNode userNameExistEntry = lbDoc.SelectSingleNode($"//{UserID}");

                                            XmlNode scoreNode = userNameExistEntry.SelectSingleNode("//Score");
                                            scoreNode.InnerText = data.GetParameterValue("TotalNebulonEver").Split(".")[0];

                                        }
                                        else
                                        {
                                            XmlElement userNameElement = lbDoc.CreateElement(UserID);
                                            XmlElement displayNameElement = lbDoc.CreateElement("DisplayName");
                                            XmlElement scoreElement = lbDoc.CreateElement("Score");

                                            scoreElement.InnerText = data.GetParameterValue("TotalNebulonEver");
                                            displayNameElement.InnerText = UserID;

                                            userNameElement.AppendChild(displayNameElement);
                                            userNameElement.AppendChild(scoreElement);

                                            lbDoc.DocumentElement.AppendChild(userNameElement);
                                        }

                                    }
                                    else
                                    {

                                        XmlElement userNameElement = lbDoc.CreateElement(UserID);
                                        XmlElement displayNameElement = lbDoc.CreateElement("DisplayName");
                                        XmlElement scoreElement = lbDoc.CreateElement("Score");

                                        scoreElement.InnerText = data.GetParameterValue("TotalNebulonEver");
                                        displayNameElement.InnerText = UserID;

                                        userNameElement.AppendChild(displayNameElement);
                                        userNameElement.AppendChild(scoreElement);

                                        lbDoc.AppendChild(userNameElement);
                                    }

                                    File.WriteAllText(lbPath, lbDoc.DocumentElement.InnerXml);
                                    #endregion
                                }
                                break;

                            case "AddInventory":
                                {
                                    string baseName = "Item";
                                    int index = 1;
                                    string newObjectId = $"{baseName}{index}";

                                    var inventoryNode = doc.SelectSingleNode("//Inventory");

                                    var inventoryItems = inventoryNode.SelectNodes("*");
                                    var existingIds = new HashSet<string>();

                                    foreach (XmlNode item in inventoryItems)
                                    {
                                        existingIds.Add(item.Name);
                                    }

                                    while (existingIds.Contains(newObjectId))
                                    {
                                        index++;
                                        newObjectId = $"{baseName}{index}";
                                    }

                                    XmlElement objectNodeEntry = doc.CreateElement(newObjectId);
                                    XmlElement ObjectIdEntry = doc.CreateElement("ObjectId");
                                    XmlElement QuantityEntry = doc.CreateElement("Quantity");

                                    ObjectIdEntry.InnerText = data.GetParameterValue("ObjectId");
                                    QuantityEntry.InnerText = "1";

                                    objectNodeEntry.AppendChild(ObjectIdEntry);
                                    objectNodeEntry.AppendChild(QuantityEntry);

                                    inventoryNode.AppendChild(objectNodeEntry);

                                    doc.SelectSingleNode("//Inventory").AppendChild(objectNodeEntry);
                                }
                                break;

                            case "UseDaily":
                                {
                                    XmlNode DailyAvailable = doc.SelectSingleNode("//DailyAvailable");
                                    if (DailyAvailable.SelectSingleNode("TimeElapsed") != null)
                                    {
                                        XmlNode timeElapsedExisting = DailyAvailable.SelectSingleNode("TimeElapsed");
                                        timeElapsedExisting.InnerText = DateTime.Now.ToString("HHmmss");
                                        DailyAvailable.AppendChild(timeElapsedExisting);
                                    } else
                                    {
                                        XmlElement timeElapsed = doc.CreateElement("TimeElapsed");
                                        timeElapsed.InnerText = DateTime.Now.ToString("HHmmss");
                                        DailyAvailable.AppendChild(timeElapsed);
                                    }

                                }
                                break;

                            case "ConfigureShip":
                                {
                                    XmlNode shipConfig = doc.SelectSingleNode("//ShipConfig");

                                    //data.GetParameterValue("Slot") 1
                                    shipConfig.SelectSingleNode("Chassis").InnerText = data.GetParameterValue("Chassis");
                                    shipConfig.SelectSingleNode("Front1").InnerText = data.GetParameterValue("Front1");
                                    shipConfig.SelectSingleNode("Front2").InnerText = data.GetParameterValue("Front2");
                                    shipConfig.SelectSingleNode("Turret1").InnerText = data.GetParameterValue("Turret1");
                                    shipConfig.SelectSingleNode("Turret2").InnerText = data.GetParameterValue("Turret2");
                                    shipConfig.SelectSingleNode("Special").InnerText = data.GetParameterValue("Special");
                                    shipConfig.SelectSingleNode("Maneuver").InnerText = data.GetParameterValue("Maneuver");
                                    shipConfig.SelectSingleNode("Upgrade1").InnerText = data.GetParameterValue("Upgrade1");
                                    shipConfig.SelectSingleNode("Upgrade2").InnerText = data.GetParameterValue("Upgrade2");
                                    shipConfig.SelectSingleNode("Upgrade3").InnerText = data.GetParameterValue("Upgrade3");
                                    shipConfig.SelectSingleNode("Upgrade4").InnerText = data.GetParameterValue("Upgrade4");
                                    shipConfig.SelectSingleNode("PaintJob").InnerText = data.GetParameterValue("PaintJob");
                                }
                                break;

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

            return $"<Response></Response>";
        }

        public static string RequestShipSlots(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string xmlProfile = string.Empty;
            string updatedXMLProfile = string.Empty;

            string profilePath = $"{WorkPath}/ClearasilSkater/User_Data/{UserID}.xml";

            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);
            else
                xmlProfile = DefaultClearasilSkaterAndSlimJimProfile;

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

                        updatedXMLProfile = doc.DocumentElement.InnerXml.Replace("<root>", string.Empty).Replace("</root>", string.Empty);
#pragma warning restore 8602
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
                xmlProfile = DefaultClearasilSkaterAndSlimJimProfile;
            }

            return $"<Response>{xmlProfile}</Response>";
        }

        public static string RequestUserSlimJim(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string profilePath = $"{WorkPath}/SlimJim/User_Data/{UserID}.xml";

            string xmlProfile;
            if (File.Exists(profilePath))
            {
                LoggerAccessor.LogInfo($"[HFGAMES] - Detected existing player data, sending!");
                xmlProfile = File.ReadAllText(profilePath);
            }
            else
            {
                LoggerAccessor.LogInfo($"[HFGAMES] - New player with no player data! Using default!");
                xmlProfile = DefaultClearasilSkaterAndSlimJimProfile;
            }

            return $"<Response>{xmlProfile}</Response>";
        }

        public static string RequestUserPoker(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string profilePath = $"{WorkPath}/Poker/User_Data/{UserID}.xml";
            string DisplayName = string.Empty;
            string HomeRegion = string.Empty;

            if (PostData != null && !string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    DisplayName = data.GetParameterValue("DisplayName");
                    HomeRegion = data.GetParameterValue("Region");
                    ms.Flush();
                }
            }
            

                string xmlProfile;
            if (File.Exists(profilePath))
            {
                LoggerAccessor.LogInfo($"[HFGAMES] - Detected existing poker player data, sending!");
                xmlProfile = File.ReadAllText(profilePath);
            }
            else
            {
                LoggerAccessor.LogInfo($"[HFGAMES] - New player with no poker player data! Using default!");
                xmlProfile = DefaultPokerProfile;
            }

            return $"<Response>{xmlProfile}</Response>";
        }

        public static string UpdateUserPoker(byte[] PostData, string boundary, string UserID, string WorkPath)
        {
            string Bankroll = string.Empty;


            if (PostData != null && !string.IsNullOrEmpty(boundary))
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    Bankroll = data.GetParameterValue("Bankroll");
                    ms.Flush();
                }


            }

                string xmlProfile = string.Empty;
            string updatedXMLProfile = string.Empty;

            // Retrieve the user's JSON profile
            string profilePath = $"{WorkPath}/Poker/User_Data/{UserID}.xml";

            if (File.Exists(profilePath))
                xmlProfile = File.ReadAllText(profilePath);
            else
                xmlProfile = DefaultPokerProfile;

            try
            {
                // Create an XmlDocument
                var doc = new XmlDocument();
                doc.LoadXml("<xml>" + xmlProfile + "</xml>"); // Wrap the XML string in a root element
                if (doc != null && PostData != null && !string.IsNullOrEmpty(boundary))
                {
                    using (MemoryStream ms = new MemoryStream(PostData))
                    {
                        var data = MultipartFormDataParser.Parse(ms, boundary);
                        // Update the profile values from the provided data
                        // Update the values based on IDs
#pragma warning disable 8602
                        doc.SelectSingleNode("//Bankroll").InnerText = Bankroll;

                        // Get the updated XML string
                        updatedXMLProfile = doc.DocumentElement.InnerXml.Replace("<xml>", string.Empty).Replace("</xml>", string.Empty);
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
    }
}
