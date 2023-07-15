using DotNetty.Common.Utilities;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Pipeline.Attribute;

namespace PSMultiServer.SRC_Addons.MEDIUS.Server.Pipeline
{
    public static class Constants
    {
        public static readonly AttributeKey<ScertClientAttribute> SCERT_CLIENT = AttributeKey<ScertClientAttribute>.ValueOf("SCERT_CLIENT");
    }
}
