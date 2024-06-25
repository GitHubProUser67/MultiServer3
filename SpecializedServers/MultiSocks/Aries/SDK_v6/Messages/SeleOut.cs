namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class SeleOut : AbstractMessage
    {
        public override string _Name { get => "sele"; }
        public string? DP { get; set; }
        public string? ASYNC { get; set; }
        public string? CTRL { get; set; }
        public string? GAMES { get; set; }
        public string? INGAME { get; set; }
        public string? ROOMS { get; set; }
        public string? USERS { get; set; }
        public string? USERSETS { get; set; }
        public string? MESGS { get; set; }
        public string? MESGTYPES { get; set; }
        public string? MYGAME { get; set; }
        public string? MORE { get; set; }
        public string? SLOTS { get; set; }
        public string? STATS { get; set; }
    }
}
