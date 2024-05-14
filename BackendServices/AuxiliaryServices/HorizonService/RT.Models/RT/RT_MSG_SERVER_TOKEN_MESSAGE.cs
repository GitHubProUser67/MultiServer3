using System.IO;
using Horizon.RT.Common;
using Horizon.LIBRARY.Common;
using Horizon.LIBRARY.Common.Stream;

namespace Horizon.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_SERVER_TOKEN_MESSAGE)]
    public class RT_MSG_SERVER_TOKEN_MESSAGE : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_SERVER_TOKEN_MESSAGE;

        public byte[] Field1 = Utils.FromString("070000");
        public byte[] Field3 = Utils.FromString("00030700");
        public byte[] Field5 = Utils.FromString("00");
        public byte[] Field7 = Utils.FromString("0000");
        public byte[] Field9 = Utils.FromString("07");
        public byte Host;

        public RT_TOKEN_MESSAGE_TYPE tokenMsgType;
        public ushort TokenID;
        public ushort TokenHost;

        public override void Deserialize(MessageReader reader)
        {
            
        }

        public override void Serialize(MessageWriter writer)
        {
            if (writer.MediusVersion == 109)
            {
                writer.Write(Field1);
                writer.Write(Host);
                writer.Write(Field3);
                writer.Write(Host);
                writer.Write(Field5);
                writer.Write(Host);
                writer.Write(Field7);
                writer.Write(Host);
                writer.Write(Field9);
            }
            else
            {
                writer.Write(tokenMsgType);
                if (tokenMsgType == RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_CLIENT_QUERY
                    || tokenMsgType == RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_CLIENT_REQUEST
                    || tokenMsgType == RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_CLIENT_RELEASE
                    || tokenMsgType == RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_OWNED
                    || tokenMsgType == RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_GRANTED
                    || tokenMsgType == RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_RELEASED
                    || tokenMsgType == RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_FREED)
                {
                    writer.Write(TokenID);

                    if (tokenMsgType == RT_TOKEN_MESSAGE_TYPE.RT_TOKEN_SERVER_OWNED)
                        writer.Write(TokenHost);
                }
            }
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"RT_TOKEN_MESSAGE_TYPE: {tokenMsgType} " +
                $"targetToken: {TokenID} " +
                $"TokenOwner: {TokenHost} " +
                $"Host: {Host}";
        }
    }
}
