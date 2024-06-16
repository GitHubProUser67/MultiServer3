namespace MultiSocks.DirtySocks.Messages
{
    public class RgetOut : AbstractMessage
    {
        public override string _Name { get => "RGET"; }
        public string? ID { get; set; }
        public string? SIZE { get; set; }
    }
}
