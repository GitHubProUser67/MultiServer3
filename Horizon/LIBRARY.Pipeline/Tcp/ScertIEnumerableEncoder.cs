using CustomLogger;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Horizon.RT.Models;

namespace Horizon.LIBRARY.Pipeline.Tcp
{
    public class ScertIEnumerableEncoder : MessageToMessageEncoder<IEnumerable<BaseScertMessage>>
    {
        public ScertIEnumerableEncoder()
        {

        }

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
            LoggerAccessor.LogWarn(exception.ToString());
            context.CloseAsync();
        }

    }
}
