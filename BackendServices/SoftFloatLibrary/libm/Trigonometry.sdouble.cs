#if NET7_0_OR_GREATER
namespace SoftFloatLibrary
{
    public static partial class libm_sdouble
    {
        const ulong pi = 0x400921fb54442d18; // 3.1415926535897932384626433832795
        const ulong half_pi = 0x3ff921fb54442d18; // 1.5707963267948966192313216916398
        const ulong two_pi = 0x401921fb54442d18; // 6.283185307179586476925286766559
        const ulong pi_over_4 = 0x3fe921fb54442d18; // 0.78539816339744830961566084581988
        const ulong pi_times_3_over_4 = 0x4002d97c7f3321d2; // 2.3561944901923449288469825374596

        /// <summary>
        /// Returns the sine of x
        /// </summary>
        public static sdouble sin(sdouble x)
        {
            const ulong pi_squared_times_five = 0x4053c2295391df8c; // 49.348022005446793094172454999381

            // https://en.wikipedia.org/wiki/Bhaskara_I%27s_sine_approximation_formula
            // sin(x) ~= (16x * (pi - x)) / (5pi^2 - 4x * (pi - x)) if 0 <= x <= pi

            // move x into range
            x %= sdouble.FromRaw(two_pi);
            if (x.IsNegative())
            {
                x += sdouble.FromRaw(two_pi);
            }

            bool negate;
            if (x > sdouble.FromRaw(pi))
            {
                // pi < x <= 2pi, we need to move x to the 0 <= x <= pi range
                // also, we need to negate the result before returning it
                x = sdouble.FromRaw(two_pi) - x;
                negate = true;
            }
            else
            {
                negate = false;
            }

            sdouble piMinusX = sdouble.FromRaw(pi) - x;
            sdouble result = ((sdouble)16.0 * x * piMinusX) / (sdouble.FromRaw(pi_squared_times_five) - (sdouble)4.0 * x * piMinusX);
            return negate ? -result : result;
        }

        /// <summary>
        /// Returns the cosine of x
        /// </summary>
        public static sdouble cos(sdouble x) => sin(x + sdouble.FromRaw(half_pi));

        /// <summary>
        /// Returns the tangent of x
        /// </summary>
        public static sdouble tan(sdouble x) => sin(x) / cos(x);

        /// <summary>
        /// Returns the square root of (x*x + y*y)
        /// </summary>
        public static sdouble hypot(sdouble x, sdouble y)
        {
            sdouble w;

            long ha = (long)x.RawValue;
            ha &= 0x7fffffffffffffff;

            long hb = (long)y.RawValue;
            hb &= 0x7fffffffffffffff;

            if (hb > ha)
            {
                long temp = ha;
                ha = hb;
                hb = temp;
            }

            sdouble a = sdouble.FromRaw((ulong)ha); /* a <- |a| */
            sdouble b = sdouble.FromRaw((ulong)hb); /* b <- |b| */

            if (ha - hb > 0xf00000000000000)
            {
                return a + b;
            } /* x/y > 2**30 */

            ulong k = 0;
            if (ha > 0x5f30000000000000)
            {
                /* a>2**50 */
                if (ha >= 0x7ff0000000000000)
                {
                    /* Inf or NaN */
                    w = a + b; /* for sNaN */
                    if (ha == 0x7ff0000000000000)
                    {
                        w = a;
                    }

                    if (hb == 0x7ff0000000000000)
                    {
                        w = b;
                    }

                    return w;
                }

                /* scale a and b by 2**-60 */
                ha -= 0x5d80000000000000;
                hb -= 0x5d80000000000000;
                k += 60;
                a = sdouble.FromRaw((ulong)ha);
                b = sdouble.FromRaw((ulong)hb);
            }

            if (hb < 0x2680000000000000)
            {
                /* b < 2**-50 */
                if (hb <= 0x0080000000000000)
                {
                    /* subnormal b or 0 */
                    if (hb == 0)
                    {
                        return a;
                    }

                    sdouble t1 = sdouble.FromRaw(0x3ff0000000000000); /* t1=2^126 */
                    b *= t1;
                    a *= t1;
                    k -= 126;
                }
                else
                {
                    /* scale a and b by 2^60 */
                    ha += 0x5d80000000000000; /* a *= 2^60 */
                    hb += 0x5d80000000000000; /* b *= 2^60 */
                    k -= 60;
                    a = sdouble.FromRaw((ulong)ha);
                    b = sdouble.FromRaw((ulong)hb);
                }
            }

            /* medium size a and b */
            w = a - b;
            if (w > b)
            {
                sdouble t1 = sdouble.FromRaw(((ulong)ha) & 0xfffffff000000000);
                sdouble t2 = a - t1;
                w = sqrt(t1 * t1 - (b * (-b) - t2 * (a + t1)));
            }
            else
            {
                a += a;
                sdouble y1 = sdouble.FromRaw(((ulong)hb) & 0xfffffff000000000);
                sdouble y2 = b - y1;
                sdouble t1 = sdouble.FromRaw(((ulong)ha) + 0x0020000000000000);
                sdouble t2 = a - t1;
                w = sqrt(t1 * y1 - (w * (-w) - (t1 * y2 + t2 * b)));
            }

            if (k != 0)
            {
                sdouble t1 = sdouble.FromRaw(0x3ff0000000000000 + (k << 52));
                return t1 * w;
            }
            else
            {
                return w;
            }
        }

        private static readonly ulong[] ATAN_HI = new ulong[4]
        {
            0x3fe16cbe41b19af, // 4.6364760399e-01, /* atan(0.5)hi */
            0x3ff921fb54442d18, // 7.8539812565e-01, /* atan(1.0)hi */
            0x3fef730bd281f69c, // 9.8279368877e-01, /* atan(1.5)hi */
            0x400921fb54442d18, // 1.5707962513e+00, /* atan(inf)hi */
        };

        private static readonly ulong[] ATAN_LO = new ulong[4]
        {
            0x3c9d084a7454ad56, // 5.0121582440e-09, /* atan(0.5)lo */
            0x3cd1d84b707e7c2a, // 3.7748947079e-08, /* atan(1.0)lo */
            0x3ccdf669d73bb48, // 3.4473217170e-08, /* atan(1.5)lo */
            0x3ce0b6aa4b428c8d, // 7.5497894159e-08, /* atan(inf)lo */
        };

        private static readonly ulong[] A_T = new ulong[5]
        {
            0x3fd5555555555555, // 3.3333328366e-01
            0xbfd4cccbcbc5b6cd, // -1.9999158382e-01
            0x3fc999999999999a, // 1.4253635705e-01
            0xbfc2492492000f26, // -1.0648017377e-01
            0x3fac6db6db3fb553, // 6.1687607318e-02
        };

        /// <summary>
        /// Returns the arctangent of x
        /// </summary>
        public unsafe static sdouble atan(sdouble x)
        {
            sdouble z;

            ulong ix = x.RawValue;
            bool sign = (ix >> 63) != 0;
            ix &= 0x7fffffffffffffff;

            if (ix >= 0x403c000000000000)
            {
                /* if |x| >= 2**26 */
                if (x.IsNaN())
                {
                    return x;
                }

                sdouble x1p_120 = sdouble.FromRaw(0x0000800000000000); // 0x1p-120 === 2 ^ (-120)
                z = sdouble.FromRaw(ATAN_HI[3]) + x1p_120;
                return sign ? -z : z;
            }

            int id;
            if (ix < 0x3fd0000000000000)
            {
                /* |x| < 0.4375 */
                if (ix < 0x3e30000000000000)
                {
                    /* |x| < 2**-12 */
                    if (ix < 0x0010000000000000)
                    {
                        /* raise underflow for subnormal x */
                        // force_eval!(x * x);
                    }
                    return x;
                }
                id = -1;
            }
            else
            {
                x = sdouble.Abs(x);
                if (ix < 0x3ff3000000000000)
                {
                    /* |x| < 1.1875 */
                    if (ix < 0x3fe6000000000000)
                    {
                        /*  7/16 <= |x| < 11/16 */
                        x = ((sdouble)2.0 * x - (sdouble)1.0) / ((sdouble)2.0 + x);
                        id = 0;
                    }
                    else
                    {
                        /* 11/16 <= |x| < 19/16 */
                        x = (x - (sdouble)1.0) / (x + (sdouble)1.0);
                        id = 1;
                    }
                }
                else if (ix < 0x4008000000000000)
                {
                    /* |x| < 2.4375 */
                    x = (x - (sdouble)1.5) / ((sdouble)1.0 + (sdouble)1.5 * x);
                    id = 2;
                }
                else
                {
                    /* 2.4375 <= |x| < 2**26 */
                    x = (sdouble)(-1.0) / x;
                    id = 3;
                }
            };

            /* end of argument reduction */
            z = x * x;
            sdouble w = z * z;

            /* break sum from i=0 to 10 aT[i]z**(i+1) into odd and even poly */
            sdouble s1 = z * (sdouble.FromRaw(A_T[0]) + w * (sdouble.FromRaw(A_T[2]) + w * sdouble.FromRaw(A_T[4])));
            sdouble s2 = w * (sdouble.FromRaw(A_T[1]) + w * sdouble.FromRaw(A_T[3]));
            if (id < 0)
            {
                return x - x * (s1 + s2);
            }

            z = sdouble.FromRaw(ATAN_HI[id]) - ((x * (s1 + s2) - sdouble.FromRaw(ATAN_LO[id])) - x);
            return sign ? -z : z;
        }

        /// <summary>
        /// Returns the signed angle between the positive x axis, and the direction (x, y)
        /// </summary>
        public static sdouble atan2(sdouble y, sdouble x)
        {
            if (x.IsNaN() || y.IsNaN())
            {
                return x + y;
            }

            ulong ix = x.RawValue;
            ulong iy = y.RawValue;

            if (ix == 0x3ff0000000000000)
            {
                /* x = 1.0 */
                return atan(y);
            }

            ulong m = ((iy >> 63) & 1) | ((ix >> 62) & 2); /* 2*sign(x)+sign(y) */
            ix &= 0x7fffffffffffffff;
            iy &= 0x7fffffffffffffff;

            const ulong PI_LO_U64 = 0xbf75b85ac0000000; // -8.7422776573e-08

            /* when y = 0 */
            if (iy == 0)
            {
                switch (m)
                {
                    case 0:
                    case 1:
                        return y; /* atan(+-0, +anything) = +-0 */
                    case 2:
                        return sdouble.FromRaw(pi); /* atan(+0, -anything) = pi */
                    case 3:
                    default:
                        return -sdouble.FromRaw(pi); /* atan(-0, -anything) = -pi */
                }
            }

            /* when x = 0 */
            if (ix == 0)
            {
                return (m & 1) != 0 ? -sdouble.FromRaw(half_pi) : sdouble.FromRaw(half_pi);
            }

            /* when x is INF */
            if (ix == 0x7ff0000000000000)
            {
                if (iy == 0x7ff0000000000000)
                {
                    switch (m)
                    {
                        case 0:
                            return sdouble.FromRaw(pi_over_4); /* atan(+INF, +INF) */
                        case 1:
                            return -sdouble.FromRaw(pi_over_4); /* atan(-INF, +INF) */
                        case 2:
                            return sdouble.FromRaw(pi_times_3_over_4); /* atan(+INF, -INF) */
                        case 3:
                        default:
                            return -sdouble.FromRaw(pi_times_3_over_4); /* atan(-INF, -INF) */
                    }
                }
                else
                {
                    switch (m)
                    {
                        case 0:
                            return sdouble.Zero; /* atan(+..., +INF) */
                        case 1:
                            return -sdouble.Zero; /* atan(-..., +INF) */
                        case 2:
                            return sdouble.FromRaw(pi); /* atan(+..., -INF) */
                        case 3:
                        default:
                            return -sdouble.FromRaw(pi); /* atan(-..., -INF) */
                    }
                }
            }

            /* |y/x| > 0x1p26 */
            if (ix + (26UL << 52) < iy || iy == 0x7ff0000000000000)
            {
                return (m & 1) != 0 ? -sdouble.FromRaw(half_pi) : sdouble.FromRaw(half_pi);
            }

            /* z = atan(|y/x|) with correct underflow */
            sdouble z = (m & 2) != 0 && iy + (26UL << 52) < ix
                ? sdouble.Zero /*|y/x| < 0x1p-26, x < 0 */
                : atan(sdouble.Abs(y / x));

            switch (m)
            {
                case 0:
                    return z; /* atan(+,+) */
                case 1:
                    return -z; /* atan(-,+) */
                case 2:
                    return sdouble.FromRaw(pi) - (z - sdouble.FromRaw(PI_LO_U64)); /* atan(+,-) */
                case 3:
                default:
                    return (z - sdouble.FromRaw(PI_LO_U64)) - sdouble.FromRaw(pi); /* atan(-,-) */
            }
        }

        /// <summary>
        /// Returns the arccosine of x
        /// </summary>
        public static sdouble acos(sdouble x)
        {
            const ulong PIO2_HI_U64 = 0x3ff921fb54442d18; // 1.5707962513e+00
            const ulong PIO2_LO_U64 = 0x3de0b6aa4f4b6b26; // 7.5497894159e-08
            const ulong P_S0_U64 = 0x3fc5555555555572; // 1.6666586697e-01
            const ulong P_S1_U64 = 0xbf66c16c16beccbc; // -4.2743422091e-02
            const ulong P_S2_U64 = 0xbef5ca05fb7cee2d; // -8.6563630030e-03
            const ulong Q_S1_U64 = 0xbfef2070d5de919b; // - 7.0662963390e-01

            static sdouble r(sdouble z)
            {
                sdouble p = z * (sdouble.FromRaw(P_S0_U64) + z * (sdouble.FromRaw(P_S1_U64) + z * sdouble.FromRaw(P_S2_U64)));
                sdouble q = (sdouble)1.0 + z * sdouble.FromRaw(Q_S1_U64);
                return p / q;
            }

            sdouble x1p_120 = sdouble.FromRaw(0x0000800000000000); // 0x1p-120 === 2 ^ (-120)

            sdouble z;
            sdouble w;
            sdouble s;
            ulong hx = x.RawValue;
            ulong ix = hx & 0x7fffffffffffffff;

            /* |x| >= 1 or nan */
            if (ix >= 0x3ff0000000000000)
            {
                if (ix == 0x3ff0000000000000)
                {
                    if ((hx >> 63) != 0)
                    {
                        return (sdouble)2.0 * sdouble.FromRaw(PIO2_HI_U64) + x1p_120;
                    }

                    return sdouble.Zero;
                }

                return sdouble.NaN;
            }

            /* |x| < 0.5 */
            if (ix < 0x3fe0000000000000)
            {
                if (ix <= 0x3e30000000000000)
                {
                    /* |x| < 2**-26 */
                    return sdouble.FromRaw(PIO2_HI_U64) + x1p_120;
                }

                return sdouble.FromRaw(PIO2_HI_U64) - (x - (sdouble.FromRaw(PIO2_LO_U64) - x * r(x * x)));
            }

            /* x < -0.5 */
            if ((hx >> 63) != 0)
            {
                z = ((sdouble)1.0 + x) * (sdouble)0.5;
                s = sqrt(z);
                w = r(z) * s - sdouble.FromRaw(PIO2_LO_U64);
                return (sdouble)2.0 * (sdouble.FromRaw(PIO2_HI_U64) - (s + w));
            }

            /* x > 0.5 */
            z = ((sdouble)1.0 - x) * (sdouble)0.5;
            s = sqrt(z);
            hx = s.RawValue;
            sdouble df = sdouble.FromRaw(hx & 0xfffffffff0000000);
            sdouble c = (z - df * df) / (s + df);
            w = r(z) * s + c;
            return (sdouble)2.0 * (df + w);
        }

        /// <summary>
        /// Returns the arcsine of x
        /// </summary>
        public static sdouble asin(sdouble x) => sdouble.FromRaw(half_pi) - acos(x);
    }
}
#endif