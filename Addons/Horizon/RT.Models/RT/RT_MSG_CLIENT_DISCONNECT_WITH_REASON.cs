using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.LIBRARY.Common.Stream;

namespace MultiServer.Addons.Horizon.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_DISCONNECT_WITH_REASON)]
    public class RT_MSG_CLIENT_DISCONNECT_WITH_REASON : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_DISCONNECT_WITH_REASON;

        public RT_MSG_CLIENT_DISCONNECT_REASON disconnectReason;

        public override void Deserialize(MessageReader reader)
        {
            disconnectReason = reader.Read<RT_MSG_CLIENT_DISCONNECT_REASON>();
        }

        public override void Serialize(MessageWriter writer)
        {
            writer.Write(disconnectReason);
        }
        public override string ToString()
        {
            return base.ToString() + " " +
                $"Reason: {disconnectReason}";
        }
    }
}