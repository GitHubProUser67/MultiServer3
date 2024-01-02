namespace BackendProject.HomeTools.BARFramework
{
    public class ZLibCompression : CompressionBase
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
            return ZLibCompressor.Compress(inData, false);
        }

        public override byte[] Decompress(byte[] inData)
        {
            return ZLibCompressor.Decompress(inData, false);
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