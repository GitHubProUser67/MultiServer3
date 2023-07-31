using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;

namespace PSMultiServer.Addons.Medius.Server.Pipeline.Tcp
{
    public class ScertTcpFrameDecoder : ByteToMessageDecoder
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ScertTcpFrameDecoder>();

        readonly ByteOrder byteOrder;
        readonly int maxFrameLength;
        readonly int lengthFieldOffset;
        readonly int lengthFieldLength;
        readonly int lengthFieldEndOffset;
        readonly int lengthAdjustment;
        readonly int initialBytesToStrip;
        readonly bool failFast;
        bool discardingTooLongFrame;
        long tooLongFrameLength;
        long bytesToDiscard;

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">
        ///     The maximum length of the frame.  If the length of the frame is
        ///     greater than this value then <see cref="TooLongFrameException" /> will be thrown.
        /// </param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        public ScertTcpFrameDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength)
            : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, 0, 0)
        {
        }

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">
        ///     The maximum length of the frame.  If the length of the frame is
        ///     greater than this value then <see cref="TooLongFrameException" /> will be thrown.
        /// </param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        /// <param name="initialBytesToStrip">the number of first bytes to strip out from the decoded frame.</param>
        public ScertTcpFrameDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip)
            : this(maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment, initialBytesToStrip, true)
        {
        }

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="maxFrameLength">
        ///     The maximum length of the frame.  If the length of the frame is
        ///     greater than this value then <see cref="TooLongFrameException" /> will be thrown.
        /// </param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        /// <param name="initialBytesToStrip">the number of first bytes to strip out from the decoded frame.</param>
        /// <param name="failFast">
        ///     If <c>true</c>, a <see cref="TooLongFrameException" /> is thrown as soon as the decoder notices the length
        ///     of the frame will exceeed <see cref="maxFrameLength" /> regardless of whether the entire frame has been
        ///     read. If <c>false</c>, a <see cref="TooLongFrameException" /> is thrown after the entire frame that exceeds
        ///     <see cref="maxFrameLength" /> has been read.
        ///     Defaults to <c>true</c> in other overloads.
        /// </param>
        public ScertTcpFrameDecoder(int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip, bool failFast)
            : this(ByteOrder.BigEndian, maxFrameLength, lengthFieldOffset, lengthFieldLength, lengthAdjustment, initialBytesToStrip, failFast)
        {
        }

        /// <summary>
        ///     Create a new instance.
        /// </summary>
        /// <param name="byteOrder">The <see cref="ByteOrder" /> of the lenght field.</param>
        /// <param name="maxFrameLength">
        ///     The maximum length of the frame.  If the length of the frame is
        ///     greater than this value then <see cref="TooLongFrameException" /> will be thrown.
        /// </param>
        /// <param name="lengthFieldOffset">The offset of the length field.</param>
        /// <param name="lengthFieldLength">The length of the length field.</param>
        /// <param name="lengthAdjustment">The compensation value to add to the value of the length field.</param>
        /// <param name="initialBytesToStrip">the number of first bytes to strip out from the decoded frame.</param>
        /// <param name="failFast">
        ///     If <c>true</c>, a <see cref="TooLongFrameException" /> is thrown as soon as the decoder notices the length
        ///     of the frame will exceeed <see cref="maxFrameLength" /> regardless of whether the entire frame has been
        ///     read. If <c>false</c>, a <see cref="TooLongFrameException" /> is thrown after the entire frame that exceeds
        ///     <see cref="maxFrameLength" /> has been read.
        ///     Defaults to <c>true</c> in other overloads.
        /// </param>
        public ScertTcpFrameDecoder(ByteOrder byteOrder, int maxFrameLength, int lengthFieldOffset, int lengthFieldLength, int lengthAdjustment, int initialBytesToStrip, bool failFast)
        {
            if (maxFrameLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFrameLength), "maxFrameLength must be a positive integer: " + maxFrameLength);
            }
            if (lengthFieldOffset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lengthFieldOffset), "lengthFieldOffset must be a non-negative integer: " + lengthFieldOffset);
            }
            if (initialBytesToStrip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialBytesToStrip), "initialBytesToStrip must be a non-negative integer: " + initialBytesToStrip);
            }
            if (lengthFieldOffset > maxFrameLength - lengthFieldLength)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFrameLength), "maxFrameLength (" + maxFrameLength + ") " +
                    "must be equal to or greater than " +
                    "lengthFieldOffset (" + lengthFieldOffset + ") + " +
                    "lengthFieldLength (" + lengthFieldLength + ").");
            }

            this.byteOrder = byteOrder;
            this.maxFrameLength = maxFrameLength;
            this.lengthFieldOffset = lengthFieldOffset;
            this.lengthFieldLength = lengthFieldLength;
            this.lengthAdjustment = lengthAdjustment;
            this.lengthFieldEndOffset = lengthFieldOffset + lengthFieldLength;
            this.initialBytesToStrip = initialBytesToStrip;
            this.failFast = failFast;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            object decoded = this.Decode(context, input);
            if (decoded != null)
            {
                output.Add(decoded);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            ServerConfiguration.LogError(exception);
            context.CloseAsync();
        }

        /// <summary>
        ///     Create a frame out of the <see cref="IByteBuffer" /> and return it.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="IChannelHandlerContext" /> which this <see cref="ByteToMessageDecoder" /> belongs
        ///     to.
        /// </param>
        /// <param name="input">The <see cref="IByteBuffer" /> from which to read data.</param>
        /// <returns>The <see cref="IByteBuffer" /> which represents the frame or <c>null</c> if no frame could be created.</returns>
        protected virtual object Decode(IChannelHandlerContext context, IByteBuffer input)
        {
            if (this.discardingTooLongFrame)
            {
                long bytesToDiscard = this.bytesToDiscard;
                int localBytesToDiscard = (int)Math.Min(bytesToDiscard, input.ReadableBytes);
                input.SkipBytes(localBytesToDiscard);
                bytesToDiscard -= localBytesToDiscard;
                this.bytesToDiscard = bytesToDiscard;

                this.FailIfNecessary(false);
            }

            if (input.ReadableBytes < this.lengthFieldEndOffset)
            {
                return null;
            }

            int actualLengthFieldOffset = input.ReaderIndex + this.lengthFieldOffset;
            long frameLength = this.GetUnadjustedFrameLength(input, actualLengthFieldOffset, this.lengthFieldLength, this.byteOrder);

            if (frameLength < 0)
            {
                input.SkipBytes(this.lengthFieldEndOffset);
                throw new CorruptedFrameException("negative pre-adjustment length field: " + frameLength);
            }

            bool signed = input.GetByte(input.ReaderIndex) >= 0x80;
            frameLength += this.lengthAdjustment + this.lengthFieldEndOffset + ((signed && frameLength > 0) ? 4 : 0);

            if (frameLength < this.lengthFieldEndOffset)
            {
                input.SkipBytes(this.lengthFieldEndOffset);
                throw new CorruptedFrameException("Adjusted frame length (" + frameLength + ") is less " +
                    "than lengthFieldEndOffset: " + this.lengthFieldEndOffset);
            }

            if (frameLength > this.maxFrameLength)
            {
                int startOff = (int)Math.Min(20, input.ArrayOffset);
                throw new CorruptedFrameException($"{context.Channel.RemoteAddress} Frame Length exceeds max frame length on buffer: start:{startOff} {BitConverter.ToString(input.Array, input.ArrayOffset - startOff, startOff + input.ReadableBytes)}");
            }

            // never overflows because it's less than maxFrameLength
            int frameLengthInt = (int)frameLength;
            if (input.ReadableBytes < frameLengthInt)
            {
                return null;
            }

            if (this.initialBytesToStrip > frameLengthInt)
            {
                input.SkipBytes(frameLengthInt);
                throw new CorruptedFrameException("Adjusted frame length (" + frameLength + ") is less " +
                    "than initialBytesToStrip: " + this.initialBytesToStrip);
            }
            input.SkipBytes(this.initialBytesToStrip);

            // extract frame
            int readerIndex = input.ReaderIndex;
            int actualFrameLength = frameLengthInt - this.initialBytesToStrip;
            IByteBuffer frame = this.ExtractFrame(context, input, readerIndex, actualFrameLength);
            input.SetReaderIndex(readerIndex + actualFrameLength);
            return frame;
        }

        /// <summary>
        ///     Decodes the specified region of the buffer into an unadjusted frame length.  The default implementation is
        ///     capable of decoding the specified region into an unsigned 8/16/24/32/64 bit integer.  Override this method to
        ///     decode the length field encoded differently.
        ///     Note that this method must not modify the state of the specified buffer (e.g.
        ///     <see cref="IByteBuffer.ReaderIndex" />,
        ///     <see cref="IByteBuffer.WriterIndex" />, and the content of the buffer.)
        /// </summary>
        /// <param name="buffer">The buffer we'll be extracting the frame length from.</param>
        /// <param name="offset">The offset from the absolute <see cref="IByteBuffer.ReaderIndex" />.</param>
        /// <param name="length">The length of the framelenght field. Expected: 1, 2, 3, 4, or 8.</param>
        /// <param name="order">The preferred <see cref="ByteOrder" /> of buffer.</param>
        /// <returns>A long integer that represents the unadjusted length of the next frame.</returns>
        protected virtual long GetUnadjustedFrameLength(IByteBuffer buffer, int offset, int length, ByteOrder order)
        {
            long frameLength;
            switch (length)
            {
                case 1:
                    frameLength = buffer.GetByte(offset);
                    break;
                case 2:
                    frameLength = order == ByteOrder.BigEndian ? buffer.GetUnsignedShort(offset) : buffer.GetUnsignedShortLE(offset);
                    break;
                case 3:
                    frameLength = order == ByteOrder.BigEndian ? buffer.GetUnsignedMedium(offset) : buffer.GetUnsignedMediumLE(offset);
                    break;
                case 4:
                    frameLength = order == ByteOrder.BigEndian ? buffer.GetInt(offset) : buffer.GetIntLE(offset);
                    break;
                case 8:
                    frameLength = order == ByteOrder.BigEndian ? buffer.GetLong(offset) : buffer.GetLongLE(offset);
                    break;
                default:
                    throw new DecoderException("unsupported lengthFieldLength: " + this.lengthFieldLength + " (expected: 1, 2, 3, 4, or 8)");
            }
            return frameLength;
        }

        protected virtual IByteBuffer ExtractFrame(IChannelHandlerContext context, IByteBuffer buffer, int index, int length)
        {
            IByteBuffer buff = buffer.Slice(index, length);
            buff.Retain();
            return buff;
        }

        void FailIfNecessary(bool firstDetectionOfTooLongFrame)
        {
            if (this.bytesToDiscard == 0)
            {
                // Reset to the initial state and tell the handlers that
                // the frame was too large.
                long tooLongFrameLength = this.tooLongFrameLength;
                this.tooLongFrameLength = 0;
                this.discardingTooLongFrame = false;
                if (!this.failFast ||
                    this.failFast && firstDetectionOfTooLongFrame)
                {
                    this.Fail(tooLongFrameLength);
                }
            }
            else
            {
                // Keep discarding and notify handlers if necessary.
                if (this.failFast && firstDetectionOfTooLongFrame)
                {
                    this.Fail(this.tooLongFrameLength);
                }
            }
        }

        void Fail(long frameLength)
        {
            if (frameLength > 0)
            {
                throw new TooLongFrameException("Adjusted frame length exceeds " + this.maxFrameLength +
                    ": " + frameLength + " - discarded");
            }
            else
            {
                throw new TooLongFrameException(
                    "Adjusted frame length exceeds " + this.maxFrameLength +
                        " - discarding");
            }
        }
    }
}
