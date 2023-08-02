using DotNetty.Common.Utilities;
using PSMultiServer.Addons.Horizon.Server.Pipeline.Attribute;

namespace PSMultiServer.Addons.Horizon.Server.Pipeline
{
    public static class Constants
    {
        public static readonly AttributeKey<ScertClientAttribute> SCERT_CLIENT = AttributeKey<ScertClientAttribute>.ValueOf("SCERT_CLIENT");
    }
}
