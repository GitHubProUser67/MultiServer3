using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.FileCreateResponse)]
    public class MediusFileCreateResponse : BaseLobbyMessage, IMediusResponse
    {

        public override byte PacketType => (byte)MediusLobbyMessageIds.FileCreateResponse;

        public bool IsSuccess => StatusCode >= 0;

        public MessageId? MessageID { get; set; }

        public MediusFile? MediusFileInfo = new();
        public MediusCallbackStatus StatusCode;

        public override void Deserialize(MessageReader reader)
        {
            MediusFileInfo = reader.Read<MediusFile>();
            StatusCode = reader.Read<MediusCallbackStatus>();

            base.Deserialize(reader);

            MessageID = reader.Read<MessageId>();
            reader.ReadBytes(3);
        }

        public override void Serialize(MessageWriter writer)
        {
            writer.Write(MediusFileInfo);
            writer.Write(StatusCode);

            base.Serialize(writer);

            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(new byte[3]);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"MediusFileInfo: {MediusFileInfo} " +
                $"StatusCode: {StatusCode}";
        }
    }
}