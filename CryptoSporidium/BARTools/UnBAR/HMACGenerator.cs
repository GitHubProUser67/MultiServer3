using CustomLogger;
using System.Security.Cryptography;

namespace CryptoSporidium.BARTools.UnBAR
{
    internal class HMACGenerator : HashGenerator
    {
        private int hashLen;
        private HMACSHA1 mac;
        private byte[] result;

        public override void setHashLen(int len) => hashLen = len == 16 || len == 20 ? len : throw new Exception("Hash len must be 0x10 or 0x14");

        public override void doInit(byte[] key)
        {
            try
            {
                mac = new HMACSHA1(key);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[HMACGenerator] - doInit thrown an exception {ex}");
            }
        }

        public override void doUpdate(byte[] i, int inOffset, int len) => result = mac.ComputeHash(i, inOffset, len);

        public override bool doFinal(byte[] generatedHash)
        {
            ConversionUtils.arraycopy(result, 0, generatedHash, 0L, result.Length);
            return true;
        }
    }
}
