// mostly from https://github.com/CodesInChaos/SoftFloat

// Copyright (c) 2011 CodesInChaos
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// The MIT License (MIT) - http://www.opensource.org/licenses/mit-license.php
// If you need a different license please contact me

using System;
using System.Diagnostics;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif
using System.Runtime.CompilerServices;

namespace SoftFloatLibrary
{
    // Internal representation is identical to IEEE binary64 floating point numbers
    [DebuggerDisplay("{ToStringInv()}")]
    public struct sdouble : IEquatable<sdouble>, IComparable<sdouble>, IComparable, IFormattable
    {
        /// <summary>
        /// Raw byte representation of an sdouble number
        /// </summary>
        private readonly ulong rawValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private sdouble(ulong raw)
        {
            rawValue = raw;
        }

        /// <summary>
        /// Creates an sdouble number from its raw byte representation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdouble FromRaw(ulong raw)
        {
            return new sdouble(raw);
        }

        public ulong RawValue => rawValue;

        public ulong RawMantissa => rawValue & 0xFFFFFFFFFFFFF;
        public long Mantissa
        {
            get
            {
                if (RawExponent != 0)
                {
                    ulong sign = (ulong)((long)rawValue >> 63);
                    return (long)(((RawMantissa | 0x10000000000000) ^ sign) - sign);
                }
                else
                {
                    ulong sign = (ulong)((long)rawValue >> 63);
                    return (long)(((RawMantissa) ^ sign) - sign);
                }
            }
        }

        public short Exponent => (short)(RawExponent - ExponentBias);
        public ushort RawExponent => (ushort)(rawValue >> MantissaBits & 0x7FF);

        private const ulong SignMask = 0x8000000000000000;
        private const int MantissaBits = 52;
        private const int ExponentBits = 11;
        private const int ExponentBias = 1023;

        private const ulong RawZero = 0;
        private const ulong RawNaN = 0xFFF8000000000000; // Same as double.NaN
        private const ulong RawPositiveInfinity = 0x7FF0000000000000;
        private const ulong RawNegativeInfinity = RawPositiveInfinity ^ SignMask;
        private const ulong RawOne = 0x3FF0000000000000;
        private const ulong RawMinusOne = RawOne ^ SignMask;
        private const ulong RawMaxValue = 0x7FEFFFFFFFFFFFFF;
        private const ulong RawMinValue = 0x7FEFFFFFFFFFFFFF ^ SignMask;
        private const ulong RawEpsilon = 0x0000000000000001;
        private const ulong RawLog2OfE = 0;

        public static sdouble Zero => new sdouble(0);
        public static sdouble PositiveInfinity => new sdouble(RawPositiveInfinity);
        public static sdouble NegativeInfinity => new sdouble(RawNegativeInfinity);
        public static sdouble NaN => new sdouble(RawNaN);
        public static sdouble One => new sdouble(RawOne);
        public static sdouble MinusOne => new sdouble(RawMinusOne);
        public static sdouble MaxValue => new sdouble(RawMaxValue);
        public static sdouble MinValue => new sdouble(RawMinValue);
        public static sdouble Epsilon => new sdouble(RawEpsilon);

        public override string ToString() => ((double)this).ToString();

        /// <summary>
        /// Creates an sdouble number from its parts: sign, exponent, mantissa
        /// </summary>
        /// <param name="sign">Sign of the number: false = the number is positive, true = the number is negative</param>
        /// <param name="exponent">Exponent of the number</param>
        /// <param name="mantissa">Mantissa (significand) of the number</param>
        /// <returns></returns>
        public static sdouble FromParts(bool sign, ulong exponent, ulong mantissa)
        {
            return FromRaw((sign ? SignMask : 0UL) | ((exponent & 0x7ffUL) << MantissaBits) | (mantissa & ((1UL << MantissaBits) - 1L)));
        }

        /// <summary>
        /// Creates an sdouble number from a double value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator sdouble(double d)
        {
            return new sdouble(ReinterpretDoubleToUInt64(d));
        }

        /// <summary>
        /// Converts an sdouble number to a double value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator double(sdouble d)
        {
            return ReinterpretUInt64ToDouble(d.rawValue);
        }

        /// <summary>
        /// Converts an sdouble number to an integer
        /// </summary>
        public static explicit operator long(sdouble d)
        {
            if (d.Exponent < 0)
            {
                return 0;
            }

            int shift = MantissaBits - d.Exponent;
            var mantissa = (long)(d.RawMantissa | (1L << MantissaBits));
            long value = shift < 0 ? mantissa << -shift : mantissa >> shift;
            return d.IsPositive() ? value : -value;
        }

        /// <summary>
        /// Creates an sdouble number from an integer
        /// </summary>
        public static explicit operator sdouble(long value)
        {
            if (value == 0)
            {
                return Zero;
            }

            if (value == long.MinValue)
            {
                // special case
                return FromRaw(0xcf00000000000000);
            }

            bool negative = value < 0L;
            long u = Math.Abs(value);

            int shifts;

            int lzcnt = 0;
            long tu = u;
            while ((tu & (1L << 63)) == 0L) {
                tu = (tu << 1);
                lzcnt++;
            }
            
            if (lzcnt < 11)
            {
                int count = 11 - lzcnt;
                u >>= count;
                shifts = -count;
            }
            else
            {
                int count = lzcnt - 11;
                u <<= count;
                shifts = count;
            }

            ulong exponent = (ulong)(ExponentBias + MantissaBits - shifts);
            return FromParts(negative, exponent, (ulong)u);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdouble operator -(sdouble d) => new sdouble(d.rawValue ^ SignMask);

        private static sdouble InternalAdd(sdouble d1, sdouble d2)
        {
            ushort rawExp1 = d1.RawExponent;
            ushort rawExp2 = d2.RawExponent;
            int deltaExp = rawExp1 - rawExp2;

            if (rawExp1 != 2047)
            {
                // Finite
                if (deltaExp > 55)
                {
                    return d1;
                }
				
                const byte roundingMultiplier = 9;

                long man1;
                long man2;
                if (rawExp2 != 0)
                {
                    // man1 = d1.Mantissa
                    // http://graphics.stanford.edu/~seander/bithacks.html#ConditionalNegate
                    ulong sign1 = (ulong)((long)d1.rawValue >> 63);
                    man1 = (long)(((d1.RawMantissa | 0x10000000000000) ^ sign1) - sign1);
                    // man2 = d2.Mantissa
                    ulong sign2 = (ulong)((long)d2.rawValue >> 63);
                    man2 = (long)(((d2.RawMantissa | 0x10000000000000) ^ sign2) - sign2);
                }
                else
                {
                    // Subnorm
                    // man2 = d2.Mantissa
                    ulong sign2 = (ulong)((long)d2.rawValue >> 63);
                    man2 = (long)((d2.RawMantissa ^ sign2) - sign2);

                    man1 = d1.Mantissa;

                    rawExp2 = 1;
                    if (rawExp1 == 0)
                    {
                        rawExp1 = 1;
                    }

                    deltaExp = rawExp1 - rawExp2;
                }

                long man = (man1 << roundingMultiplier) + ((man2 << roundingMultiplier) >> deltaExp);
                ulong absMan = (ulong)Math.Abs(man);
                if (absMan == 0)
                {
                    return Zero;
                }
                
                ulong b = absMan >> MantissaBits;
                int rawExp = rawExp1 - roundingMultiplier;
                while (b == 0)
                {
                    rawExp -= ExponentBits;
                    absMan <<= ExponentBits;
                    b = absMan >> MantissaBits;
                }

                int msbIndex = BitUtils.BitScanReverse11(b);
                rawExp += msbIndex;
                absMan >>= msbIndex;
                if ((ulong)(rawExp - 1) < 2046)
                {
                    ulong raw = (ulong)man & SignMask | (ulong)rawExp << MantissaBits | ((ulong)absMan & 0xFFFFFFFFFFFFF);
                    return new sdouble(raw);
                }
                else
                {
                    if (rawExp >= 2047)
                    {
                        // Overflow
                        return man >= 0 ? PositiveInfinity : NegativeInfinity;
                    }

                    if (rawExp >= -51)
                    {
                        ulong raw = (ulong)man & SignMask | (ulong)(absMan >> (-rawExp + 1));
                        return new sdouble(raw);
                    }

                    return Zero;
                }
            }
            else
            {
                // Special

                if (rawExp2 != 2047)
                {
                    // d1 is NaN, +Inf, -Inf and d2 is finite
                    return d1;
                }

                // Both not finite
                return d1.rawValue == d2.rawValue ? d1 : NaN;
            }
        }

        public static sdouble operator +(sdouble d1, sdouble d2)
        {
            return d1.RawExponent - d2.RawExponent >= 0 ? InternalAdd(d1, d2) : InternalAdd(d2, d1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdouble operator -(sdouble d1, sdouble d2) => d1 + (-d2);
#if NET7_0_OR_GREATER
        public static sdouble operator *(sdouble d1, sdouble d2)
        {
            long man1;
            int rawExp1 = d1.RawExponent;
            ulong sign1;
            ulong sign2;
            if (rawExp1 == 0)
            {
                // SubNorm
                sign1 = (ulong)((long)d1.rawValue >> 63);
                long rawMan1 = (long)d1.RawMantissa;
                if (rawMan1 == 0)
                {
                    if (d2.IsFinite())
                    {
                        // 0 * d2
                        return new sdouble((d1.rawValue ^ d2.rawValue) & SignMask);
                    }
                    else
                    {
                        // 0 * Infinity
                        // 0 * NaN
                        return NaN;
                    }
                }

                rawExp1 = 1;
                while ((rawMan1 & 0x10000000000000) == 0)
                {
                    rawMan1 <<= 1;
                    rawExp1--;
                }

                //Debug.Assert(rawMan1 >> MantissaBits == 1);
                man1 = (long)(((ulong)rawMan1 ^ sign1) - sign1);
            }
            else if (rawExp1 != 2047)
            {
                // Norm
                sign1 = (ulong)((long)d1.rawValue >> 63);
                man1 = (long)(((d1.RawMantissa | 0x10000000000000) ^ sign1) - sign1);
            }
            else
            {
                // Non finite
                if (d1.rawValue == RawPositiveInfinity)
                {
                    if (d2.IsZero())
                    {
                        // Infinity * 0
                        return NaN;
                    }

                    if (d2.IsNaN())
                    {
                        // Infinity * NaN
                        return NaN;
                    }

                    if ((long)d2.rawValue >= 0)
                    {
                        // Infinity * d
                        return PositiveInfinity;
                    }
                    else
                    {
                        // Infinity * -d
                        return NegativeInfinity;
                    }
                }
                else if (d1.rawValue == RawNegativeInfinity)
                {
                    if (d2.IsZero() || d2.IsNaN())
                    {
                        // -Infinity * 0
                        // -Infinity * NaN
                        return NaN;
                    }

                    if ((long)d2.rawValue < 0)
                    {
                        // -Infinity * -d
                        return PositiveInfinity;
                    }
                    else
                    {
                        // -Infinity * d
                        return NegativeInfinity;
                    }
                }
                else
                {
                    return d1;
                }
            }

            long man2;
            int rawExp2 = d2.RawExponent;
            if (rawExp2 == 0)
            {
                // SubNorm
                sign2 = (ulong)((long)d2.rawValue >> 63);
                long rawMan2 = (long)d2.RawMantissa;
                if (rawMan2 == 0)
                {
                    if (d1.IsFinite())
                    {
                        // d1 * 0
                        return new sdouble((d1.rawValue ^ d2.rawValue) & SignMask);
                    }
                    else
                    {
                        // Infinity * 0
                        // NaN * 0
                        return NaN;
                    }
                }

                rawExp2 = 1;
                while ((rawMan2 & 0x10000000000000) == 0)
                {
                    rawMan2 <<= 1;
                    rawExp2--;
                }

                //Debug.Assert(rawMan2 >> MantissaBits == 1);
                man2 = (long)(((ulong)rawMan2 ^ sign2) - sign2);
            }
            else if (rawExp2 != 2047)
            {
                // Norm
                sign2 = (ulong)((long)d2.rawValue >> 63);
                man2 = (long)(((d2.RawMantissa | 0x10000000000000) ^ sign2) - sign2);
            }
            else
            {
                // Non finite
                if (d2.rawValue == RawPositiveInfinity)
                {
                    if (d1.IsZero())
                    {
                        // 0 * Infinity
                        return NaN;
                    }

                    if ((long)d1.rawValue >= 0)
                    {
                        // d * Infinity
                        return PositiveInfinity;
                    }
                    else
                    {
                        // -d * Infinity
                        return NegativeInfinity;
                    }
                }
                else if (d2.rawValue == RawNegativeInfinity)
                {
                    if (d1.IsZero())
                    {
                        // 0 * -Infinity
                        return NaN;
                    }

                    if ((long)d1.rawValue < 0)
                    {
                        // -d * -Infinity
                        return PositiveInfinity;
                    }
                    else
                    {
                        // d * -Infinity
                        return NegativeInfinity;
                    }
                }
                else
                {
                    return d2;
                }
            }

            
            Int128 longMan = (Int128)man1 * man2;
            long man = (long)(longMan >> MantissaBits);
            //Debug.Assert(man != 0);
            ulong absMan = (ulong)Math.Abs(man);
            int rawExp = rawExp1 + rawExp2 - ExponentBias;
            ulong sign = (ulong)man & SignMask;
            if ((absMan & 0x20000000000000) != 0)
            {
                absMan >>= 1;
                rawExp++;
            }

            //Debug.Assert(absMan >> MantissaBits == 1);
            if (rawExp >= 2047)
            {
                // Overflow
                return new sdouble(sign ^ RawPositiveInfinity);
            }

            if (rawExp <= 0)
            {
                // Subnorms/Underflow
                if (rawExp <= -53)
                {
                    return new sdouble(sign);
                }

                absMan >>= -rawExp + 1;
                rawExp = 0;
            }

            ulong raw = sign | (ulong)rawExp << MantissaBits | absMan & 0xFFFFFFFFFFFFF;
            return new sdouble(raw);
        }
       
        public static sdouble operator /(sdouble d1, sdouble d2)
        {
            if (d1.IsNaN() || d2.IsNaN())
            {
                return NaN;
            }

            long man1;
            int rawExp1 = d1.RawExponent;
            ulong sign1;
            ulong sign2;
            if (rawExp1 == 0)
            {
                // SubNorm
                sign1 = (ulong)((long)d1.rawValue >> 63);
                long rawMan1 = (long)d1.RawMantissa;
                if (rawMan1 == 0)
                {
                    if (d2.IsZero())
                    {
                        // 0 / 0
                        return NaN;
                    }
                    else
                    {
                        // 0 / d2
                        return new sdouble((d1.rawValue ^ d2.rawValue) & SignMask);
                    }
                }

                rawExp1 = 1;
                while ((rawMan1 & 0x10000000000000) == 0)
                {
                    rawMan1 <<= 1;
                    rawExp1--;
                }

                // Debug.Assert(rawMan1 >> MantissaBits == 1);
                man1 = (long)(((ulong)rawMan1 ^ sign1) - sign1);
            }
            else if (rawExp1 != 2047)
            {
                // Norm
                sign1 = (ulong)((long)d1.rawValue >> 63);
                man1 = (long)(((d1.RawMantissa | 0x10000000000000) ^ sign1) - sign1);
            }
            else
            {
                // Non finite
                if (d1.rawValue == RawPositiveInfinity)
                {
                    if (d2.IsZero())
                    {
                        // Infinity / 0
                        return PositiveInfinity;
                    }

                    // +-Infinity / Infinity
                    return NaN;
                }
                else if (d1.rawValue == RawNegativeInfinity)
                {
                    if (d2.IsZero())
                    {
                        // -Infinity / 0
                        return NegativeInfinity;
                    }

                    // -Infinity / +-Infinity
                    return NaN;
                }
                else
                {
                    // NaN
                    return d1;
                }
            }

            long man2;
            int rawExp2 = d2.RawExponent;
            if (rawExp2 == 0)
            {
                // SubNorm
                sign2 = (ulong)((long)d2.rawValue >> 63);
                long rawMan2 = (long)d2.RawMantissa;
                if (rawMan2 == 0)
                {
                    // d1 / 0
                    return new sdouble(((d1.rawValue ^ d2.rawValue) & SignMask) | RawPositiveInfinity);
                }

                rawExp2 = 1;
                while ((rawMan2 & 0x10000000000000) == 0)
                {
                    rawMan2 <<= 1;
                    rawExp2--;
                }

                // Debug.Assert(rawMan2 >> MantissaBits == 1);
                man2 = (long)(((ulong)rawMan2 ^ sign2) - sign2);
            }
            else if (rawExp2 != 2047)
            {
                // Norm
                sign2 = (ulong)((long)d2.rawValue >> 63);
                man2 = (long)(((d2.RawMantissa | 0x10000000000000) ^ sign2) - sign2);
            }
            else
            {
                // Non finite
                if (d2.rawValue == RawPositiveInfinity)
                {
                    if (d1.IsZero())
                    {
                        // 0 / Infinity
                        return Zero;
                    }
                    if ((long)d1.rawValue >= 0)
                    {
                        // d1 / Infinity
                        return PositiveInfinity;
                    }
                    else
                    {
                        // -d1 / Infinity
                        return NegativeInfinity;
                    }
                }
                else if (d2.rawValue == RawNegativeInfinity)
                {
                    if (d1.IsZero())
                    {
                        // 0 / -Infinity
                        return new sdouble(SignMask);
                    }
                    if ((long)d1.rawValue < 0)
                    {
                        // -d1 / -Infinity
                        return PositiveInfinity;
                    }
                    else
                    {
                        // d1 / -Infinity
                        return NegativeInfinity;
                    }
                }
                else
                {
                    // NaN
                    return d2;
                }
            }

            Int128 longMan = ((Int128)man1 << MantissaBits) / (long)man2;
            long man = (long)longMan;
            // Debug.Assert(man != 0);
            ulong absMan = (ulong)Math.Abs(man);
            int rawExp = rawExp1 - rawExp2 + ExponentBias;
            ulong sign = (ulong)man & SignMask;

            if ((absMan & 0x10000000000000) == 0)
            {
                absMan <<= 1;
                --rawExp;
            }

            // Debug.Assert(absMan >> MantissaBits == 1);
            if (rawExp >= 2047)
            {
                // Overflow
                return new sdouble(sign ^ RawPositiveInfinity);
            }

            if (rawExp <= 0)
            {
                // Subnorms/Underflow
                if (rawExp <= -51)
                {
                    return new sdouble(sign);
                }

                absMan >>= -rawExp + 1;
                rawExp = 0;
            }

            ulong raw = sign | (ulong)rawExp << MantissaBits | absMan & 0xFFFFFFFFFFFFF;
            return new sdouble(raw);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sdouble operator %(sdouble d1, sdouble d2) => libm_sdouble.fmod(d1, d2);
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong ReinterpretDoubleToUInt64(double d) => *(ulong*)&d;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe double ReinterpretUInt64ToDouble(ulong i) => *(double*)&i;

        public override bool Equals(object obj) => obj is sdouble s && Equals(s);

        public bool Equals(sdouble other)
        {
            if (RawExponent != 2047)
            {
                // 0 == -0
                return (rawValue == other.rawValue) || ((rawValue & 0x7FFFFFFFFFFFFFFF) == 0) && ((other.rawValue & 0x7FFFFFFFFFFFFFFF) == 0);
            }
            else
            {
                if (RawMantissa == 0)
                {
                    // Infinities
                    return rawValue == other.rawValue;
                }
                else
                {
                    // NaNs are equal for `Equals` (as opposed to the == operator)
                    return other.RawMantissa != 0;
                }
            }
        }

        public override int GetHashCode()
        {
            if (rawValue == SignMask)
            {
                // +0 equals -0
                return 0;
            }

            if (!IsNaN())
            {
                return (int)rawValue;
            }
            else
            {
                // All NaNs are equal
                return unchecked((int)RawNaN);
            }
        }

        public static bool operator ==(sdouble d1, sdouble d2)
        {
            if (d1.RawExponent != 2047)
            {
                // 0 == -0
                return (d1.rawValue == d2.rawValue) || ((d1.rawValue & 0x7FFFFFFFFFFFFFFF) == 0) && ((d2.rawValue & 0x7FFFFFFFFFFFFFFF) == 0);
            }
            else
            {
                if (d1.RawMantissa == 0)
                {
                    // Infinities
                    return d1.rawValue == d2.rawValue;
                }
                else
                {
                    //NaNs
                    return false;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(sdouble d1, sdouble d2) => !(d1 == d2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(sdouble d1, sdouble d2) => !d1.IsNaN() && !d2.IsNaN() && d1.CompareTo(d2) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(sdouble d1, sdouble d2) => !d1.IsNaN() && !d2.IsNaN() && d1.CompareTo(d2) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(sdouble d1, sdouble d2) => !d1.IsNaN() && !d2.IsNaN() && d1.CompareTo(d2) <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(sdouble d1, sdouble d2) => !d1.IsNaN() && !d2.IsNaN() && d1.CompareTo(d2) >= 0;

        public int CompareTo(sdouble other)
        {
            if (IsNaN() && other.IsNaN())
            {
                return 0;
            }

            ulong sign1 = (ulong)((long)rawValue >> 63);
            long val1 = (long)(((rawValue) ^ (sign1 & 0x7FFFFFFFFFFFFFFF)) - sign1);

            ulong sign2 = (ulong)((long)other.rawValue >> 63);
            long val2 = (long)(((other.rawValue) ^ (sign2 & 0x7FFFFFFFFFFFFFFF)) - sign2);
            return val1.CompareTo(val2);
        }

        public int CompareTo(object obj) => obj is sdouble d ? CompareTo(d) : throw new ArgumentException("obj");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInfinity() => (rawValue & 0x7FFFFFFFFFFFFFFF) == 0x7FF0000000000000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNegativeInfinity() => rawValue == RawNegativeInfinity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPositiveInfinity() => rawValue == RawPositiveInfinity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNaN() => (RawExponent == 2047) && !IsInfinity();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFinite() => RawExponent != 2047;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsZero() => (rawValue & 0x7FFFFFFFFFFFFFFF) == 0;

        public string ToString(string format, IFormatProvider formatProvider) => ((double)this).ToString(format, formatProvider);
        public string ToString(string format) => ((double)this).ToString(format);
        public string ToString(IFormatProvider provider) => ((double)this).ToString(provider);
        public string ToStringInv() => ((double)this).ToString(System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Returns the absolute value of the given sdouble number
        /// </summary>
        public static sdouble Abs(sdouble d)
        {
            if (d.RawExponent != 2047 || d.IsInfinity())
            {
                return new sdouble(d.rawValue & 0x7FFFFFFFFFFFFFFF);
            }
            else
            {
                // Leave NaN untouched
                return d;
            }
        }

        /// <summary>
        /// Returns the maximum of the two given sdouble values. Returns NaN iff either argument is NaN.
        /// </summary>
        public static sdouble Max(sdouble val1, sdouble val2)
        {
            if (val1 > val2)
            {
                return val1;
            }
            else if (val1.IsNaN())
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        /// <summary>
        /// Returns the minimum of the two given sdouble values. Returns NaN iff either argument is NaN.
        /// </summary>
        public static sdouble Min(sdouble val1, sdouble val2)
        {
            if (val1 < val2)
            {
                return val1;
            }
            else if (val1.IsNaN())
            {
                return val1;
            }
            else
            {
                return val2;
            }
        }

        /// <summary>
        /// Returns true if the sdouble number has a positive sign.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsPositive() => (rawValue & SignMask) == 0;

        /// <summary>
        /// Returns true if the sdouble number has a negative sign.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNegative() => (rawValue & SignMask) != 0;

        public int Sign()
        {
            if (IsNaN())
            {
                return 0;
            }

            if (IsZero())
            {
                return 0;
            }
            else if (IsPositive())
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
}