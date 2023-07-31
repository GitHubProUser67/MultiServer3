using PSMultiServer.Addons.Medius.RT.Common;
using PSMultiServer.Addons.Medius.Server.Common.Stream;

namespace PSMultiServer.Addons.Medius.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.ClanLadderPosition)]
    public class MediusClanLadderPositionRequest : BaseLobbyMessage, IMediusRequest
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.ClanLadderPosition;



        public MessageId MessageID { get; set; }
        public int ClanID;
        public int ClanLadderStatIndex;
        public MediusSortOrder SortOrder;

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            MessageID = reader.Read<MessageId>();
            reader.ReadBytes(3);
            ClanID = reader.ReadInt32();
            ClanLadderStatIndex = reader.ReadInt32();
            SortOrder = reader.Read<MediusSortOrder>();
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            // 
            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(new byte[3]);
            writer.Write(ClanID);
            writer.Write(ClanLadderStatIndex);
            writer.Write(SortOrder);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID:{MessageID}" + " " +
                $"ClanID:{ClanID}" + " " +
                $"ClanLadderStatIndex:{ClanLadderStatIndex}" + " " +
                $"SortOrder:{SortOrder}";
        }
    }
}