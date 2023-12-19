using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
	/// <summary>
	/// Secure connection service protocol
	/// </summary>
	[RMCService(RMCProtocolId.PrivilegesService)]
	public class PrivilegesService : RMCServiceBase
	{

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
		public void ActivateKey()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(3)]
		public RMCResult ActivateKeyWithExpectedPrivileges(string uniqueKey, string languageCode, IEnumerable<uint> expectedPrivilegeIDs)
		{
			var privilegeList = new List<Privilege>();

            UNIMPLEMENTED();

            PrivilegeGroup privilege = new();
			privilege.m_description = uniqueKey + " unlock";
			privilege.m_privileges = privilegeList;

			return Result(privilege);
		}

		[RMCMethod(4)]
		public void GetPrivilegeRemainDuration()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(5)]
		public void GetExpiredPrivileges()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(6)]
		public void GetPrivilegesEx()
		{
			UNIMPLEMENTED();
		}
	}
}
