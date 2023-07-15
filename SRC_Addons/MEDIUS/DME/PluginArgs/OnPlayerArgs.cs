using PSMultiServer.SRC_Addons.MEDIUS.DME.Models;

namespace PSMultiServer.SRC_Addons.MEDIUS.DME.PluginArgs
{
    public class OnPlayerArgs
    {
        public ClientObject Player { get; set; }

        public World Game { get; set; }
    }
}
