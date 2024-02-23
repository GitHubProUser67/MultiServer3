namespace SRVEmu.DirtySocks.Messages
{
    public class AcctOut : AbstractMessage
    {
        public override string _Name { get => "acct"; }

        public string? NAME { get; set; }
        public string? PERSONAS { get; set; }
        public string? AGE { get; set; }
    }
}
