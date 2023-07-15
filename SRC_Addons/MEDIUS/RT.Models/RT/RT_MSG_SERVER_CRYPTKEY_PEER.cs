using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Stream;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_SERVER_CRYPTKEY_PEER)]
    public class RT_MSG_SERVER_CRYPTKEY_PEER : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_SERVER_CRYPTKEY_PEER;

        // 
        public byte[] SessionKey = null;
        public override void Deserialize(MessageReader reader)
        {
            SessionKey = reader.ReadBytes(0x40);
        }

        public override void Serialize(MessageWriter writer)
        {
            if (SessionKey == null || SessionKey.Length != 0x40)
                throw new InvalidOperationException("Unable to serialize SERVER_SET_CLIENT_SESSION_KEY because key is either null or not 64 bytes long!");

            writer.Write(SessionKey);
        }
    }
}