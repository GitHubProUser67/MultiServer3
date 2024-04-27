using System.IO;
using System.Text;

namespace ComponentAce.Compression.Libs.zlib
{
    public class SupportClass
    {
        public static long Identity(long literal)
        {
            return literal;
        }

        public static ulong Identity(ulong literal)
        {
            return literal;
        }

        public static float Identity(float literal)
        {
            return literal;
        }

        public static double Identity(double literal)
        {
            return literal;
        }

        public static int URShift(int number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            return (number >> bits) + (2 << ~bits);
        }

        public static int URShift(int number, long bits)
        {
            return URShift(number, (int)bits);
        }

        public static long URShift(long number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            return (number >> bits) + (2L << ~bits);
        }

        public static long URShift(long number, long bits)
        {
            return URShift(number, (int)bits);
        }

        public static int ReadInput(Stream sourceStream, byte[] target, int start, int count)
        {
            if (target.Length == 0)
                return 0;
            byte[] array = new byte[target.Length];
            int num = sourceStream.Read(array, start, count);
            if (num == 0)
                return -1;
            for (int i = start; i < start + num; i++)
            {
                target[i] = array[i];
            }
            return num;
        }

        public static int ReadInput(TextReader sourceTextReader, byte[] target, int start, int count)
        {
            if (target.Length == 0)
                return 0;
            char[] array = new char[target.Length];
            int num = sourceTextReader.Read(array, start, count);
            if (num == 0)
                return -1;
            for (int i = start; i < start + num; i++)
            {
                target[i] = (byte)array[i];
            }
            return num;
        }

        public static byte[] ToByteArray(string sourceString)
        {
            return Encoding.UTF8.GetBytes(sourceString);
        }

        public static char[] ToCharArray(byte[] byteArray)
        {
            return Encoding.UTF8.GetChars(byteArray);
        }
    }
}