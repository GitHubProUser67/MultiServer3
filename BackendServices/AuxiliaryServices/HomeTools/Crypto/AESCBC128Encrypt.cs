using System.Security.Cryptography;

namespace HomeTools.Crypto
{
    public class AESCBC128Encrypt : Decryptor
    {
        private Aes? c;
        private ICryptoTransform? ct;

        public override void doInit(byte[] key, byte[] iv)
        {
            try
            {
                c = Aes.Create();
                c.Padding = PaddingMode.None;
                c.Mode = CipherMode.CBC;
                c.Key = key;
                c.IV = iv;
                ct = c.CreateEncryptor();
            }
            catch
            {

            }
        }

        public override void doUpdate(byte[] i, int inOffset, byte[] o, int outOffset, int len) => ct?.TransformBlock(i, inOffset, len, o, outOffset);
    }
}