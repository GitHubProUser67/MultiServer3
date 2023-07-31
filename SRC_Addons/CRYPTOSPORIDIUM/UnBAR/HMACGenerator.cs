using System.Security.Cryptography;

namespace PSMultiServer.Addons.CRYPTOSPORIDIUM.UnBAR
{
    internal class HMACGenerator : HashGenerator
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

        public override bool doFinal(byte[] generatedHash)
        {
            ConversionUtils.arraycopy(this.result, 0, generatedHash, 0L, this.result.Length);
            return true;
        }
    }
}
