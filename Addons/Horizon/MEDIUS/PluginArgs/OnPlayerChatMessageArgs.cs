using MultiServer.Addons.Horizon.RT.Models.Misc;
using MultiServer.Addons.Horizon.MEDIUS.Medius.Models;

namespace MultiServer.Addons.Horizon.MEDIUS.PluginArgs
{
    public class OnPlayerChatMessageArgs
    {
        /// <summary>
        /// Source player.
        /// </summary>
        public ClientObject Player { get; set; }

        public Channel Channel { get; set; }

        /// <summary>
        /// Message.
        /// </summary>
        public IMediusChatMessage Message { get; set; }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Player: {Player} " +
                $"Message: {Message}";
        }

    }
}
