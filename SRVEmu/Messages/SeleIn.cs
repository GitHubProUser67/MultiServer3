namespace SRVEmu.Messages
{
    public class SeleIn : AbstractMessage
    {
        public override string _Name { get => "sele"; }

        public string? MYGAME { get; set; }
        public string GAMES { get; set; } = "1";
        public string ROOMS { get; set; } = "1";
        public string USERS { get; set; } = "1";
        public string MESGS { get; set; } = "1";
        public string? MESGTYPES { get; set; }
        public string? STATS { get; set; }
        public string RANKS { get; set; } = "1";
        public string? USERSETS { get; set; }

        public override void Process(AbstractDirtySockServer context, DirtySockClient client)
        {
            client.SendMessage(new SeleOut()
            {
                GAMES = GAMES,
                ROOMS = ROOMS,
                USERS = USERS,
                MESGS = MESGS,
                RANKS = RANKS
            });
        }
    }
}
