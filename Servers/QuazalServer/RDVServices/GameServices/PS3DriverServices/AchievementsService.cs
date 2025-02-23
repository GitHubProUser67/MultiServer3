using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.GameServices.PS3DriverServices
{
    [RMCService(RMCProtocolId.AchievementsService)]
    public class AchievementsService : RMCServiceBase
    {
        [RMCMethod(3)]
        public RMCResult UnlockAchievements(IEnumerable<int> achievementIds)
        {
            return Error(0);
        }
    }
}
