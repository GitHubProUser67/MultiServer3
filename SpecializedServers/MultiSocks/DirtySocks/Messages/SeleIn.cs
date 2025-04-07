namespace MultiSocks.DirtySocks.Messages
{
    public class SeleIn : AbstractMessage
    {
        public override string _Name { get => "sele"; }

        public string? MYGAME { get; set; } = "0";
        public string GAMES { get; set; } = "0";
        public string ROOMS { get; set; } = "0";
        public string USERS { get; set; } = "0";
        public string MESGS { get; set; } = "1";
        public string? MESGTYPES { get; set; } = "P";
        public string? STATS { get; set; } = "0";
        public string RANKS { get; set; } = "1";
        public string? USERSETS { get; set; } = "0";
        public string? INGAME { get; set; } = "0";
        public string? ASYNC { get; set; } = "0";

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            if (!string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5"))
                client.SendMessage(new SeleOut()
                {
                    GAMES = GAMES,
                    MYGAME = MYGAME,
                    USERS = USERS,
                    ROOMS = ROOMS,
                    USERSETS = USERSETS,
                    MESGS = MESGS,
                    MESGTYPES = (!string.IsNullOrEmpty(MESGTYPES) && MESGTYPES.StartsWith("1")) ? "01" : MESGTYPES,
                    ASYNC = ASYNC,
                    CTRL = "0",
                    STATS = STATS,
                    SLOTS = "280",
                    INGAME = INGAME,
                    DP = !string.IsNullOrEmpty(context.SKU) && context.SKU.Contains("PS3") ? "PS3/Burnout-Dec2007/mod" : "PC/Burnout-Dec2007/mod"
                });
            else
                client.SendMessage(new SeleOut()
                {
                    GAMES = GAMES,
                    ROOMS = ROOMS,
                    USERS = USERS,
                    MESGS = MESGS,
                    RANKS = RANKS,
                    MORE = "1",
                    SLOTS = "36"
                });
        }
    }
}
