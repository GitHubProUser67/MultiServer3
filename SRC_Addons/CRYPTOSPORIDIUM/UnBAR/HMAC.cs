using System.Security.Cryptography;

namespace PSMultiServer.SRC_Addons.CRYPTOSPORIDIUM.UnBAR
{
    internal class HMAC : Hash
    {
        private int hashLen;
        private HMACSHA1 mac;
        private byte[] result;

        public override void setHashLen(int len) => this.hashLen = len == 16 || len == 20 ? len : throw new Exception("Hash len must be 0x10 or 0x14");

        public override void doInit(byte[] key)
        {
            try
            {
                this.mac = new HMACSHA1(key);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override void doUpdate(byte[] i, int inOffset, int len) => this.result = this.mac.ComputeHash(i, inOffset, len);

        public override bool doFinal(byte[] expectedhash, int hashOffset, bool hashDebug) => hashDebug || this.compareBytes(this.result, 0, expectedhash, hashOffset, this.hashLen);

        public override bool doFinalButGetHash(byte[] generatedHash)
        {
            ConversionUtils.arraycopy(this.result, 0, generatedHash, 0L, this.result.Length);
            return true;
        }
    }
}
