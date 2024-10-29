using System;
using System.Collections.Specialized;

namespace EmotionEngine.Emulator
{
	// Adapted from: https://github.com/gregorygaines/ps2-floating-point-rs
	public class Ps2Float : IComparable<Ps2Float>
	{
		public bool Sign { get; private set; }
		public byte Exponent { get; private set; }
		public uint Mantissa { get; private set; }

        public const byte BIAS = 127;
		public const uint MAX_FLOATING_POINT_VALUE = 0x7FFFFFFF;
		public const uint MIN_FLOATING_POINT_VALUE = 0xFFFFFFFF;
		public const uint POSITIVE_INFINITY_VALUE = 0x7F800000;
		public const uint NEGATIVE_INFINITY_VALUE = 0xFF800000;
		public const uint ONE = 0x3F800000;
		public const uint MIN_ONE = 0xBF800000;
		public const int IMPLICIT_LEADING_BIT_POS = 23;

		public Ps2Float(uint value)
		{
			Sign = ((value >> 31) & 1) != 0;
			Exponent = (byte)((value >> 23) & 0xFF);
			Mantissa = value & 0x7FFFFF;
		}

		public Ps2Float(bool sign, byte exponent, uint mantissa)
		{
			Sign = sign;
			Exponent = exponent;
			Mantissa = mantissa;
		}

		public static Ps2Float Max()
		{
			return new Ps2Float(MAX_FLOATING_POINT_VALUE);
		}

		public static Ps2Float Min()
		{
			return new Ps2Float(MIN_FLOATING_POINT_VALUE);
		}

		public static Ps2Float One()
		{
			return new Ps2Float(ONE);
		}

		public static Ps2Float MinOne()
		{
			return new Ps2Float(MIN_ONE);
		}

		public uint AsUInt32()
		{
			uint result = 0;
			result |= (Sign ? 1u : 0u) << 31;
			result |= (uint)(Exponent << 23);
			result |= Mantissa;
			return result;
		}

		public Ps2Float Add(Ps2Float addend)
		{
			if (IsDenormalized() || addend.IsDenormalized())
				return SolveAddSubDenormalizedOperation(this, addend, true);

			if (IsAbnormal() && addend.IsAbnormal())
				return SolveAbnormalAdditionOrSubtractionOperation(this, addend, true);

			if (Sign != addend.Sign)
				return Sub(addend);

			return DoAddOrSub(addend, true);
		}

		public Ps2Float Sub(Ps2Float subtrahend)
		{
			if (IsDenormalized() || subtrahend.IsDenormalized())
				return SolveAddSubDenormalizedOperation(this, subtrahend, false);

			if (IsAbnormal() && subtrahend.IsAbnormal())
				return SolveAbnormalAdditionOrSubtractionOperation(this, subtrahend, false);

			if (CompareTo(subtrahend) == 0)
			{
				return new Ps2Float(0)
				{
					Sign = DetermineSubtractionOperationSign(this, subtrahend)
				};
			}

			return DoAddOrSub(subtrahend, false);
		}

		public Ps2Float Mul(Ps2Float mulend)
		{
			if (IsDenormalized() || mulend.IsDenormalized())
				return SolveMultiplicationDenormalizedOperation(this, mulend);

			if (IsAbnormal() && mulend.IsAbnormal())
				return SolveAbnormalMultiplicationOrDivisionOperation(this, mulend, true);

			if (IsZero() || mulend.IsZero())
			{
				return new Ps2Float(0)
				{
					Sign = DetermineSubtractionOperationSign(this, mulend)
				};
			}

			return DoMul(mulend);
		}
		
		public Ps2Float Div(Ps2Float mulend)
		{
			if (IsDenormalized() || mulend.IsDenormalized())
				return SolveDivisionDenormalizedOperation(this, mulend);

			if (IsAbnormal() && mulend.IsAbnormal())
				return SolveAbnormalMultiplicationOrDivisionOperation(this, mulend, false);

			if (IsZero())
			{
				return new Ps2Float(0)
				{
					Sign = DetermineSubtractionOperationSign(this, mulend)
				};
			}
			else if (mulend.IsZero())
				return DetermineSubtractionOperationSign(this, mulend) ? Min() : Max();

			return DoDiv(mulend);
		}

		private Ps2Float DoAddOrSub(Ps2Float other, bool add)
		{
			int expDiff = Math.Abs(Exponent - other.Exponent);
			uint selfMantissa = Mantissa | 0x800000;
			uint otherMantissa = other.Mantissa | 0x800000;

			Ps2Float result = new Ps2Float(0);

			if (Exponent >= other.Exponent)
			{
				otherMantissa >>= expDiff;
				result.Exponent = Exponent;
			}
			else
			{
				selfMantissa >>= expDiff;
				result.Exponent = other.Exponent;
			}

			if (add)
			{
				result.Mantissa = selfMantissa + otherMantissa;
				result.Sign = Sign;
			}
			else
			{
				result.Mantissa = selfMantissa - otherMantissa;
				result.Sign = DetermineSubtractionOperationSign(this, other);
			}

			if (result.Mantissa > 0)
			{
				int leadingBitPosition = GetMostSignificantBitPosition(result.Mantissa);
				while (leadingBitPosition != IMPLICIT_LEADING_BIT_POS)
				{
					if (leadingBitPosition > IMPLICIT_LEADING_BIT_POS)
					{
						result.Mantissa >>= 1;
						try
						{
							result.Exponent = checked((byte)(result.Exponent + 1));
						}
						catch (OverflowException)
						{
							return result.Sign ? Min() : Max();
						}
						leadingBitPosition--;
					}
					else if (leadingBitPosition < IMPLICIT_LEADING_BIT_POS)
					{
						result.Mantissa <<= 1;
						try
						{
							result.Exponent = checked((byte)(result.Exponent - 1));
						}
						catch (OverflowException)
						{
							return new Ps2Float(result.Sign, 0, 0);
						}
						leadingBitPosition++;
					}
				}
			}

			result.Mantissa &= 0x7FFFFF;
			return result.RoundTowardsZero();
		}

        // Rounding can be slightly off: https://fobes.dev/ps2/detecting-emu-vu-floats.
        private Ps2Float DoMul(Ps2Float other)
		{
            uint selfMantissa = Mantissa | 0x800000;
            uint otherMantissa = other.Mantissa | 0x800000;
            int resExponent = Exponent + other.Exponent - BIAS;

            Ps2Float result = new Ps2Float(0) { Sign = DetermineMultiplicationDivisionOperationSign(this, other) };

            if (resExponent >= 255)
                return result.Sign ? Min() : Max();
            else if (resExponent <= 0)
                return new Ps2Float(result.Sign, 0, 0);

            long res = 0;
            long mask = Convert.ToInt64("0xFFFFFFFFFFFFF000", 16); // Alter the precision of the multiplication slightly: https://github.com/PCSX2/pcsx2/commit/00f14b5760ab2cd73bd9577993122674852a2f67

            result.Exponent = (byte)resExponent;

            otherMantissa <<= 1;

            uint[] part = new uint[13]; // Partial products
            uint[] bit = new uint[13]; // More partial products. 0 or 1.

            for (int i = 0; i <= 12; i++, otherMantissa >>= 2)
            {
                uint test = otherMantissa & 7;
                if (test == 0 || test == 7)
                {
                    part[i] = 0;
                    bit[i] = 0;
                }
                else if (test == 3)
                {
                    part[i] = (selfMantissa << 1);
                    bit[i] = 0;
                }
                else if (test == 4)
                {
                    part[i] = ~(selfMantissa << 1);
                    bit[i] = 1;
                }
                else if (test < 4)
                {
                    part[i] = selfMantissa;
                    bit[i] = 0;
                }
                else
                {
                    part[i] = ~selfMantissa;
                    bit[i] = 1;
                }
            }

            for (int i = 0; i <= 12; i++)
            {
                res += (long)(int)part[i] << (i * 2);
                res &= mask;
                res += bit[i] << (i * 2);
            }
			
            result.Mantissa = (uint)(res >> 23);

            if (result.Mantissa > 0)
            {
                int leadingBitPosition = GetMostSignificantBitPosition(result.Mantissa);

                while (leadingBitPosition != IMPLICIT_LEADING_BIT_POS)
                {
					if (leadingBitPosition > IMPLICIT_LEADING_BIT_POS)
					{
						result.Mantissa >>= 1;
						try
						{
							result.Exponent = checked((byte)(result.Exponent + 1));
						}
						catch (OverflowException)
						{
							return result.Sign ? Min() : Max();
						}
						leadingBitPosition--;
					}
					else if (leadingBitPosition < IMPLICIT_LEADING_BIT_POS)
					{
						result.Mantissa <<= 1;
						try
						{
							result.Exponent = checked((byte)(result.Exponent - 1));
						}
						catch (OverflowException)
						{
							return new Ps2Float(result.Sign, 0, 0);
						}
						leadingBitPosition++;
					}
                }
            }

            result.Mantissa &= 0x7FFFFF;
            return result.RoundTowardsZero();
        }

        // Rounding can be slightly off: https://github.com/PSI-Rockin/DobieStation/issues/453.
        private Ps2Float DoDiv(Ps2Float other)
        {
            uint selfMantissa = Mantissa | 0x800000;
            uint otherMantissa = other.Mantissa | 0x800000;
            int resExponent = Exponent - other.Exponent + BIAS;

            Ps2Float result = new Ps2Float(0) { Sign = DetermineMultiplicationDivisionOperationSign(this, other) };

            if (resExponent >= 255)
                return result.Sign ? Min() : Max();
            else if (resExponent <= 0)
                return new Ps2Float(result.Sign, 0, 0);

            // Perform division and calculate remainder
            long dividend = (long)selfMantissa << 23;
            long divisor = (long)otherMantissa;
            long quotient = dividend / divisor;
            long remainder = dividend % divisor; // Detect remainder

            result.Exponent = (byte)resExponent;
            result.Mantissa = (uint)quotient;

            if (result.Mantissa > 0)
            {
                int leadingBitPosition = GetMostSignificantBitPosition(result.Mantissa);

                while (leadingBitPosition != IMPLICIT_LEADING_BIT_POS)
                {
					if (leadingBitPosition > IMPLICIT_LEADING_BIT_POS)
					{
						result.Mantissa >>= 1;
						try
						{
							result.Exponent = checked((byte)(result.Exponent + 1));
						}
						catch (OverflowException)
						{
							return result.Sign ? Min() : Max();
						}
						leadingBitPosition--;
					}
					else if (leadingBitPosition < IMPLICIT_LEADING_BIT_POS)
					{
						result.Mantissa <<= 1;
						try
						{
							result.Exponent = checked((byte)(result.Exponent - 1));
						}
						catch (OverflowException)
						{
							return new Ps2Float(result.Sign, 0, 0);
						}
						leadingBitPosition++;
					}
                }
            }

            result.Mantissa &= 0x7FFFFF;

            if (remainder != 0)
			{
				try
				{
					result.Mantissa = checked(result.Mantissa + 1);
				}
				catch (OverflowException)
				{
					return result.Sign ? Min() : Max();
				}
			}

            return result.RoundTowardsZero();
        }

        // Rounding can be slightly off: rsqrt(0x7FFFFFF0) -> 0x5FB504ED.
        /// <summary>
        /// Returns the square root of x
        /// </summary>
        public Ps2Float Sqrt()
		{
			int t;
			int s = 0;
			int q = 0;
			uint r = 0x01000000; /* r = moving bit from right to left */

			if (IsDenormalized())
				return new Ps2Float(0);

			// PS2 only takes positive numbers for SQRT, and convert if necessary.
			int ix = (int)new Ps2Float(false, Exponent, Mantissa).AsUInt32();

			/* extract mantissa and unbias exponent */
			int m = (ix >> 23) - BIAS;

			ix = (ix & 0x007fffff) | 0x00800000;
			if ((m & 1) == 1)
			{
				/* odd m, double x to make it even */
				ix += ix;
			}

			m >>= 1; /* m = [m/2] */

			/* generate sqrt(x) bit by bit */
			ix += ix;

			while (r != 0)
			{
				t = s + (int)r;
				if (t <= ix)
				{
					s = t + (int)r;
					ix -= t;
					q += (int)r;
				}

				ix += ix;
				r >>= 1;
			}

			/* use floating add to find out rounding direction */
			if (ix != 0)
			{
				q += q & 1;
			}

			ix = (q >> 1) + 0x3f000000;
			ix += m << 23;

			return new Ps2Float((uint)ix);
		}

        public Ps2Float Rsqrt(Ps2Float other)
        {
            return Div(other.Sqrt());
        }

        public bool IsDenormalized()
		{
			return Exponent == 0;
		}

		public bool IsAbnormal()
		{
			uint val = AsUInt32();
			return val == MAX_FLOATING_POINT_VALUE || val == MIN_FLOATING_POINT_VALUE ||
				   val == POSITIVE_INFINITY_VALUE || val == NEGATIVE_INFINITY_VALUE;
		}

		public bool IsZero()
		{
			return (AsUInt32() & MAX_FLOATING_POINT_VALUE) == 0;
		}

		public Ps2Float RoundTowardsZero()
		{
			return new Ps2Float((uint)Math.Truncate((double)AsUInt32()));
		}

		public int CompareTo(Ps2Float other)
		{
			int selfTwoComplementVal = (int)(AsUInt32() & MAX_FLOATING_POINT_VALUE);
			if (Sign) selfTwoComplementVal = -selfTwoComplementVal;

			int otherTwoComplementVal = (int)(other.AsUInt32() & MAX_FLOATING_POINT_VALUE);
			if (other.Sign) otherTwoComplementVal = -otherTwoComplementVal;

			return selfTwoComplementVal.CompareTo(otherTwoComplementVal);
		}

        private static Ps2Float SolveAbnormalAdditionOrSubtractionOperation(Ps2Float a, Ps2Float b, bool add)
        {
            uint aval = a.AsUInt32();
            uint bval = b.AsUInt32();

            if (aval == MAX_FLOATING_POINT_VALUE && bval == MAX_FLOATING_POINT_VALUE)
                return add ? Max() : new Ps2Float(0);

            if (aval == MIN_FLOATING_POINT_VALUE && bval == MIN_FLOATING_POINT_VALUE)
                return add ? Min() : new Ps2Float(0);

            if (aval == MIN_FLOATING_POINT_VALUE && bval == MAX_FLOATING_POINT_VALUE)
                return add ? Max() : Min();

            if (aval == MAX_FLOATING_POINT_VALUE && bval == MIN_FLOATING_POINT_VALUE)
                return add ? new Ps2Float(0) : Max();

            if (aval == POSITIVE_INFINITY_VALUE && bval == POSITIVE_INFINITY_VALUE)
                return add ? Max() : new Ps2Float(0);

            if (aval == NEGATIVE_INFINITY_VALUE && bval == POSITIVE_INFINITY_VALUE)
                return add ? new Ps2Float(0) : Min();

            if (aval == NEGATIVE_INFINITY_VALUE && bval == NEGATIVE_INFINITY_VALUE)
                return add ? Min() : new Ps2Float(0);

            throw new InvalidOperationException("Unhandled abnormal add/sub floating point operation");
        }

        private static Ps2Float SolveAbnormalMultiplicationOrDivisionOperation(Ps2Float a, Ps2Float b, bool mul)
        {
            uint aval = a.AsUInt32();
            uint bval = b.AsUInt32();

            if (mul)
            {
                if ((aval == MAX_FLOATING_POINT_VALUE && bval == MAX_FLOATING_POINT_VALUE) ||
                    (aval == MIN_FLOATING_POINT_VALUE && bval == MIN_FLOATING_POINT_VALUE))
                    return Max();

                if ((aval == MAX_FLOATING_POINT_VALUE && bval == MIN_FLOATING_POINT_VALUE) ||
                    (aval == MIN_FLOATING_POINT_VALUE && bval == MAX_FLOATING_POINT_VALUE))
                    return Min();

                if (aval == POSITIVE_INFINITY_VALUE && bval == POSITIVE_INFINITY_VALUE)
                    return Max();

                if (aval == NEGATIVE_INFINITY_VALUE && bval == POSITIVE_INFINITY_VALUE)
                    return Min();

                if (aval == POSITIVE_INFINITY_VALUE && bval == NEGATIVE_INFINITY_VALUE)
                    return Min();

                if (aval == NEGATIVE_INFINITY_VALUE && bval == NEGATIVE_INFINITY_VALUE)
                    return Max();
            }
            else
            {
                if ((aval == MAX_FLOATING_POINT_VALUE && bval == MAX_FLOATING_POINT_VALUE) ||
                    (aval == MIN_FLOATING_POINT_VALUE && bval == MIN_FLOATING_POINT_VALUE))
                    return One();

                if ((aval == MAX_FLOATING_POINT_VALUE && bval == MIN_FLOATING_POINT_VALUE) ||
                    (aval == MIN_FLOATING_POINT_VALUE && bval == MAX_FLOATING_POINT_VALUE))
                    return MinOne();

                if (aval == POSITIVE_INFINITY_VALUE && bval == POSITIVE_INFINITY_VALUE)
                    return One();

                if (aval == NEGATIVE_INFINITY_VALUE && bval == POSITIVE_INFINITY_VALUE)
                    return MinOne();

                if (aval == POSITIVE_INFINITY_VALUE && bval == NEGATIVE_INFINITY_VALUE)
                    return MinOne();

                if (aval == NEGATIVE_INFINITY_VALUE && bval == NEGATIVE_INFINITY_VALUE)
                    return One();
            }

            throw new InvalidOperationException("Unhandled abnormal mul/div floating point operation");
        }

        private static Ps2Float SolveAddSubDenormalizedOperation(Ps2Float a, Ps2Float b, bool add)
        {
            Ps2Float result;

            if (a.IsDenormalized() && !b.IsDenormalized())
                result = b;
            else if (!a.IsDenormalized() && b.IsDenormalized())
                result = a;
            else if (a.IsDenormalized() && b.IsDenormalized())
                result = new Ps2Float(0);
            else
                throw new InvalidOperationException("Both numbers are not denormalized");

            result.Sign = add ? DetermineAdditionOperationSign(a, b) : DetermineSubtractionOperationSign(a, b);
            return result;
        }

        private static Ps2Float SolveMultiplicationDenormalizedOperation(Ps2Float a, Ps2Float b)
        {
            return new Ps2Float(0)
            {
                Sign = DetermineMultiplicationDivisionOperationSign(a, b)
            };
        }

        private static Ps2Float SolveDivisionDenormalizedOperation(Ps2Float a, Ps2Float b)
        {
            bool sign = DetermineMultiplicationDivisionOperationSign(a, b);
            Ps2Float result;

            if (a.IsDenormalized() && !b.IsDenormalized())
                result = new Ps2Float(0);
            else if (!a.IsDenormalized() && b.IsDenormalized())
                return sign ? Min() : Max();
            else if (a.IsDenormalized() && b.IsDenormalized())
                return sign ? Min() : Max();
            else
                throw new InvalidOperationException("Both numbers are not denormalized");

            result.Sign = sign;
            return result;
        }

        private static bool DetermineMultiplicationDivisionOperationSign(Ps2Float a, Ps2Float b)
        {
            return a.Sign ^ b.Sign;
        }

        private static bool DetermineAdditionOperationSign(Ps2Float a, Ps2Float b)
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
            return a.Sign;
        }

        private static bool DetermineSubtractionOperationSign(Ps2Float a, Ps2Float b)
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
            return a.CompareTo(b) >= 0 ? a.Sign : !b.Sign;
        }

        private static int GetMostSignificantBitPosition(uint value)
        {
            for (int i = 31; i >= 0; i--)
            {
                if (((value >> i) & 1) != 0)
                    return i;
            }
            return -1;
        }

        public override string ToString()
		{
			double res = (Mantissa / Math.Pow(2, 23) + 1.0) * Math.Pow(2, Exponent - 127.0);
			if (Sign)
				res *= -1.0;

			uint value = AsUInt32();
			if (IsDenormalized())
				return $"Denormalized({res:F2})";
			else if (value == MAX_FLOATING_POINT_VALUE)
				return $"Fmax({res:F2})";
			else if (value == MIN_FLOATING_POINT_VALUE)
				return $"-Fmax({res:F2})";
			else if (value == POSITIVE_INFINITY_VALUE)
				return $"Inf({res:F2})";
			else if (value == NEGATIVE_INFINITY_VALUE)
				return $"-Inf({res:F2})";

			return $"Ps2Float({res:F2})";
		}
	}
}