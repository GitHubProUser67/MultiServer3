using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.PS3UbisoftServices
{
    [RMCService(RMCProtocolId.OverlordFriendsService)]
	public class OverlordFriendsService : RMCServiceBase
	{
		[RMCMethod(1)]
		public RMCResult SyncFriends(uint friendType, IEnumerable<string> friends)
		{
			var list = new List<string>();
			return Result(list);
		}
	}
}
