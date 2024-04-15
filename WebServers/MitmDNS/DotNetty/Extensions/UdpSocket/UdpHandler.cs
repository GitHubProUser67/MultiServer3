using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace DotNetty.Extensions.UdpSocket
{
    public class UdpHandler : SimpleChannelInboundHandler<DatagramPacket>
    {
        private UdpEvent _event;
        public UdpHandler(UdpEvent evn)
        {
            _event = evn;
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, DatagramPacket msg)
        {
            _event.OnRecieveAction?.Invoke(msg.Sender, msg.Content.ToBytes());
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _event.OnExceptionAction?.Invoke(exception);
        }
    }
}
