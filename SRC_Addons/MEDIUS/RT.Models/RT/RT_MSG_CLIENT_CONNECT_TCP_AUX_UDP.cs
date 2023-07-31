using PSMultiServer.Addons.Medius.RT.Common;
using PSMultiServer.Addons.Medius.Server.Common.Stream;

namespace PSMultiServer.Addons.Medius.RT.Models
{
    [ScertMessage(RT_MSG_TYPE.RT_MSG_CLIENT_CONNECT_TCP_AUX_UDP)]
    public class RT_MSG_CLIENT_CONNECT_TCP_AUX_UDP : BaseScertMessage
    {
        public override RT_MSG_TYPE Id => RT_MSG_TYPE.RT_MSG_CLIENT_CONNECT_TCP_AUX_UDP;

        // 
        public uint ARG1;
        public int AppId;
        public RSA_KEY Key;

        public string SessionKey = null;
        public string AccessToken = null;

        public override void Deserialize(MessageReader reader)
        {
            SessionKey = null;
            AccessToken = null;

            if (reader.MediusVersion > 108)
            {
                ARG1 = reader.ReadUInt32();
            }
            else
            {
                reader.ReadBytes(3);
                ARG1 = reader.ReadUInt16();
            }
            AppId = reader.ReadInt32();
            Key = reader.Read<RSA_KEY>();

            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                SessionKey = reader.ReadString(Constants.SESSIONKEY_MAXLEN);
                AccessToken = reader.ReadString(Constants.NET_ACCESS_KEY_LEN);
            }
        }

        public override void Serialize(MessageWriter writer)
        {
            if (writer.MediusVersion > 108)
            {
                writer.Write(ARG1);
            }
            else
            {
                writer.Write(new byte[3]);
                writer.Write((ushort)ARG1);
            }
            writer.Write(AppId);
            writer.Write(Key ?? new RSA_KEY());
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"ARG1: {ARG1} " +
                $"AppId: {AppId} " +
                $"Key: {Key} " +
                $"SessionKey: {SessionKey} " +
                $"AccessToken: {AccessToken}";
        }
    }
}