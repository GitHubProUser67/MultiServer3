using MultiSocks.Aries.SDK_v6.Model;

namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class SeleIn : AbstractMessage
    {
        public override string _Name { get => "sele"; }

        public override void Process(AbstractAriesServerV6 context, AriesClient client)
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
                        DP = "PS3/Burnout-Dec2007/mod"
                    });
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
