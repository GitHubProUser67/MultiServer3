using System.IO;
using Horizon.RT.Common;
using Horizon.LIBRARY.Common.Stream;

namespace Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassApplication, NetMessageTypeIds.NetMessageTypeMAPSHelloMessage)]
    public class NetMAPSHelloMessage : BaseApplicationMessage
    {
        public override NetMessageTypeIds PacketType => NetMessageTypeIds.NetMessageTypeMAPSHelloMessage;

        public override byte IncomingMessage => 0;
        public override byte Size => 8;
        public override byte PluginId => 31;

        public bool m_success;
        public bool m_isOnline;

        public byte[] m_availableFactions = new byte[3];

        public override void DeserializePlugin(MessageReader reader)
        {
            m_success = reader.ReadBoolean();
            m_isOnline = reader.ReadBoolean();
            reader.ReadBytes(2);
            m_availableFactions = reader.ReadBytes(m_availableFactions.Length);
        }
        public override void SerializePlugin(MessageWriter writer)
        {
            writer.Write(m_success);
            writer.Write(m_isOnline);
            writer.Write(new byte[2]);
            writer.Write(m_availableFactions);
        }

        public override string ToString()
        {
            //var ProtoBytesReversed = ReverseBytes(protocolInfo);

            return base.ToString() + " " +
                $"m_success: {m_success} " +
                $"m_isOnline: {m_isOnline} " +
                $"m_availableFactions: {m_availableFactions}";
        }
    }
}
