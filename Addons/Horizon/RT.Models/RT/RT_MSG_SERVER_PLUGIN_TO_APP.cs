using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.LIBRARY.Common.Stream;

namespace MultiServer.Addons.Horizon.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_SERVER_PLUGIN_TO_APP)]
    public class RT_MSG_SERVER_PLUGIN_TO_APP : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_SERVER_PLUGIN_TO_APP;

        public BaseMediusPluginMessage Message { get; set; } = null;



        public override bool SkipEncryption
        {
            get => Message?.SkipEncryption ?? base.SkipEncryption;
            set
            {
                if (Message != null) { Message.SkipEncryption = value; }
                base.SkipEncryption = value;
            }
        }

        public override void Deserialize(MessageReader reader)
        {
            Message = BaseMediusPluginMessage.InstantiateServerPlugin(reader);
        }

        public override void Serialize(MessageWriter writer)
        {
            if (Message != null)
            {
                writer.Write(Message.IncomingMessage);
                writer.Write(new byte[1]);

                var msgSizeInt = Convert.ToInt16(Message.Size);
                var msgSizeReverse = ReverseBytes(msgSizeInt);
                writer.Write(Message.Size);
                writer.Write(Message.PluginId);

                var msgTypeInt = Convert.ToInt32(Message.PacketType);
                var msgTypeReverse = ReverseBytes(msgTypeInt);
                
                writer.Write(msgTypeReverse);
                Message.SerializePlugin(writer);
            }
        }

        public override bool CanLog()
        {
            return ServerConfiguration.MediusDebugLogs;
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Message: {Message}";
        }

        public static int ReverseBytes(int value)
        {
            return (int)((value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24);
        }
    }
}