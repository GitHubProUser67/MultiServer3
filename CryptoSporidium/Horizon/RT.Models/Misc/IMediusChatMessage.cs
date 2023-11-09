using CryptoSporidium.Horizon.RT.Common;

namespace CryptoSporidium.Horizon.RT.Models.Misc
{
    public interface IMediusChatMessage
    {
        MediusChatMessageType MessageType { get; }
        int TargetID { get; }
        string Message { get; }
    }
}