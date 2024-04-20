using System.IO;
using Horizon.RT.Common;
using Horizon.LIBRARY.Common.Stream;

namespace Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassApplication, NetMessageTypeIds.NetMessageNewsEulaRequest)]
    public class NetMessageNewsEulaRequest : BaseApplicationMessage
    {
        public override NetMessageTypeIds PacketType => NetMessageTypeIds.NetMessageNewsEulaRequest;

        public override byte IncomingMessage => 0;
        public override byte Size => 10;

        public override byte PluginId => 30;

        public string? m_languageExtension;

        public override void DeserializePlugin(MessageReader reader)
        {
            m_languageExtension = reader.ReadString();

        }
        public override void SerializePlugin(MessageWriter writer)
        {
            writer.Write(m_languageExtension);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"m_languageExtension: {m_languageExtension} ";
        }
    }
}
