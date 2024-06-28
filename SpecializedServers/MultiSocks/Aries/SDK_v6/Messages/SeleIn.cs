using MultiSocks.Aries.SDK_v6.Model;

namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class SeleIn : AbstractMessage
    {
        public override string _Name { get => "sele"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            User? user = client.User;

            string? STATS = GetInputCacheValue("STATS");
            string? INGAME = GetInputCacheValue("INGAME");
            string? ASYNC = GetInputCacheValue("ASYNC");

            if (STATS == null && INGAME == null)
                client.SendMessage(new SeleOut()
                {
                    MORE = "0",
                    SLOTS = "4",
                    STATS = "0"
                });
            else
            {
                if ("1".Equals(INGAME))
                {
                    Dictionary<string, string?> OutCache = new();

                    if (context.SKU == "PS3")
                        OutCache.Add("DP", "PS3/Burnout-Dec2007/mod");
                    else if (context.SKU == "PC")
                    {
                        OutCache.Add("DP", "PC/Burnout-2008/na1");
                        OutCache.Add("GFID", "\"ODS:19038.110.Base Product;BURNOUT PARADISE ULTIMATE EDITION_PC_ONLINE_ACCESS\"");
                        OutCache.Add("PLATFORM", "pc");
                        OutCache.Add("PSID", "PS-REG-BURNOUT2008");
                    }
                    else
                        OutCache.Add("DP", "XBOX360/Burnout-Dec2007/mod");

                    client.SendMessage(new SeleOut()
                    {
                        INGAME = INGAME,
                        MESGS = GetInputCacheValue("MESGS") ?? "1",
                        MESGTYPES = GetInputCacheValue("MESGTYPES") ?? "P",
                        USERS = GetInputCacheValue("USERS") ?? "0",
                        GAMES = GetInputCacheValue("GAMES") ?? "0",
                        MYGAME = GetInputCacheValue("MYGAME") ?? "0",
                        ROOMS = GetInputCacheValue("ROOMS") ?? "0",
                        ASYNC = ASYNC ?? "0",
                        USERSETS = GetInputCacheValue("USERSETS") ?? "0",
                        CTRL = "0",
                        SLOTS = "280",
                        STATS = STATS,
                        OutputCache = OutCache
                    });
                }
                else
                    client.SendMessage(new SeleOut()
                    {
                        INGAME = INGAME
                    });
            }

            if (user != null && (STATS != null || INGAME != null))
                user.SendPlusWho(user);
        }
    }
}
