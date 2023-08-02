using PSMultiServer.Addons.Horizon.RT.Common;
using PSMultiServer.Addons.Horizon.RT.Models.Misc;
using PSMultiServer.Addons.Horizon.Server.Common.Stream;

namespace PSMultiServer.Addons.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.GenericChatMessage1)]
    public class MediusGenericChatMessage1 : BaseLobbyExtMessage, IMediusChatMessage
    {
        public override byte PacketType => (byte)MediusLobbyExtMessageIds.GenericChatMessage1;

        public MessageId MessageID { get; set; }

        public string SessionKey; // SESSIONKEY_MAXLEN
        public MediusChatMessageType MessageType { get; set; }
        public int TargetID { get; set; }
        public string Message { get; set; }

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();
            SessionKey = reader.ReadString(Constants.SESSIONKEY_MAXLEN);

            //
            MessageType = reader.Read<MediusChatMessageType>();
            TargetID = reader.ReadInt32();
            int length = reader.ReadInt32();
            Message = reader.ReadString(length);
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(SessionKey, Constants.SESSIONKEY_MAXLEN);

            //
            writer.Write(MessageType);
            writer.Write(TargetID);
            writer.Write(Message.Length);
            writer.Write(Message, Message.Length);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"SessionKey: {SessionKey} " +
                $"MessageType: {MessageType} " +
                $"TargetID: {TargetID} " +
                $"Message: {Message}";
        }
    }
}