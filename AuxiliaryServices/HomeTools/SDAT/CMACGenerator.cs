using System;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace HomeTools.SDAT
{
    internal class CMACGenerator : HashGenerator
    {
        private int hashLen;
        private CMac mac;
        private byte[] result;

        public CMACGenerator() => hashLen = 16;

        public override void SetHashLen(int len) => hashLen = len == 16 ? len : throw new Exception("Hash len must be 0x10");

        public override void DoInit(byte[] key)
        {
            try
            {
                mac = new CMac(new Org.BouncyCastle.Crypto.Engines.AesEngine());
                mac.Init(new KeyParameter(key));

                result = new byte[hashLen];
            }
            catch
            {
            }
        }

        public override void DoUpdate(byte[] i, int inOffset, int len)
        {
            mac.BlockUpdate(i, inOffset, len);
        }

        public override bool DoFinal(byte[] generatedHash)
        {
            mac.DoFinal(result, 0);
            ConversionUtils.Arraycopy(result, 0, generatedHash, 0L, result.Length);
            return true;
        }
    }
}