using MultiServer.Addons.Horizon.RT.Common;

namespace MultiServer.Addons.Horizon.RT.Models
{
    public abstract class BaseLobbyMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobby;

        public BaseLobbyMessage()
        {

        }

    }
}
