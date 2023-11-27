using CryptoSporidium.Horizon.RT.Common;

namespace CryptoSporidium.Horizon.RT.Models
{
    public abstract class BaseDMEMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassDME;

        public BaseDMEMessage()
        {

        }
    }
}