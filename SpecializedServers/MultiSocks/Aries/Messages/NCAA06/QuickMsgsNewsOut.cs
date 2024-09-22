namespace MultiSocks.Aries.Messages
{
    public class QuickMsgsNewsOut : AbstractMessage
    {
        public override string _Name { get => "news"; }

        public string? EASO_QuickMessageAdaptor = "Test";
    }
}
