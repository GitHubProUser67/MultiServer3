using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.AccountLoginResponse)]
    public class MediusAccountLoginResponse : BaseLobbyMessage, IMediusResponse
    {

        public override byte PacketType => (byte)MediusLobbyMessageIds.AccountLoginResponse;

        public bool IsSuccess => StatusCode >= 0;

        public MessageId MessageID { get; set; }

        public MediusCallbackStatus StatusCode;
        public int AccountID;
        public MediusAccountType AccountType;
        public int MediusWorldID;
        public NetConnectionInfo ConnectInfo;

        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);

            MessageID = reader.Read<MessageId>();

            reader.ReadBytes(3);
            StatusCode = reader.Read<MediusCallbackStatus>();
            AccountID = reader.ReadInt32();
            AccountType = reader.Read<MediusAccountType>();
            MediusWorldID = reader.ReadInt32();
            ConnectInfo = reader.Read<NetConnectionInfo>();
        }

        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);

            writer.Write(MessageID ?? MessageId.Empty);

            writer.Write(new byte[3]);
            writer.Write(StatusCode);
            writer.Write(AccountID);
            writer.Write(AccountType);
            writer.Write(MediusWorldID);
            writer.Write(ConnectInfo ?? new NetConnectionInfo());
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                    $"MessageID: {MessageID} " +
                    $"StatusCode: {StatusCode} " +
                    $"AccountID: {AccountID} " +
                    $"AccountType: {AccountType} " +
                    $"MediusWorldID: {MediusWorldID} " +
                    $"ConnectInfo: {ConnectInfo}";
        }
    }
}