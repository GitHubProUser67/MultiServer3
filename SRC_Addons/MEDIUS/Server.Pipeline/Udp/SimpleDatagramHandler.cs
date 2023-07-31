using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace PSMultiServer.Addons.Medius.Server.Pipeline.Udp
{
    public class SimpleDatagramHandler : SimpleChannelInboundHandler<DatagramPacket>
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<SimpleDatagramHandler>();

        public override bool IsSharable => true;


        public Action<IChannel> OnChannelActive;
        public Action<IChannel> OnChannelInactive;
        public Action<IChannel, DatagramPacket> OnChannelMessage;

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            // Detect when client disconnects
            ctx.Channel.CloseCompletion.ContinueWith((x) =>
            {
                ServerConfiguration.LogInfo("Channel Closed");
                OnChannelInactive?.Invoke(ctx.Channel);
            });

            // Send event upstream
            OnChannelActive?.Invoke(ctx.Channel);
        }

        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            ServerConfiguration.LogInfo("Client disconnected");

            // Send event upstream
            OnChannelInactive?.Invoke(ctx.Channel);
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, DatagramPacket message)
        {
            // Send upstream
            OnChannelMessage?.Invoke(ctx.Channel, message);
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            ServerConfiguration.LogError(exception);
        }
    }
}
