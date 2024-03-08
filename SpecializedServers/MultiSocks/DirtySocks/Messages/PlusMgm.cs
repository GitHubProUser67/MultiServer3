namespace SRVEmu.DirtySocks.Messages
{
    public class PlusMgm : AbstractMessage
    {
        public override string _Name { get => "+mgm"; }

        public string ADDR0 { get; set; } = "159.153.161.174";
        public string? ADDR1 { get; set; }
        public string COUNT { get; set; } = "2";
        public string? CUSTFLAGS { get; set; }
        public string EVGID { get; set; } = "0";
        public string EVID { get; set; } = "0";
        public string GAMEMODE { get; set; } = "0";
        public string GAMEPORT { get; set; } = "9657";
        public string? GPSHOST { get; set; }
        public string GPSREGION { get; set; } = "2";
        public string HOST { get; set; } = "@brobot2583";
        public string IDENT { get; set; } = "6450";
        public string LADDR0 { get; set; } = "10.161.162.89";
        public string LADDR1 { get; set; } = "192.168.1.5";
        public string? MADDR0 { get; set; }
        public string MADDR1 { get; set; } = "$001fc61bc95c";
        public string MAXSIZE { get; set; } = "9";
        public string MINSIZE { get; set; } = "2";
        public string? NAME { get; set; }
        public string NUMPART { get; set; } = "1";
        public string OPFLAG0 { get; set; } = "0";
        public string? OPFLAG1 { get; set; }
        public string OPID0 { get; set; } = "650";
        public string? OPID1 { get; set; }
        public string OPPART0 { get; set; } = "0";
        public string OPPART1 { get; set; } = "0";
        public string OPPO0 { get; set; } = "@brobot2583";
        public string? OPPO1 { get; set; }
        public string? PARAMS { get; set; }
        public string PARTSIZE0 { get; set; } = "9";
        public string PRES0 { get; set; } = "0";
        public string PRES1 { get; set; } = "0";
        public string? PRIV { get; set; }
        public string ROOM { get; set; } = "0";
        public string? SEED { get; set; }
        public string? SYSFLAGS { get; set; }
        public string VOIPPORT { get; set; } = "9667";
        public string WHEN { get; set; } = DateTime.Now.ToString("yyyy.MM.dd-hh:mm:ss");
        public string WHENC { get; set; } = DateTime.Now.ToString("yyyy.MM.dd-hh:mm:ss");
    }
}
