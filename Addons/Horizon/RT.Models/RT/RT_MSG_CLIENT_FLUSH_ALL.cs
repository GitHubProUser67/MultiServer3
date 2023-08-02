using PSMultiServer.Addons.Horizon.RT.Common;
using PSMultiServer.Addons.Horizon.Server.Common.Stream;

namespace PSMultiServer.Addons.Horizon.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_FLUSH_ALL)]
    public  class RT_MSG_CLIENT_FLUSH_ALL : BaseScertMessage 
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_FLUSH_ALL;

        public byte[] Contents;

        public override void Deserialize(MessageReader reader)
        {
            Contents = reader.ReadRest();
        }

        public override void Serialize(MessageWriter writer)
        {
            writer.Write(Contents);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Contents: {BitConverter.ToString(Contents)}";
        }
    }
}