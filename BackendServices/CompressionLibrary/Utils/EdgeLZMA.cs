using CustomLogger;
using EndianTools;
using SevenZip;
using SevenZip.Compression.LZMA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CompressionLibrary.Utils
{
    public class EdgeLZMA
    {
        private static int dictionary = 1 << 23;

        // static Int32 posStateBits = 2;
        // static Int32 litContextBits = 3; // for normal files
        // UInt32 litContextBits = 0; // for 32-bit data
        // static Int32 litPosBits = 0;
        // UInt32 litPosBits = 2; // for 32-bit data
        // static Int32 algorithm = 2;
        // static Int32 numFastBytes = 128;

        private static bool eos = false;

        private static CoderPropID[] propIDs =
                {
                    CoderPropID.DictionarySize,
                    CoderPropID.PosStateBits,
                    CoderPropID.LitContextBits,
                    CoderPropID.LitPosBits,
                    CoderPropID.Algorithm,
                    CoderPropID.NumFastBytes,
                    CoderPropID.MatchFinder,
                    CoderPropID.EndMarker
                };

        // these are the default properties, keeping it simple for now:
        private static object[] properties =
                {
                    (System.Int32)(dictionary),
                    (System.Int32)(2),
                    (System.Int32)(3),
                    (System.Int32)(0),
                    (System.Int32)(2),
                    (System.Int32)(128),
                    "bt4",
                    eos
                };

        /// <summary>
        /// Decompress a EdgeLZMA byte array data.
        /// <para>Decompresser un tableau de byte en format EdgeLZMA.</para>
        /// </summary>
        /// <param name="data">The byte array to decompress.</param>
        /// <param name="SegsMode">Enables an alternative decompression mode.</param>
        /// <param name="safemode">Uses a proper error checking flag.</param>
        /// <returns>A byte array.</returns>
        public static byte[] Decompress(byte[] data, bool SegsMode, bool safemode = true)
        {
            if (SegsMode)
                return SegmentsDecompress(data, safemode);
            else
                return Decompress(data);
        }

        /// <summary>
        /// Compress a given buffer compressed with EdgeLZMA.
        /// <para>Compresse un tableau de bytes avec le codec EdgeLZMA.</para>
        /// </summary>
        /// <param name="buffer">The byte array to compress.</param>
        /// <returns>A byte array.</returns>
        private static byte[] Compress(byte[] buffer)
        {
            int numFastBytes = 64;
            int litContextBits = 3;
            int litPosBits = 0;
            int posStateBits = 2;
            int inSize = buffer.Length;
            int streamCount = inSize + 0xffff >> 16;
            int offset = 0;
            MemoryStream result = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(result);

            bw.Write(new byte[] { 0x54, 0x4C, 0x5A, 0x43 }); // Tales of magic.
            bw.Write((byte)0x01); // Mode
            bw.Write((byte)0x04); // Version
            bw.Write((byte)0x00);
            bw.Write((byte)0x00);
            bw.Write(0);   // compressed size - we'll fill this in once we know it
            bw.Write(buffer.Length);   // decompressed size
            bw.Write(0);   // unknown, 0
            bw.Write(0);   // unknown, 0
            var encoder = new Encoder(); // next comes the coder properties (5 bytes), followed by stream lengths, followed by the streams themselves.
            var props = new Dictionary<CoderPropID, object>();
            props[CoderPropID.DictionarySize] = 0x10000;
            props[CoderPropID.MatchFinder] = "BT4";
            props[CoderPropID.NumFastBytes] = numFastBytes;
            props[CoderPropID.LitContextBits] = litContextBits;
            props[CoderPropID.LitPosBits] = litPosBits;
            props[CoderPropID.PosStateBits] = posStateBits;
            //props[CoderPropID.BlockSize] = blockSize; // this always throws an exception when set
            //props[CoderPropID.MatchFinderCycles] = matchFinderCycles; // ^ same here
            //props[CoderPropID.DefaultProp] = 0;
            //props[CoderPropID.UsedMemorySize] = 100000;
            //props[CoderPropID.Order] = 1;
            //props[CoderPropID.NumPasses] = 10;
            //props[CoderPropID.Algorithm] = 0;
            //props[CoderPropID.NumThreads] = ;
            //props[CoderPropID.EndMarker] = ;

            encoder.SetCoderProperties(props.Keys.ToArray(), props.Values.ToArray());

            encoder.WriteCoderProperties(result);

            // reserve space for the stream lengths. we'll fill them in later after we know what they are.
            bw.Write(new byte[streamCount * 2]);

            List<int> streamSizes = new List<int>();

            for (int i = 0; i < streamCount; i++)
            {
                long preLength = result.Length;

                encoder.Code(new MemoryStream(buffer, offset, Math.Min(inSize, 0x10000)), result, Math.Min(inSize, 0x10000), -1, null);

                int streamSize = (int)(result.Length - preLength);
                if (streamSize >= 0x10000)
                {
                    LoggerAccessor.LogDebug("[EdgeLzma] - Warning - Stream did not compress - script might not be executed on PS3 correctly.");
                    result.Position = preLength;
                    result.SetLength(preLength);
                    result.Write(buffer, offset, Math.Min(inSize, 0x10000));
                    streamSize = 0;
                }

                inSize -= 0x10000;
                offset += 0x10000;
                streamSizes.Add(streamSize);
            }

            // fill in compressed size
            result.Position = 8;
            bw.Write((int)result.Length);

            byte[] temp = result.ToArray();

            // fill in stream sizes
            for (int i = 0; i < streamSizes.Count; i++)
            {
                temp[5 + 0x18 + i * 2] = (byte)streamSizes[i];
                temp[6 + 0x18 + i * 2] = (byte)(streamSizes[i] >> 8);
            }

            return temp;
        }

        /// <summary>
        /// Decompress a given buffer to EdgeLZMA.
        /// <para>d�compresse un tableau de bytes avec le codec EdgeLZMA.</para>
        /// </summary>
        /// <param name="buffer">The byte array to decompress.</param>
        /// <returns>A byte array.</returns>
        private static byte[] Decompress(byte[] buffer)
        {
            MemoryStream result = new MemoryStream();
            int outSize = BitConverter.ToInt32(!BitConverter.IsLittleEndian ? EndianUtils.EndianSwap(buffer) : buffer, 12);
            int streamCount = outSize + 0xffff >> 16;
            int offset = 0x18 + streamCount * 2 + 5;

            Decoder decoder = new Decoder();
            decoder.SetDecoderProperties(new MemoryStream(buffer, 0x18, 5).ToArray());

            for (int i = 0; i < streamCount; i++)
            {
                int streamSize = buffer[5 + 0x18 + i * 2] + (buffer[6 + 0x18 + i * 2] << 8);
                if (streamSize != 0)
                    decoder.Code(new MemoryStream(buffer, offset, streamSize), result, streamSize, Math.Min(outSize, 0x10000), null);
                else
                    result.Write(buffer, offset, streamSize = Math.Min(outSize, 0x10000));
                outSize -= 0x10000;
                offset += streamSize;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Compress a given buffer compressed with EdgeLZMA segmented mode.
        /// <para>Compresse un tableau de bytes avec le codec EdgeLZMA en mode s�gment�.</para>
        /// </summary>
        /// <param name="inbuffer">The byte array to decompress.</param>
        /// <param name="safemode">Uses a proper error checking flag (disable for file bruteforcing).</param>
        /// <returns>A byte array.</returns>
        private static byte[] SegmentsDecompress(byte[] inbuffer, bool safemode) // Todo, make it multithreaded like original sdk.
        {
            bool LittleEndian = BitConverter.IsLittleEndian;
            try
            {
                if (inbuffer[0] == 0x73 && inbuffer[1] == 0x65 && inbuffer[2] == 0x67 && inbuffer[3] == 0x73)
                {
                    int numofsegments = BitConverter.ToInt16(!LittleEndian ? new byte[] { inbuffer[6], inbuffer[7] } : new byte[] { inbuffer[7], inbuffer[6] }, 0);
                    int OriginalSize = BitConverter.ToInt32(!LittleEndian ? new byte[] { inbuffer[8], inbuffer[9], inbuffer[10], inbuffer[11] } : new byte[] { inbuffer[11], inbuffer[10], inbuffer[9], inbuffer[8] }, 0);
                    int CompressedSize = BitConverter.ToInt32(!LittleEndian ? new byte[] { inbuffer[12], inbuffer[13], inbuffer[14], inbuffer[15] } : new byte[] { inbuffer[15], inbuffer[14], inbuffer[13], inbuffer[12] }, 0); // Unused during decompression.
                    byte[] TOCData = new byte[8 * numofsegments]; // 8 being size of each TOC entry.
                    byte[][] arrayOfArrays = new byte[numofsegments][];
                    Buffer.BlockCopy(inbuffer, 16, TOCData, 0, TOCData.Length);
                    // Check if the length of the byte array is evenly divisible by 8
                    if (TOCData.Length % 8 == 0)
                    {
                        int index = 0;
                        // Parse the byte array in blocks of 8
                        for (int i = 0; i < TOCData.Length; i += 8)
                        {
                            byte[] SegmentCompressedSizeByte = new byte[2];
                            byte[] SegmentOriginalSizeByte = new byte[2];
                            byte[] SegmentOffsetByte = new byte[4];
                            Buffer.BlockCopy(TOCData, i, SegmentCompressedSizeByte, 0, SegmentCompressedSizeByte.Length);
                            Buffer.BlockCopy(TOCData, i + 2, SegmentOriginalSizeByte, 0, SegmentOriginalSizeByte.Length);
                            Buffer.BlockCopy(TOCData, i + 4, SegmentOffsetByte, 0, SegmentOffsetByte.Length);
                            Array.Reverse(SegmentCompressedSizeByte);
                            Array.Reverse(SegmentOriginalSizeByte);
                            Array.Reverse(SegmentOffsetByte);
                            int SegmentCompressedSize = BitConverter.ToUInt16(!LittleEndian ? EndianUtils.EndianSwap(SegmentCompressedSizeByte) : SegmentCompressedSizeByte, 0);
                            int SegmentOriginalSize = BitConverter.ToUInt16(!LittleEndian ? EndianUtils.EndianSwap(SegmentOriginalSizeByte) : SegmentOriginalSizeByte, 0);
                            int SegmentOffset = 0;
                            byte[] CompressedData = Array.Empty<byte>();
                            if (SegmentCompressedSize <= 0) // Safer than just comparing with 0.
                            {
                                SegmentOffset = BitConverter.ToInt32(!LittleEndian ? EndianUtils.EndianSwap(SegmentOffsetByte) : SegmentOffsetByte, 0);
                                CompressedData = new byte[65536];
                            }
                            else
                            {
                                SegmentOffset = BitConverter.ToInt32(!LittleEndian ? EndianUtils.EndianSwap(SegmentOffsetByte) : SegmentOffsetByte, 0) - 1; // -1 cause there is an offset for compressed content... sdk bug?
                                CompressedData = new byte[SegmentCompressedSize];
                            }
                            Buffer.BlockCopy(inbuffer, SegmentOffset, CompressedData, 0, CompressedData.Length);
                            if (SegmentCompressedSize > 0 && SegmentCompressedSize <= 65536 && CompressedData.Length > 3 && CompressedData[0] == 0x5D && CompressedData[1] == 0x00 && CompressedData[2] == 0x00)
                            {
                                using (MemoryStream compressedStream = new MemoryStream(CompressedData))
                                {
                                    using (MemoryStream decompressedStream = new MemoryStream())
                                    {
                                        SegmentDecompress(compressedStream, decompressedStream);
                                        // Find the number of bytes in the stream
                                        int contentLength = (int)decompressedStream.Length;
                                        // Create a byte array
                                        byte[] buffer = new byte[contentLength];
                                        // Read the contents of the memory stream into the byte array
                                        decompressedStream.Read(buffer, 0, contentLength);
                                        arrayOfArrays[index] = buffer;
                                    }
                                }
                            }
                            else
                                arrayOfArrays[index] = CompressedData; // Can happen, just means segment is not compressed.

                            index++;
                        }

                        // Concatenate the byte arrays into a single byte array
                        byte[] FileData = ConcatenateArrays(arrayOfArrays);

                        if (FileData.Length == OriginalSize)
                            return FileData;
                        else if (safemode)
                        {
                            LoggerAccessor.LogError("[EdgeLzmaSegs] - File size is different than the one indicated in TOC!.");
                            return inbuffer;
                        }
                    }
                    else if (safemode)
                    {
                        LoggerAccessor.LogError("[EdgeLzmaSegs] - The byte array length is not evenly divisible by 8!");
                        return inbuffer;
                    }
                }
                else if (safemode)
                {
                    LoggerAccessor.LogError("[EdgeLzmaSegs] - File is not a valid segment based EdgeLzma compressed file!");
                    return inbuffer;
                }
            }
            catch (Exception ex)
            {
                if (safemode)
                {
                    LoggerAccessor.LogError($"[EdgeLzmaSegs] - SegmentsDecompress thrown an assertion : {ex}");
                    return inbuffer;
                }
            }

            return null;
        }

        /// <summary>
        /// Decompress a block of the segmented EdgeLZMA data.
        /// <para>D�compresse un block provenant d'une matrice de donn�e encod�e avec le codec EdgeLZMA.</para>
        /// </summary>
        /// <param name="inStream">The input LZMA stream.</param>
        /// <param name="outStream">The output stream.</param>
        /// <returns>Nothing.</returns>
        private static void SegmentDecompress(Stream inStream, Stream outStream)
        {
            byte[] properties = new byte[5];
            inStream.Read(properties, 0, 5);
            Decoder decoder = new Decoder();
            decoder.SetDecoderProperties(properties);
            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = inStream.ReadByte();
                outSize |= (long)(byte)v << 8 * i;
            }
            decoder.Code(inStream, outStream, inStream.Length - inStream.Position, outSize, null);
            outStream.Position = 0;
        }

        /// <summary>
        /// Compress a block of the segmented EdgeLZMA data.
        /// <para>Compresse un block provenant d'une matrice de donn�e encod�e avec le codec EdgeLZMA.</para>
        /// </summary>
        /// <param name="inStream">The input stream.</param>
        /// <param name="outStream">The output LZMA stream.</param>
        /// <returns>Nothing.</returns>
        private static void SegmentCompress(Stream inStream, Stream outStream)
        {
            Encoder encoder = new Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            using (MemoryStream strmOutStream = new MemoryStream())
            {
                encoder.WriteCoderProperties(strmOutStream);
                long fileSize = inStream.Length;
                for (int i = 0; i < 8; i++)
                strmOutStream.WriteByte((byte)(fileSize >> (8 * i)));
                encoder.Code(inStream, strmOutStream, -1, -1, null);
                strmOutStream.Position = 0;
                strmOutStream.CopyTo(outStream);
                outStream.Position = 0;
            }
        }

        /// <summary>
        /// Assemble an array of byte array to a single byte array.
        /// <para>Assemble un tableau de tableaux de bytes en un unique tableau de bytes.</para>
        /// </summary>
        /// <param name="arrays">The input array of byte array.</param>
        /// <returns>A byte array.</returns>
        private static byte[] ConcatenateArrays(byte[][] arrays)
        {
            byte[] result = new byte[arrays.Sum(arr => arr.Length)];
            int offset = 0;

            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }
    }
}
