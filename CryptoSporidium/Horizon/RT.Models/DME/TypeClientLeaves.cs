using CryptoSporidium.Horizon.RT.Common;
using CryptoSporidium.Horizon.LIBRARY.Common.Stream;

namespace CryptoSporidium.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassDME, MediusDmeMessageIds.ClientLeaves)]
    public class TypeClientLeaves : BaseDMEMessage
    {
        public override byte PacketType => (byte)MediusDmeMessageIds.ClientLeaves;

        public byte ClientIndex;

        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);

            ClientIndex = reader.ReadByte();
        }

        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);

            writer.Write(ClientIndex);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"ClientIndex: {ClientIndex}";
        }
    }
}