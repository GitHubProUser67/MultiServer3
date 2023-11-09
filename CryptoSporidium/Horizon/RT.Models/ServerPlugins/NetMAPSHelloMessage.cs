using CryptoSporidium.Horizon.RT.Common;
using CryptoSporidium.Horizon.LIBRARY.Common.Stream;

namespace CryptoSporidium.Horizon.RT.Models.ServerPlugins
{
    [MediusMessage(NetMessageClass.MessageClassApplication, NetMessageTypeIds.NetMessageTypeProtocolInfo)]
    public class NetMessageTypeProtocolInfo : BaseApplicationMessage
    {
        public override NetMessageTypeIds PacketType => NetMessageTypeIds.NetMessageTypeProtocolInfo;

        public override byte IncomingMessage => 0;
        public override byte Size => 5;

        public override byte PluginId => 31;

        public uint protocolInfo;
        public uint buildNumber;


        public override void DeserializePlugin(MessageReader reader)
        {
            protocolInfo = reader.ReadUInt32();
            buildNumber = reader.ReadUInt32();

        }
        public override void SerializePlugin(MessageWriter writer)
        {
            writer.Write(protocolInfo);
            writer.Write(buildNumber);
        }

        public override string ToString()
        {
            var ProtoBytesReversed = ReverseBytesUInt(protocolInfo);
            var BuildNumberReversed = ReverseBytesUInt(buildNumber);

            return base.ToString() + " " +
                $"protocolInfo: {ProtoBytesReversed} " +
                $"buildNumber: {BuildNumberReversed}";
        }
    }
}
