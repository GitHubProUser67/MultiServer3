using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
	[RMCService(RMCProtocolId.LocalizationService)]
	public class LocalizationService : RMCServiceBase
	{
		[RMCMethod(1)]
		public RMCResult GetLocaleCode()
		{
			return Result("en-US");
		}

		[RMCMethod(2)]
		public RMCResult SetLocaleCode(string localeCode)
		{
			return Error(0);
		}
	}
}
