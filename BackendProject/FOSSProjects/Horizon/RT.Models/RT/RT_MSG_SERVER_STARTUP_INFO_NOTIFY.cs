using Horizon.RT.Common;
using Horizon.LIBRARY.Common;
using Horizon.LIBRARY.Common.Stream;

namespace Horizon.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_SERVER_STARTUP_INFO_NOTIFY)]
    public class RT_MSG_SERVER_STARTUP_INFO_NOTIFY : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_SERVER_STARTUP_INFO_NOTIFY;

        public byte GameHostType { get; set; } = (byte)MGCL_GAME_HOST_TYPE.MGCLGameHostClientServerAuxUDP;
        public uint Timebase { get; set; } = Utils.GetUnixTime();

        public byte[] Field1 = Utils.FromString("249433");
        public byte[] Field3 = Utils.FromString("01010100");
        public short Host;

        public override void Deserialize(MessageReader reader)
        {
            if(reader.MediusVersion == 109)
            {
                Field1 = reader.ReadRest();
                Host = reader.ReadInt16();
                Field3 = reader.ReadRest();
                Host = reader.ReadInt16();
            }
            else
            {

                GameHostType = reader.ReadByte();
                Timebase = reader.ReadUInt32();
            }
        }

        public override void Serialize(MessageWriter writer)
        {
            if(writer.MediusVersion == 109)
            {
                writer.Write(Field1);
                writer.Write(Host);
                writer.Write(Field3);
                writer.Write(Host);
            }
            else
            {
                writer.Write(GameHostType);
                writer.Write(Timebase);
            }
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"GameHostType: {(MGCL_GAME_HOST_TYPE)GameHostType} " +
                $"Timebase: {Timebase}" +
                $"Host: {Host} ";
        }
    }
}