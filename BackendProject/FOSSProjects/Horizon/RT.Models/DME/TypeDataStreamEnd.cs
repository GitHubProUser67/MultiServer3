using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassDME, MediusDmeMessageIds.DataStreamEnd)]
    public class TypeDataStreamEnd : BaseDMEMessage
    {
        public override byte PacketType => (byte)MediusDmeMessageIds.DataStreamEnd;

        public byte Channel;

        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);

            Channel = reader.ReadByte();
        }

        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);

            writer.Write(Channel);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Channel: {Channel}";
        }
    }
}