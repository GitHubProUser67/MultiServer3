using System;
using System.Runtime.CompilerServices;

namespace SoftFloatLibrary
{
    public struct ps2float : IEquatable<ps2float>, IComparable<ps2float>, IComparable
    {
        private const byte BIAS = 127;
        private const byte MANTISSA_BITS = 23;
        private const uint SIGNMASK = 0x80000000;

        public const uint MAX_FLOATING_POINT_VALUE = 0x7FFFFFFF;
        public const uint MIN_FLOATING_POINT_VALUE = uint.MaxValue;
        public const uint ONE = 0x3F800000;
        public const uint MIN_ONE = 0xBF800000;
        public const uint EPSILON = 0x00000001;

        public uint raw { get; private set; }

        public uint Mantissa => raw & 0x7FFFFF;
        public byte Exponent => (byte)((raw >> MANTISSA_BITS) & byte.MaxValue);
        public bool Sign => ((raw >> 31) & 1) != 0;

        public bool dz = false;
        public bool iv = false;
        public bool of = false;
        public bool uf = false;

        public ps2float(uint raw)
        {
            this.raw = raw;
        }

        public ps2float(float value)
        {
            raw = BitConverter.ToUInt32(BitConverter.GetBytes(value));
        }

        public ps2float(bool sign, byte exponent, uint mantissa)
        {
            raw = 0;
            raw |= (sign ? 1u : 0u) << 31;
            raw |= (uint)(exponent << MANTISSA_BITS);
            raw |= mantissa & 0x7FFFFF;
        }

        public static ps2float Zero => new ps2float(0);
        public static ps2float MaxValue => new ps2float(MAX_FLOATING_POINT_VALUE);
        public static ps2float MinValue => new ps2float(MIN_FLOATING_POINT_VALUE);
        public static ps2float One => new ps2float(ONE);
        public static ps2float MinOne => new ps2float(MIN_ONE);
        public static ps2float Epsilon => new ps2float(EPSILON);

        public static ps2float operator +(ps2float f1, ps2float f2)
        {
            return f1.Add(f2);
        }

        public ps2float Add(ps2float addend)
        {
            if (IsDenormalized() || addend.IsDenormalized())
                return SolveAddSubDenormalizedOperation(this, addend, true);

            uint a = raw;
            uint b = addend.raw;

            // Exponent difference
            int expDiff = Exponent - addend.Exponent;

            // diff = 25 .. 255 , expt < expd
            if (expDiff >= 25)
            {
                b = b & SIGNMASK;
            }
            // diff = 1 .. 24, expt < expd
            else if (expDiff > 0)
            {
                expDiff = expDiff - 1;
                b = (uint)((unchecked((int)MIN_FLOATING_POINT_VALUE) << expDiff) & b);
            }
            // diff = -255 .. -25, expd < expt
            else if (expDiff <= -25)
            {
                a = a & SIGNMASK;
            }
            // diff = -24 .. -1 , expd < expt
            else if (expDiff < 0)
            {
                expDiff = -expDiff;
                expDiff = expDiff - 1;
                a = (uint)(a & (unchecked((int)MIN_FLOATING_POINT_VALUE) << expDiff));
            }

            return new ps2float(a).DoAdd(new ps2float(b));
        }

        public static ps2float operator -(ps2float f1, ps2float f2)
        {
            return f1.Sub(f2);
        }

        public ps2float Sub(ps2float subtrahend)
        {
            if (IsDenormalized() || subtrahend.IsDenormalized())
                return SolveAddSubDenormalizedOperation(this, subtrahend, false);

            uint a = raw;
            uint b = subtrahend.raw;

            // Exponent difference
            int expDiff = Exponent - subtrahend.Exponent;

            // diff = 25 .. 255 , expt < expd
            if (expDiff >= 25)
            {
                b = b & SIGNMASK;
            }
            // diff = 1 .. 24, expt < expd
            else if (expDiff > 0)
            {
                expDiff = expDiff - 1;
                b = (uint)((unchecked((int)MIN_FLOATING_POINT_VALUE) << expDiff) & b);
            }
            // diff = -255 .. -25, expd < expt
            else if (expDiff <= -25)
            {
                a = a & SIGNMASK;
            }
            // diff = -24 .. -1 , expd < expt
            else if (expDiff < 0)
            {
                expDiff = -expDiff;
                expDiff = expDiff - 1;
                a = (uint)(a & (unchecked((int)MIN_FLOATING_POINT_VALUE) << expDiff));
            }

            return new ps2float(a).DoAdd(-new ps2float(b));
        }

        public static ps2float operator *(ps2float f1, ps2float f2)
        {
            return f1.Mul(f2);
        }

        public ps2float Mul(ps2float mulend)
        {
            if (IsDenormalized() || mulend.IsDenormalized())
                return SolveMultiplicationDenormalizedOperation(this, mulend);

            if (IsZero() || mulend.IsZero())
                return new ps2float(DetermineMultiplicationDivisionOperationSign(this, mulend), 0, 0);

            return DoMul(mulend);
        }

        public static ps2float operator /(ps2float f1, ps2float f2)
        {
            return f1.Div(f2);
        }

        public ps2float Div(ps2float divend)
        {
            FpgaDiv fpga = new FpgaDiv(true, raw, divend.raw);
            return new ps2float(fpga.floatResult) {
                dz = fpga.dz,
                iv = fpga.iv,
                of = fpga.of,
                uf = fpga.uf
            };
        }

        private ps2float DoAdd(ps2float other)
        {
            byte selfExponent = Exponent;
            int resExponent = selfExponent - other.Exponent;

            if (resExponent < 0)
                return other.DoAdd(this);
            else if (resExponent >= 25)
                return this;

            const byte roundingMultiplier = 6;

            // http://graphics.stanford.edu/~seander/bithacks.html#ConditionalNegate
            uint sign1 = (uint)((int)raw >> 31);
            int selfMantissa = (int)(((Mantissa | 0x800000) ^ sign1) - sign1);
            uint sign2 = (uint)((int)other.raw >> 31);
            int otherMantissa = (int)(((other.Mantissa | 0x800000) ^ sign2) - sign2);

            int man = (selfMantissa << roundingMultiplier) + ((otherMantissa << roundingMultiplier) >> resExponent);
            int absMan = Math.Abs(man);
            if (absMan == 0)
                return Zero;

            int rawExp = selfExponent - roundingMultiplier;

            int amount = BitUtils.normalizeAmounts[BitUtils.CountLeadingSignBits(absMan)];
            rawExp -= amount;
            absMan <<= amount;

            int msbIndex = BitUtils.BitScanReverse8(absMan >> MANTISSA_BITS);
            rawExp += msbIndex;
            absMan >>= msbIndex;

            if (rawExp > byte.MaxValue)
            {
                ps2float result = man < 0 ? MinValue : MaxValue;
                result.of = true;
                return result;
            }
            else if (rawExp < 1)
                return new ps2float(man < 0, 0, 0)
                {
                    uf = true
                };

            return new ps2float((uint)man & SIGNMASK | (uint)rawExp << MANTISSA_BITS | ((uint)absMan & 0x7FFFFF));
        }

        private ps2float DoMul(ps2float other)
        {
            byte selfExponent = Exponent;
            byte otherExponent = other.Exponent;
            uint selfMantissa = Mantissa | 0x800000;
            uint otherMantissa = other.Mantissa | 0x800000;
            uint sign = (raw ^ other.raw) & SIGNMASK;

            int resExponent = selfExponent + otherExponent - BIAS;
            uint resMantissa = (uint)(BoothMultiplier.MulMantissa(selfMantissa, otherMantissa) >> MANTISSA_BITS);

            if (resMantissa > 0xFFFFFF)
            {
                resMantissa >>= 1;
                resExponent++;
            }

            if (resExponent > byte.MaxValue)
                return new ps2float(sign | MAX_FLOATING_POINT_VALUE) 
                { 
                    of = true
                };
            else if (resExponent < 1)
                return new ps2float(sign)
                {
                    uf = true
                };

            return new ps2float(sign | (uint)(resExponent << MANTISSA_BITS) | (resMantissa & 0x7FFFFF));
        }

        /// <summary>
        /// Returns the square root of x
        /// </summary>
        public ps2float Sqrt()
        {
            FpgaDiv fpga = new FpgaDiv(false, 0, new ps2float(false, Exponent, Mantissa).raw);
            return new ps2float(fpga.floatResult)
            {
                dz = fpga.dz,
                iv = fpga.iv,
            };
        }

        public ps2float Rsqrt(ps2float other)
        {
            FpgaDiv fpgaSqrt = new FpgaDiv(false, 0, new ps2float(false, other.Exponent, other.Mantissa).raw);
            FpgaDiv fpgaDiv = new FpgaDiv(true, raw, fpgaSqrt.floatResult);
            return new ps2float(fpgaDiv.floatResult)
            {
                dz = fpgaSqrt.dz || fpgaDiv.dz,
                iv = fpgaSqrt.iv || fpgaDiv.iv,
                of = fpgaDiv.of,
                uf = fpgaDiv.uf
            };
        }

        public bool IsDenormalized()
        {
            return Exponent == 0;
        }

        public bool IsZero()
        {
            return Abs() == 0;
        }

        public static bool operator ==(ps2float f1, ps2float f2)
        {
            return f1.Equals(f2);
        }

        public override bool Equals(object obj) => obj != null && GetType() == obj.GetType() && Equals((ps2float)obj);

        public bool Equals(ps2float other)
        {
            // 0 == -0
            return (raw == other.raw) || (Abs() == 0) && (other.Abs() == 0);
        }

        public override int GetHashCode()
        {
            if (raw == SIGNMASK)
                // +0 equals -0
                return 0;

            return (int)raw;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ps2float f1, ps2float f2) => !(f1 == f2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(ps2float f1, ps2float f2) => f1.CompareTo(f2) < 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(ps2float f1, ps2float f2) => f1.CompareTo(f2) > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(ps2float f1, ps2float f2) => f1.CompareTo(f2) <= 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(ps2float f1, ps2float f2) => f1.CompareTo(f2) >= 0;

        public int CompareTo(ps2float other)
        {
            int selfTwoComplementVal = (int)Abs();
            if (Sign) selfTwoComplementVal = -selfTwoComplementVal;

            int otherTwoComplementVal = (int)other.Abs();
            if (other.Sign) otherTwoComplementVal = -otherTwoComplementVal;

            return selfTwoComplementVal.CompareTo(otherTwoComplementVal);
        }

        public int CompareTo(object obj) => obj is ps2float f ? CompareTo(f) : throw new ArgumentException("obj");

        public int CompareOperands(ps2float other)
        {
            return Abs().CompareTo(other.Abs());
        }

        /// <summary>
        /// Returns the maximum of the two given PS2Float values.
        /// </summary>
        public static ps2float Max(ps2float f1, ps2float f2)
        {
            int a = (int)f1.raw;
            int b = (int)f2.raw;

            return new ps2float((a < 0 && b < 0) ? (uint)Math.Min(a, b) : (uint)Math.Max(a, b));
        }

        /// <summary>
        /// Returns the minimum of the two given PS2Float values.
        /// </summary>
        public static ps2float Min(ps2float f1, ps2float f2)
        {
            int a = (int)f1.raw;
            int b = (int)f2.raw;

            return new ps2float((a < 0 && b < 0) ? (uint)Math.Max(a, b) : (uint)Math.Min(a, b));
        }

        public static void Clip(uint f1, uint f2, out int cplus, out int cminus)
        {
            int resultPlus = 0;
            int resultMinus = 0;
            uint a;

            if ((f1 & 0x7F800000) == 0)
            {
                f1 &= 0xFF800000;
            }

            a = f1;

            if ((f2 & 0x7F800000) == 0)
            {
                f2 &= 0xFF800000;
            }

            f1 = f1 & MAX_FLOATING_POINT_VALUE;
            f2 = f2 & MAX_FLOATING_POINT_VALUE;

            if ((-1 < (int)a) && (f2 < f1))
                resultPlus = 1;

            cplus = resultPlus;

            if (((int)a < 0) && (f2 < f1))
                resultMinus = 1;

            cminus = resultMinus;
        }

        public uint Abs()
        {
            return raw & MAX_FLOATING_POINT_VALUE;
        }

        private ps2float Negate()
        {
            return new ps2float(raw ^ SIGNMASK);
        }

        private static ps2float SolveMultiplicationDenormalizedOperation(ps2float a, ps2float b)
        {
            return new ps2float(DetermineMultiplicationDivisionOperationSign(a, b), 0, 0);
        }

        private static ps2float SolveAddSubDenormalizedOperation(ps2float a, ps2float b, bool add)
        {
            bool sign = add ? DetermineAdditionOperationSign(a, b) : DetermineSubtractionOperationSign(a, b);

            if (a.IsDenormalized() && !b.IsDenormalized())
                return new ps2float(sign, b.Exponent, b.Mantissa);
            else if (!a.IsDenormalized() && b.IsDenormalized())
                return new ps2float(sign, a.Exponent, a.Mantissa);
            else if (a.IsDenormalized() && b.IsDenormalized())
                return new ps2float(sign, 0, 0);
            else
                throw new InvalidOperationException("Both numbers are not denormalized");
        }

        private static bool DetermineMultiplicationDivisionOperationSign(ps2float a, ps2float b)
        {
            return a.Sign ^ b.Sign;
        }

        private static bool DetermineAdditionOperationSign(ps2float a, ps2float b)
        {
            if (a.IsZero() && b.IsZero())
            {
                if (!a.Sign || !b.Sign)
                    return false;
                else if (a.Sign && b.Sign)
                    return true;
                else
                    throw new InvalidOperationException("Unhandled addition operation flags");
            }

            return a.CompareOperands(b) >= 0 ? a.Sign : b.Sign;
        }

        private static bool DetermineSubtractionOperationSign(ps2float a, ps2float b)
        {
            if (a.IsZero() && b.IsZero())
            {
                if (!a.Sign || b.Sign)
                    return false;
                else if (a.Sign && !b.Sign)
                    return true;
                else
                    throw new InvalidOperationException("Unhandled subtraction operation flags");
            }

            return a.CompareOperands(b) >= 0 ? a.Sign : !b.Sign;
        }

        /// <summary>
        /// Converts a float number to a ps2float value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ps2float(float f)
        {
            return new ps2float(f);
        }

        /// <summary>
        /// Creates a float number from a ps2float value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator float(ps2float f)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(f.raw));
        }

        /// <summary>
        /// Converts an sfloat number to a ps2float value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ps2float(sfloat f)
        {
            return new ps2float(f.RawValue);
        }

        /// <summary>
        /// Creates an sfloat number from a ps2float value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator sfloat(ps2float f)
        {
            return sfloat.FromRaw(f.raw);
        }

        /// <summary>
        /// Creates a double number from a ps2float value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator double(ps2float f)
        {
            return f.ToDouble();
        }

        /// <summary>
        /// Creates a int number from a ps2float value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(ps2float f)
        {
            return Ftoi(0, f.raw);
        }

        /// <summary>
        /// Creates a ps2float number from a int value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator ps2float(int value)
        {
            return Itof(0, value);
        }

        public static ps2float operator -(ps2float f) => f.Negate();

        public static ps2float Itof(int complement, int value)
        {
            if (value == 0)
                return Zero;

            int resExponent;

            bool negative = value < 0;

            if (value == int.MinValue)
            {
                if (complement <= 0)
                    // special case
                    return new ps2float(0xcf000000);
                else
                    value = int.MaxValue;
            }

            int u = Math.Abs(value);

            int shifts;

            int lzcnt = BitUtils.CountLeadingSignBits(u);
            if (lzcnt < 8)
            {
                int count = 8 - lzcnt;
                u >>= count;
                shifts = -count;
            }
            else
            {
                int count = lzcnt - 8;
                u <<= count;
                shifts = count;
            }

            resExponent = BIAS + MANTISSA_BITS - shifts - complement;

            if (resExponent >= 158)
                return negative ? new ps2float(0xcf000000) : new ps2float(0x4f000000);
            else if (resExponent >= 0)
                return new ps2float(negative, (byte)resExponent, (uint)u);

            return Zero;
        }

        public static int Ftoi(int complement, uint f)
        {
            uint a, result;

            a = f;
            if ((f & 0x7F800000) == 0)
                result = 0;
            else
            {
                complement = (int)(f >> MANTISSA_BITS & byte.MaxValue) + complement;
                f &= 0x7FFFFF;
                f |= 0x800000;
                if (complement < 158)
                {
                    if (complement > 126)
                    {
                        f = (f << 7) >> (31 - ((byte)complement - 126));
                        if ((int)a < 0)
                            f = ~f + 1;
                        result = f;
                    }
                    else
                        result = 0;
                }
                else if ((int)a < 0)
                    result = SIGNMASK;
                else
                    result = MAX_FLOATING_POINT_VALUE;
            }

            return (int)result;
        }

        public double ToDouble()
        {
            double res = (Mantissa / Math.Pow(2, MANTISSA_BITS) + 1.0) * Math.Pow(2, Exponent - 127.0);
            if (Sign)
                res *= -1.0;

            return res;
        }

        public override string ToString()
        {
            double res = ToDouble();

            uint value = raw;
            if (IsDenormalized())
                return $"Denormalized({res:F6})";
            else if (value == MAX_FLOATING_POINT_VALUE)
                return $"Fmax({res:F6})";
            else if (value == MIN_FLOATING_POINT_VALUE)
                return $"-Fmax({res:F6})";

            return $"PS2Float({res:F6})";
        }
    }
}