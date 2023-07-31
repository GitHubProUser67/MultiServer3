using System.Numerics;

namespace PSMultiServer.Addons.CRYPTOSPORIDIUM.UnBAR
{
    internal class CMAC : Hash
    {
        private int hashLen;
        private byte[] key;
        private byte[] K1;
        private byte[] K2;
        private byte[] nonProcessed;
        private byte[] previous;

        public CMAC() => this.hashLen = 16;

        public override void setHashLen(int len) => this.hashLen = len == 16 ? len : throw new Exception("Hash len must be 0x10");

        public override void doInit(byte[] key)
        {
            this.key = key;
            this.K1 = new byte[16];
            this.K2 = new byte[16];
            this.calculateSubkey(key, this.K1, this.K2);
            this.nonProcessed = (byte[])null;
            this.previous = new byte[16];
        }

        private void calculateSubkey(byte[] key, byte[] K1, byte[] K2)
        {
            byte[] numArray1 = new byte[16];
            byte[] numArray2 = new byte[16];
            ToolsImpl.aesecbEncrypt(key, numArray1, 0, numArray2, 0, numArray1.Length);
            BigInteger bigInteger1 = new BigInteger(ConversionUtils.reverseByteWithSizeFIX(numArray2));
            BigInteger bigInteger2 = ((int)numArray2[0] & 128) == 0 ? bigInteger1 << 1 : bigInteger1 << 1 ^ new BigInteger(135);
            byte[] src1 = ConversionUtils.reverseByteWithSizeFIX(bigInteger2.ToByteArray());
            if (src1.Length >= 16)
            {
                ConversionUtils.arraycopy(src1, src1.Length - 16, K1, 0L, 16);
            }
            else
            {
                ConversionUtils.arraycopy(numArray1, 0, K1, 0L, numArray1.Length);
                ConversionUtils.arraycopy(src1, 0, K1, (long)(16 - src1.Length), src1.Length);
            }
            bigInteger2 = new BigInteger(ConversionUtils.reverseByteWithSizeFIX(K1));
            byte[] src2 = ConversionUtils.reverseByteWithSizeFIX((((int)K1[0] & 128) == 0 ? bigInteger2 << 1 : bigInteger2 << 1 ^ new BigInteger(135)).ToByteArray());
            if (src2.Length >= 16)
            {
                ConversionUtils.arraycopy(src2, src2.Length - 16, K2, 0L, 16);
            }
            else
            {
                ConversionUtils.arraycopy(numArray1, 0, K2, 0L, numArray1.Length);
                ConversionUtils.arraycopy(src2, 0, K2, (long)(16 - src2.Length), src2.Length);
            }
        }

        public override void doUpdate(byte[] i, int inOffset, int len)
        {
            byte[] numArray1;
            if (this.nonProcessed != null)
            {
                numArray1 = new byte[len + this.nonProcessed.Length];
                ConversionUtils.arraycopy(this.nonProcessed, 0, numArray1, 0L, this.nonProcessed.Length);
                ConversionUtils.arraycopy(i, inOffset, numArray1, (long)this.nonProcessed.Length, len);
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
                ToolsImpl.XOR(numArray2, numArray2, this.previous);
                ToolsImpl.aesecbEncrypt(this.key, numArray2, 0, this.previous, 0, numArray2.Length);
            }
            this.nonProcessed = new byte[numArray1.Length - srcPos];
            ConversionUtils.arraycopy(numArray1, srcPos, this.nonProcessed, 0L, this.nonProcessed.Length);
        }

        public override bool doFinalButGetHash(byte[] generatedHash)
        {
            byte[] numArray1 = new byte[16];
            ConversionUtils.arraycopy(this.nonProcessed, 0, numArray1, 0L, this.nonProcessed.Length);
            if (this.nonProcessed.Length == 16)
            {
                ToolsImpl.XOR(numArray1, numArray1, this.K1);
            }
            else
            {
                numArray1[this.nonProcessed.Length] = (byte)128;
                ToolsImpl.XOR(numArray1, numArray1, this.K2);
            }
            ToolsImpl.XOR(numArray1, numArray1, this.previous);
            byte[] numArray2 = new byte[16];
            ToolsImpl.aesecbEncrypt(this.key, numArray1, 0, numArray2, 0, numArray1.Length);
            ConversionUtils.arraycopy(numArray2, 0, generatedHash, 0L, numArray2.Length);
            return true;
        }

        public override bool doFinal(byte[] expectedhash, int hashOffset, bool hashDebug)
        {
            byte[] numArray = new byte[16];
            ConversionUtils.arraycopy(this.nonProcessed, 0, numArray, 0L, this.nonProcessed.Length);
            if (this.nonProcessed.Length == 16)
            {
                ToolsImpl.XOR(numArray, numArray, this.K1);
            }
            else
            {
                numArray[this.nonProcessed.Length] = (byte)128;
                ToolsImpl.XOR(numArray, numArray, this.K2);
            }
            ToolsImpl.XOR(numArray, numArray, this.previous);
            byte[] o = new byte[16];
            ToolsImpl.aesecbEncrypt(this.key, numArray, 0, o, 0, numArray.Length);
            return hashDebug || this.compareBytes(expectedhash, hashOffset, o, 0, this.hashLen);
        }
    }
}
