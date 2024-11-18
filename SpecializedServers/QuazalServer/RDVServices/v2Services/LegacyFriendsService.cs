using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.v2Services
{
    /// <summary>
    /// User friends service
    /// </summary>
    [RMCService(RMCProtocolId.FriendsService)]
	public class LegacyFriendsService : RMCServiceBase
	{
		[RMCMethod(3)]
		public RMCResult UpdateDetails(uint uiPlayer, uint uiDetails)
        {
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(10)]
		public RMCResult GetDetailedList(byte byRelationship, bool bReversed)
        {
            UNIMPLEMENTED();
            return Error(0);
        }
	}
}