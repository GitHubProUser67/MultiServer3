using System;
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Buffers.Binary;
#endif

namespace EndianTools
{
    public class EndianUtils
    {
        /// <summary>
        /// Reverse the endianess of a given byte array by 4 bytes chunck.
        /// <para>change l'endianess d'un tableau de bytes par blocs de 4.</para>
        /// </summary>
        /// <param name="dataIn">The byte array to endian-swap.</param>
        /// <returns>A byte array.</returns>
        public static byte[] EndianSwap(byte[] dataIn)
        {
            int inputLength = dataIn.Length;
            byte[] reversedArray = new byte[inputLength];
            Array.Copy(dataIn, reversedArray, inputLength);
            int numofBytes;
            for (int i = 0; i < inputLength; i += numofBytes)
            {
                numofBytes = 4;
                int remainingBytes = inputLength - i;
                if (remainingBytes < 4)
                    numofBytes = remainingBytes;
                Array.Reverse(reversedArray, i, numofBytes);
            }
            return reversedArray;
        }

        /// <summary>
        /// Reverse the endianess of a given byte array.
        /// <para>change l'endianess d'un tableau de bytes.</para>
        /// </summary>
        /// <param name="dataIn">The byte array to endian-swap.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ReverseArray(byte[] dataIn)
        {
            // Clone the input array to avoid modifying the original array
            byte[] reversedArray = (byte[])dataIn.Clone();
            Array.Reverse(reversedArray);
            return reversedArray;
        }

        /// <summary>
        /// Reverse the endianess of a given int.
        /// <para>change l'endianess d'un int.</para>
        /// </summary>
        /// <param name="dataIn">The int to endian-swap.</param>
        /// <returns>A int.</returns>
        public static int ReverseInt(int dataIn)
        {
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return BinaryPrimitives.ReverseEndianness(dataIn);
#else
            byte[] bytes = BitConverter.GetBytes(dataIn);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
#endif
        }

        /// <summary>
        /// Reverse the endianess of a given uint.
        /// <para>change l'endianess d'un uint.</para>
        /// </summary>
        /// <param name="dataIn">The uint to endian-swap.</param>
        /// <returns>A uint.</returns>
        public static uint ReverseUint(uint dataIn)
        {
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return BinaryPrimitives.ReverseEndianness(dataIn);
#else
            return ((dataIn & 0x000000ff) << 24) +
                   ((dataIn & 0x0000ff00) << 8) +
                   ((dataIn & 0x00ff0000) >> 8) +
                   ((dataIn & 0xff000000) >> 24);
#endif
        }

        /// <summary>
        /// Reverse the endianess of a given long.
        /// <para>change l'endianess d'un long.</para>
        /// </summary>
        /// <param name="dataIn">The long to endian-swap.</param>
        /// <returns>A long.</returns>
        public static long ReverseLong(long dataIn)
        {
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return BinaryPrimitives.ReverseEndianness(dataIn);
#else
            byte[] bytes = BitConverter.GetBytes(dataIn);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
#endif
        }

        /// <summary>
        /// Reverse the endianess of a given ulong.
        /// <para>change l'endianess d'un ulong.</para>
        /// </summary>
        /// <param name="dataIn">The ulong to endian-swap.</param>
        /// <returns>A ulong.</returns>
        public static ulong ReverseUlong(ulong dataIn)
        {
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return BinaryPrimitives.ReverseEndianness(dataIn);
#else
            return (0x00000000000000FF) & (dataIn >> 56)
                 | (0x000000000000FF00) & (dataIn >> 40)
                 | (0x0000000000FF0000) & (dataIn >> 24)
                 | (0x00000000FF000000) & (dataIn >> 8)
                 | (0x000000FF00000000) & (dataIn << 8)
                 | (0x0000FF0000000000) & (dataIn << 24)
                 | (0x00FF000000000000) & (dataIn << 40)
                 | (0xFF00000000000000) & (dataIn << 56);
#endif
        }

        /// <summary>
        /// Reverse the endianess of a given double.
        /// <para>change l'endianess d'un double.</para>
        /// </summary>
        /// <param name="dataIn">The double to endian-swap.</param>
        /// <returns>A double.</returns>
        public static double ReverseDouble(double dataIn)
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
        public static float ReverseFloat(float dataIn)
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
        public static short ReverseShort(short dataIn)
        {
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return BinaryPrimitives.ReverseEndianness(dataIn);
#else
            byte[] bytes = BitConverter.GetBytes(dataIn);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
#endif
        }

        /// <summary>
        /// Reverse the endianess of a given ushort.
        /// <para>change l'endianess d'un ushort.</para>
        /// </summary>
        /// <param name="dataIn">The ushort to endian-swap.</param>
        /// <returns>A ushort.</returns>
        public static ushort ReverseUshort(ushort dataIn)
        {
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return BinaryPrimitives.ReverseEndianness(dataIn);
#else
            // Use bitwise operations to swap the bytes
            return (ushort)((ushort)((dataIn & 0xff) << 8) | ((dataIn >> 8) & 0xff));
#endif
        }
    }
}
