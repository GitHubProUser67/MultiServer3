using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
	[RMCService(RMCProtocolId.DriverUniqueIDService)]
	public class DriverUniqueIDService : RMCServiceBase
	{
		static uint UniqueIDCounter = 26434;

		[RMCMethod(2)]
		public RMCResult CreateUniqueID()
		{
			return Result(new { value = ++UniqueIDCounter });
		}
	}
}
