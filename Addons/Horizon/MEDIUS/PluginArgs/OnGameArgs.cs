using MultiServer.Addons.Horizon.MEDIUS.Medius.Models;

namespace MultiServer.Addons.Horizon.MEDIUS.PluginArgs
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
