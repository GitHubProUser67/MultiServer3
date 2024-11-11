using System;
using System.Security.Cryptography;

namespace HomeTools.SDAT
{
    internal class HMACGenerator : HashGenerator
    {
        private int hashLen;
        private HMACSHA1 mac;
        private byte[] result;

        public override void SetHashLen(int len) => hashLen = len == 16 || len == 20 ? len : throw new Exception("Hash len must be 0x10 or 0x14");

        public override void DoInit(byte[] key)
        {
            try
            {
                mac = new HMACSHA1(key);
            }
            catch
            {
            }
        }

        public override void DoUpdate(byte[] i, int inOffset, int len) => result = mac?.ComputeHash(i, inOffset, len);

        public override bool DoFinal(byte[] generatedHash)
        {
            ConversionUtils.Arraycopy(result, 0, generatedHash, 0L, result.Length);
            return true;
        }
    }
}
