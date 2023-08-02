using PSMultiServer.Addons.Horizon.RT.Models;
using PSMultiServer.Addons.Horizon.MUIS.Models;

namespace PSMultiServer.Addons.Horizon.MUIS.PluginArgs
{
    public class OnPlayerRequestArgs
    {
        /// <summary>
        /// Player making request.
        /// </summary>
        public ClientObject Player { get; set; }

        /// <summary>
        /// Create request.
        /// </summary>
        public IMediusRequest Request { get; set; }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Player: {Player} " +
                $"Request: {Request}";
        }
    }
}
