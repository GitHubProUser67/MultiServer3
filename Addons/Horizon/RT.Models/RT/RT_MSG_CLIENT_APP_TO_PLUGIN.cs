using PSMultiServer.Addons.Horizon.RT.Common;
using PSMultiServer.Addons.Horizon.Server.Common.Stream;

namespace PSMultiServer.Addons.Horizon.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_APP_TO_PLUGIN)]
    public class RT_MSG_CLIENT_APP_TO_PLUGIN : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_APP_TO_PLUGIN;

        public BaseMediusPluginMessage Message { get; set; } = null;

        public override void Deserialize(MessageReader reader)
        {
            Message = BaseMediusPluginMessage.InstantiateClientPlugin(reader);
        }

        public override void Serialize(MessageWriter writer)
        {

            if (Message != null)
            {
                writer.Write(Message.Size);
                //writer.Write(new byte[2]);
                writer.Write(Message.PacketType);
                Message.SerializePlugin(writer);
            }

        }

        public override bool CanLog()
        {
            return base.CanLog();
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Message: {Message}";
        }
    }
}