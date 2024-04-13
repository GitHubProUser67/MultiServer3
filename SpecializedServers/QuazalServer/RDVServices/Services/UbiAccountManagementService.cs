using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;

namespace QuazalServer.RDVServices.Services
{
	/// <summary>
	/// Ubi account management service
	/// </summary>
	[RMCService(RMCProtocolId.UbiAccountManagementService)]
	public class UbiAccountManagementService : RMCServiceBase
	{
		[RMCMethod(1)]
		public RMCResult CreateAccount()
		{
            if (Context != null && Context.Client.Info != null)
			{
				UbiAccount? account = UbisoftDatabase.AccountDatabase.CreateAccount(Context.Client.Info);

				if (account != null)
                    return Result(new { ubi_account = account, failed_reasons = new List<ValidationFailureReason>() });
            }

			return Error(0);
        }

		[RMCMethod(2)]
		public void UpdateAccount()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(3)]
		public RMCResult GetAccount()
		{
			if (Context != null && Context.Client.Info != null)
			{
                PlayerInfo? playerInfo = Context.Client.Info;

				if (playerInfo != null)
				{
					UbiAccount? account = UbisoftDatabase.AccountDatabase.GetAccountByUbiAcctId(playerInfo.AccountId, playerInfo.UbiAcctName);

                    if (account != null)
                        return Result(new { account = account, exist = true });
                    else if (!string.IsNullOrEmpty(playerInfo.Name))
					{
						if (playerInfo.Name == "Tracking")
                            return Result(new { account = UbisoftDatabase.AccountDatabase.GetTrackingAccount(), exist = true });
						else
						{
							if (Context.Handler.AccessKey == "QusaPha9") // Workaround for UplayV2 freeze in RPCS3.
                                return Result(new
                                {
                                    account = new UbiAccount()
                                    {
                                        m_ubi_account_id = GuidGenerator.UBISOFTGenerateGuid(playerInfo.AccountId, playerInfo.Name),
                                        m_username = playerInfo.Name,
                                        m_password = string.Empty,
                                        m_first_name = playerInfo.Name,
                                        m_last_name = playerInfo.Name,
                                        m_country_code = "KZ",
                                        m_email = "whatever@dontcare.com",
                                        m_preferred_language = "en",
                                        m_gender = 0,
                                        m_opt_in = true,
                                        m_third_party_opt_in = true,
                                        m_status = new UbiAccountStatus()
                                        {
                                            m_basic_status = 2,
                                            m_missing_required_informations = false,
                                            m_pending_deactivation = false,
                                            m_recovering_password = true
                                        },
                                        m_external_accounts = new List<ExternalAccount>()
                                        {
                                            new ExternalAccount()
                                            {
                                                m_account_type = 11,
                                                m_id = "loh",
                                                m_username = "aabb0"
                                            },
                                            new ExternalAccount()
                                            {
                                                m_account_type = 25,
                                                m_id = "pidr",
                                                m_username = "aabb1"
                                            },
                                            new ExternalAccount()
                                            {
                                                m_account_type = 31,
                                                m_id = "whatev",
                                                m_username = "aabb2"
                                            }

                                        },
                                        m_date_of_birth = new DateTime(1990, 11, 1)
                                    },
                                    exist = true
                                });
                            else
                                return Result(new { account = playerInfo.Name, exist = false });
                        }
                    }
                }
            }

            return Result(new { exist = false });
        }

        [RMCMethod(4)]
		public RMCResult? LinkAccount(string ubi_account_username, string ubi_account_password)
		{
			if (Context != null && Context.Client.Info != null)
			{
                UbiAccount? account = UbisoftDatabase.AccountDatabase.GetAccountByUsername(ubi_account_username);

                if (account != null && account.m_password == ubi_account_password)
                {
                    Context.Client.Info.UbiAcctName = account.m_username;
                    Context.Client.Info.UbiPass = account.m_password;
					Context.Client.Info.UbiMail = account.m_email;
					Context.Client.Info.UbiLanguageCode = account.m_preferred_language;
                    Context.Client.Info.UbiCountryCode = account.m_country_code;
                    return Error(0);
                }
            }

			return null; // Only way to reject invalid accounts.
        }

		[RMCMethod(5)]
		public RMCResult GetTOS(string country_code, string language_code, bool html_version)
		{
            if (Context != null && Context.Client.Info != null)
			{
                Context.Client.Info.UbiCountryCode = country_code;
                Context.Client.Info.UbiLanguageCode = language_code;
            }

            return Result(new { tos = new TOS(country_code, language_code) });
        }

		[RMCMethod(6)]
		public RMCResult ValidateUsername(string username)
		{
            if (Context != null && Context.Client.Info != null)
                Context.Client.Info.UbiAcctName = username;

            return Result(new { username_validation = new UsernameValidation() });
        }

		[RMCMethod(7)]
		public RMCResult ValidatePassword(string password, string username)
		{
            if (Context != null && Context.Client.Info != null)
                Context.Client.Info.UbiPass = password;

            return Result(new { failed_reasons = new List<ValidationFailureReason>() });
        }

		[RMCMethod(8)]
		public RMCResult ValidateEmail(string email)
		{
            if (Context != null && Context.Client.Info != null)
                Context.Client.Info.UbiMail = email;

            return Result(new { failed_reasons = new List<ValidationFailureReason>() });
        }

		[RMCMethod(9)]
		public void GetCountryList()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(10)]
		public void ForgetPassword()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(11)]
		public RMCResult LookupPrincipalIds(IEnumerable<string> ubiAccountIds)
		{
			var pids = new Dictionary<string, uint>();
            UNIMPLEMENTED();
            return Result(pids);
		}

		[RMCMethod(12)]
		public RMCResult LookupUbiAccountIDsByPids(IEnumerable<uint> pids)
		{
			var ubiaccountIDs = new Dictionary<uint, string>();
            UNIMPLEMENTED();
            return Result(ubiaccountIDs);
		}

		[RMCMethod(13)]
		public RMCResult LookupUbiAccountIDsByUsernames(IEnumerable<string> Usernames)
		{
			var UbiAccountIDs = new Dictionary<string, string>();
            UNIMPLEMENTED();
            return Result(UbiAccountIDs);
		}

		[RMCMethod(14)]
		public RMCResult LookupUsernamesByUbiAccountIDs(IEnumerable<string> UbiAccountIds)
		{
			var Usernames = new Dictionary<string, string>();
            UNIMPLEMENTED();
            return Result(Usernames);
		}

		[RMCMethod(15)]
		public RMCResult LookupUbiAccountIDsByUsernameSubString(string UsernameSubString)
		{
			var UbiAccountIDs = new Dictionary<string, string>();
			
			UNIMPLEMENTED();
			return Result(UbiAccountIDs);
		}

		[RMCMethod(16)]
		public void UserHasPlayed()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(17)]
		public void IsUserPlaying()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(18)]
		public void LookupUbiAccountIDsByUsernamesGlobal()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(19)]
		public void LookupUbiAccountIDsByEmailsGlobal()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(20)]
		public void LookupUsernamesByUbiAccountIDsGlobal()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(21)]
		public void GetTOSEx()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(22)]
		public void HasAcceptedLatestTOS()
		{
			UNIMPLEMENTED();
		}

		[RMCMethod(23)]
		public void AcceptLatestTOS()
		{
			UNIMPLEMENTED();
		}
	}
}
