using PSMultiServer.Addons.Horizon.RT.Common;

namespace PSMultiServer.Addons.Horizon.RT.Models
{
    public abstract class BaseLobbyExtMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobbyExt;

        public BaseLobbyExtMessage()
        {

        }
    }
}