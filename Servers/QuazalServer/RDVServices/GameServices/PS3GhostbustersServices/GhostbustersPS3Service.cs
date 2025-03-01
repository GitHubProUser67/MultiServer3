using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.DDL;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.GameServices.PS3GhostbustersServices
{
    [RMCService(RMCProtocolId.GhostbustersPS3Service)]
    public class GhostbustersPS3Service : RMCServiceBase
    {
        [RMCMethod(4)]
        public RMCResult RegisterGame()
        {
            UNIMPLEMENTED();
            return Result(new { retVal = true });
        }

        [RMCMethod(9)]
        public RMCResult StatsUpdate(qBuffer data)
        {
            if (Context != null && Context?.Client.PlayerInfo != null)
            {
                try
                {
                    string StatsDirectoryPath = QuazalServerConfiguration.QuazalStaticFolder + $"/Database/Ghostbusters/stats/";

                    Directory.CreateDirectory(StatsDirectoryPath);

                    File.WriteAllBytes(StatsDirectoryPath + $"{Context?.Client.PlayerInfo.PID}.bin", data.data!);
                }
                catch
                {
                    // Not Important.
                }
            }

            return Result(new { retVal = data });

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
