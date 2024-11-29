using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.QNetZ;
using QuazalServer.QNetZ.Attributes;
using QuazalServer.QNetZ.Interfaces;
using QuazalServer.RDVServices.RMC;
using RDVServices;

namespace QuazalServer.RDVServices.PS3DriverServices
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
            var playerInfo = Context.Client.PlayerInfo;
            var account = new UbiAccount()
            {
                m_ubi_account_id = playerInfo.AccountId,
                m_username = playerInfo.Name,
                m_password = "",
                m_first_name = "",
                m_last_name = "",
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
                m_date_of_birth = new System.DateTime(1990, 11, 1)
            };

            return Result(new { account = account, exist = true });
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
            var pids = new Dictionary<string, uint>();
            using (var db = DBHelper.GetDbContext(Context.Handler.Factory.Item1))
            {
                var usersList = db.Users.Where(x => ubiAccountIds.Contains(x.Username)).ToArray();

                foreach (var usr in usersList)
                {
                    pids[usr.Username] = usr.Id;
                }
            }
            return Result(pids);
        }

        [RMCMethod(12)]
        public RMCResult LookupUbiAccountIDsByPids(IEnumerable<uint> pids)
        {
            var ubiaccountIDs = new Dictionary<uint, string>();

            using (var db = DBHelper.GetDbContext(Context.Handler.Factory.Item1))
            {
                var usersList = db.Users.Where(x => pids.Contains(x.Id)).ToArray();

                foreach (var usr in usersList)
                {
                    ubiaccountIDs[usr.Id] = usr.PlayerNickName;
                }
            }

            return Result(ubiaccountIDs);
        }

        [RMCMethod(13)]
        public RMCResult LookupUbiAccountIDsByUsernames(IEnumerable<string> Usernames)
        {
            var UbiAccountIDs = new Dictionary<string, string>();

            using (var db = DBHelper.GetDbContext(Context.Handler.Factory.Item1))
            {
                var usersList = db.Users.Where(x => Usernames.Contains(x.PlayerNickName)).ToArray();

                foreach (var usr in usersList)
                {
                    UbiAccountIDs[usr.Username] = usr.PlayerNickName;
                }
            }

            return Result(UbiAccountIDs);
        }

        [RMCMethod(14)]
        public RMCResult LookupUsernamesByUbiAccountIDs(IEnumerable<string> UbiAccountIds)
        {
            var Usernames = new Dictionary<string, string>();

            using (var db = DBHelper.GetDbContext(Context.Handler.Factory.Item1))
            {
                var usersList = db.Users.Where(x => UbiAccountIds.Contains(x.Username)).ToArray();

                foreach (var usr in usersList)
                {
                    Usernames[usr.Username] = usr.Username;
                }
            }

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
