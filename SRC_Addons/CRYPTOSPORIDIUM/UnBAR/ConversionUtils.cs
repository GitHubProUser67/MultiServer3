using System.Numerics;

namespace PSMultiServer.Addons.CRYPTOSPORIDIUM.UnBAR
{
    internal class ConversionUtils
    {
        private static byte[] HEX_CHAR_TABLE = new byte[16]
        {
      (byte) 48,
      (byte) 49,
      (byte) 50,
      (byte) 51,
      (byte) 52,
      (byte) 53,
      (byte) 54,
      (byte) 55,
      (byte) 56,
      (byte) 57,
      (byte) 97,
      (byte) 98,
      (byte) 99,
      (byte) 100,
      (byte) 101,
      (byte) 102
        };

        public static BigInteger be64(byte[] buffer, int initOffset)
        {
            BigInteger bigInteger = BigInteger.Zero;
            for (int index = initOffset; index < initOffset + 8; ++index)
                bigInteger = bigInteger * new BigInteger(256) + new BigInteger((int)buffer[index] & (int)byte.MaxValue);
            return bigInteger;
        }

        public static long be32(byte[] buffer, int initOffset)
        {
            long num = 0;
            for (int index = initOffset; index < initOffset + 4; ++index)
                num = num * 256L + (long)((int)buffer[index] & (int)byte.MaxValue);
            return num;
        }

        public static int be16(byte[] buffer, int initOffset)
        {
            int num = 0;
            for (int index = initOffset; index < initOffset + 2; ++index)
                num = num * 256 + ((int)buffer[index] & (int)byte.MaxValue);
            return num;
        }

        public static void arraycopy(byte[] src, int srcPos, byte[] dest, long destPos, int length)
        {
            for (int index = 0; index < length; ++index)
                dest[destPos + (long)index] = src[srcPos + index];
        }

        public static string getHexString(byte[] raw)
        {
            byte[] b = new byte[2 * raw.Length];
            int num1 = 0;
            foreach (uint num2 in raw)
            {
                uint num3 = num2 & (uint)byte.MaxValue;
                byte[] numArray1 = b;
                int index1 = num1;
                int num4 = index1 + 1;
                int num5 = (int)ConversionUtils.HEX_CHAR_TABLE[(int)(num3 >> 4)];
                numArray1[index1] = (byte)num5;
                byte[] numArray2 = b;
                int index2 = num4;
                num1 = index2 + 1;
                int num6 = (int)ConversionUtils.HEX_CHAR_TABLE[(int)num3 & 15];
                numArray2[index2] = (byte)num6;
            }
            return new string(ConversionUtils.bytesToChar(b));
        }

        public static char[] bytesToChar(byte[] b)
        {
            char[] chArray = new char[b.Length];
            for (int index = 0; index < b.Length; ++index)
                chArray[index] = (char)b[index];
            return chArray;
        }

        public static byte[] reverseByteWithSizeFIX(byte[] b)
        {
            byte[] numArray = b[b.Length - 1] != (byte)0 ? new byte[b.Length] : new byte[b.Length - 1];
            for (int index = 0; index < numArray.Length; ++index)
                numArray[numArray.Length - 1 - index] = b[index];
            return numArray;
        }

        public static byte[] charsToByte(char[] b)
        {
            byte[] numArray = new byte[b.Length];
            for (int index = 0; index < b.Length; ++index)
                numArray[index] = (byte)b[index];
            return numArray;
        }

        public static byte[] getByteArray(string hexString) => ConversionUtils.decodeHex(hexString.ToCharArray());

        public static byte[] decodeHex(char[] data)
        {
            int length = data.Length;
            byte[] numArray = (length & 1) == 0 ? new byte[length >> 1] : throw new Exception("Odd number of characters.");
            int index1 = 0;
            int index2 = 0;
            while (index2 < length)
            {
                int num1 = ConversionUtils.toDigit(data[index2], index2) << 4;
                int index3 = index2 + 1;
                int num2 = num1 | ConversionUtils.toDigit(data[index3], index3);
                index2 = index3 + 1;
                numArray[index1] = (byte)(num2 & (int)byte.MaxValue);
                ++index1;
            }
            return numArray;
        }

        private static int GetIntegerValue(char c, int radix)
        {
            int integerValue = -1;
            if (char.IsDigit(c))
                integerValue = (int)c - 48;
            else if (char.IsLower(c))
                integerValue = (int)c - 97 + 10;
            else if (char.IsUpper(c))
                integerValue = (int)c - 65 + 10;
            if (integerValue >= radix)
                integerValue = -1;
            return integerValue;
        }

        protected static int toDigit(char ch, int index)
        {
            int integerValue = ConversionUtils.GetIntegerValue(ch, 16);
            return integerValue != -1 ? integerValue : throw new Exception("Illegal hexadecimal character " + ch.ToString() + " at index " + index.ToString());
        }
    }
}
