namespace QuazalServer.RDVServices.DDL.Models
{
	public class UbiAccountStatus
	{
		public UbiAccountStatus()
		{
			m_basicStatus = 0;
			m_missingRequiredInformations = false;
			m_recoveringPassword = false;
			m_pendingDeactivation = false;
		}

		public uint m_basicStatus { get; set; }
		public bool m_missingRequiredInformations { get; set; }
		public bool m_recoveringPassword { get; set; }
		public bool m_pendingDeactivation { get; set; }
	}

	public class ExternalAccount
	{
		public ExternalAccount()
		{
			m_accountType = 0;
		}

		public uint m_accountType { get; set; }
		public string? m_id { get; set; }
		public string? m_username { get; set; }
	}

	public class UbiAccount
	{
		public UbiAccount()
		{
			m_externalAccounts = new List<ExternalAccount>();
			m_status = new UbiAccountStatus();
			m_dateOfBirth = new DateTime(2000, 10, 12);
		}

		public string? m_ubiAccountId { get; set; } // guid
		public string? m_username { get; set; }
		public string? m_password { get; set; }
		public UbiAccountStatus m_status { get; set; }
		public string? m_email { get; set; }
		public DateTime m_dateOfBirth { get; set; }
		public uint m_gender { get; set; }
		public string? m_countryCode { get; set; } // KZ
		public bool m_optIn { get; set; }
		public bool m_thirdPartyOptIn { get; set; }
		public string? m_firstName { get; set; }
		public string? m_lastName { get; set; }
		public string? m_preferredLanguage { get; set; } // en
		public List<ExternalAccount> m_externalAccounts { get; set; }

	}
}
