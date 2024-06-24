using Newtonsoft.Json;
using QuazalServer.QNetZ.DDL;
using QuazalServer.RDVServices.DDL.Models;
using QuazalServer.RDVServices.Entities;

namespace QuazalServer.RDVServices
{
	public static class DBHelper
	{
		public static (KeyValuePair<string, AnyData<PlayerData>?>?, KeyValuePair<string, AnyData<AccountInfoPrivateData>?>?, User?)? GetUserByName(string name, string AccessKey, bool extraData = false)
		{
            if (Directory.Exists($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts/{AccessKey}"))
            {
                string? parts = Directory.GetFiles($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts/{AccessKey}", $"{name}_*.json").OrderBy(file => file.Length).FirstOrDefault();

                if (!string.IsNullOrEmpty(parts) && File.Exists(parts))
                {
                    (KeyValuePair<string, AnyData<PlayerData>?>?, KeyValuePair<string, AnyData<AccountInfoPrivateData>?>?, User?) keypair = new(null, null, JsonConvert.DeserializeObject<User>(File.ReadAllText(parts)));

                    if (keypair.Item3 == null)
                        return null;

                    if (extraData)
                    {
                        string[] underscoreparts = Path.GetFileNameWithoutExtension(parts).Split('_');

                        if (underscoreparts.Length == 2)
                        {
                            AnyData<PlayerData> playerData = new();
                            using (FileStream fileStream = new(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{underscoreparts[0]}_{underscoreparts[1]}_publicdata.dat", FileMode.Open, FileAccess.Read))
                            {
                                playerData.Read(fileStream);
                            }

                            AnyData<AccountInfoPrivateData> privateplayerData = new();
                            using (FileStream fileStream = new(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{underscoreparts[0]}_{underscoreparts[1]}_privatedata.dat", FileMode.Open, FileAccess.Read))
                            {
                                privateplayerData.Read(fileStream);
                            }

                            keypair.Item1 = new("oPublicData", playerData);
                            keypair.Item2 = new("oPrivateData", privateplayerData);
                        }
                    }

                    return keypair;
                }
            }
            
            return null;
		}

		public static (KeyValuePair<string, AnyData<PlayerData>?>?, KeyValuePair<string, AnyData<AccountInfoPrivateData>?>?, User?)? GetUserByPID(uint PID, string AccessKey, bool extraData = false)
		{
            if (Directory.Exists($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts/{AccessKey}"))
            {
                string? parts = Directory.GetFiles($"{QuazalServerConfiguration.QuazalStaticFolder}/Accounts/{AccessKey}", $"*_{PID}.json").OrderBy(file => file.Length).FirstOrDefault();

                if (!string.IsNullOrEmpty(parts) && File.Exists(parts))
                {
                    (KeyValuePair<string, AnyData<PlayerData>?>?, KeyValuePair<string, AnyData<AccountInfoPrivateData>?>?, User?) keypair = new(null, null, JsonConvert.DeserializeObject<User>(File.ReadAllText(parts)));

                    if (keypair.Item3 == null)
                        return null;

                    if (extraData)
                    {
                        string[] underscoreparts = Path.GetFileNameWithoutExtension(parts).Split('_');

                        if (underscoreparts.Length == 2)
                        {
                            AnyData<PlayerData> playerData = new();
                            using (FileStream fileStream = new(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{underscoreparts[0]}_{underscoreparts[1]}_publicdata.dat", FileMode.Open, FileAccess.Read))
                            {
                                playerData.Read(fileStream);
                            }

                            AnyData<AccountInfoPrivateData> privateplayerData = new();
                            using (FileStream fileStream = new(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{underscoreparts[0]}_{underscoreparts[1]}_privatedata.dat", FileMode.Open, FileAccess.Read))
                            {
                                privateplayerData.Read(fileStream);
                            }

                            keypair.Item1 = new("oPublicData", playerData);
                            keypair.Item2 = new("oPrivateData", privateplayerData);
                        }
                    }

                    return keypair;
                }
            }

            return null;
        }

        public static bool RegisterUser(string strPrincipalName, string strKey, uint uiGroups, string strEmail, string AccessKey, AnyData<PlayerData>? oPublicData = null, AnyData<AccountInfoPrivateData>? oPrivateData = null)
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
                    if (oPublicData != null && oPrivateData != null)
                    {
                        using (FileStream fileStream = new(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}_privatedata.dat", FileMode.Create, FileAccess.Write))
                        {
                            oPrivateData.Write(fileStream);
                        }

                        using (FileStream fileStream = new(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}_publicdata.dat", FileMode.Create, FileAccess.Write))
                        {
                            oPublicData.Write(fileStream);
                        }
                    }

                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}.json", json);
                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}_password.txt", strKey);
                    return true;
                }
            }

            return false;
        }

        public static bool RegisterUserWithPID(string strPrincipalName, string strKey, uint uiGroups, string strEmail, uint PID, string AccessKey, AnyData<PlayerData>? oPublicData = null, AnyData<AccountInfoPrivateData>? oPrivateData = null)
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
                    if (oPublicData != null && oPrivateData != null)
                    {
                        using (FileStream fileStream = new(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}_privatedata.dat", FileMode.Create, FileAccess.Write))
                        {
                            oPrivateData.Write(fileStream);
                        }

                        using (FileStream fileStream = new(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}_publicdata.dat", FileMode.Create, FileAccess.Write))
                        {
                            oPublicData.Write(fileStream);
                        }
                    }

                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}.json", json);
                    File.WriteAllText(QuazalServerConfiguration.QuazalStaticFolder + $"/Accounts/{AccessKey}/{strPrincipalName}_{PID}_password.txt", strKey);
                    return true;
                }
            }

            return false;
        }
    }
}
