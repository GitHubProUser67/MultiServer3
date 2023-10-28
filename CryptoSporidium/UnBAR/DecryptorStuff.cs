namespace CryptoSporidium.UnBAR
{
    internal abstract class Decryptor
    {
        public virtual void doInit(byte[] key, byte[] iv)
        {
        }

        public virtual void doUpdate(byte[] i, int inOffset, byte[] o, int outOffset, int len)
        {
        }
    }
}