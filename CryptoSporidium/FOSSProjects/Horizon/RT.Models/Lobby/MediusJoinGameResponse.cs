using CustomLogger;
using CryptoSporidium.Horizon.RT.Common;
using CryptoSporidium.Horizon.LIBRARY.Common.Stream;

namespace CryptoSporidium.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.JoinGameResponse)]
    public class MediusJoinGameResponse : BaseLobbyMessage, IMediusResponse
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.JoinGameResponse;

        public bool IsSuccess => StatusCode >= 0;

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId MessageID { get; set; }

        public MediusCallbackStatus StatusCode;
        public MediusGameHostType GameHostType;
        public NetConnectionInfo ConnectInfo;
        /// <summary>
        /// MaxPlayers
        /// </summary>
        public long MaxPlayers;

        public List<int> approvedMaxPlayersAppIds = new()
        { 
            20371,
            20374,
            20624,
            22500,
            22920,
            22924,
            22930,
            24000,
            24180,
            50041,
            50083,
            50089,
            50097,
            50098,
            50100,
            50121,
            50130,
            50132,
            50135,
            50141,
            50160,
            50161,
            50162,
            50165,
            50170,
            50175,
            50180,
            50182,
            50183,
            50185,
            50186
        };

        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);

            MessageID = reader.Read<MessageId>();
            reader.ReadBytes(3);

            StatusCode = reader.Read<MediusCallbackStatus>();
            GameHostType = reader.Read<MediusGameHostType>();
            ConnectInfo = reader.Read<NetConnectionInfo>();

            if (reader.MediusVersion == 113 && approvedMaxPlayersAppIds.Contains(reader.AppId))
            {
                MaxPlayers = reader.ReadInt64();
            }
        }

        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);

            writer.Write(MessageID ?? MessageId.Empty);
            writer.Write(new byte[3]);

            writer.Write(StatusCode);
            writer.Write(GameHostType);
            writer.Write(ConnectInfo);

            if (writer.MediusVersion == 113 && approvedMaxPlayersAppIds.Contains(writer.AppId))
            {
                LoggerAccessor.LogInfo($"[MediusJoinGameResponse] - Setting MaxPlayers for {writer.AppId.ToString()}");
                writer.Write(MaxPlayers);
            }
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"StatusCode: {StatusCode} " +
                $"GameHostType: {GameHostType} " +
                $"ConnectInfo: {ConnectInfo} " +
                $"MaxPlayers: {MaxPlayers}";
        }
    }
}