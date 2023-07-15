using PSMultiServer.SRC_Addons.MEDIUS.RT.Models;
using PSMultiServer.SRC_Addons.MEDIUS.DME.Models;

namespace PSMultiServer.SRC_Addons.MEDIUS.DME.PluginArgs
{
    public class OnTcpMsg
    {
        public ClientObject Player { get; set; }

        public BaseScertMessage Packet { get; set; }

        public bool Ignore { get; set; }
    }
}
