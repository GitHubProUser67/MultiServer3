using CustomLogger;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using CryptoSporidium.FileHelper;
using CryptoSporidium.WebAPIs.SSFW;
using CryptoSporidium;

namespace SSFWServer
{
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
                int logoncount = 1;
                string sessionid = string.Empty;
                string salt = string.Empty;
                string resultString = "Default";
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

                if (MiscUtils.FindbyteSequence(bufferwrite, new byte[] { 0x52, 0x50, 0x43, 0x4E }) && !SSFWServerConfiguration.SSFWCrossSave)
                {
                    LoggerAccessor.LogInfo($"[SSFW] : User {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on RPCN");

                    // Convert the modified data to a string
                    resultString = Encoding.ASCII.GetString(extractedData) + "RPCN" + homeClientVersion;

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
                        hash = hash.Substring(0, 10);

                        // Append the trimmed hash to the result
                        resultString += hash;

                        sessionid = GuidGenerator.SSFWGenerateGuid(hash, resultString);

                        md5.Clear();
                    }
                }
                else
                {
                    LoggerAccessor.LogInfo($"[SSFW] : {Encoding.ASCII.GetString(extractedData).Replace("H", string.Empty)} logged in and is on PSN");

                    // Convert the modified data to a string
                    resultString = Encoding.ASCII.GetString(extractedData) + homeClientVersion;

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
                        hash = hash.Substring(0, 14);

                        // Append the trimmed hash to the result
                        resultString += hash;

                        sessionid = GuidGenerator.SSFWGenerateGuid(hash, resultString);

                        md5.Dispose();
                    }
                }

                string userprofilefile = $"{SSFWServerConfiguration.SSFWStaticFolder}/SSFW_Accounts/{sessionid}.json";

                if (File.Exists(userprofilefile))
                {
                    string? userprofiledata = FileHelper.ReadAllText(userprofilefile, key);
                    if (userprofiledata != null)
                    {
                        // Parsing JSON data to SSFWUserData object
                        SSFWUserData? userData = JsonConvert.DeserializeObject<SSFWUserData>(userprofiledata);
                        if (userData != null)
                        {
                            // Modifying the object if needed
                            userData.LogonCount += 1;
                            logoncount = userData.LogonCount;
                            File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/SSFW_Accounts/{sessionid}.json", JsonConvert.SerializeObject(userData));
                        }
                    }
                    else
                        return null;
                }
                else
                {
                    Directory.CreateDirectory($"{SSFWServerConfiguration.SSFWStaticFolder}/SSFW_Accounts");
                    string tempcontent = $"{{\"Username\":\"{sessionid}\",\"LogonCount\":{logoncount},\"IGA\":0}}";
                    // Parsing JSON data to SSFWUserData object
                    SSFWUserData? userData = JsonConvert.DeserializeObject<SSFWUserData>(tempcontent);
                    if (userData != null)
                    {
                        LoggerAccessor.LogInfo($"[SSFW] : Account Created - {Encoding.ASCII.GetString(extractedData)} - Session ID : {userData.Username}");
                        LoggerAccessor.LogInfo($"[SSFW] : Account Created - {Encoding.ASCII.GetString(extractedData)} - LogonCount : {userData.LogonCount}");
                        LoggerAccessor.LogInfo($"[SSFW] : Account Created - {Encoding.ASCII.GetString(extractedData)} - IGA : {userData.IGA}");

                        File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/SSFW_Accounts/{sessionid}.json", JsonConvert.SerializeObject(userData));
                    }
                }

                Directory.CreateDirectory($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}");
                Directory.CreateDirectory($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{resultString}");
                Directory.CreateDirectory($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/trunks-{env}/trunks");
                Directory.CreateDirectory($"{SSFWServerConfiguration.SSFWStaticFolder}/AvatarLayoutService/{env}/{resultString}");


                if (!File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/mylayout.json"))
                    File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/LayoutService/{env}/person/{resultString}/mylayout.json", SSFWMisc.LayoutTemplate);
                if (!File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{resultString}/mini.json"))
                    File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/{env}/rewards/{resultString}/mini.json", SSFWServerConfiguration.SSFWMinibase);
                if (!File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/trunks-{env}/trunks/{resultString}.json"))
                    File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/RewardsService/trunks-{env}/trunks/{resultString}.json", "{\"objects\":[]}");
                if (!File.Exists($"{SSFWServerConfiguration.SSFWStaticFolder}/AvatarLayoutService/{env}/{resultString}/list.json"))
                    File.WriteAllText($"{SSFWServerConfiguration.SSFWStaticFolder}/AvatarLayoutService/{env}/{resultString}/list.json", "[]");

                return $"{{\"session\":[{{\"@id\":\"{sessionid}\",\"person\":{{\"@id\":\"{resultString}\",\"logonCount\":{logoncount}}}}}]}}";
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
