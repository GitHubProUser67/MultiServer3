namespace MultiSocks.DirtySocks.Messages
{
    public class FlagOut : AbstractMessage
    {
        public override string _Name { get => "flag"; }
        public string? IDENT { get; set; }
        public string? FLAGS { get; set; }
    }
}
