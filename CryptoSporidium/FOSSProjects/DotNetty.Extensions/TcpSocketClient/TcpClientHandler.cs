using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace DotNetty.Extensions
{
    class TcpClientHandler : SimpleChannelInboundHandler<object>
    {
        private TcpSocketClient _client;

        public TcpClientHandler(TcpSocketClient client)
        {
            _client = client;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            _client.channel = context.Channel;
            _client._event.OnConnectAction?.Invoke();
        }

        protected override void ChannelRead0(IChannelHandlerContext context, object msg)
        {
            var buffer = msg as IByteBuffer;
            if (buffer != null)
                _client._event.OnReceiveAction?.Invoke(buffer.ToBytes());
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _client._event.OnExceptionAction?.Invoke(exception);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _client._event.OnCloseAction?.Invoke(new Exception("ChannelInactive"));
        }

        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent)
            {
                var eventState = evt as IdleStateEvent;
                if (eventState != null)
                    context.Channel.CloseAsync();
            }
        }
    }
}
