using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Stream;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models.ServerPlugins
{
    [MediusMessage(NetMessageClass.MessageClassApplication, NetMessageTypeIds.NetMessageTypeAccountLogoutResponse)]
    public class NetMessageAccountLogoutResponse : BaseApplicationMessage
    {
        public override NetMessageTypeIds PacketType => NetMessageTypeIds.NetMessageTypeAccountLogoutResponse;

        public override byte IncomingMessage => 0;
        public override byte Size => 0;

        public override byte PluginId => 0;

        public byte m_success;

        public override void DeserializePlugin(MessageReader reader)
        {
            m_success = reader.ReadByte();
            reader.ReadBytes(3);
        }

        public override void SerializePlugin(MessageWriter writer)
        {
            writer.Write(m_success);
            writer.Write(new byte[3]);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Success: {m_success}";
        }

    }
}