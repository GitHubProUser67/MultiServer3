using DotNetty.Transport.Channels;

namespace DotNetty.Extensions
{
    class TcpServerEvent
    {
        internal Action<IChannelPipeline>? OnPipelineAction;

        internal Action? OnStartAction;

        internal Action<TcpSocketConnection>? OnConnectionConnectAction;

        internal Action<TcpSocketConnection, byte[]>? OnConnectionReceiveAction;

        internal Action<TcpSocketConnection, Exception>? OnConnectionExceptionAction;

        internal Action<TcpSocketConnection>? OnConnectionCloseAction;

        internal Action<Exception>? OnStopAction;
    }
}
