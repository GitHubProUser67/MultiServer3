using System.Text;

namespace PSMultiServer.CryptoSporidium
{
    public class EDGELZMA
    {
        public static byte[] Compress(byte[] data, bool TLZC)
        {
            if (TLZC)
            {
                return new EDGE().Compress(data, true);
            }
            else
            {
                return new EDGE().Compress(data, false);
            }
        }

        public static byte[] Decompress(byte[] data)
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

                bw.Write(new byte[] { 0x54, 0x4C, 0x5A, 0x43 });
                bw.Write((byte)0x01);
                bw.Write((byte)0x04);
                bw.Write((byte)0x00);
                bw.Write((byte)0x00);
                bw.Write(0);   // compressed size - we'll fill this in once we know it
                bw.Write(buffer.Length);   // decompressed size
                bw.Write(0);   // unknown, 0
                bw.Write(0);   // unknown, 0
                                    // next comes the coder properties (5 bytes), followed by stream lengths, followed by the streams themselves.

                var encoder = new SevenZip.Compression.LZMA.Encoder();
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
                        System.Diagnostics.Trace.WriteLine("Warning! stream did not compress at all. This will cause a different code path to be executed on the PS3 whose operation is assumed and not tested!");
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

                if (!TLZC) // Yeah, a bit ... mhee ... but it works for now ^^.
                {
                    byte[] dst = new byte[temp.Length - 4];

                    Array.Copy(temp, 4, dst, 0, dst.Length);

                    return dst;
                }
                else
                {
                    return temp;

                }
            }

            public byte[] Decompress(byte[] inbuffer)
            {
                byte[] buffer;

                if (inbuffer[0] != 'T' && inbuffer[1] != 'L' && inbuffer[2] != 'Z' && inbuffer[3] != 'C')
                {
                    buffer = Misc.Combinebytearay(Encoding.UTF8.GetBytes("TLZC"), inbuffer);
                }
                else
                    buffer = inbuffer;

                MemoryStream result = new MemoryStream();
                int outSize = BitConverter.ToInt32(buffer, 12);
                int streamCount = (outSize + 0xffff) >> 16;
                int offset = 0x18 + streamCount * 2 + 5;

                var decoder = new SevenZip.Compression.LZMA.Decoder();
                decoder.SetDecoderProperties(new MemoryStream(buffer, 0x18, 5).ToArray());

                for (int i = 0; i < streamCount; i++)
                {
                    int streamSize = (buffer[5 + 0x18 + i * 2]) + (buffer[6 + 0x18 + i * 2] << 8);
                    if (streamSize != 0)
                        decoder.Code(new MemoryStream(buffer, offset, streamSize), result, streamSize, Math.Min(outSize, 0x10000), null);
                    else
                        result.Write(buffer, offset, streamSize = Math.Min(outSize, 0x10000));
                    outSize -= 0x10000;
                    offset += streamSize;
                }

                return result.ToArray();

            }
        }
    }
}
