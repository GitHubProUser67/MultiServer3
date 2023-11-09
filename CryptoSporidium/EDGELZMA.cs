using CustomLogger;
using System.Buffers.Binary;

namespace CryptoSporidium
{
    public class EDGELZMA
    {
        public byte[] Compress(byte[] data, bool TLZC)
        {
            if (TLZC)
                return new EDGE().Compress(data, true);
            else
                return new EDGE().Compress(data, false);
        }

        public byte[] Decompress(byte[] data)
        {
            return new EDGE().Decompress(data);
        }

        class EDGE
        {
            public byte[] Compress(byte[] buffer, bool TLZC)
            {
                int numFastBytes = 64;
                int litContextBits = 3;
                int litPosBits = 0;
                int posStateBits = 2;
                MemoryStream result = new MemoryStream();
                int inSize = buffer.Length;
                int streamCount = inSize + 0xffff >> 16;
                int offset = 0;

                BinaryWriter bw = new BinaryWriter(result);

                if (TLZC) // EdgeLZMA Version 4.
                {
                    bw.Write(new byte[] { 0x54, 0x4C, 0x5A, 0x43 });
                    bw.Write((byte)0x01);
                    bw.Write((byte)0x04);
                    bw.Write((byte)0x00);
                    bw.Write((byte)0x00);
                    bw.Write(0);   // compressed size - we'll fill this in once we know it
                    bw.Write(buffer.Length);   // decompressed size
                    bw.Write(0);   // unknown, 0
                    bw.Write(0);   // unknown, 0
                }
                else // EdgeLZMA Version 5.
                {
                    bw.Write(new byte[] { 0x73, 0x65, 0x67, 0x73 }); // Home Magic.
                    bw.Write((byte)0x01);
                    bw.Write((byte)0x05); // Home expect version 5.
                    bw.Write(new byte[] { 0x00, 0x02 }); // Num of segments, we will set it later.
                    bw.Write(0); // Decompressed size, we will set it later.
                    bw.Write(0); // Unknown.
                    bw.Write(0); // Unknown.
                    bw.Write(0); // Unknown.
                    // Compressed size in coder properties, aka , 12th byte.
                }
                var encoder = new SevenZip.Compression.LZMA.Encoder(); // next comes the coder properties (5 bytes), followed by stream lengths, followed by the streams themselves.
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

                ushort segmentnumber = 0;

                for (int i = 0; i < streamCount; i++)
                {
                    long preLength = result.Length;

                    encoder.Code(new MemoryStream(buffer, offset, Math.Min(inSize, 0x10000)), result, Math.Min(inSize, 0x10000), -1, null);

                    int streamSize = (int)(result.Length - preLength);
                    if (streamSize >= 0x10000)
                    {
                        LoggerAccessor.LogDebug("[EdgeLzma] - Warning - Stream did not compress.");
                        result.Position = preLength;
                        result.SetLength(preLength);
                        result.Write(buffer, offset, Math.Min(inSize, 0x10000));
                        streamSize = 0;
                    }

                    inSize -= 0x10000;
                    offset += 0x10000;
                    streamSizes.Add(streamSize);

                    segmentnumber =+ 1;
                }

                if (TLZC) // EdgeLZMA Version 4.
                {
                    // fill in compressed size
                    result.Position = 8;
                    bw.Write((int)result.Length); // Endian swap handled internally.
                }
                else // EdgeLZMA Version 5.
                {
                    result.Position = 6;
                    MiscUtils? utils = new();
                    bw.Write(utils.endianSwap(BitConverter.GetBytes(segmentnumber)));
                    utils = null;

                    result.Position = 8;
                    bw.Write(BinaryPrimitives.ReverseEndianness(buffer.Length));

                    result.Position = 12;
                    bw.Write(BinaryPrimitives.ReverseEndianness((int)result.Length));
                }

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
                bool TLZC = false;

                if (inbuffer[0] == 'T' && inbuffer[1] == 'L' && inbuffer[2] == 'Z' && inbuffer[3] == 'C')
                    TLZC = true;

                MemoryStream result = new MemoryStream();
                int outSize = 0;

                if (TLZC)
                    outSize = BitConverter.ToInt32(inbuffer, 12);
                else
                    outSize = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(inbuffer, 8));

                int streamCount = (outSize + 0xffff) >> 16;
                int offset = 0x18 + streamCount * 2 + 5;

                var decoder = new SevenZip.Compression.LZMA.Decoder();
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
        }
    }
}
