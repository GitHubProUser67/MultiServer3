namespace MultiSocks.DirtySocks.Messages
{
    public class PentOut : AbstractMessage
    {
        public override string _Name { get => "pent"; }

        public string? GFID { get; set; }
    }
}