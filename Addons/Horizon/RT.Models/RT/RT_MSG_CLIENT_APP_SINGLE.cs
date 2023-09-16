using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.LIBRARY.Common.Stream;

namespace MultiServer.Addons.Horizon.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_APP_SINGLE)]
    public class RT_MSG_CLIENT_APP_SINGLE : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_APP_SINGLE;

        public short TargetOrSource = 0;
        public byte[] Payload;

        public override void Deserialize(MessageReader reader)
        {
            TargetOrSource = reader.ReadInt16();
            Payload = reader.ReadRest();
        }

        public override void Serialize(MessageWriter writer)
        {
            writer.Write(TargetOrSource);
            writer.Write(Payload);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"TargetOrSource: {TargetOrSource} " +
                $"Payload: {(Payload == null ? "" : BitConverter.ToString(Payload))}";
        }
    }
}