namespace HomeTools.SDAT
{
    internal abstract class Hash
    {
        public static bool CompareBytes(byte[] value1, int offset1, byte[] value2, int offset2, int len)
        {
            for (int index = 0; index < len; ++index)
            {
                if (value1[index + offset1] != value2[index + offset2])
                    return false;
            }
            return true;
        }

        public virtual void SetHashLen(int len)
        {
        }

        public virtual void DoInit(byte[] key)
        {
        }

        public virtual void DoUpdate(byte[] i, int inOffset, int len)
        {
        }

        public virtual bool DoFinal(byte[] expectedhash, int hashOffset, bool hashDebug) => false;

        public virtual bool DoFinalButGetHash(byte[] generatedHash) => false;
    }
}