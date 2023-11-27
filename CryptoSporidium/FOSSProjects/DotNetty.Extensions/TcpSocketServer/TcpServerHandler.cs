using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using System.Collections.Concurrent;

namespace DotNetty.Extensions
{
    class TcpServerHandler : SimpleChannelInboundHandler<object>
    {
        private TcpServerEvent _event;

        private ConcurrentDictionary<string, TcpSocketConnection> _dict;

        public TcpServerHandler(TcpServerEvent evt, ConcurrentDictionary<string, TcpSocketConnection> dict)
        {
            _event = evt;
            _dict = dict;
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            var conn = new TcpSocketConnection(context.Channel);
            _dict.TryAdd(context.Channel.Id.AsShortText(), conn);
            _event.OnConnectionConnectAction?.Invoke(conn);
        }

        protected override void ChannelRead0(IChannelHandlerContext context, object msg)
        {
            _dict.TryGetValue(context.Channel.Id.AsShortText(), out TcpSocketConnection? conn);
            var buffer = msg as IByteBuffer;
            if (conn != null && buffer != null)
                _event.OnConnectionReceiveAction?.Invoke(conn, buffer.ToBytes());
        }

        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _dict.TryGetValue(context.Channel.Id.AsShortText(), out TcpSocketConnection? conn);
            if (conn != null)
                _event.OnConnectionExceptionAction?.Invoke(conn, exception);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _dict.TryRemove(context.Channel.Id.AsShortText(), out TcpSocketConnection? conn);
            if (conn != null)
                _event.OnConnectionCloseAction?.Invoke(conn);
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
