using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassDME, MediusDmeMessageIds.AcceptClient)]
    public class TypeAcceptClient : BaseDMEMessage
    {

        public override byte PacketType => (byte)MediusDmeMessageIds.AcceptClient;

        public short NetObjectBufferStart;
        public short NetDataStreamStart;

        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);

            NetObjectBufferStart = reader.ReadInt16();
            NetDataStreamStart = reader.ReadInt16();
        }

        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);

            writer.Write(NetObjectBufferStart);
            writer.Write(NetDataStreamStart);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"NetObjectBufferStart: {NetObjectBufferStart} " +
                $"NetDataStreamStart: {NetDataStreamStart}";
        }
    }
}