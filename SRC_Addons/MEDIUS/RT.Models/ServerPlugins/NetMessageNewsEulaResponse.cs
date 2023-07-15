using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Stream;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models
{

    //[MediusMessage(NetMessageClass.MessageClassApplication, NetMessageTypeIds.NetMessageNewsEulaResponse)]
    public class NetMessageNewsEulaResponse : BaseApplicationMessage
    {
        public override NetMessageTypeIds PacketType => NetMessageTypeIds.kNetGameMessageTypeStart;

        public override byte IncomingMessage => 0;
        public override byte Size => 10;

        public override byte PluginId => 30;


        public byte m_finished;
        public NetMessageNewsEulaResponseContentType m_type;
        public string m_content;
        public long m_timestamp;

        public override void DeserializePlugin(MessageReader reader)
        {
            m_finished = reader.ReadByte();
            reader.ReadBytes(3);
            m_type = reader.Read<NetMessageNewsEulaResponseContentType>();
            m_content = reader.ReadString();
            m_timestamp = reader.ReadInt32();

        }
        public override void SerializePlugin(MessageWriter writer)
        {
            writer.Write(m_finished);
            writer.Write(new byte[3]);
            writer.Write(m_type);
            writer.Write(m_content);
            writer.Write(m_timestamp);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"m_finished: {m_finished} " +
                $"m_type: {m_type} " +
                $"m_content: {m_content} " +
                $"m_timestamp: {m_timestamp}";
        }
    }
}