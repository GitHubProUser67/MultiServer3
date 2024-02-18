using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
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
