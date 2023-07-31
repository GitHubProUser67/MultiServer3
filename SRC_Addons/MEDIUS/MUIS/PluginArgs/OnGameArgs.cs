using PSMultiServer.Addons.Medius.MUIS.Models;

namespace PSMultiServer.Addons.Medius.MUIS.PluginArgs
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
