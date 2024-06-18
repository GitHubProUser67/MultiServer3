using MultiSocks.DirtySocks.Model;

namespace MultiSocks.DirtySocks.Messages
{
    public class GseaIn : AbstractMessage
    {
        public override string _Name { get => "gsea"; }
        public string? START { get; set; }
        public string? COUNT { get; set; }
        public string? ASYNC { get; set; }
        public string? SYSMASK { get; set; }
        public string? SYSFLAGS { get; set; }
        public string? PLAYERS { get; set; }
        public string? CUSTOM { get; set; }
        public string? CUSTMASK { get; set; }
        public string? CUSTFLAGS { get; set; }
        public string? CANCEL { get; set; }
        public string? LANG { get; set; }
        public string? VER { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (context is not MatchmakerServer mc) return;

            if (CANCEL == "1")
            {
                client.SendMessage(new GseaOut());
                return;
            }

            if (int.TryParse(START, out int start) && int.TryParse(COUNT, out int count))
            {
                List<Game> MatchingList = mc.Games.GamesSessions.Values
                    .Where(game => (CUSTFLAGS == "0" || game.CustFlags.Equals(CUSTFLAGS)) && (SYSFLAGS == "0" || game.SysFlags.Equals(SYSFLAGS)))
                    .Skip(start - 1) // Adjusting for 1-based indexing
                    .Take(count)
                    .ToList();

                if (!string.IsNullOrEmpty(LANG) && LANG != "-1" 
                    && !string.IsNullOrEmpty(context.Project) 
                    && context.Project.Contains("DPR-09")) // Hasbro Family game Night does custom filtering on top for region specific lobbies.
                    
                    MatchingList = MatchingList.Where(game => game.Params.Contains($"LANG%3d{LANG}") 
                    && game.Params.Contains($"VER%3d{VER}")).ToList();

                client.SendMessage(new GseaOut() { COUNT = MatchingList.Count.ToString() });

                foreach (Game game in MatchingList)
                {
                    client.SendMessage(game.GetPlusGam());
                }
            }
            else
            {
                // TODO, send dirtysocks error.
            }
        }
    }
}
