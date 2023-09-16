using DotNetty.Transport.Channels;
using MultiServer.Addons.Horizon.RT.Models;
using MultiServer.Addons.Horizon.MEDIUS.Medius.Models;

namespace MultiServer.Addons.Horizon.MEDIUS.PluginArgs
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
