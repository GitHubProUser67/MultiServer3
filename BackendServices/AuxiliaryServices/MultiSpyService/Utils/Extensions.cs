using System;
using System.Globalization;
using System.Text;

namespace MultiSpyService.Utils
{
    public static class Extensions
    {
        public const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static readonly object _lock = new object();

        public static string GetString(this Random rand, int length)
        {
            char[] array = new char[length];
            lock (_lock)
            {
                for (int i = 0; i < length; i++)
                {
                    array[i] = chars[rand.Next(62)];
                }
            }
            return new string(array);
        }

        public static string GetString(this Random rand, int length, string chars)
        {
            char[] array = new char[length];
            lock (_lock)
            {
                for (int i = 0; i < length; i++)
                {
                    array[i] = chars[rand.Next(chars.Length)];
                }
            }
            return new string(array);
        }

        public static string ToMD5(this string s)
        {
            byte[] array = CastleLibrary.Utils.Hash.NetHasher.ComputeMD5(Encoding.ASCII.GetBytes(s));
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("x2", CultureInfo.InvariantCulture));
            }
            return stringBuilder.ToString();
        }
    }
}
