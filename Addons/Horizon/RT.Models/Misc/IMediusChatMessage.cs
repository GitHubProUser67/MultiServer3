using PSMultiServer.Addons.Horizon.RT.Common;

namespace PSMultiServer.Addons.Horizon.RT.Models.Misc
{
    public interface IMediusChatMessage
    {
        MediusChatMessageType MessageType { get; }
        int TargetID { get; }
        string Message { get; }
    }
}