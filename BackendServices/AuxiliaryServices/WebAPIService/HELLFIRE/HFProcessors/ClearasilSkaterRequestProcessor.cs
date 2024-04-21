using CyberBackendLibrary.HTTP;
using CustomLogger;
using HttpMultipartParser;
using WebAPIService.HELLFIRE.Helpers;

namespace WebAPIService.HELLFIRE.HFProcessors
{
    public class ClearasilSkaterRequestProcessor
    {
        public static string? ProcessMainPHP(byte[]? PostData, string? ContentType, string? PHPSessionID, string WorkPath)
        {
            if (PostData == null || string.IsNullOrEmpty(ContentType))
                return null;

            string Command = string.Empty;
            string UserID = string.Empty;
            string DisplayName = string.Empty;
            string InstanceID = string.Empty;
            string Region = string.Empty;
            string? boundary = HTTPProcessor.ExtractBoundary(ContentType);

            if (boundary != null)
            {
                using (MemoryStream ms = new(PostData))
                {
                    var data = MultipartFormDataParser.Parse(ms, boundary);
                    Command = data.GetParameterValue("Command");
                    UserID = data.GetParameterValue("UserID");

                    LoggerAccessor.LogInfo($"[HFGAMES] Command detected as {Command}");

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
                    Directory.CreateDirectory($"{WorkPath}/ClearasilSkater/User_Data");

                    switch (Command)
                    {
                        case "RequestNPTicket":
                            return NPTicket.RequestNPTicket(PostData, boundary);
                        case "RequestUser":
                            return User.RequestUserClearasilSkater(PostData, boundary, UserID, WorkPath);
                        case "UpdateUser":
                            return User.UpdateUserClearasilSkater(PostData, boundary, UserID, WorkPath);
                        case "TotalScoreLeaderboard":
                            return Leaderboards.GetLeaderboards(PostData, boundary, UserID, WorkPath);
                            /*
                            return "<Response> <table> <DisplayName>JumpSuit</DisplayName> <LeaderboardScore>1</LeaderboardScore> " +
                                "<DisplayName>Devil303</DisplayName> <LeaderboardScore>2</LeaderboardScore>" + 
                                "<DisplayName>AgentDark447</DisplayName> <LeaderboardScore>3</LeaderboardScore>" +
                                "<DisplayName>SpliceWave</DisplayName> <LeaderboardScore>4</LeaderboardScore> </table>" +
                                "</Response>";
                            */
                        case "LogMetric":
                            return "<Response></Response>";


                        case "QueryMotd":
                            return "<Response><Motd>Message of the Day!</Motd></Response>";
                        case "QueryServerGlobals":
                            return "<Response><GlobalHard>1</GlobalHard><GlobalWrinkles>1</GlobalWrinkles></Response>";
                        case "QueryHoldbacks":
                            return "<Response></Response>";
                        case "QueryRewards":
                            if (File.Exists($"{WorkPath}/ClearasilSkater/User_Data/{UserID}_Rewards.xml"))
                                return $"<Response>{File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_Rewards.xml")}</Response>";
                            else
                                return "<Response></Response>";
                        case "QueryGifts":
                            if (File.Exists($"{WorkPath}/ClearasilSkater/User_Data/{UserID}_Gifts.xml"))
                                return $"<Response>{File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_Gifts.xml")}</Response>";
                            else
                                return "<Response><Gift>111111</Gift></Response>";
                        default:
                            LoggerAccessor.LogWarn($"[HFGAMES] - Client Request a Command I don't know about, please post the message on GITHUB : {Command}");
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
