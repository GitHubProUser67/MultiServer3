using DotNetty.Common.Internal.Logging;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;

namespace PSMultiServer.SRC_Addons.MEDIUS.Server.Plugins.Interface
{
    public delegate Task OnRegisterActionHandler(PluginEvent eventType, object data);
    public delegate Task OnRegisterMessageActionHandler(RT_MSG_TYPE msgId, object data);
    public delegate Task OnRegisterMediusMessageActionHandler(NetMessageClass msgClass, byte msgType, object data);

    public interface IPluginHost
    {
        void Log(InternalLogLevel level, string message);
        void Log(InternalLogLevel level, Exception ex);
        void Log(InternalLogLevel level, Exception ex, string message);

        void RegisterAction(PluginEvent eventType, OnRegisterActionHandler callback);
        void RegisterMessageAction(RT_MSG_TYPE msgId, OnRegisterMessageActionHandler callback);
        void RegisterMediusMessageAction(NetMessageClass msgClass, byte msgType, OnRegisterMediusMessageActionHandler callback);
    }
}
