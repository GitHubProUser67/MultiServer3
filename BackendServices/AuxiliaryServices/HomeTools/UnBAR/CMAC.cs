using System;
using System.Numerics;
using HomeTools.Crypto;

namespace HomeTools.UnBAR
{
    internal class CMAC : Hash
    {
        private int hashLen;
        private byte[]? key;
        private byte[]? K1;
        private byte[]? K2;
        private byte[]? nonProcessed;
        private byte[]? previous;

        public CMAC() => hashLen = 16;

        public override void setHashLen(int len) => hashLen = len == 16 ? len : throw new Exception("Hash len must be 0x10");

        public override void doInit(byte[] key)
        {
            this.key = key;
            K1 = new byte[16];
            K2 = new byte[16];
            calculateSubkey(key, K1, K2);
            nonProcessed = null;
            previous = new byte[16];
        }

        private void calculateSubkey(byte[] key, byte[] K1, byte[] K2)
        {
            byte[] numArray1 = new byte[16];
            byte[] numArray2 = new byte[16];
            ToolsImpl.aesecbEncrypt(key, numArray1, 0, numArray2, 0, numArray1.Length);
            BigInteger bigInteger1 = new BigInteger(ConversionUtils.reverseByteWithSizeFIX(numArray2));
            BigInteger bigInteger2 = (numArray2[0] & 128) == 0 ? bigInteger1 << 1 : bigInteger1 << 1 ^ new BigInteger(135);
            byte[] src1 = ConversionUtils.reverseByteWithSizeFIX(bigInteger2.ToByteArray());
            if (src1.Length >= 16)
                ConversionUtils.arraycopy(src1, src1.Length - 16, K1, 0L, 16);
            else
            {
                ConversionUtils.arraycopy(numArray1, 0, K1, 0L, numArray1.Length);
                ConversionUtils.arraycopy(src1, 0, K1, 16 - src1.Length, src1.Length);
            }
            bigInteger2 = new BigInteger(ConversionUtils.reverseByteWithSizeFIX(K1));
            byte[] src2 = ConversionUtils.reverseByteWithSizeFIX(((K1[0] & 128) == 0 ? bigInteger2 << 1 : bigInteger2 << 1 ^ new BigInteger(135)).ToByteArray());
            if (src2.Length >= 16)
                ConversionUtils.arraycopy(src2, src2.Length - 16, K2, 0L, 16);
            else
            {
                ConversionUtils.arraycopy(numArray1, 0, K2, 0L, numArray1.Length);
                ConversionUtils.arraycopy(src2, 0, K2, 16 - src2.Length, src2.Length);
            }
        }

        public override void doUpdate(byte[] i, int inOffset, int len)
        {
            byte[] numArray1;
            if (nonProcessed != null)
            {
                numArray1 = new byte[len + nonProcessed.Length];
                ConversionUtils.arraycopy(nonProcessed, 0, numArray1, 0L, nonProcessed.Length);
                ConversionUtils.arraycopy(i, inOffset, numArray1, nonProcessed.Length, len);
            }
            else
            {
                numArray1 = new byte[len];
                ConversionUtils.arraycopy(i, inOffset, numArray1, 0L, len);
            }
            int srcPos;
            for (srcPos = 0; srcPos < numArray1.Length - 16; srcPos += 16)
            {
                byte[] numArray2 = new byte[16];
                ConversionUtils.arraycopy(numArray1, srcPos, numArray2, 0L, numArray2.Length);
                ToolsImpl.XOR(numArray2, numArray2, previous);
                ToolsImpl.aesecbEncrypt(key, numArray2, 0, previous, 0, numArray2.Length);
            }
            nonProcessed = new byte[numArray1.Length - srcPos];
            ConversionUtils.arraycopy(numArray1, srcPos, nonProcessed, 0L, nonProcessed.Length);
        }

        public override bool doFinalButGetHash(byte[] generatedHash)
        {
            byte[] numArray1 = new byte[16];
            ConversionUtils.arraycopy(nonProcessed, 0, numArray1, 0L, nonProcessed.Length);
            if (nonProcessed.Length == 16)
                ToolsImpl.XOR(numArray1, numArray1, K1);
            else
            {
                numArray1[nonProcessed.Length] = 128;
                ToolsImpl.XOR(numArray1, numArray1, K2);
            }
            ToolsImpl.XOR(numArray1, numArray1, previous);
            byte[] numArray2 = new byte[16];
            ToolsImpl.aesecbEncrypt(key, numArray1, 0, numArray2, 0, numArray1.Length);
            ConversionUtils.arraycopy(numArray2, 0, generatedHash, 0L, numArray2.Length);
            return true;
        }

        public override bool doFinal(byte[] expectedhash, int hashOffset, bool hashDebug)
        {
            byte[] numArray = new byte[16];
            ConversionUtils.arraycopy(nonProcessed, 0, numArray, 0L, nonProcessed.Length);
            if (nonProcessed.Length == 16)
                ToolsImpl.XOR(numArray, numArray, K1);
            else
            {
                numArray[nonProcessed.Length] = 128;
                ToolsImpl.XOR(numArray, numArray, K2);
            }
            ToolsImpl.XOR(numArray, numArray, previous);
            byte[] o = new byte[16];
            ToolsImpl.aesecbEncrypt(key, numArray, 0, o, 0, numArray.Length);
            return hashDebug || compareBytes(expectedhash, hashOffset, o, 0, hashLen);
        }
    }
}