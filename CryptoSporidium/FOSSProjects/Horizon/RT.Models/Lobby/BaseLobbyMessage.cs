using CryptoSporidium.Horizon.RT.Common;

namespace CryptoSporidium.Horizon.RT.Models
{
    public abstract class BaseLobbyMessage : BaseMediusMessage
    {
        public override NetMessageClass PacketClass => NetMessageClass.MessageClassLobby;

        public BaseLobbyMessage()
        {

        }

    }
}