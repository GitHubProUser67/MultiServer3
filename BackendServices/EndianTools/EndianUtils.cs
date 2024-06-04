using System;

namespace EndianTools
{
    public class EndianUtils
    {
        /// <summary>
        /// Reverse the endianess of a given byte array.
        /// <para>change l'endianess d'un tableau de bytes.</para>
        /// </summary>
        /// <param name="dataIn">The byte array to endian-swap.</param>
        /// <returns>A byte array.</returns>
        public static byte[] EndianSwap(byte[] dataIn)
        {
            int num = dataIn.Length;
            byte[] array = new byte[num];
            Array.Copy(dataIn, array, dataIn.Length);
            int num2;
            for (int i = 0; i < dataIn.Length; i += num2)
            {
                num2 = 4;
                int num3 = dataIn.Length - i;
                if (num3 < 4)
                    num2 = num3;
                Array.Reverse(array, i, num2);
            }
            return array;
        }

        /// <summary>
        /// Reverse the endianess of a given int.
        /// <para>change l'endianess d'un int.</para>
        /// </summary>
        /// <param name="dataIn">The int to endian-swap.</param>
        /// <returns>A int.</returns>
        public static int EndianSwap(int dataIn)
        {
            byte[] bytes = BitConverter.GetBytes(dataIn);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Reverse the endianess of a given uint.
        /// <para>change l'endianess d'un uint.</para>
        /// </summary>
        /// <param name="dataIn">The uint to endian-swap.</param>
        /// <returns>A uint.</returns>
        public static uint EndianSwap(uint dataIn)
        {
            return ((dataIn & 0x000000ff) << 24) +
                   ((dataIn & 0x0000ff00) << 8) +
                   ((dataIn & 0x00ff0000) >> 8) +
                   ((dataIn & 0xff000000) >> 24);
        }

        /// <summary>
        /// Reverse the endianess of a given long.
        /// <para>change l'endianess d'un long.</para>
        /// </summary>
        /// <param name="dataIn">The long to endian-swap.</param>
        /// <returns>A long.</returns>
        public static long EndianSwap(long dataIn)
        {
            byte[] bytes = BitConverter.GetBytes(dataIn);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// Reverse the endianess of a given ulong.
        /// <para>change l'endianess d'un ulong.</para>
        /// </summary>
        /// <param name="dataIn">The ulong to endian-swap.</param>
        /// <returns>A ulong.</returns>
        public static ulong EndianSwap(ulong dataIn)
        {
            return (0x00000000000000FF) & (dataIn >> 56)
                 | (0x000000000000FF00) & (dataIn >> 40)
                 | (0x0000000000FF0000) & (dataIn >> 24)
                 | (0x00000000FF000000) & (dataIn >> 8)
                 | (0x000000FF00000000) & (dataIn << 8)
                 | (0x0000FF0000000000) & (dataIn << 24)
                 | (0x00FF000000000000) & (dataIn << 40)
                 | (0xFF00000000000000) & (dataIn << 56);
        }

        /// <summary>
        /// Reverse the endianess of a given double.
        /// <para>change l'endianess d'un double.</para>
        /// </summary>
        /// <param name="dataIn">The double to endian-swap.</param>
        /// <returns>A double.</returns>
        public static double EndianSwap(double dataIn)
        {
            byte[] bytes = BitConverter.GetBytes(dataIn);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// Reverse the endianess of a given float.
        /// <para>change l'endianess d'un float.</para>
        /// </summary>
        /// <param name="dataIn">The float to endian-swap.</param>
        /// <returns>A float.</returns>
        public static float EndianSwap(float dataIn)
        {
            byte[] bytes = BitConverter.GetBytes(dataIn);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Reverse the endianess of a given short.
        /// <para>change l'endianess d'un short.</para>
        /// </summary>
        /// <param name="dataIn">The short to endian-swap.</param>
        /// <returns>A short.</returns>
        public static short EndianSwap(short dataIn)
        {
            byte[] bytes = BitConverter.GetBytes(dataIn);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Reverse the endianess of a given ushort.
        /// <para>change l'endianess d'un ushort.</para>
        /// </summary>
        /// <param name="dataIn">The ushort to endian-swap.</param>
        /// <returns>A ushort.</returns>
        public static ushort EndianSwap(ushort dataIn)
        {
            // Use bitwise operations to swap the bytes
            return (ushort)((ushort)((dataIn & 0xff) << 8) | ((dataIn >> 8) & 0xff));
        }
    }
}
