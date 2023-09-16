using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.RT.Cryptography;
using MultiServer.Addons.Horizon.RT.Models;

namespace MultiServer.Addons.Horizon.LIBRARY.Pipeline.Udp
{
    public class ScertDatagramDecoder : MessageToMessageDecoder<DatagramPacket>
    {
        readonly ICipher[] _ciphers = null;
        readonly Func<RT_MSG_TYPE, CipherContext, ICipher> _getCipher = null;

        public ScertDatagramDecoder(params ICipher[] ciphers)
        {
            _ciphers = ciphers;
            _getCipher = (id, ctx) =>
            {
                return _ciphers?.FirstOrDefault(x => x.Context == ctx);
            };
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            ServerConfiguration.LogWarn(exception.ToString());
        }

        protected override void Decode(IChannelHandlerContext context, DatagramPacket message, List<object> output)
        {
            while (message.Content.IsReadable())
            {
                object decoded = Decode(context, message);
                if (decoded == null)
                    break;

                output.Add(decoded);
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
        protected virtual object Decode(IChannelHandlerContext context, DatagramPacket input)
        {
            byte id = input.Content.GetByte(input.Content.ReaderIndex);
            byte[] hash = null;
            long frameLength = input.Content.GetShortLE(input.Content.ReaderIndex + 1);
            int headerLength = 3;

            if (!context.HasAttribute(Constants.SCERT_CLIENT))
                context.GetAttribute(Constants.SCERT_CLIENT).Set(new Attribute.ScertClientAttribute());
            var scertClient = context.GetAttribute(Constants.SCERT_CLIENT).Get();

            if (frameLength <= 0)
                return BaseScertMessage.Instantiate((RT_MSG_TYPE)(id & 0x7F), null, new byte[0], (int)scertClient.MediusVersion, scertClient.ApplicationID, scertClient.CipherService);

            if (id >= 0x80)
            {
                hash = new byte[4];
                input.Content.GetBytes(input.Content.ReaderIndex + 3, hash);
                headerLength += 4;
                id &= 0x7F;
            }

            if (frameLength < 0)
                throw new CorruptedFrameException("negative pre-adjustment length field: " + frameLength);

            // never overflows because it's less than maxFrameLength
            int frameLengthInt = (int)frameLength;
            if (input.Content.ReadableBytes < frameLengthInt)
                //input.ResetReaderIndex();
                return null;

            // extract frame
            byte[] messageContents = new byte[frameLengthInt];
            input.Content.GetBytes(input.Content.ReaderIndex + headerLength, messageContents);

            int totalFrameLength = headerLength + frameLengthInt;
            input.Content.SetReaderIndex(input.Content.ReaderIndex + totalFrameLength);
            return new ScertDatagramPacket(BaseScertMessage.Instantiate((RT_MSG_TYPE)id, hash, messageContents, (int)scertClient.MediusVersion, scertClient.ApplicationID, scertClient.CipherService), null, input.Sender);
        }
    }
}
