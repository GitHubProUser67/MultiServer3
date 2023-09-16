using DotNetty.Transport.Channels;
using MultiServer.Addons.Horizon.RT.Models;
using MultiServer.Addons.Horizon.DME.Models;

namespace MultiServer.Addons.Horizon.DME.PluginArgs
{
    public class OnMessageArgs
    {
        public ClientObject Player { get; set; } = null;

        public IChannel Channel { get; set; } = null;

        public BaseScertMessage Message { get; set; } = null;

        public bool IsIncoming { get; }

        public bool Ignore { get; set; } = false;


        public OnMessageArgs(bool isIncoming)
        {
            IsIncoming = isIncoming;
        }
    }
}
