namespace SRVEmu.Messages
{
    public class PlusGam : AbstractMessage
    {
        public override string _Name { get => "+gam"; }

        public string COUNT { get; set; } = "1";
        public string NUMPART { get; set; } = "1";
        public string PARTSIZE0 { get; set; } = "9";
        public string GPSREGION { get; set; } = "2";
        public string GAMEPORT { get; set; } = "9657";
        public string VOIPPORT { get; set; } = "9657";
        public string EVGID { get; set; } = "0";
        public string EVID { get; set; } = "0";
        public string IDENT { get; set; } = "6450";
        public string GAMEMODE { get; set; } = "0";
        public string PARTPARAMS0 { get; set; } = string.Empty;
        public string ROOM { get; set; } = "0";
        public string WHEN { get; set; } = DateTime.Now.ToString("yyyy.MM.dd-hh:mm:ss");
        public string WHENC { get; set; } = DateTime.Now.ToString("yyyy.MM.dd-hh:mm:ss");
        public string? GPSHOST { get; set; }
        public string? HOST { get; set; }
        public string? CUSTFLAGS { get; set; }
        public string? MINSIZE { get; set; }
        public string? MAXSIZE { get; set; }
        public string? NAME { get; set; }
        public string? PARAMS { get; set; }
        public string? PASS { get; set; }
        public string? PRIV { get; set; }
        public string? SEED { get; set; }
        public string? SYSFLAGS { get; set; }
        public string? FORCE_LEAVE { get; set; }
        public string? USERPARAMS { get; set; }
        public string? USERFLAGS { get; set; }
        public string OPGUEST { get; set; } = "0";
    }
}
