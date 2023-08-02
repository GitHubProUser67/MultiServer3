using System.Security.Cryptography;

namespace PSMultiServer.CryptoSporidium.UnBAR
{
    internal class AESCBC128Decrypt : Decryptor
    {
        private RijndaelManaged c;
        private ICryptoTransform ct;

        public override void doInit(byte[] key, byte[] iv)
        {
            try
            {
                c = new RijndaelManaged();
                c.Padding = PaddingMode.None;
                c.Mode = CipherMode.CBC;
                c.Key = key;
                c.IV = iv;
                ct = c.CreateDecryptor();
            }
            catch (Exception ex)
            {
            }
        }

        public override void doUpdate(byte[] i, int inOffset, byte[] o, int outOffset, int len)
        {
            try
            {
                ct.TransformBlock(i, inOffset, len, o, outOffset);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
