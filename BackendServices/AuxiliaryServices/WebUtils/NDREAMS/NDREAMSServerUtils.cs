using System.Security.Cryptography;
using System.Text;

namespace WebUtils.NDREAMS
{
    public static class NDREAMSServerUtils
    {
        public static string Server_GetSignature(string url, string playername, string data, DateTime dObj)
        {
            return BitConverter.ToString(SHA1.HashData(Encoding.UTF8.GetBytes("nDreams" + url + playername + dObj.Year + dObj.Month + dObj.Day + data + "Signature"))).Replace("-", string.Empty).ToLower();
        }

        public static string Server_KeyToHash(string key, DateTime dObj, string level)
        {
            Dictionary<char, char> map = new()
            {
                { '0', '#' },
                { '1', 'a' },
                { '2', '@' },
                { '3', 'C' },
                { '4', 'T' },
                { '5', 'U' },
                { '6', 'e' },
                { '7', 'X' },
                { '8', 'k' },
                { '9', 'z' },
                { 'A', '1' },
                { 'B', 'c' },
                { 'C', '*' },
                { 'D', 'v' },
                { 'E', 'D' },
                { 'F', 'A' },
                { 'G', 'g' },
                { 'H', 'U' },
                { 'I', '8' },
                { 'J', '2' }
            };

            StringBuilder str = new();
            for (int i = 7; i >= 0; i--)
            {
                char currentChar = key[i];
                char mappedChar;
                if (map.TryGetValue(currentChar, out char value))
                    mappedChar = value;
                else
                    mappedChar = map[Convert.ToChar(Convert.ToInt32(currentChar))];

                str.Append(mappedChar);
            }

            return BitConverter.ToString(SHA1.HashData(Encoding.UTF8.GetBytes("keyString" + level + dObj.Year + dObj.Month + dObj.Day + str.ToString() + level))).Replace("-", string.Empty).ToLower();
        }
    }
}
