namespace System
{
    public class UniqueIDGenerator
    {
        private readonly object _InternalLock = new object();

        private uint UniqueIDCounter;

        public UniqueIDGenerator(uint startingValue = 0)
        {
            UniqueIDCounter = startingValue;
        }

        public uint CreateUniqueID()
        {
            lock (_InternalLock)
                return ++UniqueIDCounter;
        }
    }
}
