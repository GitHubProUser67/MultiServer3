namespace SRVEmu.DirtySocks.Messages
{
    public class GpscOut : AbstractMessage
    {
        public override string _Name { get => "gpsc"; }

        public string IDENT { get; set; } = "1001";
        public string GAMEMODE { get; set; } = "0";
        public string PARTPARAMS { get; set; } = "0";
        public string ROOM { get; set; } = "1";
        public string OPGUEST { get; set; } = "0";
        public string WHEN { get; set; } = DateTime.Now.ToString("yyyy.MM.dd-hh:mm:ss");
        public string WHENC { get; set; } = DateTime.Now.ToString("yyyy.MM.dd-hh:mm:ss");
        public string? GPSHOST { get; set; }
        public string? HOST { get; set; }
    }
}
