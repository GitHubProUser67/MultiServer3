using Newtonsoft.Json;
using QuazalServer.QNetZ;
using QuazalServer.RDVServices.DDL.Models;

namespace QuazalServer.RDVServices
{
    public class UplayDBHelper
    {
        public static UbiAccount? GetUserByUbiUserName(string userName)
        {
            string UplayDataPath = QuazalServerConfiguration.QuazalStaticFolder + $"/Database/Uplay/account_data/{userName}.json";

            if (File.Exists(UplayDataPath))
                return JsonConvert.DeserializeObject<UbiAccount>(File.ReadAllText(UplayDataPath));

            return null;
        }

        public static UbiAccount? RegisterUser(PlayerInfo playerInfo)
        {
            string UplayDataPath = QuazalServerConfiguration.QuazalStaticFolder + $"/Database/Uplay/account_data/{playerInfo.UbiAcctName}.json";

            if (File.Exists(UplayDataPath))
                return null;
            else
            {
                UbiAccount account = new()
                {
                    m_ubi_account_id = playerInfo.AccountId,
                    m_username = playerInfo.UbiAcctName,
                    m_password = playerInfo.UbiPass,
                    m_first_name = playerInfo.Name,
                    m_last_name = "",
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
                    m_date_of_birth = new System.DateTime(1990, 11, 1)
                };

                Directory.CreateDirectory(QuazalServerConfiguration.QuazalStaticFolder + $"/Database/Uplay/account_data/currency");

                File.WriteAllText(UplayDataPath, JsonConvert.SerializeObject(account));
                File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Database/Uplay/account_data/currency/{playerInfo.UbiAcctName}.txt", "10");

                return account;
            }
        }

        public static UbiAccount? UpdateUser(PlayerInfo playerInfo)
        {
            string UplayDataPath = QuazalServerConfiguration.QuazalStaticFolder + $"/Database/Uplay/account_data/{playerInfo.UbiAcctName}.json";

            if (File.Exists(UplayDataPath))
            {
                UbiAccount account = new()
                {
                    m_ubi_account_id = playerInfo.AccountId,
                    m_username = playerInfo.UbiAcctName,
                    m_password = playerInfo.UbiPass,
                    m_first_name = playerInfo.Name,
                    m_last_name = "",
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
                    m_date_of_birth = new System.DateTime(1990, 11, 1)
                };

                Directory.CreateDirectory(QuazalServerConfiguration.QuazalStaticFolder + $"/Database/Uplay/account_data");

                File.WriteAllText(UplayDataPath, JsonConvert.SerializeObject(account));

                return account;
            }

            return null;
        }
    }
}
