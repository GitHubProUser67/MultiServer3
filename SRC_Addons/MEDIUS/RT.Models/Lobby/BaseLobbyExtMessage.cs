using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models
{
    public abstract class BaseLobbyExtMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobbyExt;

        public BaseLobbyExtMessage()
        {

        }
    }
}