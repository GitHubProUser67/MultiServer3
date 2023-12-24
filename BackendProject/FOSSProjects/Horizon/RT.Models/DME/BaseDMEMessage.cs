using BackendProject.Horizon.RT.Common;

namespace BackendProject.Horizon.RT.Models
{
    public abstract class BaseDMEMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassDME;

        public BaseDMEMessage()
        {

        }
    }
}