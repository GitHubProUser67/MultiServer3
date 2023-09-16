using MultiServer.Addons.Horizon.RT.Common;

namespace MultiServer.Addons.Horizon.RT.Models
{
    public abstract class BaseMGCLMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobbyReport;

        public BaseMGCLMessage()
        {

        }

    }
}