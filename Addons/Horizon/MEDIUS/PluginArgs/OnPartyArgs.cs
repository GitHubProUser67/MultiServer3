using PSMultiServer.Addons.Horizon.MEDIUS.Medius.Models;

namespace PSMultiServer.Addons.Horizon.MEDIUS.PluginArgs
{
    public class OnPartyArgs
    {
        /// <summary>
        /// Party.
        /// </summary>
        public Party Party { get; set; }

        public override string ToString()
        {
            return base.ToString() + " " +
                $"Party: {Party}";
        }
    }
}
