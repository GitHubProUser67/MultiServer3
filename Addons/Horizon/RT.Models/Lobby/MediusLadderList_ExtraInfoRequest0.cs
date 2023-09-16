using MultiServer.Addons.Horizon.RT.Common;
using MultiServer.Addons.Horizon.LIBRARY.Common.Stream;

namespace MultiServer.Addons.Horizon.RT.Models
{
    [MediusMessage(NetMessageClass.MessageClassLobby, MediusLobbyMessageIds.LadderList_ExtraInfo0)]
    public class MediusLadderList_ExtraInfoRequest0 : MediusLadderList_ExtraInfoRequest, IMediusRequest
    {
        public override byte PacketType => (byte)MediusLobbyMessageIds.LadderList_ExtraInfo0;

    }
}
