namespace SRVEmu.DirtySocks.Messages
{
    public class PlusAgm : AbstractMessage
    {
        public override string _Name { get => "+agm"; }

        public string? IDENT { get; set; }
        public string WHEN { get; set; } = "2003.12.8 15:52:54";
        public string? NAME { get; set; }
        public string? HOST { get; set; }
        public string? PARAMS { get; set; }
        public string? ROOM { get; set; }
        public string? MAXSIZE { get; set; }
        public string? MINSIZE { get; set; }
        public string COUNT { get; set; } = "1";
        public string USERFLAGS { get; set; } = "0";
        public string? SYSFLAGS { get; set; }
        public string? PASS { get; set; }
        public string? PRIV { get; set; }
        public string? SEED { get; set; }
        public string? CUSTFLAGS { get; set; }
        public string? OPID { get; set; }
        public string? OPPO { get; set; }
        public string? ADDR { get; set; }

    }
}
