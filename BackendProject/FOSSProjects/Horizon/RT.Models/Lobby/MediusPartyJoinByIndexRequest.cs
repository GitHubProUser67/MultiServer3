using BackendProject.Horizon.RT.Common;
using BackendProject.Horizon.LIBRARY.Common.Stream;

namespace BackendProject.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobbyExt, MediusLobbyExtMessageIds.PartyJoinByIndexRequest)] // Set GameState
    public class MediusPartyJoinByIndexRequest : BaseLobbyExtMessage, IMediusRequest
    {
        public override byte PacketType => (byte)MediusLobbyExtMessageIds.PartyJoinByIndexRequest; // Set GameState

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId MessageID { get; set; }
        /// <summary>
        /// Session Key
        /// </summary>
        public string SessionKey; // SESSIONKEY_MAXLEN
        /// <summary>
        /// MediusWorldID
        /// </summary>
        public int MediusWorldID;
        /// <summary>
        /// Party Password
        /// </summary>
        public string PartyPassword; //PASSWORD_MAXLEN
        /// <summary>
        /// PartyHostType
        /// </summary>
        public MediusGameHostType PartyHostType;
        /// <summary>
        /// Security Key of this session
        /// </summary>
        public RSA_KEY pubKey;
        /// <summary>
        /// AddressList of this player
        /// </summary>
        public NetAddressList addressList;
        /// <summary>
        /// AddressList of this player
        /// </summary>
        public int partyIndex;

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>(); 
            SessionKey = reader.ReadString(Constants.SESSIONKEY_MAXLEN);
            reader.ReadBytes(2);

            //
            MediusWorldID = reader.ReadInt32();
            PartyPassword = reader.ReadString(Constants.PARTYPASSWORD_MAXLEN);
            PartyHostType = reader.Read<MediusGameHostType>();
            pubKey = reader.Read<RSA_KEY>();
            addressList = reader.Read<NetAddressList>();
            partyIndex = reader.ReadInt32();
            //MatchGameState = reader.Read<MediusMatchGameState>();
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(SessionKey, Constants.SESSIONKEY_MAXLEN);
            writer.Write(new byte[2]);

            // 
            writer.Write(MediusWorldID);
            writer.Write(PartyPassword);
            writer.Write(PartyHostType);
            writer.Write(pubKey);
            writer.Write(addressList);
            writer.Write(partyIndex);
            //writer.Write(MatchGameState);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"SessionKey: {SessionKey} " +
                $"MediusWorldID: {MediusWorldID} " +
                $"PartyPassword: {PartyPassword} " +
                $"PartyHostType: {PartyHostType} " +
                $"pubKey: {pubKey} " +
                $"addressList: {addressList} " +
                $"partyIndex: {partyIndex}";
            //$"GameState: {MatchGameState}";
        }
    }
}