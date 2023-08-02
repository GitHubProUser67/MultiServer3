using PSMultiServer.Addons.Horizon.RT.Common;

namespace PSMultiServer.Addons.Horizon.RT.Models
{
    public abstract class BaseLobbyMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobby;

        public BaseLobbyMessage()
        {

        }

    }
}
