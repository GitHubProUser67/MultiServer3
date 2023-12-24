using BackendProject.Horizon.RT.Common;

namespace BackendProject.Horizon.RT.Models.Misc
{
    public interface IMediusChatMessage
    {
        MediusChatMessageType MessageType { get; }
        int TargetID { get; }
        string Message { get; }
    }
}