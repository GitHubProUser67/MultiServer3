using PSMultiServer.Addons.Medius.RT.Common;
using PSMultiServer.Addons.Medius.Server.Common.Stream;

namespace PSMultiServer.Addons.Medius.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.PartyJoinByIndexResponse)]
    public class MediusPartyJoinByIndexResponse : BaseLobbyExtMessage, IMediusResponse
    {
        public override byte PacketType => (byte)MediusLobbyExtMessageIds.PartyJoinByIndexResponse;

        public bool IsSuccess => StatusCode >= 0;

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId MessageID { get; set; }
        /// <summary>
        /// Response code for the request to join a party
        /// </summary>
        public MediusCallbackStatus StatusCode;
        /// <summary>
        /// PartyHostType
        /// </summary>
        public MediusGameHostType PartyHostType;
        /// <summary>
        /// ConnectionInfo of the player to return for this session
        /// </summary>
        public NetConnectionInfo ConnectionInfo;

        public int partyIndex;

        public int maxPlayers;

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>(); 
            reader.ReadBytes(3);

            //
            StatusCode = reader.Read<MediusCallbackStatus>();
            PartyHostType = reader.Read<MediusGameHostType>();
            ConnectionInfo = reader.Read<NetConnectionInfo>();
            partyIndex = reader.ReadInt32();
            maxPlayers = reader.ReadInt32();
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(new byte[3]);

            // 
            writer.Write(StatusCode);
            writer.Write(PartyHostType);
            writer.Write(ConnectionInfo);
            writer.Write(partyIndex);
            writer.Write(maxPlayers);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"StatusCode: {StatusCode} " +
                $"PartyHostType: {PartyHostType} " +
                $"ConnectionInfo: {ConnectionInfo} " +
                $"partyIndex: {partyIndex} " +
                $"maxPlayers: {maxPlayers}";
        }
    }
}