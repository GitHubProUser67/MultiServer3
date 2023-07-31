using DotNetty.Common.Internal.Logging;

namespace PSMultiServer.Addons.Medius.MEDIUS.Medius.Models
{
    public class Clan
    {
        static readonly IInternalLogger _logger = InternalLoggerFactory.GetInstance<Clan>();
        protected virtual IInternalLogger Logger => _logger;

        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";

        public Clan()
        {

        }
    }
}
