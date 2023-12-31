namespace SRVEmu.Messages
{
    public class FgetOut : AbstractMessage
    {
        public override string _Name { get => "fget"; }
        public string? F { get; set; }
    }
}
