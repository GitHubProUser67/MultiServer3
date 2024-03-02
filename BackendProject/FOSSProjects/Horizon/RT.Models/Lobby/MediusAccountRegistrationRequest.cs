using Horizon.RT.Common;
using Horizon.LIBRARY.Common.Stream;

namespace Horizon.RT.Models
{
    /// <summary>
    /// Request to register a new account
    /// </summary>
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.AccountRegistration)]
    public class MediusAccountRegistrationRequest : BaseLobbyMessage, IMediusRequest
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.AccountRegistration;

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId MessageID { get; set; }
        /// <summary>
        /// Session Key
        /// </summary>
        public string SessionKey; // SESSIONKEY_MAXLEN
        /// <summary>
        /// Medius Account type
        /// </summary>
        public MediusAccountType AccountType;
        /// <summary>
        /// Account Name requested
        /// </summary>
        public string AccountName; // ACCOUNTNAME_MAXLEN
        /// <summary>
        /// Password requested
        /// </summary>
        public string Password; // PASSWORD_MAXLEN

        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);

            MessageID = reader.Read<MessageId>();

            SessionKey = reader.ReadString(Constants.SESSIONKEY_MAXLEN);
            reader.ReadBytes(2);

            AccountType = reader.Read<MediusAccountType>();
            AccountName = reader.ReadString(Constants.ACCOUNTNAME_MAXLEN);
            Password = reader.ReadString(Constants.PASSWORD_MAXLEN);
        }

        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);

            writer.Write(MessageID ?? MessageId.Empty);

            writer.Write(SessionKey, Constants.SESSIONKEY_MAXLEN);
            writer.Write(new byte[2]);

            writer.Write(AccountType);
            writer.Write(AccountName, Constants.ACCOUNTNAME_MAXLEN);
            writer.Write(Password, Constants.PASSWORD_MAXLEN);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"SessionKey: {SessionKey} " +
                $"AccountType: {AccountType} " +
                $"AccountName: {AccountName} " +
                $"Password: {Password}";
        }
    }
}