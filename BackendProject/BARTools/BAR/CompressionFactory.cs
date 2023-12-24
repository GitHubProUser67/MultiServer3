namespace BackendProject.BARTools.BAR
{
    public static class CompressionFactory
    {
        public static CompressionBase? Create(CompressionMethod method, ArchiveFlags flags)
        {
            CompressionBase? result = null;
            if (method == CompressionMethod.Uncompressed)
                result = new NoCompression();
            else if (method == CompressionMethod.ZLib)
            {
                if ((flags & ArchiveFlags.Bar_Flag_LeanZLib) == ArchiveFlags.Bar_Flag_LeanZLib)
                    result = new LeanZLibCompression();
                else
                    result = new ZLibCompression();
            }
            else if (method == CompressionMethod.EdgeZLib)
                result = new EdgeZLibCompression_ICSharp();
            else if (method == CompressionMethod.Encrypted)
                result = new NoCompression();
            return result;
        }

        public static byte[]? Compress(byte[] inData, CompressionMethod method, ArchiveFlags flags)
        {
            CompressionBase? compressionBase = Create(method, flags);
            if (compressionBase != null)
                return compressionBase.Compress(inData);
            return null;
        }

        public static byte[]? Decompress(byte[] inData, CompressionMethod method, ArchiveFlags flags)
        {
            CompressionBase? compressionBase = Create(method, flags);
            if (compressionBase != null)
                return compressionBase.Decompress(inData);
            return null;
        }

        public static byte[]? Decompress(TOCEntry te, CompressionMethod method, ArchiveFlags flags)
        {
            CompressionBase? compressionBase = Create(method, flags);
            if (compressionBase == null)
                return null;
            if (method == CompressionMethod.Encrypted)
                return compressionBase.Decrypt(te);
            return compressionBase.Decompress(te.RawData);
        }

        public static byte[]? Compress(TOCEntry te, CompressionMethod method, ArchiveFlags flags)
        {
            CompressionBase? compressionBase = Create(method, flags);
            if (compressionBase != null)
                return compressionBase.Compress(te);
            return null;
        }
    }
}