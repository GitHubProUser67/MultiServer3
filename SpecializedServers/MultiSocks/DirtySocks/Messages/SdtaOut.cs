namespace MultiSocks.DirtySocks.Messages
{
    public class SdtaOut : AbstractMessage
    {
        public override string _Name { get => "sdta"; }

        public string SLOT { get; set; } = "0";
        public string STATS { get; set; } = "1,2,3,4,5,6,7,8,9,10,11,12,13";
    }
}
