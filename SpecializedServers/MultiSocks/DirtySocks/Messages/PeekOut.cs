namespace MultiSocks.DirtySocks.Messages
{
    public class PeekOut : AbstractMessage
    {
        public override string _Name { get => "peek"; }

        public string? IDENT { get; set; }
        public string? NAME { get; set; }
        public string? COUNT { get; set; }
        public string FLAGS { get; set; } = "C";
    }
}
