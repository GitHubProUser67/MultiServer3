using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Stream;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common;
using System.IO;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models
{

    public class sceotTelemetryProtocol_StopServiceRequest : BaseScertMessage { 

        public override RT_MSG_TYPE Id => throw new System.NotImplementedException();

        public byte msgType;
        public long requestId;

        public override void Deserialize(MessageReader reader)
        {
            msgType = reader.ReadByte();
            reader.ReadBytes(3);
            requestId = reader.ReadInt64();
        }

        public override void Serialize(MessageWriter writer)
        {
            writer.Write(msgType);
            writer.Write(new byte[3]);
            writer.Write(requestId);
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"msgType: {msgType} " + 
                $"requestId: {requestId}";
        }

    }
    
}