using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.PS3GhostbustersServices
{
    [RMCService(RMCProtocolId.GhostbustersPS3CustService)]
    public class GhostbustersPS3CustService : RMCServiceBase
    {
        [RMCMethod(4)]
        public RMCResult RegisterGame()
        {
            UNIMPLEMENTED();
            return Result(new { retVal = true });
        }

        [RMCMethod(9)]
        public RMCResult Stats()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(10)]
        public RMCResult GetFriends() // Error(0) when no friends.
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(16)]
        public RMCResult LeaveGame()
        {
            UNIMPLEMENTED();
            return Result(new { retVal = true });
        }

        [RMCMethod(17)]
        public RMCResult Leaderboard()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(20)]
        public RMCResult GlobalSearchGames() // Error(0) when no games available.
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(21)]
        public RMCResult SearchGames() // Error(0) when no games available.
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(22)]
        public RMCResult ViewInvites() // Error(0) when no invites.
        {
            UNIMPLEMENTED();
            return Error(0);
        }
    }
}
