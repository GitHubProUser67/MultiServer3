using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.PS3UbisoftServices
{
    [RMCService(RMCProtocolId.LadderHelperProtocol)]
    public class LadderHelperProtocol : RMCServiceBase
    {
        [RMCMethod(1)]
        public RMCResult GetUnixUtc()
        {
            return Result(new { time = Convert.ToUInt32(Math.Abs((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds)) });
        }

        [RMCMethod(2)]
        public RMCResult AreLaddersAvailableInCountry()
        {
            return Result(new { allowed = true });
        }

        [RMCMethod(3)]
        public RMCResult CheckLadderIsRunning(uint start_time, uint end_time)
        {
            return Result(new { running = true });
        }

        [RMCMethod(4)]
        public RMCResult ClearLadderLeaderboard(uint stat_set)
        {
            return Result(new { success = true });
        }
    }
}
