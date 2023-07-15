using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;

namespace PSMultiServer.SRC_Addons.MEDIUS.RT.Models
{
    public abstract class BaseMGCLMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobbyReport;

        public BaseMGCLMessage()
        {

        }

    }
}