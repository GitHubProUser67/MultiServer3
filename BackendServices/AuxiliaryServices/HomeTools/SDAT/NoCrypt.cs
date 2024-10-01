namespace HomeTools.SDAT
{
    internal class NoCrypt : Decryptor
    {
        public override void DoInit(byte[] key, byte[] iv)
        {
        }

        public override void DoUpdate(byte[] i, int inOffset, byte[] o, int outOffset, int len) => ConversionUtils.Arraycopy(i, inOffset, o, outOffset, len);
    }
}