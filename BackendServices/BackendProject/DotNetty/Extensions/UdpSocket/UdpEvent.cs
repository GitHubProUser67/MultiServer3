using DotNetty.Transport.Channels;
using System.Net;

namespace BackendProject.DotNetty.Extensions.UdpSocket
{
    public class UdpEvent
    {
        internal Action<IChannelPipeline>? OnPipelineAction;

        internal Action? OnStartAction;

        internal Action<EndPoint, byte[]>? OnRecieveAction;

        internal Action<Exception>? OnExceptionAction;

        internal Action<Exception>? OnStopAction;
    }
}
