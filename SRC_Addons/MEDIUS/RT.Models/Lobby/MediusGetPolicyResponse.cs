using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Stream;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.PolicyResponse)]
    public class MediusGetPolicyResponse : BaseLobbyMessage, IMediusResponse
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.PolicyResponse;

        public bool IsSuccess => StatusCode >= 0;

        /// <summary>
        /// Message ID
        /// </summary>
        public MessageId MessageID { get; set; }

        public MediusCallbackStatus StatusCode;
        public string Policy; // POLICY_MAXLEN
        public bool EndOfText;

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            //
            MessageID = reader.Read<MessageId>();

            // 
            reader.ReadBytes(3);
            StatusCode = reader.Read<MediusCallbackStatus>();
            Policy = reader.ReadString(Constants.POLICY_MAXLEN);
            EndOfText = reader.ReadBoolean();
            reader.ReadBytes(3);
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            //
            writer.Write(MessageID ?? MessageId.Empty);

            // 
            writer.Write(new byte[3]);
            writer.Write(StatusCode);
            writer.Write(Policy, Constants.POLICY_MAXLEN);
            writer.Write(EndOfText);
            writer.Write(new byte[3]);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"MessageID: {MessageID} " +
                $"StatusCode: {StatusCode} " +
                $"Policy: {Policy} " +
                $"EndOfText: {EndOfText}";
        }

        public static List<MediusGetPolicyResponse> FromText(MessageId messageId, string policy)
        {
            List<MediusGetPolicyResponse> policies = new List<MediusGetPolicyResponse>();
            int i = 0;

            while (i < policy.Length)
            {
                // Determine length of string
                int len = policy.Length - i;
                if (len > Constants.POLICY_MAXLEN)
                    len = Constants.POLICY_MAXLEN;

                // Add policy subtext
                policies.Add(new MediusGetPolicyResponse()
                {
                    MessageID = messageId,
                    StatusCode = MediusCallbackStatus.MediusSuccess,
                    Policy = policy.Substring(i, len)
                });

                // Increment i
                i += len;
            }

            // Set end of text
            if (policies.Count > 0)
                policies[policies.Count - 1].EndOfText = true;

            //
            return policies;
        }
    }
}