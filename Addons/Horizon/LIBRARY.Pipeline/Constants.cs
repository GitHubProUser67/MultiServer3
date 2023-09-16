using DotNetty.Common.Utilities;
using MultiServer.Addons.Horizon.LIBRARY.Pipeline.Attribute;

namespace MultiServer.Addons.Horizon.LIBRARY.Pipeline
{
    public static class Constants
    {
        public static readonly AttributeKey<ScertClientAttribute> SCERT_CLIENT = AttributeKey<ScertClientAttribute>.ValueOf("SCERT_CLIENT");
    }
}
