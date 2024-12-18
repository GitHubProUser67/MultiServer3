using EndianTools;
using System;
using System.IO;
using ComponentAce.Compression.Libs.zlib;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CompressionLibrary.Edge
{
    public class Zlib
    {
        public static async Task<byte[]> EdgeZlibDecompress(byte[] inData, bool ICSharp = false)
        {
            int chunkIndex = 0;
            List<KeyValuePair<int, Task<byte[]>>> zlibTasks = new List<KeyValuePair<int, Task<byte[]>>>();

            using (MemoryStream memoryStream = new MemoryStream(inData))
            {
                byte[] array = new byte[ChunkHeader.SizeOf];
                while (memoryStream.Position < memoryStream.Length)
                {
                    memoryStream.Read(array, 0, ChunkHeader.SizeOf);
                    array = EndianUtils.EndianSwap(array);
                    ChunkHeader header = ChunkHeader.FromBytes(array);
                    int compressedSize = header.CompressedSize;
                    byte[] array2 = new byte[compressedSize];
                    memoryStream.Read(array2, 0, compressedSize);
                    zlibTasks.Add(ICSharp
                        ? new KeyValuePair<int, Task<byte[]>>(chunkIndex, ICSharpDecompressEdgeZlibChunk(array2, header))
                        : new KeyValuePair<int, Task<byte[]>>(chunkIndex, ComponentAceDecompressEdgeZlibChunk(array2, header)));
                    chunkIndex++;
                }
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                foreach (var result in zlibTasks.OrderBy(kv => kv.Key))
                {
                    try
                    {
                        // Await each decompression task
                        byte[] decompressedChunk = await result.Value.ConfigureAwait(false);
                        memoryStream.Write(decompressedChunk, 0, decompressedChunk.Length);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"[Zlib] - EdgeZlibDecompress: Error during decompression at chunk {result.Key}", ex);
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public static async Task<byte[]> EdgeZlibCompress(byte[] inData)
        {
            int chunkIndex = 0;
            List<KeyValuePair<int, Task<byte[]>>> zlibTasks = new List<KeyValuePair<int, Task<byte[]>>>();

            using (MemoryStream memoryStream = new MemoryStream(inData))
            {
                while (memoryStream.Position < memoryStream.Length)
                {
                    int num = Math.Min((int)(memoryStream.Length - memoryStream.Position), ushort.MaxValue);
                    byte[] array = new byte[num];
                    memoryStream.Read(array, 0, num);
                    zlibTasks.Add(new KeyValuePair<int, Task<byte[]>>(chunkIndex, ComponentAceCompressEdgeZlibChunk(array)));
                    chunkIndex++;
                }
            }

            using (MemoryStream memoryStream = new MemoryStream(inData.Length))
            {
                foreach (var result in zlibTasks.OrderBy(kv => kv.Key))
                {
                    try
                    {
                        // Await each compression task
                        byte[] compressedChunk = await result.Value.ConfigureAwait(false);
                        memoryStream.Write(compressedChunk, 0, compressedChunk.Length);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"[Zlib] - EdgeZlibCompress: Error during compression at chunk {result.Key}", ex);
                    }
                }

                return memoryStream.ToArray();
            }
        }

        private static Task<byte[]> ICSharpDecompressEdgeZlibChunk(byte[] inData, ChunkHeader header)
        {
            if (header.CompressedSize == header.SourceSize)
                return Task.FromResult(inData);
            InflaterInputStream inflaterInputStream = new InflaterInputStream(new MemoryStream(inData), new Inflater(true));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] array = new byte[4096];
                for (; ; )
                {
                    int num = inflaterInputStream.Read(array, 0, array.Length);
                    if (num <= 0)
                        break;
                    memoryStream.Write(array, 0, num);
                }
                inflaterInputStream.Dispose();
                return Task.FromResult(memoryStream.ToArray());
            }
        }

        private static Task<byte[]> ComponentAceDecompressEdgeZlibChunk(byte[] InData, ChunkHeader header)
        {
            if (header.CompressedSize == header.SourceSize)
                return Task.FromResult(InData);
            using (MemoryStream memoryStream = new MemoryStream())
            using (ZOutputStream zoutputStream = new ZOutputStream(memoryStream, true))
            {
                byte[] array = new byte[InData.Length];
                Array.Copy(InData, 0, array, 0, InData.Length);
                zoutputStream.Write(array, 0, array.Length);
                zoutputStream.Close();
                memoryStream.Close();
                return Task.FromResult(memoryStream.ToArray());
            }
        }

        private static Task<byte[]> ComponentAceCompressEdgeZlibChunk(byte[] InData)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (ZOutputStream zoutputStream = new ZOutputStream(memoryStream, 9, true))
            {
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
                return Task.FromResult(array3);
            }
        }

        internal struct ChunkHeader
        {
            internal byte[] GetBytes()
            {
                byte[] array = new byte[4];
                Array.Copy(BitConverter.GetBytes(!BitConverter.IsLittleEndian ? EndianUtils.ReverseUshort(SourceSize) : SourceSize), 0, array, 2, 2);
                Array.Copy(BitConverter.GetBytes(!BitConverter.IsLittleEndian ? EndianUtils.ReverseUshort(CompressedSize) : CompressedSize), 0, array, 0, 2);
                return array;
            }

            internal static byte SizeOf
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
