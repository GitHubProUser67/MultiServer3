using PSMultiServer.Addons.Horizon.RT.Common;

namespace PSMultiServer.Addons.Horizon.RT.Models
{
    public abstract class BaseMGCLMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobbyReport;

        public BaseMGCLMessage()
        {

        }

    }
}