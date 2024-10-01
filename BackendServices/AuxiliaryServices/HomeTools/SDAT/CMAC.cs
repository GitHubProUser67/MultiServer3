using System;
using System.Numerics;

namespace HomeTools.SDAT
{
    internal class CMAC : Hash
    {
        private int hashLen;
        private byte[] key;
        private byte[] K1;
        private byte[] K2;
        private byte[] nonProcessed;
        private byte[] previous;

        public CMAC() => hashLen = 16;

        public override void SetHashLen(int len) => hashLen = len == 16 ? len : throw new Exception("Hash len must be 0x10");

        public override void DoInit(byte[] key)
        {
            this.key = key;
            K1 = new byte[16];
            K2 = new byte[16];
            CalculateSubkey(key, K1, K2);
            nonProcessed = null;
            previous = new byte[16];
        }

        private static void CalculateSubkey(byte[] key, byte[] K1, byte[] K2)
        {
            byte[] numArray1 = new byte[16];
            byte[] numArray2 = new byte[16];
            CryptUtils.AesecbEncrypt(key, numArray1, 0, numArray2, 0, numArray1.Length);
            BigInteger bigInteger1 = new BigInteger(ConversionUtils.ReverseByteWithSizeFIX(numArray2));
            BigInteger bigInteger2 = (numArray2[0] & 128) == 0 ? bigInteger1 << 1 : bigInteger1 << 1 ^ new BigInteger(135);
            byte[] src1 = ConversionUtils.ReverseByteWithSizeFIX(bigInteger2.ToByteArray());
            if (src1.Length >= 16)
                ConversionUtils.Arraycopy(src1, src1.Length - 16, K1, 0L, 16);
            else
            {
                ConversionUtils.Arraycopy(numArray1, 0, K1, 0L, numArray1.Length);
                ConversionUtils.Arraycopy(src1, 0, K1, 16 - src1.Length, src1.Length);
            }
            bigInteger2 = new BigInteger(ConversionUtils.ReverseByteWithSizeFIX(K1));
            byte[] src2 = ConversionUtils.ReverseByteWithSizeFIX(((K1[0] & 128) == 0 ? bigInteger2 << 1 : bigInteger2 << 1 ^ new BigInteger(135)).ToByteArray());
            if (src2.Length >= 16)
                ConversionUtils.Arraycopy(src2, src2.Length - 16, K2, 0L, 16);
            else
            {
                ConversionUtils.Arraycopy(numArray1, 0, K2, 0L, numArray1.Length);
                ConversionUtils.Arraycopy(src2, 0, K2, 16 - src2.Length, src2.Length);
            }
        }

        public override void DoUpdate(byte[] i, int inOffset, int len)
        {
            byte[] numArray1;
            if (nonProcessed != null)
            {
                numArray1 = new byte[len + nonProcessed.Length];
                ConversionUtils.Arraycopy(nonProcessed, 0, numArray1, 0L, nonProcessed.Length);
                ConversionUtils.Arraycopy(i, inOffset, numArray1, nonProcessed.Length, len);
            }
            else
            {
                numArray1 = new byte[len];
                ConversionUtils.Arraycopy(i, inOffset, numArray1, 0L, len);
            }
            int srcPos;
            for (srcPos = 0; srcPos < numArray1.Length - 16; srcPos += 16)
            {
                byte[] numArray2 = new byte[16];
                ConversionUtils.Arraycopy(numArray1, srcPos, numArray2, 0L, numArray2.Length);
                CryptUtils.XOR(numArray2, numArray2, previous);
                CryptUtils.AesecbEncrypt(key, numArray2, 0, previous, 0, numArray2.Length);
            }
            nonProcessed = new byte[numArray1.Length - srcPos];
            ConversionUtils.Arraycopy(numArray1, srcPos, nonProcessed, 0L, nonProcessed.Length);
        }

        public override bool DoFinalButGetHash(byte[] generatedHash)
        {
            byte[] numArray1 = new byte[16];
            ConversionUtils.Arraycopy(nonProcessed, 0, numArray1, 0L, nonProcessed.Length);
            if (nonProcessed.Length == 16)
                CryptUtils.XOR(numArray1, numArray1, K1);
            else
            {
                numArray1[nonProcessed.Length] = 128;
                CryptUtils.XOR(numArray1, numArray1, K2);
            }
            CryptUtils.XOR(numArray1, numArray1, previous);
            byte[] numArray2 = new byte[16];
            CryptUtils.AesecbEncrypt(key, numArray1, 0, numArray2, 0, numArray1.Length);
            ConversionUtils.Arraycopy(numArray2, 0, generatedHash, 0L, numArray2.Length);
            return true;
        }

        public override bool DoFinal(byte[] expectedhash, int hashOffset, bool hashDebug)
        {
            byte[] numArray = new byte[16];
            ConversionUtils.Arraycopy(nonProcessed, 0, numArray, 0L, nonProcessed.Length);
            if (nonProcessed.Length == 16)
                CryptUtils.XOR(numArray, numArray, K1);
            else
            {
                numArray[nonProcessed.Length] = 128;
                CryptUtils.XOR(numArray, numArray, K2);
            }
            CryptUtils.XOR(numArray, numArray, previous);
            byte[] o = new byte[16];
            CryptUtils.AesecbEncrypt(key, numArray, 0, o, 0, numArray.Length);
            return hashDebug || CompareBytes(expectedhash, hashOffset, o, 0, hashLen);
        }
    }
}