using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
	[RMCService(RMCProtocolId.LocalizationService)]
	public class LocalizationService : RMCServiceBase
	{
		private static string LocaleCode = "en-US";

        [RMCMethod(1)]
		public RMCResult GetLocaleCode()
		{
			return Result(LocaleCode);
		}

		[RMCMethod(2)]
		public RMCResult SetLocaleCode(string localeCode)
		{
            LocaleCode = localeCode;
            return Error(0);
		}
	}
}
