using PSMultiServer.Addons.Medius.RT.Common;

namespace PSMultiServer.Addons.Medius.RT.Models
{
    public abstract class BaseLobbyMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobby;

        public BaseLobbyMessage()
        {

        }

    }
}
