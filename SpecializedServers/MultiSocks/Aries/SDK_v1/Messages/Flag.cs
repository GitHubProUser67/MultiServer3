namespace MultiSocks.Aries.SDK_v1.Messages
{
    public class Flag : AbstractMessage
    {
        public override string _Name { get => "flag"; }
        public string? CLR { get; set; }
        public string? SET { get; set; }

        public override void Process(AbstractAriesServerV1 context, AriesClient client)
        {
            client.SendMessage(new FlagOut()
            {
                IDENT = "1",
                FLAGS = SET
            });
        }
    }

    public class FlagOut : AbstractMessage
    {
        public override string _Name { get => "flag"; }
        public string? IDENT { get; set; }
        public string? FLAGS { get; set; }
    }
}