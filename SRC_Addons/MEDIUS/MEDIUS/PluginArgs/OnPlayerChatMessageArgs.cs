using PSMultiServer.Addons.Medius.RT.Models.Misc;
using PSMultiServer.Addons.Medius.MEDIUS.Medius.Models;

namespace PSMultiServer.Addons.Medius.MEDIUS.PluginArgs
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
