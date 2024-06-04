namespace MultiSocks.DirtySocks.Messages
{
    public class GqwkOut : AbstractMessage
    {
        public override string _Name { get => "gqwk"; }

        public string? IDENT { get; set; } // Game ID
        public string WHEN { get; set; } = DateTime.Now.ToString("yyyy.MM.dd-hh:mm:ss");
        public string WHENC { get; set; } = DateTime.Now.ToString("yyyy.MM.dd-hh:mm:ss");
        public string? NAME { get; set; }
        public string? HOST { get; set; }
        public string? ROOM { get; set; }
        public string? MAXSIZE { get; set; }
        public string? MINSIZE { get; set; }
        public string? COUNT { get; set; }
        public string? PRIV { get; set; }
        public string? CUSTFLAGS { get; set; }
        public string? SYSFLAGS { get; set; }
        public string? EVID { get; set; }
        public string? EVGID { get; set; }
        public string? PARAMS { get; set; }
        public string? SEED { get; set; }
        public string? GPSHOST { get; set; }
        public string? GPSREGION { get; set; }
        public string? GAMEMODE { get; set; }
        public string? GAMEPORT { get; set; }
        public string? VOIPPORT { get; set; }
        public string? NUMPART { get; set; }
        public string? PARTSIZE0 { get; set; }
        public string PARTPARAMS0 { get; set; } = string.Empty;
        public string OPPO0 { get; set; } = "@brobot24";
        public string? OPPART0 { get; set; }
        public string? OPFLAG0 { get; set; }
        public string? PRES0 { get; set; }
        public string OPID0 { get; set; } = "24";
        public string ADDR0 { get; set; } = "127.0.0.1";
        public string LADDR0 { get; set; } = "127.0.0.1";
        public string MADDR0 { get; set; } = string.Empty;
        public string? OPPARAM0 { get; set; }
        public Dictionary<string, string>? PLAYERSLIST { get; set; }
    }
}
