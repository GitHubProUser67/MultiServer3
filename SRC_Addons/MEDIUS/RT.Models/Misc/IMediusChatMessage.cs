using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models.Misc
{
    public interface IMediusChatMessage
    {
        MediusChatMessageType MessageType { get; }
        int TargetID { get; }
        string Message { get; }
    }
}