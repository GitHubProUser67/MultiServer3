using System.Numerics;

namespace PSMultiServer.SRC_Addons.CRYPTOSPORIDIUM.UnBAR
{
    internal class EDATData
    {
        public long flags;
        public long blockSize;
        public BigInteger fileLen;

        public static EDATData createEDATData(byte[] data) => new EDATData()
        {
            flags = ConversionUtils.be32(data, 0),
            blockSize = ConversionUtils.be32(data, 4),
            fileLen = ConversionUtils.be64(data, 8)
        };

        public long getBlockSize() => this.blockSize;

        public BigInteger getFileLen() => this.fileLen;

        public long getFlags() => this.flags;
    }
}
