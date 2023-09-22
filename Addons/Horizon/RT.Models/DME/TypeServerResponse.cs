using MultiServer.Addons.Org.BouncyCastle.Utilities.Net;
using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.LIBRARY.Common.Stream;

namespace MultiServer.Addons.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassDME, MediusDmeMessageIds.ServerResponse)]
    public class TypeServerResponse : BaseDMEMessage
    {

        public override byte PacketType => (byte)MediusDmeMessageIds.ServerResponse;

        public long Port;
        public IPAddress IPAddress;

        public override void Deserialize(MessageReader reader)
        {
            base.Deserialize(reader);

            Port = reader.ReadUInt32();
            IPAddress = reader.Read<IPAddress>();
        }

        public override void Serialize(MessageWriter writer)
        {
            base.Serialize(writer);

            writer.Write(Port);
            writer.Write(IPAddress);
        }


        public override string ToString()
        {
            return base.ToString() + " " +
                $"IPAddress: {IPAddress} " +
            $"Port: {Port}";
        }
    }
}