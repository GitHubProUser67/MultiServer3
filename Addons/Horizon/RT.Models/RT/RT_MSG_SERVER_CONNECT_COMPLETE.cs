using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.LIBRARY.Common.Stream;

namespace MultiServer.Addons.Horizon.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_SERVER_CONNECT_COMPLETE)]
    public class RT_MSG_SERVER_CONNECT_COMPLETE : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_SERVER_CONNECT_COMPLETE;

        // 
        public ushort ClientCountAtConnect = 0x0001;

        public override void Deserialize(MessageReader reader)
        {
            ClientCountAtConnect = reader.ReadUInt16();
        }

        public override void Serialize(MessageWriter writer)
        {
            writer.Write(ClientCountAtConnect);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"ClientCountAtConnect: {ClientCountAtConnect}";
        }
    }
}