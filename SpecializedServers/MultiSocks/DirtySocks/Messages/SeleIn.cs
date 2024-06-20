using MultiSocks.DirtySocks.Model;

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
            User? user = client.User;

            if (!string.IsNullOrEmpty(context.Project) && context.Project.Contains("BURNOUT5"))
            {
                if (context.SKU == "PS3")
                    client.SendMessage(new SeleOut()
                    {
                        GAMES = GAMES,
                        MYGAME = MYGAME,
                        USERS = USERS,
                        ROOMS = ROOMS,
                        USERSETS = USERSETS,
                        MESGS = MESGS,
                        MESGTYPES = MESGTYPES,
                        ASYNC = ASYNC,
                        CTRL = "0",
                        STATS = STATS,
                        SLOTS = "280",
                        INGAME = INGAME,
                        DP = "PS3/Burnout-Dec2007/mod"
                    });
                else
                    client.SendMessage(new SeleOut()
                    {
                        GAMES = GAMES,
                        MYGAME = MYGAME,
                        USERS = USERS,
                        ROOMS = ROOMS,
                        USERSETS = USERSETS,
                        MESGS = MESGS,
                        MESGTYPES = MESGTYPES,
                        ASYNC = ASYNC,
                        CTRL = "0",
                        STATS = STATS,
                        SLOTS = "280",
                        INGAME = INGAME,
                        DP = "PC/Burnout-Dec2007/mod",
                        GFID = "\"ODS:19038.110.Base Product;BURNOUT PARADISE ULTIMATE EDITION_PC_ONLINE_ACCESS\"",
                        PSID = "PS-REG-BURNOUT2008"
                    });
            }
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

            if (user != null && (USERSETS != "0" || INGAME != "0"))
            {
                if (!string.IsNullOrEmpty(context.Project))
                {
                    if (context.Project.Contains("DPR-09"))
                        user.SendPlusWho(user, "DPR-09");
                    else if (context.Project.Contains("BURNOUT5"))
                        user.SendPlusWho(user, "BURNOUT5");
                    else
                        user.SendPlusWho(user, string.Empty);
                }
                else
                    user.SendPlusWho(user, string.Empty);
            }
        }
    }
}
