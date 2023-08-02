using DotNetty.Transport.Channels;
using PSMultiServer.Addons.Horizon.RT.Models;
using PSMultiServer.Addons.Horizon.DME.Models;

namespace PSMultiServer.Addons.Horizon.DME.PluginArgs
{
    public class OnMediusMessageArgs
    {
        public ClientObject Player { get; set; } = null;

        public IChannel Channel { get; set; } = null;

        public BaseMediusMessage Message { get; set; } = null;

        public bool IsIncoming { get; }

        public bool Ignore { get; set; } = false;

        public OnMediusMessageArgs(bool isIncoming)
        {
            IsIncoming = isIncoming;
        }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Player:{Player} " +
                $"Channel:{Channel} " +
                $"Message:{Message} " +
                $"Ignore:{Ignore}";
        }
    }
}
