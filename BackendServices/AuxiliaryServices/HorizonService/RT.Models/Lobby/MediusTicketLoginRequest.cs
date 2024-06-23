using System.IO;
using Horizon.RT.Common;
using Horizon.LIBRARY.Common.Stream;
using System;

namespace Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.TicketLogin)]
    public class MediusTicketLoginRequest : BaseLobbyExtMessage, IMediusRequest
    {
        public override byte PacketType => (byte)MediusLobbyExtMessageIds.TicketLogin;

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId? MessageID { get; set; }
        /// <summary>
        /// Session Key
        /// </summary>
        public string? SessionKey; // SESSIONKEY_MAXLEN
        /// <summary>
        /// Ticket Size
        /// </summary>
        public uint TicketSize; 
        public byte[]? UNK0;
        /// <summary>
        /// Account Name
        /// </summary>
        public uint UserAccountId;
        public byte UserOnlineIDLen;
        public string? UserOnlineId;
        public string? UserRegion;
        public string? UserDomain;
        public byte[]? UNK1;
        public byte[]? UNK2;
        public byte[]? UNK3;
        /// <summary>
        /// NP Service ID
        /// </summary>
        public string? ServiceID;

        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);

            MessageID = reader.Read<MessageId>();

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
            UNK3 = reader.ReadBytes(98);
        }

        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);

            writer.Write(MessageID ?? MessageId.Empty);

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
            writer.Write(ServiceID ?? string.Empty, Constants.SERVICE_ID_MAXLEN);
            writer.Write(UNK3 ?? new byte[98], 98);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID:{MessageID} " +
                $"SessionKey:{SessionKey} " +
                $"TicketSize: {TicketSize} " +
                $"UNK0: {(UNK0 != null ? BitConverter.ToString(UNK0) : string.Empty)} " +
                $"UserAccountId: {ReverseBytes(UserAccountId)} " +
                $"UserOnlineIDLen: {Convert.ToInt32(UserOnlineIDLen)} " +
                $"UserOnlineId: {UserOnlineId} " +
                $"UserRegion: {UserRegion} " +
                $"UserDomain: {UserDomain} " +
                $"UNK1: {(UNK1 != null ? BitConverter.ToString(UNK1) : string.Empty)} " +
                $"ServiceID: {ServiceID} " +
                $"UNK3: {(UNK3 != null ? BitConverter.ToString(UNK3) : string.Empty)} ";
        }

        public static uint ReverseBytes(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
    }
}
