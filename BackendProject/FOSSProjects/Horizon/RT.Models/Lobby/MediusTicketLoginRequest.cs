using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.TicketLogin)]
    public class MediusTicketLoginRequest : BaseLobbyExtMessage, IMediusRequest
    {
        public override byte PacketType => (byte)MediusLobbyExtMessageIds.TicketLogin;

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId MessageID { get; set; }
        /// <summary>
        /// Session Key
        /// </summary>
        public string SessionKey; // SESSIONKEY_MAXLEN
        /// <summary>
        /// Ticket Size
        /// </summary>
        public uint TicketSize; 
        public byte[] UNK0;
        /// <summary>
        /// Account Name
        /// </summary>
        public uint UserAccountId;
        public byte UserOnlineIDLen;
        public string UserOnlineId;
        public string UserRegion;
        public string UserDomain;
        public uint UserStatus;
        public byte[] UNK1;
        public byte[] UNK2;
        /// <summary>
        /// NP Service ID
        /// </summary>
        public string ServiceID;

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();

            // 
            SessionKey = reader.ReadString(Constants.SESSIONKEY_MAXLEN);
            reader.ReadBytes(2);
            TicketSize = reader.ReadUInt32();
            UNK0 = reader.ReadBytes(74);
            UserAccountId = reader.ReadUInt32();
            reader.ReadBytes(3);
            UserOnlineIDLen = reader.ReadByte();
            UserOnlineId = reader.ReadString(UserOnlineIDLen);
            UserDomain = reader.ReadString(Constants.USER_DOMAIN_MAXLEN);
            UserRegion = reader.ReadString(Constants.USER_REGION_MAXLEN);
            UNK1 = reader.ReadBytes(12);
            ServiceID = reader.ReadString(Constants.SERVICE_ID_MAXLEN);
            UserStatus = reader.ReadUInt32();
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);

            // 
            writer.Write(SessionKey, Constants.SESSIONKEY_MAXLEN);
            writer.Write(2);
            writer.Write(TicketSize);
            writer.Write(UNK0 ?? new byte[73], 73);
            writer.Write(UserAccountId);
            writer.Write(UserOnlineIDLen);
            writer.Write(UserOnlineId, UserOnlineIDLen);
            writer.Write(UserRegion, Constants.USER_REGION_MAXLEN);
            writer.Write(UserDomain, Constants.USER_DOMAIN_MAXLEN);
            writer.Write(UNK1 ?? new byte[12], 12);
            writer.Write(ServiceID ?? "", Constants.SERVICE_ID_MAXLEN);
            writer.Write(UserStatus);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID:{MessageID} " +
                $"SessionKey:{SessionKey} " +
                $"TicketSize: {TicketSize} " +
                $"UNK0: {UNK0} " +
                //$"UNK0: {BitConverter.ToString(UNK2)} " +
                $"UserAccountId: {ReverseBytes(UserAccountId)} " +
                $"UserOnlineIDLen: {Convert.ToInt32(UserOnlineIDLen)} " +
                $"UserOnlineId: {UserOnlineId} " +
                $"UserRegion: {UserRegion} " +
                $"UserDomain: {UserDomain} " +
                $"UNK1: {UNK1} " +
                $"ServiceID: {ServiceID} " +
                $"UserStatus: {UserStatus} ";
        }

        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
    }
}