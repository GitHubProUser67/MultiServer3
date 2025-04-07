using Newtonsoft.Json;
using QuazalServer.QNetZ;
using QuazalServer.RDVServices.DDL.Models;

namespace QuazalServer.RDVServices.UbisoftDatabase
{
    public class AccountDatabase
    {
        public static List<UbiAccount>? MemoryPlayerDB = null;

        public static UbiAccount? CreateAccount(PlayerInfo playerInfo)
        {
            if (!string.IsNullOrEmpty(playerInfo.UbiAcctName) && !string.IsNullOrEmpty(playerInfo.AccountId))
            {
                // Check if the username already exists
                if (MemoryPlayerDB != null && MemoryPlayerDB.Any(existingAccount => existingAccount.m_username == playerInfo.UbiAcctName))
                    return null;

                // Username doesn't exist, create a new account
                UbiAccount account = new()
                {
                    m_ubi_account_id = GuidGenerator.UBISOFTGenerateGuid(playerInfo.AccountId, playerInfo.UbiAcctName),
                    m_username = playerInfo.UbiAcctName,
                    m_password = playerInfo.UbiPass,
                    m_first_name = string.Empty,
                    m_last_name = string.Empty,
                    m_country_code = playerInfo.UbiCountryCode,
                    m_email = playerInfo.UbiMail,
                    m_preferred_language = playerInfo.UbiLanguageCode,
                    m_gender = 0,
                    m_opt_in = true,
                    m_third_party_opt_in = true,
                    m_status = new UbiAccountStatus()
                    {
                        m_basic_status = 2,
                        m_missing_required_informations = false,
                        m_pending_deactivation = false,
                        m_recovering_password = false
                    },
                    m_external_accounts = new List<ExternalAccount>(),
                    m_date_of_birth = new DateTime(1990, 11, 1)
                };

                MemoryPlayerDB?.Add(account);

                return account;
            }

            return null;
        }

        public static UbiAccount? GetTrackingAccount()
        {
            return MemoryPlayerDB?.FirstOrDefault(account => account.m_ubi_account_id == GuidGenerator.UBISOFTGenerateGuid("Tracking", "Tracking"));
        }

        public static UbiAccount? GetAccountByUsername(string username)
        {
            return MemoryPlayerDB?.FirstOrDefault(account => account.m_username == username);
        }

        public static UbiAccount? GetAccountByUbiAcctId(string? acctid, string? ubiacctname)
        {
            if (string.IsNullOrWhiteSpace(acctid) || string.IsNullOrEmpty(ubiacctname))
                return null;

            return MemoryPlayerDB?.FirstOrDefault(account => account.m_ubi_account_id == GuidGenerator.UBISOFTGenerateGuid(acctid, ubiacctname));
        }

        public static void InitiateDatabase()
        {
            if (File.Exists(QuazalServerConfiguration.QuazalStaticFolder + "/Ubisoft/UplayUserDatabase.json"))
                MemoryPlayerDB = JsonConvert.DeserializeObject<List<UbiAccount>>(File.ReadAllText(QuazalServerConfiguration.QuazalStaticFolder + "/Ubisoft/UplayUserDatabase.json")) ?? new();
            else
                MemoryPlayerDB = new();

            if (GetAccountByUsername("Tracking") == null)
                MemoryPlayerDB.Add(new UbiAccount
                {
                    m_ubi_account_id = GuidGenerator.UBISOFTGenerateGuid("Tracking", "Tracking"),
                    m_username = "Tracking",
                    m_password = string.Empty,
                    m_first_name = string.Empty,
                    m_last_name = string.Empty,
                    m_country_code = "US",
                    m_email = "dummy@ubisoft.fr",
                    m_preferred_language = "en",
                    m_gender = 0,
                    m_opt_in = true,
                    m_third_party_opt_in = true,
                    m_status = new UbiAccountStatus()
                    {
                        m_basic_status = 2,
                        m_missing_required_informations = false,
                        m_pending_deactivation = false,
                        m_recovering_password = false
                    },
                    m_external_accounts = new List<ExternalAccount>(),
                    m_date_of_birth = new DateTime(1990, 11, 1)
                });
        }

        public static void ScheduledDatabaseUpdate(object? state)
        {
            Directory.CreateDirectory(QuazalServerConfiguration.QuazalStaticFolder + "/Ubisoft/");

            // Serialize the List<UbiAccount> to JSON
            File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + "/Ubisoft/UplayUserDatabase.json", JsonConvert.SerializeObject(MemoryPlayerDB, Formatting.Indented));
        }
    }
}
