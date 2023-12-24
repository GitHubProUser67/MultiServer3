using SRVEmu.Arcadia.Storage;

namespace SRVEmu.Arcadia.Handlers
{
    public class StaticCache
    {
        public static SharedCounters _sharedCounters = new();
        public static SharedCache _sharedCache = new();
    }
}
