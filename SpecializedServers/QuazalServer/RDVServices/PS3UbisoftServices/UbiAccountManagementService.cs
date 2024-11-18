using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;

namespace QuazalServer.RDVServices.PS3UbisoftServices
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
            UNIMPLEMENTED();
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
            UNIMPLEMENTED();
            return Error(0);
        }

        [RMCMethod(4)]
		public RMCResult? LinkAccount(string ubi_account_username, string ubi_account_password)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		[RMCMethod(5)]
		public RMCResult GetTOS(string country_code, string language_code, bool html_version)
		{
            if (Context != null && Context.Client.PlayerInfo != null)
			{
                Context.Client.PlayerInfo.UbiCountryCode = country_code;
                Context.Client.PlayerInfo.UbiLanguageCode = language_code;
            }

            return Result(new { tos = new TOS(country_code, language_code) });
        }

		[RMCMethod(6)]
		public RMCResult ValidateUsername(string username)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		[RMCMethod(7)]
		public RMCResult ValidatePassword(string password, string username)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		[RMCMethod(8)]
		public RMCResult ValidateEmail(string email)
		{
            UNIMPLEMENTED();
            return Error(0);
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
            UNIMPLEMENTED();
            return Error(0);
        }

		[RMCMethod(12)]
		public RMCResult LookupUbiAccountIDsByPids(IEnumerable<uint> pids)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		[RMCMethod(13)]
		public RMCResult LookupUbiAccountIDsByUsernames(IEnumerable<string> Usernames)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		[RMCMethod(14)]
		public RMCResult LookupUsernamesByUbiAccountIDs(IEnumerable<string> UbiAccountIds)
		{
            UNIMPLEMENTED();
            return Error(0);
        }

		[RMCMethod(15)]
		public RMCResult LookupUbiAccountIDsByUsernameSubString(string UsernameSubString)
		{
            UNIMPLEMENTED();
            return Error(0);
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
