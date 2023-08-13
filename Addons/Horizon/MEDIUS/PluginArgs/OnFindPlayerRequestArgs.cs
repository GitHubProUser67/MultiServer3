using PSMultiServer.Addons.Horizon.RT.Models;
using PSMultiServer.Addons.Horizon.MEDIUS.Medius.Models;

namespace PSMultiServer.Addons.Horizon.MEDIUS.PluginArgs
{
    public class OnFindPlayerRequestArgs
    {
        /// <summary>
        /// Player making request.
        /// </summary>
        public ClientObject Player { get; set; }

        /// <summary>
        /// Find Player request.
        /// </summary>
        public IMediusRequest Request { get; set; }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Player:{Player} " +
                $"Request:{Request}";
        }
    }
}
