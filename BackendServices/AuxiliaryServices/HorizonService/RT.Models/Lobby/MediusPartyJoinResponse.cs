using Horizon.RT.Common;
using Horizon.LIBRARY.Common.Stream;

namespace Horizon.RT.Models
{
    /// <summary>
    /// PartyJoinResponse in earlier Medius 3.00 PS3, was later modified to join by an Index 
    /// </summary>
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.PartyJoinByIndexResponse)]
    public class MediusPartyJoinByIndexResponse : BaseLobbyExtMessage, IMediusResponse
    {
        public override byte PacketType => (byte)MediusLobbyExtMessageIds.PartyJoinByIndexResponse;

        public bool IsSuccess => StatusCode >= 0;

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId? MessageID { get; set; }
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
        public NetConnectionInfo? ConnectionInfo;

        public int partyIndex;

        public int maxPlayers;

        public List<int> maxPlayersUnpprovedList = new List<int> { 21784, 20371, 20374 };

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
            if (!maxPlayersUnpprovedList.Contains(reader.AppId))
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
            if (!maxPlayersUnpprovedList.Contains(writer.AppId))
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