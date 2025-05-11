using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.GameServices.PS3SparkServices
{
    /// <summary>
	/// Secure connection service protocol
	/// </summary>
	[RMCService((ushort)RMCProtocolId.SparkProtocolService)]
    public class SparkProtocolService : RMCServiceBase
    {

        [RMCMethod(4)]
        public RMCResult CreateGame(string gameName)
        {
            UNIMPLEMENTED();
            return Result(new { retVal = true });
        }

        [RMCMethod(5)]
        public RMCResult JoinGame(string gameName)
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(6)]
        public RMCResult GetFriendStats()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(7)]
        public RMCResult GetSelfStats()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(8)]
        public RMCResult GetParticipationStats()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(9)]
        public RMCResult GetStats()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(10)]
        public RMCResult GetDetailedFriendInfoList()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(11)]
        public RMCResult GetPlayerStatus()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(12)]
        public RMCResult ReportStats()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(13)]
        public RMCResult GetSecretQuestion()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(14)]
        public RMCResult ValidateSecretAnswer()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(15)]
        public RMCResult EndGame()
        {
            UNIMPLEMENTED();
            return Result(new { retVal = true });
        }

        [RMCMethod(16)]
        public RMCResult CancelGame()
        {
            UNIMPLEMENTED();
            return Result(new { retVal = true });
        }

        [RMCMethod(17)]
        public RMCResult GetLeaderboardStats(string LbType)
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(18)]
        public RMCResult SelectTheOwnerForPlayAgain()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(19)]
        public RMCResult CloseParticipation()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(20)]
        public RMCResult BrowseMatchesWithHostUrls(string hostUrls)
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(21)]
        public RMCResult QuickMatchWithHostUrls(int matchType) // Error(0) when no matches found
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(22)]
        public RMCResult GetDetailedInvitationsReceivedWithHostUrls()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(23)]
        public RMCResult OpenParticipation()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(25)]
        public RMCResult ReportStatsWithGlobalLeaderboardList()
        {
            UNIMPLEMENTED();
            return Error(0);
        }
    }
}