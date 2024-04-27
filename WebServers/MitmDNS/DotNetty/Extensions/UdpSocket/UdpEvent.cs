using DotNetty.Transport.Channels;
using System;
using System.Net;

namespace DotNetty.Extensions.UdpSocket
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
