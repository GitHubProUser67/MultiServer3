#if NET5_0_OR_GREATER
using ILGPU;
using ILGPU.Runtime;

namespace CastleLibrary.ILGPUModels;

public unsafe struct Sha256
{
    /**************************** VARIABLES *****************************/
    static readonly uint[] K =
    {
        0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5, 0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
        0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3, 0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
        0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc, 0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
        0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7, 0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
        0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13, 0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
        0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3, 0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
        0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5, 0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
        0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208, 0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
    };

    /****************************** FIELDS ******************************/
    private fixed uint state[8];
    private fixed byte data[64];
    private uint dataLen;
    private ulong bitlen;

    /****************************** MACROS ******************************/
    private static uint Rotr(uint x, int n) => (x >> n) | (x << (32 - n));
    private static uint Ch(uint x, uint y, uint z) => (x & (y ^ z)) ^ z;
    private static uint Maj(uint x, uint y, uint z) => (x & (y | z)) | (y & z);
    private static uint Ep0(uint x) => Rotr(x, 2) ^ Rotr(x, 13) ^ Rotr(x, 22);
    private static uint Ep1(uint x) => Rotr(x, 6) ^ Rotr(x, 11) ^ Rotr(x, 25);
    private static uint Sig0(uint x) => Rotr(x, 7) ^ Rotr(x, 18) ^ (x >> 3);
    private static uint Sig1(uint x) => Rotr(x, 17) ^ Rotr(x, 19) ^ (x >> 10);

    /*********************** FUNCTION DEFINITIONS ***********************/
    private void Transform(ArrayView<uint> k)
    {
        uint a, b, c, d, e, f, g, h, i, j, t1, t2;
        uint[] m = new uint[64];

        for (i = 0, j = 0; i < 16; ++i, j += 4)
            m[i] = (uint)((data[j] << 24) | (data[j + 1] << 16) | (data[j + 2] << 8) |
                          data[j + 3]);

        for (; i < 64; ++i)
            m[i] = Sig1(m[i - 2]) + m[i - 7] + Sig0(m[i - 15]) + m[i - 16];

        a = state[0];
        b = state[1];
        c = state[2];
        d = state[3];
        e = state[4];
        f = state[5];
        g = state[6];
        h = state[7];

        for (i = 0; i < 64; ++i)
        {
            t1 = h + Ep1(e) + Ch(e, f, g) + k[(long)i] + m[i];
            t2 = Ep0(a) + Maj(a, b, c);
            h = g;
            g = f;
            f = e;
            e = d + t1;
            d = c;
            c = b;
            b = a;
            a = t1 + t2;
        }

        state[0] += a;
        state[1] += b;
        state[2] += c;
        state[3] += d;
        state[4] += e;
        state[5] += f;
        state[6] += g;
        state[7] += h;
    }

    public Sha256()
    {
        dataLen = 0;
        bitlen = 0;
        state[0] = 0x6a09e667;
        state[1] = 0xbb67ae85;
        state[2] = 0x3c6ef372;
        state[3] = 0xa54ff53a;
        state[4] = 0x510e527f;
        state[5] = 0x9b05688c;
        state[6] = 0x1f83d9ab;
        state[7] = 0x5be0cd19;
    }

    public void Update(ArrayView<byte> payload, ArrayView<uint> k)
    {
        for (long i = 0; i < payload.Length; ++i)
        {
            data[dataLen] = payload[i];
            dataLen++;
            if (dataLen == 64)
            {
                Transform(k);
                bitlen += 512;
                dataLen = 0;
            }
        }
    }

    public void Final(ArrayView<byte> hash, ArrayView<uint> k)
    {
        int i = (int)dataLen;

        // Pad whatever data is left in the buffer.
        if (dataLen < 56)
        {
            data[i++] = 0x80;
            while (i < 56)
                data[i++] = 0x00;
        }
        else
        {
            data[i++] = 0x80;
            while (i < 64)
                data[i++] = 0x00;
            Transform(k);
            for (var _ = 0; _ < 56; _++) data[_] = 0;
        }

        // Append to the padding the total message's length in bits and transform.
        bitlen += dataLen * 8;
        data[63] = (byte)(bitlen);
        data[62] = (byte)(bitlen >> 8);
        data[61] = (byte)(bitlen >> 16);
        data[60] = (byte)(bitlen >> 24);
        data[59] = (byte)(bitlen >> 32);
        data[58] = (byte)(bitlen >> 40);
        data[57] = (byte)(bitlen >> 48);
        data[56] = (byte)(bitlen >> 56);
        Transform(k);

        // Since implementation uses little endian byte ordering and SHA uses big endian,
        // reverse all the bytes when copying the final state to the output hash.
        for (i = 0; i < 4; ++i)
        {
            hash[i] = (byte)((state[0] >> (24 - i * 8)) & 0x000000ff);
            hash[i + 4] = (byte)((state[1] >> (24 - i * 8)) & 0x000000ff);
            hash[i + 8] = (byte)((state[2] >> (24 - i * 8)) & 0x000000ff);
            hash[i + 12] = (byte)((state[3] >> (24 - i * 8)) & 0x000000ff);
            hash[i + 16] = (byte)((state[4] >> (24 - i * 8)) & 0x000000ff);
            hash[i + 20] = (byte)((state[5] >> (24 - i * 8)) & 0x000000ff);
            hash[i + 24] = (byte)((state[6] >> (24 - i * 8)) & 0x000000ff);
            hash[i + 28] = (byte)((state[7] >> (24 - i * 8)) & 0x000000ff);
        }
    }

    static void Kernel(Index1D index, ArrayView<uint> k, ArrayView<byte> data, ArrayView<byte> outdata)
    {
        Sha256 ctx = new();
        ctx.Update(data, k);
        ctx.Final(outdata, k);
    }

    public static byte[] ComputeSHA256(Context context, Device device, byte[] input)
    {
        using Accelerator accelerator = device.CreateAccelerator(context);

        var kernel = accelerator
            .LoadAutoGroupedStreamKernel<Index1D, ArrayView<uint>, ArrayView<byte>, ArrayView<byte>>(Kernel);

        using var dK = accelerator.Allocate1D(K);
        using var dX = accelerator.Allocate1D(input);
        using var dY = accelerator.Allocate1D<byte>(32);

        kernel(1, dK.View, dX.View, dY.View);

        return dY.GetAsArray1D();
    }
}
#endif