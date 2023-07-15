using PSMultiServer.SRC_Addons.MEDIUS.MUIS.Models;

namespace PSMultiServer.SRC_Addons.MEDIUS.MUIS.PluginArgs
{
    public class OnGameArgs
    {
        /// <summary>
        /// Game.
        /// </summary>
        public Game Game { get; set; }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Game: {Game}";
        }
    }
}
