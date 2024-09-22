namespace MultiSocks.Aries.Messages
{
    public class Rrgt : AbstractMessage
    {
        public override string _Name { get => "rrgt"; }
        public string? R { get; set; }
        public string? SET { get; set; }

        public override void Process(AbstractAriesServer context, AriesClient client)
        {
            client.SendMessage(new RrgtTime());
        }
    }
}
