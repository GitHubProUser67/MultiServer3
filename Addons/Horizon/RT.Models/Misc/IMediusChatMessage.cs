using MultiServer.Addons.Horizon.RT.Common;

namespace MultiServer.Addons.Horizon.RT.Models.Misc
{
    public interface IMediusChatMessage
    {
        MediusChatMessageType MessageType { get; }
        int TargetID { get; }
        string Message { get; }
    }
}