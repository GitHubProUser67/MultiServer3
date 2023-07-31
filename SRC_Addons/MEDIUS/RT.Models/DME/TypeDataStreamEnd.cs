using PSMultiServer.Addons.Medius.RT.Common;
using PSMultiServer.Addons.Medius.Server.Common.Stream;

namespace PSMultiServer.Addons.Medius.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassDME, MediusDmeMessageIds.DataStreamEnd)]
    public class TypeDataStreamEnd : BaseDMEMessage
    {

        public override byte PacketType => (byte)MediusDmeMessageIds.DataStreamEnd;

        public byte Channel;

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            Channel = reader.ReadByte();
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            // 
            writer.Write(Channel);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"Channel: {Channel}";
        }
    }
}