using CyberBackendLibrary.HTTP;
using CustomLogger;
using HttpMultipartParser;
using WebAPIService.HELLFIRE.Helpers;
using System.IO;
using System.Threading;

namespace TycoonServer.HFProcessors
{
    public class TycoonRequestProcessor
    {
        public static string ProcessMainPHP(byte[] PostData, string ContentType, string PHPSessionID, string WorkPath, bool https)
        {
            if (PostData == null || string.IsNullOrEmpty(ContentType))
                return null;

            string Command = string.Empty;
            string UserID = string.Empty;
            string DisplayName = string.Empty;
            string InstanceID = string.Empty;
            string Region = string.Empty;
            string boundary = HTTPProcessor.ExtractBoundary(ContentType);

            if (boundary != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    Command = data.GetParameterValue("Command");
                    try
                    {
                        UserID = data.GetParameterValue("UserID");
                    }
                    catch
                    {
                        // Not Important.
                    }
                    try
                    {
                        DisplayName = data.GetParameterValue("DisplayName");
                    }
                    catch
                    {
                        // Not Important.
                    }
                    try
                    {
                        InstanceID = data.GetParameterValue("InstanceID");
                    }
                    catch
                    {
                        // Not Important.
                    }
                    try
                    {
                        Region = data.GetParameterValue("Region");
                    }
                    catch
                    {
                        // Not Important.
                    }
                    ms.Flush();
                }

                if (!string.IsNullOrEmpty(Command))
                {
                    string ServerFilesPath = $"{WorkPath}/TYCOON/Server_Data";
                    string userDataPath = $"{WorkPath}/TYCOON/User_Data";
                    Directory.CreateDirectory(userDataPath);

                    LoggerAccessor.LogInfo($"Command requested: {Command}");

                    switch (Command)
                    {
                        case "VersionCheck":
                            return $"<Response><URL>{(https ? "https" : "http")}://game2.hellfiregames.com/HomeTycoon</URL></Response>";
                        case "QueryMotd":
                            LoggerAccessor.LogInfo($"Checking if {ServerFilesPath + "/MOTD.xml"} exists!");
                            if (File.Exists(ServerFilesPath + "/MOTD.xml"))
                            {
                                return $"<Response>{File.ReadAllText(ServerFilesPath + "/MOTD.xml")}</Response>";
                            }
                            else
                            {
                                return "<Response><Motd><en>Message of the Day!</en></Motd></Response>";
                            };
                        case "QueryServerGlobals":
                            //Enables Debug
                            return "<Response><GlobalHard>0</GlobalHard><GlobalWrinkles>0</GlobalWrinkles></Response>";
                        case "QueryPrices":
                            return "<Response><Buildings><GlobosynTower>10</GlobosynTower></Buildings><Services>" +
                                "<GlobosynService>10</GlobosynService></Services>" +
                                "<Vehicles><GlobosynVehicle>10</GlobosynVehicle>" +
                                "</Vehicles><Expansions><GlobosynExpansion>10</GlobosynExpansion>" +
                                "</Expansions><WorkerPackages><GlobosynWorker>10</GlobosynWorker></WorkerPackages>" +
                                "</Response>";
                        case "QueryBoosters":
                            return "<Response><Booster><Type>1</Type><Value>1</Value><Param>1</Param>" +
                                "<UUID>7A8BC3DB-399F4457-8117F099-D9F5D132</UUID><Reward>true</Reward>" +
                                "</Booster></Response>";
                        case "QueryHoldbacks":
                            //Holdback some items
                            return "<Response></Response>";
                        case "QueryRewards":
                            if (File.Exists(ServerFilesPath + $"/Server_Rewards.xml"))
                                return $"<Response>{File.ReadAllText(userDataPath + $"/Server__Rewards.xml")}</Response>";
                            else
                                return @"<Response>
                                <Reward>
                                    <Name>HT_T_Shirt</Name>
                                    <SCEA type=""table"">
                                       <UUID>3EC5EAB2-5C5D4379-AE88BD53-A2B26938</UUID>
                                    </SCEA>
                                </Reward>

                                <Reward>
                                    <Name>Construction_Hat</Name>
                                    <SCEA type=""table"">
                                       <UUID>E13282E0-662243D5-9FFE2923-DE784629</UUID>
                                    </SCEA>
                                </Reward>

                                <Reward>
                                    <Name>TU_Shirt</Name>
                                    <SCEA type=""table"">
                                       <UUID>5A8D5748-86974730-AFAEDD07-1762E91D</UUID>
                                    </SCEA>
                                </Reward>
                                </Response>";
                        case "QueryGifts":
                            if (File.Exists(userDataPath + $"/{UserID}_Gifts.xml"))
                                return $"<Response>{File.ReadAllText(userDataPath + $"/{UserID}_Gifts.xml")}</Response>";
                            else
                                return "<Response><Gift>5</Gift></Response>";
                        case "RequestNPTicket":
                            return NPTicket.RequestNPTicket(PostData, boundary);
                        case "RequestTownInstance":
                            return TownInstance.RequestTownInstance(UserID, DisplayName, PHPSessionID);
                        case "RequestTown":
                            Thread.Sleep(3000); // Why is that in here? Because the game is so bugged that responding too fast makes it crash.
                            return TownInstance.RequestTown(UserID, InstanceID, DisplayName, WorkPath);
                        case "RequestUser":
                            if (File.Exists(userDataPath + $"/{UserID}.xml"))
                                return $"<Response>{File.ReadAllText(userDataPath + $"/{UserID}.xml")}</Response>";
                            else
                                return $"<Response>{User.DefaultHomeTycoonProfile}</Response>";
                        case "RequestVisitingUser":
                            if (File.Exists(userDataPath + $"/{UserID}.xml"))
                                return $"<Response>{File.ReadAllText(userDataPath + $"/{UserID}.xml")}</Response>";
                            else
                                return $"<Response>{User.DefaultHomeTycoonProfile}</Response>";
                        case "RequestUserTowns":
                            return "<Response><MyTown></MyTown></Response>";
                        case "UpdateTownTime":
                            return "<Response></Response>";
                        case "UpdateTownPlayers":
                            return "<Response></Response>";
                        case "UpdateInstance":
                            return "<Response></Response>";
                        case "UpdateUser":
                            return User.UpdateUserHomeTycoon(PostData, boundary, UserID, WorkPath);
                        case "HasUser":
                            return User.GetUserHomeTycoon(PostData, boundary, UserID, WorkPath);
                        case "SetPrivacy":
                            //Set Town Privacy for User
                            return User.SetPrivacy(PostData, boundary, UserID, WorkPath);
                        case "AddUnlocked":
                            //Add UnlockedBuildings to User
                            return User.AddUnlocked(PostData, boundary, UserID, WorkPath);
                        case "AddMissionToJournal":
                            //Add MissionToJournal for User
                            //return User.AddUnlocked(PostData, boundary, UserID, WorkPath);
                            return "<Response></Response>";
                        case "AddDialog":
                            //Random NPC Encounters
                            //Add a Mission NPC Dialog and add entry for User
                            //return User.AddUnlocked(PostData, boundary, UserID, WorkPath);
                            return "<Response></Response>";
                        case "CompleteDialog":
                            //Complete a Mission NPC Dialog and remove for User
                            //return User.AddUnlocked(PostData, boundary, UserID, WorkPath);
                            return "<Response></Response>";
                        case "ClearMissionRevenueCollected":
                            //Complete a Mission NPC Dialog and remove for User
                            //return User.AddUnlocked(PostData, boundary, UserID, WorkPath);
                            return "<Response></Response>";
                        case "CreateBuilding":
                            return TownProcessor.CreateBuilding(PostData, boundary, UserID, WorkPath);
                        case "UpdateBuildings":
                            return TownProcessor.UpdateBuildings(PostData, boundary, UserID, WorkPath);
                        case "RemoveBuilding":
                            return TownProcessor.RemoveBuilding(PostData, boundary, UserID, WorkPath);
                        case "GlobalPopulationLeaderboard":
                            return Leaderboards.GetGlobalPopulationLeaderboard(PostData, boundary, UserID, WorkPath);
                        case "GlobalRevenueCollectedLeaderboard":
                            return Leaderboards.GetGlobalRevenueCollectedLeaderboard(PostData, boundary, UserID, WorkPath);
                        default:
                            LoggerAccessor.LogWarn($"[HFGAMES] - Client Requested an unknown Home Tycoon Command, please report as issue on GITHUB : {Command}");
                            return "<Response></Response>";
                    }
                }
            }

            return null;
        }

        public static string ProcessPostCards(byte[] PostData, string ContentType) // Not yet handled.
        {
            if (PostData == null || string.IsNullOrEmpty(ContentType))
                return null;

            return null;
        }
    }
}
