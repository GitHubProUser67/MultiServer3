namespace HomeTools.SDAT
{
    internal abstract class Decryptor
    {
        public virtual void DoInit(byte[] key, byte[] iv)
        {
        }

        public virtual void DoUpdate(byte[] i, int inOffset, byte[] o, int outOffset, int len)
        {
        }
    }
}