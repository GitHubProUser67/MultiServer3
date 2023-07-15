using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Stream;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_SET_AGG_TIME)]
    public class RT_MSG_CLIENT_SET_AGG_TIME : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_SET_AGG_TIME;

        public short AggTime { get; set; }

        public override void Deserialize(MessageReader reader)
        {
            AggTime = reader.ReadInt16();
        }

        public override void Serialize(MessageWriter writer)
        {
            writer.Write(AggTime);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"AggTime: {AggTime}";
        }
    }
}