using Horizon.RT.Models;
using Horizon.MUIS.Models;

namespace Horizon.MUIS.PluginArgs
{
    public class OnPlayerRequestArgs
    {
        /// <summary>
        /// Player making request.
        /// </summary>
        public ClientObject? Player { get; set; }

        /// <summary>
        /// Create request.
        /// </summary>
        public IMediusRequest? Request { get; set; }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Player: {Player} " +
                $"Request: {Request}";
        }
    }
}