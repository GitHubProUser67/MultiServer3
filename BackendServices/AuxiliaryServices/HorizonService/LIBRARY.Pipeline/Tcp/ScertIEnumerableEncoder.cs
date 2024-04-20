using CustomLogger;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Horizon.RT.Models;
using System;
using System.Collections.Generic;

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
            foreach (BaseScertMessage msg in messages)
                output.Add(msg);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            LoggerAccessor.LogError(exception.ToString());
            context.CloseAsync();
        }
    }
}
