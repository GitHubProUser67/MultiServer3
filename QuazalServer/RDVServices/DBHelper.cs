using Newtonsoft.Json;
using QuazalServer.RDVServices.Entities;

namespace QuazalServer.RDVServices
{
	public static class DBHelper
	{
		public static User? GetUserByName(string name)
		{
            if (Directory.Exists($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts"))
            {
                // Assuming file name is in the format {username}_{PID}.json
                string[] parts = Directory.GetFiles($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts", $"{name}_*.json");

                if (parts.Length == 1 && File.Exists(parts[0]))
                    return JsonConvert.DeserializeObject<User>(File.ReadAllText(parts[0]));
            }
            
            return null;
		}

		public static User? GetUserByPID(uint PID)
		{
            if (Directory.Exists($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts"))
            {
                // Assuming file name is in the format {username}_{PID}.json
                string[] parts = Directory.GetFiles($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts", $"*_{PID}.json");

                if (parts.Length == 1 && File.Exists(parts[0]))
                    return JsonConvert.DeserializeObject<User>(File.ReadAllText(parts[0]));
            }

            return null;
        }

        public static bool RegisterUser(string strPrincipalName, string strKey, uint uiGroups, string strEmail)
		{
            uint PID = (uint)new Random().Next();

            DateTime servertime = DateTime.Now;

            User newUser = new()
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
            };

            // Serialize the user object to JSON
            string? json = JsonConvert.SerializeObject(newUser, Formatting.Indented);

            if (!string.IsNullOrEmpty(json))
            {
                Directory.CreateDirectory(QuazalServerConfiguration.QuazalStaticFolder + "/Accounts" ?? Directory.GetCurrentDirectory() + "/static/Quazal/Accounts");

                // Assuming file name is in the format {username}_{PID}.json
                string[] parts = Directory.GetFiles($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts", $"{strPrincipalName}_*.json");

                if (parts.Length <= 0 ) // Not create account with same name.
                {
                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{strPrincipalName}_{PID}.json", json);
                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{strPrincipalName}_{PID}_password.txt", strKey);
                    return true;
                }
            }

            return false;
        }

        public static bool RegisterUserWithPID(string strPrincipalName, string strKey, uint uiGroups, string strEmail, uint PID)
        {
            DateTime servertime = DateTime.Now;

            User newUser = new()
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
            };

            // Serialize the user object to JSON
            string? json = JsonConvert.SerializeObject(newUser, Formatting.Indented);

            if (!string.IsNullOrEmpty(json))
            {
                Directory.CreateDirectory(QuazalServerConfiguration.QuazalStaticFolder + "/Accounts" ?? Directory.GetCurrentDirectory() + "/static/Quazal/Accounts");

                // Assuming file name is in the format {username}_{PID}.json
                string[] parts = Directory.GetFiles($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts", $"{strPrincipalName}_*.json");

                if (parts.Length <= 0) // Not create account with same name.
                {
                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{strPrincipalName}_{PID}.json", json);
                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{strPrincipalName}_{PID}_password.txt", strKey);
                    return true;
                }
            }

            return false;
        }
    }
}
