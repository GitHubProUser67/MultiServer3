using CustomLogger;
using HttpMultipartParser;
using NetworkLibrary.HTTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPIService.HELLFIRE.Helpers;

namespace WebAPIService.HELLFIRE.HFProcessors
{
    public class NovusPrimeRequestProcessor
    {
        public static string ProcessMainPHP(byte[] PostData, string ContentType, string PHPSessionID, string WorkPath)
        {
            if (PostData == null || string.IsNullOrEmpty(ContentType))
                return null;

            string Command = string.Empty;
            string UserID = string.Empty;
            string DisplayName = string.Empty;
            string InstanceID = string.Empty;
            string Region = string.Empty;

            string Type = string.Empty;
            string Str = string.Empty;
            string Amount = string.Empty;

            string boundary = HTTPProcessor.ExtractBoundary(ContentType);

            if (boundary != null)
            {
                using (MemoryStream ms = new MemoryStream(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    Command = data.GetParameterValue("Command");
                    UserID = data.GetParameterValue("UserId");

                    LoggerAccessor.LogInfo($"[HFGAMES] Command detected as {Command}");

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


                    try
                    {
                        //for Log cmd
                        Type = data.GetParameterValue("Type");
                        Str = data.GetParameterValue("Str");
                        Amount = data.GetParameterValue("Amount");
                    }
                    catch
                    {
                        // Not Important.
                    }

                    ms.Flush();
                }

                if (!string.IsNullOrEmpty(Command))
                {
                    Directory.CreateDirectory($"{WorkPath}/SlimJim/User_Data");

                    switch (Command)
                    {
                        case "RequestNPTicket":
                            return NPTicket.RequestNPTicket(PostData, boundary);
                        case "RequestInitialData":
                            return User.RequestInitialDataNovusPrime(PostData, boundary, UserID, WorkPath);
                        case "UpdateUser":
                            return User.UpdateUserSlimJim(PostData, boundary, UserID, WorkPath);
                        case "RequestScoreLeaderboard":
                            return Leaderboards.GetLeaderboardsNovusPrime(PostData, boundary, UserID, WorkPath);
                        case "CompleteMission":
                            return User.NovusCompleteMission(PostData, boundary, UserID, WorkPath);
                        case "RequestCharacter":
                            return User.UpdateCharacter(PostData, boundary, UserID, WorkPath, Command);
                        case "UpdateCharacter":
                            return User.UpdateCharacter(PostData, boundary, UserID, WorkPath, Command);
                        case "ReadShipConfigData":
                            return "<Response></Response>";
                        case "ConfigureShip":
                            return User.UpdateCharacter(PostData, boundary, UserID, WorkPath, Command);
                        case "RequestStore":
                            return "<Response></Response>";
                        case "RequestInventory":
                            return User.UpdateCharacter(PostData, boundary, UserID, WorkPath, Command);
                        case "AddInventory":
                            return User.UpdateCharacter(PostData, boundary, UserID, WorkPath, Command);
                        case "UseDaily":
                            return User.UpdateCharacter(PostData, boundary, UserID, WorkPath, Command);
                        case "QueryDaily":
                            if (File.Exists($"{WorkPath}/NovusPrime/User_Data/{UserID}_Rewards.xml"))
                                return $"<Response>{File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_Rewards.xml")}</Response>";
                            else
                                return "<Response></Response>";
                        case "Log":
#if DEBUG
                            LoggerAccessor.LogInfo($"Novus Log: {Type} {Str} {Amount}");
#endif
                            return "<Response></Response>";
                        default:
                            LoggerAccessor.LogWarn($"[HFGAMES] - Client Request a Command I don't know about, please post the message on GITHUB : {Command}");
                            return "<Response></Response>";
                    }
                }
            }

            return null;
        }
    }
}
