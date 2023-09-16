using MultiServer.Addons.Horizon.RT.Common;

namespace MultiServer.Addons.Horizon.RT.Models
{
    public abstract class BaseDMEMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassDME;

        public BaseDMEMessage()
        {

        }
    }
}