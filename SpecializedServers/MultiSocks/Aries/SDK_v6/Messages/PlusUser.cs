namespace MultiSocks.Aries.SDK_v6.Messages
{
    public class PlusUser : AbstractMessage
    {
        public override string _Name { get => "+usr"; }

        public string? I { get; set; }
        public string? N { get; set; }
        public string? M { get; set; }
        public string? A { get; set; }
        public string? P { get; set; }
        public string? X { get; set; }
        public string? G { get; set; }
    }
}
