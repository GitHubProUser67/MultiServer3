using CustomLogger;
using SevenZip.Compression.LZMA;

namespace CryptoSporidium
{
    public class EDGELZMA
    {
        public byte[]? Decompress(byte[] data, bool SegsMode)
        {
            if (SegsMode)
                return SegmentsDecompress(data);
            else
                return Decompress(data);
        }

        public byte[] Compress(byte[] buffer)
        {
            int numFastBytes = 64;
            int litContextBits = 3;
            int litPosBits = 0;
            int posStateBits = 2;
            int inSize = buffer.Length;
            int streamCount = inSize + 0xffff >> 16;
            int offset = 0;
            MemoryStream result = new();
            BinaryWriter bw = new(result);

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
            var props = new Dictionary<SevenZip.CoderPropID, object>();
            props[SevenZip.CoderPropID.DictionarySize] = 0x10000;
            props[SevenZip.CoderPropID.MatchFinder] = "BT4";
            props[SevenZip.CoderPropID.NumFastBytes] = numFastBytes;
            props[SevenZip.CoderPropID.LitContextBits] = litContextBits;
            props[SevenZip.CoderPropID.LitPosBits] = litPosBits;
            props[SevenZip.CoderPropID.PosStateBits] = posStateBits;
            //props[SevenZip.CoderPropID.BlockSize] = blockSize; // this always throws an exception when set
            //props[SevenZip.CoderPropID.MatchFinderCycles] = matchFinderCycles; // ^ same here
            //props[SevenZip.CoderPropID.DefaultProp] = 0;
            //props[SevenZip.CoderPropID.UsedMemorySize] = 100000;
            //props[SevenZip.CoderPropID.Order] = 1;
            //props[SevenZip.CoderPropID.NumPasses] = 10;
            //props[SevenZip.CoderPropID.Algorithm] = 0;
            //props[SevenZip.CoderPropID.NumThreads] = ;
            //props[SevenZip.CoderPropID.EndMarker] = ;

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

        public byte[] Decompress(byte[] inbuffer)
        {
            MemoryStream result = new();
            int outSize = BitConverter.ToInt32(inbuffer, 12);
            int streamCount = (outSize + 0xffff) >> 16;
            int offset = 0x18 + streamCount * 2 + 5;

            Decoder decoder = new();
            decoder.SetDecoderProperties(new MemoryStream(inbuffer, 0x18, 5).ToArray());

            for (int i = 0; i < streamCount; i++)
            {
                int streamSize = inbuffer[5 + 0x18 + i * 2] + (inbuffer[6 + 0x18 + i * 2] << 8);
                if (streamSize != 0)
                    decoder.Code(new MemoryStream(inbuffer, offset, streamSize), result, streamSize, Math.Min(outSize, 0x10000), null);
                else
                    result.Write(inbuffer, offset, streamSize = Math.Min(outSize, 0x10000));
                outSize -= 0x10000;
                offset += streamSize;
            }

            return result.ToArray();
        }

        public byte[]? SegmentsDecompress(byte[] inbuffer) // Todo, make it multithreaded like original sdk.
        {
            try
            {
                if (inbuffer[0] == 0x73 && inbuffer[1] == 0x65 && inbuffer[2] == 0x67 && inbuffer[3] == 0x73)
                {
                    int numofsegments = BitConverter.ToInt16(new byte[] { inbuffer[7], inbuffer[6] }, 0);
                    int OriginalSize = BitConverter.ToInt32(new byte[] { inbuffer[11], inbuffer[10], inbuffer[9], inbuffer[8] }, 0);
                    //int CompressedSize = BitConverter.ToInt32(new byte[] { inbuffer[15], inbuffer[14], inbuffer[13], inbuffer[12] }, 0); // Unused during decompression.
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
                            int SegmentCompressedSize = BitConverter.ToUInt16(SegmentCompressedSizeByte, 0);
                            int SegmentOriginalSize = BitConverter.ToUInt16(SegmentOriginalSizeByte, 0);
                            int SegmentOffset = 0;
                            byte[] CompressedData = Array.Empty<byte>();
                            if (SegmentCompressedSize <= 0) // Safer than just comparing with 0.
                            {
                                SegmentOffset = BitConverter.ToInt32(SegmentOffsetByte, 0);
                                CompressedData = new byte[65536];
                            }
                            else
                            {
                                SegmentOffset = BitConverter.ToInt32(SegmentOffsetByte, 0) - 1; // -1 cause there is an offset for compressed content... sdk bug?
                                CompressedData = new byte[SegmentCompressedSize];
                            }
                            Buffer.BlockCopy(inbuffer, SegmentOffset, CompressedData, 0, CompressedData.Length);
                            if (SegmentCompressedSize > 0 && SegmentCompressedSize <= 65536 && CompressedData.Length > 3 && CompressedData[0] == 0x5D && CompressedData[1] == 0x00 && CompressedData[2] == 0x00)
                            {
                                using (MemoryStream compressedStream = new(CompressedData))
                                {
                                    using (MemoryStream decompressedStream = new())
                                    {
                                        SegmentDecompress(compressedStream, decompressedStream);
                                        decompressedStream.Position = 0;
                                        // Find the number of bytes in the stream
                                        int contentLength = (int)decompressedStream.Length;
                                        // Create a byte array
                                        byte[] buffer = new byte[contentLength];
                                        // Read the contents of the memory stream into the byte array
                                        decompressedStream.Read(buffer, 0, contentLength);
                                        arrayOfArrays[index] = buffer;
                                        decompressedStream.Flush();
                                    }
                                    compressedStream.Flush();
                                }
                            }
                            else
                                arrayOfArrays[index] = CompressedData; // Can happen, just means segment is not compressed.

                            index++;
                        }

                        // Concatenate the byte arrays into a single byte array
                        byte[] FileData = new MiscUtils().ConcatenateArrays(arrayOfArrays);

                        if (FileData.Length == OriginalSize)
                            return FileData;
                        else
                        {
                            LoggerAccessor.LogError("[EdgeLzmaSegs] - File size is different than the one indicated in TOC! Sending input file instead.");
                            return inbuffer;
                        }
                    }
                    else
                        LoggerAccessor.LogError("[EdgeLzmaSegs] - The byte array length is not evenly divisible by 8, decompression failed!");
                }
                else
                    LoggerAccessor.LogError("[EdgeLzmaSegs] - File is not a valid segment based EdgeLzma compressed file!");
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[EdgeLzmaSegs] - SegmentsDecompress thrown an assertion : {ex}");
            }

            return null;
        }

        public static void SegmentDecompress(Stream inStream, Stream outStream)
        {
            byte[] properties = new byte[5];
            inStream.Read(properties, 0, 5);
            Decoder decoder = new();
            decoder.SetDecoderProperties(properties);
            long outSize = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = inStream.ReadByte();
                outSize |= ((long)(byte)v) << (8 * i);
            }
            long compressedSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, outStream, compressedSize, outSize, null);
        }
    }
}
