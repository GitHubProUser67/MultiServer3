using PSMultiServer.Addons.Horizon.RT.Common;

namespace PSMultiServer.Addons.Horizon.RT.Models
{
    public abstract class BaseDMEMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassDME;

        public BaseDMEMessage()
        {

        }
    }
}