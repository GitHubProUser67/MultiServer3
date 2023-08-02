using PSMultiServer.Addons.Horizon.MUIS.Models;

namespace PSMultiServer.Addons.Horizon.MUIS.PluginArgs
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
