using PSMultiServer.SRC_Addons.MEDIUS.MEDIUS.Medius.Models;

namespace PSMultiServer.SRC_Addons.MEDIUS.MEDIUS.PluginArgs
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
