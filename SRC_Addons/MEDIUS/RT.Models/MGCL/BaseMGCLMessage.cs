using PSMultiServer.Addons.Medius.RT.Common;

namespace PSMultiServer.Addons.Medius.RT.Models
{
    public abstract class BaseMGCLMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobbyReport;

        public BaseMGCLMessage()
        {

        }

    }
}