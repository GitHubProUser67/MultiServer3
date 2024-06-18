namespace MultiSocks.DirtySocks.Messages
{
    public class UcreOut : AbstractMessage
    {
        public override string _Name { get => "ucre"; }

        public string? IDENT { get; set; }
        public string? NAME { get; set; }
    }
}
