using MultiServer.Addons.Horizon.MEDIUS.Medius.Models;

namespace MultiServer.Addons.Horizon.MEDIUS.PluginArgs
{
    public class OnPlayerGameArgs
    {
        /// <summary>
        /// Player.
        /// </summary>
        public ClientObject Player { get; set; }

        /// <summary>
        /// Game.
        /// </summary>
        public Game Game { get; set; }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Player: {Player} " +
                $"Game: {Game}";
        }
    }
}
