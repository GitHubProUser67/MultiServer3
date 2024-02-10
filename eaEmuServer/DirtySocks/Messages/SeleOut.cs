namespace SRVEmu.DirtySocks.Messages
{
    public class SeleOut : AbstractMessage
    {
        public override string _Name { get => "sele"; }
        public string? DP { get; set; }
        public string? ASYNC { get; set; }
        public string? CTRL { get; set; }
        public string GAMES { get; set; } = "1";
        public string? GFID { get; set; }
        public string? INGAME { get; set; }
        public string ROOMS { get; set; } = "1";
        public string USERS { get; set; } = "1";
        public string? USERSETS { get; set; }
        public string MESGS { get; set; } = "1";
        public string? MESGTYPES { get; set; }
        public string? MYGAME { get; set; }
        public string? PLATFORM { get; set; }
        public string? PSID { get; set; }
        public string RANKS { get; set; } = "0";
        public string MORE { get; set; } = "1";
        public string SLOTS { get; set; } = "36";
        public string? STATS { get; set; }
    }
}
