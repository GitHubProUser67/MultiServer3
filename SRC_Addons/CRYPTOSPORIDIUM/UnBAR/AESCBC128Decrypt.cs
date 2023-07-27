using System.Security.Cryptography;

namespace PSMultiServer.SRC_Addons.CRYPTOSPORIDIUM.UnBAR
{
    internal class AESCBC128Decrypt : Decryptor
    {
        private RijndaelManaged c;
        private ICryptoTransform ct;

        public override void doInit(byte[] key, byte[] iv)
        {
            try
            {
                this.c = new RijndaelManaged();
                ((SymmetricAlgorithm)this.c).Padding = PaddingMode.None;
                ((SymmetricAlgorithm)this.c).Mode = CipherMode.CBC;
                ((SymmetricAlgorithm)this.c).Key = key;
                ((SymmetricAlgorithm)this.c).IV = iv;
                this.ct = ((SymmetricAlgorithm)this.c).CreateDecryptor();
            }
            catch (Exception ex)
            {
            }
        }

        public override void doUpdate(byte[] i, int inOffset, byte[] o, int outOffset, int len)
        {
            try
            {
                this.ct.TransformBlock(i, inOffset, len, o, outOffset);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
