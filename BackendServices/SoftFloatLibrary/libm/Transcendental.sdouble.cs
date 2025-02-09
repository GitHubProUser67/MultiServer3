#if NET7_0_OR_GREATER
namespace SoftFloatLibrary
{
    public static partial class libm_sdouble
    {
        private static sdouble scalbn(sdouble x, long n)
        {
            sdouble x1p1023 = sdouble.FromRaw(0x7fe0000000000000); // 2^1023
            sdouble x1p_1022 = sdouble.FromRaw(0x0010000000000000); // 2^-1022
            sdouble x1p53 = sdouble.FromRaw(0x4340000000000000); // 2^53

            if (n > 1023)
            {
                x *= x1p1023;
                n -= 1023;
                if (n > 1023)
                {
                    x *= x1p1023;
                    n -= 1023;
                    if (n > 1023)
                    {
                        n = 1023;
                    }
                }
            }
            else if (n < -1022)
            {
                x *= x1p_1022 * x1p53;
                n += 1022 - 53;
                if (n < -1022)
                {
                    x *= x1p_1022 * x1p53;
                    n += 1022 - 53;
                    if (n < -1022)
                    {
                        n = -1022;
                    }
                }
            }

            return x * sdouble.FromRaw(((ulong)(0x3ff + n)) << 52);
        }

        /// <summary>
        /// Returns e raised to the power x (e ~= 2.71828182845904523536)
        /// </summary>
        public static sdouble exp(sdouble x)
        {
            const ulong LN2_HI_U64 = 0x3fe62e42fee00000; // 6.9314718036912381649e-01
            const ulong LN2_LO_U64 = 0x3dea39ef35793c76; // 1.9082149292705877e-10
            const ulong INV_LN2_U64 = 0x3ff71547652b82fe; // 1.4426950408889634074e+00

            const ulong P1_U64 = 0x3fc5555555555555; // 0.16666666666666666
            const ulong P2_U64 = 0x3fa5555555555555; // -2.7667332906e-3

            sdouble x1p1023 = sdouble.FromRaw(0x7fe0000000000000); // 2^1023
            sdouble x1p_1022 = sdouble.FromRaw(0x0010000000000000); // 2^-1022
            ulong hx = x.RawValue;
            int sign = (int)(hx >> 63); // sign bit of x
            bool signb = sign != 0;
            hx &= 0x7fffffffffffffff; // high word of |x|

            // special cases
            if (hx >= 0x40862e42fee00000)
            {
                // if |x| >= -708.39 or NaN
                if (hx > 0x7ff0000000000000)
                {
                    // NaN
                    return x;
                }

                if (hx >= 0x408630d1533a2800 && !signb)
                {
                    // x >= 709.78
                    // overflow
                    x *= x1p1023;
                    return x;
                }

                if (signb)
                {
                    // underflow
                    if (hx >= 0x409bdf8d2beab400)
                    {
                        // x <= -746.2
                        return sdouble.Zero;
                    }
                }
            }

            // argument reduction
            int k;
            sdouble hi;
            sdouble lo;
            if (hx > 0x3fd62e42fefa39ef)
            {
                // if |x| > 0.5 ln2
                if (hx > 0x3ff0a2b23f876d8a)
                {
                    // if |x| > 1.5 ln2
                    k = (int)(sdouble.FromRaw(INV_LN2_U64) * x + (signb ? (sdouble)0.5f : (sdouble)(-0.5f)));
                }
                else
                {
                    k = 1 - sign - sign;
                }

                sdouble kf = (sdouble)k;
                hi = x - kf * sdouble.FromRaw(LN2_HI_U64); // k*ln2hi is exact here
                lo = kf * sdouble.FromRaw(LN2_LO_U64);
                x = hi - lo;
            }
            else if (hx > 0x3e30000000000000)
            {
                // |x| > 2^-28
                k = 0;
                hi = x;
                lo = sdouble.Zero;
            }
            else
            {
                // raise inexact
                return sdouble.One + x;
            }

            // x is now in primary range
            sdouble xx = x * x;
            sdouble c = x - xx * (sdouble.FromRaw(P1_U64) + xx * sdouble.FromRaw(P2_U64));
            sdouble y = sdouble.One + (x * c / ((sdouble)2.0 - c) - lo + hi);
            return k == 0 ? y : scalbn(y, k);
        }

        /// <summary>
        /// Returns the natural logarithm (base e) of x
        /// </summary>
        public static sdouble log(sdouble x)
        {
            const ulong LN2_HI_U64 = 0x3fe62e42fefa3800; // 6.9314718036912381649e-01
            const ulong LN2_LO_U64 = 0xbcaae9b198587d6d; // 1.9082149292705877e-10

            const ulong LG1_U64 = 0x3fd5555555555800; // 0.6666666666666666
            const ulong LG2_U64 = 0x3fc99999999973e0; // 0.3999999999940942
            const ulong LG3_U64 = 0x3fc2492494229359; // 0.28571428576433174
            const ulong LG4_U64 = 0x3fbc71c6fea0fae6; // 0.2222222222222222

            ulong ix = x.RawValue;
            int k = 0;

            if ((ix < 0x0010000000000000) || ((ix >> 63) != 0))
            {
                // x < 2**-1022
                if (ix << 1 == 0)
                {
                    return sdouble.NegativeInfinity; // log(+-0)=-inf
                }

                if ((ix >> 63) != 0)
                {
                    return sdouble.NaN; // log(-#) = NaN
                }

                // subnormal number, scale up x
                sdouble x1p54 = sdouble.FromRaw(0x4350000000000000); // 2^54
                k -= 54;
                x *= x1p54;
                ix = x.RawValue;
            }
            else if (ix >= 0x7ff0000000000000)
            {
                return x;
            }
            else if (ix == 0x3ff0000000000000)
            {
                return sdouble.Zero;
            }

            // reduce x into [sqrt(2)/2, sqrt(2)]
            ix += 0x3ff0000000000000 - 0x3fe6a09e667f3bcd;
            k += ((int)(ix >> 52)) - 0x3ff;
            ix = (ix & 0x000fffffffffffff) + 0x3fe6a09e667f3bcd;
            x = sdouble.FromRaw(ix);

            sdouble f = x - sdouble.One;
            sdouble s = f / ((sdouble)2.0 + f);
            sdouble z = s * s;
            sdouble w = z * z;
            sdouble t1 = w * (sdouble.FromRaw(LG2_U64) + w * sdouble.FromRaw(LG4_U64));
            sdouble t2 = z * (sdouble.FromRaw(LG1_U64) + w * sdouble.FromRaw(LG3_U64));
            sdouble r = t2 + t1;
            sdouble hfsq = (sdouble)0.5 * f * f;
            sdouble dk = (sdouble)k;

            return s * (hfsq + r) + dk * sdouble.FromRaw(LN2_LO_U64) - hfsq + f + dk * sdouble.FromRaw(LN2_HI_U64);
        }

        /// <summary>
        /// Returns the base 2 logarithm of x
        /// </summary>
        public static sdouble log2(sdouble x)
{
            const ulong IVLN2HI_U64 = 0x3ff7154765200000; // 1.4426950408889634074
            const ulong IVLN2LO_U64 = 0x3de7054b26800000; // 1.9259629911266174688e-8

            const ulong LG1_U64 = 0x3fd5555555555800; // 0.6666666666666666
            const ulong LG2_U64 = 0x3fc99999999973e0; // 0.3999999999940942
            const ulong LG3_U64 = 0x3fc2492494229359; // 0.28571428576433174
            const ulong LG4_U64 = 0x3fbc71c6fea0fae6; // 0.2222222222222222

            sdouble x1p54 = sdouble.FromRaw(0x4350000000000000); // 2^54
            ulong ui = x.RawValue;
            sdouble hfsq;
            sdouble f;
            sdouble s;
            sdouble z;
            sdouble r;
            sdouble w;
            sdouble t1;
            sdouble t2;
            sdouble hi;
            sdouble lo;
            ulong ix;
            int k;

            ix = ui;
            k = 0;
            if (ix < 0x0010000000000000 || (ix >> 63) > 0)
            {
                // x < 2^(-1022)
                if (ix << 1 == 0)
                {
                    return sdouble.NegativeInfinity; // log(+-0)=-inf
                }

                if ((ix >> 63) > 0)
                {
                    return sdouble.NaN; // log(-#) = NaN
                }

                // subnormal number, scale up x
                k -= 54;
                x *= x1p54;
                ui = x.RawValue;
                ix = ui;
            }
            else if (ix >= 0x7ff0000000000000)
            {
                return x;
            }
            else if (ix == 0x3ff0000000000000)
            {
                return sdouble.Zero;
            }

            // reduce x into [sqrt(2)/2, sqrt(2)]
            ix += 0x3ff0000000000000 - 0x3fe6a09e667f3bcd;
            k += (int)(ix >> 52) - 0x3ff;
            ix = (ix & 0x000fffffffffffff) + 0x3fe6a09e667f3bcd;
            ui = ix;
            x = sdouble.FromRaw(ui);

            f = x - sdouble.One;
            s = f / ((sdouble)2.0 + f);
            z = s * s;
            w = z * z;
            t1 = w * (sdouble.FromRaw(LG2_U64) + w * sdouble.FromRaw(LG4_U64));
            t2 = z * (sdouble.FromRaw(LG1_U64) + w * sdouble.FromRaw(LG3_U64));
            r = t2 + t1;
            hfsq = (sdouble)0.5 * f * f;

            hi = f - hfsq;
            ui = hi.RawValue;
            ui &= 0xfffffffff8000000;
            hi = sdouble.FromRaw(ui);
            lo = f - hi - hfsq + s * (hfsq + r);
            return (lo + hi) * sdouble.FromRaw(IVLN2LO_U64) + lo * sdouble.FromRaw(IVLN2HI_U64) + hi * sdouble.FromRaw(IVLN2HI_U64) + (sdouble)k;
        }

        /// <summary>
        /// Returns x raised to the power y
        /// </summary>
        public static sdouble pow(sdouble x, sdouble y)
        {
            const ulong BP_0_U64 = 0x3ff0000000000000; // 1.0
            const ulong BP_1_U64 = 0x3ff8000000000000; // 1.5
            const ulong DP_H_0_U64 = 0x0000000000000000; // 0.0
            const ulong DP_H_1_U64 = 0x3fe0000000000000; // 5.84960938e-01
            const ulong DP_L_0_U64 = 0x0000000000000000; // 0.0
            const ulong DP_L_1_U64 = 0x3e71c71c71c71c00; // 1.56322085e-06
            const ulong TWO54_U64 = 0x4350000000000000; // 2^54
            const ulong HUGE_U64 = 0x7fe0000000000000; // 1.0e+300
            const ulong TINY_U64 = 0x0010000000000000; // 1.0e-300
            const ulong L1_U64 = 0x3fe5555555555536; // 0.599999999999993
            const ulong L2_U64 = 0x3fd999999997fa04; // 0.4285714285785502
            const ulong L3_U64 = 0x3fd249249422932d; // 0.3333333333711996
            const ulong L4_U64 = 0x3fcc71c51d8e78af; // 0.2727272516307515
            const ulong L5_U64 = 0x3fc7466496cb03de; // 0.230836859642324
            const ulong L6_U64 = 0x3fc39a09d078c69f; // 0.206945051938914
            const ulong P1_U64 = 0x3fc555555555aaaa; // 0.16666666666666666
            const ulong P2_U64 = 0x3fa55555555422d1; // -2.7777777777015593e-3
            const ulong P3_U64 = 0x3f564b2bd656619b; // 6.6137562131e-05
            const ulong P4_U64 = 0xbf66c16c16bebd6d; // -1.6533901999e-06
            const ulong P5_U64 = 0x3ef355555555550e; // 4.1381369442e-08
            const ulong LG2_U64 = 0x3fe62e42fe000000; // 6.9314718055994530E-01
            const ulong LG2_H_U64 = 0x3fe62e42fefa3800; // 6.9314718036912381649e-01
            const ulong LG2_L_U64 = 0xbcaae9b198587d6d; // 1.9082149292705877e-10
            const ulong OVT_U64 = 0x3ff0000000000000; // -(128-log2(ovfl+.5ulp))
            const ulong CP_U64 = 0x3fe6216472280000; // 9.6179670095e-01
            const ulong CP_H_U64 = 0x3fe6216400000000; // 9.6191406250000000E-01
            const ulong CP_L_U64 = 0xbf61a60000000001; // -1.1736857402e-04
            const ulong IVLN2_U64 = 0x3ff71547652b82fe; // 1.4426950408889634074
            const ulong IVLN2_H_U64 = 0x3ff7154765200000; // 1.4426950216e+00
            const ulong IVLN2_L_U64 = 0x3de7054b26800000; // 1.9259629911266174688e-8

            sdouble z;
            sdouble ax;
            sdouble z_h;
            sdouble z_l;
            sdouble p_h;
            sdouble p_l;
            sdouble y1;
            sdouble t1;
            sdouble t2;
            sdouble r;
            sdouble s;
            sdouble sn;
            sdouble t;
            sdouble u;
            sdouble v;
            sdouble w;
            long i;
            long j;
            long k;
            long yisint;
            long n;
            long hx;
            long hy;
            long ix;
            long iy;
            long iS;

            hx = (long)x.RawValue;
            hy = (long)y.RawValue;

            ix = hx & 0x7fffffff;
            iy = hy & 0x7fffffff;

            // x**0 = 1, even if x is NaN
            if (iy == 0)
            {
                return sdouble.One;
            }

            // 1**y = 1, even if y is NaN */
            if (hx == 0x3ff0000000000000)
            {
                return sdouble.One;
            }

            // NaN if either arg is NaN
            if (ix > 0x7ff0000000000000 || iy > 0x7ff0000000000000)
            {
                return sdouble.NaN;
            }

            // determine if y is an odd int when x < 0
            // yisint = 0       ... nothing special
            // yisint = 1       ... y is an odd int
            // yisint = 2       ... y is an even int
            yisint = 0;
            if (hx < 0)
            {
                if (iy >= 0x4330000000000000)
                {
                    yisint = 2; // even integer y
                }
                else if (iy >= 0x3ff0000000000000)
                {
                    k = (iy >> 52) - 0x3ff; // exponent
                    j = iy >> (int)(52 - k);
                    if (j << (int)(52 - k) == iy)
                    {
                        yisint = 2 - (j & 1);
                    }
                }
            }

            // special value of y
            if (iy == 0x7ff0000000000000)
            {
                // y is +-inf
                if (ix == 0x3ff0000000000000)
                {
                    // (-1)**+-inf is 1
                    return sdouble.One;
                }
                else if (ix > 0x3ff0000000000000)
                {
                    // (|x|>1)**+-inf = inf,0
                    return hy >= 0 ? y : sdouble.Zero;
                }
                else
                {
                    // (|x|<1)**+-inf = 0,inf
                    return hy >= 0 ? sdouble.Zero : -y;
                }
            }

            if (iy == 0x3ff0000000000000)
            {
                // y is +-1
                return hy >= 0 ? x : sdouble.One / x;
            }

            if (hy == 0x4000000000000000)
            {
                // y is 2
                return x * x;
            }

            if (hy == 0x3fe0000000000000
               // y is 0.5 and x >= 0
               && hx >= 0)
            {
                return sqrt(x);
            }

            ax = sdouble.Abs(x);
            // special value of x
            if (ix == 0x7ff0000000000000 || ix == 0 || ix == 0x3ff0000000000000)
            {
                // x is +-0, +-inf, +-1
                z = ax;
                if (hy < 0)
                {
                    z = sdouble.One / z; // z = (1/|x|)
                }

                if (hx < 0)
                {
                    if (((ix - 0x3ff0000000000000) | yisint) == 0)
                    {
                        z = (z - z) / (z - z); // (-1)**non-int = NaN
                    }
                    else if (yisint == 1)
                    {
                        z = -z; // (x < 0)**odd = -(|x|**odd)
                    }
                }

                return z;
            }

            sn = sdouble.One; // sign of result
            if (hx < 0)
            {
                if (yisint == 0)
                {
                    return sdouble.NaN; // (x<0)**(non-int) = NaN
                }

                if (yisint == 1)
                {
                    sn = -sdouble.One; // (x<0)**(odd int)
                }
            }

            // |y| is HUGE
            if (iy > 0x4330000000000000)
            {
                // if |y| > 2**52
                // over/underflow if x is not close to one
                if (ix < 0x3fefffff00000000)
                {
                    return hy < 0
                        ? sn * sdouble.FromRaw(HUGE_U64) * sdouble.FromRaw(HUGE_U64)
                        : sn * sdouble.FromRaw(TINY_U64) * sdouble.FromRaw(TINY_U64);
                }

                if (ix > 0x3ff00000f0000000)
                {
                    return hy > 0
                        ? sn * sdouble.FromRaw(HUGE_U64) * sdouble.FromRaw(HUGE_U64)
                        : sn * sdouble.FromRaw(TINY_U64) * sdouble.FromRaw(TINY_U64);
                }

                // now |1-x| is TINY <= 2**-20, suffice to compute
                // log(x) by x-x^2/2+x^3/3-x^4/4
                t = ax - sdouble.One; // t has 20 trailing zeros
                w = (t * t) * (sdouble.FromRaw(0x3fc9999999999999) - t * (sdouble.FromRaw(0x3fd5555555555555) - t * sdouble.FromRaw(0x3fd0000000000000)));
                u = sdouble.FromRaw(IVLN2_H_U64) * t; // IVLN2_H_U64 has 16 sig. bits
                v = t * sdouble.FromRaw(IVLN2_L_U64) - w * sdouble.FromRaw(IVLN2_U64);
                t1 = u + v;
                iS = (int)t1.RawValue;
                t1 = sdouble.FromRaw((ulong)iS & 0xfffffffff8000000);
                t2 = v - (t1 - u);
            }
            else
            {
                sdouble s2;
                sdouble s_h;
                sdouble s_l;
                sdouble t_h;
                sdouble t_l;

                n = 0;
                // take care subnormal number
                if (ix < 0x0010000000000000)
                {
                    ax *= sdouble.FromRaw(TWO54_U64);
                    n -= 54;
                    ix = (int)ax.RawValue;
                }

                n += ((ix) >> 52) - 0x3ff;
                j = ix & 0x000fffffffffffff;
                // determine interval
                ix = j | 0x3ff0000000000000; // normalize ix
                if (j <= 0x3988e14000000000)
                {
                    // |x| < sqrt(3/2)
                    k = 0;
                }
                else if (j < 0x3ef162e400000000)
                {
                    // |x| < sqrt(3)
                    k = 1;
                }
                else
                {
                    k = 0;
                    n += 1;
                    ix -= 0x0010000000000000;
                }

                ax = sdouble.FromRaw((ulong)ix);

                // compute s = s_h+s_l = (x-1)/(x+1) or (x-1.5)/(x+1.5)
                u = ax - sdouble.FromRaw(k == 0 ? BP_0_U64 : BP_1_U64); // bp[0]=1.0, bp[1]=1.5
                v = sdouble.One / (ax + sdouble.FromRaw(k == 0 ? BP_0_U64 : BP_1_U64));
                s = u * v;
                s_h = s;
                iS = (int)s_h.RawValue;
                s_h = sdouble.FromRaw((ulong)iS & 0xfffffffff8000000);

                // t_h=ax+bp[k] High
                iS = (int)((((ulong)ix >> 1) & 0xfffffffff8000000) | 0x40000000);
                t_h = sdouble.FromRaw((ulong)iS + 0x800000000000000 + (((ulong)k) << 20));
                t_l = ax - (t_h - sdouble.FromRaw(k == 0 ? BP_0_U64 : BP_1_U64));
                s_l = v * ((u - s_h * t_h) - s_h * t_l);

                // compute log(ax)
                s2 = s * s;
                r = s2 * s2 * (sdouble.FromRaw(L1_U64) + s2 * (sdouble.FromRaw(L2_U64) + sdouble.FromRaw(L3_U64) + sdouble.FromRaw(L4_U64) + (sdouble.FromRaw(L5_U64) + s2 * sdouble.FromRaw(L6_U64))));
                r += s_l * (s_h + s);
                s2 = s_h * s_h;
                t_h = sdouble.FromRaw(0x3ff0000000000000) + s2 + r;
                iS = (int)t_h.RawValue;
                t_h = sdouble.FromRaw((ulong)iS & 0xfffffffff8000000);
                t_l = r - ((t_h - sdouble.FromRaw(0x3ff0000000000000)) -                s2);

                // u+v = s*(1+...)
                u = s_h * t_h;
                v = s_l * t_h + t_l * s;

                // 2/(3log2)*(s+...)
                p_h = u + v;
                iS = (int)p_h.RawValue;
                p_h = sdouble.FromRaw((ulong)iS & 0xfffffffff8000000);
                p_l = v - (p_h - u);
                z_h = sdouble.FromRaw(CP_H_U64) * p_h; // cp_h + cp_l = 2/(3*log2)
                z_l = sdouble.FromRaw(CP_L_U64) * p_h + p_l * sdouble.FromRaw(CP_U64) + sdouble.FromRaw(k == 0 ? DP_L_0_U64 : DP_L_1_U64);

                // log2(ax) = (s+..) * 2/(3*log2) = n + dp_h + z_h + z_l
                t = (sdouble)n;
                t1 = ((z_h + z_l) + sdouble.FromRaw(k == 0 ? DP_H_0_U64 : DP_H_1_U64)) + t;
                iS = (int)t1.RawValue;
                t1 = sdouble.FromRaw((ulong)iS & 0xfffffffff8000000);
                t2 = z_l - (((t1 - t) - sdouble.FromRaw(k == 0 ? DP_H_0_U64 : DP_H_1_U64)) - z_h);
            };

            // split up y into y1 + y2 and compute (y1 + y2)*(t1 + t2)
            iS = (long)y.RawValue;
            y1 = sdouble.FromRaw((ulong)iS & 0xfffffffff8000000);
            p_l = (y - y1) * t1 + y * t2;
            p_h = y1 * t1;
            z = p_l + p_h;
            j = (long)z.RawValue;
            if (j > 0x40862e42fefa3800)
            {
                // if z > 709.78
                return sn * sdouble.FromRaw(HUGE_U64) * sdouble.FromRaw(HUGE_U64); // overflow
            }
            else if (j == 0x40862e42fefa3800)
            {
                // if z == 708.39
                if (p_l + sdouble.FromRaw(OVT_U64) > z - p_h)
                {
                    return sn * sdouble.FromRaw(HUGE_U64) * sdouble.FromRaw(HUGE_U64); // overflow
                }
            }
            else if ((j & 0x7fffffffffffffff) > 0x4096b00000000000)
            {
                // z < -745.13
                // FIXME: check should be  (uint32_t)j > 0x8000000000000000
                return sn * sdouble.FromRaw(TINY_U64) * sdouble.FromRaw(TINY_U64); // underflow
            }
            else if ((ulong)j == 0x8000000000000000
                    // z == -745.13
                    && p_l <= z - p_h)
            {
                return sn * sdouble.FromRaw(TINY_U64) * sdouble.FromRaw(TINY_U64); // underflow
            }

            /*
             * compute 2**(p_h + p_l)
             */
            i = j & 0x7fffffffffffffff;
            k = (i >> 52) - 0x3ff;
            n = 0;
            if (i > 0x3fe0000000000000)
            {
                // if |z| > 0.5, set n = [z + 0.5]
                n = j + (0x0010000000000000 >> (int)(k + 1));
                k = ((n & 0x7fffffffffffffff) >> 52) - 0x3ff; // new k for n
                t = sdouble.FromRaw((ulong)n & (ulong)(~(0x000fffffffffffff >> (int)k)));
                n = ((n & 0x000fffffffffffff) | 0x0010000000000000) >> (int)(52 - k);
                if (j < 0)
                {
                    n = -n;
                }
                p_h -= t;
            }

            t = p_l + p_h;
            iS = (int)t.RawValue;
            t = sdouble.FromRaw((ulong)iS & 0xfffffffff8000000);
            u = t * sdouble.FromRaw(LG2_H_U64);
            v = (p_l - (t - p_h)) * sdouble.FromRaw(LG2_U64) + t * sdouble.FromRaw(LG2_L_U64);
            z = u + v;
            w = v - (z - u);
            t = z * z;
            t1 = z - t * (sdouble.FromRaw(P1_U64) + t * (sdouble.FromRaw(P2_U64) + t * (sdouble.FromRaw(P3_U64) + t * (sdouble.FromRaw(P4_U64) + t * sdouble.FromRaw(P5_U64)))));
            r = (z * t1) / (t1 - sdouble.FromRaw(0x4000000000000000)) - (w + z * w);
            z = sdouble.One - (r - z);
            j = (int)z.RawValue;
            j += n << 52;
            if ((j >> 52) <= 0x3fe)
            {
                // subnormal output
                z = scalbn(z, n);
            }
            else
            {
                z = sdouble.FromRaw((ulong)j);
            }

            return sn * z;
        }
    }
}
#endif