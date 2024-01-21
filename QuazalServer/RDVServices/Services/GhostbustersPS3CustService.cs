using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
    [RMCService(RMCProtocolId.GhostbustersPS3CustService)]
    public class GhostbustersPS3CustService : RMCServiceBase
    {
        [RMCMethod(4)]
        public RMCResult RegisterGame()
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(9)]
        public RMCResult UKN9()
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
