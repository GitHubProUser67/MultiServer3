using CustomLogger;
using DotNetty.Transport.Channels;

namespace BackendProject.Horizon.LIBRARY.Pipeline.Udp
{
    public class ScertDatagramHandler : SimpleChannelInboundHandler<ScertDatagramPacket>
    {
        public override bool IsSharable => true;


        public Action<IChannel> OnChannelActive;
        public Action<IChannel> OnChannelInactive;
        public Action<IChannel, ScertDatagramPacket> OnChannelMessage;

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            // Detect when client disconnects
            ctx.Channel.CloseCompletion.ContinueWith((x) =>
            {
                LoggerAccessor.LogInfo("[UDP] - Channel Closed");
                OnChannelInactive?.Invoke(ctx.Channel);
            });

            // Send event upstream
            OnChannelActive?.Invoke(ctx.Channel);
        }

        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            LoggerAccessor.LogInfo("[UDP] - Client disconnected");

            // Send event upstream
            OnChannelInactive?.Invoke(ctx.Channel);
        }


        protected override void ChannelRead0(IChannelHandlerContext ctx, ScertDatagramPacket message)
        {
            // Handle medius version
            var scertClient = ctx.GetAttribute(Constants.SCERT_CLIENT).Get();
            if (scertClient != null && scertClient.OnMessage(message.Message))
                ctx.GetAttribute(Constants.SCERT_CLIENT).Set(scertClient);

            // Send upstream
            OnChannelMessage?.Invoke(ctx.Channel, message);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            LoggerAccessor.LogWarn(exception.ToString());
        }
    }
}
