namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class MoveOut : AbstractMessage
    {
        public override string _Name { get => "move"; }

        public string? IDENT { get; set; }
        public string? NAME { get; set; }
        public string? COUNT { get; set; }
        public string FLAGS { get; set; } = "C";
        public string? LIDENT { get; set; }
        public string? LCOUNT { get; set; }
    }
}
