using DotNetty.Transport.Channels;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Models;
using PSMultiServer.SRC_Addons.MEDIUS.MEDIUS.Medius.Models;


namespace PSMultiServer.SRC_Addons.MEDIUS.MEDIUS.PluginArgs
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
                $"Player: {Player} " +
                $"Channel: {Channel} " +
                $"Message: {Message} " +
                $"Ignore: {Ignore}";
        }
    }
}
