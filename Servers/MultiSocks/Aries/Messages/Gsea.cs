using MultiSocks.Aries.Model;

namespace MultiSocks.Aries.Messages
{
    public class Gsea : AbstractMessage
    {
        public override string _Name { get => "gsea"; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            if (context is not MatchmakerServer mc) return;

            AriesUser? user = client.User;
            if (user == null) return;

            string? CANCEL = GetInputCacheValue("CANCEL");

            if (!string.IsNullOrEmpty(CANCEL) && "1".Equals(CANCEL))
            {
                client.CanAsyncGameSearch = false;

                client.SendMessage(this);
                return;
            }
            else
                client.CanAsyncGameSearch = true;

            if (int.TryParse(GetInputCacheValue("START"), out int start) && int.TryParse(GetInputCacheValue("COUNT"), out int count))
            {
                string? CUSTFLAGS = GetInputCacheValue("CUSTFLAGS");
                string? SYSFLAGS = GetInputCacheValue("SYSFLAGS");
                string? LANG = GetInputCacheValue("LANG");

                List<AriesGame> MatchingList = mc.Games.GamesSessions.Values
                    .Where(game => (string.IsNullOrEmpty(CUSTFLAGS) || CUSTFLAGS == "0" || game.CustFlags.Equals(CUSTFLAGS)) && (string.IsNullOrEmpty(SYSFLAGS) || SYSFLAGS == "0" || game.SysFlags.Equals(SYSFLAGS)) && !game.Started)
                    .Skip(start - 1) // Adjusting for 1-based indexing
                    .Take(count)
                    .ToList();

                if (!string.IsNullOrEmpty(LANG) && LANG != "-1" && !string.IsNullOrEmpty(context.Project) && context.Project.Equals("DPR-09")) // Hasbro Family game Night does custom filtering on top for region specific lobbies.
                    MatchingList = MatchingList.Where(game => game.Params.Contains($"LANG%3d{LANG}") && game.Params.Contains($"VER%3d{GetInputCacheValue("VER")}")).ToList();

                OutputCache.Add("COUNT", MatchingList.Count.ToString());

                client.SendMessage(this);

                foreach (AriesGame game in MatchingList)
                {
                    client.SendMessage(game.GetGameDetails("+gam"));
                }
            }
        }
    }
}
