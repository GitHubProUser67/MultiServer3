using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WebAPIService.HELLFIRE.Helpers
{
    public class TownInstance
    {
        public static string? RequestTownInstance(string UserID, string DisplayName, string? PHPSessionId)
        {
            string hash = string.Empty;

            using (MD5 md5 = MD5.Create())
            {
                hash = BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(UserID + "G0TOH00000!!!!m3TycoonN0?w*" + DisplayName))).Replace("-", string.Empty);
                md5.Clear();
            }

            if (string.IsNullOrEmpty(PHPSessionId))
                return $"<Response><InstanceID>{GenerateTycoonguid(hash, UserID + hash)}</InstanceID></Response>";
            else
                return $"<Response><InstanceID>{GenerateTycoonguid(hash, UserID + hash + PHPSessionId)}</InstanceID></Response>";
        }

        public static string? RequestTown(string UserID, string InstanceID, string DisplayName, string WorkPath)
        {
            if (File.Exists($"{WorkPath}/TYCOON/User_Data/{UserID}_{GenerateCityguid(InstanceID, UserID)}.xml"))
                return $"<Response>{File.ReadAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{GenerateCityguid(InstanceID, UserID)}.xml")}</Response>";
            else
            {
                StringBuilder? gridBuilder = new();

                for (int i = 1; i <= 256; i++)
                {
                    gridBuilder.Append($"<{i}.000000><TimeBuilt></TimeBuilt><Orientation></Orientation><Index>{i}.000000</Index><Type></Type></{i}.000000>");
                }

                string xml = $"<UserID>{UserID}</UserID><DisplayName>{DisplayName}</DisplayName>" +
                    $"<TownID>{GenerateCityguid(InstanceID, UserID)}</TownID>" +
                    $"<InstanceID>{InstanceID}</InstanceID><LastVisited>0</LastVisited><Grid>{gridBuilder.ToString()}</Grid>";

                gridBuilder = null;

                File.WriteAllText($"{WorkPath}/TYCOON/User_Data/{UserID}_{GenerateCityguid(InstanceID, UserID)}.xml", xml);
                return $"<Response>{xml}</Response>";
            }
        }

        private static string GenerateTycoonguid(string input1, string input2)
        {
            string md5hash = string.Empty;
            string sha512hash = string.Empty;

            using (MD5 md5 = MD5.Create())
            {
                string salt = "**H0mEIsG3reATW1tHH0meTYC000N!!!!!!!!!!!!!!";

                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input1 + salt));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Clear();
            }

            using (SHA512 sha512 = SHA512.Create())
            {
                string salt = "C0MeW1tHH0meTYC111NBaCKHOm3*!*!*!*!*!*!*!*!";

                byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(salt + input2));
                sha512hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                sha512.Clear();
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
            StringBuilder? stringBuilder = new();

            foreach (char c in result)
            {
                if (charMapping.TryGetValue(c, out char mappedChar))
                    stringBuilder.Append(mappedChar);
                else
                    stringBuilder.Append(c);
            }

            result = stringBuilder.ToString();

            stringBuilder = null;

            return result;
        }

        public static string GenerateCityguid(string input1, string input2)
        {
            string md5hash = "";
            string sha512hash = "";

            using (MD5 md5 = MD5.Create())
            {
                string salt = "**MyC1TY1sTH3be5T!!!!!!!!!!!!!!";

                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input1 + salt));
                md5hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                md5.Clear();
            }

            using (SHA512 sha512 = SHA512.Create())
            {
                string salt = "1L0veH0mmmmeT1c000000nnnnn!!!!!";

                byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(salt + input2));
                sha512hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

                sha512.Clear();
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
            StringBuilder? stringBuilder = new();

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

            stringBuilder = null;

            return result;
        }
    }
}
