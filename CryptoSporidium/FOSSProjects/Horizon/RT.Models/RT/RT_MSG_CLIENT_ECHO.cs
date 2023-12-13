using CryptoSporidium.Horizon.RT.Common;
using CryptoSporidium.Horizon.LIBRARY.Common.Stream;

namespace CryptoSporidium.Horizon.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_ECHO)]
    public class RT_MSG_CLIENT_ECHO : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_ECHO;

        public byte[]? Value;

        public override void Deserialize(MessageReader reader)
        {
            Value = reader.ReadRest();
        }

        public override void Serialize(MessageWriter writer)
        {
            writer.Write(Value);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Value: {BitConverter.ToString(Value)}";
        }

    }
}