using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using PSMultiServer.Addons.Horizon.RT.Models;

namespace PSMultiServer.Addons.Horizon.Server.Pipeline.Tcp
{
    public class ScertIEnumerableEncoder : MessageToMessageEncoder<IEnumerable<BaseScertMessage>>
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ScertIEnumerableEncoder>();

        public ScertIEnumerableEncoder()
        { }

        protected override void Encode(IChannelHandlerContext ctx, IEnumerable<BaseScertMessage> messages, List<object> output)
        {
            if (messages is null)
                return;

            // Serialize and add
            foreach (var msg in messages)
                output.Add(msg);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            ServerConfiguration.LogError(exception);
            context.CloseAsync();
        }

    }
}
