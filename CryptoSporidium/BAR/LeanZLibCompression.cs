namespace MultiServer.CryptoSporidium.BAR
{
    public class LeanZLibCompression : CompressionBase
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
            return ZLibCompressor.Compress(inData, true);
        }

        public override byte[] Decompress(byte[] inData)
        {
            return ZLibCompressor.Decompress(inData, true);
        }

        public override CompressionMethod Method
        {
            get
            {
                return CompressionMethod.ZLib;
            }
        }
    }
}
