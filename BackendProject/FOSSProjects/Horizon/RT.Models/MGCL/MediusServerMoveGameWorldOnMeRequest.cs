using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    /// <summary>
    /// Request to move a game world from one host to this host using the 
    /// </summary>
    [MediusMessage(NetMessageClass.MessageClassLobbyReport, MediusMGCLMessageIds.ServerMoveGameWorldOnMeRequest)]
    public class MediusServerMoveGameWorldOnMeRequest : BaseMGCLMessage, IMediusRequest
    {

        public override byte PacketType => (byte)MediusMGCLMessageIds.ServerMoveGameWorldOnMeRequest;

        public MessageId MessageID { get; set; }
        public int CurrentMediusWorldID;
        public int NewGameWorldID;
        public NetAddressList AddressList;

        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);

            MessageID = reader.Read<MessageId>();
            reader.ReadBytes(3);
            CurrentMediusWorldID = reader.ReadInt32();
            NewGameWorldID = reader.ReadInt32();
            AddressList = reader.Read<NetAddressList>();
        }

        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);

            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(new byte[3]);
            writer.Write(CurrentMediusWorldID);
            writer.Write(NewGameWorldID);
            writer.Write(AddressList);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"CurrentMediusWorldID: {CurrentMediusWorldID} " +
                $"NewGameWorldID: {NewGameWorldID} " +
                $"AddressList: {AddressList}";
        }
    }
}