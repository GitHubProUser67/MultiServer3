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
			var privileges = new Dictionary<uint, Privilege>();

			uint id = 1;
			privileges.Add(id++, new Privilege()
			{
				m_ID = 1,
				m_description = "Allow to play online"
			});

            UNIMPLEMENTED();

            return Result(privileges);
		}

		[RMCMethod(2)]
		public RMCResult ActivateKey(string uniqueKey, string languageCode)
		{
            UNIMPLEMENTED();

            PrivilegeGroup privilege = new()
            {
                m_description = uniqueKey + " unlock",
                m_privileges = new List<Privilege>()
            };

            return Result(privilege);
		}

        [RMCMethod(3)]
		public RMCResult ActivateKeyWithExpectedPrivileges(string uniqueKey, string languageCode, IEnumerable<uint> expectedPrivilegeIDs)
		{
            UNIMPLEMENTED();

            PrivilegeGroup privilege = new()
            {
                m_description = uniqueKey + " unlock",
                m_privileges = new List<Privilege>()
            };

            return Result(privilege);
		}

		[RMCMethod(4)]
		public RMCResult GetPrivilegeRemainDuration(uint privilege_id)
		{
            return Result(new { seconds = (uint)1000000 });
        }

        [RMCMethod(5)]
		public RMCResult GetExpiredPrivileges()
		{
            return Result(new { expired_privileges = new List<PrivilegeEx>() });
        }

        [RMCMethod(6)]
		public RMCResult GetPrivilegesEx(string locale_code)
		{
            return Result(new { privileges_ex = new List<PrivilegeEx>() });
        }
    }
}
