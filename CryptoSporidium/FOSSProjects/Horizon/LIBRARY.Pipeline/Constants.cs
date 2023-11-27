using DotNetty.Common.Utilities;
using CryptoSporidium.Horizon.LIBRARY.Pipeline.Attribute;

namespace CryptoSporidium.Horizon.LIBRARY.Pipeline
{
    public static class Constants
    {
        public static readonly AttributeKey<ScertClientAttribute> SCERT_CLIENT = AttributeKey<ScertClientAttribute>.ValueOf("SCERT_CLIENT");
    }
}