using PSMultiServer.Addons.Horizon.RT.Common;

namespace PSMultiServer.Addons.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.ChannelList_ExtraInfo0)]
    public class MediusChannelList_ExtraInfoRequest0 : MediusChannelList_ExtraInfoRequest, IMediusRequest
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.ChannelList_ExtraInfo0;

    }
}