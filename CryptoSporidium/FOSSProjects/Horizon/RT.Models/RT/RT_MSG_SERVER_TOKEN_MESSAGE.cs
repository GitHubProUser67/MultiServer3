using CryptoSporidium.Horizon.RT.Common;
using CryptoSporidium.Horizon.LIBRARY.Common;
using CryptoSporidium.Horizon.LIBRARY.Common.Stream;

namespace CryptoSporidium.Horizon.RT.Models
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
        public ushort targetToken;

        public override void Deserialize(MessageReader reader)
        {
            if(reader.MediusVersion == 109)
            {

            }
            else
            {

                tokenMsgType = reader.Read<RT_TOKEN_MESSAGE_TYPE>();
                targetToken = reader.Read<ushort>();
            }
        }

        public override void Serialize(MessageWriter writer)
        {
            if(writer.MediusVersion == 109)
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
            } else
            {
                writer.Write(tokenMsgType);
                writer.Write(targetToken);
            }
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"RT_TOKEN_MESSAGE_TYPE: {tokenMsgType} " +
                $"targetToken: {targetToken} " +
                $"Host: {Host}";
        }
    }
}