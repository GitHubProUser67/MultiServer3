namespace CryptoSporidium.BARTools.BAR
{
    public class NoCompression : CompressionBase
    {
        public override byte[] Compress(TOCEntry te)
        {
            return Compress(te.RawData);
        }

        public override byte[] Decrypt(TOCEntry te)
        {
            return Decompress(te.RawData);
        }

        public override byte[] Compress(byte[] inData)
        {
            return inData;
        }

        public override byte[] Decompress(byte[] inData)
        {
            return inData;
        }

        public override CompressionMethod Method
        {
            get
            {
                return CompressionMethod.Uncompressed;
            }
        }
    }
}