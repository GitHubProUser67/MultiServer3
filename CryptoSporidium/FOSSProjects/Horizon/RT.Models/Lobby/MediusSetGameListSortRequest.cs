using CryptoSporidium.Horizon.RT.Common;
using CryptoSporidium.Horizon.LIBRARY.Common.Stream;

namespace CryptoSporidium.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.SetGameListSortPriorityRequest)]
    public class MediusSetGameListSortRequest : BaseLobbyExtMessage, IMediusRequest
    {
        public override byte PacketType => (byte)MediusLobbyExtMessageIds.SetGameListSortPriorityRequest;
        public MessageId? MessageID { get; set; }
        public string? SessionKey { get; set; } // Constants.SESSIONKEY_MAXLEN
        public int SortPriority;
        public MediusGameListFilterField SortField;
        public int Mask;
        public MediusGameListSortDirection SortDirection;
        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);
            MessageID = reader.Read<MessageId>();
            SessionKey = reader.ReadString(Constants.SESSIONKEY_MAXLEN);
            SortPriority = reader.ReadInt32();
            SortField = reader.Read<MediusGameListFilterField>();
            Mask = reader.ReadInt32();
            SortDirection = reader.Read<MediusGameListSortDirection>();
        }
        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);
            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(SessionKey, Constants.SESSIONKEY_MAXLEN);
            writer.Write(SortPriority);
            writer.Write(SortField);
            writer.Write(Mask);
            writer.Write(SortDirection);
        }
        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"SessionKey: {SessionKey} " +
                $"SortPriority: {SortPriority} " +
                $"SortField: {SortField} " +
                $"Mask: {Mask} " +
                $"SortDirection: {SortDirection}";
        }
    }
}
