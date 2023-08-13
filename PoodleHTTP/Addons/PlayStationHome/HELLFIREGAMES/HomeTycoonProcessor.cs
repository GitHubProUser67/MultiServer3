using HttpMultipartParser;
using Newtonsoft.Json.Linq;
using PSMultiServer.PoodleHTTP.Addons.PlayStationHome.SSFW;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PSMultiServer.PoodleHTTP.Addons.PlayStationHome.HELLFIREGAMES
{
    public class HomeTycoonProcessor
    {
        public static async Task<byte[]> RequestNPTicket(string boundary, MemoryStream copyStream)
        {
            string resultString = "";

            string userid = "";

            string sessionid = "";

            byte[] ticketData = NpTicketData.ExtractTicketData(copyStream, boundary);

            // Extract the desired portion of the binary data
            byte[] extractedData = new byte[0x63 - 0x54 + 1];

            // Copy it
            Array.Copy(ticketData, 0x54, extractedData, 0, extractedData.Length);

            // Convert 0x00 bytes to 0x20 so we pad as space.
            for (int i = 0; i < extractedData.Length; i++)
            {
                if (extractedData[i] == 0x00)
                {
                    extractedData[i] = 0x20;
                }
            }

            if (Misc.FindbyteSequence(ticketData, new byte[] { 0x52, 0x50, 0x43, 0x4E }))
            {
                ServerConfiguration.LogInfo($"[HomeTycoon] : User {Encoding.ASCII.GetString(extractedData).Replace("H", "")} logged in and is on RPCN");

                // Convert the modified data to a string
                resultString = Encoding.ASCII.GetString(extractedData) + "RPCN";

                userid = resultString.Replace(" ", "");

                // Calculate the MD5 hash of the result
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(resultString + "H0mETyc00n!"));
                    string hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                    // Trim the hash to a specific length
                    hash = hash.Substring(0, 10);

                    // Append the trimmed hash to the result
                    resultString += hash;

                    sessionid = SSFWProcessor.ssfwgenerateguid(hash, resultString);

                    md5.Dispose();
                }
            }
            else
            {
                ServerConfiguration.LogInfo($"[HomeTycoon] : {Encoding.ASCII.GetString(extractedData).Replace("H", "")} logged in and is on PSN");

                // Convert the modified data to a string
                resultString = Encoding.ASCII.GetString(extractedData);

                userid = resultString.Replace(" ", "");

                // Calculate the MD5 hash of the result
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(resultString + "H0mETyc00n!"));
                    string hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                    // Trim the hash to a specific length
                    hash = hash.Substring(0, 14);

                    // Append the trimmed hash to the result
                    resultString += hash;

                    sessionid = SSFWProcessor.ssfwgenerateguid(hash, resultString);

                    md5.Dispose();
                }
            }

            return Encoding.UTF8.GetBytes($"<response><Thing>{userid};{sessionid}</Thing></response>");
        }

        public static async Task<byte[]> RequestTownInstance(string UserID, string DisplayName, string phpSessionId)
        {
            string hash = "";

            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(UserID + "G0TOH00000!!!!m3TycoonN0?w*" + DisplayName));
                hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
                md5.Dispose();
            }

            return Encoding.UTF8.GetBytes($"<Response><InstanceID>{generatetycconguid(hash, UserID + hash + phpSessionId)}</InstanceID></Response>");
        }

        public static async Task<byte[]> QueryServerGlobals(string UserID)
        {
            return Encoding.UTF8.GetBytes("<Response><GlobalHard>1</GlobalHard><GlobalWrinkles>1</GlobalWrinkles></Response>");
        }

        public static async Task<byte[]> QueryPrices()
        {
            return Misc.Combinebytearay(Misc.Combinebytearay(Encoding.UTF8.GetBytes("<Response>"), await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/Prices.xml", HTTPPrivateKey.HTTPPrivatekey)), Encoding.UTF8.GetBytes("</Response>"));
        }

        public static async Task<byte[]> QueryBoosters()
        {
            return Misc.Combinebytearay(Misc.Combinebytearay(Encoding.UTF8.GetBytes("<Response>"), await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/Boosters.xml", HTTPPrivateKey.HTTPPrivatekey)), Encoding.UTF8.GetBytes("</Response>"));
        }

        public static async Task<byte[]> QueryHoldbacks()
        {
            return Misc.Combinebytearay(Misc.Combinebytearay(Encoding.UTF8.GetBytes("<Response>"), await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/Holdbacks.xml", HTTPPrivateKey.HTTPPrivatekey)), Encoding.UTF8.GetBytes("</Response>"));
        }

        public static async Task<byte[]> QueryRewards(string UserID)
        {
            byte[] returnvalue = Encoding.UTF8.GetBytes("");

            if (File.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_Rewards.xml"))
                returnvalue = await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_Rewards.xml", HTTPPrivateKey.HTTPPrivatekey);

            return Misc.Combinebytearay(Misc.Combinebytearay(Encoding.UTF8.GetBytes("<Response>"), returnvalue), Encoding.UTF8.GetBytes("</Response>"));
        }

        public static async Task<byte[]> QueryGifts(string UserID)
        {
            byte[] returnvalue;

            if (File.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_Gifts.xml"))
                returnvalue = await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_Gifts.xml", HTTPPrivateKey.HTTPPrivatekey);
            else
            {
                await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_Gifts.xml", HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes("<Gift>111111</Gift>"));
                returnvalue = Encoding.UTF8.GetBytes("<Gift>111111</Gift>");
            }

            return Misc.Combinebytearay(Misc.Combinebytearay(Encoding.UTF8.GetBytes("<Response>"), returnvalue), Encoding.UTF8.GetBytes("</Response>"));
        }

        public static async Task<byte[]> RequestTown(string UserID, string InstanceID) // Not work very fine
        {
            if (File.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_{generatecityguid(InstanceID, UserID)}.xml"))
                return Misc.Combinebytearay(Misc.Combinebytearay(Encoding.UTF8.GetBytes("<Response>"), await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_{generatecityguid(InstanceID, UserID)}.xml", HTTPPrivateKey.HTTPPrivatekey)), Encoding.UTF8.GetBytes("</Response>"));
            else
            {
                StringBuilder gridBuilder = new StringBuilder();

                for (int i = 1; i <= 256; i++)
                {
                    gridBuilder.Append($"<{i}.000000><TimeBuilt></TimeBuilt><Orientation></Orientation><Index>{i}.000000</Index><Type></Type></{i}.000000>");
                }

                byte[] xml = Encoding.UTF8.GetBytes($"<UserID>{UserID}</UserID><DisplayName></DisplayName><TownID>{generatecityguid(InstanceID, UserID)}</TownID><InstanceID>{InstanceID}</InstanceID><LastVisited>0</LastVisited><Grid>{gridBuilder}</Grid>");

                await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_{generatecityguid(InstanceID, UserID)}.xml", HTTPPrivateKey.HTTPPrivatekey, xml);

                return Misc.Combinebytearay(Misc.Combinebytearay(Encoding.UTF8.GetBytes("<Response>"), xml), Encoding.UTF8.GetBytes("</Response>"));
            }
        }

        public static async Task<byte[]> RequestUser(string UserID)
        {
            byte[] returnvalue;

            if (File.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}.xml"))
                returnvalue = await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}.xml", HTTPPrivateKey.HTTPPrivatekey);
            else
            {
                returnvalue = await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/DefaultProfile.xml", HTTPPrivateKey.HTTPPrivatekey);
                await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}.xml", HTTPPrivateKey.HTTPPrivatekey, returnvalue);
            }

            return Misc.Combinebytearay(Misc.Combinebytearay(Encoding.UTF8.GetBytes("<Response>"), returnvalue), Encoding.UTF8.GetBytes("</Response>"));
        }

        public static async Task<byte[]> UpdateUser(string UserID, MultipartFormDataParser data) // Not work
        {
            string GoldCoins = data.GetParameterValue("GoldCoins");

            string Workers = data.GetParameterValue("Workers");

            string TotalCollected = data.GetParameterValue("TotalCollected");

            string Wallet = data.GetParameterValue("Wallet");

            string MusicVolume = "1";

            JObject jsonObject = JObject.Parse(data.GetParameterValue("Options"));
            var fieldValues = new Dictionary<string, object>();

            foreach (var property in jsonObject.Properties())
            {
                fieldValues[property.Name] = property.Value.ToObject<object>();
            }

            foreach (var fieldValue in fieldValues)
            {
                Console.WriteLine($"{fieldValue.Key}: {fieldValue.Value}");

                if (fieldValue.Key == "MusicVolume")
                    MusicVolume = fieldValue.Value.ToString();
            }

            string xmlprofile = "";

            if (File.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}.xml"))
                xmlprofile = Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}.xml", HTTPPrivateKey.HTTPPrivatekey));
            else
                xmlprofile = Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/DefaultProfile.xml", HTTPPrivateKey.HTTPPrivatekey));

            // Regular expression pattern to match the GoldCoins tags and capture the value between them
            string GoldCoinspattern = @"<GoldCoins>(.*?)</GoldCoins>";

            // Use Regex.Replace to modify the value between the tags
            string firstpassXml = Regex.Replace(xmlprofile, GoldCoinspattern, $"<GoldCoins>{GoldCoins}</GoldCoins>");

            // Regular expression pattern to match the Workers tags and capture the value between them
            string Workerspattern = @"<Workers>(.*?)</Workers>";

            // Use Regex.Replace to modify the value between the tags
            string secondpassXml = Regex.Replace(firstpassXml, Workerspattern, $"<Workers>{Workers}</Workers>");

            // Regular expression pattern to match the TotalCollected tags and capture the value between them
            string TotalCollectedpattern = @"<TotalCollected>(.*?)</TotalCollected>";

            // Use Regex.Replace to modify the value between the tags
            string thirdpassXml = Regex.Replace(secondpassXml, TotalCollectedpattern, $"<TotalCollected>{TotalCollected}</TotalCollected>");

            // Regular expression pattern to match the TotalCollected tags and capture the value between them
            string Walletpattern = @"<Wallet>(.*?)</Wallet>";

            // Use Regex.Replace to modify the value between the tags
            string fourthpassXml = Regex.Replace(thirdpassXml, Walletpattern, $"<Wallet>{Wallet}</Wallet>");

            // Regular expression pattern to match the TotalCollected tags and capture the value between them
            string MusicVolumepattern = @"<MusicVolume>(.*?)</MusicVolume>";

            // Use Regex.Replace to modify the value between the tags
            string modifiedXml = Regex.Replace(thirdpassXml, MusicVolumepattern, $"<MusicVolume>{MusicVolume}</MusicVolume>");

            await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}.xml", HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(modifiedXml));

            return Misc.Combinebytearay(Misc.Combinebytearay(Encoding.UTF8.GetBytes("<Response>"), Encoding.UTF8.GetBytes(modifiedXml)), Encoding.UTF8.GetBytes("</Response>"));
        }

        public static async Task<byte[]> CreateBuilding(string UserID, MultipartFormDataParser data) // Not work very fine
        {
            string Orientation = data.GetParameterValue("Orientation");

            string Type = data.GetParameterValue("Type");

            string TownID = data.GetParameterValue("TownID");

            string Index = data.GetParameterValue("Index");

            string xmlprofile = "";

            if (File.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_{TownID}.xml"))
                xmlprofile = Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_{TownID}.xml", HTTPPrivateKey.HTTPPrivatekey));

            string pattern = $"<{Index:F6}>(<TimeBuilt>).*?(</TimeBuilt>)(<Orientation>).*?(</Orientation>)(<Index>){Index:F6}(</Index>)(<Type>).*?(</Type>)</{Index:F6}>";
            string replacement = $"<{Index:F6}><TimeBuilt>{DateTime.Now}</TimeBuilt><Orientation>{Orientation}</Orientation><Index>{Index}</Index><Type>{Type}</Type></{Index:F6}>";

            string modifiedXmlData = Regex.Replace(xmlprofile, pattern, replacement);

            await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_{TownID}.xml", HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(modifiedXmlData));

            return Encoding.UTF8.GetBytes($"<Response><TimeBuilt>{DateTime.Now}</TimeBuilt><Orientation>{Orientation}</Orientation><Index>{Index}</Index><Type>{Type}</Type></Response>");
        }

        public static async Task<byte[]> RemoveBuilding(string UserID, MultipartFormDataParser data) // Not work very fine
        {
            string TownID = data.GetParameterValue("TownID");

            string BuildingIndex = data.GetParameterValue("BuildingIndex");

            string xmlprofile = "";

            if (File.Exists(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_{TownID}.xml"))
                xmlprofile = Encoding.UTF8.GetString(await FileHelper.CryptoReadAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_{TownID}.xml", HTTPPrivateKey.HTTPPrivatekey));

            string pattern = $"<{BuildingIndex:F6}>(<TimeBuilt>).*?(</TimeBuilt>)(<Orientation>).*?(</Orientation>)(<Index>){BuildingIndex:F6}(</Index>)(<Type>).*?(</Type>)</{BuildingIndex:F6}>";
            string replacement = $"<{BuildingIndex:F6}><TimeBuilt></TimeBuilt><Orientation></Orientation><Index>{BuildingIndex}</Index><Type></Type></{BuildingIndex:F6}>";

            string modifiedXmlData = Regex.Replace(xmlprofile, pattern, replacement);

            await FileHelper.CryptoWriteAsync(Directory.GetCurrentDirectory() + $"{ServerConfiguration.HTTPStaticFolder}HOME_TYCOON/User_Data/{UserID}_{TownID}.xml", HTTPPrivateKey.HTTPPrivatekey, Encoding.UTF8.GetBytes(modifiedXmlData));

            return Encoding.UTF8.GetBytes($"<Response></Response>");
        }

        public static string generatetycconguid(string input1, string input2)
        {
            string md5hash = "";
            string sha512hash = "";

            using (MD5 md5 = MD5.Create())
            {
                string salt = "**H0mEIsG3reATW1tHH0meTYC000N!!!!!!!!!!!!!!";

                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input1 + salt));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Dispose();
            }

            using (SHA512 sha512 = SHA512.Create())
            {
                string salt = "C0MeW1tHH0meTYC111NBaCKHOm3*!*!*!*!*!*!*!*!";

                byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(salt + input2));
                sha512hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                sha512.Dispose();
            }

            string result = (md5hash.Substring(1, 8) + sha512hash.Substring(2, 4) + md5hash.Substring(10, 4) + sha512hash.Substring(16, 4) + sha512hash.Substring(19, 16)).ToLower();

            // Use a dictionary to map characters 'a' to 'f' to specific numbers
            Dictionary<char, char> charMapping = new Dictionary<char, char>
            {
                { 'a', '6' },
                { 'b', '1' },
                { 'c', '2' },
                { 'd', '3' },
                { 'e', '4' },
                { 'f', '5' }
            };

            // Replace characters in the result based on the mapping
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in result)
            {
                if (charMapping.TryGetValue(c, out char mappedChar))
                {
                    stringBuilder.Append(mappedChar);
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            result = stringBuilder.ToString();

            return result;
        }

        public static string generatecityguid(string input1, string input2)
        {
            string md5hash = "";
            string sha512hash = "";

            using (MD5 md5 = MD5.Create())
            {
                string salt = "**MyC1TY1sTH3be5T!!!!!!!!!!!!!!";

                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input1 + salt));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Dispose();
            }

            using (SHA512 sha512 = SHA512.Create())
            {
                string salt = "1L0veH0mmmmeT1c000000nnnnn!!!!!";

                byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(salt + input2));
                sha512hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                sha512.Dispose();
            }

            string result = (md5hash.Substring(1, 8) + sha512hash.Substring(2, 4) + md5hash.Substring(10, 4) + sha512hash.Substring(16, 4) + sha512hash.Substring(19, 16)).ToLower();

            // Use a dictionary to map characters 'a' to 'f' to specific numbers
            Dictionary<char, char> charMapping = new Dictionary<char, char>
            {
                { 'a', '3' },
                { 'b', '5' },
                { 'c', '6' },
                { 'd', '8' },
                { 'e', '4' },
                { 'f', '9' }
            };

            // Replace characters in the result based on the mapping
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in result)
            {
                if (charMapping.TryGetValue(c, out char mappedChar))
                {
                    stringBuilder.Append(mappedChar);
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            result = stringBuilder.ToString();

            return result;
        }
    }
}
