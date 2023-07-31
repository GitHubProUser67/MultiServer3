using PSMultiServer.Addons.Medius.RT.Common;

namespace PSMultiServer.Addons.Medius.RT.Models
{
    public abstract class BaseDMEMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassDME;

        public BaseDMEMessage()
        {

        }
    }
}