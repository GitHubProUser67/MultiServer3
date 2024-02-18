using CustomLogger;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.RT.Models;

namespace BackendProject.Horizon.LIBRARY.Pipeline.Tcp
{
    public class ScertDecoder : MessageToMessageDecoder<IByteBuffer>
    {
        /// <summary>
        ///     Create a new instance.
        /// </summary>
        public ScertDecoder()
        {

        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                var decoded = Decode(context, input);
                if (decoded != null)
                    output.Add(decoded);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogWarn(ex.ToString());
            }
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
        protected virtual object? Decode(IChannelHandlerContext context, IByteBuffer input)
        {
            //input.MarkReaderIndex();
            byte id = input.GetByte(input.ReaderIndex);
            byte[]? hash = null;
            long frameLength = input.GetShortLE(input.ReaderIndex + 1);
            int totalLength = 3;

            if (!context.HasAttribute(Constants.SCERT_CLIENT))
                context.GetAttribute(Constants.SCERT_CLIENT).Set(new Attribute.ScertClientAttribute());
            var scertClient = context.GetAttribute(Constants.SCERT_CLIENT).Get();

            if (frameLength <= 0)
                return BaseScertMessage.Instantiate((RT_MSG_TYPE)(id & 0x7F), null, new byte[0], (int)scertClient.MediusVersion, scertClient.ApplicationID, scertClient.CipherService);

            if (id >= 0x80)
            {
                hash = new byte[4];
                input.GetBytes(input.ReaderIndex + 3, hash);
                totalLength += 4;
                id &= 0x7F;
            }

            if (frameLength < 0)
            {
                LoggerAccessor.LogError("negative pre-adjustment length field: " + frameLength);
                return null;
            }

            // never overflows because it's less than maxFrameLength
            int frameLengthInt = (int)frameLength;
            if (input.ReadableBytes < frameLengthInt)
                //input.ResetReaderIndex();
                return null;

            // extract frame
            byte[] messageContents = new byte[frameLengthInt];
            input.GetBytes(input.ReaderIndex + totalLength, messageContents);

            input.SetReaderIndex(input.ReaderIndex + totalLength + frameLengthInt);
            return BaseScertMessage.Instantiate((RT_MSG_TYPE)id, hash, messageContents, (int)scertClient.MediusVersion, scertClient.ApplicationID, scertClient.CipherService);
        }
    }
}
