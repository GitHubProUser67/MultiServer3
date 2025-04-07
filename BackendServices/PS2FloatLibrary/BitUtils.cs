﻿using System;
using System.Runtime.CompilerServices;
#if NETCOREAPP || NETSTANDARD1_0_OR_GREATER || NET40_OR_GREATER
using System.Threading.Tasks;
#endif
#if NETCOREAPP3_0_OR_GREATER
using System.Numerics;
#endif

namespace PS2FloatLibrary
{
    public static class BitUtils
    {
        private static readonly sbyte[] msb = InitMostSignificantBitTable();
#if !NETCOREAPP3_0_OR_GREATER
        private static readonly int[] debruijn32 = new int[64]
        {
            32, 8,  17, -1, -1, 14, -1, -1, -1, 20, -1, -1, -1, 28, -1, 18,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0,  26, 25, 24,
            4,  11, 23, 31, 3,  7,  10, 16, 22, 30, -1, -1, 2,  6,  13, 9,
            -1, 15, -1, 21, -1, 29, 19, -1, -1, -1, -1, -1, 1,  27, 5,  12
        };
#endif
        public static readonly byte[] normalizeAmounts = InitNormalizationTable();
		
        public static long CombineTo64Bit(int left, int right)
        {
            // Combine left (high 32 bits) and right (low 32 bits) into a single 64-bit signed value
            return (long)left << 32 | (long)(uint)right;
        }

        public static ulong ConvertToUInt64(this uint[] array)
        {
            if (array.Length != 8)
                throw new ArgumentException("Array must contain exactly 8 elements.");

            ulong result = 0;

            for (int i = 0; i < 8; i++)
            {
                result |= ((ulong)array[i] & byte.MaxValue) << (8 * i);
            }

            return result;
        }

        /// <summary>
        /// Returns the leading zero count of the given 32-bit integer
        /// </summary>
        public static int CountLeadingSignBits(int n)
        {
            // If the sign bit is 1, we invert the bits to 0 for count-leading-zero.
            if (n < 0)
                n = ~n;

#if NETCOREAPP3_0_OR_GREATER
            return BitOperations.LeadingZeroCount((uint)n);
#else
            // If BSR is used directly, it would have an undefined value for 0.
            if (n == 0)
                return 32;

            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;

            return debruijn32[(uint)n * 0x8c0b2891u >> 26];
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitScanReverse8(int b) => msb[b];

        private static sbyte[] InitMostSignificantBitTable()
        {
            const short msbTableSize = 256;
            sbyte[] msb = new sbyte[msbTableSize];
#if NETCOREAPP || NETSTANDARD1_0_OR_GREATER || NET40_OR_GREATER
            Parallel.For(0, msbTableSize, i =>
            {
                if (i < 1) msb[i] = -1;
                else if (i < 2) msb[i] = 0;
                else if (i < 4) msb[i] = 1;
                else if (i < 8) msb[i] = 2;
                else if (i < 16) msb[i] = 3;
                else if (i < 32) msb[i] = 4;
                else if (i < 64) msb[i] = 5;
                else if (i < 128) msb[i] = 6;
                else msb[i] = 7;
            });
#else
            for (int i = 0; i < msbTableSize; i++)
            {
                if (i < 1) msb[i] = -1;
                else if (i < 2) msb[i] = 0;
                else if (i < 4) msb[i] = 1;
                else if (i < 8) msb[i] = 2;
                else if (i < 16) msb[i] = 3;
                else if (i < 32) msb[i] = 4;
                else if (i < 64) msb[i] = 5;
                else if (i < 128) msb[i] = 6;
                else msb[i] = 7;
            }
#endif
            return msb;
        }

        private static byte[] InitNormalizationTable()
        {
            const byte normalizationTableSize = 32;
            byte[] normalizationTable = new byte[normalizationTableSize];
#if NETCOREAPP || NETSTANDARD1_0_OR_GREATER || NET40_OR_GREATER
            Parallel.For(0, normalizationTableSize, i =>
            {
                if (i < 9) normalizationTable[i] = 0;
                else if (i < 17) normalizationTable[i] = 8;
                else if (i < 25) normalizationTable[i] = 16;
                else normalizationTable[i] = 24;
            });
#else
            for (int i = 0; i < normalizationTableSize; i++)
            {
                if (i < 9) normalizationTable[i] = 0;
                else if (i < 17) normalizationTable[i] = 8;
                else if (i < 25) normalizationTable[i] = 16;
                else normalizationTable[i] = 24;
            }
#endif
            return normalizationTable;
        }
    }
}
