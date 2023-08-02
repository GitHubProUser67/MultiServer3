using PSMultiServer.Addons.Horizon.RT.Models;
using PSMultiServer.Addons.Horizon.MEDIUS.Medius.Models;

namespace PSMultiServer.Addons.Horizon.MEDIUS.PluginArgs
{
    public class OnAccountLoginRequestArgs
    {
        /// <summary>
        /// Player making request.
        /// </summary>
        public ClientObject Player { get; set; }
        /// <summary>
        /// AccountLogin request.
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
