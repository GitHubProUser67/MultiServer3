namespace SRVEmu.Messages
{
    public class AuthOut : AbstractMessage
    {
        public override string _Name { get => "auth"; }

        public string? TOS { get; set; }
        public string? NAME { get; set; }
        public string MAIL { get; set; } = "tsbo@freeso.net";
        public string BORN { get; set; } = "19800325";
        public string GEND { get; set; } = "M";
        public string FROM { get; set; } = "US";
        public string LANG { get; set; } = "en";
        public string SPAM { get; set; } = "NN";
        public string SINCE { get; set; } = DateTime.Now.ToString("yyyy.MM.dd-hh:mm:ss");
        public string ADDR { get; set; } = "24.141.32.61";
        public string LUID { get; set; } = "$000000000b32588d";
        public string? PERSONAS { get; set; } // comma separated list
        public string LAST { get; set; } = DateTime.Now.ToString("yyyy.MM.dd-hh:mm:ss");
    }
}
