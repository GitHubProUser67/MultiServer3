using BackendProject.Horizon.RT.Common;

namespace BackendProject.Horizon.RT.Models
{
    public abstract class BaseMGCLMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobbyReport;

        public BaseMGCLMessage()
        {

        }

    }
}