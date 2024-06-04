namespace MultiSocks.DirtySocks.Messages
{
    public class PrivOut : AbstractMessage
    {
        public override string _Name { get => "priv"; }
        public string? PRIV {  get; set; }
    }
}
