using DotNetty.Common.Utilities;
using BackendProject.Horizon.LIBRARY.Pipeline.Attribute;

namespace BackendProject.Horizon.LIBRARY.Pipeline
{
    public static class Constants
    {
        public static readonly AttributeKey<ScertClientAttribute> SCERT_CLIENT = AttributeKey<ScertClientAttribute>.ValueOf("SCERT_CLIENT");
    }
}