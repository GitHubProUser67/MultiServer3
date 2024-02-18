using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.LadderListResponse)]
    public class MediusLadderListResponse : BaseLobbyMessage, IMediusResponse
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.LadderListResponse;

        public bool IsSuccess => StatusCode >= 0;

        public MessageId MessageID { get; set; }

        public int AccountID;
        public string AccountName; // ACCOUNTNAME_MAXLEN
        public uint LadderPosition;
        public MediusCallbackStatus StatusCode;
        public bool EndOfList;

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();

            // 
            reader.ReadBytes(3);
            AccountID = reader.ReadInt32();
            AccountName = reader.ReadString(Constants.ACCOUNTNAME_MAXLEN);
            LadderPosition = reader.ReadUInt32();
            StatusCode = reader.Read<MediusCallbackStatus>();
            EndOfList = reader.ReadBoolean();
            reader.ReadBytes(3);
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);

            // 
            writer.Write(new byte[3]);
            writer.Write(AccountID);
            writer.Write(AccountName, Constants.ACCOUNTNAME_MAXLEN);
            writer.Write(LadderPosition);
            writer.Write(StatusCode);
            writer.Write(EndOfList);
            writer.Write(new byte[3]);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"AccountID: {AccountID} " +
                $"AccountName: {AccountName} " +
                $"LadderPosition: {LadderPosition} " +
                $"StatusCode: {StatusCode} " +
                $"EndOfList: {EndOfList}";
        }
    }
}