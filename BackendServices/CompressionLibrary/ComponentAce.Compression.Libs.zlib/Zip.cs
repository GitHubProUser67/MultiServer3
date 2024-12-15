using System.IO;

#if NET6_0_OR_GREATER
using System.IO.Compression;
#else
using System;
#endif

namespace ComponentAce.Compression.Libs.zlib
{
    internal static class Zip
    {
        internal static Stream CompressOutput(Stream stream, int zlibCompressionLevel, bool leaveOpen = false)
        {
#if NET6_0_OR_GREATER
            return new DeflateStream(stream, ZLib.GetCompressionLevel(zlibCompressionLevel), leaveOpen);
#else
            throw new NotImplementedException("[Zip] - CompressOutput requires NET6 or higher!");
#endif
        }

        internal static Stream DecompressInput(Stream stream, bool leaveOpen = false)
        {
#if NET6_0_OR_GREATER
            return new DeflateStream(stream, CompressionMode.Decompress, leaveOpen);
#else
            throw new NotImplementedException("[Zip] - DecompressInput requires NET6 or higher!");
#endif
        }
    }
}
