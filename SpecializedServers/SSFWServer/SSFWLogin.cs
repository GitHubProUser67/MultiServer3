using CustomLogger;
using CyberBackendLibrary.DataTypes;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using WebAPIService.SSFW;

namespace SSFWServer
{
    public static class SSFWUserSessionManager
    {
        private static List<UserSession>? userSessions = new();

        public static void RegisterUser(string username, string sessionid)
        {
            if (userSessions != null)
            {
                if (userSessions.Any(u => u.SessionId == sessionid))
                {
                    // Do nothing.
                }
                else
                {
                    userSessions.Add(new UserSession { Username = username, SessionId = sessionid });
                    LoggerAccessor.LogInfo($"[UserSessionManager] - User '{username}' successfully registered with SessionId '{sessionid}'.");
                }
            }
        }

        public static string? GetUsernameBySessionId(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            return userSessions?.FirstOrDefault(u => u.SessionId == sessionId)?.Username;
        }
    }

    public class UserSession
    {
        public string? Username { get; set; }
        public string? SessionId { get; set; }
    }

    public class SSFWLogin : IDisposable
    {
        private string? XHomeClientVersion;
        private string? generalsecret;
        private string? homeClientVersion;
        private string? xsignature;
        private string? key;
        private bool disposedValue;

        public SSFWLogin(string XHomeClientVersion, string generalsecret, string homeClientVersion, string? xsignature, string? key)
        {
            this.XHomeClientVersion = XHomeClientVersion;
            this.generalsecret = generalsecret;
            this.homeClientVersion = homeClientVersion;
            this.xsignature = xsignature;
            this.key = key;
        }

        public string? HandleLogin(byte[]? bufferwrite, string env)
        {
            if (bufferwrite != null)
            {
                bool rpcn = false;
                int logoncount = 1;
                string sessionid = string.Empty;
                string salt = string.Empty;
                string username = string.Empty;
                string resultString = string.Empty;
                // Create a byte array

                // Extract the desired portion of the binary data
                byte[] extractedData = new byte[0x63 - 0x54 + 1];

                // Copy it
                Array.Copy(bufferwrite, 0x54, extractedData, 0, extractedData.Length);

                // Convert 0x00 bytes to 0x48 so FileSystem can support it
                for (int i = 0; i < extractedData.Length; i++)
                {
                    if (extractedData[i] == 0x00)
                        extractedData[i] = 0x48;
                }

                if (DataTypesUtils.FindbyteSequence(bufferwrite, new byte[] { 0x52, 0x50, 0x43, 0x4E }))
                {
                    rpcn = true;
                    LoggerAccessor.LogInfo($"[SSFW] : User {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on RPCN");
                }
                else
                    LoggerAccessor.LogInfo($"[SSFW] : {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on PSN");

                if (rpcn && !SSFWServerConfiguration.SSFWCrossSave)
                {
                    // Convert the modified data to a string
                    username = resultString = Encoding.ASCII.GetString(extractedData) + "RPCN" + homeClientVersion;

                    // Calculate the MD5 hash of the result
                    using (MD5 md5 = MD5.Create())
                    {
                        if (!string.IsNullOrEmpty(xsignature))
                            salt = generalsecret + xsignature + XHomeClientVersion;
                        else
                            salt = generalsecret + XHomeClientVersion;

                        byte[] hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(resultString + salt));
                        string hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                        // Trim the hash to a specific length
                        hash = hash[..10];

                        // Append the trimmed hash to the result
                        resultString += hash;

                        sessionid = GuidGenerator.SSFWGenerateGuid(hash, resultString);

                        SSFWUserSessionManager.RegisterUser(username, sessionid);
                    }
                }
                else
                {
                    // Convert the modified data to a string
                    username = resultString = Encoding.ASCII.GetString(extractedData) + homeClientVersion;

                    // Calculate the MD5 hash of the result
                    using (MD5 md5 = MD5.Create())
                    {
                        if (!string.IsNullOrEmpty(xsignature))
                            salt = generalsecret + xsignature + XHomeClientVersion;
                        else
                            salt = generalsecret + XHomeClientVersion;

                        byte[] hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(resultString + salt));
                        string hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                        // Trim the hash to a specific length
                        hash = hash[..14];

                        // Append the trimmed hash to the result
                        resultString += hash;

                        sessionid = GuidGenerator.SSFWGenerateGuid(hash, resultString);

                        SSFWUserSessionManager.RegisterUser(username, sessionid);
                    }
                }

                string userprofilefile = $"{SSFWServerConfiguration.SSFWStaticFolder}/SSFW_Accounts/{username}.json";

                string olduserprofilefile = $"{SSFWServerConfiguration.SSFWStaticFolder}/SSFW_Accounts/{sessionid}.json";

                if (File.Exists(olduserprofilefile))
                    File.Move(olduserprofilefile, userprofilefile);

                if (File.Exists(userprofilefile))
                {
                    string? userprofiledata = FileHelper.ReadAllText(userprofilefile, key);
                    if (!string.IsNullOrEmpty(userprofiledata))
                    {
                        // Parsing JSON data to SSFWUserData object
                        SSFWUserData? userData = JsonConvert.DeserializeObject<SSFWUserData>(userprofiledata);
                        if (userData != null)
                        {
                            // Modifying the object if needed
                            userData.LogonCount += 1;
                            logoncount = userData.LogonCount;
                            File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/SSFW_Accounts/{username}.json",JsonConvert.SerializeObject(userData, Formatting.Indented));
                        }
                    }
                    else
                        LoggerAccessor.LogError($"[SSFWLogin] - Profile data couldn't be read at this location: {userprofilefile}");
                }
                else
                {
                    Directory.CreateDirectory($"{SSFWServerConfiguration.SSFWStaticFolder}/SSFW_Accounts");

                    // Parsing JSON data to SSFWUserData object
                    SSFWUserData? userData = JsonConvert.DeserializeObject<SSFWUserData>($"{{\"Username\":\"{sessionid}\",\"LogonCount\":{logoncount},\"IGA\":0}}");
                    if (userData != null)
                    {
                        LoggerAccessor.LogInfo($"[SSFW] : Account Created - {Encoding.ASCII.GetString(extractedData)} - Session ID : {userData.Username}");
                        LoggerAccessor.LogInfo($"[SSFW] : Account Created - {Encoding.ASCII.GetString(extractedData)} - LogonCount : {userData.LogonCount}");
                        LoggerAccessor.LogInfo($"[SSFW] : Account Created - {Encoding.ASCII.GetString(extractedData)} - IGA : {userData.IGA}");

                        File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/SSFW_Accounts/{username}.json", JsonConvert.SerializeObject(userData, Formatting.Indented));
                    }
                }

                Directory.CreateDirectory($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}");
                Directory.CreateDirectory($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{resultString}");
                Directory.CreateDirectory($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/trunks-{env}/trunks");
                Directory.CreateDirectory($"{SSFWServerConfiguration.SSFWStaticFolder}/AvatarLayoutService/{env}/{resultString}");

                if (File.Exists(SSFWServerConfiguration.ScenelistFile))
                {
                    bool handled = false;

                    Dictionary<string, string> scenemap = ScenelistParser.sceneDictionary;

                    if (File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/mylayout.json")) // Migrate data.
                    {
                        // Parsing each value in the dictionary
                        foreach (var kvp in new Services.SSFWLayoutService(key).SSFWGetLegacyFurnitureLayouts($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/mylayout.json"))
                        {
                            if (kvp.Key == "00000000-00000000-00000000-00000004")
                            {
                                File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/HarborStudio.json", kvp.Value);
                                handled = true;
                            }
                            else
                            {
                                string scenename = scenemap.FirstOrDefault(x => x.Value == SSFWMisc.ExtractPortion(kvp.Key, 13, 18)).Key;
                                if (!string.IsNullOrEmpty(scenename))
                                {
                                    if (File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/{kvp.Key}.json")) // SceneID now mapped, so SceneID based file has become obsolete.
                                        File.Delete($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/{kvp.Key}.json");

                                    File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/{scenename}.json", kvp.Value);
                                    handled = true;
                                }
                            }

                            if (!handled)
                                File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/{kvp.Key}.json", kvp.Value);

                            handled = false;
                        }

                        File.Delete($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/mylayout.json");
                    }
                    else if (!File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/HarborStudio.json"))
                        File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/HarborStudio.json", SSFWMisc.HarbourStudioLayout);
                }
                else
                {
                    if (!File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/mylayout.json"))
                        File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/mylayout.json", SSFWMisc.LegacyLayoutTemplate);
                }
                if (!File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{resultString}/mini.json"))
                    File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{resultString}/mini.json", SSFWServerConfiguration.SSFWMinibase);
                if (!File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/trunks-{env}/trunks/{resultString}.json"))
                    File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/trunks-{env}/trunks/{resultString}.json", "{\"objects\":[]}");
                if (!File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/AvatarLayoutService/{env}/{resultString}/list.json"))
                    File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/AvatarLayoutService/{env}/{resultString}/list.json", "[]");

                return $"{{\"session\":[{{\"@id\":\"{sessionid}\",\"person\":{{\"@id\":\"{resultString}\",\"logonCount\":\"{logoncount}\"}}}}]}}";
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    XHomeClientVersion = null;
                    generalsecret = null;
                    homeClientVersion = null;
                    xsignature = null;
                    key = null;
                }

                // TODO: libérer les ressources non managées (objets non managés) et substituer le finaliseur
                // TODO: affecter aux grands champs une valeur null
                disposedValue = true;
            }
        }

        // // TODO: substituer le finaliseur uniquement si 'Dispose(bool disposing)' a du code pour libérer les ressources non managées
        // ~SSFWLogin()
        // {
        //     // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Ne changez pas ce code. Placez le code de nettoyage dans la méthode 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}