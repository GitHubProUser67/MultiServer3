namespace MultiSocks.DirtySocks.Messages
{
    public class AuthOut : AbstractMessage
    {
        public override string _Name { get => "auth"; }

        public string? TOS { get; set; }
        public string? NAME { get; set; }
        public string MAIL { get; set; } = "tsbo@freeso.net";
        public string? BORN { get; set; }
        public string? GEND { get; set; }
        public string? FROM { get; set; }
        public string? SHARE { get; set; }
        public string? GFIDS { get; set; }
        public string? LANG { get; set; }
        public string? LOC { get; set; }
        public string SPAM { get; set; } = "NN";
        public string? PERSONAS { get; set; } // comma separated list
        public string? LAST { get; set; }
        public string? SINCE { get; set; }
        public string? ADDR { get; set; }
        public string? LUID { get; set; }
        public string? TOKEN { get; set; }
    }
}
