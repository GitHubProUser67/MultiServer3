using PSMultiServer.SRC_Addons.MEDIUS.MUIS.Models;

namespace PSMultiServer.SRC_Addons.MEDIUS.MUIS.PluginArgs
{
    public class OnPlayerArgs
    {
        /// <summary>
        /// Player.
        /// </summary>
        public ClientObject Player { get; set; }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Player: {Player}";
        }
    }
}
