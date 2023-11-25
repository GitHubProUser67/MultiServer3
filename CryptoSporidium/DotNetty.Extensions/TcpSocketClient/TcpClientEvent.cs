using DotNetty.Transport.Channels;

namespace DotNetty.Extensions
{
    class TcpClientEvent
    {
        internal Action<IChannelPipeline>? OnPipelineAction;

        internal Action? OnConnectAction;

        internal Action<byte[]>? OnReceiveAction;

        internal Action<Exception>? OnExceptionAction;

        internal Action<Exception>? OnCloseAction;
    }
}
