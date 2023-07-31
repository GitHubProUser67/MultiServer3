using System.Security.Cryptography;

namespace PSMultiServer.Addons.CRYPTOSPORIDIUM.UnBAR
{
    internal class AESCBC128Encrypt : Decryptor
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
                this.ct = ((SymmetricAlgorithm)this.c).CreateEncryptor();
            }
            catch (Exception ex)
            {
            }
        }

        public override void doUpdate(byte[] i, int inOffset, byte[] o, int outOffset, int len) => this.ct.TransformBlock(i, inOffset, len, o, outOffset);
    }
}
