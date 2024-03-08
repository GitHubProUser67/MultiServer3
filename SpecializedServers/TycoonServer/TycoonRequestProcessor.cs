using BackendProject.MiscUtils;
using CustomLogger;
using HttpMultipartParser;

namespace TycoonServer
{
    public class TycoonRequestProcessor
    {
        public static string? ProcessMainPHP(byte[]? PostData, string? ContentType, string? PHPSessionID)
        {
            if (PostData == null || string.IsNullOrEmpty(ContentType))
                return null;

            string Command = string.Empty;
            string UserID = string.Empty;
            string DisplayName = string.Empty;
            string InstanceID = string.Empty;
            string Region = string.Empty;
            string? boundary = HTTPUtils.ExtractBoundary(ContentType);

            if (boundary != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    Command = data.GetParameterValue("Command");
                    UserID = data.GetParameterValue("UserID");
                    try
                    {
                        DisplayName = data.GetParameterValue("DisplayName");
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                    try
                    {
                        InstanceID = data.GetParameterValue("InstanceID");
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                    try
                    {
                        Region = data.GetParameterValue("Region");
                    }
                    catch (Exception)
                    {
                        // Not Important.
                    }
                    ms.Flush();
                }

                if (!string.IsNullOrEmpty(Command))
                {
                    Directory.CreateDirectory($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data");
					
                    switch (Command)
                    {
                        case "QueryMotd":
                            return "<Response><Motd>Message of the Day!</Motd></Response>";
                        case "RequestNPTicket":
                            return NPTicket.RequestNPTicket(PostData, boundary);
                        case "RequestTownInstance":
                            return TownInstance.RequestTownInstance(UserID, DisplayName, PHPSessionID);
                        case "QueryServerGlobals":
                            return "<Response><GlobalHard>1</GlobalHard><GlobalWrinkles>1</GlobalWrinkles></Response>";
                        case "QueryPrices":
                            return "<Response><Buildings><GlobosynTower>10</GlobosynTower></Buildings><Services>" +
                                "<GlobosynService>10</GlobosynService></Services><Vehicles><GlobosynVehicle>10" +
                                "</GlobosynVehicle></Vehicles><Expansions><GlobosynExpansion>10</GlobosynExpansion" +
                                "></Expansions><WorkerPackages><GlobosynWorker>10</GlobosynWorker></WorkerPackages>" +
                                "</Response>";
                        case "QueryBoosters":
                            return "<Response><Booster><Type>1</Type><Value>1</Value><Param>1</Param>" +
                                "<UUID>7A8BC3DB-399F4457-8117F099-D9F5D132</UUID><Reward>true</Reward>" +
                                "</Booster></Response>";
                        case "QueryHoldbacks":
                            return "<Response></Response>";
                        case "QueryRewards":
                            if (File.Exists($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}_Rewards.xml"))
                                return $"<Response>{File.ReadAllText($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}_Rewards.xml")}</Response>";
                            else
                                return "<Response></Response>";
                        case "QueryGifts":
                            if (File.Exists($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}_Gifts.xml"))
                                return $"<Response>{File.ReadAllText($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}_Gifts.xml")}</Response>";
                            else
                                return "<Response><Gift>111111</Gift></Response>";
                        case "RequestTown":
                            Thread.Sleep(1000); // Why is that in here? Because the game is so bugged that responding too fast makes it crash.
                            return TownInstance.RequestTown(UserID, InstanceID, DisplayName);
                        case "RequestUser":
                            if (File.Exists($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}.xml"))
                                return $"<Response>{File.ReadAllText($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}.xml")}</Response>";
                            else
                                return $"<Response>{User.DefaultProfile}</Response>";
                        case "RequestVisitingUser":
                            if (File.Exists($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}.xml"))
                                return $"<Response>{File.ReadAllText($"{TycoonServerConfiguration.TycoonStaticFolder}/TYCOON/User_Data/{UserID}.xml")}</Response>";
                            else
                                return $"<Response>{User.DefaultProfile}</Response>";
                        case "RequestUserTowns":
                            return "<Response><MyTown></MyTown></Response>";
                        case "UpdateTownTime":
                            return "<Response></Response>";
                        case "UpdateUser":
                            return User.UpdateUser(PostData, boundary, UserID);
                        case "CreateBuilding":
                            return BuildingsProcessor.CreateBuilding(PostData, boundary, UserID);
                        case "RemoveBuilding":
                            return BuildingsProcessor.RemoveBuilding(PostData, boundary, UserID);
                        default:
                            LoggerAccessor.LogWarn($"[Tycoon] - Client Request a Command I don't know about, please post the message on GITHUB : {Command}");
                            return "<Response></Response>";
                    }
                }
            }

            return null;
        }

        public static string? ProcessPostCards(byte[]? PostData, string? ContentType) // Not yet handled.
        {
            if (PostData == null || string.IsNullOrEmpty(ContentType))
                return null;

            return null;
        }
    }
}
