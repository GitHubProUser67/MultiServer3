using System.Numerics;

namespace HomeTools.SDAT
{
    internal class EDATData
    {
        public long flags;
        public long blockSize;
        public BigInteger fileLen;

        public static EDATData CreateEDATData(byte[] data) => new EDATData()
        {
            flags = ConversionUtils.Be32(data, 0),
            blockSize = ConversionUtils.Be32(data, 4),
            fileLen = ConversionUtils.Be64(data, 8)
        };

        public long GetBlockSize() => blockSize;

        public BigInteger GetFileLen() => fileLen;

        public long GetFlags() => flags;
    }
}