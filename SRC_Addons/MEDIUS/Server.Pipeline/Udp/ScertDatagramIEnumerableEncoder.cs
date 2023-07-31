using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using PSMultiServer.Addons.Medius.Server.Common;
using System.Net;

namespace PSMultiServer.Addons.Medius.Server.Pipeline.Udp
{
    public class ScertDatagramIEnumerableEncoder : MessageToMessageEncoder<IEnumerable<ScertDatagramPacket>>
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ScertDatagramIEnumerableEncoder>();

        readonly int maxPacketLength;

        public ScertDatagramIEnumerableEncoder(int maxPacketLength)
        {
            this.maxPacketLength = maxPacketLength;
        }

        protected override void Encode(IChannelHandlerContext ctx, IEnumerable<ScertDatagramPacket> messages, List<object> output)
        {
            List<byte[]> temp;
            Dictionary<EndPoint, List<byte[]>> msgsByEndpoint = new Dictionary<EndPoint, List<byte[]>>();
            if (messages is null)
                return;

            //
            if (!ctx.HasAttribute(Constants.SCERT_CLIENT))
                ctx.GetAttribute(Constants.SCERT_CLIENT).Set(new Attribute.ScertClientAttribute());
            var scertClient = ctx.GetAttribute(Constants.SCERT_CLIENT).Get();

            // Serialize and add
            foreach (var msg in messages)
            {
                if (!msgsByEndpoint.TryGetValue(msg.Destination, out temp))
                    msgsByEndpoint.Add(msg.Destination, temp = new List<byte[]>());

                temp.AddRange(msg.Message.Serialize(scertClient.MediusVersion, scertClient.ApplicationID, scertClient.CipherService));
            }

            foreach (var kvp in msgsByEndpoint)
            {
                // Condense as much as possible
                if (kvp.Value.Count > 1)
                {

                }
                var condensedMsgs = kvp.Value.GroupWhileAggregating(0, (sum, item) => sum + item.Length, (sum, item) => sum < maxPacketLength);

                // 
                foreach (var msgGroup in condensedMsgs)
                {
                    var byteBuffer = ctx.Allocator.Buffer(msgGroup.Sum(x => x.Length));
                    foreach (var msg in msgGroup)
                        byteBuffer.WriteBytes(msg);
                    output.Add(new DatagramPacket(byteBuffer, kvp.Key));
                }
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Logger.Error(exception);
            context.CloseAsync();
        }
    }
}
