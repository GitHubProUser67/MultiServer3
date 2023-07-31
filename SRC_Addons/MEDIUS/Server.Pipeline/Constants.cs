using DotNetty.Common.Utilities;
using PSMultiServer.Addons.Medius.Server.Pipeline.Attribute;

namespace PSMultiServer.Addons.Medius.Server.Pipeline
{
    public static class Constants
    {
        public static readonly AttributeKey<ScertClientAttribute> SCERT_CLIENT = AttributeKey<ScertClientAttribute>.ValueOf("SCERT_CLIENT");
    }
}
