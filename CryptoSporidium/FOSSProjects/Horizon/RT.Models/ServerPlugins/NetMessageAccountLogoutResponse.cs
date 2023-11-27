using CryptoSporidium.Horizon.RT.Common;
using CryptoSporidium.Horizon.LIBRARY.Common.Stream;

namespace CryptoSporidium.Horizon.RT.Models.ServerPlugins
{
    [MediusMessage(NetMessageClass.MessageClassApplication, NetMessageTypeIds.NetMessageTypeAccountLogoutResponse)]
    public class NetMessageAccountLogoutResponse : BaseApplicationMessage
    {
        public override NetMessageTypeIds PacketType => NetMessageTypeIds.NetMessageTypeAccountLogoutResponse;

        public override byte IncomingMessage => 0;
        public override byte Size => 0;

        public override byte PluginId => 0;

        public bool m_success;

        public override void DeserializePlugin(MessageReader reader)
        {
            m_success = reader.ReadBoolean();
        }

        public override void SerializePlugin(MessageWriter writer)
        {
            writer.Write(m_success);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Success: {m_success}";
        }

    }
}