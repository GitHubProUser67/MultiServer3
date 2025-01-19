namespace SoftFloatLibrary
{
    public static partial class libm_sdouble
    {
#if NET7_0_OR_GREATER
        /// <summary>
        /// Returns the remainder and the quotient when dividing x by y, so that x == y * quotient + remainder
        /// </summary>
        public static void remquo(sdouble x, sdouble y, out sdouble remainder, out int quotient)
        {
            ulong ux = x.RawValue;
            ulong uy = y.RawValue;
            int ex = (int)((ux >> 52) & 0x7ff);
            int ey = (int)((uy >> 52) & 0x7ff);
            bool sx = (ux >> 63) != 0;
            bool sy = (uy >> 63) != 0;
            ulong q;
            ulong i;
            var uxi = ux;

            if ((uy << 1) == 0 || y.IsNaN() || ex == 0x7ff)
            {
                sdouble m = (x * y);
                remainder = m / m;
                quotient = 0;
                return;
            }

            if ((ux << 1) == 0)
            {
                remainder = x;
                quotient = 0;
                return;
            }

            /* normalize x and y */
            if (ex == 0)
            {
                i = uxi << 11;
                while ((i >> 63) == 0)
                {
                    ex -= 1;
                    i <<= 1;
                }

                uxi <<= -ex + 1;
            }
            else
            {
                uxi &= (~0UL) >> 11;
                uxi |= 1UL << 52;
            }

            if (ey == 0)
            {
                i = uy << 11;
                while ((i >> 63) == 0)
                {
                    ey -= 1;
                    i <<= 1;
                }

                uy <<= -ey + 1;
            }
            else
            {
                uy &= (~0UL) >> 11;
                uy |= 1UL << 52;
            }

            q = 0;
            if (ex + 1 != ey)
            {
                if (ex < ey)
                {
                    remainder = x;
                    quotient = 0;
                    return;
                }

                /* x mod y */
                while (ex > ey)
                {
                    i = uxi - uy;
                    if ((i >> 63) == 0)
                    {
                        uxi = i;
                        q += 1;
                    }

                    uxi <<= 1;
                    q <<= 1;
                    ex -= 1;
                }

                i = uxi - uy;
                if ((i >> 63) == 0)
                {
                    uxi = i;
                    q += 1;
                }

                if (uxi == 0)
                {
                    ex = -60;
                }
                else
                {
                    while ((uxi >> 52) == 0)
                    {
                        uxi <<= 1;
                        ex -= 1;
                    }
                }
            }

            /* scale result and decide between |x| and |x|-|y| */
            if (ex > 0)
            {
                uxi -= 1UL << 52;
                uxi |= ((ulong)ex) << 52;
            }
            else
            {
                uxi >>= -ex + 1;
            }

            x = sdouble.FromRaw(uxi);
            if (sy)
            {
                y = -y;
            }

            if ((ex == ey || (ex + 1 == ey && ((sdouble)2.0 * x > y || ((sdouble)2.0 * x == y && (q % 2) != 0)))) && x > y)
            {
                x -= y;
                q += 1;
            }

            q &= 0x7fffffffffffffffUL;
            int quo = sx ^ sy ? -(int)q : (int)q;
            remainder = sx ? -x : x;
            quotient = quo;
        }

        /// <summary>
        /// Returns the remainder when dividing x by y
        /// </summary>
        public static sdouble remainder(sdouble x, sdouble y)
        {
            remquo(x, y, out sdouble remainder, out _);
            return remainder;
        }

        /// <summary>
        /// Returns x modulo y
        /// </summary>
        public static sdouble fmod(sdouble x, sdouble y)
        {
            ulong uxi = x.RawValue;
            ulong uyi = y.RawValue;
            int ex = (int)(uxi >> 52 & 0x7ff);
            int ey = (int)(uyi >> 52 & 0x7ff);
            ulong sx = uxi & 0x8000000000000000UL;
            ulong i;

            if (uyi << 1 == 0 || y.IsNaN() || ex == 0x7ff)
            {
                return (x * y) / (x * y);
            }

            if (uxi << 1 <= uyi << 1)
            {
                if (uxi << 1 == uyi << 1)
                {
                    // return 0.0 * x;
                    return sdouble.Zero;
                }

                return x;
            }

            /* normalize x and y */
            if (ex == 0)
            {
                i = uxi << 11;
                while (i >> 63 == 0)
                {
                    ex -= 1;
                    i <<= 1;
                }

                uxi <<= -ex + 1;
            }
            else
            {
                uxi &= ulong.MaxValue >> 11;
                uxi |= 1UL << 52;
            }

            if (ey == 0)
            {
                i = uyi << 11;
                while (i >> 63 == 0)
                {
                    ey -= 1;
                    i <<= 1;
                }

                uyi <<= -ey + 1;
            }
            else
            {
                uyi &= ulong.MaxValue >> 11;
                uyi |= 1UL << 52;
            }

            /* x mod y */
            while (ex > ey)
            {
                i = uxi - uyi;
                if (i >> 63 == 0)
                {
                    if (i == 0)
                    {
                        // return 0.0 * x;
                        return sdouble.Zero;
                    }

                    uxi = i;
                }

                uxi <<= 1;

                ex -= 1;
            }

            i = uxi - uyi;
            if (i >> 63 == 0)
            {
                if (i == 0)
                {
                    // return 0.0 * x;
                    return sdouble.Zero;
                }

                uxi = i;
            }

            while (uxi >> 52 == 0)
            {
                uxi <<= 1;
                ex -= 1;
            }

            /* scale result up */
            if (ex > 0)
            {
                uxi -= 1UL << 52;
                uxi |= ((ulong)ex) << 52;
            }
            else
            {
                uxi >>= -ex + 1;
            }

            uxi |= sx;
            return sdouble.FromRaw(uxi);
        }
#endif
        /// <summary>
        /// Rounds x to the nearest integer
        /// </summary>
        public static sdouble round(sdouble x)
        {
            sdouble TOINT = (sdouble)4503599627370496.0; // 2^52

            ulong i = x.RawValue;
            ulong e = (i >> 52) & 0x7ff;
            sdouble y;

            if (e >= 0x3ff + 52)
            {
                return x;
            }

            if (e < 0x3ff - 1)
            {
                // force_eval!(x + TOINT);
                // return 0.0 * x;
                return sdouble.Zero;
            }

            if (i >> 63 != 0)
            {
                x = -x;
            }

            y = x + TOINT - TOINT - x;

            if (y > (sdouble)0.5)
            {
                y = y + x - sdouble.One;
            }
            else if (y <= (sdouble)(-0.5))
            {
                y = y + x + sdouble.One;
            }
            else
            {
                y += x;
            }

            return i >> 63 != 0 ? -y : y;
        }

        /// <summary>
        /// Rounds x down to the nearest integer
        /// </summary>
        public static sdouble floor(sdouble x)
        {
            ulong ui = x.RawValue;
            int e = (((int)(ui >> 52)) & 0x7ff) - 0x3ff;

            if (e >= 52)
            {
                return x;
            }

            if (e >= 0)
            {
                ulong m = 0x000fffffffffffffUL >> e;
                if ((ui & m) == 0)
                {
                    return x;
                }
                if (ui >> 63 != 0)
                {
                    ui += m;
                }
                ui &= ~m;
            }
            else
            {
                if (ui >> 63 == 0)
                {
                    ui = 0;
                }
                else if (ui << 1 != 0)
                {
                    return (sdouble)(-1.0);
                }
            }

            return sdouble.FromRaw(ui);
        }

        /// <summary>
        /// Rounds x up to the nearest integer
        /// </summary>
        public static sdouble ceil(sdouble x)
        {
            ulong ui = x.RawValue;
            int e = (int)(((ui >> 52) & 0x7ff) - (0x3ff));

            if (e >= 52)
            {
                return x;
            }

            if (e >= 0)
            {
                ulong m = 0x000fffffffffffffUL >> e;
                if ((ui & m) == 0)
                {
                    return x;
                }
                if (ui >> 63 == 0)
                {
                    ui += m;
                }
                ui &= ~m;
            }
            else
            {
                if (ui >> 63 != 0)
                {
                    return (sdouble)(-0.0);
                }
                else if (ui << 1 != 0)
                {
                    return sdouble.One;
                }
            }

            return sdouble.FromRaw(ui);
        }

        /// <summary>
        /// Truncates x, removing its fractional parts
        /// </summary>
        public static sdouble trunc(sdouble x)
        {
            ulong i = x.RawValue;
            int e = (int)(i >> 52 & 0x7ff) - 0x3ff + 12;
            ulong m;

            if (e >= 52 + 12)
            {
                return x;
            }

            if (e < 12)
            {
                e = 1;
            }

            m = unchecked((ulong)-1) >> e;
            if ((i & m) == 0)
            {
                return x;
            }

            i &= ~m;
            return sdouble.FromRaw(i);
        }

        /// <summary>
        /// Returns the square root of x
        /// </summary>
        public static sdouble sqrt(sdouble x)
        {
            int sign = unchecked((int)0x8000000000000000);
            long ix;
            long s;
            long q;
            long m;
            long t;
            long i;
            ulong r;

            ix = (long)x.RawValue;

            /* take care of Inf and NaN */
            if (((ulong)ix & 0x7ff0000000000000) == 0x7ff0000000000000)
            {
                // return x * x + x; /* sqrt(NaN)=NaN, sqrt(+inf)=+inf, sqrt(-inf)=sNaN */
                if (x.IsNaN() || x.IsNegativeInfinity())
                {
                    return sdouble.NaN;
                }
                else // if (x.IsPositiveInfinity())
                {
                    return sdouble.PositiveInfinity;
                }
            }

            /* take care of zero */
            if (ix <= 0)
            {
                if ((ix & ~sign) == 0)
                {
                    return x; /* sqrt(+-0) = +-0 */
                }

                if (ix < 0)
                {
                    // return (x - x) / (x - x); /* sqrt(-ve) = sNaN */
                    return sdouble.NaN;
                }
            }

            /* normalize x */
            m = ix >> 52;
            if (m == 0)
            {
                /* subnormal x */
                i = 0;
                while ((ix & 0x0010000000000000) == 0)
                {
                    ix <<= 1;
                    i += 1;
                }

                m -= i - 1;
            }

            m -= 1023; /* unbias exponent */
            ix = (ix & 0x000fffffffffffff) | 0x0010000000000000;
            if ((m & 1) == 1)
            {
                /* odd m, double x to make it even */
                ix += ix;
            }

            m >>= 1; /* m = [m/2] */

            /* generate sqrt(x) bit by bit */
            ix += ix;
            q = 0;
            s = 0;
            r = 0x0010000000000000; /* r = moving bit from right to left */

            while (r != 0)
            {
                t = s + (long)r;
                if (t <= ix)
                {
                    s = t + (long)r;
                    ix -= t;
                    q += (long)r;
                }

                ix += ix;
                r >>= 1;
            }

            /* use floating add to find out rounding direction */
            if (ix != 0)
            {
                q += q & 1;
            }

            ix = (q >> 1) + 0x3fe0000000000000;
            ix += m << 52;
            return sdouble.FromRaw((ulong)ix);
        }
    }
}