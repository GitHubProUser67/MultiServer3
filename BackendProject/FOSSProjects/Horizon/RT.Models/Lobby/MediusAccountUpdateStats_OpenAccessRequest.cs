using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.AccountUpdateStats_OpenAccess)]
    public class MediusAccountUpdateStats_OpenAccessRequest : BaseLobbyExtMessage, IMediusRequest
    {

        public override byte PacketType => (byte)MediusLobbyExtMessageIds.AccountUpdateStats_OpenAccess;

        public MessageId MessageID { get; set; }

        public string SessionKey; // SESSIONKEY_MAXLEN
        public byte[] Stats = new byte[Constants.ACCOUNTSTATS_MAXLEN];

        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);

            MessageID = reader.Read<MessageId>();

            SessionKey = reader.ReadString(Constants.SESSIONKEY_MAXLEN);
            Stats = reader.ReadBytes(Constants.ACCOUNTSTATS_MAXLEN);
        }

        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);

            writer.Write(MessageID ?? MessageId.Empty);

            writer.Write(SessionKey, Constants.SESSIONKEY_MAXLEN);
            writer.Write(Stats, Constants.ACCOUNTSTATS_MAXLEN);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"SessionKey: {SessionKey} " +
                $"Stats: {BitConverter.ToString(Stats)}";
        }
    }
}