namespace SRVEmu.DirtySocks.Messages
{
    public class SdtaOut : AbstractMessage
    {
        public override string _Name { get => "sdta"; }

        public string SLOT { get; set; } = "0";
        public string STATS { get; set; } = "0,0,0,0,0,0,0,0,0";
    }
}
