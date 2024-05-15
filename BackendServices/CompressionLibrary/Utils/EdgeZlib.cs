using EndianTools;
using System;
using System.IO;
using ComponentAce.Compression.Libs.zlib;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace CompressionLibrary.Utils
{
    public class EdgeZlib
    {
        public static byte[] ICSharpEdgeZlibDecompress(byte[] inData)
        {
            MemoryStream memoryStream = new MemoryStream();
            MemoryStream memoryStream2 = new MemoryStream(inData);
            byte[] array = new byte[ChunkHeader.SizeOf];
            while (memoryStream2.Position < memoryStream2.Length)
            {
                memoryStream2.Read(array, 0, ChunkHeader.SizeOf);
                array = EndianUtils.EndianSwap(array);
                ChunkHeader header = ChunkHeader.FromBytes(array);
                int compressedSize = header.CompressedSize;
                byte[] array2 = new byte[compressedSize];
                memoryStream2.Read(array2, 0, compressedSize);
                byte[] array3 = ICSharpDecompressEdgeZlibChunk(array2, header);
                memoryStream.Write(array3, 0, array3.Length);
            }
            memoryStream2.Close();
            memoryStream.Close();
            return memoryStream.ToArray();
        }

        private static byte[] ICSharpDecompressEdgeZlibChunk(byte[] inData, ChunkHeader header)
        {
            if (header.CompressedSize == header.SourceSize)
                return inData;
            MemoryStream baseInputStream = new MemoryStream(inData);
            InflaterInputStream inflaterInputStream = new InflaterInputStream(baseInputStream, new Inflater(true));
            MemoryStream memoryStream = new MemoryStream();
            byte[] array = new byte[4096];
            for (; ; )
            {
                int num = inflaterInputStream.Read(array, 0, array.Length);
                if (num <= 0)
                    break;
                memoryStream.Write(array, 0, num);
            }
            inflaterInputStream.Close();
            return memoryStream.ToArray();
        }

        public static byte[] ComponentAceEdgeZlibDecompress(byte[] inData)
        {
            MemoryStream memoryStream = new MemoryStream();
            MemoryStream memoryStream2 = new MemoryStream(inData);
            byte[] array = new byte[ChunkHeader.SizeOf];
            while (memoryStream2.Position < memoryStream2.Length)
            {
                memoryStream2.Read(array, 0, ChunkHeader.SizeOf);
                array = EndianUtils.EndianSwap(array);
                ChunkHeader header = ChunkHeader.FromBytes(array);
                int compressedSize = header.CompressedSize;
                byte[] array2 = new byte[compressedSize];
                memoryStream2.Read(array2, 0, compressedSize);
                byte[] array3 = ComponentAceDecompressEdgeZlibChunk(array2, header);
                memoryStream.Write(array3, 0, array3.Length);
            }
            memoryStream2.Close();
            memoryStream.Close();
            return memoryStream.ToArray();
        }

        private static byte[] ComponentAceDecompressEdgeZlibChunk(byte[] InData, ChunkHeader header)
        {
            if (header.CompressedSize == header.SourceSize)
                return InData;
            MemoryStream memoryStream = new MemoryStream();
            ZOutputStream zoutputStream = new ZOutputStream(memoryStream, true);
            byte[] array = new byte[InData.Length];
            Array.Copy(InData, 0, array, 0, InData.Length);
            zoutputStream.Write(array, 0, array.Length);
            zoutputStream.Close();
            memoryStream.Close();
            return memoryStream.ToArray();
        }

        public static byte[] ComponentAceEdgeZlibCompress(byte[] inData)
        {
            MemoryStream memoryStream = new MemoryStream(inData.Length);
            MemoryStream memoryStream2 = new MemoryStream(inData);
            while (memoryStream2.Position < memoryStream2.Length)
            {
                int num = Math.Min((int)(memoryStream2.Length - memoryStream2.Position), 65535);
                byte[] array = new byte[num];
                memoryStream2.Read(array, 0, num);
                byte[] array2 = ComponentAceCompressEdgeZlibChunk(array);
                memoryStream.Write(array2, 0, array2.Length);
            }
            memoryStream2.Close();
            memoryStream.Close();
            return memoryStream.ToArray();
        }

        private static byte[] ComponentAceCompressEdgeZlibChunk(byte[] InData)
        {
            MemoryStream memoryStream = new MemoryStream();
            ZOutputStream zoutputStream = new ZOutputStream(memoryStream, 9, true);
            zoutputStream.Write(InData, 0, InData.Length);
            zoutputStream.Close();
            memoryStream.Close();
            byte[] array = memoryStream.ToArray();
            byte[] array2;
            if (array.Length >= InData.Length)
                array2 = InData;
            else
                array2 = array;
            byte[] array3 = new byte[array2.Length + 4];
            Array.Copy(array2, 0, array3, 4, array2.Length);
            ChunkHeader chunkHeader = default;
            chunkHeader.SourceSize = (ushort)InData.Length;
            chunkHeader.CompressedSize = (ushort)array2.Length;
            byte[] array4 = chunkHeader.GetBytes();
            array4 = EndianUtils.EndianSwap(array4);
            Array.Copy(array4, 0, array3, 0, ChunkHeader.SizeOf);
            return array3;
        }

        internal struct ChunkHeader
        {
            internal byte[] GetBytes()
            {
                byte[] array = new byte[4];
                Array.Copy(BitConverter.GetBytes((!BitConverter.IsLittleEndian) ? EndianUtils.EndianSwap(SourceSize) : SourceSize), 0, array, 2, 2);
                Array.Copy(BitConverter.GetBytes((!BitConverter.IsLittleEndian) ? EndianUtils.EndianSwap(CompressedSize) : CompressedSize), 0, array, 0, 2);
                return array;
            }

            internal static int SizeOf
            {
                get
                {
                    return 4;
                }
            }

            internal static ChunkHeader FromBytes(byte[] inData)
            {
                ChunkHeader result = default;
                byte[] array = inData;
                if (inData.Length > SizeOf)
                {
                    array = new byte[4];
                    Array.Copy(inData, array, 4);
                }

                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(array);

                result.SourceSize = BitConverter.ToUInt16(array, 2);
                result.CompressedSize = BitConverter.ToUInt16(array, 0);
                return result;
            }

            internal ushort SourceSize;

            internal ushort CompressedSize;
        }
    }
}
