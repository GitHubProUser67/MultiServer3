using CryptoSporidium.Horizon.RT.Common;

namespace CryptoSporidium.Horizon.RT.Models
{
    public abstract class BaseLobbyExtMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobbyExt;

        public BaseLobbyExtMessage()
        {

        }
    }
}