using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Stream;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassDME, MediusDmeMessageIds.ServerVersion)]
    public class TypeServerVersion : BaseDMEMessage
    {

        public override byte PacketType => (byte)MediusDmeMessageIds.ServerVersion;

        public string Version = "2.10.1143227940";

        public override void Deserialize(MessageReader reader)
        {
            // 
            base.Deserialize(reader);

            // 
            Version = reader.ReadString(Constants.DME_VERSION_LENGTH);
        }

        public override void Serialize(MessageWriter writer)
        {
            // 
            base.Serialize(writer);

            // 
            writer.Write(Version, Constants.DME_VERSION_LENGTH);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Version: {Version}";
        }
    }
}