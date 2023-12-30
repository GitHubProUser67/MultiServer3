namespace SRVEmu.Messages
{
    public class PlusMgm : AbstractMessage
    {
        public override string _Name { get => "+mgm"; }

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
    }
}
