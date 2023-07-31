using PSMultiServer.Addons.Medius.RT.Common;
using PSMultiServer.Addons.Medius.Server.Common.Stream;

namespace PSMultiServer.Addons.Medius.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_ECHO)]
    public class RT_MSG_CLIENT_ECHO : BaseScertMessage
    {
        //
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_ECHO;

        //
        public byte[] Value;

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