using System;
using System.Numerics;

namespace HomeTools.PS3_Creator
{
    public class ConversionUtils
    {
        public static BigInteger be64(byte[] buffer, int initOffset)
        {
            BigInteger result = BigInteger.Zero;
            for (int i = initOffset; i < initOffset + 8; i++)
            {
                result = result * (new BigInteger(256));
                result = result + (new BigInteger(buffer[i] & 0xFF));
            }
            return result;
        }

        public static long be32(byte[] buffer, int initOffset)
        {
            long result = 0;
            for (int i = initOffset; i < initOffset + 4; i++)
            {
                result = result * 256 + (buffer[i] & 0xFF);
            }

            return result;
        }

        public static int be16(byte[] buffer, int initOffset)
        {
            int result = 0;
            for (int i = initOffset; i < initOffset + 2; i++)
            {
                result = result * 256 + (buffer[i] & 0xFF);
            }
            return result;
        }

        static byte[] HEX_CHAR_TABLE = {
        (byte) '0', (byte) '1', (byte) '2', (byte) '3',
        (byte) '4', (byte) '5', (byte) '6', (byte) '7',
        (byte) '8', (byte) '9', (byte) 'a', (byte) 'b',
        (byte) 'c', (byte) 'd', (byte) 'e', (byte) 'f'
    };

        public static void arraycopy(byte[] src, int srcPos, byte[] dest, long destPos, int length)
        {
            for (int i = 0; i < length; i++)
            {
                dest[destPos + i] = src[srcPos + i];
            }
        }

        public static String getHexString(byte[] raw)
        {
            byte[] hex = new byte[2 * raw.Length];
            int index = 0;

            foreach (byte b in raw)
            {
                uint v = (uint)b & 0xFF;
                hex[index++] = HEX_CHAR_TABLE[v >> 4];
                hex[index++] = HEX_CHAR_TABLE[v & 0xF];
            }
            return new String(bytesToChar(hex));
        }

        public static char[] bytesToChar(byte[] b)
        {
            char[] c = new char[b.Length];
            for (int i = 0; i < b.Length; i++)
                c[i] = (char)b[i];
            return c;
        }

        public static byte[] reverseByteWithSizeFIX(byte[] b)
        {

            byte[] b2;
            if(b[b.Length -1] == 0x00)
                b2 = new byte[b.Length - 1];
            else
                b2 = new byte[b.Length];
            for (int i = 0; i < b2.Length; i++)
                b2[b2.Length - 1 - i] = b[i];
            return b2;
        }

        public static byte[] charsToByte(char[] b)
        {
            byte[] c = new byte[b.Length];
            for (int i = 0; i < b.Length; i++)
                c[i] = (byte)b[i];
            return c;
        }

        public static byte[] getByteArray(String hexString)
        {
            return decodeHex(hexString.ToCharArray());
        }

        public static byte[] decodeHex(char[] data)
        {
            int len = data.Length;
            if ((len & 0x01) != 0)
            {
                throw new Exception("Odd number of characters.");
            }

            byte[] o = new byte[len >> 1];

            // two characters form the hex value.
            for (int i = 0, j = 0; j < len; i++)
            {
                int f = toDigit(data[j], j) << 4;
                j++;
                f = f | toDigit(data[j], j);
                j++;
                o[i] = (byte)(f & 0xFF);
            }

            return o;
        }

        static int GetIntegerValue(char c, int radix)
        {
            int val = -1;
            if (char.IsDigit(c))
                val = (int)(c - '0');
            else if (char.IsLower(c))
                val = (int)(c - 'a') + 10;
            else if (char.IsUpper(c))
                val = (int)(c - 'A') + 10;
            if (val >= radix)
                val = -1;
            return val;
        }


        protected static int toDigit(char ch, int index)
        {
            int digit = GetIntegerValue(ch, 16);
            if (digit == -1)
            {
                throw new Exception("Illegal hexadecimal character " + ch + " at index " + index);
            }
            return digit;
        }
    }
}
