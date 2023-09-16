using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.LIBRARY.Common.Stream;

namespace MultiServer.Addons.Horizon.RT.Models
{
    /// <summary>
    /// General error message from the server to client
    /// </summary>
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.ErrorMessage)]
    public class MediusErrorMessage : BaseLobbyMessage
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.ErrorMessage;

        public int ErrorCode;
        public string ErrorMessage; // ERRORMSG_MAXLEN

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            ErrorCode = reader.ReadInt32();
            ErrorMessage = reader.ReadString(Constants.ERRORMSG_MAXLEN);
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            // 
            writer.Write(ErrorCode);
            writer.Write(ErrorMessage, Constants.ERRORMSG_MAXLEN);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID:{ErrorCode}" + " " +
                $"SessionKey: {ErrorMessage}";
        }
    }
}