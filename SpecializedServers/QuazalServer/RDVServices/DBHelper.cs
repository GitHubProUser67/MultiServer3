using Newtonsoft.Json;
using QuazalServer.RDVServices.Entities;

namespace QuazalServer.RDVServices
{
	public static class DBHelper
	{
		public static User? GetUserByName(string name, string AccessKey)
		{
            if (Directory.Exists($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts/{AccessKey}"))
            {
                string? parts = Directory.GetFiles($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts/{AccessKey}", $"{name}_*.json").Where(file => !file.Contains("password")).FirstOrDefault();

                if (!string.IsNullOrEmpty(parts) && File.Exists(parts))
                    return JsonConvert.DeserializeObject<User>(File.ReadAllText(parts));
            }
            
            return null;
		}

		public static User? GetUserByPID(uint PID, string AccessKey)
		{
            if (Directory.Exists($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts/{AccessKey}"))
            {
                string? parts = Directory.GetFiles($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts/{AccessKey}", $"*_{PID}.json").Where(file => !file.Contains("password")).FirstOrDefault();

                if (!string.IsNullOrEmpty(parts) && File.Exists(parts))
                    return JsonConvert.DeserializeObject<User>(File.ReadAllText(parts));
            }

            return null;
        }

        public static bool RegisterUser(string strPrincipalName, string strKey, uint uiGroups, string strEmail, string AccessKey)
		{
            uint PID = QNetZ.NetworkPlayers.GenerateUniqueUint(strPrincipalName);

            DateTime servertime = DateTime.Now;

            // Serialize the user object to JSON
            string? json = JsonConvert.SerializeObject(new User()
            {
                Id = PID,
                Username = strPrincipalName,
                PlayerNickName = strPrincipalName,
                PID = PID,
                Name = strPrincipalName,
                UiGroups = uiGroups,
                Email = strEmail,
                CreationDate = servertime,
                EffectiveDate = servertime,
                ExpiryDate = servertime.AddYears(500),
                Password = strKey
            }, Formatting.Indented);

            if (!string.IsNullOrEmpty(json))
            {
                Directory.CreateDirectory(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}" ?? Directory.GetCurrentDirectory() + $"/static/Quazal/Accounts/{AccessKey}");

                string[] parts = Directory.GetFiles($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts/{AccessKey}", $"{strPrincipalName}_*.json");

                if (parts.Length == 0) // Not create account with same name.
                {
                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}.json", json);
                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}_password.txt", strKey);
                    return true;
                }
            }

            return false;
        }

        public static bool RegisterUserWithPID(string strPrincipalName, string strKey, uint uiGroups, string strEmail, uint PID, string AccessKey)
        {
            DateTime servertime = DateTime.Now;

            // Serialize the user object to JSON
            string? json = JsonConvert.SerializeObject(new User()
            {
                Id = PID,
                Username = strPrincipalName,
                PlayerNickName = strPrincipalName,
                PID = PID,
                Name = strPrincipalName,
                UiGroups = uiGroups,
                Email = strEmail,
                CreationDate = servertime,
                EffectiveDate = servertime,
                ExpiryDate = servertime.AddYears(500),
                Password = strKey
            }, Formatting.Indented);

            if (!string.IsNullOrEmpty(json))
            {
                Directory.CreateDirectory(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}" ?? Directory.GetCurrentDirectory() + $"/static/Quazal/Accounts/{AccessKey}");

                string[] parts = Directory.GetFiles($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts/{AccessKey}", $"{strPrincipalName}_*.json");

                if (parts.Length == 0) // Not create account with same name.
                {
                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}.json", json);
                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}_password.txt", strKey);
                    return true;
                }
            }

            return false;
        }
    }
}
