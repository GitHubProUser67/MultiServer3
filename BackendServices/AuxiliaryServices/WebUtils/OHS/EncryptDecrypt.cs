using CustomLogger;
using System.Text;

namespace WebUtils.OHS
{
    public class EncryptDecrypt
    {
        private static int Wrapped(int index, int max)
        {
            if (index > max)
                return 1 + index % (max + 1);
            else
                return index;
        }

        public static string Encrypt(string? str, int offset, int game)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            if (offset > 95 * 95)
            {
                LoggerAccessor.LogError("[OHS - Encrypt] - Offset is too large.");
                return string.Empty;
            }

            var chars = new StringBuilder();

            int offsethi = (offset - 1) / 95;
            int offsetlo = (offset - 1) % 95;
            chars.Append((char)(offsethi + 32));
            chars.Append((char)(offsetlo + 32));

            for (int i = 0; i < str.Length; i++)
            {
                int srcbyte = str[i] - 32;
                if (srcbyte < 0 || srcbyte > 95)
                {
                    LoggerAccessor.LogError("[OHS - Encrypt] - Invalid character in input string");
                    return string.Empty;
                }

                int cipherbyte = 0;

                if (game == 1)
                    cipherbyte = StaticKeys.version1cipher[Wrapped(i + offset, StaticKeys.version1cipher.Length)];
                else if (game == 2)
                    cipherbyte = StaticKeys.version2cipher[Wrapped(i + offset, StaticKeys.version2cipher.Length)];
                else
                    return string.Empty;

                int encbyte = (srcbyte + cipherbyte) % 95 + 32;

                chars.Append((char)encbyte);
            }

            return chars.ToString();
        }

        public static string Decrypt(string str, int game)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            var chars = new StringBuilder();

            int offsethi = str[0] - 32;
            int offsetlo = str[1] - 32;
            int offset = offsethi * 95 + offsetlo + 1;

            for (int i = 2; i < str.Length; i++) // Corrected loop start index to 2
            {
                int srcbyte = str[i] - 32;
                if (srcbyte < 0 || srcbyte > 95)
                {
                    LoggerAccessor.LogError("[OHS - Decrypt] - Invalid character in input string");
                    return string.Empty;
                }

                int cipherbyte = 0;

                if (game == 1)
                    cipherbyte = StaticKeys.version1cipher[Wrapped(i - 2 + offset, StaticKeys.version1cipher.Length)];
                else if (game == 2)
                    cipherbyte = StaticKeys.version2cipher[Wrapped(i - 2 + offset, StaticKeys.version2cipher.Length)];
                else
                    return string.Empty;

                int decbyte = (srcbyte - cipherbyte + 95) % 95 + 32;

                chars.Append((char)decbyte);
            }

            return chars.ToString();
        }

        private static ushort Hash16(string str, ushort histart, ushort lostart)
        {
            ushort hi = histart, lo = lostart;
            for (int i = 0; i < str.Length; i++)
            {
                byte b = (byte)str[i];
                lo = (ushort)((b + lo) % 255);
                hi = (ushort)((255 - b + hi) % 255);

                ushort lolo = (ushort)(lo % 16);
                ushort lohi = (ushort)(lo / 16);
                ushort hilo = (ushort)(hi % 16);
                ushort hihi = (ushort)(hi / 16);

                lo = (ushort)(hilo * 16 + lolo);
                hi = (ushort)(hihi * 16 + lohi);

                ushort temp = lo;
                lo = hi;
                hi = temp;
            }

            return (ushort)(hi * 255 + lo);
        }

        private static (ushort, ushort) Hash32(string str)
        {
            ushort hi = Hash16(str, 170, 204);
            ushort lo = Hash16(str, 11, 252);
            return (hi, lo);
        }

        public static string Hash32Str(string str)
        {
            (ushort hi, ushort lo) = Hash32(str);
            return string.Format("{0:X4}{1:X4}", hi, lo);
        }

        public static string Escape(string str)
        {
            // Replace '%' with '&m' and '&' with '&a'
            // I tried escaping '&' as '&&' but that leads to pain. <- Original LUA comment ^^.
            return str.Replace("%", "&m").Replace("&", "&a");
        }

        public static string UnEscape(string str)
        {
            // Replace '&a' with '&' and '&m' with '%'
            return str.Replace("&a", "&").Replace("&m", "%");
        }
    }
}
