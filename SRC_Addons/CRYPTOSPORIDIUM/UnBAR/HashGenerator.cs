namespace PSMultiServer.Addons.CRYPTOSPORIDIUM.UnBAR
{
    internal abstract class HashGenerator
    {
        public bool compareBytes(byte[] value1, int offset1, byte[] value2, int offset2, int len)
        {
            for (int index = 0; index < len; ++index)
            {
                if ((int)value1[index + offset1] != (int)value2[index + offset2])
                    return false;
            }
            return true;
        }

        public virtual void setHashLen(int len)
        {
        }

        public virtual void doInit(byte[] key)
        {
        }

        public virtual void doUpdate(byte[] i, int inOffset, int len)
        {
        }

        public virtual bool doFinal(byte[] generateHash) => false;
    }
}
