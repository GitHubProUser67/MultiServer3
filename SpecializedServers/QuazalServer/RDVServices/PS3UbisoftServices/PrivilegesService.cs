using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.PS3UbisoftServices
{
    /// <summary>
    /// Secure connection service protocol
    /// </summary>
    [RMCService(RMCProtocolId.PrivilegesService)]
	public class PrivilegesService : RMCServiceBase
	{
        // TODO, properly handle all the privileges per account.

        [RMCMethod(1)]
		public RMCResult GetPrivileges(string localeCode)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		[RMCMethod(2)]
		public RMCResult ActivateKey(string uniqueKey, string languageCode)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(3)]
		public RMCResult ActivateKeyWithExpectedPrivileges(string uniqueKey, string languageCode, IEnumerable<uint> expectedPrivilegeIDs)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		[RMCMethod(4)]
		public RMCResult GetPrivilegeRemainDuration(uint privilege_id)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(5)]
		public RMCResult GetExpiredPrivileges()
		{
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(6)]
		public RMCResult GetPrivilegesEx(string locale_code)
		{
            UNIMPLEMENTED();
            return Error(0);
        }
    }
}
