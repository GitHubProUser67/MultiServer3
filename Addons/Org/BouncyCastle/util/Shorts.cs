using System;
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Buffers.Binary;
#endif

namespace MultiServer.Addons.Org.BouncyCastle.Utilities
{
    public static class Shorts
    {
        public const int NumBits = 16;
        public const int NumBytes = 2;

        public static short ReverseBytes(short i)
        {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return BinaryPrimitives.ReverseEndianness(i);
#else
            return RotateLeft(i, 8);
#endif
        }

        public static ushort ReverseBytes(ushort i)
        {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return BinaryPrimitives.ReverseEndianness(i);
#else
            return RotateLeft(i, 8);
#endif
        }

        public static short RotateLeft(short i, int distance)
        {
            return (short)RotateLeft((ushort)i, distance);
        }

        public static ushort RotateLeft(ushort i, int distance)
        {
            return (ushort)((i << distance) | (i >> (16 - distance)));
        }

        public static short RotateRight(short i, int distance)
        {
            return (short)RotateRight((ushort)i, distance);
        }

        public static ushort RotateRight(ushort i, int distance)
        {
            return (ushort)((i >> distance) | (i << (16 - distance)));
        }
    }
}
