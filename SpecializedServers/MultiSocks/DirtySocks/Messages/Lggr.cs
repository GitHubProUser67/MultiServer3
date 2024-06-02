namespace MultiSocks.DirtySocks.Messages
{
    public class Lggr : AbstractMessage
    {
        public override string _Name { get => "lggr"; }

        public string? STR { get; set; }
    }
}
