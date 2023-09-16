using MultiServer.Addons.Horizon.RT.Common;

namespace MultiServer.PluginManager
{
    public delegate Task OnRegisterActionHandler(PluginEvent eventType, object data);
    public delegate Task OnRegisterMessageActionHandler(RT_MSG_TYPE msgId, object data);
    public delegate Task OnRegisterMediusMessageActionHandler(NetMessageClass msgClass, byte msgType, object data);

    public interface IPluginHost
    {
        void RegisterAction(PluginEvent eventType, OnRegisterActionHandler callback);
        void RegisterMessageAction(RT_MSG_TYPE msgId, OnRegisterMessageActionHandler callback);
        void RegisterMediusMessageAction(NetMessageClass msgClass, byte msgType, OnRegisterMediusMessageActionHandler callback);
    }
}
