using System.Security.Cryptography;

namespace HomeTools.SDAT
{
    internal class AESCBC128Decrypt : Decryptor
    {
        private Aes c;
        private ICryptoTransform ct;

        public override void doInit(byte[] key, byte[] iv)
        {
            try
            {
                c = Aes.Create();
                c.Padding = PaddingMode.None;
                c.Mode = CipherMode.CBC;
                c.Key = key;
                c.IV = iv;
                ct = c.CreateDecryptor();
            }
            catch
            {
            }
        }

        public override void doUpdate(byte[] i, int inOffset, byte[] o, int outOffset, int len)
        {
            try
            {
                ct?.TransformBlock(i, inOffset, len, o, outOffset);
            }
            catch
            {
            }
        }
    }
}